**Cartas da nave e áudio original v25 — 22/10/2026:** coleção ativa v21 de 25 sistemas instalados na nave, sem Croc nas frentes; metadados e catálogo preservados. Fontes aprovadas em SourceArt/CardCollection_v21_Integration; exportador Tools/Art/apply_ship_cards_v21.cjs. Histórico rastreado movido para Backup/Cartas, fora da Unity. Efeitos externos graduais e nove sons originais, sem pacotes de áudio externos. Lendárias/míticas têm somente cintilação irregular sem música, uma fonte por oferta; ganhos de revelação divididos pelo número de cartas altas. Receita ativa Tools/Audio/prepare_card_audio_v25.py (NumPy+wave; usa o gerador original v24 para os demais sons), manifesto completo SourceArt/CardAudio_v25_Sparkle. Play Mode 122 verificações/errors=0, build Windows 6000.6.4f1 e startup8s aprovados. Fotos atuais Docs/ArtPreviews/CardReview; vídeo real com três/duas míticas Docs/ArtPreviews/CardAudio_v25/brilho-sem-musica-volume-controlado.mp4. Docs atuais CartasIntegracao_v21.md, AudioCartasBrilho_v25.md e BuildWindows.md. RAR local ignorado em Builds/Pacotes/CrocodiloInvaders-DEV-BrilhoSemMusica-v25-Windows.rar. Atualização publicada na mesma branch/PR, com commits por tópico.

# Crocodilo Invaders — contexto para agentes de código e arte

Este documento é o ponto de entrada para Claude, Codex ou outro colaborador. Descreve o estado encontrado em **17/10/2026**, incluindo alterações locais ainda sem commit. Leia-o antes de implementar; consulte os arquivos citados e o estado atual do Git antes de assumir que um detalhe continua igual. Atualize este guia quando mudar arquitetura, fluxo, arte ou regras importantes.

## 1. Comece pelo projeto certo

**Bombardiro Crocodilo Invaders** é um shooter aéreo 2D em pixel art, com progressão roguelite dentro de cada tentativa: nove hordas, cartas entre hordas e Gusini em duas fases como boss da décima onda. O protagonista é B. Crocodilo, piloto crocodilo militar. O usuário desenvolve código e arte de forma iterativa e costuma fornecer ZIPs de sprites, animações, HUDs e áudio.

```text
Crocodilo-Invaders/                   ← raiz Git
├── agent.md                         ← este guia
├── CLAUDE.md                         ← encaminha o Claude para este guia
├── .github/workflows/windows-release.yml
├── Crocodilo Invaders/               ← PROJETO UNITY ATIVO
│   ├── Assets/
│   ├── Packages/
│   ├── ProjectSettings/
│   ├── Docs/
│   ├── SourceArt/
│   └── Tools/
├── Gaming portugol/                  ← original em Portugol; legado
└── unity/                            ← outro port antigo; legado
```

Todos os caminhos `Assets/`, `Docs/`, `SourceArt/`, `Tools/`, `Packages/` e `ProjectSettings/` abaixo são relativos a **`Crocodilo Invaders/`**. Caminhos `.github/` são relativos à raiz Git. Não trabalhe por engano em `unity/`. O README da raiz ainda descreve principalmente versões antigas; o README do projeto também conserva algumas referências antigas ao Executor.

- Editor: **Unity 6000.6.4f1**, conforme `ProjectSettings/ProjectVersion.txt`.
- C#, GameObjects/MonoBehaviours, Rigidbody2D/Collider2D, SpriteRenderer e Animator.
- UI: Canvas/uGUI + TextMesh Pro + `InputSystemUIInputModule`.
- Input System **1.20.1**, uGUI **2.6.0**, Test Framework **1.8.0** no manifest atual. Consulte `Packages/manifest.json` e o lock antes de mudar dependências.
- Não existe dependência de tween necessária às animações atuais; várias usam coroutines e curvas calculadas no código.
- Janela padrão: 1200 × 720. Várias interfaces têm sua própria resolução de referência; preserve seus CanvasScalers.

## 2. Estado atual, legado e planos

| Área | Situação |
| --- | --- |
| Menu, nome do jogador, loading, gameplay, pausa e opções de áudio | Implementados e ligados entre si |
| Nove hordas com composições sorteadas + cartas + boss na onda 10 | Implementado |
| Player animado, tiros por Animation Events, especial, dash e feedback de dano | Implementado |
| Patrulheiro, interceptor e artilheiro | Implementados |
| Gusini Fase 1, transformação e Fase 2 com quatro ataques | Boss ativo; integrado ao fluxo da run |
| HUD cockpit do player e barra própria do Gusini | Implementadas |
| Anúncio "ONDA N" em PNG e alerta ONDA 10 → PERIGO! GOOSINI com SFX sincronizado | Implementado em 18/10/2026; substitui o banner TMP antigo. Ver `Docs/AnunciosOndasBoss.md` |
| Céu de Batalha com FX e parallax | Implementado; refinamento mais recente está local, sem commit no início deste guia |
| Ranking | Local, persistente; não é serviço online |
| Executor V-9 | Lógica anterior preservada; HUD antiga removida das cenas atuais |
| Diálogos e cenas narrativas (abertura da run, encontro com Gusini, falsa derrota antes da F2) | Implementados em 18/10/2026; ver `Docs/Cinematicas.md` |
| Encerramento do capítulo 1 (parasita destruído, Gusini resgatado, Central, fade para `BaseImprovisada`) | Implementado; substitui a tela de vitória. Base mínima agora existe e está no build |
| Música do Gusini F1/F2 | Implementada (`GusiniMusic`); mixagem ainda sem escuta crítica |
| HQ de introdução | Oito imagens sem texto existem; não confundir assets com uma sequência jogável integrada |
| Base lateral com personagem a pé, seleção de missão e retorno após vitória/derrota | Cenário v01 da BaseImprovisada implementado em 18/10/2026 (`Docs/BaseImprovisada_Map.md`). Personagem/rig/Idle/Run e lógica da câmera continuam ausentes, por decisão do usuário. Missões continuam planejadas |
| Gusini Fase 3 | Descartada: o boss tem só duas fases |
| Metaprogressão, loja e multiplayer | Não implementados nesta etapa |

Alguns documentos registram etapas anteriores. Exemplos: `Docs/Pausa.md` ainda menciona opções provisórias, mas a tela real está em `Docs/OpcoesAudio.md`; referências ao Executor/reforços em `Docs/Ondas.md` não substituem a integração atual do Gusini. A tabela de raridades fixa original de 60/25/11/3,5/0,5% foi substituída pela progressão descrita adiante. Código, dados serializados atuais e seções mais recentes dos documentos resolvem essas divergências.

### Fotografia do Git ao preparar este guia

- Repositório: `https://github.com/Tutsdevtj/Crocodilo-Invaders`; branch de trabalho encontrada: `main`.
- Último commit encontrado: `1baea88b2a6da46123aa9cd5bc3d2ffc7b11d714`, `Integrate Gusini boss, animated HUDs and battle sky`.
- Havia alterações de parallax em `BattleSkyTools.cs`, `BattleSky.prefab`, `DistantAirBattle.cs`, `Docs/BattleSky.md`, mais o novo `BackgroundShipDrift.cs` e seu `.meta`.
- Também havia `Assets/fdasdf.unity` e seu `.meta`, uma cena provisória separada. Não incluí-la automaticamente em builds ou commits de outra tarefa.
- Esta é uma fotografia, não uma ordem para manter esses arquivos modificados. Sempre comece com `git status` e preserve trabalho existente de outros colaboradores.

## 3. Cenas e fluxo real

| Cena | Uso |
| --- | --- |
| `Assets/Scenes/MainMenu.unity` | Entrada de produção; cena 0 do build |
| `Assets/Scenes/Tests/PlayerMovementTest.unity` | **Gameplay real**, apesar do nome; cena 1 do build |
| `Assets/Scenes/Jogo.unity` | Port legado com `CrocodiloGame.cs`; cena 2 habilitada atualmente |
| `Assets/Scenes/BaseImprovisada.unity` | Base lateral após o capítulo 1; cena 3 habilitada. Mapa horizontal de 105 unidades (zonas A–G, prefabs `Hub*`, parallax, halos, anchors, setores com estado); câmera nativa fixa (só Y = 2,4); sem personagem, rig, animações, lógica de câmera ou HUD |
| `Assets/Scenes/Tests/StageOneWaves.unity` | Cena alternativa de desenvolvimento das hordas |
| `Assets/Scenes/Tests/GusiniBossTest.unity` | Teste direto de Gusini F1 no Editor |

