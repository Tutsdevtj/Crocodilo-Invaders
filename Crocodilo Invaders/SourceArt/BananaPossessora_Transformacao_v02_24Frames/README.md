# Banana possessora — transformação para a fase 2, v02

Pedido de 20/10/2026: animação quadro a quadro, com mais frames e detalhes. O boss aumenta gradualmente a força e a raiva, agarra e arranca a ponta superior da banana, joga esse pedaço fora e rasga a casca pelo meio.

São **24 desenhos diferentes**, em vez dos seis quadros-chave da v01. Duração normal **4,22 s**, incluindo a sustentação final de 900 ms. Os desenhos foram gerados com image_gen integrado em quatro segmentos contínuos; cada segmento usa o último quadro anterior como referência. Não há interpolação, morphing ou repetição de um desenho para simular novos frames.

## Sequência

- 01–06: dedos se fecham, postura tensiona, músculos aumentam e expressão fica mais agressiva.
- 07–12: mão alcança a ponta, agarra o talo, puxa e separa a parte superior da casca; testa e olhos verdes do macaco aparecem.
- 13–18: arremessa a ponta, retorna as mãos ao peito, segura os lados da casca e inicia um rasgo vertical.
- 19–24: puxa a casca para os lados, expõe o peito, ruge, solta as abas e assume a pose da fase 2.

A ponta superior e o talo **não reaparecem** depois do arremesso. A forma final mantém olhos verdes, corpo mais forte, casca inferior amarela presa ao quadril e abas laterais rasgadas. As poses antigas e a pose do cipó foram preservadas.

## Arquivos

- `frames/`: 24 PNGs RGBA 384 × 384, alpha binário, pés alinhados, pivô comum (192,352) a partir do topo esquerdo.
- `animation.json`: ordem, nomes das ações, tempos, pivô, informações da spritesheet e PPU sugerido 100. Coordenadas do pivô normalizado da Unity: x=0,5, y=1−352/384.
- `spritesheet_8x3.png`: atlas de 3072 × 1152, células 384 × 384; leitura por linhas, da esquerda para a direita. Preferível à faixa horizontal para importação.
- `spritesheet_horizontal.png`: faixa de inspeção de 9216 × 384. Não usar um limite de textura baixo que a reduza ao importar.
- `sources/`: quatro folhas originais da geração.
- `frames_source/`: recortes em resolução de origem, preservados antes do alinhamento e exportação.
- `prompts.txt`: os quatro prompts completos, com identidade e continuidade solicitadas.
- `qa_report.json`: hashes dos 24 frames, unicidade, deslocamentos, quantidade de quadros e tempos do GIF.

Prévias em `Docs/ArtPreviews/BananaPossessora_Transformacao_v02/`: GIF em repetição, WebP sem perdas, folha de contato e HTML independente com play/pausa, quadro anterior/próximo, controle por frame, velocidades 0,5×/1×/1,5× e repetição opcional. O HTML conserva a última pose quando termina; a repetição do GIF é apenas para revisão.

## Reconstrução e verificação

`Tools/Art/split_banana_transformacao_v02.ps1` salva os segmentos e os recortes; `Tools/Art/package_banana_transformacao_v02.cjs` alinha a base dos pés em um canvas comum, exporta por vizinho mais próximo, gera atlas e prévias. Pequenos fragmentos desconectados que tocam as bordas e pertencem a células vizinhas são removidos do export, preservando o personagem, o pedaço arremessado e detritos internos. Fontes intactas permanecem disponíveis.

Verificados: 24 hashes diferentes, dimensões uniformes, RGBA, alpha binário, duração e ordem dos 24 frames do GIF, sintaxe do JavaScript da prévia e inspeção visual da folha de contato e dos quadros da ponta arrancada/arremessada. Não houve validação do HTML em navegador ou importação no Editor Unity. Como a arte foi gerada por segmentos, a consistência fina do traço ainda pode receber polimento manual.

Entrega de animação em sprites, sem alterações em combate, Animator, prefabs ou cenas. Não substitui o boss ativo do jogo.
