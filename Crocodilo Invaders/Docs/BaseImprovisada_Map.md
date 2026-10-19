# BaseImprovisada — mapa v01 (cenário)

Primeira versão visual explorável da base, montada em 18/10/2026 a partir de
`BaseImprovisada_Kit_Implementacao_v01.zip`. A montagem inicial foi só cenário, com
marcador de escala e câmera fixa. **Estado atual:** o pedido seguinte adicionou Croc
simplificado, Idle aprovado, movimento horizontal e câmera específica do HUB, sem Run.
Ver `BaseImprovisadaCamera.md`; o rig antigo descartado continua fora do projeto.

**Revisão de montagem:** furos visuais, encaixes do terreno e escala pequena do Croc foram
corrigidos em seguida. O estado atual está abaixo; os resultados v01 no fim são históricos.

Cena: `Assets/Scenes/BaseImprovisada.unity`, mesmo nome técnico, no build e destino de
`Chapter1.asset.nextScene`. Ferramentas: `Crocodilo/Base/Install BaseImprovisada map`
(mutação idempotente que reconstrói os grupos gerados), `Validate BaseImprovisada map` e
`Validate BaseImprovisada map in Play Mode`.

## Escala e câmera

- **80 px do blueprint = 1 unidade.** O blueprint propõe uma vista de 1280 px, que equivale a
  16 unidades na câmera atual (ortográfica 4,5, 16:9). O mapa de 8400 px vira **105 unidades**,
  cerca de 6,5 telas. Conversão: `x_mundo = x_design / 80 − 6` (o spawn em 430 px fica em x ≈ −0,6).
- **Croc simplificado:** PNGs originais 48×72/PPU 80 preservados, escala visual inteira
  **2×**, canvas de **1,8 unidade** (20% da vista de referência). Cápsula 0,88×1,7,
  CameraTarget Y = 0,85. A raiz física continua em escala 1.
- **Câmera:** mesmo objeto, enquadramento de referência 4,5 e **Y de 0,9 para 2,4**, para
  mostrar construções e céu. A etapa seguinte adicionou HubCameraController2D e Pixel Perfect;
  a lente adapta a escala inteira à janela (detalhes em `BaseImprovisadaCamera.md`).
- **Limites:** `Collisions/MapExtents` traz `PlayableLeft` (−5,3), `PlayableRight` (98,6),
  `VisualLeft` (−13,5) e `VisualRight` (106,5). O novo controlador usa os limites visuais,
  recuando meia viewport, independentemente das paredes físicas. Altura visual −2,1…6,9.

## Importação e adaptações

Todos os PNGs usam Point, sem compressão, sem mipmaps, alpha como transparência e Clamp.

| Grupo | Origem | PPU / tamanho | Adaptação |
| --- | --- | --- | --- |
| Terreno (82) | `Art/Environment/BaseImprovisada/Terrain` | 80 | Fontes preservadas. Rota atual usa recortes nativos de Sprite em `TerrainSlices`, com faixas opacas verificadas; importers anteriores permanecem para referências existentes |
| Suprimentos (10) | `.../Supplies` | 80 | **Cópias reduzidas** (moda por bloco, sem blur) de ~1230 px para 36–120 px de altura. Originais em `SourceArt/BaseImprovisada/Originals/02_suprimentos` |
| Ambientação (10) | `.../Ambient` | PPU calculado pela altura/largura alvo | Pivô no centro inferior do recorte opaco (os PNGs têm margem transparente) |
| Construções (5) | `.../Buildings` | Largura alvo: hangar 10,5 · oficina 7 · enfermaria 9 · comando 9,5 · rádio 8,5 | Arte original intacta, mesh Tight, base apoiada em y = −0,06 |
| Panorama | `.../Background/03_BG2_montanhas_referencia_RGB.png` | 80 (27 × 9 unidades), até 4096 | Único fundo opaco |
| Halo | `.../Lighting/base_halo_steps.png` (gerado) | 32 | Anéis em degraus, sem gradiente suave |
| Referência | `.../_Reference/croc_hub_master.png` | 80, pivô na sola | Só marcador de escala (tag EditorOnly, fora do build) |

Não usados no runtime: BG0, BG1 e BG3 opacos e a folha BG4 (concept com texto), além de
concepts e blueprint. Eles continuam no ZIP. O manifest, o layout JSON, o README e a Art Bible
estão em `SourceArt/BaseImprovisada`.

