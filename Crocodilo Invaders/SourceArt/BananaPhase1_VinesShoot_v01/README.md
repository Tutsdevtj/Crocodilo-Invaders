# Boss fase 1 — cipós, pausa e tiro

Pedido de 20/10/2026: pixel art em frames seguindo a identidade do boss do projeto, ainda no nível1, pulando em cipós, parando e atirando com a arma. Referências diretas: primeiro quadro da transformação pixelartv03 (casca fechada e membros esguios) e arma orgânicav04 (menos tecnologia, talo marrom, veios verdes). O Goosini nativo foi conferido como contexto visual do sistema.

São **24 desenhos distintos** em PNG transparente **160×160**, paleta compartilhada de até24cores (22 usadas nos frames) e alpha binário. A cabeça e o tronco do macaco continuam escondidos pela banana fechada em todos os quadros; o rosto maligno pertence à casca. Somente a casca da arma abre. Sem a musculatura ou cabeça exposta da fase2.

## Ações

- 01–06: pendura no cipóA, balança, prepara impulso, solta e salta.
- 07–12: alcança o cipóB, agarra, amortece e reduz o balanço até parar.
- 13–18: ergue a arma, sustenta a mira, abre a ponta, dispara e absorve o recoil.
- 19–24: retorna a arma fechada, faz uma pausa, prepara impulso e volta ao cipóA.

Duração **4,08s**, com pausas maiores na chegada e na mira. O disparo acontece no índice15 (quadro16), a2660ms. O projétil grande é o PNG64×32 da arma orgânicav04, copiado separadamente e composto apenas na prévia. Não está pintado dentro dos sprites do boss.

## Arquivos

- `frames/phase1_00.png` a `phase1_23.png`: exports nativos.
- `spritesheet_8x3.png`: atlas1280×480, células160×160, leitura por linhas.
- `clips/`: quatro faixas de seis frames (960×160): `swing_jump`, `catch_stop`, `aim_shoot`, `recover_return`.
- `animation.json`: nomes, ordem, tempos, divisão dos clips, evento de tiro e contato de cipó sugerido. A posição da mão de apoio é uma aproximação extraída da imagem; conferir ao fazer um rig.
- `projectile/banana.png`: banana grande separada,64×32.
- `palette.json` e `qa_report.json`: cores, hashes, dimensões, eventos e verificações.
- `sources/`, `frames_source/` e `prompts.txt`: folhas geradas, recortes e prompts completos.

Prévias em `Docs/ArtPreviews/BananaPhase1_VinesShoot_v01/`: GIF nativo, `phase1_vines_shoot_x2.gif` com projétil e ampliação inteira2×, quadro de tiro, atlas ampliado2× e HTML para inspecionar cada desenho. O HTML usa os sprites sem projétil; a composição completa aparece no GIF. O GIF repete a demonstração, e o HTML tem repetição opcional.

Verificados24frames distintos por hash, tamanho comum, paleta, alpha binário, contagem/tempos do GIF e sintaxe do JavaScript; revisados visualmente atlas e disparo, incluindo manutenção da fase1. Ferramentas: `Tools/Art/split_banana_phase1_vines.ps1` e `package_banana_phase1_vines.cjs`.

## Escopo de integração

Os PNGs incluem corpo, arma na mão e cipós na mesma composição; o movimento entre cipós está desenhado dentro dos frames. São arte de animação, não um rig modular ou sistema de navegação. Para uso com cipós independentes do cenário, será preciso separar as camadas e transferir a trajetória para o controlador. As folhas geradas também podem receber polimento de continuidade do cipó antes dessa integração. Não foram criados prefab, Animator, colisões ou lógica de boss. O boss ativo e as artes anteriores foram preservados.
