# Efeitos das cartas v22 e áudio original v25

As 25 artes da coleção v21 continuam ativas. Os efeitos externos permanecem
progressivos: comum discreta, rara azul com poucas faíscas, épica com energia
violeta, lendária com estrelas douradas e mítica com arcos curtos carmesim.

Os nove sons de `Assets/Audio/CardRewards` são originais, sintetizados para o
jogo. Não usam samples ou pacotes de áudio externos. As revelações combinam
pulso arcade suave, frases ascendentes em tom maior e cristais FM. A composição
fica mais cheia conforme a raridade. A confirmação fecha com notas de conquista;
o reroll usa um deslocamento eletrônico curto e leve.

Lendárias e míticas têm somente cintilações curtas e irregulares, sem música,
acordes, melodia ou batida. O brilho é compartilhado pela oferta, com volumes
0,24/0,28. Duas ou três cartas altas dividem o ganho das revelações por sua
quantidade, impedindo a multiplicação do volume. Pausa, reroll e saída limpam
as fontes como antes.

Receita ativa: `Tools/Audio/prepare_card_audio_v25.py` (recria os sons originais
v24 e troca os dois fundos por brilho). Manifesto completo de hashes e procedência:
`SourceArt/CardAudio_v25_Sparkle`. Detalhes em `AudioCartasBrilho_v25.md`.
Os três pacotes de áudio Kenney baixados anteriormente e os geradores v22/v23
foram removidos do projeto. Os vídeos/pacotes antigos são registros históricos.

Detalhes atuais: [AudioCartasBrilho_v25.md](AudioCartasBrilho_v25.md).