## Fundo e parallax (`ParallaxLayer2D`)

`scrollFactor` é a velocidade aparente em relação ao mundo: 0 acompanha a câmera, 1 é o mundo
e acima de 1 é primeiro plano. A posição montada vale quando a câmera está em
`referenceCameraX`. O componente roda em LateUpdate, sem alocação, com repetição opcional (só
para arte sem emenda; hoje desligada) e snap de pixel opcional.

| Camada | Arte | Fator | Sorting layer |
| --- | --- | ---: | --- |
| BG0_Panorama | BG2 opaco (céu roxo, sol, montanhas, floresta) | 0,03 | BackgroundSky |
| BG1_Clouds | reservado: não há nuvens RGBA separadas | — | BackgroundClouds |
| BG2_Mountains | as montanhas estão dentro do panorama | — | BackgroundMountains |
| BG3_Ruins | torre, postes, posto, plataforma, treliça, comando e rádio miniaturizados, tingidos de violeta frio | 0,30 | BackgroundRuins |
| BG4_Near | cercas, treliças, árvores e postes em silhueta violeta | 0,60 | BackgroundNear |
| Foreground | entulho escuro no rodapé (sacos, barris, rochas, grama) | 1,25 | WorldFront |

Os quatro fundos opacos não foram empilhados. Também não separei camadas por limiar de cor:
as rochas tingidas usadas numa primeira tentativa viraram manchas pretas e foram trocadas por
silhuetas com recortes reais. Quando houver nuvens ou fumaça RGBA, entram como filhas de
`BG1_Clouds` com o próprio `ParallaxLayer2D` (sugestão de 0,08–0,15).

Sorting layers adicionadas ao projeto, de trás para a frente: BackgroundSky, BackgroundClouds,
BackgroundMountains, BackgroundRuins, BackgroundNear, WorldBack, WorldGround, WorldCharacters,
WorldFront, AmbientEffects. Default continua a primeira, então as outras cenas não mudam.
O personagem futuro deve usar **WorldCharacters**: fica à frente de construções e chão e atrás
apenas do primeiro plano (tufos de grama e entulho do rodapé).

## Terreno e colisões

- `Environment/Terrain/Surface`: cortes que excluem bordas quebradas. Três linhas
  consecutivas opacas identificam o apoio, alinhado a y = 0. Grade de 1/80, sobreposição de
  dois pixels e última peça recortada no limite de cada trecho. O avanço usa a largura útil.
- `Subsoil`: três faixas contínuas de pedra (0…−0,65; −0,65…−1,3; −1,3…−6). O recorte é o
  maior retângulo inteiramente opaco da fonte. SpriteRenderers Tiled substituem fileiras
  irregulares. O Ground original sobe até y = 0, mas não participa da prova de cobertura.
- `TerrainSlices`: 12 assets nativos de Sprite referenciam PNGs originais sem modificar
  pixels (seis gramados, cinco concretos, um núcleo).
- Concreto na entrada, enfermaria, hangar/oficina, comando, rádio e portão. Grama nos
  intervalos/acampamento. Tufos menores e esparsos, fora dos pavimentos.
- Física, só por geometria de gameplay (nenhum PolygonCollider de silhueta):
  `Collisions/Floor` (o mesmo objeto, agora com 120 × 4 e topo em y = 0, contínuo),
  `LeftWall` (−5,3), `RightWall` (98,6) e o `Blocker` do portão da expansão (84,6).
- Escadas, plataformas e andares superiores são **decorativos**.

## Zonas

| Zona | x (mundo) | Estado | Conteúdo |
| --- | --- | --- | --- |
| A Entrada | −5,3 a 5,3 | Open | Posto avançado (checkpoint), cercas, poste de luz, caixas, barreiras, sacos de areia, `PlayerSpawn` |
| B Enfermaria | 5,3 a 18,5 | Open | `HubInfirmary`, caixotes, sacos de areia, poste. Anchors de médico e feridos |
| C Hangar/Oficina | 18,5 a 42 | Open | Caminhão, palete, prateleira, `HubHangar` (hangar com avião + oficina auxiliar), `GoosiniAnchor` |
| D Acampamento | 42 a 57,7 | Open | Árvore, `HubCamp` (tenda, lona, caixas), barris, poste. Anchors de conversa e fogueira |
| E Comando | 57,7 a 72 | Scenic | Poste elétrico, `HubCommandRuins` com a entrada barricada (barreiras e sacos) |
| F Rádio | 72 a 84,6 | Locked | `HubRadio` com a entrada barricada, `RadioLockedAnchor` |
| G Expansão | 84,6 a 98,6 | Locked | Portão (grade, chapas, barricadas e o collider `Blocker`), torre, tapumes, treliça, plataforma, escada, abrigo, cercas |

