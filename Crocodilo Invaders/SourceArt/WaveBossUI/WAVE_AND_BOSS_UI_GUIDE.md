# WAVE & BOSS UI — Guia técnico para integração (Crocodilo Invaders)

Este pacote contém só arte, animação de referência e dados. **Não há código Unity.** Não alterar waves, combate nem o diálogo existente.

O pacote cobre duas peças:

- **Wave UI definitiva:** a direção "Arcade Swoosh" aprovada. O 7 é idêntico ao do conceito; os outros algarismos (0–9) foram construídos a partir dele.
- **Apresentação PERIGO! GOOSINI:** composta por:
  - uma estrutura genérica de boss, reaproveitável;
  - peças exclusivas do Goosini;
  - partículas de pena.

Os dados numéricos completos (posições, kerning, exemplos, timelines) estão em `wave_boss_ui.json`. Este guia explica as regras.

---

## 1. Importação

| Configuração | Valor |
|---|---|
| Resolução de referência | 1200×720 |
| Escala dos pixels | Sprites nativos (1x), exibidos **2x** na tela |
| Pixels Per Unit | 50, com Canvas Reference PPU = 100 (ou Set Native Size e escala 2) |
| Filter Mode | Point (no filter) |
| Compression | None |
| Mip Maps | Desligado |
| Alpha | Somente 0/255. Sem blur, sem anti-aliasing, sem fundo embutido |
| Pivô de todos os sprites | **Top-left** (Unity pivot `(0, 1)`) |
| Coordenadas deste guia e do JSON | Px nativos, origem no canto superior esquerdo do banner, y para baixo. Na tela, multiplicar por 2 |
| Posição na tela | Arredondar sempre para px de tela **par**, para não quebrar o pixel nativo |

---

## 2. Estrutura de pastas

```
Wave_UI/sprites/        faixa (head / mid / end), ONDA, burst, speedlines
Wave_UI/digits/         algarismos 0–9 + variantes pop1 / pop2 / flash + atlas (png + json)
Boss_UI_Common/         genérico de boss: faixa (head / mid / end), PERIGO! (3 estados), impact burst, speedlines, paleta
Boss_Goosini/           exclusivo: título GOOSINI (+ flash), pena longa, cachecol (4 quadros), ponta-bico
Particles_Feathers/     4 penas pequenas (giro) + 2 etapas de dissolução cada
previews/               PNGs e GIFs de avaliação (não importar)
tools/                  scripts Python que geram tudo (referência / regeneração)
wave_boss_ui.json       manifesto
```

---

## 3. Wave UI

### 3.1 Peças

| Sprite | Tamanho | Posição no banner | Função |
|---|---|---|---|
| `wave_bar_head` | 138×42 | (14, 24) | Frente: corte inclinado + cunha em degraus |
| `wave_bar_mid` | 1×42 | de x = 14+138 até 14+L−12, y = 24 | Repetir ou esticar no X (largura inteira) |
| `wave_bar_end` | 76×42 | (14+L−12, 24) | Fim irregular, cerdas e blocos que se desprendem |
| `wave_word_onda` | — | (40, 32) | "ONDA" |
| `wave_digit_N` | ver JSON | ver regras | Algarismos; topo y = 14 |
| `wave_burst` | — | ver regras | Estalo no canto do último algarismo |
| `wave_speedlines_{s,m,l}_{0,1}` | — | (14+L+50, 0) | Linhas de velocidade, só durante o movimento |

A faixa montada a partir das peças reproduz pixel a pixel a faixa desenhada inteira. Isso foi verificado.

### 3.2 Montagem do número (1 ou 2+ dígitos)

```
x0          = digits[d0].first_x                    # 1º dígito: espaço ótico calibrado com o 7 aprovado
x(i+1)      = x(i) + kerning_advance["d(i)d(i+1)"]  # tabela de pares (100 entradas) no JSON
L           = max(150, x_last + digits[d_last].bar_end_offset − 14)
burst       = (x_last + digits[d_last].burst_offset[0],  14 + burst_offset[1])   # y = 8
speedlines  = (14 + L + 50, 0)
```

