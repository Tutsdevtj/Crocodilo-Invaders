# Gusini F2 — Ataque 2: Ovo Corrompido

O Gusini solta um ovo corrompido pela escotilha da barriga, com a mesma soltura aprovada da F1, agora com luz verde vazando. O ovo abre um paraquedas rasgado com um **POP bem marcado** e desce pulsando verde, numa contagem que acelera. Depois racha duas vezes com luz verde e **estoura em 5 gemas ácidas** em leque. **O ovo não pode ser abatido.**

- **Boss:** recomposto a partir das **peças aprovadas da F2**. A escotilha usa o mesmo mecanismo e os mesmos estados da F1 (abre em etapas, sem "pop" de sprite).
- **Canvas e pivô:** **256×196**, pivô **(120,118)** = Unity **(0.4688, 0.398)**.
- **Início e fim:** F0 = F8 = pose base da F2 (0 px de diferença).

## 1. Animação do boss (não-loop · 9 frames · 710 ms)
| F | ms | Início | O que acontece | Escotilha | Evento |
|---|---|---|---|---|---|
| 0 | 80 | 0 | Pose base F2 | fechada | `telegraph_start` |
| 1 | 100 | 80 | Trava pisca **verde**, olho do módulo acende, cabeça olha para baixo | trava | |
| 2 | 60 | 180 | Abrindo 1: **luz verde vaza** para baixo | 18° | `hatch_open` |
| 3 | 70 | 240 | Abrindo 2 | 40° | |
| 4 | 90 | 310 | Aberta: **o ovo corrompido aparece** | 62° | |
| 5 | 90 | 400 | **SOLTA O OVO** (bico mais aberto) | 66° | `egg_drop` |
| 6 | 70 | 490 | Fechando 1 | 38° | `recover_start` |
| 7 | 60 | 560 | Fechando 2 | 15° | |
| 8 | 90 | 620 | Fechada (= F0) | fechada | `recover_end` |

- **Spawn do ovo:** **(94,152)** no canvas = **(−26, −34) px a partir do pivô**, no início do F5. No 1º frame de vida, o ovo fica **atrás** do boss, como na F1.
- O F4 já mostra o ovo exatamente onde a entidade nasce, então não há salto na troca.

## 2. Ovo (entidade) — estados e tempos sugeridos
Canvas do ovo **24×28**, pivô no centro (0.5, 0.5).

| Estado | Sprite | Tempo | Nota |
|---|---|---|---|
| `Fall` | `egg_p2_idle` | 250 ms | queda livre |
| `Deploy` | `parachute_p2_deploy_0→2` (60/70/80 ms) + **`fx_parachute_pop_0→1`** (60/70 ms) | 210 ms | **O POP é o aviso**: flash branco, anel verde e raios em volta do saquinho, 22 px acima do pivô do ovo |
| `Descent` | `parachute_p2_0/1` em loop (160 ms) + ovo | **1,0–1,3 s** | **Contagem**: troque o ovo por `pulse_1` em 0,20–0,36 s e 0,55–0,68 s, e por `pulse_2` em 0,82–0,92 s e 1,00–1,08 s. Os pulsos ficam cada vez mais próximos e fortes |
| `Crack1` | `egg_p2_crack_1` | 200 ms | rachaduras vazando luz verde |
| `Crack2` | `egg_p2_crack_2` | 160 ms | tampa levanta, gema verde brilhando, halo |
| `Burst` | `egg_p2_burst` (44×44) | 120 ms | **`OnEggBurst`**: criar as 5 gemas no pivô |

- Do primeiro pulso `pulse_2` até o estouro passam **~640 ms**. Do Crack1 até o estouro, **360 ms**.
- **Velocidades sugeridas:** descida 20–25 px/s, deriva 15 px/s para a esquerda.
- **Paraquedas (modular, como na F1):** canvas 36×26, `attach_point` (18,25) no topo do ovo, que fica em (12,4) no canvas 24×28.
- O paraquedas está **rasgado** (furos e entalhes), mas mantém a silhueta. Ele **não tem verde**, para contrastar com o ovo.
- Depois do estouro, dá para reaproveitar o `parachute_collapsed.png` da F1, voando embora.

## 3. Gemas ácidas (pasta `yolks/`)
São 5 sprites já orientados, de 22×22 com pivô no centro. **Não aplique rotação.**

| Arquivo | Direção (Unity) |
|---|---|
| `yolk_p2_up` | (0, 1) |
| `yolk_p2_up_left` | (−0.707, 0.707) |
| `yolk_p2_left` | (−1, 0) |
| `yolk_p2_down_left` | (−0.707, −0.707) |
| `yolk_p2_down` | (0, −1) |

- O leque cobre 180° para a esquerda, com **45° entre as gemas**, o que garante o vão.
- Velocidade sugerida: 130–160 px/s.
- Hitbox: raio ~4 px (a cabeça da gota).
- Visual: gota verde-ácida com contorno escuro, brilho e **pupila escura**, a mesma linguagem alien da bola de energia.

## 4. Arquivos
```
frames/  gusini_p2_egg_drop_sheet.png (+_x2)  gusini_p2_egg_drop.json
parts/       sprite e fx separados por frame do boss
egg/         egg_p2_idle, pulse_1, pulse_2, crack_1, crack_2, burst
parachute/   parachute_p2_deploy_0-2, parachute_p2_0-1
yolks/       yolk_p2_up, up_left, left, down_left, down
fx/          fx_parachute_pop_0-1
previews/    gusini_p2_egg_drop_x3.gif · sequencia_ovo_completa_x3.gif · gusini_p2_egg_drop_em_tela.gif
             ovo_folha_x5.png · comparacao_f1_f2_x5.png
tools/       egg_p2.py, egg_p2_previews.py
```

## Autorrevisão
- Canvas e pivô idênticos em todos os frames. A nave não muda de largura; o corpo sobe só 1 px, como na F1.
- F0 = F8 = pose base da F2.
- Alpha só 0/255, sem anti-aliasing.
- A cabeça não muda de tamanho: só inclina 1–2 px para olhar o ovo.
- Paraquedas checado: a abertura é clara (POP com flash e anel) e a cor é diferente da do ovo.
- O ovo da F2 continua reconhecível como o ovo da F1 (mesma forma, faixa roxa), com veias e emblema verdes.
