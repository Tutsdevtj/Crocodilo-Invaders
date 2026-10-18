# Cenas narrativas e diálogos

Implementação de 18/10/2026. Três cenas curtas, feitas na própria Unity, sem vídeo:
abertura da run, encontro com Gusini e falsa derrota antes da Fase 2. A lógica de
jogo continua sendo a fonte de verdade: o sistema de cenas só segura o início de
estados existentes e espera os sinais deles.

## Estrutura

| Peça | Responsabilidade |
| --- | --- |
| `Scripts/Entities/DialogueCast.cs` | Falantes: id, nome, cor da tarja, lado, flip, pitch do blip e expressões (id → sprite) |
| `Scripts/Entities/DialogueSequence.cs` | Roteiro: falas, pausas, sons opcionais, efeito de interferência, espera de abertura/fechamento |
| `Scripts/Presentation/DialogueRunner.cs` | Caixa "Rádio de Piloto": abertura, nome, retratos, expressões, digitação, avanço, skip, encerramento e callback |
| `Scripts/Presentation/DialogueBlipAudio.cs` | Som das letras com limite de vozes e intervalo |
| `Scripts/Controllers/RunCutscenes.cs` | Orquestrador fino das três cenas |
| `Scripts/Presentation/GusiniMusic.cs` | Música F1/F2 do boss, uma fonte só |
| `Assets/Prefabs/UI/Cinematics.prefab` | Canvas 1200×720 (match 0,5, ordem 250: acima de HUD/cartas, abaixo da pausa) com runner, blips e orquestrador |
| `Assets/Data/Dialogue/*.asset` | Elenco e as três sequências |
| `Editor/DialogueTools.cs` | Importação, fonte TMP, dados, prefab, música no Gusini e ligação das cenas |
| `Editor/DialoguePlayValidation.cs` | Testes de Play Mode e capturas |

Integrações mínimas nos sistemas existentes:

- `WaveController`: fase nova `Intro` (no fim do enum), campo opcional `cutscenes`. `StartRun` pede a abertura e só anuncia a onda 1 quando ela termina ou é pulada. `StartBossForDevelopment` não toca a abertura. `BeginBoss` chama `RunCutscenes.PrepareBoss` antes de `BeginBattle`.
- `Boss`: `HoldCombat(bool)`/`CombatHeld`. `Ready` fica falso enquanto segurado; a entrada continua igual e, ao chegar, o boss segura a posição pela API existente de `EnemyMovement`. Sem hitbox, invulnerável, `GusiniCombat` espera `Ready` como antes.
- `GusiniPhaseTwo`: evento `ApparentDefeatHeld`, disparado pelo Animation Event nativo `apparent_defeat_hold` (690 ms) do clip `P2Transform`.
- `HealthPickupSpawner`: não abre caixa nova enquanto o combate do boss está segurado pelo diálogo (o player está sem controle).

Não houve mudança em cadência, padrões de ataque, HP, dano, dash, upgrades, ondas ou arte.

## As três cenas

**1. Abertura (`RunIntro.asset`)** — `WaveController.StartRun` → `RunCutscenes.PlayRunIntro`.
Fase `Intro`, player sem movimento/tiro/dash. 0,9 s de cenário, estática de rádio
antes da primeira fala e antes da segunda (sem resposta da Central), silêncio
antes da terceira. Ao fechar: controles liberados e anúncio normal da onda 1.
Uma vez por run nova (flag no orquestrador + `Phase == Waiting`); anúncios de
onda não reabrem.

**2. Encontro (`GusiniBossIntro.asset`)** — `WaveController.BeginBoss` → `PrepareBoss`.
Combate segurado e player bloqueado desde o spawn. A entrada original acontece;
o diálogo só abre quando `Movement.HasArrived`, mais 0,5 s. Dez falas, retrato do
Gusini surge na primeira fala dele. Depois da última, 0,4 s e a caixa fecha;
`HoldCombat(false)` libera `GusiniCombat` (delay inicial e primeiro Burst normais).

**3. Falsa derrota (`GusiniPhaseTransition.asset`)** — ao zerar a F1, a
transformação existente começa (`GusiniPhaseTwo.OnDepleted`: limpa ameaças,
barra F1 vazia, golpe final e stagger nativos). No evento `apparent_defeat_hold`
o orquestrador congela o jogo (`Time.timeScale = 0`) exatamente na pose de falsa
derrota. Pausa dramática de 0,9 s, seis falas e, antes da última, interferência:
estática, tremor da caixa, o retrato aprovado `corrupted_furious` pisca sobre o
hostil e toca o master `P2FirstPulse` já existente. Ao fechar, o tempo volta e o
mesmo clip continua de 690 ms: primeiro sinal, corrupção, pico, revelação em
2,82 s (barra F2), fim em 4,1 s, quando a IA da F2 e a música da F2 começam.
Nenhuma transformação paralela foi criada.

