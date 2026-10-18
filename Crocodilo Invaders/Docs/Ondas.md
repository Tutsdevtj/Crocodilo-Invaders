# Progressão de dez ondas

O MainMenu passa pelo loading e inicia a partida após ENTER. Ao abrir diretamente
`Assets/Scenes/Tests/PlayerMovementTest.unity` ou `StageOneWaves.unity`, o botão
provisório **INICIAR** continua disponível. As duas cenas usam os mesmos pools.

São nove hordas e o boss na décima onda. O aviso e a HUD contam o boss no total,
sem criar uma décima horda antes dele.

Cada slot das nove hordas possui três opções configuradas. A tabela abaixo mostra
a **opção A**, que preserva integralmente a composição anterior. A opção B acrescenta
um patrulheiro; a C acrescenta um patrulheiro e uma unidade do tipo mais avançado
já presente naquele slot (patrulheiro nas ondas 1–2, interceptor nas 3–5 e artilheiro
nas 6–9). Intervalo, limite de vivos e ordem dos grupos originais são preservados.
As faixas de quantidade são, respectivamente: **4–6, 8–10, 12–14, 16–18, 20–22,
26–28, 32–34, 38–40, 44–46**. Não são geradas composições proceduralmente durante a run.

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
Conforme as opções sorteadas, as nove hordas contêm **200–218 inimigos** e rendem
**169.000–183.400 pontos**. O boss concede
40.000 pontos e um abate: a vitória rende **209.000–223.400 pontos e 201–219 abates**,
mais 800 pontos e um abate para cada interceptor de reforço realmente derrotado.
Veja `Docs/Boss.md` para os ataques e reforços.
Destruir um inimigo sem derrotá-lo não concede pontos por abate.

Na transição, os projéteis inimigos restantes são removidos para não causar dano
antes da recompensa. Não há mais aviso textual de onda concluída. Nas duas cenas configuradas, cada horda abre uma escolha
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
- `EnemyWavePool`: opções completas destinadas a um único slot. Rejeita pools
  sem opções ou com composições inválidas. O controlador sorteia uma opção por
  slot ao iniciar a run e guarda a sequência; não sorteia novamente durante a horda.
- `BossReinforcements`: agenda os reforços durante a luta, usa o mesmo spawner,
  limita os reforços vivos e os remove quando o boss para de lutar.
- `RunScore`: contabiliza abates e pontos, recebe o bônus e informa a HUD existente.
- `Player`: libera ou bloqueia movimento/combate pelo início/fim da partida. Seus sistemas
  continuam independentes do menu e da progressão de ondas.
- `WavesUI`: botão provisório, contador, resultado e ponte com o `WaveAnnouncer`. Segura o
  anúncio (`HoldAnnouncement`) até a apresentação "ONDA N" (1050 ms) ou ONDA 10 → PERIGO!
  GOOSINI (2500 ms) terminar; quem decide o próximo passo continua sendo o `WaveController`.
  `announcementDuration` só vale sem apresentação. Detalhes em `Docs/AnunciosOndasBoss.md`.
- `Projectile`: informa quais layers pode atingir, permitindo limpar os tiros na transição.

As nove escolhas de cartas, suas raridades, reroll e bônus acumulados estão descritos
em `Docs/Cartas.md`. Loja, persistência e rede continuam fora desta etapa.

## Ajustes no Inspector

No GameObject **EnemySpawner**, o componente **WaveController** contém:

- **Wave Pools / Options**: um elemento por horda, com uma ou mais composições
  completas destinadas somente àquela posição. Cada opção contém **Groups**,
  **Spawn Interval** e **Max Alive**. Para novas variantes, duplique uma opção e
  ajuste os grupos/valores dentro do limite de dificuldade desejado para o slot.
- **Use Debug Seed / Debug Seed**: reproduz a seleção da sequência de composições.
  Não controla posições de spawn, cartas, ataques nem os demais UnityEngine.Random.
  O sorteio é independente entre runs, sem histórico em disco; repetições são possíveis.
- **Wave Composition / Groups**: fallback da sequência fixa quando **Wave Pools**
  está vazio. As configurações anteriores permanecem disponíveis nas duas cenas.
- **Spawn Interval e Max Alive**: ritmo e pressão da opção escolhida.
  Esses valores são aplicados ao entrar no combate da horda.
- **Enemies Per Wave**: fallback quando ambos **Wave Pools** e **Wave Composition**
  estão vazios. Mantém compatibilidade com as cenas que só usam o patrulheiro.
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

O anúncio de cada onda é o banner arcade aprovado ("ONDA N" em PNG, 1050 ms, qualquer
número), e a onda do boss encadeia ONDA 10 → penas → PERIGO! GOOSINI (2500 ms). A curva,
a montagem dos algarismos e os tempos vêm do manifesto do pacote; não são ajustados no
prefab. O antigo texto TMP com entrada/leitura/saída em coroutine foi removido. Veja
`Docs/AnunciosOndasBoss.md`.

O EventSystem da cena usa **InputSystemUIInputModule**, compatível com o Input System atual.
O MainMenu inicia a partida na cena de gameplay; os novos inimigos não alteram essa ligação.
Veja `Docs/Inimigos.md` para os comportamentos e os ajustes dos prefabs.