Regras gerais:

- **A faixa cresce, os algarismos nunca são comprimidos.** Com dois dígitos, só a `wave_bar_mid` estica.
- O número fica **sobre** a faixa. O fim irregular dela fica escondido atrás do último algarismo, como no conceito aprovado.
- **Centralização:** o centro do conjunto é `(bbox_x0 + bbox_x1) / 2`. Os exemplos prontos estão em `wave.examples` no JSON. Na tela: `X_banner = 600 − center_x·2 + deslocamento_da_animação`.
- **Vertical:** `Y_banner = 236 − 76 = 160` px de tela (ajustável).

**Exemplos de validação (JSON):**

| Onda | L | Dígitos x | bbox |
|---|---|---|---|
| 1 | 150 | [124] | 14–228 |
| 7 | 150 | [120] | idêntico ao conceito aprovado |
| 10 | 185 | [124, 151] | 14–263 |
| 99 | 210 | [132, 176] | 14–288 |

**Ordem de render (de trás para a frente):**

1. speedlines
2. `wave_bar` (head, mid, end)
3. `wave_word_onda`
4. algarismos, da esquerda para a direita (o seguinte cobre a extrusão do anterior)
5. `wave_burst`

### 3.3 Ênfase do número (assentamento)

No assentamento, cada algarismo troca de sprite, sem escalar bitmap: as variantes foram redesenhadas maiores.

| Tempo | Sprite |
|---|---|
| 180–213 ms | `wave_digit_N_flash` (branco, +6 px) |
| 213–246 ms | `wave_digit_N_pop1` (+3 px) |
| a partir de 246 ms | `wave_digit_N` (base) |

- Somar `digits[N].variant_offset[variante]` ao x/y do dígito. Isso mantém a base alinhada e o algarismo centralizado.
- O `wave_burst` aparece em 180 ms e fica estático até a saída.

### 3.4 Animação (aprovada — não alterar)

`x` em px de tela, relativo ao centro. Total: **1050 ms**.

| t (ms) | De → para (x) | Easing | Momento |
|---|---|---|---|
| 0–180 | +760 → +24 | out_cubic | ENTRADA (speedlines ligadas) |
| 180–230 | +24 → −8 | out_cubic | Assentamento / overshoot (pop + burst) |
| 230–280 | −8 → 0 | out_cubic | Assentamento |
| 280–800 | 0 → −16 | linear | LEITURA (deriva lenta, nada mais anima) |
| 800–840 | −16 → −8 | out_cubic | Micro-antecipação |
| 840–1050 | −8 → −1000 | in_cubic | SAÍDA (speedlines ligadas) |

- **Easings:** `out_cubic = 1−(1−u)³`, `in_cubic = u³`.
- **Speedlines:** comprimento = `min(130, |velocidade px/ms| × 10)`.
  - Escolha do sprite: `s` se < 60, `m` se < 105, senão `l`.
  - Esconder se < 6.
  - Alternar `_0` / `_1` a cada 33 ms.

---

## 4. Apresentação de boss — PERIGO! GOOSINI

### 4.1 O que é genérico e o que é do Goosini

**Genérico** (`Boss_UI_Common`):

- `boss_band_head` / `mid` / `end`: faixa angular com ponta afiada, fim rasgado e 3 dentes.
- `boss_alert_perigo_{red, amber, flash}`.
- `boss_impact_burst`.
- `boss_speedlines_*`.
- `boss_band_palette_goosini.png`.

**Exclusivo** (`Boss_Goosini`):

- `goosini_title`: os **dois O são os óculos azuis** do Goosini, ligados por uma alça.
- `goosini_title_flash`.
- `goosini_feather_long`: o penacho.
- `goosini_scarf_0..3`: o cachecol vermelho tremulando.
- `goosini_beak_tip`: a ponta da faixa vira o bico laranja.

