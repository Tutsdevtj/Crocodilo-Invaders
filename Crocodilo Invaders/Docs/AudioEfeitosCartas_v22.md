# Sons CC0 e efeitos graduais das cartas — v22

Os nove WAVs em `Assets/Audio/CardRewards` foram preparados a partir de gravações dos pacotes Kenney Interface Sounds, Sci-fi Sounds e Music Jingles, todos CC0. Originais, ZIPs, licenças e manifesto de hashes preservados em `SourceArt/CardAudio_v22_CC0`. Receitas reproduzíveis em `Tools/Audio/prepare_card_audio_v22.py`; dependência de decodificação local `soundfile==0.13.1` e NumPy.

## Sons

- Comum: seleção curta e discreta.
- Rara: confirmação e pequeno brilho.
- Épica: vinheta arcade com mais notas e camadas.
- Lendária: vinheta de recompensa com reforço metálico e brilho.
- Mítica: fanfarra mais longa, impacto grave e brilho em camadas.
- Confirmação e reroll também usam samples CC0; os MP3 antigos não são mais usados pelas cartas.

Lendária e mítica têm loops próprios de tesouro de 6,4 segundos, com gravações de brilho em camadas, alturas e ecos. Os loops têm fades nas junções e picos limitados. Não foram criados por osciladores.

`CardRewardView` administra duas fontes compartilhadas de ambiência com transição suave: somente a maior raridade revelada continua tocando; três míticas não triplicam o loop. As fontes herdam o grupo do mixer já utilizado pelas cartas. Volumes de repouso configurados no prefab: 0,32 lendária e 0,40 mítica. Fontes 2D, prioridade 180, sem playOnAwake e respeitando pausa. Escolha faz fade de saída; reroll/fechamento/disable/menu interrompem e limpam os loops.

`CardAudioPackageTools.Install` importa PCM/preload e configura as referências por APIs públicas do Editor, sem criar grupos nem editar o mixer. `Validate` verifica os nove clips, referências do prefab, progressão de duração/RMS, picos e continuidade dos loops.

## Visuais

Comum tem só um estalo breve, sem aura contínua. Rara tem poucas faíscas azuis e um brilho que percorre o contorno. Épica acrescenta energia violeta e segmentos de luz. Lendária ganha estrelas douradas e raios contidos. Mítica tem brilho carmesim, mais traços e arcos curtos presos ao entorno da moldura. A malha é reutilizada, sem GameObjects por partícula. Alfa dos vértices é limitado verticalmente para proteger títulos e comparações.

O antigo sprite vertical ThunderRed deixa de ser esticado nas laterais; os novos arcos são geometria nativa curta e localizada. O asset antigo permanece preservado. Artes, foils/relevos ilustrados e metadados das 25 cartas não foram alterados nesta etapa.

## Validação e teste

A suíte `CardEffectsPlayValidation` passou **118 verificações, sem erros**, conferindo avanço real do áudio, pausa, um loop por oferta, três míticas sem multiplicação de ambiência, limpeza por escolha/reroll e troca para o menu com o tesouro ativo. Inclui as 25 cartas, capturas e limites da malha. A importação/atribuição dos nove clips e os dois loops também passaram em `CardAudioPackageTools.Validate`, integrado à validação do build.

Build Windows 6000.6.4f1 passou. Executável iniciou por oito segundos sem exceções de carregamento/runtime. Capturas das cinco raridades e resoluções 1200×720, 800×600 e 1920×1080 em `Docs/ArtPreviews/CardAudioFX_v22`. Não foi afirmada escuta humana dos áudios ou partida manual completa.

Pacote `Builds/Pacotes/CrocodiloInvaders-DEV-AudioFX-v22-Windows.rar`, 87.716.663 bytes, RAR5 verdadeiro testado por CRC e conteúdo obrigatório. Checksum em `SHA256SUMS-v22.txt`. Extraia todos os arquivos e abra o executável; DEV/F8 permite fixar a raridade e gerar ofertas sem limite.

Backup anterior: `Backup/Cartas/2026-10-22_AntesAudioFX_v22`, na raiz do repositório. Licenças dos samples incluídas também em `Creditos/Audio` no pacote Windows. O compactador RAR oficial é usado localmente, sem instalação global e sem redistribuir a ferramenta no jogo.
