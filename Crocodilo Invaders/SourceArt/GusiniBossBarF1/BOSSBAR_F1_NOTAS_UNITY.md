# Boss bar do Gusini, Fase 1: notas para a Unity

## Conceito
A barra é uma placa roxa em forma de **asa**, com as penas na borda de baixo. Os elementos são:

- **À esquerda:** a ponta da moldura é um **bico de ganso** de latão.
- **À direita:** um **medalhão de comandante** com **asas de aviador** (brevê). Dentro dele há uma silhueta simplificada do ganso (cabeça, óculos e cachecol). Não é um retrato.
- **Embaixo do medalhão:** o **cachecol vermelho** com laço, tremulando para trás.
- **Acima:** a placa **"GUSINI"** com uma estrela de comandante.

A vida tem cor âmbar quente e fica vermelha quando está crítica. **O fill esvazia da esquerda para a direita** (Fill Origin = Right): a barra é espelhada em relação à HUD do player, no estilo "versus".

Essa é a versão **Fase 1**: elegante, sem corrupção. As fases 2 e 3 ainda não existem.

Fonte da verdade: **`bossbar_f1_layout.json`**. Lá estão os retângulos, as posições na UI, a ordem de camadas e os tempos.

## 1. Import e Canvas
- Arte nativa de **238×50 px**, exibida em **x2**, ou seja, **476×100 unidades de UI** (Canvas 1200×720, Scale With Screen Size, match 0.5).
- Escolha **um** dos conjuntos:
  - `parts_x2` + `fx_x2` com **PPU 100**;
  - `parts_x1` + `fx_x1` com **PPU 50**.
- Em todos: Filter **Point**, Compression **None**, sem mipmaps, `Set Native Size`.
- **Posição recomendada:** canto **inferior direito**, espelhando a HUD do player.
  - Raiz com anchor/pivot `(1,0)`, `anchoredPosition (-16,14)`, `sizeDelta (476,100)`.
  - **Variante no topo:** anchor/pivot `(0.5,1)`, `anchoredPosition (0,-12)`. Os assets são os mesmos.
- Dentro da raiz, todas as peças usam **anchor e pivot (0,0)**. O JSON traz `anchoredPosition_ui_from_bottomleft` de cada uma. A exceção é o medalhão: para fazer pop/escala, use o pivot no centro dele, que fica em (209,27) px nativos.

## 2. Peças e camadas (de baixo para cima)
| # | Peça | Tipo | Função | Animação |
|---|---|---|---|---|
| 1 | `lowhp_glow` | Simple | contorno vermelho de alerta | pulso na vida crítica |
| 2 | `orn_scarf_tail_0-3` | Simple, troca de sprite | pontas do cachecol | loop contínuo |
| 3 | `boss_backplate` | Simple | placa-asa escura (contraste sobre céu claro) | entrada |
| 4 | `nameplate` | Simple | placa do nome | entrada |
| 5 | `orn_star` | Simple | estrela de comandante | entrada |
| 6 | `name_text` | Simple | "GUSINI". Pode virar TMP: PressStart2P, 16 em UI | entrada |
| 7 | `hp_track` | Simple | trilho vazio com "barbas de pena" | — |
| 8 | `hp_damage_lag` | **Filled, Horizontal, Origin Right** | vida atrasada (creme) | damage lag |
| 9 | `hp_fill` / `hp_fill_low` | **Filled, Horizontal, Origin Right** | vida (âmbar ou vermelho) | carga na entrada, dano |
| 10 | `hp_hit_flash` | **Filled, Origin Right**, mesmo `fillAmount` | flash branco | dano e fim da entrada |
| 11 | `fx_shimmer` | Simple, dentro de um **RectMask2D** do tamanho do fill atual | brilho que varre a vida | ocioso |
| 12 | `fx_fill_edge` | Simple | ponta brilhante da carga | entrada |
| 13 | `boss_frame` | Simple | moldura de latão; marcas de 25/50/75%; janela transparente | entrada |
| 14 | `orn_beak` | Simple | ponta em forma de bico | entrada |
| 15 | `orn_wings` | Simple | asas de aviador | entrada |
| 16 | `emblem_roundel` | Simple | medalhão de comandante | entrada |
| 17 | `emblem_alert` | Simple + alpha | anel interno e lente vermelhos | pulso na vida crítica |
| 18 | `orn_scarf_knot` | Simple | laço do cachecol | entrada |
| 19 | `fx_hit_spark_0-2` | Simple, pool | estilhaço na borda da vida | dano |
| 20 | `fx_glint_0-3` | Simple | estrelinha de brilho | ocioso e fim da entrada |

