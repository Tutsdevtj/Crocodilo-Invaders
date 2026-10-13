# Executor V-9

O boss é a **onda 10**, após concluir nove hordas, nas cenas `PlayerMovementTest`
e `StageOneWaves`. Ele também recebe o anúncio de onda antes de entrar.
Ele entra pela direita, passa a patrulhar a área reservada e só começa a atacar
depois de chegar. A vitória acontece ao derrotá-lo, após três segundos de explosão.

## O que foi preservado do port

- Arte e animação do Executor V-9, mísseis, lasers, explosão e retrato.
- Música do boss, sons de dano, mísseis, lasers e morte.
- Lasers horizontais e verticais anunciados antes de causarem 5 de dano.
- Recompensa de 40.000 pontos.

O port mantinha o boss parado e administrava tiros/colisões com coordenadas de GUI.
Agora ele usa SpriteRenderer, Rigidbody2D, Collider2D, Health e prefabs de projéteis.
As cópias de arte em `Assets/Art/Boss` são sprites; as texturas de Resources e o importer
do port mantêm seus usos anteriores.

O contador registra um boss como um abate. O port acrescentava dez abates ao matá-lo,
um atalho do contador antigo que não representa dez inimigos mortos. Com os valores
atuais, a partida completa rende pelo menos **209.000 pontos e 201 abates**,
mais os pontos e abates dos interceptores de reforço realmente derrotados.

Durante a luta, `BossReinforcements` chama dois interceptores, pela primeira vez
6 segundos após o boss chegar e depois a cada 9–12 segundos. Há no máximo dois
reforços vivos. Eles usam o mesmo `EnemySpawner`, área e alvo das hordas; abates
concedem 800 pontos normalmente. A pausa congela a agenda. Morte do boss, derrota
do player ou desativação do componente remove os reforços restantes sem conceder
pontos por remoção. Derrotar os reforços não encerra a luta contra o boss.

O boss mantém **258 HP** neste balanceamento. A segunda fase continua usando
50% da vida máxima: com esse valor, começa em 129 HP ou menos, entre padrões.

A espera de vitória do port era de aproximadamente 11,7 segundos; aqui acompanha a
explosão de três segundos. O tiro especial mantém o dano atual do player, 4, para
preservar o combate já migrado. Os mísseis do boss causam 2 de dano, editável no prefab.

## Ataques e respostas possíveis

| Padrão | Aviso | Ataque | Como responder |
| --- | --- | --- | --- |
| Leques | Carga no canhão e arco dourado | 2 leques de 7 tiros vermelhos; o seguinte tem um pequeno deslocamento angular | Recuar para aumentar os espaços e mover-se entre os raios |
| Mísseis | Carga e linha de mira sobre uma posição travada | 3 mísseis em sequência, com direções próximas da posição anunciada | Sair da linha marcada antes da salva; eles não perseguem o player |
| Varredura | Carga e guia que varre um ângulo | 18 tiros vermelhos emitidos progressivamente pela arena | Acompanhar o espaço livre atrás da varredura ou recuar |
| Lasers | Duas faixas douradas pulsantes durante 1,15 segundo | Duas faixas simultâneas; orientação alterna vertical/horizontal | Sair das faixas durante o aviso; há espaço entre elas e um pulso acerta cada alvo uma vez |

O boss continua se movimentando durante os padrões. A posição usada na mira fica
travada antes dos disparos: mudar de posição funciona porque os projéteis já lançados
mantêm sua direção. Uma rodada apresenta os quatro padrões em ordem embaralhada;
não há repetição imediata na passagem para a rodada seguinte.

Após cair para **50% de HP ou menos**, ele termina o padrão atual, limpa seus ataques
e faz uma pausa de 1,1 segundo antes da segunda fase. São 3 leques de 9 tiros, 4 mísseis,
22 tiros na varredura, intervalos menores e lasers com aviso de 1 segundo. A porcentagem
usa a vida máxima de Health; não compara com um número fixo de HP.

Não há mistura irrestrita de padrões ou mudança da velocidade de um ataque no meio
do disparo. Os ajustes são uma primeira passagem de balanceamento: o ritmo percebido
precisa ser avaliado jogando com os controles e a área atuais.

## Separação de responsabilidades

- `Boss`: fachada da entidade, entrada, liberação da hitbox, início/fim do combate e morte.
- `BossCombat`: sequência de ataques, mira, padrões de projéteis, lasers e segunda fase.
- `BossLaser`: ciclo aviso/disparo/fim de um laser e dano por Collider2D, independente da UI.
- `BossPresentation`: som, música, carga, guia de mira, explosão e desaparecimento da nave.
- `BossHUD`: observa Health para mostrar os sprites da barra e do retrato original.
- `WaveController`: conecta o fim das ondas ao boss e concede a recompensa/vitória.
- `RunScore`: contabiliza pontos e abates. A UI não calcula a recompensa.

`EnemyMovement`, `Health`, `SpriteHitFlash` e `Projectile` são reutilizados.
Projectile aceita uma velocidade opcional no lançamento, permitindo usar o mesmo
prefab vermelho em padrões com cadências/velocidades distintas, preservando o disparo
dos inimigos normais. Não há leitura de Input System nos sistemas do boss.

## Onde ajustar

No prefab `Assets/Prefabs/Boss/ExecutorV9.prefab`:

- **Health / Max Health**: 258.
- **EnemyMovement**: velocidade de entrada 2,8 e patrulha 1,35.
- **Boss / Patrol Area**: área reservada à direita das paredes do player.
- **BossCombat**: intervalo de aviso, recuperação, velocidades de tiros/mísseis,
  porcentagem da segunda fase e área do player utilizada pelos lasers.
- **Boss / Points On Defeat** e **Defeat Duration**: recompensa e espera da explosão.

Ao alterar as paredes da arena, ajuste **Player Arena** do BossCombat para manter a
mira dos lasers na área do player. **Laser Coverage** define o alcance completo dos
feixes, incluindo o lado do boss: padrão X de -9,5 a 9,5 e Y de -4,2 a 6,4.
A câmera não determina as regras do ataque.

`BossLaser.prefab` define o dano e a layer alvo, além de **Visual Thickness** (1,5)
e **Damage Thickness** (0,45). A faixa de aviso usa a mesma largura da colisão;
o brilho é mais largo. `BossMissile.prefab` define dano e duração.
`BossHUD.prefab` contém só apresentação, sem painel preto.

## Pesquisa e arte

A [análise de telegraphs de Scott Fine](https://scottfinegamedesign.com/sfgd-notes/2026/8/10/sf-game-design-notes-16-a-look-at-telegraphs)
discute como distância, preparação visual e áudio dão informação para reagir a ataques.
Aplicamos esses princípios aos avisos, à mira travada e às faixas dos lasers.
A palestra [Boss Up, de Itay Keren na GDC](https://www.gdcvault.com/play/1025398/Boss-Up-Boss-Battle-Design)
apresenta o boss como uma culminação das habilidades aprendidas; aqui a luta cobra
movimentação, leitura dos tiros e uso do especial já praticados nas ondas.

Os três packs indicados foram consultados. Foi usado o efeito **charged**, de
[Warped Shooting FX / ansimuz](https://ansimuz.itch.io/warped-shooting-fx), para preparar
os ataques. ARCBIT Lite e a edição gratuita de Super Pixel Effects Gigapack ficaram
como referências; não foram incorporados assets das versões pagas.
O registro para a futura tela de créditos está em `Docs/Creditos.md`.
