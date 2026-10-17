# HUD do Player v2 — "Painel de cockpit" · Notas para a Unity

O conceito é um painel de instrumentos de avião militar cartunesco, preso no canto inferior esquerdo:

- **Mostrador de integridade** redondo, com o aviãozinho, à esquerda.
- **HP = integridade do casco**: uma faixa de placas de blindagem com rebites.
- **SP = reservatório de energia** de vidro, que alimenta uma **turbina**. Quando o reservatório enche, a turbina acende uma chama azul.

Não há portrait, e a barra do boss **não foi alterada**.

Fonte da verdade para posições, tamanhos, ordem e tempos: **`hud_v2_layout.json`**.

---

## 1. Import e Canvas
- Canvas: Screen Space Overlay · Scale With Screen Size · referência **1200×720** · match 0.5 (igual ao projeto atual).
- Arte nativa: **182×56 px**. Ela é exibida em **x2**, ou seja, **364×112 unidades de UI**.
  - `parts_x2/` + `fx_x2/` → **PPU 100**
  - ou `parts_x1/` + `fx_x1/` → **PPU 50** (resultado idêntico; use um dos dois conjuntos, nunca os dois misturados)
- Todos os sprites usam: Filter **Point**, Compression **None**, Mip Maps desligado, Wrap Clamp. Use `Set Native Size` em todo Image.
- **Raiz `PlayerHUD_v2`:** anchor e pivot no **canto inferior esquerdo (0,0)**, `anchoredPosition (16,16)`, `sizeDelta (364,112)`.
- A chama e as centelhas **passam da borda direita** da raiz, porque o bocal fica em x=178 de 182. Isso é intencional. A raiz não deve ter máscara.

## 2. Tipos de peça
| Tipo | Peças | Como posicionar |
|---|---|---|
| **Canvas inteiro** (182×56, transparente fora da peça) | `hud_plate`, `hud_name`, `hp_housing`, `hp_emblem*`, `hp_dial_crack`, `hp_lowhealth_glow`, `sp_glass`, `sp_housing`, `sp_turbine`, `sp_full_glow` | stretch na raiz, ou pos (0,0) com Native Size; já vêm alinhadas |
| **Fills recortados** | `hp_track`, `hp_damage_lag`, `hp_fill`, `hp_fill_low`, `hp_heal_flash` (113×8) · `sp_track`, `sp_fill_*`, `sp_refill_flash` (103×7) | `anchoredPosition` no JSON: HP **(84,50)**, SP **(84,18)** |
| **Pequenas animadas** | `sp_fan_0-2`, `sp_lamp_*`, `sp_wave_0-3`, `sp_bubble_0-2` | posições no JSON |
| **FX** | `fx_flame_0-5`, `fx_activate_0-2`, `fx_shutdown_0-3`, `fx_spark_*` | presos no bocal (178,43 px na arte; **(356,26)** em UI a partir do canto inferior esquerdo) |

Nada vem com partícula, preenchimento ou brilho "assado" na moldura. Cada camada é um PNG próprio.

## 3. Ordem das camadas (de baixo para cima)
1. `hud_plate`
2. `hud_name`
3. `hp_track`
4. `hp_damage_lag` (Filled)
5. `hp_fill` **ou** `hp_fill_low` (Filled)
6. `hp_heal_flash`
7. `hp_housing`
8. `hp_emblem` **ou** `hp_emblem_low`
9. `hp_dial_crack`
10. `hp_lowhealth_glow`
11. `sp_track`
12. `sp_fill_low` | `sp_fill_mid` | `sp_fill_full` (Filled; só um ativo por vez)
13. `sp_wave_N` e 14. `sp_bubble_N`: os dois **dentro de um RectMask2D** que vai de x=84 até 84 + 206·fill
15. `sp_refill_flash`
16. `sp_glass`
17. `sp_housing`
18. `sp_turbine`
19. `sp_fan_N`
20. `sp_lamp_*`
21. `sp_full_glow`
22. `fx_flame_N` / `fx_shutdown_N`
23. `fx_activate_N`
24. centelhas

## 4. HP (integridade do casco)
- `hp_fill.fillAmount = hp`, aplicado na hora.
- **Damage lag:** `hp_damage_lag.fillAmount` **segura 0.35 s** e depois desce **0.9 da barra por segundo** até alcançar `hp`. Em uma cura, o lag pula direto para `hp`.
- **Cura:** `hp_heal_flash` usa o mesmo `fillAmount` de `hp_fill`. Ele aparece com alpha 1 e some em **0.25 s**.
- **HP crítica (`hp <= 0.25`):**
  - troca `hp_fill` → `hp_fill_low`;
  - troca `hp_emblem` → `hp_emblem_low`;
  - liga `hp_dial_crack`;
  - `hp_lowhealth_glow` pulsa o alpha entre 0.25 e 1 num **período de 0.9 s**.
