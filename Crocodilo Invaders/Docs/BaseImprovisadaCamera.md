# Base Improvisada — Croc simplificado e câmera

Atualizado em 24/10/2026, Unity 6000.6.4f1. Abra `Assets/Scenes/BaseImprovisada.unity`, pressione Play e use A/D, setas ou o eixo horizontal do analógico esquerdo. O Croc usa o novo Idle parado e a nova Run em movimento, conforme a velocidade física real.

**ESC / Start do controle:** abre o **mesmo PauseMenu.prefab do combate**, com Continuar,
Opções e Voltar ao menu. ESC fecha a pausa; nas opções, volta ao painel de pausa.
O retorno direto foi removido, incluindo seu componente e ação de input. PauseController
agora aceita o componente de input da cena (Behaviour), preservando as referências do
PlayerInputReader no combate e usando HubPlayerInput na base. O prefab visual, blur,
callbacks dos botões e fluxo do pause são os existentes. EventSystem usa Input System.

`Logs/HubPauseVerified.log`: **16 verificações em Play Mode passaram**, incluindo abrir/
fechar com ESC/Start, congelar Croc/Idle/câmera/áudio, Continuar via Enter, Opções e retorno
ao painel, Voltar ao menu sem duplicar carga, recarga da base e pausa/retomada do combate
com a referência original de PlayerInputReader preservada. Capturas inspecionadas:
`Logs/HubCamera/HUB_pause_existing_menu.png` e `HUB_pause_options.png`.

## Personagem e fontes

`Assets/Prefabs/Characters/CrocHub.prefab` tem apenas três objetos:

```text
CrocHub            Rigidbody2D, CapsuleCollider2D, HubPlayerInput, HubCharacterMotor2D, HubCharacterPresentation
├── VisualRoot     SpriteRenderer, Animator
└── CameraTarget   ponto estável, offset Y = 0,765
```

As folhas novas fornecidas em 24/10/2026 estão nos mesmos assets `CrocHub_Idle.png` e `CrocHub_Run.png`, com GUIDs e pixels originais preservados. Idle: cinco quadros 00–04 de uma grade 3×2; a célula final vazia fica fora. Todos os quadros novos são transparentes, incluindo o 00. Run: oito quadros 01–08 de uma grade 3×3; o primeiro (00) foi excluído a pedido do usuário para corrigir o loop. Ambas usam células 47×73, PPU 80, pivô (23,5; 2) px, Full Rect, Point, sem mipmaps/compressão ou redimensionamento.

Os clips e o controller mantêm seus GUIDs. Idle roda a 6 FPS (5/6 s), Run a 12 FPS (2/3 s). `HubCharacterPresentation` alimenta Speed com a velocidade horizontal real dividida pela máxima; a Run acompanha o analógico/aceleração e volta a Idle ao parar ou encontrar uma parede.

VisualRoot passa de 2× para 1,8×; a altura opaca fica próxima de 1,55 unidade (cerca de 10% menor). A cápsula acompanha a redução: 0,792×1,53, offset Y=0,765; CameraTarget Y=0,765. Os pés usam o mesmo chão nos dois estados. Não há curvas de Transform, root motion ou rig.

A câmera usa referência de render PPU 72, limite ortográfico 5 e largura máxima 23,333: a área vista fica 11,1% maior que antes em cada dimensão, preservando os degraus de zoom inteiro. CameraBounds passa a Y=-2,6..7,4. Só o panorama opaco do fundo foi ampliado para 10,25 unidades de altura, centrado em Y=2,4, para cobrir o afastamento. Terreno e construções não foram redimensionados.

Hashes e importação atuais em `SourceArt/CrocHubSimplifiedIdle/PixelLab_2026-10-24.json`; os registros de 18 e 21/10 são históricos. `HubCharacterTools.CreatePreviewFrames` renderiza os clips reais para os GIFs de preview, fora de Assets.

## Movimento

