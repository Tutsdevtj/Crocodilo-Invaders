# Menu de pausa

ESC abre e fecha a pausa; Start do controle também funciona. Mouse, setas e
Enter navegam nos botões. No controle, B volta da tela provisória de opções.

- **Continuar** retoma a mesma partida e a música do ponto em que pararam.
- **Opções** mostra uma página provisória de “em desenvolvimento”, com Voltar.
- **Voltar ao menu** encerra a partida atual e carrega MainMenu. Jogar começa
  uma partida nova normalmente.

Na tela de opções, ESC volta à pausa. É possível abrir a pausa na tela de
resultado para voltar ao menu; Continuar apenas retorna ao resultado, sem
reviver o player. A cena de gameplay já contém o prefab `PauseMenu`.

## Apresentação

As nove artes enviadas foram copiadas sem alteração para `Assets/Art/PauseMenu`.
O céu da referência e o divisor duplicado não foram usados. Os recortes são
metadados de importação Sprite, Point, sem mipmaps nem compressão.

O fundo é uma captura da câmera do próprio jogo, processada por um blur em duas
passagens na GPU. A captura ocorre uma vez ao abrir a pausa; o jogo segue sem
custo de blur enquanto o menu está fechado. A HUD fica coberta pela captura.
O escurecimento externo é de 28%; o preenchimento central acrescenta 83% de
opacidade. Os dois valores são ajustáveis nas imagens do prefab.

A entrada dura 0,18 segundo e a saída 0,10 segundo, usando tempo independente
da pausa. O painel sobe 12 pixels e amplia suavemente; o logo acompanha a entrada.
Dezoito partículas pequenas flutuam ao fundo. Os botões reaproveitam o feedback
do menu inicial: escala, diamantes e sons de foco/clique.

## Responsabilidades

- `PauseController`: comandos de pausa/volta, bloqueio do input local, relógio,
  áudio e troca para MainMenu. Continuar restaura o estado anterior; voltar ao
  menu restabelece a velocidade normal e libera o áudio para a partida seguinte.
- `PauseMenuView`: entrada/saída, foco dos botões e página provisória de opções.
- `PauseBackdrop` e `PauseBlur.shader`: captura, blur e liberação das texturas.
- `PauseMenuParticles`: animação visual em tempo não escalado, sem consumir o
  Random usado pelo gameplay.
- `MenuButtonFeedback`: componente existente, compartilhado com MainMenu.

`Time.timeScale` congela a física, os animators e os timers atuais de gameplay.
`PlayerInputReader` fica desabilitado para que as setas do menu não enviem
comandos ao player. A pausa não altera HP, pontuação nem o estado da onda.

O áudio usa [AudioListener.pause](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AudioListener-pause.html),
que suspende também o relógio do áudio. Os sons da interface usam
[ignoreListenerPause](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AudioSource-ignoreListenerPause.html).
O blur usa [Graphics.Blit](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Graphics.Blit.html)
com texturas distintas para cada passagem.

Esta pausa é para a partida local atual. Quando houver multiplayer, pausar a
interface local não deverá congelar a simulação autoritativa do servidor.
