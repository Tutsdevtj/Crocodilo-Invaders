# Banana possessora — primeira fase, bordas externas v06

Correção explícita do usuário: retirar APENAS as bordas dos encaixes voltadas para o interior da banana. As bordas de fora devem continuar aparecendo.

As saídas dos membros conservam pequenos lábios externos da casca, em arcos abertos que acompanham a silhueta. Não há aros circulares completos nem metades internas desenhadas sobre a barriga. A casca cobre as raízes dos membros, com sobreposição para evitar peças soltas.

Corpo amarelo fechado, olhos e detalhes alienígenas verdes; sem macaco espiando. Braços relaxados da v04, perna distante atrás da casca e rabo atrás do quadril. Os cinco PNGs dos membros são preservados diretamente da v04.

- `master.png`: montagem transparente conferida visualmente.
- `master.ora`: seis camadas nomeadas, para Krita/GIMP.
- `master.svg`: objetos individuais com imagens incorporadas.
- `parts/`: corpo e cinco membros em PNGs separados.
- `corpo_fonte.png`: edição da casca com image_gen integrada.
- `parts_base/`: PNGs anteriores para montagem reproduzível.
- `layout.json`: posições, escalas, fontes e ordem.
- `package.ps1`: recorte da casca e montagem.
- `prompts.txt`: instruções de geração.
- `qa_report.json`: validação das camadas e da sobreposição das junções.

Master estático para design; cada membro é um módulo inteiro. Sem animação ou integração de gameplay. A v05 sem nenhuma borda foi substituída por esta correção.
