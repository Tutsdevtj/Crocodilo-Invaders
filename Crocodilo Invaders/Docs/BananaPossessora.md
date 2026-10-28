# Banana possessora — direção de arte e história

## Integração atual — Macaquini v13 (23/10/2026)

O boss integrado usa jetpack na primeira forma e cipó na segunda. A v13 revisa a expressão da casca como banana infectada por alienígena, coerente com a corrupção do Goosini, preservando o macaco. O rasgo tem 42 quadros cronológicos com progresso visual protegido e busca/pegada do cipó com o mesmo braço. Fonte: `SourceArt/Macaquini_v13`; detalhes em `Fase2MacaquiniContinuidade_v13.md`. Os estudos abaixo são históricos e não substituem o pedido atual de jetpack e transformação.

## Fase1 nos cipós, com tiro (20/10/2026)

`SourceArt/BananaPhase1_VinesShoot_v01/`: 24 desenhos nativos160×160/4,08s, mantendo a banana do corpo fechada, membros esguios e rosto do macaco oculto. Balanço/salto → chegada no outro cipó e pausa → mira/abertura da arma/tiro/recoil → fechamento e retorno. Usa arma orgânicav04, banana-projétil64×32 separada, disparo no quadro16/índice15 a2660ms. Atlas8×3, quatro clips de seis quadros, JSON de tempos/eventos, fontes/prompts e QA. GIF com projétil em `Docs/ArtPreviews/BananaPhase1_VinesShoot_v01/phase1_vines_shoot_x2.gif`. Cipós e arma na mão estão desenhados nos mesmos PNGs do personagem; movimento está incorporado nos frames. Não é rig nem implementação de gameplay, e ainda requer separação de cipós para cenário independente. Preserva boss ativo e arquivos anteriores.

Arma atual v04, pedido “menos tech e mais banana”: `SourceArt/BananaAlienWeapon_v04_Organic/`, casca curva natural e talo marrom na pegada, sem colar/placas roxas, reatores ou aleta tecnológica. Alienígena apenas nos veios verdes e interior da abertura. Ciclo de12 posições/1s, recoil8px, arma128×96 e projétil64×32 preservados. Pegada(86,60), boca(32,39), tiro índice3/320ms. GIF em `Docs/ArtPreviews/BananaAlienWeapon_v04_Organic/`; seis desenhos-base e retorno em ordem inversa. Históricos preservados, sem integração ao personagem.

Arma atual v03, pedido seguinte de 20/10/2026: banana disparada ampliada de32×16 para64×32. Pacote `SourceArt/BananaAlienWeapon_v03_BigProjectile/`, prévia em `Docs/ArtPreviews/BananaAlienWeapon_v03_BigProjectile/`. A saída foi reposicionada para o projétil maior; os 12 frames da arma são idênticos à v02 por SHA-256. Ciclo/recoil preservados, sem integração ao gameplay.

## Arma mais poderosa — v02 (20/10/2026)

Pedido seguinte: “Deixa mais poderosa.” Redesenho em `SourceArt/BananaAlienWeapon_v02_Power/`, com corpo mais grosso, nervuras douradas, duas câmaras de energia verde, colar/empunhadura roxos reforçados, abas maiores, recoil de 8 px (antes 4) e banana-projétil 32×16 (antes 24×12). 12 posições em canvas nativo 128×96, paleta20/alpha binário, ciclo de 1 s. Pegada (86,60), evento de tiro índice3/320ms. GIF em `Docs/ArtPreviews/BananaAlienWeapon_v02_Power/`. A v01 continua salva; força aumentada visualmente, sem mudanças de combate nem encaixe no personagem.

## Arma separada — banana alienígena (20/10/2026)

`SourceArt/BananaAlienWeapon_v01/`: arma separada para futuro encaixe no macaco, em formato de banana, voltada à esquerda, casca amarela com circuitos verdes e empunhadura roxa. Ciclo de 12 posições/1 s: abre a casca dianteira, dispara a fruta como projétil separado, recua até quatro pixels para trás, retorna fechando e repete. Exports nativos 112×80 e projétil 24×12, até 20 cores e alpha binário. Ponto nominal de pegada (72,50), boca (32,34), tiro no índice 3/320 ms; JSON inclui variação por frame. GIF com projétil composto em `Docs/ArtPreviews/BananaAlienWeapon_v01/`. Sem arma ou mão incorporada ao boss, sem gameplay nesta entrega.

## Correção visual atual — pixel art v03 (20/10/2026)

O usuário rejeitou o acabamento ilustrado da v02 e pediu pixel art real. A animação atual está em `SourceArt/BananaPossessora_Transformacao_v03_PixelArt/`: 24 desenhos redesenhados com referência do Goosini, resolução nativa 128 × 128, uma paleta compartilhada de 20 cores mais transparência, alpha binário e contornos em degraus. Ação e tempos preservados (4,22 s). GIF nativo, prévia exata 4× e player por quadro em `Docs/ArtPreviews/BananaPossessora_Transformacao_v03_PixelArt/`. A v02 permanece como histórico, não como acabamento aprovado. Fontes e prompts salvos; sem mudança em cenas ou gameplay.

## Transformação atual — 24 frames (20/10/2026)

`SourceArt/BananaPossessora_Transformacao_v02_24Frames/` entrega a transformação detalhada solicitada: força e raiva crescentes → agarra a ponta superior → arranca o talo com a ponta → arremessa → rasga a casca verticalmente com as duas mãos → rugido e pose semiaberta da fase 2. A ponta superior não reaparece na forma final. São 24 desenhos diferentes, PNGs RGBA 384 × 384, pés alinhados, atlas 8×3 e tempos em JSON (4,22 s). Prévia GIF/WebP e player HTML com inspeção por quadro em `Docs/ArtPreviews/BananaPossessora_Transformacao_v02/`. Fontes, prompts, hashes e ferramenta de reconstrução preservados. As versões de seis quadros e as poses do cipó continuam salvas. Sem integração ao combate ou Animator nesta entrega.