`HubPlayerInput` instancia as ações específicas `Assets/Inputs/Hub/HubInputActions.inputactions`, habilita somente Hub/Move, lê em Update e limpa/desabilita ao sair ou pausar. Os comandos de pausa vêm do PauseController compartilhado. Não interfere no action map do combate. Analógico com dead zone 0,15–0,95.

`HubCharacterMotor2D` é o único escritor da velocidade física horizontal: FixedUpdate, `Rigidbody2D.linearVelocity`, componente Y preservada. Os campos do Inspector no prefab são:

| Campo | Inicial | Efeito |
| --- | --- | --- |
| maxMoveSpeed | 3 un/s | 240 pixels de arte por segundo |
| acceleration | 30 un/s² | Arranque em aproximadamente 0,1 s |
| deceleration | 40 un/s² | Parada curta ao soltar |
| turnAcceleration | 42 un/s² | Resposta ao trocar de sentido |

O visual muda somente o sinal de sua escala X. A raiz física e CameraTarget nunca são espelhados. Corpo Dynamic, gravidade 3 configurável no Rigidbody, rotação congelada, Continuous e Interpolate; cápsula única 0,792×1,53, centro Y 0,765, ajustada à nova escala. Material físico próprio com atrito e restituição zero evita perder velocidade no chão ou prender nas paredes. O collider permanece independente do desenho respirando. Não há pulo, dash, combate ou interação.

## Arquitetura e referências

Não havia Cinemachine instalado. A versão 6.6 dos links fornecidos não corresponde a um pacote instalado; `6.6.0` no manifest é Timeline. A solução é o componente próprio `HubCameraController2D`, específico desta cena, com as técnicas estudadas nestas fontes:

