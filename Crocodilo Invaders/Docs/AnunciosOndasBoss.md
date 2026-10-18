# Anúncio de ondas e alerta PERIGO! GOOSINI

Apresentação definitiva das hordas ("ONDA N") e do boss ("PERIGO! GOOSINI"), integrada em
18/10/2026 a partir de `Wave_Boss_UI_Goosini.zip` (arte, manifesto e previews aprovados) e
`Crocodilo_Invaders_Goosini_Boss_Alert.zip` (WAV final do alerta, gerado no ElevenLabs).
Substitui por completo o banner TMP antigo do `WavesUI`; não há dois sistemas convivendo.

Fonte de verdade: `SourceArt/WaveBossUI/wave_boss_ui.json` e `WAVE_AND_BOSS_UI_GUIDE.md`.
Previews de referência (somente documentação): `Docs/ArtPreviews/WaveBossUI`.

## Componentes

Tudo vive no `Assets/Prefabs/UI/WavesUI.prefab` (Canvas overlay 1200 × 720, match 0,5), em
`Announcements`:

| Objeto / classe | Papel |
| --- | --- |
| `Announcements` — `WaveAnnouncer` | Relógio único da sequência (tempo escalado). Toca "ONDA N" e, se houver tema de boss, transição + alerta. Pulo, cancelamento, fila e callback único. |
| `Wave` — `WaveBanner` | Monta "ONDA N" com os PNGs; só avalia o tempo que o diretor passa. |
| `WaveBannerStyle` (`Assets/Data/UI/WaveBannerStyle.asset`) | Sprites, métricas dos algarismos, kerning dos 100 pares, montagem e curva de 1050 ms, importados do JSON. Contém `Compose()` (layout puro, sem alocação). |
| `BossAlert` — `BossAlertBanner` + `AudioSource` | Peças genéricas (faixa, PERIGO!, impacto, speedlines, tempos) e posições do manifesto. Recebe o conteúdo do boss por tema. Toca o SFX do tema. |
| `BossAlert/Particles` — `BannerParticleLayer` | Pool de 20 imagens de pena (mais 4 da transição). Depois de nascer, cada pena fica no lugar da tela, deriva 0,18/0,06 px/ms, gira a cada 66 ms e se dissolve por troca de sprite (62% e 82% da vida de 260 ms). |
| `BossAlertTheme` (`Assets/Data/UI/GoosiniBossAlert.asset`) | Exclusivo do Goosini: título + flash, pena longa, cachecol (4 quadros/90 ms), ponta-bico, 4 penas com dissoluções, WAV, volume e offset do som. |
| `BannerMotion` | Trechos de animação (`out_cubic`, `in_cubic`, linear), velocidade, arredondamento para px par; `BannerSpeedlines` (s/m/l × 2 quadros); `BannerPieces` (imagens top-left × 2). |
| `Boss.alertTheme` | Cada boss aponta o seu tema. `Gusini.prefab` → `GoosiniBossAlert`. `ExecutorV9` sem tema: só "ONDA 10". |

`WavesUI.cs` continua com o que não é banner: painel PRONTO?/VITÓRIA/FIM DE JOGO, botão
INICIAR provisório, retorno ao menu e som de vitória/derrota. O contador TMP
"ONDA x / y" (e "• BOSS") foi removido em 18/10/2026 a pedido do usuário,
incluindo o objeto WaveCounter e sua atualização em WavesUI.
Também é a ponte com o `WaveController` (segura e libera o anúncio).

## Montagem dinâmica do número

Coordenadas nativas, origem no canto superior esquerdo do banner; tela = nativo × 2.

```text
x0     = digits[d0].first_x
x(i+1) = x(i) + kerning[d(i) d(i+1)]
L      = max(150, x_last + digits[d_last].bar_end_offset − 14)
faixa  = head em 14; mid esticado de 152 até 14+L−12; end em 14+L−12 (y = 24)
burst  = (x_last + burst_offset.x, 14 + burst_offset.y)
centro = (bbox_min + bbox_max) / 2      X_banner = 600 − centro·2 + x(t), arredondado para par
```

Qualquer inteiro positivo funciona (testados 1, 7, 9, 10, 15, 99, 100 e 1234). Só a coluna
central da faixa estica; os algarismos nunca são comprimidos. O pool começa com três
algarismos e cresce uma única vez se aparecer um número maior. Os exemplos do JSON
(1–10, 11, 15, 99) são recalculados pela validação de Editor e devem bater exatamente.

Ênfase: 180–213 ms `_flash`, 213–246 ms `_pop1`, depois base, com `variant_offset` (não
há escala de bitmap). `pop2` não é usado pelo guia e não foi importado. O estalo aparece
em 180 ms. Speedlines: comprimento `min(130, |v|·10)`, `s` < 60, `m` < 105, senão `l`;
escondidas abaixo de 6; quadro `_0`/`_1` a cada 33 ms.

