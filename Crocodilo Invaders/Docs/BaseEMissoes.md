# Base 2D e ciclo de missões

Atualização de 18/10/2026: a implementação anterior do personagem e do acompanhamento
da câmera de `BaseImprovisada` foi removida a pedido do usuário. A cena conserva
chão/colisões e uma câmera nativa fixa, recebendo a transição ao concluir o capítulo 1. Ver
[Base Improvisada e rig](BaseImprovisadaRig.md). O restante deste documento continua
como planejamento; não foram implementados terminal, missões, NPCs ou economia.
Personagem, rig, animações e lógica de câmera serão refeitos em uma nova etapa.

Planejamento para a próxima etapa do jogo. A base será um cenário lateral
explorável com um personagem a pé. A missão continuará usando o combate atual,
com nove ondas e o boss na décima. Uma tentativa termina em vitória ou derrota,
seguida de um resumo e do retorno à base.

## Fluxo do jogador

Menu principal → Jogar → Base → Interagir com o ponto de missão → Escolher fase
→ Loading → Enter para iniciar → Dez ondas → Vitória/derrota → Resumo → Base.

Uma fase é a tentativa completa de dez ondas. Entre ondas, o jogador escolhe
cartas dentro da missão. A base aparece entre tentativas.

- **Vitória:** derrotar o boss, concluir sua apresentação de morte, salvar o
  resultado, mostrar o resumo e oferecer “Voltar à base”.
- **Derrota:** morrer, encerrar o combate, salvar o resultado, mostrar o resumo
  e oferecer “Voltar à base”.
- **Abandono, proposta inicial:** uma opção no pause confirma a saída e leva à
  base; a tentativa fica cancelada e não entra no ranking de partidas concluídas.
- **Nova tentativa:** iniciar na primeira onda, com vida completa e atributos
  iniciais. As cartas são bônus daquela tentativa e são reiniciadas.

O nome do jogador e o ranking permanecem salvos. Desbloqueios e melhorias
permanentes poderão ser acrescentados ao perfil quando suas regras forem definidas.

## Primeira base

Um hangar pequeno, visto de lado, combina com o combate aéreo. A primeira versão
terá uma plataforma contínua, câmera fixa e três pontos principais:

1. **Entrada/retorno:** posição em que o personagem aparece ao entrar ou retornar.
2. **Terminal de missões ou porta do hangar:** aproximação mostra “E — Missões”.
   A interação abre a seleção da primeira fase e o botão “Iniciar missão”.
3. **Terminal de ranking:** abre a interface de leaderboard já existente.

Uma oficina pode ocupar um espaço reservado do cenário para uma futura etapa
de melhorias permanentes. O primeiro ciclo funciona com os três pontos acima.

## Personagem da base

Criar um prefab `BaseCharacter`, com controle próprio de plataforma lateral:

- Movimento horizontal, gravidade, colisão com o chão e detecção de solo.
- Idle e caminhada, com inversão do sprite conforme a direção.
- Interação ao apertar E dentro do alcance de um terminal.
- Pulo, caso o layout da base tenha plataformas ou desníveis que o utilizem.
- Entrada bloqueada enquanto uma interface ou transição estiver aberta.

O `Player` de combate e o `BaseCharacter` terão prefabs e controles separados.
O perfil mantém a identidade do jogador; cada cena cria o corpo que utiliza.
Isso permite evoluir as animações da base sem acoplar movimento lateral à nave.

## Organização proposta

| Parte | Responsabilidade |
| --- | --- |
| Cena `Base` | Plataforma, decoração, câmera, terminais e personagem a pé |
| `BaseCharacterController` | Movimento e animação do personagem da base |
| `BaseInteractor` | Alcance de interação, prompt e acionamento dos terminais |
| `MissionDefinition` | ID da fase, nome, imagem e cena de combate; configuração por asset |
| `MissionSelectionView` | Mostrar as fases disponíveis e iniciar a selecionada |
| `GameFlowController` | Coordenar saída da base, término da missão e retorno |
| `RunResult` | ID da tentativa, fase, desfecho, onda alcançada, pontos, abates e duração |
| `RunResultView` | Resumo da tentativa e botão de retorno |
| `SceneTransitionController` | Fade e carregamento assíncrono entre menu e base, e no retorno |

O coordenador pode sobreviver às trocas de cena. Personagem, inimigos, HUD e
interfaces específicas pertencem às suas cenas. O resultado transporta dados,
sem carregar referências aos objetos da cena que será descarregada.

## Integração com o projeto atual

- `MainMenuController.PlayGame` passará a levar à base. A preparação da tentativa
  no `LeaderboardSession` acontecerá ao iniciar uma missão, após definir o nome.
- `WaveController.RunEnded(bool)` já oferece o evento de vitória/derrota.
  O fluxo de retorno observará esse evento e aproveitará o encerramento atual
  de spawns, projéteis e controle do jogador.
- `LeaderboardSession` já tem ID de tentativa e resultado pendente. O resultado
  deve ser salvo antes da troca para a base, com proteção contra gravação duplicada.
- `GameplayLoadingController` seguirá preparando cenas de combate e iniciando
  `WaveController.StartRun` após Enter. A transição para `Base` terá seu próprio
  carregamento: a base não depende de um `WaveController`.
- O pause receberá a entrada do contexto ativo. Na missão, oferecerá abandono
  para a base; na base, permitirá opções e retorno ao menu principal.
- Trocas de cena devem liberar os bloqueios de entrada e restaurar o relógio
  e o áudio, inclusive quando a saída parte do pause ou do resumo.
- `PlayerRunUpgrades.ResetForRun` continuará reiniciando os bônus temporários.
- Incluir `Base` nas cenas do build. O validador Windows atual exige MainMenu e
  PlayerMovementTest nas duas primeiras posições; a base pode ser acrescentada
  depois delas, usando caminhos de cena para as transições.

## Assets para procurar

| Asset | O que procurar |
| --- | --- |
| Personagem a pé | Vista lateral, idle e caminhada; pulo/queda se houver plataformas |
| Chão e paredes | Tiles de plataforma, colisão simples e cantos compatíveis |
| Hangar | Fundo, porta de saída, plataformas e estruturas de suporte |
| Terminal | Monitor, painel ou mesa para seleção de missão e ranking |
| Decoração | Caixas, ferramentas, tubulações e iluminação para dar vida à base |
| UI de missão/resultado | Molduras que combinem com azul escuro, marfim e dourado do jogo |

Escolher uma escala de pixels consistente entre personagem e tiles. Priorizar
packs cujas animações e cenário compartilhem proporções, perspectiva e paleta.
Registrar a licença e os créditos de cada pack escolhido.

## Ordem de implementação

1. Criar uma base provisória com chão, personagem simples e ponto de missão.
2. Ligar Jogar → Base → Loading → combate atual.
3. Ligar vitória e derrota → resumo → Base, mantendo o ranking salvo.
4. Conferir novas tentativas, reset de cartas/vida e navegação pelo pause.
5. Substituir os placeholders pelos assets e animar personagem, porta e terminais.
6. Expandir seleção de fases e progressão permanente quando suas regras forem definidas.

Critérios da primeira versão: vencer ou perder sempre permite retornar à base;
uma nova missão começa limpa; o resultado é salvo uma única vez; interfaces
bloqueiam movimento; o retorno a partir do pause não deixa jogo ou áudio congelados.