As duas últimas não estão na lista habilitada atual de `EditorBuildSettings`. Não assuma que `SceneManager.LoadScene` consegue abri-las em um player. O teste direto do boss usa `BossDevelopmentStart` e `WaveController.StartBossForDevelopment()`, protegido por `UNITY_EDITOR`; desde 18/10/2026 ele passa pelo mesmo anúncio da onda do boss (ONDA 10 → PERIGO! GOOSINI, 2,5 s) antes de instanciar o Gusini. Na cena GusiniBossTest, o starter aplica após o reset da run um kit de teste: 240 HP cheios, normal x4, especial x3, cadência x2,5, movimento x1,4, dash com distância x1,3/cooldown x0,4/i-frames x1,4 e especial inicialmente carregado (recarrega em cinco tiros). Só nessa cena e no Editor; não muda o prefab/base das runs normais. Valores principais ajustáveis no Inspector.

Fluxo atual: **MainMenu → Jogar → nome → loading assíncrono → ENTER → fade → abertura por rádio (fase `Intro`) → nove hordas com nove escolhas de cartas → ONDA 10 → penas → PERIGO! GOOSINI → entrada do Gusini + diálogo → boss F1 → falsa derrota + diálogo → transformação → F2 → parasita destruído + diálogo final + RÁDIO → fade → `BaseImprovisada` (aviso de desenvolvimento enquanto a cena não existir)**. Derrota do player ainda mostra "FIM DE JOGO" → menu. Também é possível perder ou abandonar pela pausa. Abrir diretamente a cena de gameplay inicia a abertura da run; PRONTO?/INICIAR foram removidos.

Controles atuais:

- Setas: movimento; Espaço segurado: tiro normal; C: especial carregado.
- Shift esquerdo/direito: dash na direção de movimento, ou última direção válida; inicialmente direita.
- Esc: pausa/volta; Start do controle também pausa.
- Cartas: mouse, esquerda/direita + Enter, teclas 1–3; R faz reroll. Baixo seleciona reroll, cima retorna às cartas.
- Diálogos: Enter/clique esquerdo (ou A do controle) revela a fala inteira e, com ela completa, avança; segurar Tab (ou Select) 0,5 s pula a cena inteira.
- Menus: mouse, setas/Enter e navegação do EventSystem para controle.

## 4. Organização do código

| Pasta/classe | Responsabilidade |
| --- | --- |
| `Scripts/Entities/` | Vida, entidades, pontuação, composições/pools de ondas, catálogo e pesos de cartas, pickup |
| `Scripts/Controllers/` | Movimento, combate, input, dash, progressão, pausa, upgrades e regras de áudio |
| `Scripts/Presentation/` | Animação, HUD, menus, loading, avisos, feedback visual e sonoro |
| `Scripts/Environment/` | Rolagem, deriva da nave-mãe, FX do cenário e batalha distante |
| `Scripts/Persistence/` | `LeaderboardStore` e `LeaderboardSession` |
| `Scripts/States/PlayerState.cs` | Estado auxiliar existente do player; leia os consumidores antes de ampliar |
| `Scripts/CrocodiloGame.cs` | Monólito do port legado; não é o lugar de implementar as regras modernas |
| `Assets/Editor/` | Setup/importação, ferramentas de reconstrução/validação e build |

Convenções existentes: componentes pequenos, referências serializadas nos prefabs/cenas, eventos para apresentação observar gameplay. Os scripts atuais em geral não usam namespaces. Reaproveite o padrão local. Não transfira cálculo de dano, pontuação ou progressão para a HUD. Não introduza um segundo controlador do mesmo estado global.

### Player, física, vida e projéteis

- `Player` é a fachada de `PlayerMovement`, `PlayerCombat`, `PlayerDash` e `Health`; controla liberação de gameplay e morte.
- `PlayerInputReader` usa Input System e clona as actions por instância, habilitando/desabilitando/descartando corretamente. Preserve as actions e seus GUIDs/bindings existentes.
- **`PlayerMovement` é o único escritor da `Rigidbody2D.linearVelocity` do player.** Dash fornece um override, em vez de disputar a física ou teleportar.
- `Health` conserva vida fracionária, `Fraction`, eventos de dano/cura/morte e invulnerabilidade. `CurrentHealth` e o evento inteiro continuam disponíveis; o número visível arredonda para cima enquanto vivo. Não arredonde o dano real: +5% sobre 1 deve causar 1,05.
- `Projectile` concentra lançamento, layers atingíveis, contato e dano. Seu dano recebe multiplicador na criação; tiros que já voam não mudam quando uma carta é adquirida.
- Base atual: 20 HP nas partidas normais (ajuste de 18/10/2026); intervalo normal 0,45 s; carga especial exige dez tiros normais efetivamente liberados. Não trocar carga por abates. O kit de GusiniBossTest continua com 240 HP.
- `PlayerCombat` aceita o pedido e emite `ShotStarted`/`SpecialStarted`. **O projétil só nasce no release do Animation Event.** `ShotFired`/`SpecialFired` significam criação real e sincronizam áudio/carga.
- No máximo um ataque pendente; especial carregado tem prioridade sobre input simultâneo de tiro. Carga é consumida no release do especial, com no máximo um projétil especial ativo. Morte/disable cancelam pendências.

### Animações e dano do protagonista

`Assets/Prefabs/Player.prefab` mantém física, lógica e origens no root; somente o filho `Visual` contém o sprite que recebe shake. `PlayerPresentation` liga Animator e combate.

- 26 PNGs nativos do pacote Bombardiro, canvas 160 × 120, em `Assets/Art/Player/Bombardiro`.
- `SourceArt/BombardiroAnimations` conserva os cinco JSONs de timings.
- `Assets/Animations/Player`: Idle, Fly, Shoot, Special, Hurt e `Player.controller`.
- Point, Full Rect, PPU 100, pivot `(0.45, 1/3)`; escala visual/offset e origens já compensam a mudança de arte. Não alterar collider/root para acomodar arte sem uma tarefa de gameplay.
- Curvas usam durações variáveis dos frames, em timestamps cumulativos. A grade de 1000 fps permite milissegundos; não significa exibir mil imagens por segundo.
- Release normal em 0,12 s e especial em 0,29 s nos clips nativos. `ShootSpeed` acelera somente Shoot conforme cadência; não acelerar Hurt/Fly/Idle por carta.
- Dano aciona flash branco e shake curto do Visual. Hurt espera um release pendente para não apagar um ataque já aceito.
- `SpriteHitFlash`/`SpriteHitFlash.shader` usam MaterialPropertyBlock; também servem inimigos e boss. Não criar um material novo por hit.
- `DamageAppearance`, `EnemyPresentation`, `ProjectileImpact` e `OneShotEffect` mantêm avarias, fumaça, impactos e explosões. Referência: `Docs/FeedbackDeAcerto.md`.

### Dash

Atualização de 18/10/2026: base 2,2 unidades/0,16 s, cooldown 1,1 s e proteção 0,13 s. Curva editável 2,0 → 0,8 (65%) → 0,2, integral acumulada normalizada em 256 intervalos, diferenças por passo preservando distância independente de FPS/curva. PlayerMovement segue como único escritor da velocidade. Teste detectou tunneling com upgrade e obstáculo fino; somente o Rigidbody2D do player passou a Continuous, sem mudar física global.

Eventos DashStarted, InvulnerabilityEnded, DashEnded e DashCancelled conduzem PlayerDashVFX. 22 PNGs originais Dash_VFX.zip: ignição 8, rastro 6 e fim 8; PPU 100, pivôs exatos e orientações só flips/90°. Ignição/fim em mundo; rastro segue centro real do Visual; seis afterimages em pool, snapshots físicos a ~30 ms, vida 130 ms, silhueta ciano/contorno/sombra conforme manifesto. ShaderHitFlash preserva dano e acrescenta indicador de um pixel somente nos i-frames. Saúde oferece SetInvulnerable(owner,bool), assim o dash não cancela outra proteção. Pausa escala tudo, bloqueio/morte/disable limpam efeitos e unload destrói a raiz da cena.