## Decisão atual — v09 e próxima forma

Poses preservadas a pedido do usuário: `SourceArt/BananaPossessora_Poses_v01/` reúne cópias verificadas por hash dos seis quadros anteriores e a nova pose estática `cipo/boss_semiaberto_cipo.png`, do boss semiaberto pendurado em um cipó com uma mão. Geração integrada image_gen, PNG transparente, prompt e manifesto salvos; pacote ZIP ao lado da pasta. Não substitui os masters modulares ou os quadros originais.

Em 19/10/2026, o usuário aprovou a nova montagem da conversa e pediu que a pixel art fosse modular e compatível com os modelos do projeto. A v09 sucede a v08 como master atual: `SourceArt/BananaPossessora_Fase1_Master_v09_Modular/`. Seis módulos RGBA, casca fechada amarela e detalhes verdes, braços/pernas com raízes independentes, rabo atrás do quadril, master 256 × 272 em PNG/ORA/SVG e layout com pivôs. Os aros pertencem apenas à casca; os centros transparentes mostram os membros sobrepostos por trás. Exportações Unity estáticas em `Assets/Art/Boss/BananaPossessora/Fase1/`, Point/Full Rect/PPU 100, sem mipmaps/compressão. Versões antigas preservadas.

Definição direta do usuário para a próxima forma: “o boss vai ficar mais forte, gradualmente como uma animação de transformação, e ele irá rasgar a casca na metade para ficar semi aberto e vai aparecer o macaco parcialmente, muito brabo com os olhos verdes também.”

`SourceArt/BananaPossessora_Transformacao_v01/` contém seis quadros-chave transparentes 256 × 256, spritesheet horizontal e tempos em JSON (2,19 s). A casca abre no centro, cabeça e peito do macaco aparecem; a parte inferior continua envolvendo o quadril. A prévia HTML permite separar os módulos da primeira forma, conferir os ombros e reproduzir a transformação. HTML, montagem ampliada, peças e GIF ficam em `Docs/ArtPreviews/BananaPossessora_v09/`.

Verificadas transparência, limites, pivôs e hashes dos seis módulos; GIF com seis frames e tempos corretos, JavaScript da prévia com sintaxe válida. Revisão visual das montagens e da faixa de transformação realizada. A abertura da prévia HTML no navegador integrado foi bloqueada pela política de URLs locais; não houve validação visual dos controles no navegador. Sem validação dentro do Editor Unity. Sem rig, Animator, ataques ou integração em cenas. A transformação ainda requer intermediários, continuidade exata do primeiro quadro com o master modular e separação da forma semiaberta em peças.

As decisões abaixo são histórico das revisões anteriores.

Definição do usuário, 19/10/2026: a banana foi infectada pelos alienígenas, o macaco comeu a banana e agora ela o possui. A banana é a vilã; Croc precisa derrotá-la para libertar o macaco.

Primeira fase, conforme a definição mais recente do usuário: macaco menos forte, com membros mais esguios e sem a hipertrofia da v03; banana de casca fechada, grande o suficiente para prender o corpo e a cabeça do macaco. Apenas os braços, as pernas e o rabo ficam para fora. Rosto, olho, focinho e dedos internos não aparecem. O rabo sai da região posterior do quadril, com a raiz alongada escondida sob a casca para melhorar o encaixe. A expressão maligna pertence à banana; a casca é AMARELA e apenas os olhos e detalhes antes roxos são VERDES, como a infecção do Gusini. A referência mais recente do usuário orienta a silhueta mais esguia, a curvatura e o sorriso. Não tornar a casca verde.

Referências de estilo do jogo: somente Goosini e B. Crocodilo. Outros estudos de banana/macaco não devem orientar revisões.

Master da primeira fase: `SourceArt/BananaPossessora_Fase1_Master_v08_BaseRetomada/`. Prévia: `Docs/ArtPreviews/BananaPossessora_Fase1_Master_v08_BaseRetomada/master.png`. Seis módulos independentes em PNG e master em ORA/SVG. A v03 musculosa foi preservada como alternativa; sua fase ainda não foi definida. A v02 com o macaco espiando fica como histórico.

Esta etapa entrega arte estática para design. Comportamentos, ataques, transformação, resgate e integração em cenas não foram implementados.


Revisão v04: perna distante atrás da casca na ordem de camadas, com quadril oculto; braços novos pendentes, cotovelos suaves e mãos relaxadas, conforme a referência do usuário. Corpo, pernas e rabo preservados sem redesenho; casca amarela com detalhes verdes.


Revisão atual v07: encaixes dos braços voltados para trás; conservar as bordas externas e remover apenas as bordas internas voltadas à barriga. Sem aros completos. Casca cobre as raízes dos membros. Pernas cerca de 18% mais compridas, principalmente nas canelas, mantendo os quadris e larguras. Seis módulos; camadas e sobreposição conferidas.


DECISÃO MAIS RECENTE — v08: o usuário retomou explicitamente a imagem da v03_Amarela como a base correta, descartando as alterações de design das v04–v07. Fazer somente braços e pernas mais compridos e perna de dentro atrás da ponta. Não redesenhar banana, rosto, rabo ou encaixes da referência. Master v08 usa os mesmos PNGs-fonte da v03 com transformação de comprimento e ajuste de ordem de camadas. Banana e rabo exportados idênticos à base por SHA-256.

