# Áudio das cartas — recompensa e tesouro v23

Revisão da v22 após o usuário avaliar que os sons pareciam falha/derrota.
As nove referências e GUIDs foram preservados. Os WAVs ativos estão em
`Assets/Audio/CardRewards`; a receita é `Tools/Audio/prepare_card_audio_v23.py`.

As revelações agora usam frases ascendentes em Dó maior, com um timbre de sino
feito a partir da ressonância de `glass_001.ogg` do Kenney Interface Sounds
(CC0). O sample recebe equalização suave, afinação, envelopes e ecos. Não há
vinhetas NES prontas, explosões graves ou varreduras descendentes nesta versão.
Nenhum oscilador foi usado para substituir a gravação. O reroll usa `tick_001`.

- Comum: duas notas leves e curtas.
- Rara: três notas ascendentes.
- Épica: quatro notas com apoio de acorde maior.
- Lendária: subida mais longa, acorde maior e brilhos agudos.
- Mítica: duas subidas, resolução aguda e acorde mais cheio.
- Escolha: confirmação curta com resolução em acorde maior.

Os fundos de tesouro são pequenas músicas de 6,4 segundos: acordes C/F/G/C,
arpejos de sinos e ecos cíclicos. A mítica acrescenta cintilação na oitava
superior e mais presença. As bordas dos WAVs têm fade para evitar estalos.
Apenas um fundo toca por oferta, conforme a maior raridade; volumes, mixer,
pausa, reroll e limpeza permanecem sob o controle existente de CardRewardView.

Originais/licenças continuam preservados em `SourceArt/CardAudio_v22_CC0`.
Manifesto de hashes e descrição atual: `SourceArt/CardAudio_v23_Reward`.
Backup dos sons anteriores: `Backup/Cartas/2026-10-22_AntesAudioRecompensa_v23`.

A prévia usa a cena e o fluxo reais, com cinco rerolls e três segundos completos
após cada oferta estar pronta. A captura inclui a saída de áudio da Unity.
O RAR v22 anterior continua histórico e contém os sons anteriores.

Validação: importação/atribuição dos nove clips aprovada; suíte real de Play Mode
passou 118 verificações, sem erros. Log: `Logs/CardAudio-v23-Play.log`.
Vídeo MP4 validado por decodificação completa: 778 quadros, 30 fps, 25,93 s,
1280×720, áudio estéreo capturado da Unity em 48 kHz. Cinco rerolls registrados
em `Docs/ArtPreviews/CardAudio_v23/events.csv`, com 90 quadros de espera após Ready.
Prévia: `Docs/ArtPreviews/CardAudio_v23/recompensas-tesouro-5-rerolls.mp4`.
