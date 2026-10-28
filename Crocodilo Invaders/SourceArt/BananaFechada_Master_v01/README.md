# Boss — banana fechada, forma 1

Novo estudo independente. Referências de identidade visual do jogo: exclusivamente Goosini e B. Crocodilo. As duas imagens fornecidas pelo usuário orientam a banana fechada e a anatomia dos membros do macaco. Nenhum outro estudo de boss banana/macaco foi usado como referência de geração.

Corpo: banana com casca completamente fechada, sem cabeça ou rosto acrescentado. Braços, pernas e rabo marrons, mãos e pés claros. Vista lateral em três quartos, orientada para a esquerda.

## Arquivos

- `atlas.png`: arte-fonte gerada, com as seis peças isoladas.
- `parts/`: corpo, braço distante, braço próximo, perna distante, perna próxima e rabo em PNGs transparentes individuais.
- `master.ora`: master OpenRaster em seis camadas independentes, com nomes e ordem de sobreposição. Pode ser aberto no Krita ou GIMP.
- `master.svg`: montagem editável com os mesmos seis objetos nomeados e imagens incorporadas.
- `master.png`: montagem estática transparente para revisão.
- `layout.json`: posições, tamanhos, ordem e origem de cada peça no atlas.
- `prompts.txt`: instruções utilizadas na geração com a ferramenta integrada image_gen.

Cada membro é um módulo completo; cotovelos, punhos, joelhos e dedos ainda não são subcamadas. Este é um master estático para design, sem animações ou integração de gameplay. Os sprites originais do jogo não foram substituídos.

O empacotamento apenas recorta as células e monta as peças. O atlas original é preservado para futuras revisões.