SetUpgradeMultipliers continua relativo à base; upgrades em andamento só valem no próximo dash. GusiniBossTest conserva seus bônus de teste anteriores (distância x1,3, cooldown x0,4, i-frames x1,4 limitados a 0,16 s). Não existem cartas/HUD novos. SFX de dash fornecido pelo usuário (originais/A/1.wav) em Assets/Audio/Player/SFX_Player_Dash.wav, ligado em Dash Sound: 2D, Effects, volume local 0,25 ajustável; uma reprodução no início bem-sucedido, sem repetir no cooldown. Original estéreo 48 kHz/0,4865 s preservado, importação PCM com preload. Ver Docs/PlayerDash.md, SourceArt/DashVFX e PlayerDashTools/PlayerDashPlayValidation. 284 verificações de Play Mode, rodada adicional de 63 verificações de input/visual e 69 verificações após ligar o áudio passaram; sem commits nesta tarefa.

Projectile e BossLaser continuam consumindo/registrando contatos esquivados, sem dano retroativo; ataques, bosses e waves não foram modificados.

## 5. Hordas, inimigos, cura e pontos

`WaveController` coordena os estados `Waiting`, `Announcing`, `Combat`, `Intermission`, `Boss`, `BossDefeated`, `Completed`, `Failed`. Eventos principais: `WaveAnnounced`, `WaveCleared`, `BossStarted`, `RunEnded(bool)`.

- Cada uma das nove hordas tem um `EnemyWavePool` com três composições completas. Sorteia uma opção por slot **uma vez no início da run**, usando `System.Random` próprio. O debug seed reproduz só essa seleção.
- `EnemyWave` guarda grupos de prefab/quantidade, intervalo e máximo vivo. `EnemySpawner` executa a fila e conta vivos/pendentes; não escolhe a próxima onda.
- Ordem de configuração: pools → composição fixa → `enemiesPerWave` legado. Não eliminar fallbacks sem migrar cenas.
- Uma horda acaba apenas quando não há vivos **nem spawns pendentes**. Cada conclusão concede 1.000 pontos e limpa projéteis inimigos restantes.
- Após cada horda, `WaveController` espera explicitamente a recompensa; a animação da UI não decide quando avançar.
- Anúncio: `WavesUI` segura a fase `Announcing` (`HoldAnnouncement`/`ReleaseAnnouncement` com token) até o `WaveAnnouncer` terminar: 1050 ms por horda, 2500 ms na onda do boss (`IsBossWave`; tema em `Boss.alertTheme`). `announcementDuration` (1,6 s) só vale sem apresentação. Intervalo 2 s; após escolha, transição de 0,35 s. O boss conta como onda 10, sem horda adicional. Não existe mais aviso textual de onda concluída.

Composição A de referência; B/C aumentam ligeiramente o total, sem introduzir um tipo antes da hora:

| Onda | Patrulheiros | Interceptores | Artilheiros | Faixa total A–C |
| --- | ---: | ---: | ---: | ---: |
| 1 | 4 | 0 | 0 | 4–6 |
| 2 | 8 | 0 | 0 | 8–10 |
| 3 | 10 | 2 | 0 | 12–14 |
| 4 | 12 | 4 | 0 | 16–18 |
| 5 | 12 | 8 | 0 | 20–22 |
| 6 | 14 | 10 | 2 | 26–28 |
| 7 | 16 | 12 | 4 | 32–34 |
| 8 | 18 | 14 | 6 | 38–40 |
| 9 | 20 | 16 | 8 | 44–46 |
| 10 | — | — | — | Gusini F1 → F2 e seus próprios ataques |

Inimigos comuns compartilham `Enemy`, `Health`, `EnemyMovement`, apresentação e flash:

- Patrulheiro: `Assets/Prefabs/Enemies/Enemy.prefab`, 4 HP, patrulha e tiro mirando a posição do player.
- Interceptor: `Assets/Prefabs/Enemies/Interceptor.prefab`, 3 HP; aviso de 0,7 s, investida a 10 un/s, recuperação 0,4 s; contato causa 2.
- Artilheiro: `Assets/Prefabs/Enemies/Artillery.prefab`, 10 HP; aviso de 0,9 s, cinco projéteis em leque de 48°, velocidade 3,6 un/s.
- `EnemyAttack` é a base dos ataques; `EnemyCombat`, `InterceptorCombat`, `ArtilleryCombat` decidem seus padrões. `EnemyAttackPresentation` desenha os avisos.
- Investida e leque travam a mira ao anunciar, permitindo esquiva. Animações/avisos devem corresponder ao ataque real.
- `RunScore` contabiliza; inimigo comum concede 800 pontos, boss 40.000 e um abate. Limpeza por destruição/saída não deve contar como derrota.

### Caixa de vida

A condição antiga por contador de abates foi substituída por `HealthPickupSpawner`. Só gera durante combate de horda/boss com player vivo, faltando pelo menos os 6 HP que a caixa cura. HP ≤65% precisa persistir por 4 s; HP ≤25% reduz essa espera a 1,5 s. Uma caixa ativa; cooldown de 20 s após coleta ou 10 s após perder a caixa. Estado crítico não ignora o cooldown.

Nasce à direita, atravessa a 2 un/s, Y perto do player com variação de 1,2 e limites alcançáveis. HP cheio não consome, cura não revive e fim de combate limpa sobras. `HealthBox.prefab`, `HealthRestoreEffect.prefab`, `HealthPickup`, `HealthPickupPresentation` e áudio de `Health.Healed` reaproveitam a arte/som originais. Pausa congela os tempos.

## 6. Cartas e progressão roguelite

`Assets/Data/UpgradeCards.asset` contém 25 cartas: cinco tipos × cinco raridades. `UpgradeCardCatalog` e `RarityWeights` cuidam do sorteio; `PlayerRunUpgrades` aplica os atributos; `CardRewardController` coordena onda, input, pausa, reroll e aplicação única. Apresentação em `CardRewardView`/`CardOptionView`, prefab `Assets/Prefabs/UI/CardRewards.prefab`.

- Três tipos distintos por oferta; raridade independente por carta.
- **Um reroll gratuito por escolha**, que troca as três cartas; renova só na próxima recompensa. Não melhora artificialmente o peso do sorteio.
- Nove escolhas: após ondas 1–9. Vitória no boss não abre outra carta. Nova run zera todos os bônus temporários.
- Soma aditiva por atributo: +5% e +30% = +35% sobre a base inicial, não multiplicação composta.
- Cadência: intervalo `base / (1 + bônus/100)`. Dano/movimento: `base × (1 + bônus/100)`.
- HP máximo arredonda para cima e preenche só a capacidade adicionada, preservando dano anterior. Não cura tudo nem revive.
- Bases são capturadas do player da cena, incluindo overrides; não substituí-las silenciosamente pelos valores do prefab.

| Buff | Comum | Raro | Épico | Lendário | Mítico |
| --- | ---: | ---: | ---: | ---: | ---: |
| Dano normal / velocidade de ataque / dano especial | 5% | 10% | 15% | 20% | 30% |
| Vida máxima | 10% | 15% | 20% | 30% | 40% |
| Movimento | 3% | 5% | 8% | 10% | 15% |

Pesos atuais interpolam início → fim; progresso `(onda concluída - 1) / (recompensas - 1)`, capturado ao abrir. Reroll usa esse mesmo progresso:

| Raridade | Após onda 1 | Após onda 5 | Após onda 9 |
| --- | ---: | ---: | ---: |
| Comum | 70% | 50% | 30% |
| Raro | 24% | 28% | 32% |
| Épico | 5% | 15% | 25% |
| Lendário | 0,9% | 5,45% | 10% |
| Mítico | 0,1% | 1,55% | 3% |

Não existe garantia de raridade por run. Os pesos editáveis não precisam somar 100; validação rejeita negativos, não finitos e total zero. Tipos/valores correspondem aos números impressos na arte; atualizar um lado exige conferir o outro.

Cartas entram de baixo mostrando o verso e viram em sequência; entrada 0,38 s, virada 0,32 s, stagger 0,10 s. Fundo congelado/desfocado; música continua na recompensa. Pausa por cima interrompe música e relógio visual, bloqueia escolha/reroll e depois retorna à mesma oferta.