Pular qualquer cena chama o mesmo encerramento da conclusão normal.

## Encerramento do capítulo 1 (18/10/2026)

Substitui o antigo final de vitória. Fluxo:
`BossDefeated → AlienRecovery → FinalDialogue → RadioTransmission → Farewell → SceneTransition`
(`RunCutscenes.FinaleStage`, evento `FinaleStageChanged`).

1. **F2 zera** → `Health.Depleted` → `GusiniPhaseTwo` passa para `Stage.Defeated` (cancela coroutines, ameaças, núcleo, multiplicador e áudio, como o `Stop` antigo, sem o fade) e dispara `PhaseTwoDefeated`. Sem ouvinte, a derrota antiga continua igual (Executor e testes isolados).
2. **`GusiniDefeat`** (no `Gusini.prefab`) chama `Boss.RetainAfterDefeat()` — o `Boss` ainda emite `Defeated`/pontos, mas não cai nem é destruído — troca para `GusiniDefeat.controller` e toca `P2Defeat` (1,9 s, 18 quadros do pacote v2, durações e eventos do JSON). Eventos: `defeat_hit` (tremor 0,06/120 ms + master `P2FinalHit`), `stagger_start` (`P2EngineStall`), `parasite_burst` (tremor 0,04/150 ms + `P2EggBurst`, `ParasiteBurst`), `clip_end` → loop `P1DefeatedIdle` (4 × 120 ms) e `RecoveryCompleted`. O último quadro e o loop são os da nave **avariada**, com cratera e resíduo verde morto; nenhum sprite limpo da F1 é usado.
3. **`WaveController.OnBossDefeated`**: pontos e limpeza de projéteis como antes; para boss retido, bloqueia o player e chama `RunCutscenes.PlayBossFinale`. O timer antigo (`EndRun(true)` após `DefeatDuration`) não roda para ele.
4. **`RunCutscenes.PlayBossFinale`** espera `RecoveryCompleted` (sem delay fixo) e toca `GusiniFinale.asset` (20 falas). A música F2 já terminou no fade de 1,2 s do `BattleStopped`; `openDelay` 1,3 s é o silêncio da dúvida. Marcadores `beat` nas falas avançam para `RadioTransmission` e `Farewell`.
5. Fim do diálogo (ou skip) → `WaveController.CompleteChapter`: fase `ChapterCompleted`, combate parado, `LeaderboardSession.Complete(..., true)` e evento `ChapterCompleted`. **Não chama `RunEnded(true)`**, então o painel "VITÓRIA"/voltar ao menu do `WavesUI` não aparece.
6. **`ChapterFlow`** registra os marcos do `Chapter1.asset` uma única vez em `CampaignProgress` e pede `SceneTransition.Go(nextScene)`.
7. **`SceneTransition`** (overlay próprio, ordem 260, abaixo da pausa): cancela o diálogo, fade-out de 1,2 s em tempo não escalado, salva o resultado pendente do ranking e:
   - se a cena estiver no Build Settings (`Application.CanStreamedLevelBeLoaded`): `LoadSceneAsync`, uma vez;
   - senão: estado `DestinationUnavailable`, sem chamar o SceneManager, com o aviso de desenvolvimento "FIM DO CAPÍTULO 1 — BASE IMPROVISADA EM DESENVOLVIMENTO" e "Esc: menu de pausa". A pausa e o "Sair para o menu" continuam funcionando.

Roteiro (`Assets/Data/Dialogue/GusiniFinale.asset`, editável no Inspector):

| # | Falante | Expressão | Observação |
| --- | --- | --- | --- |
| 1 | Croc | worried | |
| 2 | Gusini | vulnerable | exausto e confuso; pausa 0,7 s antes |
| 3 | Croc | neutral | aliviado, brincando — o master neutro é "confiante e marrento" |
| 4 | Gusini | vulnerable | abalado |
| 5 | Croc | serious | |
| 6 | Gusini | smug | "Heh..." sincero, sorriso cansado |
| 7 | Croc | neutral | provocando |
| 8 | Gusini | vulnerable | pausa 0,3 s |
| 9 | RÁDIO | — | estática 1 s antes + interferência (sem vislumbre); `beat: radio` |
| 10 | Croc | alert | |
| 11 | RÁDIO | — | |
| 12 | Croc | worried | |
| 13–14 | RÁDIO | — | |
| 15 | Croc | determined | |
| 16 | Gusini | vulnerable | inseguro; estática 0,9 s antes (sinal encerra); `beat: farewell` |
| 17 | Croc | neutral | provocando |
| 18 | Gusini | resentful | indignado — mais próximo aprovado; `hostile` lembraria o confronto |
| 19 | Croc | neutral | debochado |
| 20 | Gusini | smug | sorrindo, cansado |

