# Céu de Batalha

O prefab `Assets/Prefabs/Environment/BattleSky.prefab` substitui Background,
SkyEnvironment/AnimatedSun e ScrollingGround nas cenas PlayerMovementTest,
StageOneWaves e GusiniBossTest. O menu principal conserva seu cenário.

Foram importados os **46 PNGs nativos** do pacote Background_CeuDeBatalha.zip,
em `Assets/Art/Environment/BattleSky`: oito camadas, 25 FX da nave-mãe e 13
sprites da batalha distante. Importação PPU 50, Filter Point, Compression None,
sem mipmaps e Full Rect. A fumaça usa o pivot (0,18; 0,06) do pacote. As cópias
x2, GIFs, previews e ferramentas não entram nos assets de runtime. O layout e
as notas originais estão em `SourceArt/BattleSky` como referência da arte.

`BackgroundFit` enquadra o conjunto com escala uniforme e cobertura da câmera.
Agora o encaixe acontece antes do Awake/Update das camadas de rolagem, além
da atualização final, para que elas calculem as larguras na escala correta.
As posições verticais vêm do layout: camadas altas pelo topo; mar, montanhas
e ilhas alinhados à base da composição.

| Camada | Velocidade (unidades/s) | Sorting order |
| --- | ---: | ---: |
| Céu opaco | 0 | -100 |
| Nave-mãe | 0, fixa ao enquadramento | -96 |
| Nuvens distantes | 0,25 | -92 |
| Batalha distante estática | 0,4 | -88 |
| Nuvens médias | 0,9 | -84 |
| Horizonte distante | 1,2 | -76 |
| Nuvens próximas | 2,2 | -72 |
| Ilhas e base militar | 4 | -68 |

As seis camadas móveis reutilizam `ScrollingLayer`, com dois segmentos de
16 unidades, sem espelhamento e sem espaçamento. `BackgroundFit` mantém a
nave-mãe quase imóvel como referência de distância; não é um inimigo.

`MothershipBackgroundFX` coordena três fumaças com defasagem, explosões nos
pontos de avaria a cada 1,2–2,5 s, clarões breves, falhas alien a cada 0,8–1,5 s
e 2–3 detritos por explosão. Os seis detritos são reutilizados e desaparecem
em 1,2 s. Os efeitos acompanham a nave, atrás das nuvens distantes, com alpha
reduzido. O sorteio visual usa System.Random, separado do gameplay.

`DistantAirBattle` reutiliza quatro naves, dois pequenos traçantes e dois
efeitos. As rotas ficam na parte alta do céu, com cores atenuadas e ordem -87.
Todo o cenário é visual: não possui colliders, dano ou áudio. Movimentos,
animações e timers usam o tempo do jogo e congelam na pausa. Não há objetos
persistentes entre cenas nem criação contínua de objetos para os FX.

As duas HUDs, os colliders e os corpos físicos da arena permaneceram iguais.
Nenhum limite de movimento, câmera de gameplay, inimigo ou configuração de
combate foi alterado para acomodar o cenário.

Menus no Editor:

- `Crocodilo → Environment → Install Battle Sky`: remonta o prefab e instala
  nas três cenas acima. Substitui apenas os objetos antigos de cenário.
- `Rebuild Battle Sky prefab`: remonta apenas o prefab usando o layout fonte.
- `Validate Battle Sky`: confere camadas, importação, ordem, ausência de
  componentes físicos/áudio e referências das cenas.

Validação na Unity 6000.6.4f1: **59 verificações em Play Mode** passaram. Foram
conferidos movimento, velocidades, continuidade das seis faixas, escala,
fumaça/FX/batalha distante, HUDs, pausa, 100 segundos simulados sem crescimento
de objetos e troca real de cena sem decoração órfã. Capturas da partida foram
inspecionadas em 1200×720, 1920×1080 e 800×600. Os 46 PNGs foram comparados byte
a byte com a fonte; o céu base é totalmente opaco. A validação de build e as
validações das HUDs também passaram. As capturas/testes locais ficam em Logs.
