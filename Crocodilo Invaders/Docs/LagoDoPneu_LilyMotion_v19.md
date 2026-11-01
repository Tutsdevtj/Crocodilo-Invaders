# Folhas à deriva v19

As seis folhas decorativas agora seguem a corrente continuamente para a esquerda, a 0,11–0,185 unidade/s, com pequenas variações laterais. Ao sair completamente da tela, voltam pelo outro lado ainda fora da área visível. Profundidade e perspectiva baixa da arte v16 são preservadas.

As duas folhas que sustentam a Saperereca também derivam, oscilando lateralmente cerca de meia unidade em ritmos diferentes, sem abandonar a região de combate. Todas balançam verticalmente com duas frequências, rodam discretamente (apoios até 1,1 grau; decorativas até 1,8) e flexionam até 3,5% em altura. Não foi alterado o PNG.

Ao aterrissar, a folha cede e recupera com uma oscilação amortecida de 0,07 unidade. Support calcula o mesmo deslocamento, rotação e altura usados no desenho; a Saperereca acompanha esse ponto no repouso, aviso e aterrissagem, inclusive durante o arroto. O destino do salto acompanha a deriva e sua escolha considera a posição atual da folha.

Tudo usa o relógio escalado do LakeBackdrop e congela junto na pausa. Mantém ausência de colisão com a nave/tiros e as camadas existentes. Referência de física usada: `create-2d-physics/references/legacy-2d-physics.md`; o movimento mantém o Rigidbody2D cinemático e MovePosition já usados no personagem.

Validação real pela entrada DEV: **41 checks, zero erros**, incluindo deriva dos apoios, corrente das decorativas, pausa de posição e rotação, acompanhamento dos pés sem escorregar, aterrissagem, apoio no combate e regressão do HUD, moscas, arroto e limpeza. Vídeo decodificado sem erros em `Docs/ArtPreviews/LagoDoPneu_LilyMotion_v19/lago-lily-motion-v19.mp4`. Logs e restauração em `Logs/LagoDoPneu/v19`.

Alteração local, sem commit/push/PR e sem build nova. A build v17 não inclui os ajustes v18/v19.
