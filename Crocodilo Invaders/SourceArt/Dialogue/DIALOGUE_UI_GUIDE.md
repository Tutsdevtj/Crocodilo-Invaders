# Crocodilo Invaders — Interface de diálogo "Rádio de Piloto"

## Conceito
A caixa de diálogo é uma pequena central de rádio de cockpit, com o **mesmo material do HUD do player**:

- **Painel:** tela escura de ardósia.
- **Moldura:** chapa oliva com bisel e um **trilho interno de bronze**.
- **Cantos:** chanfrados, com **cantoneiras de bronze rebitadas** que saem 1 px para fora da silhueta.
- **Detalhes de instrumento:** pequenas marcas no topo da moldura.

Do painel saem duas "abas" na borda de cima:

- **à esquerda ou à direita:** a **placa de nome**, com tarja colorida por falante;
- **do lado oposto:** o **módulo de rádio** (LED verde, 3 barras de sinal âmbar, grelha e antena), que acende quando alguém fala.

Nenhum PNG traz texto, portrait ou posição de personagem.

## 1. Assets (`sprites/`, arte nativa 1x)
| Arquivo | Tamanho | Tipo | 9-slice (L, R, T, B) | Pivô |
|---|---|---|---|---|
| `dialogue_panel_backplate.png` | 64×44 | Sliced | 14, 14, 12, 12 | (0.5, 0) |
| `dialogue_panel_frame.png` | 64×44 | Sliced, centro transparente | 14, 14, 12, 12 | (0.5, 0) |
| `dialogue_highlight.png` | 64×44 | Sliced (realce de troca de falante) | 14, 14, 12, 12 | (0.5, 0) |
| `dialogue_nameplate.png` | 40×15 | Sliced só na horizontal | 10, 7, 0, 0 | (0, 0.5) |
| `dialogue_nameplate_accent.png` | 40×15 | Sliced, **tingir por falante** (Image.color) | 10, 7, 0, 0 | (0, 0.5) |
| `dialogue_radio_idle.png` | 34×17 | Simples | — | (1, 0) |
| `dialogue_radio_active_0..2.png` | 34×17 | Simples, animação | — | (1, 0) |
| `dialogue_continue_0..2.png` | 11×10 | Simples, animação | — | (0.5, 0.5) |

**Detalhes técnicos:**
- **Alpha:** só 0 ou 255.
- **Paleta:** a mesma do HUD (ver `dialogue_ui.json`).
- **Espessura da moldura:** 7 px nativos.
- **Bordas de 9-slice:** as cantoneiras, os chanfros, as marcas de instrumento e a faixa de cima do fundo ficam dentro delas. A parte que estica é **uniforme**, então não deforma em nenhum tamanho. Testado em 360×70, 200×52 e 140×40 (`previews/01_*`).

## 2. Escala e importação
- **Referência:** tela de 1200×720. Toda a UI é exibida em **2×**.
- **Import:**
  - Texture Type Sprite (2D and UI);
  - Filter **Point**;
  - Compression **None**;
  - sem mipmaps;
  - Mesh Type **Full Rect** (obrigatório para Sliced).
- **Sprite Editor:** defina as bordas de 9-slice da tabela acima, em pixels nativos.
- **PPU:** sprite com **Pixels Per Unit = 50** e Canvas com Reference Pixels Per Unit 100. Assim, 1 pixel da arte vira 2 pixels de UI.
  - Alternativa: manter PPU 100 e usar **Image › Pixels Per Unit Multiplier = 0.5**.
- **Tamanhos:** use sempre múltiplos de 2 px de UI, para não quebrar o grid.

## 3. Tamanhos sugeridos (pixels de UI em 1200×720 = 2× nativo)
| Elemento | Nativo | Em tela |
|---|---|---|
| Painel padrão | 360×70 | **720×140** |
| Painel mínimo recomendado | 140×40 | 280×80 |
| Placa de nome | altura 15; largura = 10 + largura do nome + 11 | altura 30 |
| Rádio | 34×17 | 68×34 |
| Continuar | 11×10 | 22×20 |

**Sugestão de montagem do painel (o Codex decide a posição final):** painel centralizado embaixo, com 32 px de margem da base da tela. Os portraits ficam por conta do Codex, ao lado ou atrás; o painel funciona sem portrait (`previews/03b_sem_portrait.png`).

