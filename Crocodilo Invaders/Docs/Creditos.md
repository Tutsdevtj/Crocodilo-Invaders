# Registro de arte para a futura tela de créditos

## Incorporado nesta etapa

- **Warped Shooting FX — ansimuz**
  - Fonte: https://ansimuz.itch.io/warped-shooting-fx
  - Licença publicada na página: Creative Commons Zero v1.0 Universal (CC0).
  - Download: versão gratuita, `Warped shooting fx files.zip`.
  - Arquivos usados: seis frames `charged1.png` até `charged6.png`, copiados de
    `Assets/Pixelart/charged/sprites` para `Assets/Art/Boss/Charge`.
  - Uso: animação de preparação dos ataques do Executor V-9.
  - Crédito sugerido: “Efeito de carga: ansimuz — Warped Shooting FX”.

## Música do Gusini, som do diálogo e fonte da caixa de diálogo (18/10/2026)

- **Phantasy Boss Battle Encounter — Ansimuz (Luis Zuno)**, música da Fase 1.
  - Fonte: https://opengameart.org/content/phantasy-boss-battle-encounter — CC0.
  - Arquivo: `Assets/Audio/Music/MUS_Gusini_Phase1_Loop.wav` (original `BossBattle.wav`, bytes preservados; SHA-256 conferido com o pacote).
  - Crédito voluntário: “Music: Phantasy Boss Battle Encounter by Ansimuz (CC0)”.
- **Boss Fight 2 — Ansimuz (Luis Zuno)**, música da Fase 2.
  - Fonte: https://opengameart.org/content/boss-fight-2 — CC0, com permissão do autor incluída.
  - Arquivo: `Assets/Audio/Music/MUS_Gusini_Phase2_Loop.wav` (original `Boss_Fight_2.wav`, bytes preservados; SHA-256 conferido).
- **5 Blip UI sound effects! — FunnyDude**, som das letras do diálogo.
  - Fonte: https://opengameart.org/content/5-blip-ui-sound-effects — CC0.
  - Arquivo: `Assets/Audio/Dialogue/SFX_Dialogo_TextBlip_05.wav`, a variante 05 já editada no pacote (50 ms, corte/fades/nível/reamostragem).
  - Crédito voluntário: “Dialogue blips: FunnyDude (CC0); editorial adaptations”.
- **Pixelify Sans — The Pixelify Sans Project Authors**, texto das falas.
  - Fonte: Google Fonts / github.com/google/fonts (ofl/pixelifysans). Licença SIL OFL 1.1 em `Assets/Fonts/PixelifySans/OFL.txt`.
- Pacotes, licenças, manifests e hashes: `SourceArt/GusiniMusic/Phase1` e `SourceArt/GusiniMusic/Phase2`.

A composição e os sons originais não são do autor do projeto nem do agente. Os
retratos, a interface "Rádio de Piloto" e a direção dos diálogos foram
fornecidos pelo autor do projeto (pacote `Dialogo_Completo.zip`); não se presume
autoria externa para eles.

## Derrota do Gusini (F2 → recuperado) fornecida pelo autor do projeto

Pacote `Gusini_Derrota_F2_F1_v2.zip` (versão com a nave avariada): 18 quadros
compostos e o loop de 4 quadros do Gusini exausto em `Assets/Art/Boss/GusiniDefeat`,
copiados sem alteração; JSON e notas em `SourceArt/GusiniDefeat`, geradores em
`Tools/Art/GusiniDefeat`. Previews, GIFs, sheets, camadas separadas e FX soltos não
foram importados. Não se presume autoria externa.

## Arte do novo menu fornecida pelo autor do projeto

Os onze PNGs de céu, logo, botões, diamante e nuvens enviados no chat foram
incorporados em `Assets/Art/MainMenu`. Os arquivos originais não foram alterados;
os recortes são metadados de importação dos sprites na Unity. As duas imagens
de rachaduras enviadas junto por engano não foram usadas nesta etapa.

