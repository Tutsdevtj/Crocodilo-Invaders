# Sons originais das cartas — v24

Substitui os sons v22/v23 rejeitados. Os nove WAVs são composições próprias
sintetizadas para Crocodilo Invaders: sem samples, jingles ou loops externos.

O timbre combina pulso arcade suave com cristais FM e pad de acordes maiores.
As frases sobem e resolvem em notas consonantes, com mais camadas na raridade
mais alta. Comum 0,26s, rara 0,62s, épica 0,94s, lendária 1,40s, mítica 1,95s.
Escolha 0,65s e reroll 0,20s completam a identidade sonora.

Os fundos de tesouro têm 6,4 segundos, acordes maiores e arpejos cintilantes.
A mítica acrescenta a oitava superior e um acorde mais cheio. Mixer, GUIDs,
referências e controles de pausa/limpeza foram preservados.

Receita: `Tools/Audio/prepare_card_audio_v24.py`. Depende só de NumPy e da
biblioteca padrão Python, sem leitura de qualquer áudio externo. Manifesto e
procedência em `SourceArt/CardAudio_v24_Original`. PCM mono 44,1kHz / 16 bits.
As pastas completas dos pacotes de áudio adicionados, ZIPs, originais, geradores
v22/v23 e créditos Kenney do build de trabalho foram removidos.

Verificação de WAVs: nove hashes conferidos, junções dos loops sem salto,
progressão de duração/energia e picos seguros. A mistura de três míticas com
intervalo de 200ms mais o fundo de tesouro fica em pico 0,7453, sem clipping.
Os vídeos e RARs anteriores são históricos e contêm os sons anteriores.

Unity Play Mode: 118 verificações passaram, errors=0. Importação e referências
aprovadas, assim como PCM/loops, pausa, reroll, escolha e limpeza no menu.
Log: `Logs/CardAudio-v24-Play.log`. Checkout: 1.890 GUIDs conferidos.
Vídeo real: `Docs/ArtPreviews/CardAudio_v24/sons-originais-5-rerolls.mp4`,
782 quadros / 30 fps / 1280×720, com mixagem da Unity e cinco rerolls após 3s.

Build Windows 6000.6.4f1 aprovado; inicialização de 8s sem exceções listadas.
Pacote RAR5 testado por CRC e conteúdo obrigatório:
`Builds/Pacotes/CrocodiloInvaders-DEV-SonsOriginais-v24-Windows.rar`.
SHA-256: `89bd2c7625ead5109c1d9dd67344db7c5de1ac31d3ef7bca3461b57d60f865fd`.