Épica: halo roxo; lendária: dourado; mítica: trovão vermelho de 48 frames a 40 fps, seguido de halo vermelho pulsante. `CardRarityFeedback`, `CardRarityParticles`, `CardRarityAudio` e `CardPixelGlow.shader` desenham FX atrás das cartas, sem interceptar cliques. UI Overlay usa brilho aditivo quantizado, não Light2D/câmera adicional. Partículas usam mesh por carta. Sons de revelação foram sintetizados para o projeto em `Assets/Audio/CardRewards`. Reroll/saída limpam efeitos e sons; não tocar de novo a cada atualização de face.

As 25 frentes e o verso preservam os PNGs do usuário. Importação limitada a 512, Point, sem compressão/mipmaps; catálogo carrega somente as frentes oferecidas e mantém cache da sessão.

## 7. Boss atual: Gusini Fases 1 e 2

Entrada principal: `Assets/Prefabs/Boss/Gusini.prefab`; arte `Assets/Art/Boss/GusiniPhase1`; clips/controllers `Assets/Animations/Boss/GusiniPhase1`; fontes/manifest `SourceArt/GusiniPhase1`. São 50 PNGs nativos, 13 clips e cinco controllers, preservando os timings variáveis dos oito JSONs utilizados. O ZIP antigo não foi usado para F2/F3. A F2 agora vem exclusivamente do novo pacote Gusini_Fase2_Pacote; F3 descartada.

`Boss` mantém entrada, vida, hitboxes, início/fim/morte e pontos. `IBossCombatController` permite tanto `GusiniCombat` quanto o antigo `BossCombat`. `GusiniPresentation` faz a ponte dos eventos; não duplicar releases com timers. Origens ficam fora das curvas do Visual.

Balanceamento atual, aumentado a pedido do usuário:

- 258 HP; movimento de entrada 4,5 un/s e patrulha 2,4; Animator do boss em **2×**, ovo em velocidade nativa.
- Primeiro ataque sempre Burst; depois sacolas embaralhadas de Burst/Squadron/EggParachute, sem repetição imediata entre sacolas.
- Delay inicial e recuperação 0,35 s; salva de cinco tiros, velocidade 7,5, intervalo adicional 0,025 s entre animações de tiro. Os clips ainda determinam o ritmo real de cada release.
- Esquadrão: três formações de cinco gansos, separadas por 0,22 s, velocidade 7,2 un/s, slots em V. `GusiniSquadron` move o bloco; `GusiniSquadronGoose` cuida de vida/contato e visual de cada ganso.
- Dois ovos no mesmo release, planejando até destinos distintos na região central: X de -3 a 1 e Y de -1,4 a 3,2, separação desejada 1,6. Glide 5,2 un/s; ao chegar, fuse 0,25 s, cracks de 0,26/0,22 s e burst de 0,12 s.
- `GusiniParachuteEgg` lança três gemas direcionais a 6 un/s. Prefabs separados para ovo, bala, esquadrão/ganso e gemas esquerda/cima/baixo.
- Raiz das ameaças pertence à cena e não acompanha o movimento do boss. Morte do boss/player, stop, disable e unload limpam ameaças/coroutines. Não deixar projéteis ou áudio órfãos.
- Gusini F1 **não tem enrage**. Ao zerar seus 258 HP, GusiniPhaseTwo intercepta Health.Depleted antes de Died, limpa ameaças e executa transformação invulnerável de 4,1 s. HUD F1 vazia até revelação em 2,82 s; tomada verde e recarga no mesmo prefab. F2: 320 HP, Animator 1x, idle estático com bob. Primeiro ciclo Energy/Egg/Squadron/Tentacles; depois bag dos três comuns + Tentacles por ciclo. Três bolas em zigue-zague a 300 ms, ovo indestrutível com cinco gemas, cinco gansinhos de 2 HP com mergulho abaixo de 60%, três golpes de duas faixas com 900 ms de aviso e núcleo exposto por 2 s (dano recebido x2). Só F2 concede derrota/pontos. São 213 PNGs originais (171 boss/42 HUD), 12 clips e SourceArt/GusiniPhase2. Ver Docs/GusiniFase2.md e GusiniPhaseTwoTools; 41 checks de combate + 7 de ciclo da HUD passaram na Unity, além da integração F1/Executor/áudio/build. Cleanup por cena, pause escalada, sem F3. Áudio da F2: 29 masters próprios do pacote Gusini_Audio_Phase2.zip (18/10/2026), Assets/Audio/Boss/GusiniPhase2, config GusiniPhaseTwoAudio.asset, metadados SourceArt/GusiniAudioPhase2. IDs acrescentados ao fim do enum F1. Mesmo pool de dez fontes/Effects e orçamento 0,82; sem motor F1 na transformação/F2. Eventos do Animator e relógios de entidades sincronizam disparos, ticks, avisos, mergulhos e núcleo, com supressão de loops/repetições. Low/High importados desativados; um único Mid discreto pelo líder. Ver Docs/GusiniAudioFase2.md e GusiniPhaseTwoAudioTools. A atualização de áudio passou em 83 verificações nativas de combate/áudio, incluindo pausa e limpeza; mixagem inicial ainda sem escuta crítica em run manual.

### Áudio do Gusini: decisão importante do usuário

`GusiniAudio`, `GusiniAudioConfig`, `Assets/Data/GusiniAudio.asset` e `Assets/Audio/Boss/GusiniPhase1` integram 24 WAVs originais em 20 IDs. `SourceArt/GusiniAudio` registra fonte/manifest. Não reordenar enum/IDs serializados sem migração.

O usuário rejeitou fortemente a versão com melodias Low/Mid/High dos passarinhos. **O estado aprovado usa um único honk Mid curto por ataque inteiro de esquadrão, não por formação ou por pássaro. Não restaurar o coral.** Low/High estão importados mas desativados; bicos continuam animados só visualmente.

- Honk Mid: 190 ms, ganho 0,09, pitch 1, cooldown 2 s; agenda após o assovio terminar +30 ms.
- Assovio 0,22; passagem coletiva 0,04; motor discreto 0,055; canhão com três variações.
- Um pool central de dez AudioSources, roteado a Effects, com controle de headroom; nenhuma fonte por ganso/ovo.
- Cracks, escotilha e disparos sincronizados por eventos; bursts simultâneos dos ovos são agrupados numa janela de 35 ms.
- CommandWhoosh, FlutterLoop e YolkWhoosh estão disponíveis, mas desligados para evitar acúmulo.
- Respeitar pausa/limpeza. Conferir a mixagem ouvindo em jogo: validação técnica não prova que o áudio está agradável. Capturas via AudioRenderer neste ambiente já retornaram silêncio mesmo com dados válidos.

### Compatibilidade com Executor V-9

`ExecutorV9.prefab`, `BossCombat`, `BossLaser`, `BossReinforcements` e `BossPresentation` continuam presentes. Executor tem padrões de leque, mísseis, varredura, lasers, segunda fase e reforços interceptores. Essa lógica histórica não foi removida; a HUD antiga e seu fallback em `BossHUD` foram explicitamente descartados pelo usuário. Leia a seção própria em `Docs/Boss.md` como histórico.

## 8. HUDs novas

**Player:** `PlayerHUD` liga eventos/estado; `PlayerHUDCockpit` só apresenta. Prefab da HUD foi preservado, arte em `Assets/Art/UI/PlayerHUDv2` (51 PNGs x2), layout em `SourceArt/PlayerHUDv2`.

- Cockpit 364 × 112 no canto inferior esquerdo, margem 16, Canvas 1200 × 720 com match 0,5. Pontos/abates mantêm seus objetos/posições.
- HP contínuo pela fração precisa; barra de atraso segura 0,35 s e escoa a 0,9 barra/s. Cura sincroniza e dá flash; crítico ≤25% usa vermelho/avaria/glow.
- Especial contínuo com máscara, bolhas limitadas, lâmpada, ventilador, ignição/chama e sparks reutilizados; cheio e gasto têm animações próprias.
- Relógio visual escalado: pausa congela efeitos. Sem cálculo de combate no componente visual.

**Boss:** `Assets/Prefabs/UI/BossHUD.prefab` contém apenas `GusiniBarF1`, salva oculta até o encontro. `BossInfo` do Executor e o fallback foram removidos por pedido explícito; não restaurar. `BossHUD` liga Gusini F1/F2 e `BossBarFX` anima. Arte em `Assets/Art/UI/GusiniBossBarF1` (29 PNGs x2), fonte/layout em `SourceArt/GusiniBossBarF1`.

