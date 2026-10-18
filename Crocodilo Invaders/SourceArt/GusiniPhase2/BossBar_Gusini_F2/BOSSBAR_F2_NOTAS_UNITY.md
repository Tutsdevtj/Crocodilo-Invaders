# Boss bar do Gusini, Fase 2 (corrompida): notas para a Unity

## Conceito
A barra segue o **mesmo conceito e o mesmo layout da F1**: placa em forma de asa, bico na ponta esquerda, medalhão de aviador à direita, cachecol, placa "GUSINI" e vida que esvazia na direção do bico (Fill Origin = Right). A corrupção alien da transformação tomou **parte** dela:

- **Vida verde alien**, com veias de energia correndo por dentro.
- **Carapaça do módulo alien** (as mesmas cores da F2) cobre a metade direita da placa, da moldura e do aro do medalhão. As penas da borda viraram lâminas, e há espinhos no dorso.
- **Olho alien** no aro do medalhão, piscando. Está separado, para animar.
- **Bico entreaberto** (o rosnado da F2), com brilho verde dentro.
- **Asa direita** do brevê virou uma lâmina de carapaça quebrada. A **asa esquerda dourada continua intacta**.
- **Ícone do ganso** com olho verde e bico aberto.
- **Cachecol** rasgado e desfiado.
- **Placa do nome** escurecida, rachada, com falha de sinal ocasional.

**Corrupção parcial, de propósito:** o latão roxo e dourado ainda resiste na metade esquerda. Isso combina com a ideia de que o Gusini é recuperável.

Fonte da verdade: **`bossbar_f2_layout.json`**. Os retângulos `rect_x1` das peças estão em `pieces_raw`.

## 1. Mesmo setup da F1
- Arte nativa de 238×50, exibida em x2 = **476×100 UI**.
  - `parts_x2` e `fx_x2` com PPU 100, ou `parts_x1` e `fx_x1` com PPU 50.
  - Point, sem compressão.
- **Mesmos nomes de peça e mesmas posições da F1.** O mais simples é usar **o mesmo prefab** e só **trocar os sprites** na transição. As fills continuam em (32,34) UI, com 332×18.
- **Peças novas:**

| Peça | Função | Animação |
|---|---|---|
| `emblem_eye_0-3` | olho alien no aro: aberto, semicerrado, fechado, aberto com brilho | piscar a cada ~2.6 s (0→1→2→1→0, 60–100 ms); frame 3 em vida crítica |
| `name_text_glitch_1-2` | nome com falha de sinal | trocar por 40–60 ms a cada 2–4 s (aleatório) |
| `takeover_0-3` | rachaduras verdes crescendo **sobre a barra da F1** (canvas inteiro) | só na entrada (60 ms cada) |
| `fx_mote_0-2` | partícula verde subindo da vida | 6 partículas em loop, ~1.1 s cada, nascendo ao longo do fill |

## 2. Ordem das camadas
É a mesma da F1, com acréscimos:

`lowhp_glow` → `orn_scarf_tail` → `boss_backplate` → `nameplate` / `orn_star` / `name_text` → `hp_track` → `hp_damage_lag` → `hp_fill` (ou `hp_fill_low`) → `hp_hit_flash` → `fx_shimmer` (RectMask2D) → `fx_fill_edge` → `boss_frame` → `orn_beak` → `orn_wings` → `emblem_roundel` → `emblem_alert` → **`emblem_eye_N`** → `orn_scarf_knot` → **`fx_mote_N`** → `fx_hit_spark_N` → `fx_glint_N`

Durante a entrada, **`takeover_N`** vai por cima de tudo.

## 3. Entrada da F2, ligada à transformação
A barra **não desliza** como na F1. Ela é **tomada pela corrupção no lugar**, sincronizada com `gusini_transform_f1_f2.json`:

| Tempo (a partir de `phase2_reveal_frame` = 2820 ms) | O quê |
|---|---|
| durante toda a derrota aparente | a barra da **F1 fica vazia** na tela |
| 0.00–0.24 s | `takeover_0→3` sobre a barra da F1 (60 ms cada): rachaduras verdes saindo do medalhão |
| 0.24–0.36 s | flash branco: silhueta da barra da F1 e, em seguida, da F2. **Troque os sprites para a F2 aqui.** |
| 0.40–0.98 s | `fillAmount` vai de 0 a 1 (OutCubic) com `fx_fill_edge` na borda. Olho: fechado → semicerrado → aberto. **Não aplique o estado de vida crítica durante a carga.** |
| 0.98–1.18 s | flash final no fill e `fx_glint` no olho. A carga termina no `phase2_ready_frame` (3800 ms). |

## 4. Comportamentos da F2
São os mesmos sistemas da F1, com outro clima:

- **Dano:** mesma regra da F1. Flash branco de 0.12 s; `fx_hit_spark` (verde e branco) na borda; shake de 2 UI px; lag segura 0.40 s e desce 0.8 da barra por segundo.
- **Pulso de energia** (o shimmer verde): a cada **3.5 s**, varrendo em 0.45 s.
- **Olho piscando:** a cada ~2.6 s.
- **Falha do nome:** de vez em quando. Opcional: deslocar 1 px duas fileiras da barra inteira por 1 frame (preview `anim_ocioso_f2`).
- **Partículas verdes:** sobem da vida o tempo todo.
- **Cachecol rasgado:** `orn_scarf_tail_0-3` a 100 ms.
- **Vida crítica (≤ 30%):**
  - o fill vira **amarelo-ácido**, como energia instável (`hp_fill_low`);
  - `emblem_alert` (vermelho) e `lowhp_glow` pulsam com **período de 0.7 s**, mais rápido que na F1;
  - o olho fica no frame 3 (brilhando) nos picos do pulso;
  - as falhas ficam mais frequentes;
  - o cachecol passa a 70 ms.

## 5. Arquivos
```
BossBar_Gusini_F2/
  parts_x1/, parts_x2/   peças (mesmos nomes da F1 + emblem_eye_0-3, name_text_glitch_1-2, takeover_0-3)
  fx_x1/, fx_x2/         fx_glint (verde), fx_hit_spark, fx_fill_edge, fx_shimmer, fx_mote_0-2
  states/                montagens de referência (não importar)
  previews/              entrada (tomada), damage lag, vida crítica, ocioso, cachecol+olho x8,
                         comparação F1×F2, telas (base/topo/dano/crítica),
                         transformacao_com_barras_em_tela.gif (transformação + barra F1 vazia + barra F2 subindo)
  bossbar_f2_layout.json
  bossbar_f2_x1.png / _x2.png
  tools/                 bossbar2.py (gera a partir do bossbar1.py da F1), bossbar2_previews.py
```

Nada foi alterado no projeto Unity nem no `BossHUD.cs`.
