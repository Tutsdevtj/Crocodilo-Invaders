# Menu principal

Abra `Assets/Scenes/MainMenu.unity` e entre em Play Mode. A cena usa o prefab
`Assets/Prefabs/UI/MainMenu.prefab`: fundo, logo, três nuvens e os cinco botões
ficam visíveis e editáveis na Hierarchy, sem construção de UI em runtime.

## Ações e navegação

- Mouse: passar sobre um botão transfere a seleção; clicar confirma.
- Teclado: setas para cima/baixo navegam; Enter confirma. A navegação dá a volta
  entre Jogar e Sair. O Input System UI também oferece os controles padrão de gamepad.
- Jogar abre `PlayerMovementTest` e solicita `WaveController.StartRun()` após
  carregar a cena. O anúncio da primeira onda aparece sem outro clique em Iniciar.
- Abrir a cena de gameplay diretamente ainda conserva o botão provisório Iniciar.
- Sair fecha o aplicativo em builds desktop; no Editor, encerra somente o Play Mode.
- Online, Sobre e Opções têm seleção e animação, mas ainda não executam ações.

## Responsabilidades

- `MenuIntro`: apresentação de entrada e início sincronizado da música. Anima
  o logo e as raízes dos botões, sem disputar o movimento das artes com o hover.
- `MainMenuController`: ações Jogar/Sair, saída suave e solicitação de início da
  partida. Não controla inimigos, vida, pontuação ou regras das ondas.
- `MenuButtonFeedback`: aumento discreto, pressão, diamantes e sons de seleção.
  Mouse e teclado usam o mesmo foco do EventSystem.
- `MenuAmbientMotion`: oscila cada nuvem em torno da posição definida na cena,
  com amplitude, período e fase ajustáveis. Usa tempo não escalado.

As nuvens não usam o spawner ou o scrolling do gameplay. Os diamantes permanecem
na última opção selecionada mesmo depois que o mouse sai dela, mantendo o foco
visível para quem continua a navegação pelo teclado.

## Entrada musical

- O céu clareia suavemente durante os primeiros 0,75 s da música.
- O logo entra a partir de 1,15 s e se acomoda em 0,8 s.
- Em 2,59 s, o logo recebe um pequeno pulso e os botões aparecem em cascata,
  com deslocamento de 14 pixels, fade de 0,32 s e intervalo de 0,065 s entre itens.
- Por volta de 3,17 s, o menu está inteiro visível e Jogar recebe a seleção.

O ponto de 2,59 s corresponde a um ataque forte medido nos primeiros segundos
do `menu_music.mp3`, perto dos 2,5 s estimados no pedido. `Drop Time` no componente
`MenuIntro` é ajustável no Inspector; os demais tempos também são configuráveis.

A música é preparada e iniciada com `AudioSource.PlayScheduled`. A animação
acompanha `AudioSettings.dspTime`, evitando que FPS ou `Time.timeScale` alterem
a marcação musical. Não é adicionada outra música nem um som de impacto artificial.

Uma tecla, clique ou botão principal do controle pode pular a entrada após os
primeiros 0,15 s. Esse comando só revela o menu: não aciona Jogar junto. A música
segue do mesmo ponto. Os botões invisíveis não recebem foco nem ações durante a
entrada. A cortina de fade fica inativa na edição e é habilitada só em runtime.

## Arte e composição

Os onze PNGs recebidos foram copiados sem alterar os pixels para
`Assets/Art/MainMenu`. Os sprites são recortados na importação para remover
margens transparentes; Point, sem mipmaps e sem compressão preservam a arte.
`CrocodiloTextureImporter` permanece intacto, pois esses assets ficam fora de
`Assets/Resources/Art`.

Canvas em espaço de tela, referência 1536 × 1024, escala pela altura. O fundo
preenche a janela preservando a proporção, com corte nas bordas quando necessário.
A composição foi conferida em 1536 × 1024, 1920 × 1080 e 1024 × 768.

Música e sons: `menu_music`, `selection` e `select`, já existentes no port.

## Integração com as cenas

O rascunho do menu em Canvas World Space foi substituído na cena; os arquivos
UXML, PanelSettings e a textura de madeira não foram apagados.

`CrocodiloProjectSetup` registra MainMenu como primeira cena, seguido do gameplay
e do port legado. Preserva outras cenas registradas. O build WebGL usa a lista
de cenas habilitadas, permitindo carregar o gameplay a partir do menu.
O comando `Crocodilo/Preparar cena` também abre o MainMenu.

`WaveController.Start()` só bloqueia o player quando a partida está aguardando
início, para não desfazer uma solicitação feita no evento `sceneLoaded`.

## Referências consultadas

- Microsoft, foco visível na UI:
  https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/113
- Microsoft, navegação em ordem consistente com a disposição visual:
  https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/112
- Unity, integração de uGUI com o módulo do Input System:
  https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/Documentation~/introduction-ui-input-module.md
- Unity, início de áudio agendado e relógio DSP:
  https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AudioSource.PlayScheduled.html
  https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AudioSettings-dspTime.html
- Microsoft, movimento e distrações visuais em jogos:
  https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/117

Aplicação nesta cena: foco indicado por dois diamantes, ordem vertical explícita,
resposta curta à seleção e movimento ambiente de baixa amplitude. Tempos e
amplitudes são escolhas desta implementação, não valores prescritos pelas fontes.
