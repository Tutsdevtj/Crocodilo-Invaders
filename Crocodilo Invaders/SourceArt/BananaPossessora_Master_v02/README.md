# Banana possessora — master v02

A banana é a vilã infectada pelos alienígenas. O macaco comeu a banana e ficou aprisionado dentro dela. Croc precisa derrotar a banana para libertá-lo.

Direção: banana maior e mais larga, casca como prisão orgânica, expressão alienígena maligna na casca, sinais visíveis do macaco assustado por dentro. Braços, pernas e rabo continuam pertencendo ao macaco. Bordas da casca prendem os membros nas aberturas.

Referências do estilo do jogo: exclusivamente Goosini e B. Crocodilo. A v01 desta conversa foi usada como alvo da revisão; outros estudos banana/macaco não foram usados.

`concept.png` registra a proposta visual completa. `master.png` é a montagem das peças separadas, conferida visualmente. Arte estática de design, sem integração de gameplay ou animação.

- `master.ora`: master OpenRaster com seis camadas nomeadas (Krita/GIMP).
- `master.svg`: seis objetos editáveis com imagens incorporadas.
- `parts/`: corpo/casca, dois braços, duas pernas e rabo em PNGs transparentes.
- `layout.json`: posições, escalas e ordem de sobreposição; membros encaixados nas aberturas da casca.
- `atlas.png`: atlas-fonte da decomposição. O corpo ultrapassa a célula superior; o script usa recortes personalizados para preservar a ponta e não contaminar as pernas.
- `package.ps1`: recorta e monta as peças; `qa_report.json`: validação do pacote.

O rosto do macaco preso, a expressão da banana e os sinais de infecção fazem parte da camada do corpo. Cada membro é uma peça inteira; articulações internas ainda não são subcamadas.

Ferramenta: image_gen integrada. As instruções de geração estão em `prompts.txt`. A arte-fonte é preservada; recortes e montagem são empacotados separadamente.
