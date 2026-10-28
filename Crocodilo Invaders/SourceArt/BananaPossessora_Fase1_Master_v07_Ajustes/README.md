# Banana possessora — primeira fase, ajustes v07

Revisão pelas marcações do usuário:

1. Bordas externas dos ombros voltadas para trás, acompanhando o encaixe posterior dos braços. Sem ganchos projetados para a frente, aros completos ou metades internas na superfície frontal.
2. Pernas um pouco mais compridas, principalmente nas canelas. Na montagem, perna distante passa de 144 para 170 px e próxima de 149 para 176 px, conservando as larguras de 132/134 px e a posição do quadril.

Casca AMARELA com olhos e detalhes VERDES. Corpo fechado, sem macaco espiando, braços relaxados e anatomia normal da primeira fase. Rabo e raízes dos membros ficam atrás da casca; a perna distante continua atrás.

- `master.png`: montagem transparente conferida visualmente.
- `master.ora`: seis camadas nomeadas, para Krita/GIMP.
- `master.svg`: objetos com imagens incorporadas.
- `parts/`: corpo e cinco membros em PNGs separados.
- `corpo_fonte.png`: edição da casca com image_gen integrada.
- `pernas_fonte.png`: folha das pernas mais compridas, gerada com image_gen.
- `parts_base/`: módulos usados na montagem; braços e rabo preservados da v04, pernas novas extraídas da folha.
- `layout.json`: fontes, recortes, posições, escalas e ordem. `width` e `height` são os tamanhos finais da montagem.
- `package.ps1`: recorte da casca e montagem das peças-base.
- `prompts.txt`: instruções de geração.
- `qa_report.json`: validação das camadas e sobreposição das junções.

Arte estática para design; cada membro é um módulo inteiro. Sem animação ou integração de gameplay.