Não foi presumida autoria externa nem licença para essas imagens. O registro
da origem deverá ser preenchido pelo autor do projeto antes da tela de créditos.

## Arte do menu de pausa fornecida pelo autor do projeto

As nove artes de logo, moldura, título, botões, dica, divisor e diamante enviadas
no chat ficam em `Assets/Art/PauseMenu`. Os PNGs originais foram preservados;
os recortes são metadados de sprites da Unity. O background de céu da referência
não foi incorporado: a pausa mostra uma captura do próprio jogo com blur.
O divisor duplicado também não foi copiado. A autoria externa não foi presumida.

## Naves dos novos inimigos — CraftPix

- Pack gratuito: **Free Enemy Spaceship 2D Sprites Pixel Art**.
- Autor/distribuidor: **CraftPix / Free Game Assets (GUI, Sprite, Tilesets)**.
- Página: https://free-game-assets.itch.io/free-enemy-spaceship-2d-sprites-pixel-art
- Origem: https://craftpix.net/freebies/free-pixel-art-enemy-spaceship-2d-sprites/
- Licença do autor: https://craftpix.net/file-licenses/ — seção Freebie License.
- Uso: Ship1 no interceptor; Ship3 no artilheiro. Casco, motor e explosão ficam em
  `Assets/Art/Enemies/Interceptor` e `Assets/Art/Enemies/Artillery`.
- Os PNGs usados foram copiados sem alteração; o ZIP, PSD e SCML não fazem parte dos assets do jogo.
- Crédito sugerido: “Naves interceptor e artilheiro: CraftPix”.
- O efeito de carga do ansimuz já registrado acima também é usado nos avisos dessas naves.

## Arte e áudio do port original

O boss, retrato, barras, mísseis, lasers e explosão vêm dos recursos já presentes
no Crocodilo Invaders. Suas cópias para SpriteRenderer ficam em `Assets/Art/Boss`;
os arquivos originais continuam em Resources.

A autoria/licença de cada recurso legado ainda precisa ser identificada para a
tela completa de créditos. O projeto não registra essas fontes no README atual;
não foi atribuída uma autoria presumida aos arquivos antigos.

A caixa de vida usa cópias de `health_box` e `hp_up` do port em
`Assets/Art/Pickups`, além do áudio original `powerUp_Sound`, já ligado ao
`PlayerHealthAudio`. Não foram adicionadas artes externas para essa mecânica.

## Packs consultados, sem arquivos incorporados

- **ARCBIT Lite — gennai**: https://gennai.itch.io/arcbit-lite
- **Super Pixel Effects Gigapack — unTied Games**:
  https://untiedgames.itch.io/super-pixel-effects-gigapack

Ao usar outros efeitos futuramente, registrar quais arquivos e qual edição foram
utilizados. No caso do Gigapack, o pedido atual autoriza somente a edição gratuita;
a página do autor solicita atribuição.

## Gusini — áudio da Fase 2

29 masters WAV fornecidos pelo usuário em `Gusini_Audio_Phase2.zip`, em 18/10/2026. O pacote registra geração no ElevenLabs e edição editorial própria; os masters foram incorporados sem alteração. Manifesto de origem, mapa de eventos, hashes e informações fornecidas sobre direitos estão preservados em `SourceArt/GusiniAudioPhase2`. Candidatos, previews e os oito opcionais adiados não fazem parte dos assets de runtime.

## B. Crocodilo a pé — rig da Base Improvisada

Pacote `Croc_Rig_Ready.zip` fornecido pelo usuário em 18/10/2026. Os 23 módulos PNG
foram incorporados sem alteração; master, idle aprovado, manifesto, guia, previews
e ferramentas originais estão em `SourceArt/Croc_Rig_Ready`. O idle modular da
Unity transcreve os deslocamentos aprovados. Não se presume licença ou autoria
externa além das informações fornecidas no pacote.

