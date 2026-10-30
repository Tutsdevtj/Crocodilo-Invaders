# Língua encaixada no asset — v09 local

**Protótipo local. Nenhum commit, push ou atualização da PR até o usuário dizer que está finalizado.**

O corpo da língua foi redesenhado junto da ponta em um único asset de pixel art, usando a mesma paleta coral, sombras vinho, relevo e brilhos da ponta e da boca. A renderização usa uma malha contínua com essa imagem, substituindo as três linhas e a ponta separada da v08.

A base se ancora no centro da língua enrolada dentro da boca, em (698,625) pixels da pose original de 1536 × 1024, com origem inferior esquerda. A transição inicial de alfa integra a base à cavidade. As regiões da base e da ponta mantêm suas proporções; o trecho central se alonga na extensão e se curva suavemente durante o recolhimento. A orientação acompanha o sapo para os dois lados.

Moscas, avisos, dano, dash, variantes, saltos e duração dos golpes mantêm a lógica do protótipo. A malha é ocultada quando o ataque acaba ou a luta é encerrada, e usa um material compartilhado próprio sem criar instâncias a cada golpe.

## Arquivos

- Master e prompt: `SourceArt/LagoDoPneu_TongueFit_v09`.
- Sprite, material e dados nativos: `Assets/Resources/LakeTongueV09`.
- Renderização: `LakeTongueSkin` e `LakeTongueSkinData`, acionados por `LakeTongueAttack`.
- Importação: `LakeTongueSkinArtTools.Install`; a reinstalação da v08 também preserva a nova pele.
- Prévias reais: `ArtPreviews/LagoDoPneu_TongueFit_v09`.

Build local: `Builds/Windows-LagoDoPneu-TongueFit-v09/CrocodiloInvaders.exe`. Entrada **DEV → BOSS FINAL → SAPO DO PNEU**.

Validação nativa: **47 verificações sem erros**, incluindo uma única superfície com a textura correta, posição dos vértices na boca e na ponta física, as duas orientações, dano, dash, variantes e limpeza. A língua herda a cor do sapo na fúria e no clarão de dano, mantendo a base integrada à boca.

Vídeo aproximado e lento: `ArtPreviews/LagoDoPneu_TongueFit_v09/mouth-fit-closeup-v09.mp4`. Vídeo completo: `lago-tongue-fit-v09.mp4`.

Build Windows concluída, inicialização de 10 segundos sem exceções, ZIP com arquivos conferidos: `Builds/Pacotes/CrocodiloInvaders-LagoDoPneu-TongueFit-v09-TESTE-LOCAL-Windows.zip`. HEAD segue `f16c05e`, índice vazio, sem commits, push ou atualização de PR.
