# Gusini F2 — Ataque 4: Tentáculos do Parasita (v2, refeito do zero)

A ideia continua a mesma; a arte foi refeita do zero.

1. O módulo do parasita racha e **dois tentáculos** rompem a carapaça:
   - **A:** grande, em arco de escorpião por cima da traseira, com o ferrão apontado para o player.
   - **B:** menor, enrolado atrás do motor.
2. Em cada golpe, **duas das três faixas** ficam marcadas por 900 ms, e a faixa sem marca é a segura.
3. Os tentáculos mergulham para trás (saem pela direita) e atravessam as faixas marcadas da direita para a esquerda, terminando num estalo na ponta.
4. São 3 golpes. No fim vem a **resistência**: a cabeça da F1 pisca e o **núcleo fica exposto por 2,0 s**.

- **Canvas e pivô:** **256×196**, pivô **(120,118)** = Unity (0.4688, 0.398).
- **Início e fim:** o F0 do início e o F7 da resistência são iguais à pose base da F2.

## O que melhorou em relação à v1
| v1 | v2 |
|---|---|
| 3 tentáculos finos e iguais, que liam como "macarrão" e emaranhavam a silhueta | **2 tentáculos com papéis claros**: um arco grande (ameaça) e um enrolado (apoio). Menos elementos, silhueta limpa |
| Tubo de largura quase constante | **Afunilamento real**, da base grossa a uma ponta fina com **ferrão**: bulbo, olho verde, farpas e gancho |
| Tentáculo de faixa montado com segmentos de 32 px repetidos (parecia vagão ou cano) | **Desenhado inteiro em cada frame**, ao longo de uma curva contínua, com volume cilíndrico iluminado pela mesma luz do boss (cima/esquerda) |
| Placas retas iguais | **Placas curvas sobrepostas**, que diminuem junto com o raio, com aro claro e **espinhos dorsais alternados** voltados para trás |
| Verde só em pontinhos | **Veia de energia na barriga** com pulsos que viajam até a ponta, e **energia vazando entre as placas** |
| Cinza solto, sem ligação com o boss | Carapaça **cinza-ardósia puxando para o verde do parasita** nas sombras, a mesma família do módulo. O tentáculo **nasce rachando o casco** (rachaduras verdes) e por isso lê como parte da corrupção do Gusini |
| Faixa de aviso com xadrez fraco | **Listras diagonais de perigo** que correm, bordas fortes e seta dupla, com **a mesma altura do tentáculo** |

## 1. Clips do boss
**`gusini_p2_tentacle_start`** (6 frames · 610 ms)
| F | ms | Ação | Evento |
|---|---|---|---|
| 0 | 80 | Pose base | `telegraph_start` |
| 1 | 110 | O módulo pulsa, a cabeça tranca e o casco racha | |
| 2 | 90 | Os tentáculos rompem o módulo | `tentacles_emerge` |
| 3 | 100 | Crescem | |
| 4 | 140 | **POSE DE AMEAÇA**: o ferrão aponta para o player | |
| 5 | 90 | Entra no wind-up | `start_end` |

**`gusini_p2_tentacle_lash`** (5 frames · repetir a cada golpe)
| F | ms | Ação | Evento |
|---|---|---|---|
| 0–1 | 150 + 150 | **Wind-up em loop** durante os 900 ms de aviso | `warn_loop_start` / `warn_loop_end` |
| 2 | 90 | **CHICOTADA**: os dois tentáculos mergulham para trás e saem pela direita, com rastro | `lash_hit` → soltar os tentáculos de faixa |
| 3 | 110 | Acompanhamento (os tentáculos estão lá fora, nas faixas) | |
| 4 | 120 | Voltam: segue para o próximo wind-up ou para a resistência | `lash_end` |

**`gusini_p2_tentacle_resist`** (8 frames · 810 ms)
| F | ms | Ação | Evento |
|---|---|---|---|
| 0 | 100 | Os tentáculos recolhem e o Gusini tranca | `resist_start` |
| 1 | 90 | **Resiste**: olhos normais (cabeça da F1) | |
| 2 | 70 | O parasita puxa de volta | |
| 3 | 110 | Resiste de novo e o módulo contrai | |
| 4–5 | 120 + 120 | **NÚCLEO EXPOSTO**, em loop por **2,0 s** | `weakpoint_open` |
| 6 | 90 | O parasita retoma | `weakpoint_close` |
| 7 | 110 | Pose base | `recover_end` |

