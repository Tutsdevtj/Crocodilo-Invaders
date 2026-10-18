# Gusini F2 — Ataque 1: Rajada Corrompida (3 bolas de energia gigantes)

O canhão com pinças da Fase 2 carrega e lança **3 bolas de energia gigantes**, quase do tamanho do Bombardiro. Elas saem **uma por vez, com 0,3 s entre uma e outra**. Cada bola faz **o próprio zigue-zague vertical, quase na altura da tela inteira**. Esse movimento é feito na Unity; na preview em tela ele é só ilustrativo.

- **Arte:** recomposta das **peças aprovadas da Fase 2**. As pinças do canhão abrem por cisalhamento das pontas, com a base fixa.
- **Canvas e pivô:** **256×196**, pivô **(120,118)** = Unity **(0.4688, 0.398)**, os mesmos da idle, do shoot da F1 e da transformação.
- **Início e fim:** o F0 e o F12 são **idênticos** à pose base da F2 (0 px de diferença).
- **Movimento:** o shake do boss inteiro fica com a Unity. A spritesheet só tem o recoil do canhão, a mira de nariz e o tranco da cabeça no 3º disparo.

## 1. Frames (não-loop · 13 frames · 1530 ms)
| F | ms | Início | O que acontece | Evento |
|---|---|---|---|---|
| 0 | 60 | 0 | Pose base F2 | `telegraph_start` |
| 1 | 90 | 60 | Antecipação: nariz mira, pinças abrem, faíscas convergem, olho do módulo acende | |
| 2 | 100 | 150 | Carga 1 | |
| 3 | 110 | 250 | Carga 2 | |
| 4 | 130 | 360 | **Carga máxima, segura**: esfera de 24 px na boca, pinças abertas | `charge_full` |
| 5 | 60 | 490 | **DISPARO 1**: flash, canhão recua | `fire_1` |
| 6 | 240 | 550 | Recarga | |
| 7 | 60 | 790 | **DISPARO 2** | `fire_2` |
| 8 | 240 | 850 | Recarga mais forte | |
| 9 | 70 | 1090 | **DISPARO 3**: recuo maior, cabeça chuta | `fire_3` |
| 10 | 120 | 1160 | Recoil e vapor | `recover_start` |
| 11 | 140 | 1280 | Pinças fechando | |
| 12 | 110 | 1420 | Volta à base (= F0) | `recover_end` |

- **Telegraph:** 490 ms.
- **Disparos:** a cada **300 ms** (490 / 790 / 1090 ms).
- **Janela de dano:** a partir de `recover_start`.
- **Primeiro ataque após a transformação:** segure o F4 por +300 ms.

## 2. Spawn da bola
- Centro da bola: **(−18, 142)** no canvas = **(−138, −24) px a partir do pivô** (÷ PPU para unidades). Isso fica fora do canvas, à esquerda, de propósito: a bola é grande demais para nascer entre as pinças, então nasce à frente delas, com a borda encostando no flash.
- Em cada `fire_N`: instanciar a bola, tocar **`ball_spawn_0→2`** (60 ms cada; a esfera cresce de 20 a 48 px) e depois o **loop**.
- O zigue-zague, a velocidade e a fase de cada bola ficam no seu script. Como são independentes, cada bola pode começar subindo ou descendo, e com período próprio.

## 3. Projétil (pasta `projectile/`)
| Arquivo | Tamanho | Uso |
|---|---|---|
| `gusini_p2_energy_ball_0-5` | **72×72**, pivô (0.5, 0.5) | **loop**, 70 ms/frame: espiral de energia girando, núcleo pulsando ±1 px, raios na borda |
| `gusini_p2_energy_ball_spawn_0-2` | 72×72 | nascimento, 60 ms cada |
| `gusini_p2_energy_ball_pop_0-4` | 72×72 | dissipação ao acertar ou sair, 50 ms cada |
| `gusini_p2_energy_trail_0-2` | 24×16 | rastro opcional: tufos roxo-escuros com faíscas verdes |

- **Tamanho:** núcleo de **~56 px** (halo até ~62 px). O Bombardiro tem 116×95.
- **Hitbox sugerida:** círculo de **raio 22–24 px** (o núcleo tem 27). Não conte o halo nem os raios.
- **Leitura:** borda verde-escura e halo pontilhado, espiral, **olho alien em fenda** no centro e nenhum contorno preto. Não se confunde com o tiro verde do player, que é pequeno e tem contorno preto.

## 4. FX soltos (pasta `fx/`, já compostos nos frames)
- `fx_charge_orb_0-3`: carga;
- `fx_charge_gather_0-1`: faíscas;
- `fx_muzzle_flash_0-1`: flash;
- `fx_vent_0-2`: vapor.

`parts/` traz cada frame separado em sprite (boss) e fx.

## 5. Arquivos
```
frames/  gusini_p2_energy_shot_sheet.png (+_x2)  gusini_p2_energy_shot.json
projectile/  fx/  parts/  tools/
previews/  _x3.gif · _canhao_x6.gif · _em_tela.gif · energy_ball_loop_x4.gif · projetil_folha_x3.png · comparacao_tamanho_x3.png
```

## Autorrevisão
- Canvas e pivô idênticos em todos os frames; a nave não muda de largura.
- F0 = F12 = pose base da F2.
- Alpha só 0/255, sem anti-aliasing.
- A cabeça só dá um tranco de 1 px.
- A bola foi checada no tamanho nativo (72 px) e em tela: as três são legíveis e distintas no zigue-zague.