Só retratos normais da F1 depois do parasita; o `corrupted_furious` não aparece.

**Speaker RÁDIO** (`DialogueCast`, id `radio`): `Side.None` — nenhum retrato; quem está em cena só escurece; a placa fica à esquerda com tarja cinza `#C8C8C8` e pitch de blip 0,82. A placa usa a Pixelify 22 px via `nameFont` do falante, porque a Press Start 2P desenha o acento do Á com 1 px ("RADIO"). Reutilizável em qualquer sequência; a estática usa o indicador de rádio existente. Não havia SFX de chiado adequado, então nenhum foi adicionado.

**Conectar a BaseImprovisada quando existir:** crie a cena e adicione-a ao Build Settings com o nome configurado em `Assets/Data/Flow/Chapter1.asset` (`nextScene`, hoje `BaseImprovisada`). Nada mais muda: `SceneTransition` passa a carregá-la no lugar do aviso. `CrocodiloWindowsBuild` exige MainMenu e PlayerMovementTest nas duas primeiras posições; acrescente a nova depois delas.

**Estado narrativo:** `CampaignProgress` (`Scripts/Persistence/CampaignProgress.cs`) guarda marcos em `campaign.json` no `persistentDataPath`, versionado, com temporário e `.bak`, como o ranking. `Chapter1.asset` registra `gusini_rescued` e `chapter1_completed`; `Record` é idempotente e dispara `FlagRecorded`. O futuro HUB consulta `CampaignProgress.Has("gusini_rescued")`. Testes usam `CampaignProgress.UseDirectory` numa pasta temporária.

**Fluxo antigo de vitória:** não foi apagado, foi desacoplado do Gusini. `EndRun(true)`/`RunEnded(true)` e o painel do `WavesUI` continuam para bosses não retidos (Executor V-9) e para a derrota do player ("FIM DE JOGO"), que compartilham esse painel.

## Falas, expressões e retratos

Edite no Inspector, sem código:

- `Assets/Data/Dialogue/RunIntro.asset`, `GusiniBossIntro.asset`, `GusiniPhaseTransition.asset`: texto, `speakerId`, `speakerName` (vazio = nome do elenco), `expressionId`, `pauseBefore`/`pauseStyle` (Silent ou RadioStatic), `pauseAfter`, `soundOverride` (troca o blip daquela fala), `startSound`, `effect` (Interference + retrato do vislumbre). `openDelay`/`closeDelay` por cena.
- `DialogueSequence.Validate` recusa falante/expressão inexistentes e texto vazio. O instalador só cria os assets que faltam; não sobrescreve edições.
- Rich text do TMP funciona: `maxVisibleCharacters` conta caracteres visíveis, acentos inclusos. Para novos caracteres fora do conjunto Latin-1/pontuação, adicione-os ao atlas estático da fonte.

Mapeamento usado (`DialogueCast.asset`):

| Pedido | ID | Arte aprovada |
| --- | --- | --- |
| Croc alerta / surpreso | `alert` | `crocodilo_01_surpreso_indignado` |
| Croc sério / firme | `serious` | `crocodilo_02_serio_autoritario` |
| Croc preocupado | `worried` | `crocodilo_03_confuso_preocupado` |
| Croc sincero | `sincere` | `crocodilo_04_sincero_magoado` |
| Croc determinado | `determined` | `crocodilo_05_determinado` |
| Gusini debochado; cansado disfarçando a derrota ("Heh... vai me buscar de novo?") | `smug` | `gusini_p1_smug` — escolhido por ser um sorriso forçado; `vulnerable` era a alternativa |
| Gusini ressentido / amargo | `resentful` | `gusini_p1_resentful` |
| Gusini arrogante ("não preciso mais ser salvo") | `power` | `gusini_p1_power` |
| Gusini assustado / vulnerável | `vulnerable` | `gusini_p1_vulnerable` |
| Gusini hostil | `hostile` | `gusini_p1_hostile` |
| Gusini Corrompido | `corrupted_furious` | `gusini_p2_corrupted_furious` (único da F2; tarja `#7CEC48`) |

