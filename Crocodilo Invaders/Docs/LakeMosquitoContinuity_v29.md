# Mosquitos pequenos — continuidade v29

Corrigidos dois saltos independentes:

- A patrulha começava com uma senoide baseada no relógio inteiro da chegada, causando deslocamento vertical instantâneo ao entrar no voo. Agora a oscilação começa a partir da chegada, cresce suavemente em 0,6 s, tem amplitude de 0,22 unidade e deslocamento vertical limitado a 0,75 unidade/s.
- Os quadros originais das asas tinham os olhos e o corpo em alturas diferentes. LakeMosquitoRegistrationTools.Install registra os quatro quadros pelo centro dos olhos vermelhos e salva os deslocamentos em LakeJourneyArt. O renderer aplica o deslocamento correto em ambos os sentidos.

As imagens originais são preservadas, sem redesenho. Mosquitos grandes, ataques, fundo opaco, demais inimigos e boss permanecem na versão anterior.

Conferência temporária na primeira horda real, com chegadas e patrulha de vários mosquitos. Captura e registros em Logs/LagoDoPneu/v29; vídeo em Docs/ArtPreviews/LakeMosquitoContinuity_v29. Auxiliar de captura arquivado fora de Assets após o uso.

Sem novos commits/push ou atualização da PR.


Conferência concluída na primeira horda: quatro mosquitos e quatro chegadas, 996 amostras de patrulha. Maior velocidade vertical medida em patrulha: 0,587 unidade/s; na transição de entrada: 2,041 unidades/s, dentro do deslocamento normal da chegada. Sem descontinuidade, sem erros de execução. Checkout: 2627 GUIDs válidos; vídeo nativo decodificado sem erros.

Build Windows-LagoDoPneu-MosquitoContinuity-v29 concluída com sucesso (207224339 bytes de conteúdo informado pela Unity). Pacote local inclui jogo e vídeo.