- [Itay Keren — Scroll Back](https://www.gamedeveloper.com/design/scroll-back-the-theory-and-practice-of-cameras-in-side-scrollers): janela persistente, foco direcional e tratamento independente dos eixos.
- [Position Composer 3.1](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachinePositionComposer.html): proporções de viewport, amortecimento e filtragem da antecipação. Referência conceitual, sem instalar Cinemachine.
- [Confiner 2D 3.1](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineConfiner2D.html): confinar a janela visível inteira, considerando lente e proporção.
- [Unity 6000.6 — preparação dos sprites](https://docs.unity.com/en-us/engine/6000.6/manual/unity2d/2d-urp/2d-pixelperfect/prep-sprites): PPU, filtros e alinhamento; a implementação URP dessa página não foi usada no Built-in.
- [Pixel Perfect 6.0](https://docs.unity3d.com/Packages/com.unity.2d.pixel-perfect@6.0/manual/index.html): componente standalone para Built-in. Pacote 6.0.0 compatível com Unity 6000.5+, dependente apenas do módulo IMGUI já existente.

## Janela, antecipação e amortecimento

A largura da dead zone é **20% da largura visível**. O centro da janela é um estado persistente. Enquanto o alvo está dentro dela, a janela não avança. Ao cruzar sua lateral, somente o excesso desloca a janela: isto é uma camera window, não apenas um Lerp atrasado.

A posição inicial é ajustada diretamente ao alvo/confins, sem atravessar o mapa. Em deslocamentos normais a câmera usa SmoothDamp com deltaTime, tempo horizontal **0,26 s** e velocidade máxima de acompanhamento 12 un/s. Teletransportes acima de seis unidades reinicializam o enquadramento; caminhada não aciona esse caminho.

A direção vem do deslocamento real do CameraTarget, filtrado em 0,08 s, com limiar 0,15 un/s e confirmação de 0,14 s. Uma troca rápida de tecla não vira a antecipação imediatamente. O foco só é atualizado quando a janela realmente avança. A antecipação usa **12% da largura visível**, além da compensação da meia janela que ficou atrás do alvo. O objetivo fica em 38% da tela indo à direita e 62% indo à esquerda; durante a caminhada, o atraso suave da câmera aproxima o resultado de 43%/57% na vista de referência. A transição do foco tem seu próprio SmoothDamp de **0,4 s**.

Ao parar, o último enquadramento é mantido e as duas suavizações terminam; não há recentralização periódica. Um limite de segurança de 12% das bordas contém deslocamentos anormalmente rápidos, sujeito aos confins do mapa. Nas paredes, o enquadramento prioriza não revelar o vazio.

O eixo vertical começa desligado, com `baseHeight = 2,4`. Em janelas pequenas ou extremamente largas, a altura fixa se adapta somente à lente, reservando 0,35 unidade de chão e margem acima do personagem de altura `subjectHeight = 1,8`. Isso mantém o Croc ampliado visível em 320×180. A animação nunca participa do tracking. Para futuras escadas/plataformas, `verticalFollow` habilita uma janela vertical em unidades com amortecimento independente; será necessário definir regras de pulo/aterrissagem quando existir esse gameplay.

## Limites e parallax

`cameraBounds` é um Rect independente dos colliders do personagem: X −13,5…106,5 e Y −2,1…6,9, correspondendo à área visual do mapa atual. Para mudar o cenário, ajuste este retângulo no componente da Main Camera, conferindo panorama e chão nas bordas. Os marcadores de MapExtents continuam servindo como referência de autoria; a câmera não usa as paredes físicas como limite visual.

O centro permitido recua meia largura/altura da viewport a partir dos limites; a largura é calculada usando o aspecto real. Há uma margem horizontal adicional de um pixel para o arredondamento da câmera de render. Se a área for menor que a viewport em um eixo, a posição fica estável no centro desse eixo. Nesse caso é geometricamente impossível esconder o exterior sem ampliar arte ou aproximar a lente: o clamp não inventa cobertura.

As quatro camadas `ParallaxLayer2D` existentes continuam com seus fatores e referências. A câmera roda em LateUpdate na ordem 900; o parallax existente, em 1000. Não há um segundo escritor do fundo. Cenário, setores, luzes e transição do capítulo foram preservados.

## Pixels e tamanho da janela

O projeto continua **Built-in**. Foi adicionado apenas `com.unity.2d.pixel-perfect` 6.0.0; não houve instalação de Cinemachine, migração URP ou alteração da câmera aérea. A Main Camera da base possui um único PixelPerfectCamera, PPU 72, render snapping ligado, sem Upscale RT, crop ou stretch. MSAA foi desligado nessa câmera.

A posição lógica da câmera permanece contínua, seguindo o corpo interpolado em LateUpdate. O componente oficial arredonda a matriz de render e os sprites; não há arredondamento adicional do Transform do personagem/câmera nem snapping extra no parallax. Isso evita alimentar a suavização com posições já quantizadas. A câmera não segue a imagem de respiração.

A lente escolhe o menor zoom inteiro que mantém a meia altura visível em até 5 unidades e a largura em até 23,333 unidades (limite de cobertura do parallax existente). Assim também não abre buracos no panorama em janelas ultralargas. Usa a API pública `CorrectCinemachineOrthoSize` do Pixel Perfect como adaptador de lente; apesar do nome, essa API não depende de Cinemachine. Ela permite ao controlador ser o proprietário da lente sem o OnPreCull do Pixel Perfect sobrescrevê-la.

Consequência deliberada: **redimensionar a janela pode aproximar um pouco o enquadramento**, em degraus de escala inteira. Em 1280×720 são 1 pixel de tela por pixel de arte, meia altura 4,5; em 1920×1080, escala 2 e meia altura 3,375; em 800×600, escala 1 e meia altura 3,75. A lente permanece estável durante cada tamanho de janela, sem animação de zoom ou barras pretas. Esse compromisso preserva pixels uniformes e a cobertura vertical do panorama existente, sem ampliar o personagem permanentemente.

O mapa preexistente contém construções e ornamentos com PPUs/escala distintos, ajustados pela dimensão artística; eles foram preservados. O Pixel Perfect garante a grade de render PPU 72 para Croc/terreno, mas não transforma essas fontes de resoluções diferentes em tiles nativos uniformes. Para uniformizar o cenário inteiro no futuro, será necessário preparar suas artes na mesma grade; não arredondar escalas arbitrariamente durante o gameplay.

Não adicionar outro follow, Animator de câmera, transform snapping ou Cinemachine ativo na mesma câmera. Manter CameraTarget fora de VisualRoot. Os gizmos opcionais (selecione a câmera na Scene View) exibem alvo, janela, limites, antecipação e objetivo antes da suavização; não entram no jogo.

## Edição das animações

Abra `CrocHub.controller` no Animator para ver Idle ↔ Run e os clips nativos na janela Animation. O parâmetro Speed é normalizado pela velocidade máxima do motor. As transições usam limiar 0,02 e duração zero. A Run usa esse mesmo parâmetro como multiplicador de reprodução; Idle mantém 6 FPS. Os quadros são os cinco de Idle e os oito selecionados de Run fornecidos em 24/10.

`HubCharacterTools.InstallAnimations` reimporta as duas folhas, atualiza clips/controller preservando GUIDs e conecta a apresentação no prefab existente. É uma mutação explícita, sem reconstruir o mapa ou a câmera. `HubCharacterTools.Install` também usa esses assets novos e não restaura o Idle anterior.

## Validação

As medições e hashes dos sete PNGs abaixo são históricos da versão de 18/10; a importação e os testes de 21/10 usam as novas folhas.

`HubCharacterTools.Validate` verifica importação, clip, hierarquia, referências, câmera e preservação do cenário. `BaseImprovisadaMapTools.Validate` aceita os componentes específicos novos sem relaxar as verificações de arte/colliders do mapa. A inspeção isolada do cenário suspende o follow somente durante o teste que posiciona a câmera manualmente; `HubPlayValidation` testa os controles, personagem e câmera reais juntos.

Rodada original: `Logs/HubPlayFinal.log`, 86 assertions. **Após ampliar o Croc e corrigir o mapa**, `Logs/MapRepair/HubPlay.log`: **88 assertions passaram em Play Mode real**, além dos assets HUB e **172 verificações estáticas do mapa**. A rodada inclui o percurso físico completo de ida/volta, Idle, caminhada nos dois sentidos, janela, foco direcional, inversões curtas, parada, analógico/dead zone, disable/enable, pausa, bloqueio da expansão, paredes, viewport menor/maior que os limites e recarga da cena. Não houve erro/exception de runtime. O percurso foi acelerado em timeScale 2; demais cenários/FPS usaram tempo normal.

| Limite solicitado | FPS médio medido | Velocidade física medida |
| --- | --- | --- |
| 30 | 30,0 | 3,000 un/s |
| 60 | 60,0 | 3,000 un/s |
| 120 | 120,0 | 3,000 un/s |

Game View real: 1280×720, 1920×1080, 800×600, 1680×720, 2560×720 e 320×180. A lente manteve escala inteira, o sprite permaneceu visível e os confins/panorama cobriram as amostras nas duas extremidades e no meio. Janela extremamente pequena aproxima bastante a arte, pois não há redução dos pixels nativos. Os limites de FPS foram efetivamente medidos no Editor, não apenas calculados a partir de um deltaTime inventado.

Capturas do renderer nativo em Play Mode estão em `Logs/HubCamera` (ignoradas pelo Git): A_idle, B_right, C_left, E_stop, F_left/right_wall, G_move_cap_*, H_hangar_* e comparativos de snapping. As imagens principais foram inspecionadas visualmente. Interpolate + LateUpdate + render snapping único foi mantido; câmera lógica sem arredondamento evita realimentação de quantização. As capturas com/sem sprite snapping não substituem avaliar conforto/jitter jogando por um período maior. **Não houve playtest manual, vídeo ou novo EXE.**

Os sete PNGs foram comparados por SHA-256 com os originais: todos idênticos. Após a revisão do terreno, a validação Python passou com 1873 GUIDs. A suíte isolada do mapa passou 13 assertions após o ajuste final do primeiro plano (`Logs/MapRepair/FinalMapPlay.log`); a validação geral de build/assets também passou, sem gerar executável. A suíte longa de ondas não foi reexecutada: somente sua assertion final da base foi atualizada para o novo estado. Gameplay/HUD/câmera aérea não foram modificados.

### Atualização Idle/Run — 21/10/2026

A importação nativa conferiu os seis sprites de Idle (sem quadro 00), 13 de Run, ordem exata, pivôs, duração, ausência de curvas de Transform e referências do prefab/controller. A suíte `HubPlayValidation.Run` passou **94 assertions** em `Logs/CrocHubPixelLab-Play.log`: todos os quadros de Run foram vistos, Run nos dois sentidos, Idle ao parar/desabilitar input/encostar no portão, pausa congelando sprite/clock, percurso do mapa, FPS, resoluções e recarga da cena. Capturas A_idle, B_right e C_left em `Logs/HubCamera` conferidas visualmente. Os dois PNGs têm SHA-256 idêntico aos fornecidos; não houve redesenho nem alteração do fundo do arquivo original, somente exclusão do quadro 00 na importação.

### Revisão de 24/10/2026

Folhas atuais com cinco/nove quadros conferidas; 94 assertions em Play Mode passaram em `Logs/CrocHub-20261024-PlayFinal.log`. Incluem Idle/Run, pausa, paredes, movimento nos dois sentidos, FPS, percurso completo e seis tamanhos reais de janela, com cobertura do panorama nas extremidades. Capturas A_idle/B_right conferidas. Previews em `Docs/ArtPreviews/CrocHub_2026-10-24/Idle.gif` e `Run.gif`, renderizados a partir dos clips nativos (ampliados, sem alterar os PNGs de origem). O enquadramento real está na captura A_idle. A referência de render 72 preserva zoom inteiro; as artes do mapa mantêm sua importação existente.

Revisão do loop: Run 00 removido; ficam exatamente 01–08, a 12 FPS e 2/3 s. Compilação e validação nativa de quantidade, ordem, duração, loop e referências passaram em `Logs/CrocHub-RunEightFrames.log`. Preview Run.gif atualizado com oito imagens renderizadas do clip. A suíte completa de Play acima corresponde à versão anterior de nove quadros; não foi repetida para esta exclusão.

Cauda da Run (24/10): folha adicional fornecida em `pixellab-Very-subtle-seamless-idle-loop-1792871227103.png`, preservada em `CrocHub_Tail.png`. Grade 3×3 de células 226×228, PPU 480, pivô (205; 120) px; oito poses 00–07 acompanham os oito quadros do corpo. A pose final de retorno 08 fica fora do loop. `VisualRoot/Tail` tem apenas Transform e SpriteRenderer, na posição local (0,02; 0,30), escala local 0,85× (15% menor que a original e mais para dentro do corpo), ordem de desenho um nível atrás do corpo; herda a direção/escala do VisualRoot. O mesmo Animator anima seu sprite e o deixa nulo em Idle. Não há física, Animator ou script adicional. A hierarquia passa de três para quatro objetos. O preview Run.gif inclui essa composição.

Validação da cauda: compilação e assets passaram, assim como 172 verificações do mapa e seis assertions específicas de Play Mode (oculta em Idle, oito poses em Run, direita, esquerda, pausa e retorno a Idle), em `Logs/CrocHub-RunTailPlay.log`. PNG de origem preservado por comparação de bytes e SHA-256. Validador do mapa reconhece a cauda intencionalmente sem sprite em Idle.

Validação após integrar a main em 02/11/2026: compilação, assets, 172 checks do mapa e seis assertions de Play da cauda passaram novamente em `Logs/CrocHub-PostPull-20261102.log`. Encaixe final da cauda: posição (0,02; 0,30), escala 0,85×. Checkout conferido com 2643 GUIDs.