O neutro `crocodilo_master_neutral` também está no elenco, sem uso nas cenas atuais.
O roteiro do JSON do pacote (`SourceArt/Dialogue/dialogue_scene_pre_boss_gusini.json`)
era uma reconstrução provisória; valeu o roteiro do pedido.

Posições ficam no prefab: `Content/PortraitLeft` e `PortraitRight` (320×320 = 2× de
160, pivô base) e `Content/DialogueUI` (painel 720×140, 32 px da base). Os retratos
desenham **na frente** do painel, ancorados ao centro-inferior a ±480 px (120 px da
borda em 1200×720), para a sobreposição ser igual em 4:3 e 16:9; por isso o texto tem
78 px de margem lateral, a placa de nome/rádio recuam 64 px e a seta de continuar fica
a 64 px da direita. Esses valores ficam em `DialogueTools.ApplyLayout`. O lado de
cada falante vem do elenco; a placa de nome segue o lado do falante e o rádio vai
ao lado oposto. Quem não fala fica escurecido (`inactivePortrait`).

## Interface, fonte e importação

- Sprites da UI em `Assets/Art/UI/Dialogue`: PPU 50 (1 px de arte = 2 px de UI), Point, sem compressão/mipmaps, Full Rect, bordas 9-slice do guia. Retratos em `Assets/Art/Portraits`: 160×160, PPU 100, pivô (0,5; 0).
- Fala: Pixelify Sans 22 px, raster hinted, atlas estático Point (`PixelifySans Dialogue.asset`), entrelinha +16, kerning desligado, cor `#F6ECCC`. Nome: Press Start 2P 16 px já existente.
- Entrada 240 ms (painel sobe 24 px), saída 140 ms, realce de troca de falante 250 ms, pop da placa 120 ms, rádio 120 ms/quadro com clique de 80 ms, seta de continuar 140 ms/quadro. Posições animadas arredondadas para pares.
- Fundo levemente escurecido (12%) que também bloqueia cliques na UI de jogo.

## Entrada

- **Enter, Enter do teclado numérico, clique esquerdo ou A/Cruz do controle**: enquanto digita, revela a fala inteira em silêncio e permanece nela; com a fala completa, avança; na última, fecha e notifica a cena. Só pressionamento (`WasPressedThisFrame`), uma ação por frame: um clique nunca completa e avança junto.
- **Skip**: segurar **Tab** (ou Select) por 0,5 s; dica "SEGURE TAB: PULAR" no topo. `skipMode` no `DialogueRunner`: Disabled, Press ou Hold; duração e rótulos configuráveis.
- Pausa (Esc/Start) funciona sobre qualquer cena, inclusive a congelada (`AllowPauseWhileFrozen`). Com a pausa aberta, digitação, timers e input do diálogo param; o ENTER/clique que fecha a pausa é descartado.
- `EventSystem` perde a seleção ao abrir, e o player fica com `SetGameplayActive(false)`.

## Áudio

- Letras: `SFX_Dialogo_TextBlip_05.wav` (variante escolhida no pacote do usuário), 2 vozes, um blip a cada 2 letras, intervalo mínimo 70 ms, volume 0,35, jitter de pitch 3%, pitch por falante (Croc 0,92; Gusini 1,08; Corrompido 0,86). Espaços nunca tocam; pontuação com `punctuationChance` 0. Revelar a fala inteira não toca as letras restantes. Para ao trocar de fala, fechar, pular, cancelar ou trocar de cena. Roteado a Effects.
- Música (`GusiniMusic` no `Gusini.prefab`, fonte própria em raiz da cena, fora do pool de 10 vozes do `GusiniAudio`, roteada a Music, streaming Vorbis 0,7):
  - F1 `MUS_Gusini_Phase1_Loop.wav`, volume 0,28, começa quando o combate é liberado (depois do encontro), fade-in 0,6 s.
  - Ao zerar a F1: fade-out 0,35 s, antes do quadro de falsa derrota; silêncio durante o diálogo.
  - F2 `MUS_Gusini_Phase2_Loop.wav`, volume 0,24, no `PhaseTwoStarted` (4,1 s), uma vez.
  - Derrota do boss/morte do player/fim da run: fade-out 1,2 s. Pausa congela via `AudioListener.pause`.

Não foram encontrados assets de explosão distante adequados para a abertura; a cena usa só a estática do rádio. Os SFX de combate não mudaram.

## Tempos configuráveis