**Partículas** (`Particles_Feathers`): `feather_particle_0..3`, com `_dissolve_1` e `_dissolve_2`.

### 4.2 Posições

Canvas do banner: 340×120 nativo, ou seja, 680×240 na tela.

| Peça | x, y |
|---|---|
| Faixa (comprimento 236) | (30, 40) |
| Ponta-bico | (30, 40), mesma origem da faixa |
| Pena longa | (150, 0) |
| Título | (66, 34) |
| Cachecol | (268, 52) = (faixa.x + comprimento + 2, faixa.y + 12) |
| PERIGO! | (56, 14) |
| Impact burst | (40, 0) |
| Speedlines | (300, 20) |

**Faixa por partes:**

- head = 61 px (colunas 0–60);
- mid = 1 coluna, esticada;
- end = a partir da coluna 126.

O comprimento mínimo é 170. Para outros bosses, o comprimento da faixa deve ser a largura do título + 26 (o GOOSINI tem 210, então 236).

**Ordem de render (de trás para a frente):**

1. speedlines
2. cachecol
3. faixa
4. ponta-bico
5. pena longa
6. título
7. PERIGO!
8. impact burst

As partículas de pena ficam numa camada própria, em espaço de tela/mundo, **não** presas ao banner.

**Posição na tela:**

- `X_banner = 600 − 340 + x_animação` (o canvas tem 680 px de tela de largura);
- `Y_banner = 228 − 120 = 108` px de tela.

O banner não cobre o jogador (x ~180–310, y ~340–440) nem a HUD.

### 4.3 Animação do boss

Total: **1300 ms**.

| t (ms) | x (px de tela) | Easing | Momento |
|---|---|---|---|
| 0–200 | +820 → +28 | out_cubic | ENTRADA: penas ficam para trás |
| 200–260 | +28 → −10 | out_cubic | IMPACTO (overshoot) |
| 260–320 | −10 → 0 | out_cubic | Assenta |
| 320–1050 | 0 → −20 | linear | LEITURA |
| 1050–1090 | −20 → −12 | out_cubic | Antecipação |
| 1090–1300 | −12 → −1100 | in_cubic | SAÍDA: penas se soltam e se dissolvem |

**No impacto (200 ms):**

- `goosini_title_flash` de 200 a 233 ms;
- `boss_impact_burst` de 200 a 300 ms.

**Linha do tempo do PERIGO!:**

| t (ms) | Estado |
|---|---|
| 0–200 | oculto |
| 200–233 | flash |
| 233–266 | amber |
| 266–420 | red |
| 420–480 | amber |
| 480–540 | red |
| 540–600 | amber |
| 600 em diante | red |

São duas piscadas, e depois o aviso fica fixo. Nada de piscar contínuo.

**Cachecol:** `goosini_scarf_0..3` em loop, 90 ms por quadro, do começo ao fim. É o único detalhe que se mexe durante a leitura, e é discreto.

**Penas (partículas):**

- **Vida e quadros:**
  - vida de 260 ms;
  - troca de quadro de giro a cada 66 ms;
  - `_dissolve_1` a partir de 62% da vida e `_dissolve_2` a partir de 82%. A dissolução é por pixel, sem alpha parcial.
- **Movimento:** ficam paradas no **mundo** onde nasceram e derivam +0.18 px/ms em x e +0.06 px/ms em y. Como o banner dispara para a esquerda, elas "ficam para trás".
- **Nascimento na entrada:** uma a cada 33 ms, entre 0 e 200 ms, no lado direito do banner.
- **Nascimento na saída:**
  - 4 de uma vez em 1090 ms;
  - depois uma a cada 30 ms até 1300 ms, espalhadas ao longo do banner.

---

## 5. Sequência ONDA 10 → GOOSINI

