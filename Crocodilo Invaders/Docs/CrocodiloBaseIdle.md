# Crocodilo da base — idle 32 × 32

Registro histórico: os assets deste estudo foram retirados de Assets em
18/10/2026, junto da implementação descartada da base. Não é o personagem atual.

Interpretação de corpo inteiro, em três quartos voltado para a direita,
baseada no retrato do crocodilo fornecido pelo usuário.
Revisão 3: cabeça larga e verde escura, focinho pesado, olhos âmbar estreitos,
mandíbula cor de areia e capacete oliva arredondado, seguindo a identidade
do retrato. O uniforme da base tem camisa de manga curta com gola e bolsos,
cinto, calça oliva e coturnos. Canvas, pivôs e durações foram preservados.
Sem arma, colete ou camuflagem miúda. O capacete foi mantido por fazer parte
da referência do personagem, substituindo o gorro da revisão anterior.

## Pesquisa e referências

Foram estudadas fontes dos próprios artistas, com foco em leitura em baixa
resolução, construção de formas, controle de pixels e animação de idle.
As referências orientam a técnica; seus personagens não foram copiados nem
incluídos no projeto.

A referência de identidade é `SourceArt/CrocodiloBase/portrait-reference.png`,
uma cópia do retrato enviado pelo usuário. Ela orienta anatomia, cores,
expressão e capacete; os estudos abaixo orientam a animação e o desenho.