- Barra 476 × 100 no canto inferior direito, margem (-16,14), mesmo Canvas de referência. Nome GUSINI na arte, medalhão de ganso, asas, bico e cachecol.
- HP esvazia **da esquerda para a direita**, com origem do preenchimento à direita. Não inverter sem pedido.
- Entrada de 1,3 s com slide, medalhão, nome, carga da barra e glint. Dano durante entrada fica pendente e aplica o HP mais recente depois da carga.
- Hit tem flash, spark na borda do HP e shake curto; lag segura 0,4 s e escoa a 0,8/s. Crítico ≤30% usa vermelho/pulsação; shimmer só fora do crítico.
- Desinscrever handlers ao trocar/esconder boss. Não reabrir a barra ao reativar UI após morte/fim da run.

### Cenas narrativas, diálogos e música do boss

`RunCutscenes` (no `Assets/Prefabs/UI/Cinematics.prefab`, ligado em PlayerMovementTest e GusiniBossTest) só orquestra: `WaveController.StartRun` pede a abertura (fase `Intro`, no fim do enum); `BeginBoss` chama `PrepareBoss` antes de `BeginBattle`, que segura `Boss.Ready` via `HoldCombat` até o diálogo terminar, sem mudar a entrada; o evento nativo `apparent_defeat_hold` (690 ms) de `P2Transform` dispara `GusiniPhaseTwo.ApparentDefeatHeld`, e o orquestrador congela o tempo nessa pose durante o diálogo, retomando o mesmo clip depois. Não crie outra transformação nem outra máquina de estados.

- Falas/expressões/pausas ficam em `Assets/Data/Dialogue/*.asset` (editáveis no Inspector); elenco em `DialogueCast.asset`. Retratos aprovados em `Assets/Art/Portraits` (160×160, pivô base), UI "Rádio de Piloto" em `Assets/Art/UI/Dialogue` (PPU 50 = 2×), fonte Pixelify Sans raster 22 px; fontes e guias em `SourceArt/Dialogue`.
- `DialogueRunner` usa tempo não escalado, `maxVisibleCharacters`, uma ação por pressionamento e respeita `PauseController`. `DialogueBlipAudio` limita os blips (variante 05 de FunnyDude) e nunca toca as letras restantes ao revelar.
- `GusiniMusic` (Gusini.prefab): F1 ao liberar o combate, silêncio na falsa derrota, F2 no fim da transformação (4,1 s); fonte única fora do pool de 10 vozes, roteada a Music. Faixas CC0 de Ansimuz em `Assets/Audio/Music`; registros em `SourceArt/GusiniMusic`.
- Ferramentas: `Crocodilo/Dialogue/Install cinematics` (mutação, idempotente), `Validate cinematics`, `Validate cinematics in Play Mode` (168 verificações) e `Capture dialogue previews`.
- Fim do capítulo: F2 zerada → `GusiniPhaseTwo.Stage.Defeated` → `GusiniDefeat` (retém o boss, clip nativo `P2Defeat` + loop avariado) → `RunCutscenes.PlayBossFinale` → `WaveController.CompleteChapter` (fase `ChapterCompleted`, sem `RunEnded`) → `ChapterFlow` grava `CampaignProgress` (`gusini_rescued`, `chapter1_completed`, em `campaign.json`) → `SceneTransition` para `Chapter1.asset.nextScene`. O speaker `radio` (RÁDIO, `Side.None`) serve para outras transmissões.

## 9. Cenário atual e parallax

`Assets/Prefabs/Environment/BattleSky.prefab` substitui os objetos antigos de fundo, sol e chão nas três cenas de desenvolvimento/gameplay citadas acima. **MainMenu conserva sua própria arte.** São 46 PNGs nativos em `Assets/Art/Environment/BattleSky`: oito camadas, 25 FX da nave-mãe e 13 sprites de batalha distante. Fontes em `SourceArt/BattleSky`.

Importação PPU 50, Point, Full Rect, sem mipmaps/compressão. `BackgroundFit` encaixa uniformemente cobrindo a câmera; execução antecipada evita calcular larguras de scrolling antes da escala final. Não mudar paredes/arena/velocidades de gameplay para encaixar decoração.

| Camada | Unidades de mundo/s para a esquerda | Sorting order |
| --- | ---: | ---: |
| Céu opaco | 0 | -100 |
| Nave-mãe | 0,08 | -96 |
| Nuvens distantes | 0,20 | -92 |
| Batalha distante estática | 0,38 | -88 |
| Nuvens médias | 0,9 | -84 |
| Horizonte distante | 0,52 | -76 |
| Nuvens próximas | 1,8 | -72 |
| Ilhas/base | 3,4 | -68 |

- Seis faixas usam `ScrollingLayer`, dois segmentos de 16 unidades e cobertura adicional só quando necessária, sem espelhar ou criar gaps.
- Novo `BackgroundShipDrift`: nave-mãe se move em world space a 0,08, com FX filhos; só recicla quando todo o sprite sai à esquerda, reaparecendo inteiramente fora da direita. Uma nave, sem clonagem contínua.
- `MothershipBackgroundFX`: três fumaças defasadas, explosões/clarões/falhas e seis detritos reutilizados. `DistantAirBattle`: quatro naves, dois tiros e dois FX reutilizados, discretos no alto do céu.
- Tudo é decoração, sem collider/dano/áudio; sem persistência entre cenas; tempo escalado e pausa. Aleatoriedade visual separada da aleatoriedade de gameplay.
- `BattleSkyTools` mantém o perfil de velocidades acima; reconstruir deve preservá-lo em vez de restaurar sugestões antigas dos JSONs.

## 10. Menus, loading, pausa e opções

- `MainMenu.prefab`: arte em `Assets/Art/MainMenu`, logo, cinco botões, diamantes, nuvens e troféu. `MainMenuController`, `MenuButtonFeedback`, `MenuAmbientMotion`, `MenuIntro` separam ações de apresentação.
- Entrada musical acompanha `AudioSettings.dspTime`/`PlayScheduled`: clareamento inicial, logo a partir de 1,15 s, pulso/cascata no drop medido de **2,59 s**. Menu completo por volta de 3,17 s. Pular intro revela sem acionar Jogar na mesma tecla.
- `GameplayLoading.prefab` + `GameplayLoadingController`/`View`: fundo preto, patrulheiro branco via `UISilhouette.shader`, reticências animadas. Carrega cena assíncrona e prepara áudio; só depois pede Enter, exigindo soltar a tecla anterior. Fade final 0,45 s; só então restaura e chama `StartRun`.
- `PauseController` guarda/restaura estado anterior de tempo/input/áudio. Isso é essencial ao pausar **sobre cartas**, quando o combate já está congelado. Sair para menu normaliza tudo.
- `PauseMenuView`, `PauseBackdrop`, `PauseBlur.shader`, `PauseMenuParticles`: captura única do jogo ao abrir, blur GPU, escurecimento exterior mais leve que o centro, 18 partículas, entrada rápida de 0,18 s e saída de 0,10 s. Não usar céu da imagem de referência como fundo da pausa.
- `AudioListener.pause` congela também a música; sons de UI apropriados usam `ignoreListenerPause`. Não reiniciar música ao continuar.
- `AudioOptions.prefab` abre tanto no menu quanto na pausa. `AudioOptionsView` e `AudioSliderFeedback` implementam sliders de geral, efeitos e música; arte em `Assets/Art/AudioOptions`.
- `GameAudioSettings` é serviço inicializado automaticamente, com `Assets/Resources/Audio/GameAudio.mixer`: Master/Effects/Music e parâmetros MasterVolume/EffectsVolume/MusicVolume. Conversão `20*log10(valor)`, zero em -80 dB. **Não sobrescrever o volume artístico individual de cada AudioSource.** Rotear fontes novas via mixer/`GameAudioSettings.Route`.
- Preferências em `Crocodilo.Audio.Master.v1`, `.Effects.v1`, `.Music.v1`; salvar ao fechar/suspender/sair quando houve mudança, não a cada arraste.
- Esc em opções da pausa volta à pausa; outro Esc continua. Online/Sobre do menu seguem provisórios.

## 11. Ranking e salvamento

`LeaderboardStore` guarda JSON versionado em `Application.persistentDataPath`, com arquivo temporário e backup; `LeaderboardSession` coordena nome, ID da run e resultado pendente. Nome até 16 caracteres; ranking pelo troféu, sete entradas por página, empates estáveis. Não há backend.