## 4. Área de texto (dentro do painel)
- **Margens internas** (UI 2×): esquerda **30**, direita **30**, topo **26**, base **24**. Em nativo: 15, 15, 13, 12.
- **Painel padrão (720×140):** a área de texto é **660×90**, ou seja, **3 linhas de 30 px**.
- **Ícone de continuar:** fica no canto inferior direito, dentro da margem. Reserve 22 px à direita na última linha, ou deixe o texto passar por baixo, já que o ícone só aparece no fim da fala.

## 5. Fontes
### Texto da fala: **Pixelify Sans** (como você sugeriu)
- **Origem:** Google Fonts, https://fonts.google.com/specimen/Pixelify+Sans
- **Arquivo incluído:** `fonts/PixelifySans-VariableFont_wght.ttf`, baixado do repositório oficial `github.com/google/fonts` (ofl/pixelifysans).
- **Licença:** **SIL Open Font License 1.1** (`fonts/PixelifySans_OFL.txt`). Pode ser usada e embutida no jogo; não pode ser vendida sozinha.
- **Português:** conferi todos os caracteres: á à â ã é ê í ó ô õ ú ü ç (maiúsculas e minúsculas), … — “ ” ‘ ’.
- **Peso:** **Medium**.
  - Na fonte variável, use o eixo `wght = 500`, ou gere a instância Medium no Font Asset Creator.
  - O Regular fica fino demais sobre o fundo escuro (`previews/07_peso_da_fonte_x3.png`).
- **Tamanho:** **22 px** em 1200×720.
  - A fonte é desenhada num grid de 1/11 do em, então fica nítida em 11, 22 e 33 px. Em 22 px, cada pixel da fonte vira 2 pixels de tela, igual ao resto da UI.
  - Para modo acessível ou letra maior, use **33 px** com o painel em 3 linhas mais altas.
- **Entrelinha:** **30 px** por linha. No TMP, o padrão em 22 px dá cerca de 26 px, então use **Line Spacing ≈ +16**.
- **Espaçamento entre letras:** 0.
- **Cor:** `#F6ECCC` (creme do HUD).
- **Capacidade:** cerca de 55 caracteres por linha no painel padrão.
- **TMP:**
  - Font Asset em Bitmap / Raster, **Atlas Render Mode = RASTER (hinted)**, Sampling Point Size 22, Padding 2;
  - material sem outline e sem sombra;
  - Extra Settings: desligar Kerning se aparecer "meio pixel".
- **Por que não uso a Pixelify no nome:** no tamanho base, o "B" maiúsculo dela tem o canto cortado e lê como "G" ("G. Crocodilo").

### Nome do falante: **Press Start 2P** (a mesma do HUD, já está no projeto em `Assets/Fonts/PressStart2P`, OFL)
- **Tamanho:** **16 px** em 1200×720 (8 px nativos, grid de 8).
- **Cor:** `#F6ECCC`.
- **Posição do texto dentro da placa:** x = 22, y = 8 (UI 2×, a partir do canto superior esquerdo da placa).
- Assim a placa do diálogo fala a mesma língua visual da placa "B. Crocodilo" do HUD.

## 6. Placa de nome
- **Posição:** ancorada na **borda de cima do painel**, sobrepondo a moldura em 14 px de UI (7 nativos) e recuada 24 px da lateral.
  - Pode ficar à esquerda ou à direita. Sugestão: do lado do falante.
- **Largura:** dinâmica = `20 + larguraDoTexto + 22` (UI 2×). O 9-slice horizontal preserva a tampa da esquerda (com a tarja) e o rebite da direita.
- **Tarja colorida:** `dialogue_nameplate_accent` fica por cima, com o **mesmo tamanho e RectTransform** da placa. Use **Image.color** para tingir (a arte é branca e cinza, então o tingimento preserva o sombreamento).

| Falante | Cor da tarja |
|---|---|
| B. Crocodilo | `#96BE5A` (oliva claro) |
| Gusini | `#E2463C` (vermelho do cachecol) |
| Gusini Corrompido | `#7CEC48` (verde alienígena) |
| Sistema / outros | `#C8C8C8` |

  A interface em si continua neutra (oliva, bronze e ardósia): **só a tarja muda de cor**.

## 7. Indicador de rádio
- **Posição:** fica na borda de cima, **do lado oposto à placa**, com a base dele 16 px de UI abaixo do topo do painel (por cima da moldura) e recuado 24 px da lateral.