Os props ficam dentro do grupo de cada zona (não há um grupo `Props` global), para mover a zona
inteira junto. O caminho é andável de A até a frente do rádio (F). O comando e o rádio ficam visíveis, mas
com as entradas barricadas, sem jeito de loja. A zona G é o limite físico.

Cada zona é um `BaseSector` (`sectorId`, `state`, `closedOnly[]`, `openOnly[]`). Fora de
`Open`, os objetos de `closedOnly` (barricadas, bloqueios) ficam ativos. **Desbloquear no
futuro:** chamar `SetState(BaseSector.State.Open)` no setor; o teste fez isso com a expansão,
e o portão e o collider sumiram. Novos acessos ou pontos de interação entram em `openOnly`.
Nenhuma regra de desbloqueio foi inventada.

Na revisão, cercas passaram para trás das fachadas (antes atravessavam entradas), suprimentos
foram separados em pequenos grupos laterais e as bases das construções ficaram a um pixel
do apoio. A barreira suspensa do portão foi assentada. O primeiro plano ficou só com pequenos
tufos/rochas junto ao chão, com tom menos escuro. As silhuetas retangulares de caixas/barris
foram desativadas nessa camada porque pareciam buracos no piso. Suprimentos normais das zonas
continuam ativos. Halos acompanham seus objetos de origem.

## Iluminação

O projeto usa o pipeline Built-in (sem URP/Light2D), e nada foi migrado. São 39 halos âmbar
(`BaseLightHalo.mat`, aditivo, alpha ≤ 0,32) sobre as lâmpadas desenhadas nas construções e
nos postes, na sorting layer AmbientEffects. Um único `BaseLightFlicker` faz variar um em cada
três halos (±12%). O frio vem do fundo violeta e das silhuetas tingidas; o calor vem das
oficinas, da enfermaria e dos postes.

## Prefabs (`Assets/Prefabs/Environment/Base`)

`HubInfirmary`, `HubHangar` (hangar + oficina), `HubCamp` (tenda + lona + caixas),
`HubCommandRuins`, `HubRadio` e `HubLampPost`. Cada prefab tem a arte e os seus halos, com o
pivô no chão.

## Personagens e anchors (`NPCAnchors`)

`GoosiniAnchor`, `MedicAnchor`, `WoundedSoldierAnchor`, `WoundedCivilianAnchor`,
`SoldiersConversationAnchor`, `CampfireAnchor` e `RadioLockedAnchor`, todos no chão (y = 0) e
com ícone no Editor. Não há prefab do Goosini para o HUB, então só existe o anchor (o boss
voando não foi reutilizado). NPCs genéricos futuros são humanos.

## Como editar

- **Mover construções:** mover o objeto `Hub*` na cena, mantendo y = −0,0125. Os halos vão junto.
- **Aumentar a largura:** acrescentar módulos à direita, mover `RightWall` e
  `MapExtents/PlayableRight`, e aumentar `VisualRight`, o tamanho do `Floor` e a escala do
  `Ground` (ou ajustar as constantes e rodar Install). Para manter a cobertura, as camadas de
  parallax precisam de conteúdo até `fator × alcance da câmera + meia vista`.
- **Novo módulo:** criar um `GameObject` em `Environment`, com um `BaseSector` se tiver estado,
  e usar sprites nas sorting layers acima. Construção grande vira prefab com o pivô no chão.
- **Reinstalar:** Install apaga e recria Background, Props, Lighting, NPCAnchors,
  Foreground e os filhos de Environment (exceto `Ground`). **Ajustes manuais nesses grupos se
  perdem**; Characters e o alvo de câmera são preservados. O instalador usa a receita corrigida.
  Depois de editar à mão, passe o ajuste para a ferramenta antes de reinstalar.
- **Reparar esta montagem:** `Crocodilo/Base/Repair terrain, scenery and Croc scale` →
  `BaseImprovisadaMapRepairTools.Repair`. Reconstrói Terrain, aplica posições conhecidas,
  ajusta o prefab do Croc e salva a cena. É mutação, não validação.