## Macaquini — transformação v12 (23/10/2026)

Revisão v13: cinco folhas finais geradas com image_gen integrado a partir do Macaquini do próprio projeto; retrato corrompido do Goosini como referência de coerência da infecção. Fontes, prompts, exclusões e metadados em `SourceArt/Macaquini_v13`. Desenhos completos recortados em `Assets/Art/JungleStage/Macaquini_v13`, sem incorporar personagens ou áudio externos.

Cinco folhas de sprites geradas com o image_gen integrado a partir do modelo já existente do projeto, preservando sua identidade. Fontes e prompts em `SourceArt/Macaquini_v12`, sprites recortados em `Assets/Art/JungleStage/Macaquini_v12`. As imagens anexadas pelo usuário foram referências de movimento, sem incorporar seus personagens. `power_roar_v12.wav` é uma edição em camadas do próprio `growl.wav` já existente do Macaquini; não foi incorporado áudio externo nesta revisão.

## Música das ondas

Going Up — adventure Chiptune, de Ansimuz / Luis Zuno. WAV original do pacote fornecido pelo usuário, sem alteração, usado nas hordas antes do boss. O pacote registra CC0; licença original e manifesto preservados em SourceArt/WaveMusic. Fonte registrada: https://opengameart.org/content/going-up-adventure-chiptune .

## Anúncio de ondas e alerta PERIGO! GOOSINI (18/10/2026)

- Arte `Wave_Boss_UI_Goosini.zip` fornecida pelo usuário (cores amostradas do retrato aprovado do Gusini F1; scripts de geração em `Tools/Art/WaveBossUI`): 74 PNGs em `Assets/Art/UI/WaveBanner` e `Assets/Art/UI/BossAlert`, sem alteração. Guia, manifesto, paleta e faixa montada de referência em `SourceArt/WaveBossUI`; previews em `Docs/ArtPreviews/WaveBossUI`. Atlas, variantes `pop2` e GIFs não foram importados. Não se presume autoria externa.
- SFX `goosini_boss_alert_final.wav` (candidato B1): exportação do ElevenLabs Sound Effects do pacote `Crocodilo_Invaders_Goosini_Boss_Alert.zip`, editada no pacote (DC, corte a 2 s, fades curtos, pico −3 dBFS). O uso segue os termos da conta ElevenLabs do usuário. Prompts, créditos consumidos, proveniência, QA e hashes em `SourceArt/GoosiniBossAlert`. Candidatos, originais e prévias não fazem parte do runtime.

## Base Improvisada — cenário v01 (18/10/2026)

Arte do kit `BaseImprovisada_Kit_Implementacao_v01.zip` fornecido pelo usuário (lotes 01–03 e construções, gerados para o projeto): PNGs originais em `Assets/Art/Environment/BaseImprovisada`. Os 10 suprimentos entram como cópias reduzidas, e os originais ficam em `SourceArt/BaseImprovisada/Originals`. O panorama é `03_BG2_montanhas_referencia_RGB.png`. Manifest, layout, README e Art Bible estão em `SourceArt/BaseImprovisada`. O marcador de escala usa `croc_hub_master.png` do pacote `Croc_Hub_Simplified_Idle_v01` (usuário). Não se presume autoria externa nem licença.

## B. Crocodilo simplificado — HUB (18/10/2026)

Master 48×72 e seis quadros de Idle de `Croc_Hub_Simplified_Idle_v01.zip`, fornecidos pelo usuário e incorporados sem redesenho/alteração de pixels. ZIP, manifesto e guias preservados em `SourceArt/CrocHubSimplifiedIdle`; runtime em `Assets/Art/Characters/CrocHub`. Usa a alternativa de frames prontos autorizada pelo guia do pacote. Não se presume autoria/licença externa. Nenhuma Run foi incorporada.
