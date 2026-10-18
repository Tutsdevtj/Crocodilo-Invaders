# Gusini: transformação Fase 1 → Fase 2

Esta é a **única** transformação da luta. A Fase 3 foi descartada.

- **Forma final:** a **Fase 2 já aprovada**. Nenhuma peça foi redesenhada. Os frames recompõem as peças aprovadas das duas fases, que têm a mesma montagem, os mesmos offsets e os mesmos pivôs.
- **Em campo:** o Gusini nunca sai da tela.
- **Duração:** **4,1 s**, em 30 frames com tempos variáveis.
- **Canvas e pivô:** **256×196**, pivô **(120,118)** = Unity (0.4688, 0.398), os mesmos da `gusini_p1_idle`. É só trocar o clip no mesmo SpriteRenderer.
- **Fonte da verdade:** `gusini_transform_f1_f2.json`, com frames, tempos, eventos e camadas.

## 1. Roteiro
| Trecho | Frames | Tempo | O que acontece |
|---|---|---|---|
| **Golpe final** | 0–1 | 0–160 ms | Flash branco estilo RPG e estrela de impacto. O Gusini é empurrado para trás e o nariz sobe. A barra da F1 chega a 0. |
| **Derrota aparente** | 2–8 | 160–1510 ms | O motor engasga (faíscas) e solta fumaça escura do motor e do cockpit. A nave perde altura (afunda 11 px) e mergulha de nariz. A cabeça cai e o cachecol murcha. Há uma **pausa longa e silenciosa** (frames 6–7, 640 ms): o jogador acha que venceu. No frame 8 aparece um **único brilho verde** no lugar do módulo, o "primeiro sinal". |
| **Corrupção** | 9–20 | 1510–2530 ms | Veias verdes da F2 vazam pela nave, a partir do ponto do módulo. O módulo alien **brota**, com raios de energia e partículas subindo. **Alternância acelerando:** F1 → flash branco → F1 mais corrompida → silhueta verde da F2 → **frame intermediário** (traseira já alien, rosto ainda da F1) → flash → **"resistência"** (volta à F1 por um instante) → F2 sem o rosto → flashes. A nave vai sendo **erguida** pela energia (o mergulho se desfaz). |
| **Pico** | 21–22 | 2530–2820 ms | Silhueta branca da F2 com clarão central, anel de energia e estilhaços. |
| **Revelação** | 23–25 | 2820–3230 ms | A F2 aparece em cores, **instável**: peças tremem 1 px, a aura verde vai diminuindo e sobra o último raio. |
| **Retomada** | 26–29 | 3230–4100 ms | **Pose de retorno:** a cabeça sobe com o bico entreaberto, num "rugido" (280 ms), e a nave sobe 2 px. Depois assenta na pose base da F2. **Pronto.** |

## 2. Eventos (frame / ms)
| Evento | Frame | ms |
|---|---|---|
| `final_hit_frame` | 0 | 0 |
| `start_stagger_frame` | 2 | 160 |
| `apparent_defeat_hold_frame` | 6 | 690 |
| `first_sign_frame` | 8 | 1330 |
| `corruption_start_frame` | 9 | 1510 |
| `intermediate_frame` | 13 | 1930 |
| `resist_flicker_frame` | 15 | 2110 |
| `transformation_peak_frame` | 21 | 2530 |
| `phase2_reveal_frame` | 23 | 2820 |
| `return_pose_frame` | 26 | 3230 |
| `phase2_ready_frame` | 29 | 3800 (fim do clip em 4100) |

**Sugestões de gameplay** (só sugestões; nada foi implementado):
- **Invulnerabilidade:** durante toda a sequência.
- **Tiros do boss:** limpar no `final_hit_frame`.
- **Caixas de vida:** 1 ou 2, criadas no `start_stagger_frame`. A janela segura vai de 160 a 1510 ms: o Gusini está afundando e não ataca.
- **Screen shake:** leve no golpe final e forte no pico.
- **Nova barra de vida:** sobe de `phase2_reveal_frame` até `phase2_ready_frame` (~1 s).
- **IA da Fase 2:** começa ao fim do clip.

As caixas de vida que aparecem no GIF em tela são **placeholders**, só para mostrar o timing.

## 3. Camadas e arquivos
```
frames/                 30 frames completos (sprite + corrupção + fx)
layers/sprite/          só o boss em cada frame (inclui os frames de flash branco/verde)
layers/corruption/      overlay das veias verdes e do módulo brotando sobre a F1
layers/fx/              só os efeitos (aura, fumaça, raios, anel, partículas)
fx/                     FX avulsos reutilizáveis:
   fx_tf_smoke_0-3      fumaça (loop, 100 ms/frame)
   fx_tf_ring_0-2       anel de energia (pico/revelação)
   fx_tf_burst          clarão central do pico
   fx_tf_shards         estilhaços
   fx_tf_bolts_0-2      raios verdes
   fx_tf_hit_0-1        estrela do golpe
   fx_tf_sputter_0-1    faíscas do motor
   fx_tf_mote           partícula verde
   gusini_p1/p2_flash_white, gusini_p1/p2_flash_green
                        silhuetas de flash (canvas inteiro, mesmo pivô): servem para piscar o boss na Unity sem shader
   gusini_p2_aura_lv1-3 aura verde da F2 parada (canvas inteiro)
gusini_transform_sheet.png   spritesheet 6×5 (256×196 por célula)
previews/
   gusini_transform_x3_timeline.gif   ampliada, com linha do tempo e eventos
   gusini_transform_em_tela.gif       em tela, com o fundo novo, o player e a HUD
   sheet_rotulado.png                 todos os frames com duração e evento
   inicio_vs_fim_x3.png
```

**Na Unity, há duas formas de usar:**
- **(a) Simples:** um clip com os 30 `frames/` e as durações do JSON.
- **(b) Modular:** o clip de `layers/sprite`, com `layers/fx` num filho por cima (e por trás, se preferir separar). Os FX de `fx/` podem ser reaproveitados em partículas próprias.

## 4. Revisão obrigatória
- **Comunica a derrota aparente?** Sim. Há golpe, empurrão, fumaça escura, motor engasgando e a nave afundando de nariz com a cabeça caída. Depois vêm **640 ms de pausa** sem nada acontecendo e só então o primeiro brilho verde. Esse silêncio é o que vende o "venci".
- **A corrupção está clara?** Sim. O verde nasce num ponto, vaza pelas veias e o módulo brota. A alternância F1/F2 acelera, com flashes brancos e verdes. Há um frame intermediário claro: a traseira já corrompida com o rosto ainda normal.
- **A forma final é a Fase 2 aprovada?** Sim. O último frame é a montagem da F2 aprovada, na pose base, peça por peça. Nada da antiga Fase 3 foi usado.
- **Está monstruosa demais?** Não. A corrupção fica na nave e nas veias. O rosto só muda no fim e continua o do Gusini (óculos, cachecol, bico). O momento de "resistência" (volta à F1 por um frame) reforça que ele ainda está ali dentro. Isso deixa a porta aberta para ele sobreviver e virar NPC depois.
- **Ainda parece o Gusini?** Sim. A silhueta, o cachecol, o bico e o óculos são os mesmos o tempo todo, e até os flashes mantêm a silhueta reconhecível.
- **É forte o bastante para ser a única transformação?** Sim. São 4,1 s com cinco atos (queda, silêncio, corrupção, pico e revelação com rugido), além de screen shake e da barra nova subindo.

Não foram feitos: ataques da F2, barra da F2, código Unity, nem qualquer forma nova.