- `DialogueRunner`: `characterDelay` 0,035 s, `sentencePause` 0,12 s, `commaPause` 0,06 s, `textDelay` 0,08 s, entrada/saída, entrada do retrato, realce, interferência 0,48 s, skip.
- Sequências: `openDelay`/`closeDelay` e `pauseBefore`/`pauseAfter` de cada fala.
- `GusiniMusic`: volumes e fades.
- `RunCutscenes`: liga/desliga cada cena (`playRunIntro` está desligado em `GusiniBossTest`).

## Ferramentas e testes

- `Crocodilo/Dialogue/Install cinematics` (`DialogueTools.Install`): mutação; configura importação, cria fonte/dados que faltam, cria o prefab só se faltar, ajusta a dica de skip, adiciona `GusiniMusic` e liga `PlayerMovementTest` e `GusiniBossTest`.
- `Crocodilo/Dialogue/Validate cinematics`: importação, glifos, elenco, sequências, prefab, música.
- `Crocodilo/Dialogue/Validate cinematics in Play Mode`: 168 verificações com teclado/mouse virtuais do Input System nas cenas reais (inclui o encerramento do capítulo).
- `Crocodilo/Boss/Install Gusini defeat (F2 → recovered)` (mutação; também chamada pelo instalador acima) e `Validate Gusini defeat`.
- `Crocodilo/Dialogue/Capture dialogue previews`: PNGs em `Logs/Dialogue` (ignorado pelo Git).

Resultado na Unity 6000.6.4f1 (batch, 18/10/2026): validação estática e 74 verificações
de Play Mode passaram — abertura uma vez por run, player bloqueado, Space sem tiro,
Enter/clique revelam sem avançar, segunda pressão avança uma fala, blips limitados
(19 para 49 letras na digitação natural, nenhum extra ao revelar), pausa congela e
não vaza input, entrada do Gusini antes da conversa, nenhum ataque durante o diálogo,
expressões corretas em todas as dez falas, música F1 só após liberar, diálogo da falsa
derrota no quadro nativo de 690 ms antes da revelação, pausa sobre a cena congelada,
vislumbre do retrato corrompido, skip descongelando e parando sons, transformação
completa, uma única faixa na F2, HP 320 preservado, sem repetição de cenas e skip da
abertura chegando à onda 1. Também passaram: `CrocodiloWindowsBuild.ValidateProject`,
`GusiniPhaseTwoTools.ValidateIntegration` e as validações de Play Mode da F2
(41 de combate, 83 com áudio). Capturas inspecionadas em 1200×720, 1920×1080 e 800×600.

Encerramento do capítulo (18/10/2026, mesma bateria, 168 no total): F1 zerada não inicia o final; F2 zerada passa à derrota nativa, Gusini fica em cena, ameaças somem, player bloqueado, boss não toma dano; diálogo só depois de `clip_end`, nunca no estouro; loop avariado `gusini_p1_defeated_idle` sem fade; música F2 encerrada; 18 falas conferidas (retrato/expressão, RÁDIO sem retrato, nenhum retrato corrompido, estágios do final, Enter e clique alternados, uma ação por entrada) e skip na 19ª; `ChapterCompleted` sem `RunEnded`; dois marcos gravados uma vez em pasta isolada; fade para `BaseImprovisada`; segunda transição recusada; cena ausente não é carregada nem gera exceção; nenhuma onda, ataque ou vitória depois. `GusiniPhaseTwoPlayValidation` (41 e 83) teve duas asserções antigas atualizadas para a nova derrota (estágio `Defeated` e o boss retido no teste de unload). Capturas: `finale_recovered`, `finale_radio`, `chapter_end_fallback`.

## Pendências reais
- Cena `BaseImprovisada` ainda não existe; o final mostra o aviso de desenvolvimento. Conferir a escolha de expressões (`neutral` para o Croc provocando, `resentful` para "Talvez?!") e os três masters reutilizados na derrota ouvindo em jogo.

- Escuta crítica em jogo: volume das músicas contra os SFX, emenda dos loops, timbre/frequência do blip. Nada foi ouvido pelo agente.
- Jogar uma run manual completa com a pausa dramática e o ritmo das falas.
- A fonte é a instância padrão (Regular, wght 400) da Pixelify variável; o guia sugere Medium (500). Gerar uma instância estática Medium (ex.: fontTools) e trocar o arquivo-fonte do atlas, se o Regular parecer fino.
- Em 1920×1080 a escala 1,6 do Canvas deixa a fonte raster com pixels irregulares; o mesmo vale para as HUDs atuais.
- Durante o congelamento da falsa derrota a barra da F1 fica parada onde estava (atrás do retrato) e a caixa de cura da transição também espera; ambas seguem depois.
