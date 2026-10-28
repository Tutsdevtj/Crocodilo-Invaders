# Cartas: brilho sem música e ganho por oferta — v25

Os fundos lendário e mítico agora são somente cintilações curtas, inarmônicas
e espaçadas irregularmente. Não há melodia, acordes, pad sustentado ou batida.
Os arquivos mantêm 6,4 segundos por compatibilidade, com silêncio entre os brilhos.
São sons originais; nenhum sample/pacote externo foi adicionado.

O brilho continua compartilhado pela oferta, usando somente a maior raridade.
Volumes lendário/mítico: 0,24/0,28. Pausa, reroll, escolha e menu mantêm a limpeza.

CardRewardView conta as lendárias/míticas antes da revelação. CardRarityAudio
aplica ganho 1/N a cada uma delas: duas recebem 0,5; três recebem 0,3333. Os sons
das cartas comuns/raras/épicas mantêm seus ganhos. Assim, as revelações altas
compartilham o orçamento de volume, inclusive em ofertas com lendária + mítica.

Receita ativa: Tools/Audio/prepare_card_audio_v25.py. Recria os sons originais
v24 e substitui os dois fundos. Manifesto completo: SourceArt/CardAudio_v25_Sparkle.
GUIDs/referências mantidos; nenhuma alteração de mixer ou arte das cartas.

Verificação de áudio: simulação com uma/duas/três míticas deu picos
0,3208 / 0,3097 / 0,2557 antes do mixer. Limite conservador para as 125 combinações
de raridade, incluindo os dois brilhos durante crossfade: 0,7012 (<1).
A gravação real da Unity teve pico global 0,0963, sem clipping.

A prévia em Docs/ArtPreviews/CardAudio_v25/brilho-sem-musica-volume-controlado.mp4
inclui ofertas com três e depois duas míticas, cinco rerolls e 3s de espera.
Ofertas de teste são configuradas na captura antes da revelação real, sem salvar
a cena ou alterar o catálogo. Os eventos estão no CSV ao lado do vídeo.

Unity Play Mode: 122 verificações aprovadas, errors=0, incluindo duas míticas
com ganho 0,5, três com 1/3 e um só brilho por oferta. Log: Logs/CardAudio-v25-Play.log.
Checkout aprovado: 1.890 GUIDs. Vídeo validado por decodificação completa: 789 quadros/30fps.

Build Windows aprovado; inicialização 8s sem exceções listadas. RAR5 aprovado
por CRC/conteúdo: `Builds/Pacotes/CrocodiloInvaders-DEV-BrilhoSemMusica-v25-Windows.rar`.
SHA-256: `b88b57e88a378b02bb1056dd184a4e915345f3799863d20bfd566a32b43cee5e`.