**Ponto fraco:**
- Centro no canvas: **(161,102)**.
- Em relação ao pivô na Unity: **(+41, +16) px nativos**.
- Sugestão: dano ×2.

## 2. Tentáculo de faixa (pasta `lane_tentacle/`)
- **Sprites:** 8 sprites de **600×128**, já desenhados inteiros.
- **Pivô:** **(600,64)** = Unity (1.0, 0.5). Coloque o pivô **na borda direita da tela, no centro da faixa**. A base fica fora da tela e o tentáculo avança para a esquerda.
- **Escala:** na preview está em **2×** (ocupa ~168 px de altura numa tela de 720).

| # | ms | Fase |
|---|---|---|
| 0–2 | 50 / 50 / 60 | Entra (ondulando) |
| 3 | 80 | **GOLPE** (estica quase reto, ponta no extremo esquerdo). `lane_strike`: tocar `fx_tentacle_crack` na ponta |
| 4 | 80 | Segura |
| 5–7 | 70 × 3 | Recolhe |

- **Hitbox:** de y **30 a 96** (nativo), entre `tip_x_px` de cada frame (está no JSON) e a borda direita. Os espinhos ficam fora do hitbox (perdão visual).

## 3. Aviso de faixa (pasta `lane/`)
- **Preenchimento:** `lane_warning_fill_0-1` (32×32, Tiled): listras diagonais que correm, trocando a cada 120 ms.
- **Bordas:** `lane_warning_edge_0-1` (32×6, Tiled), no topo e na base da faixa.
- **Seta:** `lane_warning_arrow_0-1` (44×56), na borda direita.
- **Altura:** a faixa tem a altura do tentáculo (84 px nativos, 168 a 2×).
- **A faixa segura fica sem nada.**

## 4. Sequência sugerida (~9 s)
1. `start`.
2. 3 golpes, cada um com:
   - aviso de 900 ms;
   - `lash_hit` com o tentáculo de faixa (530 ms);
   - recolhe e respiro.
3. `resist`, com o núcleo exposto por 2,0 s.

Na preview, a faixa segura vai meio → baixo → topo.

## 5. Arquivos
```
frames/<clip>/  parts/ (sprite e fx separados por frame)  *_sheet.png (+_x2)  gusini_p2_tentacles.json
lane_tentacle/  lane_tentacle_sheet.png  lane/  fx/  tools/ (gerador refeito: tentacle_art.py)
previews/ clips_boss_x3.gif · tentaculos_v2_em_tela.gif · tentaculo_faixa_x2.gif
          quadros_chave_em_tela.png · comparacao_v1_v2.png · folha_aviso_fx_x4.png
```

## Revisão obrigatória
- **Ainda parece tosco?**
  - Não no boss: o arco tem volume, placas e ferrão legíveis a 1×.
  - No tentáculo de faixa, a primeira passada da v2 ainda estava **cinza demais** (lia como mangueira de metal) e os pulsos pareciam "luzinhas de LED". Refiz antes de entregar:
    - a carapaça foi para o tom do parasita;
    - os pulsos ficaram mais longos;
    - a energia passou a vazar entre as placas.
- **Tentáculo convincente?** Sim: afunila, ondula ao entrar, estica reto no golpe e tem um ferrão claro na ponta.
- **Boa silhueta?** Sim. Cortei de 3 para 2 tentáculos. O arco de escorpião é a forma mais forte do ataque e não cobre a cabeça.
- **Parece parte do Gusini?**
  - Nasce rachando o módulo, usa a mesma paleta de carapaça e verde, e recebe a mesma luz.
  - Na resistência, a cabeça da F1 mostra que o ganso continua lá dentro: corrompido, não grotesco.
- **Corrupção interessante?** Sim: veia pulsante, energia vazando entre as placas, olho no ferrão e rachaduras no casco.
- **Leitura de gameplay:**
  - O aviso agora é uma faixa listrada clara da altura exata do tentáculo.
  - A faixa segura fica limpa.
  - Os espinhos ficam fora do hitbox.
- **Técnico:** alpha só 0/255, sem anti-aliasing, canvas e pivô fixos.
