# Canhão de banana alienígena — versão poderosa v02

Pedido de 20/10/2026: deixar a arma mais poderosa. A v01 foi preservada.

O redesenho mantém o formato de banana e a direção para a esquerda, mas aumenta a espessura do corpo e das abas, reforça a casca com nervuras douradas, adiciona duas câmaras verdes de energia e amplia o colar e a empunhadura roxos. A arma continua independente do macaco.

- Arma nativa em PNG transparente **128×96**, com 12 posições do ciclo em `frames/`.
- Paleta fixa **20 cores** e alpha binário, sem suavização; previews em escala inteira.
- Recoil visual de **8 pixels**, o dobro da v01; retorno seguido do fechamento da casca.
- Projétil separado **32×16**, maior que os 24×12 da v01; fonte da banana preservada, sem mudar sua identidade.
- Ciclo de 1 segundo; evento de tiro no índice 3/320 ms. Última pose idêntica à primeira para a emenda do loop.
- Ponto de pegada nominal **(86,60)** e pivô normalizado **(86/128,1−60/96)**; posições durante o recuo em `animation.json`.

`weapon_idle.png`, `spritesheet_6x2.png` (768×192), `frames/`, `projectile/banana.png`, `animation.json`, `palette.json` e `qa_report.json` são os arquivos finais. `sources/` e `prompts.txt` preservam as gerações. A folha de retorno foi ordenada por abertura decrescente (células 3,4,2,1,0,5); o export remove fragmentos desconectados de células vizinhas. O último quadro é a cópia intencional do idle para fechar o ciclo.

Prévia com projétil independente em `Docs/ArtPreviews/BananaAlienWeapon_v02_Power/banana_weapon_fire_x4.gif`. O GIF pode consolidar frames iguais mantendo os tempos. Conferidas a pose de disparo, pixels nativos, cores, transparência, repetição inicial/final e metadados. Ferramenta: `Tools/Art/package_banana_alien_weapon_v02.cjs`.

Esta revisão aumenta o impacto visual. Não altera dano, cadência ou lógica no jogo e não encaixa a arma no macaco; mantém essa etapa para depois, conforme o pedido original.
