# Crocodilo Invaders — VFX do Dash: "Arranque Evasivo"

Um motor auxiliar dá um tranco, a nave dispara e deixa para trás **silhuetas ciano** dela mesma e **linhas de energia**. No fim, o impulso solta um sopro de pressão.

- **Paleta:** o azul da **barra de energia do HUD** (o dash "é" do player), com faíscas **âmbar do motor** e fumaça clara.
- **Densidade:** toda a arte está **na densidade da sprite do player** (1 px de efeito = 1 px da nave), com alpha só 0 ou 255.
- **Mecânica:** nenhum script, prefab ou valor de gameplay foi alterado.

## 1. Assets
| Efeito | Arquivos | Tamanho | Pivô (px / Unity) | Frames | Ritmo | Duração |
|---|---|---|---|---|---|---|
| **Ignição** | `dash_ignition/dash_ignition_{h,d}_00..03.png` | 72×72 | (36,36) / (0.5, 0.5) | 4 | 20 ms (50 fps) | 80 ms, sem loop |
| **Rastro** | `dash_speed_streak/dash_speed_streak_h_00..02.png` | 176×112 | (128,56) / (0.727, 0.5) | 3 | 30 ms | loop |
|  | `dash_speed_streak/dash_speed_streak_d_00..02.png` | 168×168 | (118,50) / (0.702, 0.702) | 3 | 30 ms | loop |
| **Fim** | `dash_end/dash_end_{h,d}_00..03.png` | 64×64 | (32,32) / (0.5, 0.5) | 4 | 25 ms (40 fps) | 100 ms, sem loop |

- **Folhas:** em `sheets/`, uma por efeito e orientação, com os frames lado a lado na mesma ordem.
- **Import:** Point, sem compressão, sem mipmaps, **o mesmo PPU da sprite da nave**.

### O que cada frame faz
- **Ignição:**
  - 00: clarão forte, com o cone começando;
  - 01: chama direcional longa, núcleo branco e 3 faíscas âmbar;
  - 02: a chama se parte em 3 jatos e uma onda de choque pontilhada abre;
  - 03: fiapos e faíscas finais.
  - Parece um motor, **não dano**: é direcional, ciano, sem vermelho e sem destroços.
- **Rastro:** 7 linhas de energia com cabeça clara perto da nave e cauda que afina e quebra em pontilhado. Tem contorno azul-escuro só acima e abaixo das linhas (leitura no céu claro). Os 3 frames variam o comprimento, o que dá o tremor de velocidade.
- **Fim:**
  - 00: puff de pressão, anel pontilhado e 2 faíscas;
  - 01: o puff cresce;
  - 02: o puff dissolve em pontilhado;
  - 03: some.
  - É curto e claro, não parece impacto.

## 2. Orientação (8 direções sem perder pixel)
Os efeitos foram **desenhados** viajando para a direita (`h`) e para cima-direita a 45° (`d`), nunca girados como bitmap. As outras direções saem só com flips e rotações de 90°:

| Direção do dash | Variante | Transformação |
|---|---|---|
| → | h | nenhuma |
| ← | h | flipX |
| ↑ | h | rotZ +90 |
| ↓ | h | rotZ −90 |
| ↗ | d | nenhuma |
| ↖ | d | flipX |
| ↘ | d | flipY |
| ↙ | d | flipX + flipY |

- **Nunca** gire em ângulo livre: a pixel art serrilha.
- **Entrada analógica:** arredonde para a direção de 45° mais próxima.
- **Pivô nas rotações de 90°:** o pivô em (0.5, 0.5) da ignição e do fim não muda. O do rastro está no centro da nave, então gire em torno do próprio pivô.

## 3. Onde prender cada efeito
- **Centro visual da nave:** (72,59) no canvas 160×120 da sprite, ou seja, **(−8, +1) px** do centro do canvas (Unity, y para cima).
- **Traseira:** `centroNave − direçãoDash × R`, com **R = 56 px** na horizontal, **47** na vertical e **52** na diagonal.