Curva (1050 ms): +760→+24 (0–180, out) · +24→−8 (180–230, out) · −8→0 (230–280, out) ·
0→−16 (280–800, linear) · −16→−8 (800–840, out) · −8→−1000 (840–1050, in).

## Sequência ONDA 10 → GOOSINI (2500 ms)

| Sequência | O que aparece |
| --- | --- |
| 0–1050 | "ONDA 10" normal |
| 1050 | WAV começa (−150 ms do banner do boss) |
| 1050–1200 (+80 de cauda) | 4 penas atravessam a tela (in_cubic, 140 ms, defasagens 0/30/55/75, y 190/262/150/310) |
| 1200–2500 | Banner PERIGO! GOOSINI (1300 ms) |
| 2500 | Callback → `WaveController` libera `BeginBoss` → entrada existente → diálogo → combate |

Banner do boss: +820→+28 (0–200) · +28→−10 (200–260) · −10→0 (260–320) · 0→−20 (320–1050,
linear) · −20→−12 (1050–1090) · −12→−1100 (1090–1300, in). Título com flash 200–233 ms,
impacto 200–300 ms. PERIGO!: oculto até 200, flash, âmbar (233), vermelho (266), duas
piscadas âmbar (420–480, 540–600) e fixo em vermelho. Cachecol em loop do início ao fim.
Penas: uma a cada 33 ms na entrada (0–200), quatro de uma vez em 1090 e uma a cada 30 ms
até 1300; mesma tabela de posições das previews. Ordem de desenho: speedlines, penas,
cachecol, faixa, bico, pena longa, título, PERIGO!, impacto.

Divergência registrada: o guia cita y 150/262/150/310 para as penas da transição; as previews
aprovadas foram geradas com 190/262/150/310 (`tools/anim_all.py`). Usamos o valor das previews.

## Áudio e sincronização

`Assets/Audio/Boss/BossAlert/goosini_boss_alert_final.wav` (B1): PCM 16-bit, 48 kHz,
estéreo, 2,000 s, sem loop, SHA-256 igual ao do pacote. Importação: PCM, Decompress On Load,
preload, sem forçar mono. Candidatos A/B/C, originais e prévias não foram importados;
metadados, prompts, créditos e timestamps em `SourceArt/GoosiniBossAlert`.

- O banner bate em +200 ms e começa em 1200 → impacto visual em **1400 ms**.
- O pico grave medido do WAV (`impact_peak`) é 349 ms. Iniciando em **1050 ms**
  (`alertSoundOffsetMs = −150`), o pico cai em **1399 ms**. A parte ativa termina perto de
  2479 ms; a cauda atenuada vai até 3050 ms e não é cortada no fim normal.
- Um único `Play()` por anúncio, no instante em que o relógio cruza o offset. Se o quadro
  cair alguns ms depois, o som começa no sample correspondente (sem pitch-shift/stretch).
- Relógio único: a animação usa tempo escalado; enquanto o WAV toca, se a diferença para o
  relógio de áudio passar de 50 ms (travada de frame), a animação é realinhada ao áudio.
- `AudioSource` 2D no próprio `BossAlert`, roteado ao grupo **Effects** do
  `GameAudio.mixer` (`GameAudioSettings.Route`), portanto obedece ao volume de efeitos e ao
  geral. Respeita `AudioListener.pause`. Nenhum gerenciador de áudio novo.
- **Volume:** `GoosiniBossAlert.asset → Alert Volume` (0,70). Offset: `Alert Sound Offset Ms`.

Música: `WaveMusic` já faz fade de 0,9 s no anúncio da onda do boss (agora detectada por
`WaveController.IsBossWave`), então a trilha das ondas está em silêncio antes de o alerta
começar em 1050 ms, e para de vez em `BossStarted`. A música do Gusini continua começando só
quando o combate é liberado; transformação, diálogos e F2 não foram tocados.

## Integração com o WaveController

- `AnnounceNextWave` gera um id de anúncio. `WavesUI` chama `HoldAnnouncement()` dentro de
  `WaveAnnounced` e `ReleaseAnnouncement(token)` quando o `WaveAnnouncer` conclui.
  Enquanto houver hold, a fase `Announcing` não termina; tokens de anúncios antigos são
  ignorados.
- `announcementDuration` (1,6 s) vale só quando nenhuma apresentação segura o anúncio
  (cena sem `WavesUI`). Com a UI, o combate começa ao fim dos 1050 ms; o boss, ao fim dos 2500.
- `IsBossWave(wave)` = há `bossPrefab` e a onda passa das hordas configuradas. Não há
  `wave == 10` espalhado; o tema vem de `BossPrefab.AlertTheme`.
- A apresentação é acionada por `WaveAnnounced`; WavesUI não assina BossStarted
  para atualizar contadores. Os anúncios visuais de ondas e boss permanecem.
