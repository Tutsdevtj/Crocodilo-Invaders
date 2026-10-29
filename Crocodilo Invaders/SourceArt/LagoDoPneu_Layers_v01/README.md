# Lago do Pneu — cenário em camadas v01
Gerado com a ferramenta integrada image_gen. Oito gerações independentes. Fontes originais preservadas, sem redimensionamento; cada PNG tem 1672 × 941 pixels. Céu opaco; demais camadas RGBA, canto superior esquerdo transparente confirmado. A transparência das bordas ainda contém pixels parciais.
Abra preview.html para inspecionar a composição, alternar camadas e experimentar a rolagem. preview.png contém a composição estática verificada visualmente.
## Ordem de trás para frente e ajustes
- 01_ceu.png: Céu; deslocamento Y 0 px no eixo da imagem (positivo para baixo); fator demonstrativo de parallax 0.
- 02_nuvens_distantes.png: Nuvens distantes; deslocamento Y 0 px no eixo da imagem (positivo para baixo); fator demonstrativo de parallax 0.06.
- 03_margem_distante.png: Margem distante; deslocamento Y -12 px no eixo da imagem (positivo para baixo); fator demonstrativo de parallax 0.12.
- 04_vegetacao_intermediaria.png: Vegetação intermediária; deslocamento Y -23 px no eixo da imagem (positivo para baixo); fator demonstrativo de parallax 0.23.
- 05_margem_proxima.png: Margem próxima; deslocamento Y -31 px no eixo da imagem (positivo para baixo); fator demonstrativo de parallax 0.37.
- 06_agua_base.png: Água base; deslocamento Y 42 px no eixo da imagem (positivo para baixo); fator demonstrativo de parallax 0.45.
- 07_superficie_agua.png: Superfície da água; deslocamento Y 11 px no eixo da imagem (positivo para baixo); fator demonstrativo de parallax 0.52.
- 08_primeiro_plano.png: Primeiro plano; deslocamento Y 0 px no eixo da imagem (positivo para baixo); fator demonstrativo de parallax 0.7.
O canvas compartilhado usa origem superior esquerda. Linha d'água da composição próxima de Y=678. Em Unity, usando pivô central e mesmo PPU para estas oito imagens, offset local Y = -offsetY / PPU; largura e escala devem ser iguais. Importar como Sprite, Filter Mode Point, sem mipmaps e sem compressão, após aprovação e acabamento.
## Referências usadas
Bombardiro Idle e Gusini Phase 1 atuais do projeto; captura salva Docs/ArtPreviews/MacaquiniFighter_v22/combo_1.png para escala e câmera; referência do lago enviada pelo usuário preservada em SourceArt/SapoDoPneu_Master_v01/referencia-lago.png. Não foi criada uma captura nova do Editor.
## Escopo e revisão
Pacote de arte e prévia, ainda fora de Assets e sem alteração da campanha. Sem personagens, HUD, projéteis ou colisões. Superfície separada em pose estática; ciclo de seis frames ainda não produzido. Nenúfares e outros objetos de gameplay serão assets separados.
A geração não garantiu emendas horizontais perfeitas. A prévia permite inspecionar a repetição; acertar continuidade das bordas, regularizar grade/paleta e testar parallax na Unity antes de considerar os tiles prontos para produção. Os offsets resolvem o encaixe visual inicial sem modificar os PNGs.
Prompts completos por camada nos arquivos *-prompt.txt. manifest.json registra as fontes, solicitações e offsets; qa.json registra dimensões e amostras de alfa.