Vitória/derrota capturam resultado; retorno ao menu/saída salva uma vez por ID. Abandono em andamento não é uma run concluída. Falha de gravação mantém pendência para tentar novamente. Recuperação de backup não deve sobrescrevê-lo com principal corrompido. No Windows atual: `%USERPROFILE%/AppData/LocalLow/Arthur Reis/Crocodilo Invaders`.

Evite contaminar o ranking/preferências reais com testes; use diretório/identificação de aplicação separados. Prefabs: `Leaderboard.prefab` e `PlayerName.prefab`; arte em `Assets/Art/Leaderboard`.

## 12. Direção de arte e fontes

Identidade: pixel art com leitura clara, céu azul/ciano e nuvens marfim, UI azul-marinho/marfim/dourado; militar cartunesco. Crocodilo já possui identidade definida pelo retrato: verde escuro, focinho largo, olhos amarelos/âmbar, dentes claros, capacete oliva e uniforme militar. Não inventar outro protagonista, óculos ou silhueta incompatível ao criar variantes. Gusini é o ganso piloto de avião roxo, cachecol vermelho e detalhes amarelos.

- Preserve os PNGs finais fornecidos; recortes/pivôs são metadados do importer. Não redesenhar/redimensionar arquivos silenciosamente.
- Use apenas a escala necessária em runtime. Player/Gusini/cenário usam sprites nativos; as HUDs novas usam a escala x2 escolhida de seus packs. Point, sem mipmaps/compressão quando indicado; não aplicar o mesmo PPU a todos os packs.
- JSONs/layouts/manifests/Aseprite editáveis ficam em `SourceArt`; previews ampliados em `Docs/ArtPreviews`; ferramentas de arte em `Tools/Art`.
- Não importar GIFs, reviews, contact sheets, x4/x6/x8, ZIPs inteiros ou duplicatas modulares de imagens já compostas para `Assets`.
- Resources ainda contém recursos legados e recursos explicitamente carregados em runtime; não remover/mover sem rastrear os caminhos de carregamento.
- `CrocodiloTextureImporter` atende o port original; importadores/configurações das artes novas devem respeitar sua separação.
- Novos sons devem ser roteados e limitados; o usuário é sensível a repetições irritantes e sobreposição, especialmente dos pássaros.

**HQ:** oito PNGs `Assets/Art/IntroHQ/Quadro01.png` a `Quadro08.png`, sem texto/balões, gerados individualmente com identidade referenciada. Roteiro/prompts e sequência em `Docs/IntroHQ.md`. Não alterar a arte para embutir diálogos antes de definir a apresentação.

**Personagem a pé:** há várias revisões de estudos 32/64 px, poses/idle, PNGs e fontes Aseprite nas pastas `Assets/Art/Characters` e `SourceArt/CrocodiloBase*`, `CrocodiloNovo*`, `CrocodiloIdle*`. São alternativas históricas; não tratar todas como versões finais nem escolher uma sem considerar o pedido atual. Ver `Docs/CrocodiloBaseIdle.md` e `Docs/CrocodiloBase64.md`. Esses estudos não substituem o Player aéreo.

**Créditos:** `Docs/Creditos.md` registra CraftPix (Ship1 interceptor, Ship3 artilheiro, pack gratuito/Freebie License), ansimuz Warped Shooting FX (CC0, efeito charged) e referências sem incorporação. Arte/áudio originais e materiais fornecidos pelo usuário não ganham licença/autoria presumida. A identificação completa dos recursos legados ainda está pendente. Consultar Pinterest/itch.io não autoriza copiar qualquer asset; registrar origem/licença quando incorporar novos.

## 13. Próxima estrutura planejada: base e missões

`Docs/BaseEMissoes.md` é planejamento: Menu → Base lateral → terminal de missões → loading → tentativa de dez ondas → resultado → Base. A fundação `BaseImprovisada` foi implementada em 18/10/2026 e recebe a transição existente após o capítulo 1; o fluxo completo de missões continua planejado. Ver `Docs/BaseImprovisadaRig.md`.

Em 18/10/2026, por pedido explícito do usuário, foram removidos personagem, rig, prefab, módulos importados, Idle/Run, Animator Controller, ações Base/Move, BaseCharacterController e BaseCameraFraming. As ferramentas BaseImprovisadaTools/BaseRunTools e validadores/geradores relacionados também foram removidos para impedir restauração acidental. O personagem será refeito completamente e a câmera será refeita depois. As aprovações antigas de Idle/Run não valem como ordem de preservar/restaurar a implementação descartada.

BaseImprovisada permanece no build e como destino do capítulo, com chão 60×4, paredes em ±29,7 e câmera ortográfica nativa fixa, sem script. O pixel do chão está em Assets/Art/Environment/BaseBlockoutSolid.png, movido com seu GUID. Originais do ZIP estão em SourceArt/Croc_Rig_Ready, fora de Assets; descartes arquivados em Logs/RemovedBaseSetup-20261018-171046. Estado atual em Docs/BaseImprovisadaRig.md. A limpeza também removeu os TMP PointsText/KillsText do PlayerHUD e WaveCounter do WavesUI, seus campos/eventos e overrides obsoletos. Vida/especial, anúncios de onda/boss, pontuação interna e resultado/ranking continuam. Rebuild Player cockpit não recria os textos removidos. Validações em Logs/BaseAndHudCleanup.log e Logs/BaseAndHudCleanupPlay.log.
Aproveitar `RunEnded`, `LeaderboardSession`, loading e reset de upgrades ao implementar missões. Cartas são temporárias da missão; desbloqueios permanentes ainda precisam de regras. Preservar nome/ranking e evitar salvar o mesmo resultado duas vezes nas trocas de cena.

## 14. Como editar e validar sem perder trabalho

1. Confira raiz/pasta ativa, `git status`, pedido atual e documentação da área. Com outro agente trabalhando, combine arquivos/responsabilidades antes de editar os mesmos prefabs/cenas. Não usar reset/clean/revert sobre trabalho alheio.
2. **A Unity aberta já sobrescreveu alterações de cena/prefab feitas por arquivo diversas vezes.** Antes dessas mudanças, confira se o Editor deste projeto está aberto; se estiver, peça para salvar/fechar, explicando o motivo. Pode preparar scripts/docs em paralelo, mas não matar o Editor nem sobrescrever cenas não salvas. Para documentação apenas, não é necessário fechar.
3. Prefira APIs nativas do Editor (`PrefabUtility`, `AssetDatabase`, `SerializedObject`, `EditorSceneManager`, AnimationUtility) para alterações estruturais. Preserve `.meta`/GUIDs; mover asset requer mover seu `.meta`. Não recriar componentes/refs em YAML complexo por tentativa.
4. Depois de alterar dados, confira overrides das cenas, links de prefab, callbacks dos botões, events de clips, importação e scripts ausentes. Mudança C# sozinha pode deixar a configuração serializada incompatível.
5. Valide o que mudou: compilação, validação de dados e Play Mode quando envolver gameplay/tempo/colisão/fluxo. Confira pausa, morte, disable/enable, reroll e troca de cena nas áreas afetadas. Uma execução acelerada não substitui avaliar o balanceamento jogando.
6. UI/cenário já foram inspecionados em 1200×720, 1920×1080 e 800×600; menu também em suas proporções próprias. Reconfira resoluções se mexer em layout/cobertura.
7. Documente resultado e limitações reais: não dizer que escutou áudio, jogou manualmente, gerou EXE ou viu CI passar sem ter feito/verificado isso.

Tempos: gameplay/física/dash/cenário usam tempo escalado; UI que aparece com jogo congelado usa não escalado. Cartas precisam interromper explicitamente sua animação ao abrir a pausa. Na saída para menu, restaurar relógio/input/áudio mesmo em cancelamentos/erros.

### Ferramentas existentes

