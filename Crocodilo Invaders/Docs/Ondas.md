# Progressão de dez ondas

O MainMenu passa pelo loading e inicia a partida após ENTER. Ao abrir diretamente
`Assets/Scenes/Tests/PlayerMovementTest.unity` ou `StageOneWaves.unity`, o botão
provisório **INICIAR** continua disponível. As duas cenas usam a mesma composição.

São nove hordas e o boss na décima onda. O aviso e a HUD contam o boss no total,
sem criar uma décima horda antes dele.

| Onda | Patrulheiros | Interceptores | Artilheiros | Total | Máximo vivo | Intervalo entre pares |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| 1 | 4 | 0 | 0 | 4 | 2 | 2,8–3,5 s |
| 2 | 8 | 0 | 0 | 8 | 4 | 2,5–3,2 s |
| 3 | 10 | 2 | 0 | 12 | 4 | 2,3–3,0 s |
| 4 | 12 | 4 | 0 | 16 | 6 | 2,1–2,8 s |
| 5 | 12 | 8 | 0 | 20 | 6 | 2,0–2,6 s |
| 6 | 14 | 10 | 2 | 26 | 6 | 1,9–2,5 s |
| 7 | 16 | 12 | 4 | 32 | 8 | 1,7–2,3 s |
| 8 | 18 | 14 | 6 | 38 | 8 | 1,6–2,2 s |
| 9 | 20 | 16 | 8 | 44 | 8 | 1,5–2,1 s |
| 10 | — | Reforços periódicos | — | Boss | 2 reforços | 9–12 s |

Os grupos entram em pares. O interceptor aparece primeiro na onda 3 e o artilheiro
primeiro na onda 6; nas demais ondas, os grupos alternam tipos. A dificuldade sobe
pela composição, quantidade, cadência e limite simultâneo, sem aumentar o HP dos
inimigos comuns. Quantidades ímpares continuam permitindo um último spawn sozinho.

O player começa com **0,45 segundo entre tiros**, em vez de 0,20: aproximadamente
2,2 tiros por segundo. Segurar Espaço, carga do especial, sons e demais controles
continuam funcionando. As melhorias por carta aumentam a cadência sobre esta base.

Uma onda termina quando todos os inimigos previstos nasceram e nenhum continua vivo.
Cada inimigo mantém os pontos configurados no prefab, atualmente **800**.
Cada onda concluída acrescenta **1.000 pontos**, sem incrementar o contador de inimigos.
Completar as nove hordas rende **169.000 pontos e 200 inimigos**. O boss concede
40.000 pontos e um abate: o mínimo da vitória é **209.000 pontos e 201 abates**,
mais 800 pontos e um abate para cada interceptor de reforço realmente derrotado.
Veja `Docs/Boss.md` para os ataques e reforços.
Destruir um inimigo sem derrotá-lo não concede pontos por abate.

Na transição, os projéteis inimigos restantes são removidos para não causar dano
durante o aviso de conclusão. Nas duas cenas configuradas, cada horda abre uma escolha
de melhoria: o combate fica congelado até selecionar uma carta. A próxima onda espera
explicitamente pela escolha, independentemente do tempo do intervalo. Ao derrotar o
boss ou morrer, o combate para e aparece o resultado.
Uma nova partida pode ser iniciada voltando ao MainMenu pela pausa.

## Responsabilidades

- `WaveController`: início, quantidade de ondas, anúncio, combate, intervalo e fim
  da partida. Decide quando liberar o spawn e conceder o bônus; não desenha UI nem lê input.
- `EnemySpawner`: instancia os inimigos pedidos e conta vivos/pendentes. Não decide
  qual é a próxima onda. Sem um controlador de ondas, mantém o spawn contínuo anterior.
- `EnemyWave`: grupos, intervalo entre pares e limite de vivos de cada horda.
- `BossReinforcements`: agenda os reforços durante a luta, usa o mesmo spawner,
  limita os reforços vivos e os remove quando o boss para de lutar.
- `RunScore`: contabiliza abates e pontos, recebe o bônus e informa a HUD existente.
- `Player`: libera ou bloqueia movimento/combate pelo início/fim da partida. Seus sistemas
  continuam independentes do menu e da progressão de ondas.
- `WavesUI`: botão provisório, contador, resultado e animação dos avisos. Apenas apresenta
  os eventos do controlador. A animação não determina a progressão da partida.
- `Projectile`: informa quais layers pode atingir, permitindo limpar os tiros na transição.

As nove escolhas de cartas, suas raridades, reroll e bônus acumulados estão descritos
em `Docs/Cartas.md`. Loja, persistência e rede continuam fora desta etapa.

## Ajustes no Inspector

No GameObject **EnemySpawner**, o componente **WaveController** contém:

- **Wave Composition / Groups**: prefab e quantidade de cada grupo, na ordem de entrada.
  O número de ondas e os orçamentos vêm desta composição quando ela está preenchida.
- **Wave Composition / Spawn Interval e Max Alive**: ritmo e pressão de cada onda.
  Esses valores são aplicados ao entrar no combate da horda.
- **Enemies Per Wave**: configuração anterior usada somente quando `Wave Composition`
  está vazia. Mantém compatibilidade com as cenas que só usam o patrulheiro.
- **Announcement Duration**: espera antes do spawn, padrão 1,6 segundo.
- **Intermission Duration**: intervalo após a conclusão, padrão 2 segundos.
- **Reward Transition Duration**: espera após escolher a carta, padrão 0,35 segundo.
  Sem um controlador de recompensas, o intervalo anterior continua funcionando.
- **Wave Clear Bonus**: pontos por conclusão, padrão 1.000; zero desativa o bônus.
- **Player**: referência ao player da cena.

O componente **EnemySpawner** mantém a área de patrulha, alturas e distância entre
os pares. Seu intervalo e limite são usados como fallback quando não há composição.
No prefab **Player**, `PlayerCombat / Shot Interval` ajusta a cadência inicial.
No prefab **ExecutorV9**, `BossReinforcements` ajusta a espera inicial de 6 segundos
após a chegada, intervalo de 9–12 segundos, lote de dois e limite de dois reforços.

No prefab **WavesUI**, os tempos do aviso são ajustáveis: entrada 0,22 segundo,
leitura 0,65 segundo e saída 0,55 segundo. O texto entra pela direita, freia no centro,
permanece legível e sai pela esquerda acelerando e desaparecendo. As durações escolhidas
são um ponto de partida para experimentar o ritmo do jogo.

A entrada usa uma curva cúbica de desaceleração (`ease-out`) e a saída uma curva cúbica
de aceleração (`ease-in`). A [documentação de transições da Unity](https://docs.unity3d.com/6000.0/Documentation/Manual/UIE-Transitions.html)
descreve o papel dessas curvas no ritmo da animação. Aqui a apresentação usa o Canvas/uGUI
e TextMesh Pro já presentes no projeto; o cálculo equivalente fica numa coroutine,
sem dependência adicional de tween. A [CanvasGroup](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/CanvasGroup.html)
controla a opacidade, e o aviso não bloqueia cliques.

O EventSystem da cena usa **InputSystemUIInputModule**, compatível com o Input System atual.
O MainMenu inicia a partida na cena de gameplay; os novos inimigos não alteram essa ligação.
Veja `Docs/Inimigos.md` para os comportamentos e os ajustes dos prefabs.
