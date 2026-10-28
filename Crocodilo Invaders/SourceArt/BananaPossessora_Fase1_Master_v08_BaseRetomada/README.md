# Primeira fase — referência retomada, v08

O usuário retomou explicitamente a imagem fornecida em `referencia_usuario.png` como base correta e pediu somente:

1. Braços mais compridos.
2. Pernas mais compridas.
3. Perna de dentro atrás da ponta da banana.

Foi retomado o master v03_Amarela que corresponde a esse desenho. Os PNGs-fonte originais foram preservados integralmente. Somente os tamanhos verticais dos quatro membros foram ajustados no master nativo e a perna distante foi colocada atrás da banana na ordem de camadas. Larguras, posições das raízes, poses, cores, desenho da casca, encaixes e rabo vêm da base escolhida.

Braços: 136→163 e 168→202 px. Pernas: 144→170 e 149→176 px. A banana e o rabo exportados são idênticos aos da base, verificados por SHA-256. Nenhuma geração de imagem ou redesenho nesta revisão: apenas transformações dos módulos existentes.

- `master.png`: montagem transparente.
- `master.ora`: seis camadas nomeadas, para Krita/GIMP.
- `master.svg`: seis objetos editáveis, com imagens incorporadas.
- `parts/`: seis PNGs exportados com as dimensões finais.
- `parts_base/`: seis PNGs-fonte originais, sem alteração.
- `layout.json`: posições, dimensões e ordem final.
- `rebuild.ps1`: reconstrói o master e exporta os módulos.
- `qa_report.json`: verificações dos arquivos e da sobreposição.
- `referencia_usuario.png`: imagem que o usuário escolheu retomar.

As versões intermediárias v04–v07 não orientam esta revisão. Arte estática, sem animação ou integração no gameplay.