- `StartBossForDevelopment` (GusiniBossTest, só Editor) agora passa pelo mesmo anúncio:
  começa a run já com as hordas concluídas, mostra ONDA 10 → GOOSINI e então `BeginBoss`.
  O kit de teste do `BossDevelopmentStart` continua aplicado.
- Nada nasce durante os 2500 ms: o spawner não recebe onda e o boss não é instanciado.
- Se a UI for desativada ou destruída, `WavesUI.OnDisable` cancela e libera o hold, para a run não
  travar esperando uma apresentação que não existe mais.

## Pausa, pulo e cancelamento

- Pausa (`PauseController`): `timeScale = 0` congela relógio, penas e fade; `AudioListener.pause`
  congela o WAV. Ao retomar, continuam sincronizados.
- `WaveAnnouncer.Skip()` (API; não há tecla ligada a ele nesta etapa): onda antes de 800 ms
  salta para 800; boss antes de 1050 salta para 1050; pular durante a onda do boss mostra só
  a saída do boss, sem transição nem som. O som já iniciado faz fade junto com a saída. Penas
  vivas terminam a vida; nenhuma nova nasce. O callback continua único.
- Novo `Play()` com outro anúncio tocando: o atual pula para a saída e a comprime para
  120 ms; só o callback do novo é chamado.
- `Cancel()` (fim de run, disable, troca de cena): esconde banners, limpa penas, para o som e
  descarta o callback.

## Novo boss

1. Desenhar o título com o sistema de letras do guia (§7; `Tools/Art/WaveBossUI/boss_ui.py`).
2. Criar um `BossAlertTheme` (Create → Crocodilo → UI → Boss Alert Theme): título, flash,
   `bandLength` = largura do título + 26, anexos nas âncoras (frente, acima/direita, trilha
   no fim da faixa), partículas próprias e o SFX com volume/offset.
3. Para outra cor, gerar a faixa recolorida (palette swap de `SourceArt/WaveBossUI/boss_band_palette_goosini.png`,
   contorno navy fixo) e preencher `bandHead/Mid/End` do tema; vazio usa a faixa genérica.
4. Atribuir o tema em `Boss → Alert Theme` do prefab do boss. Tempos, PERIGO!, impacto e
   speedlines são compartilhados.

## Removido

- `WavesUI`: `bannerText`, `bannerGroup`, `enterDuration`, `holdDuration`, `exitDuration`,
  `ShowBanner`, `AnimateBanner`, `SetBannerX`, `animationRoutine`, o banner textual de
  `ShowWave`, o nome cru do boss em `ShowBoss` e o aviso "ONDA N CONCLUÍDA / +pontos"
  (`ShowCleared`, sem arte aprovada; bônus, cartas e transição seguem iguais).
- `WavesUI.prefab`: objeto `WaveBanner` (TextMeshPro + CanvasGroup). Nenhum asset antigo foi
  apagado: o banner só usava a fonte/material TMP compartilhados com o painel e outros menus.

## Ferramentas e testes

- `Crocodilo/UI/Install wave and boss announcements` (`WaveBossUITools.Install`, mutação
  idempotente): importação Point/sem compressão/sem mips/PPU 50/pivô top-left, WAV, assets de
  dados a partir do JSON, hierarquia do `WavesUI.prefab` e tema no `Gusini.prefab`.
- `Crocodilo/UI/Validate wave and boss announcements`: 321 verificações (importação, exemplos
  do JSON, kerning, curvas, linha do PERIGO!, total de 2500 ms, sincronismo calculado, prefab,
  referências, ausência do legado, três cenas).
- `Crocodilo/UI/Validate announcements in Play Mode` (`WaveBossUIPlayValidation`): 128
  verificações em Play Mode real e capturas em `Logs/WaveBossUI` (1200 × 720, 1920 × 1080,
  800 × 600). Cobre números, poses, speedlines, sequência sem sobreposição, PERIGO!/cachecol/
  título/impacto, penas no espaço da tela, pulo, cancelamento, fila, pausa com WAV, onda 1 real
  (combate só depois dos 1050 ms), salto real à onda do boss com a música das ondas já em
  silêncio no alerta, derrota, e GusiniBossTest (som em 1050 ms, um único som, deriva máxima
  medida de 28–39 ms, entrada → diálogo → música F1). Ranking e progresso em pasta temporária.
- Regressão: `DialoguePlayValidation` (168) e `CrocodiloWindowsBuild.ValidateProject` passaram.

Pendências: escuta crítica do WAV sobre a música e em run completa (nenhum agente ouviu o
áudio); avaliação visual numa run manual inteira; em 1920 × 1080 o CanvasScaler usa escala
1,6 (não inteira), como as demais HUDs, então o pixel 2× não é perfeitamente uniforme ali.