| Menu/método | Uso |
| --- | --- |
| `Crocodilo/Build Windows` → `CrocodiloWindowsBuild.Build` | Build Mono Windows x64 com validação |
| `CrocodiloWindowsBuild.ValidateProject` | Cenas, scripts, prefabs, cartas/pools e mixer sem gerar player |
| `Crocodilo/Validate Roguelite` | Distribuições, tipos distintos, pesos inválidos e pools |
| `Crocodilo/Validate Leaderboard Storage` | Persistência isolada do ranking |
| `Crocodilo/Boss/Validate Gusini Phase 1 assets` | Arte, clips, events e integração Gusini |
| `Crocodilo/HUD/Validate Player cockpit` | HUD player e importação |
| `Crocodilo/HUD/Validate Gusini F1 bar` | HUD boss e importação |
| `Crocodilo/Environment/Validate Battle Sky` | Camadas, importação e referências |
| `Crocodilo/Environment/Apply Battle Sky depth speeds` | Só aplica perfil de velocidades/deriva ao prefab |
| `Crocodilo/UI/Validate wave and boss announcements` | Importação, exemplos do JSON, curvas, prefab, tema e cenas dos anúncios (Install é a mutação) |
| `Crocodilo/UI/Validate announcements in Play Mode` | Play Mode real dos anúncios, alerta, áudio, pausa e fluxo; capturas em `Logs/WaveBossUI` |
| `Crocodilo/Base/Validate BaseImprovisada map` (+ `in Play Mode`) | Importação, escala, setores, colisões, parallax; sonda física e capturas em `Logs/BaseImprovisadaMap`. `Install BaseImprovisada map` é mutação e recria os grupos gerados da cena |

Comandos **Rebuild/Install/Setup** também existem, mas são mutações, não simples checks: podem reconstruir prefabs/instalar cenário ou modificar cenas. Leia o método antes de chamar. Não rodar toda ferramenta de setup por rotina.

Validação rápida, sem abrir Unity, executada da raiz Git:

```powershell
python "Crocodilo Invaders/Tools/validate_ci.py" "Crocodilo Invaders"
git diff --check
```

O validador Python confere assets/metas/GUIDs/cenas/arquivos indevidos; não compila C# nem prova comportamento. Testes Play Mode descartáveis de integrações anteriores foram removidos após uso; logs/capturas em `Logs` são evidência local ignorada, não uma suíte permanente toda executada pelo CI. As validações duráveis em `Assets/Editor` são a referência repetível disponível.

Neste PC, Editor em `C:/Program Files/Unity/Hub/Editor/6000.6.4f1/Editor/Unity.exe`; projeto em `C:/Users/Arthu/OneDrive/Desktop/Crocodilo-Invaders/Crocodilo Invaders`. Para batch, use caminho do projeto entre aspas, `-batchmode -quit -executeMethod ... -logFile ...`; execuções de Play Mode que precisam continuar não devem receber `-quit` antes de terminar. Não abrir dois Editores no mesmo projeto; serviços em background devem ficar com janela oculta.

Cuidados práticos do ambiente:

- Unity 6 atual usa `EntityId`/`GetEntityId`; `GetInstanceID` já causou erro de obsolescência neste projeto. Consulte APIs da versão instalada.
- Preload de áudio fica em `AudioImporter.defaultSampleSettings.preloadAudioData`.
- Unity fake-null exige comparação apropriada; `??` em referência destruída não equivale a `== null`.
- Animation Events podem ocorrer antes da aplicação do sprite daquele frame; assertions visuais precisam observar o estado após a atualização, por exemplo em LateUpdate.
- Pausa/física/Animator podem exigir um frame de acomodação em testes; distinguir isso de timers que seguem indevidamente.
- OneDrive pode deixar arquivos antigos offline. Não remover fontes ou recriar conteúdo indiscriminadamente por falha de leitura de um cache antigo.

## 15. Peso do projeto, Git e publicação

Milhares de linhas adicionadas eram em boa parte YAML nativo de prefabs/animators e `.meta`, não 40 mil linhas de lógica. Esses arquivos são necessários para referências estáveis. Cache e arte duplicada são outro assunto. Uma auditoria reduziu a memória reportada das texturas do MainMenu de 134,1 para 71,9 MiB e pausa de 110,8 para 33,8 MiB; isso não é uma medição atual de RAM/FPS do jogo inteiro. Loading organiza a preparação; desempenho no combate precisa do Profiler.

- Versionar código, `.meta`, assets realmente usados, fontes úteis, cenas, prefabs, Docs, Packages e ProjectSettings.
- Não versionar Library, Temp, Obj, Logs, Builds, UserSettings, `.vs`, projetos gerados, caches de anexos ou licenças `.ulf/.alf`. Confira `.gitignore` da raiz e do projeto.
- Evitar criar/deletar milhares de arquivos, reimportar tudo ou trocar configurações globais por uma mudança pequena. Preserve pools/reuso de FX.
- Nome/e-mail de autoria usados a pedido do usuário: **Arthur Reis / elswzito@gmail.com**. Não confundir com credenciais Unity/GitHub.
- O usuário já autorizou commits/pushes específicos à main em pedidos anteriores; isso não significa autorização automática para publicar toda tarefa futura. Siga o pedido vigente, revise o diff e respeite proteções do repositório.
- Não colocar senhas/tokens/licenças em guia, código, logs ou commits. Secrets ficam nas configurações do GitHub.

Workflow `.github/workflows/windows-release.yml`: validação Python em push/PR para main; GameCI na Unity exata com imagem `unityci/editor:ubuntu-6000.6.4f1-windows-mono-3`; cache de Library; build pelo método `CrocodiloWindowsBuild.Build`; ZIP completo e artifact. Execuções de main publicam GitHub Release após upload; forks só fazem validação sem secrets, outras branches em execução manual não publicam Release.

Secrets de ativação: `UNITY_EMAIL`, `UNITY_PASSWORD`, `UNITY_LICENSE` (Personal) **ou** `UNITY_SERIAL` (Pro). O usuário informou configuração anterior; verificar presença/resultado no serviço sem imprimir valores se houver falha. Ao atualizar Unity, alinhar ProjectVersion, imagem Docker e suporte de build.

Saída local: `Builds/Windows/CrocodiloInvaders.exe`. Distribuir **pasta inteira/ZIP** com `UnityPlayer.dll`, `_Data` e dependências. Só o EXE não basta. O build exige MainMenu/PlayerMovementTest nas duas primeiras posições. Existe comando WebGL, mas a publicação automática atual é Windows. Não assumir sucesso de uma Release pela existência do workflow ou de um push.

## 16. Mapa para aprofundar

Limites do combate (18/10/2026): `ArenaViewportBounds2D` em ArenaBounds substitui a
parede direita fixa em X=2,27 por colisões nas quatro bordas visíveis da câmera,
adaptadas à proporção/tamanho da janela. Ligado em PlayerMovementTest, StageOneWaves
e GusiniBossTest; limites da Base não mudaram. Resize estreitando a tela recoloca a
nave dentro do novo viewport; PlayerMovement continua único escritor de velocidade.
As ferramentas temporárias GameplayCleanupTools/ArenaViewportPlayValidation foram
arquivadas em Logs/CurrentCombatUiCleanup/ArchivedTools e removidas de Assets após
a validação. Só o componente de limites é necessário em runtime. Ver `Docs/ArenaBounds.md`.

Limpeza de UI da mesma tarefa: página provisória OptionsPage do PauseMenu removida
(11 objetos, quatro TMP), junto dos campos optionsPage/optionsBackButton no
PauseMenuView. Opções atuais continuam usando AudioOptionsView. Excluídos somente
NewUXMLTemplate.uxml e UI Toolkit/PanelSettings + tema, confirmados sem dependentes.
TMP ativo de diálogos/cartas/nome/ranking/resultados continua.
LeaderboardRuntimeValidation não é órfão: é um validador de Editor acionado por flag;
PlayerState continua usado pelo port Jogo. Não remover por ausência de referências
serializadas. Auditoria e 130 checks de Play em `Logs/GameplayCleanup`.

Correção seguinte, solicitada com captura da GusiniBossTest: removidos de verdade
StartButton/Label (verde), PRONTO? e o estado de início provisório de WavesUI.
O painel Result fica salvo oculto e só mostra derrota/resultado; não deletar esse
fluxo funcional. WaveController inicia a abertura ao abrir a cena diretamente,
mas espera GameplayLoadingController quando vier do menu; BossDevelopmentStart
continua iniciando somente o boss na cena de teste. BossInfo/Name/Bar/Portrait foram
excluídos de BossHUD.prefab, junto de campos/eventos/fallback do Executor e Boss.Portrait
sem consumidores. Portraits dos diálogos são separados e continuam intactos.
Não resolver esta limpeza apenas desativando os objetos velhos em Awake.