| Efeito | Espaço | Ponto |
|---|---|---|
| Ignição | **Mundo** (fica para trás quando a nave sai) | Traseira, no instante do início do dash |
| Rastro | **Filho da nave** | Centro da nave (o próprio pivô) |
| Fim | **Mundo** | Traseira, na posição final |

## 4. Coreografia (dash de ~150 ms; tudo escala com a duração real do dash e com os upgrades)
| Tempo | Evento |
|---|---|
| **0 ms** (início) | Ignição (80 ms), contorno de invulnerabilidade **liga**, 1º afterimage |
| **+20 ms** | Rastro aparece (loop de 30 ms) |
| **+30 / 60 / 90 / 120 ms** | Novos afterimages, um a cada `duraçãoDash / 5` |
| **fim dos i-frames** (pelo código) | Contorno **desliga** (independente do resto) |
| **fim do dash** | `dash_end` (100 ms) na traseira. Rastro sai em 40 ms (alpha 1 → 0, ou troca para pontilhado) |
| **até +280 ms** | Últimos afterimages e o puff terminam. Nada mais na tela |

Sorting, de trás para frente:
1. ignição;
2. rastro;
3. afterimages;
4. **nave**;
5. contorno de invulnerabilidade (no material da nave);
6. dash_end.

**Os projéteis inimigos devem ficar acima de todos os efeitos do player**, para nunca serem escondidos.

**Blend:** Sprites-Default (alpha normal). Os sprites já são 0 ou 255; a transparência dos afterimages e o fade do rastro vêm do alpha do SpriteRenderer.

## 5. Afterimages (usar a sprite REAL, não arte nova)
- **Spawn:** a cada **30 ms** durante o dash (cerca de 5 por dash). Cada um copia o **sprite atual**, o flipX e a posição da nave naquele instante.
- **Vida:** **130 ms**, com alpha de **0,70 → 0** linear.
- **Material de cor sólida** (a silhueta não usa as cores da nave):

| Idade | Cor | Borda de cima |
|---|---|---|
| 0–45 ms | `#AAEAFF` | `#F0FCFF` |
| 45–90 ms | `#46C4FA` | `#AAEAFF` |
| 90–130 ms | `#247CDC` | — |

  - os **38% de baixo** da silhueta ficam um tom abaixo (volume);
  - contorno de **1 px `#1848A0`** (é o que faz ler no céu azul).
- **Ordem:** atrás da nave. Use pool de 6 objetos, sem instanciar a cada dash.
- **Preview:** `previews/dash_pico_x4_inspecao.png` mostra o resultado esperado.

## 6. Invulnerabilidade (shader/material, não sprites)
- **Contorno de 1 px** em volta da sprite da nave (outline por amostragem dos 4 vizinhos do alpha).
- **Cor:** alterna **`#AAEAFF` ↔ `#F0FCFF` a cada 40 ms** (um pulso discreto).
- **Duração:** liga no início dos i-frames e desliga no fim deles, **pelo código existente**. É independente do rastro e dos afterimages.
- **Sem piscar a nave** e sem escudo: a nave fica 100% visível o tempo todo.

## 7. Cancelamento, morte e pausa
- **Dash cancelado ou interrompido:** toca `dash_end` na posição atual, o rastro sai em 40 ms e os afterimages vivos terminam a vida normalmente.
- **Morte durante o dash:** desliga **tudo** na hora (rastro, afterimages, contorno), para que a explosão do player seja lida sozinha. Ignição e fim que já estão no mundo podem terminar.
- **Pausa:** tudo usa **tempo escalado**, então congela junto com o jogo.
- **Dash em cooldown:** nenhum efeito.

## 8. Previews (`previews/`)
| Arquivo | O que mostra |
|---|---|
| `dash_{horizontal,vertical,diagonal}_tamanho_real.gif` | Gameplay real (céu, nuvens, tiros inimigos), 1:1 |
| `dash_*_x2.gif` | O mesmo, ampliado com nearest-neighbor |
| `dash_*_quadros_chave.png` | −40, 0, +20, +60, +100, +140, +180 ms |
| `dash_pico_x4_inspecao.png` | Pico do dash em 4×, para conferir os pixels |

Os tiros laranjas do Gusini continuam visíveis durante todo o dash.