Os fills são recortados exatamente na área da vida: **166×9 nativo (332×18 UI)**, em **(32,34)** UI. Nada está "assado" na moldura.

## 3. Animações
Todas são curtas e discretas. Os tempos também estão em `timings` no JSON.

**Entrada (≈1.3 s)**, disparada em `BossStarted`:
| Tempo | O quê |
|---|---|
| 0.00–0.28 s | Grupo **bar** (backplate, frame, bico, trilho e fills) desliza 120 UI px da direita para o lugar, com OutCubic, e o alpha vai de 0 a 1 (CanvasGroup) |
| 0.18–0.40 s | Grupo **emblem** (asas, medalhão, laço, cachecol) desce 20 UI px com **OutBack** (um pequeno "quique") |
| 0.32–0.46 s | Grupo **name** desce 12 UI px, com fade in |
| 0.50–1.10 s | **Carga:** `fillAmount` vai de 0 a 1 com OutCubic, com `fx_fill_edge` acompanhando a borda esquerda do fill |
| 1.10–1.30 s | Flash final (`hp_hit_flash`, alpha de 0.9 a 0) e `fx_glint` no medalhão (4 frames × 60 ms) |

O dano recebido durante a entrada só é mostrado depois da carga. Guarde o valor e aplique quando ela terminar.

**Dano**, em `HealthChanged` com perda de vida:
- `hp_fill.fillAmount` vai na hora para o novo valor.
- `hp_hit_flash`: alpha de 0.85 a 0 em **0.12 s**.
- `fx_hit_spark_0→2` a **50 ms** cada, na borda esquerda do fill: `x = 32 + 332·(1 − hp)` UI.
- **Shake** de 2 UI px na raiz por **0.10 s**.
- **Damage lag:** segura **0.40 s** e depois desce **0.8 da barra por segundo** até alcançar o `hp`.
- Para golpes muito seguidos (tiro contínuo), limite o flash e o spark a um a cada ~80 ms e deixe o lag acumular.

**Brilho ocioso (shimmer):** a cada **4.5 s**, `fx_shimmer` varre a vida da direita para a esquerda em **0.55 s**. Logo depois, um `fx_glint` de 4 frames × 60 ms aparece, alternando entre o medalhão e o bico. As posições estão em `glint_spots_x1`. O shimmer fica desligado em vida crítica.

**Vida crítica (≤ 30%):**
- troca `hp_fill` por `hp_fill_low`;
- `emblem_alert` pulsa o alpha entre 0.25 e 1;
- `lowhp_glow` pulsa entre 0.15 e 0.9;
- os dois no mesmo **período de 0.8 s**;
- o cachecol acelera para 80 ms por frame.

**Cachecol:** `orn_scarf_tail_0→3` em loop a **120 ms** por frame. Ele fica sempre ligado, inclusive com a barra parada.

## 4. Ligação com o `BossHUD.cs` atual (sugestão, sem alterar código agora)
- O script hoje troca sprites (`healthSprites[]`) e tem `portraitImage`. Na nova barra:
  - use `fillAmount = health.Fraction` nos dois fills;
  - deixe `portraitImage` **vazio** (o script já trata `null`) ou remova o objeto `Portrait` do prefab.
- `nameText`: pode continuar como TMP, ou ser substituído pelo `name_text.png`. O nome do PNG é fixo "GUSINI".
- `ShowEnrage` muda a cor do nome. Com o PNG isso não se aplica; se quiser manter, use TMP.
- Um componente pequeno `BossBarFX` (Animator ou corrotinas) cuida de entrada, flash, lag, shimmer, vida crítica e cachecol.

## 5. Arquivos
```
BossBar_Gusini_F1/
  parts_x1/, parts_x2/       peças (PNG transparente, recortadas)
  fx_x1/, fx_x2/             glint, hit_spark, fill_edge, shimmer
  states/                    barra montada em cada estado (referência, NÃO importar)
  previews/                  GIFs/APNGs das animações, folhas de peças, montagens ampliadas, telas do jogo
  bossbar_f1_layout.json     posições, ordem, tempos, paleta
  bossbar_f1_x1.png / _x2    composição de referência
  tools/                     bossbar1.py, bossbar1_previews.py, build_docs.py
```

## 6. Observações
- A tela "antes" da comparação foi **reconstruída** a partir do `BossHUD.prefab`: barra 350×30, nome, portrait 120×120. Não havia screenshot no projeto.
- Nos mockups, a HUD do player é a **v2**.
- Nada foi gravado no projeto Unity.