- O `PlayerHUD.cs` atual usa arrays de sprites (9 de HP). Na v2 isso vira `fillAmount`. Os sprites antigos ficam sem uso.

## 5. SP (reservatório, turbina e chama)
**Estados**, comparando com as marcas de bronze no vidro:

| SP | Fill | Lâmpada | Pás | Chama |
|---|---|---|---|---|
| `< 1/3` | `sp_fill_low` (azul escuro) | `sp_lamp_low` (vermelha) | paradas (`sp_fan_0`) | — |
| `1/3 – <1` | `sp_fill_mid` (azul) | `sp_lamp_mid` (âmbar) | giram, **80 ms/frame** | — |
| `= 1` | `sp_fill_full` (ciano claro) | `sp_lamp_full` (ciano) | giram, **50 ms/frame** | **liga** |

Detalhes do comportamento:
- **Onda:** `sp_wave_0-3` em loop de **100 ms/frame** enquanto `sp > 0`.
- **Bolhas:** só aparecem **enquanto recarrega**. No máximo 3 por vez. Sobem e andam para a direita a ~40 px/s (arte x1), trocando `sp_bubble_0→2` a cada 100 ms. A máscara as corta no nível atual.
- **Ganho de carga:** `sp_refill_flash`, com o mesmo `fillAmount`, some em 0.2 s.

**Sequência ao encher (`sp` chega a 1):**
1. `fx_activate_0→2`, a **100 ms** cada, centrado no bocal. Ao mesmo tempo:
   - uma rajada de **6–9 centelhas** (`fx_spark_*` aleatórias);
   - vida de 0.25–0.5 s;
   - cone de ~50° para a direita;
   - 25–60 px/s (arte x1).
2. Depois, `fx_flame_0→5` em loop a **80 ms/frame**, com pivot no bocal `(0, 0.5)`.
3. Enquanto cheio:
   - `sp_full_glow` pulsa o alpha entre 0.35 e 1 num **período de 1.2 s**;
   - centelhas esparsas (~2.5/s).

**Ao gastar o especial:** a chama para e toca `fx_shutdown_0→3` a **100 ms** cada (a chama encolhe e solta uma baforada de fumaça). Depois esconde. O fill volta ao estado correspondente.

Sugestão de implementação: um `Animator` pequeno no objeto `TurbineFX`, com os estados `Off → Activate → FlameLoop → Shutdown → Off` e os triggers `Full` e `Spend`. As centelhas podem ser um `ParticleSystem` com UI (ou Images de um pool). Nenhuma delas faz parte das molduras.

## 6. Arquivos
```
HUD_Player_v2/
  parts_x1/, parts_x2/     peças da HUD (PNG transparente)
  fx_x1/, fx_x2/           chama, ignição, desligamento, centelhas
  states/                  HUD composta em cada estado (referência, NÃO importar)
  previews/                mockups na tela do jogo, comparações, GIFs das animações, folhas de peças
  hud_v2_layout.json       retângulos, âncoras, posições na UI, ordem, tempos, paleta
  hud_v2_x1.png / _x2.png  composição de referência
  tools/hud2.py            gera todas as peças  (python3 hud2.py <pasta_saida>)
  tools/previews2.py       gera previews e mockups (caminhos dos sprites do jogo estão no topo)
  tools/build_docs.py      reescreve a parte de peças e tempos do JSON
```

## 7. Mockups
`previews/compare_screens_original_v1_v2.png` e `compare_hud_zoom_x2.png` comparam três versões: **original**, **v1** e **v2** (estática e em ação).

**Atenção:** o projeto não tinha um screenshot salvo. A tela "original" foi **reconstruída** a partir de:
- `background.png`;
- os sprites do Bombardiro e do Gusini;
- a HUD antiga montada nas posições exatas do `PlayerHUD.prefab` (portrait, `hp_bar`, `special`, nome, Pontos, Inimigos).

`screen_v2_static.png` mostra a v2 **sem nenhuma animação**. A identidade nova (mostrador, placa de cockpit, blindagem, tanque e turbina) é visível sem chama, pulso ou brilho.

## 8. Fora do escopo
- O `PlayerHUD.cs` e o prefab **não foram alterados**.
- A barra do boss não foi tocada.
- Não há portraits.
- Os textos Pontos e Inimigos continuam onde estão no prefab (x=450). A placa termina em x≈380 UI e a chama mais longa em x≈430, então não há colisão.