1. **Raymond Schlitter / Slynyrd — [Pixelblog 8: Intro to Animation](https://www.slynyrd.com/blog/2018/8/19/pixelblog-8-intro-to-animation).**
   Referência visual principal: a prancha animada “13 — Idle Cycles”, que compara
   ciclos com 2, 4, 6 e 8 frames. Foi inspecionada visualmente para estudar
   movimentos pequenos e formas que permanecem legíveis no loop. O artigo
   recomenda começar com sprites pequenos e poucas cores, cuidar dos grupos
   de pixels em todos os frames e ajustar a duração das poses. Oito frames por
   si só não garantem uma animação melhor.
2. **Slynyrd — [Pixelblog 52: Idle Fighting Stance](https://www.slynyrd.com/blog/2024/9/26/pixelblog-52-idle-fighting-stance).**
   Estudo complementar de animação: movimentos do tronco coordenam cabeça e
   braços; partes diferentes podem acompanhar o movimento em momentos
   diferentes. Os exemplos são maiores e estão em postura de luta. Aqui foi
   adotado apenas esse princípio de acompanhamento, com amplitude menor e
   sem a postura de combate ou os calcanhares levantados.
3. **Pedro Medeiros / Saint11 — [My Thoughts on Very Low Resolution](https://saint11.art/blog/thoughts_on_low_resolution/).**
   Priorizar os elementos que comunicam o personagem; excluir informação
   secundária; evitar pixels arbitrários, especialmente no rosto; limitar
   cores e usar iluminação simples. Neste desenho, focinho, olho, cauda e
   roupa fazem o trabalho de identificação. Não há escamas pontilhadas,
   costuras repetidas ou ornamentos que disputem esses poucos pixels.
4. **Derek Yu — [Pixel Art Tutorial: Basics](https://derekyu.com/makegames/pixelart.html).**
   A seção específica de 32 × 32 enfatiza a importância de cada pixel e das
   massas de cor para definir um personagem pequeno. Foi usada uma grade real
   de 32 × 32 desde o início; a arte não foi desenhada em resolução grande e
   depois reduzida.
5. **Derek Yu — [Pixel Art: Common Mistakes](https://derekyu.com/makegames/pixelart2.html).**
   Reduzir cores parecidas, simplificar formas e sombrear volumes em vez de
   criar anéis de sombra ao redor do contorno. A camiseta, a pele e a calça
   usam massas distintas de cor. A luz principal vem de cima e da esquerda;
   não há gradientes, dithering ou suavização nas bordas.

## Decisões de desenho

- **Silhueta:** focinho comprido e cauda curva distinguem o crocodilo. Cabeça,
  mãos e pés ficam inteiros dentro do canvas, com margem abaixo das botas.
- **Postura:** braços soltos junto ao corpo e pés separados, apoiados no chão.
- **Roupa:** gola, bolsos do peito, mangas dobradas, cinto com fivela e
  capacete arredondado. A mandíbula clara e os planos de sombra separam a
  pele verde escura da roupa oliva, seguindo a paleta do retrato.
  Os detalhes seguem a posição do tronco, sem mudar
  arbitrariamente de um frame para o seguinte.
- **Contorno:** azul-cinza escuro; separação interna apenas quando ajuda a ler
  mandíbula, braços, pernas e roupa.
- **Respiração:** ombros sobem primeiro; cabeça e mãos acompanham. O peito
  expande um pixel, depois retorna. Calça, botas e cauda ficam fixas.
- **Loop:** 8 frames com 7 poses diferentes. O primeiro e o último repetem a
  posição de descanso intencionalmente, criando uma pausa sem um salto na
  passagem de um ciclo para o seguinte.

## Paleta

16 cores opacas e transparência. Nenhum pixel possui alpha intermediário.

| Uso | Cor |
| --- | --- |
| Contorno | `#192A25` |
| Pele, sombra | `#223328` |
| Pele, base | `#3B4D31` |
| Pele, luz | `#697D44` |
| Dentes | `#E2D09B` |
| Farda, sombra | `#393E2B` |
| Camisa e capacete, base | `#5D6041` |
| Farda, luz | `#81805A` |
| Calça, base | `#6C6949` |
| Calça, sombra | `#4B4B34` |
| Botas, base | `#38372D` |
| Botas, luz | `#595041` |
| Íris e metal | `#C6A04C` |
| Tecido, luz | `#948967` |
| Mandíbula | `#B3A272` |
| Pele, sombra intermediária | `#344530` |

## Arquivos

- `SourceArt/CrocodiloBase/crocodilo-base-idle.aseprite`: fonte editável, indexed,
  32 × 32, cinco camadas, tag `Idle_Respirando`.
- `Assets/Art/Characters/CrocodiloBase/crocodilo-base-idle.png`: spritesheet RGBA
  transparente, 256 × 32, oito células de 32 × 32, da esquerda para a direita.
- `Assets/Art/Characters/CrocodiloBase/crocodilo-base-idle.json`: retângulos e
  durações dos frames no formato de metadados de spritesheet do Aseprite.
- `Assets/Art/Characters/CrocodiloBase/crocodilo-base-idle-pose.png`: pose inicial
  transparente, 32 × 32.
- `Docs/ArtPreviews/crocodilo-base-idle-x8.gif`: prévia animada ampliada 8 vezes,
  sem interpolação, sobre fundo claro apenas para apresentação.
- `Docs/ArtPreviews/crocodilo-base-idle-v3-x8.gif`: a mesma revisão atual, com
  nome distinto para evitar que a prévia anterior permaneça em cache.
- `Docs/ArtPreviews/crocodilo-base-idle-frames-x8.png`: prancha ampliada dos frames.
- `Docs/ArtPreviews/crocodilo-base-idle-validation.txt`: conferência dos pixels.
- `Tools/Art/create_crocodilo_base_idle.lua`: desenho e exportações reproduzíveis
  pela API Lua do Aseprite instalado. Nenhum MCP foi utilizado.

## Animação e importação

As durações são **300, 200, 200, 360, 200, 200, 200, 300 ms**. Ciclo total:
**1,96 segundo**. O timing está preservado na fonte, no JSON e na prévia GIF.

Na Unity, a spritesheet usa Sprite (2D and UI), Multiple, Point, sem mipmaps
e sem compressão. Cada corte tem 32 × 32. O pivot fica na linha do chão,
em `(16,31)` no espaço do Aseprite, equivalente a `(0.5,0.03125)` normalizado
na Unity. Pixels Per Unit: 32. A pose avulsa usa o mesmo pivot e importação.

Se os frames forem colocados num AnimationClip com taxa fixa, reproduzir
as durações com keyframes nos tempos **0, 0.30, 0.50, 0.70, 1.06, 1.26,
1.46 e 1.66 segundo**, fechando o ciclo em 1.96. Reproduzir oito imagens
com uma duração uniforme altera o ritmo desenhado aqui.

O asset foi criado separadamente. Nenhum prefab, controlador, cena ou
animação existente do jogador foi substituído.

## Verificação

A exportação confere tamanho, oito frames, no máximo 16 cores opacas,
transparência binária, pixels da base fixos e igualdade entre as poses
inicial e final. As prévias foram inspecionadas em ampliação por pixels.

API oficial utilizada: [Sprite](https://www.aseprite.org/api/sprite),
[Image](https://www.aseprite.org/api/image) e
[Frame.duration](https://www.aseprite.org/api/frame#frameduration).