| t (ms) | Evento |
|---|---|
| 0 | Wave UI "ONDA 10", animação normal (1050 ms) |
| 1050 | **Transição (150 ms):** 4 `feather_particle` atravessam a tela da direita para a esquerda (in_cubic, ~140 ms cada, defasagens 0/30/55/75 ms, y de tela 150/262/150/310). É o "vento" do Goosini anunciando a chegada |
| 1200 | Banner PERIGO! GOOSINI (1300 ms) |
| 2500 | Liberar a cena de entrada e o diálogo do boss **que já existem** (não alterar) |

- Os dois anúncios nunca aparecem juntos.
- **Não spawnar inimigos nem projéteis durante os 2500 ms.**
- Previews: `previews/sequencia_onda10_goosini.gif` e `sequencia_onda10_goosini_quadros.png`.

---

## 6. Pular / cancelar

- **Pausa:** congela tudo, incluindo as partículas.
- **Pular a onda:** se t < 800, saltar para t = 800, para que a saída sempre toque. Nunca remover a UI sem a saída.
- **Nova onda antes do fim:** comprimir o tempo restante da saída para 120 ms.
- **Pular o boss:** se t < 1050, saltar para t = 1050. A saída de 250 ms sempre toca.
- **Pular a sequência inteira:** pular a onda e iniciar o boss já em t = 1050 (só a saída).
- Ao pular, as partículas já nascidas terminam a vida normalmente. Não criar novas.

---

## 7. Reutilizar para um novo boss

1. **Nome:** desenhar o título do novo boss com o mesmo sistema de letras: grade de 28 px renderizada a 32 px de altura, inclinação 0.30, contorno navy de 2 px e extrusão de 4 px. Ver `tools/boss_ui.py` (`BIG` e `title()`).
2. **Faixa:** reaproveitar `boss_band_head` / `mid` / `end`. Esticar o mid até `largura_do_título + 26`.
3. **Cor:** trocar os 6 tons da faixa (`boss_band_palette_goosini.png`, do mais escuro ao mais claro) por palette swap. Dá para fazer com shader de paleta ou regerando com `tools/boss_ui.py` (`band(length, pal=[...])`). O contorno navy `(16, 20, 48)` é fixo para todos os bosses, e é o que mantém a família visual.
4. **PERIGO!, impacto, speedlines e timings** são os mesmos para todos.
5. **Identidade própria:** substituir apenas as peças exclusivas. No Goosini são pena longa, cachecol, ponta-bico e partículas de pena. Outro boss usa as suas (por exemplo, parafusos para um robô), nas mesmas âncoras:
   - anexo da frente da faixa;
   - elemento acima e à direita;
   - elemento que tremula no fim da faixa;
   - partícula própria.
6. Os timings do JSON (`boss.timing_ms`) valem para qualquer boss.

---

## 8. Paleta

| Uso | Cores (RGB) |
|---|---|
| Contorno (família) | navy `(16, 20, 48)` |
| Wave — faixa | ciano `CY0 (190, 242, 255)`, `CY1 (90, 210, 255)`, `CY2 (40, 150, 235)`, `CY3 (28, 92, 190)` |
| Wave — número | amarelo/laranja `Y0 (255, 236, 120)`, `Y1 (255, 208, 50)`, `Y2 (255, 150, 30)`, `Y3 (226, 92, 24)`, extrusão `(30, 46, 120)` |
| Goosini — roxo | `(37, 24, 61)`, `(60, 36, 92)`, `(88, 54, 124)`, `(118, 72, 152)`, `(150, 96, 174)`, `(186, 140, 214)` |
| Goosini — vermelho | `(84, 10, 24)` … `(240, 90, 56)` |
| Goosini — pena | `(255, 255, 255)` … `(145, 134, 163)` |
| Goosini — bico | `(204, 98, 30)` … `(250, 212, 108)` |
| Goosini — lente | `(30, 106, 190)`, `(66, 196, 242)`, `(196, 240, 255)` |

As cores do Goosini foram amostradas do retrato aprovado da Fase 1. Sem tentáculos, sem verde alienígena e sem nada da Fase 2.