Estado atual do HUB (pedido seguinte de 18/10/2026): novo `CrocHub.prefab` simplificado,
três objetos (raiz física, VisualRoot, CameraTarget), master e seis frames originais do
pacote `Croc_Hub_Simplified_Idle_v01.zip`. Sprite-swap a 6 FPS/1 s, PPU 80, **sem Run**.
`HubPlayerInput` lê ações HUB próprias; `HubCharacterMotor2D` aplica velocidade em
FixedUpdate; `HubCameraController2D` usa janela 20%, foco direcional 12%, amortecimento,
altura fixa e limites visuais. Built-in preservado; Pixel Perfect standalone 6.0.0 somente
na câmera da base. O mapa/parallax/setores preexistentes foram preservados. Não restaurar
o rig/câmera descartados. `Docs/BaseImprovisadaCamera.md` é a referência atual, substituindo
as descrições históricas de base vazia/câmera fixa abaixo e na seção 13. Validadores nativos
`HubCharacterTools.Validate` e `HubPlayValidation.Run`. Instalador do mapa preserva Characters.

Revisão de montagem seguinte: usuário pediu corrigir furos/posicionamento e aumentar Croc.
VisualRoot 2× (canvas 1,8), cápsula 0,88×1,7/offset 0,85 e CameraTarget Y 0,85; PNGs/Idle
preservados, sem Run. `BaseImprovisadaMapRepairTools` cria recortes nativos TerrainSlices
com faixas opacas, superfície alinhada à grade de 80 PPU e solo texturizado contínuo.
Validação renderiza 9600 pixels de largura sem Ground de fundo, conferindo alpha em vez
de só colisão. Cercas atrás das fachadas, suprimentos agrupados, portão/primeiro plano
assentados. Instalador do mapa usa essa receita e não restaura os encaixes antigos.
Ver Docs/BaseImprovisada_Map.md e Logs/MapRepair; ajustes atuais de câmera em
Docs/BaseImprovisadaCamera.md (subjectHeight 1,8 para não cortar o Croc em janela pequena).

Pausa do HUB: ESC/Start abre o mesmo PauseMenu.prefab do combate, com Continuar, Opções e
Voltar ao menu. O retorno direto anterior e seu componente/action foram removidos.
PauseController.playerInput aceita Behaviour (mantém PlayerInputReader nas cenas aéreas;
na base referencia HubPlayerInput), sem duplicar a implementação do pause. EventSystem
usa InputSystemUIInputModule. Teste específico: HubPlayValidation.RunPauseMenu.

BaseImprovisada (18/10/2026): implementação anterior de rig/Idle/Run/movimento/câmera removida por pedido do usuário. A cena recebeu depois o cenário v01 (Docs/BaseImprovisada_Map.md: escala 80 px = 1 unidade, Croc 48×72 = 0,9, sorting layers Background*/World*/AmbientEffects, BaseSector para desbloqueios futuros), ainda com câmera fixa e sem personagem. Não restaurar os assets descartados. Ver Docs/BaseImprovisadaRig.md e seção 13; logs BaseAndHudCleanup/Play.

Validação final da segunda rodada de dificuldade (18/10/2026): compilação/integração nativas e 49 checks de Play Mode passaram, incluindo três pontos distintos de estouro dentro da arena, tempos separados, gemas a 420 px/s e mergulhos a 600 px/s. Log `Logs/GusiniEggScatterBalance.log`. Sem playtest manual completo ou nova escuta da mixagem.

Rebalanceamento de 18/10/2026: player normal com 20 HP no prefab; kit GusiniBossTest permanece 240 HP. F2 do Gusini tem três ovos que viajam suavemente a pontos aleatórios da arena e estouram separados por cerca de 0,55 s, com margem das bordas e separação entre destinos. Cada ovo mantém pulsos/rachaduras após chegar, liberando cinco gemas a 420 px/s (antes 145). Gansinhos avançam em V a 180 px/s, slots 1/4 mergulham em toda a fase a 600 px/s; aviso de 400 ms e mira travada preservados. Marcadores ajustados à nova entrada e Rigidbody Continuous nos gansinhos. Patrulha ×1,5 (2,4 → 3,6 un/s), só na F2. Dados de quantidade/espaçamento inicial dos ovos e movimento no Inspector de GusiniPhaseTwo. A primeira rodada passou 45 checks antes da segunda aceleração/dispersão; ainda sem avaliar dificuldade numa run manual. Ver Docs/GusiniFase2.md.

| Tema | Documentação principal |
| --- | --- |
| Menu/entrada musical | [Menu](Crocodilo%20Invaders/Docs/Menu.md) |
| Loading/performance da UI | [Loading](Crocodilo%20Invaders/Docs/Loading.md) |
| Pausa/blur e áudio | [Pausa](Crocodilo%20Invaders/Docs/Pausa.md), [Opções](Crocodilo%20Invaders/Docs/OpcoesAudio.md) |
| Ondas e inimigos | [Ondas](Crocodilo%20Invaders/Docs/Ondas.md), [Inimigos](Crocodilo%20Invaders/Docs/Inimigos.md) |
| Cartas, pesos e feedback raro | [Cartas](Crocodilo%20Invaders/Docs/Cartas.md) |
| Cura e impacto | [Cura](Crocodilo%20Invaders/Docs/Cura.md), [Feedback](Crocodilo%20Invaders/Docs/FeedbackDeAcerto.md) |
| Player e dash | [Player animado](Crocodilo%20Invaders/Docs/PlayerAnimado.md), [Dash](Crocodilo%20Invaders/Docs/PlayerDash.md) |
| Gusini Fase 2 | [Fase 2](Crocodilo%20Invaders/Docs/GusiniFase2.md), [Áudio F2](Crocodilo%20Invaders/Docs/GusiniAudioFase2.md) |
| Diálogos, cenas narrativas e música do boss | [Cinemáticas](Crocodilo%20Invaders/Docs/Cinematicas.md) |
| Gusini, áudio e Executor | [Boss](Crocodilo%20Invaders/Docs/Boss.md), seções específicas de cada boss |
| HUDs | [Player](Crocodilo%20Invaders/Docs/PlayerHUD.md), [Gusini](Crocodilo%20Invaders/Docs/GusiniBossBar.md) |
| Anúncio de ondas e alerta do boss | [Anúncios](Crocodilo%20Invaders/Docs/AnunciosOndasBoss.md) |
| Mapa da BaseImprovisada | [Mapa](Crocodilo%20Invaders/Docs/BaseImprovisada_Map.md) |
| Parallax/cenário atual | [BattleSky](Crocodilo%20Invaders/Docs/BattleSky.md); Cenario.md registra o cenário anterior |
| Persistência e build | [Leaderboard](Crocodilo%20Invaders/Docs/Leaderboard.md), [BuildWindows](Crocodilo%20Invaders/Docs/BuildWindows.md) |
| Fontes/licenças | [Créditos](Crocodilo%20Invaders/Docs/Creditos.md), manifests em SourceArt |
| HQ/base futura/estudos | [HQ](Crocodilo%20Invaders/Docs/IntroHQ.md), [Base e missões](Crocodilo%20Invaders/Docs/BaseEMissoes.md), CrocodiloBaseIdle.md e CrocodiloBase64.md |
| Base mínima e rig a pé | [BaseImprovisada e rig](Crocodilo%20Invaders/Docs/BaseImprovisadaRig.md) |

Ao entregar uma mudança, explique em português o comportamento final, os arquivos principais quando úteis, como validou e o que falta verificar. Conserve as decisões já tomadas pelo usuário, especialmente a identidade da arte, soma aditiva de cartas, pausa correta e áudio breve do esquadrão.

### Música das ondas (18/10/2026)

WaveController adiciona WaveMusic automaticamente no Awake válido, sem modificar cenas abertas. Going Up (Ansimuz), WAV original fornecido em Crocodilo_Invaders_Musica_Ondas.zip, em Assets/Resources/Audio/Music/MUS_Crocodilo_Ondas_Loop.wav; streaming estéreo, Music, volume 0,28. Começa no anúncio da primeira horda após a abertura, mantém o loop nas hordas/cartas, fade de 0,9 s no anúncio do boss e parada garantida em BossStarted/RunEnded/disable/unload. Pausa via AudioListener; música do Gusini preservada. Proveniência/licença do pacote em SourceArt/WaveMusic; não importar previews como música.