| Estado | Sprites | Quando |
|---|---|---|
| **Desligado** | `dialogue_radio_idle` | Painel aberto, mas ninguém falando (pausas, escolhas, entre falas) |
| **Transmitindo** | loop `active_0 → 1 → 2 → 1`, **120 ms por frame** | Enquanto o texto está sendo digitado |
| **Fim da transmissão** | `active_0` parado | Texto completo, esperando o jogador |

- **Intensidade:** na troca de falante, toque `active_2` por 80 ms antes de entrar no loop (um "clique" de rádio).
- **Opcional:** Image.color com alpha 0.6 no idle, para apagar ainda mais.

## 8. Indicador de continuar
- **Frames:** `dialogue_continue_0 → 1 → 2 → 1`, **140 ms por frame**. A seta desce 2 px e acende.
- **Quando aparece:** só quando o texto termina de ser digitado. Some ao avançar.
- **Posição:** canto inferior direito do painel, a 22 px da direita e 18 px da base (UI 2×), medidos até a borda do sprite.

## 9. Realce de troca de falante
- **Como usar:** `dialogue_highlight` fica por cima da moldura, com o mesmo tamanho do painel.
- **Animação:** na troca de falante, alpha **1 → 0 em 250 ms** (ease-out). É uma linha âmbar no topo interno.
- **Opcional:** no mesmo instante, um "pop" da placa de nome (escala 1.0 → 1.06 → 1.0 em 120 ms).

## 10. Entrada e saída (feitas pela Unity, sem frames extras)
- **Entrada (240 ms):**
  - o painel sobe 24 px e o alpha vai de 0 a 1, com ease-out;
  - a placa e o rádio entram **60 ms depois**, descendo 6 px até o lugar;
  - o rádio liga com `active_2` e depois entra no loop.
- **Texto:** começa 80 ms após o fim da entrada. Typewriter sugerido: **40 caracteres/s**, com pausa de 120 ms em . ! ? e de 60 ms em , …
- **Saída (140 ms):** o rádio vai para idle, o texto some na hora e o painel desce 12 px com alpha 1 → 0 (ease-in).
- **Grid:** arredonde as posições animadas para inteiros pares de UI, para não borrar o pixel art.

## 11. Ordem das camadas
```
DialogueUI (RectTransform, ancorado no centro-inferior)
├── Panel (RectTransform 720×140; pivot 0.5,0)
│   ├── Backplate        Image Sliced  dialogue_panel_backplate
│   ├── Frame            Image Sliced  dialogue_panel_frame
│   ├── Highlight        Image Sliced  dialogue_highlight (alpha 0 por padrão)
│   ├── Body             TextMeshProUGUI (margens 30/30/26/24)
│   └── ContinueIcon     Image (animado)
├── Nameplate (RectTransform; ancorado no topo do Panel)
│   ├── Plate            Image Sliced  dialogue_nameplate
│   ├── Accent           Image Sliced  dialogue_nameplate_accent (color = cor do falante)
│   └── NameText         TextMeshProUGUI (Press Start 2P 16)
└── Radio                Image (idle / active_0..2), ancorado no topo do Panel, lado oposto à placa
```
Os portraits são irmãos do DialogueUI, posicionados pelo Codex. Nada no painel depende do lado em que estão.

## 12. Previews (`previews/`)
| Arquivo | O que mostra |
|---|---|
| `01_painel_sozinho_9slice_x4.png` | Painel em 3 tamanhos (teste de 9-slice) |
| `02_com_nome_e_fala_tamanho_real.png` | Painel com nome e fala, em tamanho real |
| `04_ampliado_x4.png` | O mesmo, ampliado com nearest-neighbor |
| `03_mockup_jogo_1..4.png` | Sobre a tela do jogo, 1200×720 (portraits só para contexto) |
| `03b_sem_portrait.png` | Painel funcionando sem portrait |
| `05_estados_x4.png` e `05_estados_animados_x4.gif` | Rádio e indicador de continuar |
| `06_regiao_clara_vs_escura.png` | Painel sobre céu claro e sobre a nave-mãe escura |
| `07_peso_da_fonte_x3.png` | Regular × Medium × SemiBold |

Nas previews, a fonte foi rasterizada no grid exato dela. No jogo, quem desenha o texto é o TextMeshPro.