- **Conferir furos:** `Validate repaired ground coverage` renderiza Surface e Terrain
  separadamente numa Preview Scene, sem o Ground sólido. Confere 9600 pixels de largura,
  três linhas da superfície e 240 de profundidade, além dos intervalos/grade dos objetos.

## Validação da revisão de montagem

`Logs/MapRepair/FinalMapPlay.log`: compilação, cobertura renderizada (zero pixels transparentes
abaixo da caminhada), assets HUB, 172 verificações estáticas do mapa e
`CrocodiloWindowsBuild.ValidateProject` passaram. A cena tem 387 SpriteRenderers, contra
764 antes da revisão; nenhum colisor de decoração foi acrescentado.

`Logs/MapRepair/HubPlay.log`: **88 assertions passaram** com o Croc ampliado. Inclui o
percurso físico completo de A até G e volta (portão liberado somente durante o teste), sem
perder apoio, além de Idle, controles, parada, inversão, colisões, pausa, disable/enable,
recarga e câmera. O percurso foi acelerado a timeScale 2 para inspeção; os testes de FPS
foram em tempo normal e mediram 30,0 / 60,0 / 120,0 FPS, sempre a 3,000 unidades/s.

`Logs/MapRepair/FinalMapPlay.log`: **13 assertions de Play Mode** passaram após o ajuste
final do primeiro plano. Verificaram parallax, cobertura 21:9 em toda a faixa da câmera,
39 halos, chão contínuo nos dois sentidos, portão, desbloqueio e paredes.

`Logs/MapRepair/FinalTour.log`: percurso completo com o Croc repetido após o último ajuste
visual; **4 assertions passaram**, capturas atualizadas. Raiz física permaneceu entre
y = 0,0036 e 0,0050 durante a ida/volta, sem afundar ou saltar nas emendas.

Capturas reais de A–G em `Logs/HubCamera/tour_*.png` e `Logs/BaseImprovisadaMap`. Foram
inspecionadas entrada, enfermaria, hangar, oficina, acampamento, comando, rádio e expansão;
as versões em 1920×1080 e 800×600 também foram renderizadas. A suíte HUB inclui ainda
1280×720, 1680×720, 2560×720 e 320×180 para o enquadramento do Croc maior.

Os 97 PNGs de terreno/ambientação/construções foram comparados por SHA-256 com o ZIP
fornecido: todos idênticos. Validação Python do checkout: 1873 GUIDs. Não houve playtest
manual nem build novo; a inspeção visual usou capturas do renderer nativo em Play Mode.

## Validação histórica v01

- Unity 6000.6.4f1, batch: Install e validação estática com **172 verificações** (importação,
  reduções, escala 48×72 = 0,9, sorting layers, prefabs, Chapter1/build, ausência de scripts
  faltando, uma única câmera sem scripts, um único panorama opaco cobrindo 21:9, 4 colisores,
  nenhum PolygonCollider, estados dos setores, anchors, construções apoiadas, só 4 componentes
  de parallax + setores + 1 gerenciador de luz).
- Play Mode real: **13 verificações**. Nenhuma exceção. Velocidade aparente medida: BG0 0,03,
  BG3 0,30, BG4 0,60, primeiro plano 1,25. A cobertura vale em todo o trajeto possível da câmera
  (margem mínima de 0,41 em 21:9). Os halos variam. Uma sonda física do tamanho do Croc
  (0,5 × 0,88) apoiou no spawn (centro em y 0,455), andou para a direita sem afundar até parar
  no portão (x 84,14), passou com o setor aberto e parou no fim do mapa (98,24), e também parou
  na parede esquerda (−4,94). A câmera foi deslocada só durante o teste e voltou ao lugar.
- Capturas em `Logs/BaseImprovisadaMap` (ignorado pelo Git): A–G em 1200×720 e o hangar em
  1920×1080 e 800×600.
- `CrocodiloWindowsBuild.ValidateProject` passou.

**Limites daquela rodada histórica:** não validava A/D, Idle ou follow, pois o personagem
ainda não existia. O HUB foi testado na etapa seguinte (`BaseImprovisadaCamera.md`).
Os pixels da arte gerada por IA não seguem uma
grade uniforme entre pacotes: o terreno e as construções ficam com ~3 px de tela por "pixel"
desenhado em 720p, e os suprimentos reduzidos e o Croc, com 1 px.
