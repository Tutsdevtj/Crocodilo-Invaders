# Gusini — combate em duas fases

Boss ativo da onda 10 em `Assets/Scenes/Tests/PlayerMovementTest.unity` e
`Assets/Scenes/Tests/StageOneWaves.unity`. A integração preserva o fluxo de
vitória/derrota e a pontuação de `WaveController`. A F1 descrita abaixo agora leva à
[Fase 2 corrompida](GusiniFase2.md), importada do novo pacote. A Fase 3 foi descartada.
Antes da entrada, a onda do boss mostra ONDA 10 → penas → PERIGO! GOOSINI (2500 ms, tema
`GoosiniBossAlert` em `Boss.alertTheme`); o boss só é instanciado depois. Ver
[Anúncios](AnunciosOndasBoss.md).

## Fonte e importação

Arte proveniente de `Gusini_Boss_Completo.zip`, pasta `Gusini_Boss/boss_gusini`.
Os oito JSONs usados foram copiados, sem alteração, para `SourceArt/GusiniPhase1/`:

- `gusini_phase1/gusini_phase1_parts.json`;
- `gusini_phase1_anim/gusini_p1_idle/gusini_p1_idle.json`;
- `gusini_phase1_anim/gusini_p1_shoot/gusini_p1_shoot.json`;
- `gusini_phase1_anim/gusini_p1_projectile/gusini_p1_bullet.json`;
- `gusini_phase1_anim/gusini_p1_squadron_call/gusini_p1_squadron_call.json`;
- `gusini_phase1_anim/gansinho/gansinho.json`;
- `gusini_phase1_anim/gusini_p1_egg_drop/gusini_p1_egg_drop.json`;
- `gusini_phase1_anim/egg_parachute/egg_parachute.json`.

`SourceArt/GusiniPhase1/RuntimeManifest.json` registra cada arquivo importado,
SHA-256, dimensão, pivô, duração, eventos e a lista completa de arquivos ignorados.
Os **50 PNGs nativos** em `Assets/Art/Boss/GusiniPhase1` são cópias byte a byte:

| Subpasta | Arquivos | Quantidade |
| --- | --- | --- |
| gusini_p1_idle | gusini_p1_idle_0…3.png | 4 |
| gusini_p1_shoot | gusini_p1_shoot_0…4.png | 5 |
| gusini_p1_projectile | gusini_p1_bullet_0…1.png | 2 |
| gusini_p1_squadron_call | gusini_p1_squadron_call_0…6.png | 7 |
| gusini_p1_egg_drop | gusini_p1_egg_drop_0…8.png | 9 |
| gansinho | gansinho_0…3, gansinho_b_0…3, gansinho_beak_0…2.png | 11 |
| egg_parachute | egg_fall; egg_parachute_deploy_0…2; egg_parachute_open_0…1; egg_crack_1/2; egg_burst.png | 9 |
| egg_parachute | yolk_left, yolk_up_left, yolk_down_left.png | 3 |

Explicitamente ignorados: arquivos `preview`, `review`, `closeup`, `contact`,
`native_check`, `scale_check`, `formation_reference`, `sheet_x4/x6/x8/x10`, GIFs,
exploded/comparison, spritesheets e duplicatas estáticas. Exemplos reais:
`gusini_p1_idle_preview_x4.gif`, `gusini_p1_idle_review.png`,
`gusini_p1_idle_sheet_x4.png`, `gusini_p1_scarf_idle_poses_x4.png` e
`gansinho_formation_V_reference_x2.png`. Partes modulares e FX já compostos nos
frames finais não foram duplicados em runtime. Paraquedas modular e collapsed
opcional não são necessários ao fluxo composto utilizado.

Importação feita com APIs do Editor: **Point, sem compressão, sem mipmaps,
Full Rect e 100 PPU**, consistente com o Player. Escala uniforme 2 nos prefabs
do boss/projéteis/ovo/formação; os gansinhos filhos têm escala local 1.
Pivôs normalizados dos JSONs são usados literalmente, inclusive arredondamentos.
Origens são calculadas em relação a esses mesmos pivôs, com inversão do Y da imagem.
Não há recorte, redimensionamento ou redesenho dos PNGs.

## Estrutura e responsabilidades

`Assets/Prefabs/Boss/Gusini.prefab` possui Rigidbody2D, duas hitboxes trigger
(fuselagem e cabeça), Health, EnemyMovement, Boss, GusiniCombat,
GusiniPresentation, GusiniAudio, Animator e SpriteHitFlash. Filhos:
`Visual` (SpriteRenderer), `ShotOrigin`, `EggDropOrigin` e `SquadronOrigin`.
As origens ficam fora das curvas de animação. A raiz de ameaças criada durante a
luta pertence à mesma cena, mas não segue a movimentação do boss.

- `IBossCombatController`: somente SetTarget/StopCombat. `Boss` conserva entrada,
  Health, invulnerabilidade até chegar, hitboxes, begin/stop, morte e pontos.
- `GusiniCombat`: rotação dos ataques, consumo único dos releases e propriedade
  de todas as ameaças. Primeiro ataque sempre Burst; depois sacolas embaralhadas
  dos três ataques, sem repetição imediata na passagem entre sacolas.
- `GusiniPresentation`: triggers, ponte pequena de eventos, áudio opcional e fade
  na morte. Usa os sprites completos finais, incluindo cachecol, recoil e FX.
- `GusiniSquadron`: movimento único do bloco e configuração dos cinco slots.
- `GusiniSquadronGoose`: Health/contact damage, variante de voo e bico independente.
- `GusiniParachuteEgg`: queda, desaceleração, fuse e lançamento das três gemas.
- `BossHUD`: seleciona a barra exclusiva do Gusini Fase 1, com preenchimento
  contínuo e efeitos em `BossBarFX`; detalhes em `Docs/GusiniBossBar.md`.
  Executor mantém nome via `Boss.DisplayName`, retrato via `Boss.Portrait`,
  sprites de HP e enrage legados. `WavesUI` não mostra mais o nome em texto: o boss com
  `Boss.alertTheme` recebe o alerta PERIGO! (Gusini: `GoosiniBossAlert`); sem tema, só "ONDA 10".
  Gusini Fase 1 não possui enrage.

## Animator e eventos

`Assets/Animations/Boss/GusiniPhase1/Gusini.controller`: Idle é default e loop.
Triggers **Shoot**, **Squadron** e **EggDrop** levam aos estados Shoot,
SquadronCall e EggDrop. One-shots retornam ao Idle ao terminar o clip, com
exitTime=1 e transição de duração zero, sem ciclos adicionais ou blend.
Não há AnyState que possa rearmar o mesmo ataque, root motion nem curvas de Transform.
Todos os clips alteram somente `Visual/SpriteRenderer.m_Sprite`.

As chaves de sprite ficam nos timestamps cumulativos; frameRate=1000 fornece a
grade de milissegundos, sem converter as durações individuais em FPS uniforme.
A última chave mantém o último sprite até o fim. Os tempos abaixo vêm dos JSONs:
No balanceamento atual, `GusiniPresentation.animationSpeed=2` reproduz o Animator
do boss em 2×: os eventos de Shoot/Squadron/Egg acontecem em 90/290/200 ms reais.
Os PNGs, curvas e timestamps nativos não foram regravados. O Animator do ovo mantém
sua velocidade original para preservar os avisos de abertura/crack/burst.

| Clip | Durações por frame, ms | Total | Evento (frame 0-based / instante) |
| --- | --- | --- | --- |
| Idle | 130,170,130,170 | 600 | loop |
| Shoot | 80,100,60,80,110 | 430 | OnShootRelease: 2 / 180 ms |
| SquadronCall | 70,100,110,150,150,180,110 | 870 | OnWhistle: 2 / 170 ms; OnSquadronRelease: 5 / 580 ms |
| EggDrop | 80,100,60,70,90,90,70,60,90 | 710 | OnEggRelease: 5 / 400 ms |
| Bullet | 60,60 | 120 | loop |
| FlightA/B | 80,70,80,70 | 300 | loop independente do bico |
| EggDeploy | 60,70,80 | 210 | OnParachuteOpened no final |
| EggOpen | 160,160 | 320 | loop |
| EggCrack1 / EggCrack2 | 260 / 220 | 260 / 220 | transição direta |
| EggBurst | 120 | 120 | OnYolkRelease no início; OnBurstFinished no final |

Os três clips de ataque também enviam `OnAttackFinished` no final, com o tipo do
ataque. O próximo trigger só é armado após retornar realmente a Idle. Eventos de
release exigem boss vivo/pronto, BattleActive, alvo vivo, componente habilitado,
animação correspondente pendente e release ainda não consumido. Chamadas repetidas
ou atrasadas após encerrar combate não criam ameaças.

## Ataques

**Burst:** cinco reproduções separadas de Shoot, com 25 ms de intervalo após cada
clip anterior retornar a Idle. A animação acelerada reduz também a cadência entre
disparos. Cada release cria um `GusiniBullet`, via Projectile
existente, exatamente no muzzle (22,142). Origem local
(-0,980128,-0,240080), antes da escala 2; a precisão adicional corresponde ao pivô
normalizado arredondado do JSON. Bullet anima dois frames e viaja à esquerda sem
rotacionar a arte.

**Squadron:** o assovio visual começa em 170 ms nativos; a primeira formação só
nasce no comando, em 580 ms nativos (290 ms com playback 2×). Cada chamada lança
três formações, separadas por 220 ms: quinze pássaros no total. As alturas são
sorteadas em três faixas, evitando empilhar as formações no mesmo ponto.
As chamadas extras são canceladas se o combate parar.
SquadronOrigin fica à direita do boss. Cada `GusiniSquadron` carrega cinco instâncias separadas do prefab
`GusiniSquadronGoose`. Offsets locais em unidades: (0,0), (0,34,+0,18),
(0,34,-0,18), (0,68,+0,36), (0,68,-0,36), multiplicados por spacing e escala da raiz.
O líder fica mais à esquerda. Variantes A/B/A/A/B e fases de voo 0/1/1/2/2
seguem o JSON; fase normalizada é calculada pelas durações reais, não por i/4.

O filho Beak tem posição local zero, mesmo pivô do corpo e sortingOrder+1.
`Sing(delay,pitch)` usa estados 0→1→2→1→0, durações 60/60/120/70 ms;
o honk opcional dispara aos 120 ms. Coral: delays 0/140/140/280/280 ms.
O voo continua em seu Animator, sem ser reiniciado pelo canto. Geese podem ser
atingidos por projéteis do Player e causam um contato antes de serem removidos.

**Egg:** dois ovos no mesmo evento/frame de release em (94,152), origem local (-0,260128,-0,340080), inicialmente
atrás do boss (sorting 2, contra 3 da fuselagem), para preservar a sobreposição da
escotilha. A sequência final completa da porta não é recriada por Transform.
Após queda livre curta, passa por todos os três frames Deploy e desacelera durante
os 210 ms. Com o paraquedas aberto, cada ovo avança a 5,2 unidades/s até um destino
XY sorteado no centro: X [-3;1], Y [-1,4;3,2]. Os dois destinos ficam separados
por pelo menos 1,6 unidade na configuração padrão. Ao chegar, o ovo para, espera
o fuse de 250 ms e executa Crack1→Crack2→Burst, explodindo exatamente no destino.
Open anima em loop durante o deslocamento e o fuse. Cada ovo tem lifetime de
segurança de 10 s, independente da distância configurada.
O evento inicial do Burst cria três Projectile separados no pivô do ovo; esses
objetos são filhos da raiz de ameaças, sobrevivendo ao desaparecimento do ovo.
Direções Unity: (-1,0), (-0,72,+0,69), (-0,72,-0,69), normalizadas por Projectile.
As três sprites já orientadas não são rotacionadas. EggFall não causa contact damage;
a ameaça ofensiva é a explosão das três gemas, com aviso de cracks.

## Valores iniciais e configuração

| Local | Valores |
| --- | --- |
| Gusini Health / Boss | 258 HP; 40.000 pontos; fade/defeat 3 s; patrol (5,6,-0,2,0,8,3,8) |
| EnemyMovement | entrada 4,5; patrulha 2,4; brakingDistance 0,5 |
| GusiniPresentation | animationSpeed 2× |
| GusiniCombat | initialDelay 0,35 s; recovery 0,35 s; burstShots 5; burstShotInterval 0,025 s; bulletSpeed 7,5; squadronGroups 3; squadronGroupInterval 0,22 s; squadronHeightRange [-1,6;4,3] |
| GusiniCombat / ovos | eggsPerDrop 2; eggBurstArea: x=-3, y=-1,4, largura=4, altura=4,6; eggTargetSeparation 1,6 |
| GusiniBullet / Projectile | dano 1; lifetime 5 s; alvo layer Player (8) |
| GusiniSquadron | speed 7,2; spacing 1; lifetime 5 s; despawnX -12, incluindo margem para a cauda do V |
| Goose / Health e controller | HP 2; contactDamage 1; delays, fases, variantes e bicos serializados |
| GusiniParachuteEgg | initialFallSpeed 3,4; freeFallDuration 0,1 s; parachuteFallSpeed 0,65; deployDuration 0,21 s; travelSpeed 5,2; fuseDuration 0,25 s após chegada; yolkSpeed 6; damage 1; lifetime 10 s |
| GusiniYolkLeft/UpLeft/DownLeft | Projectile reutilizado, lifetime 5 s; direções e prefabs serializados no ovo |

O áudio está integrado pelo catálogo `Assets/Data/GusiniAudio.asset`, usando os
24 WAVs finais do pacote `Gusini_Audio`. AudioSources seguem o mixer Effects e a
pausa global. Nenhum arquivo de áudio foi inventado ou substituído.

## Áudio e efeito curto da esquadrilha

Fontes: `Gusini_Audio/Phase1`, `gusini_phase1_audio.json` e `README_AUDIO.md`.
Os WAVs são cópias byte a byte em `Assets/Audio/Boss/GusiniPhase1`, sem importar
`_candidates`, `_previews` ou ferramentas de geração. Manifesto e notas ficam em
`SourceArt/GusiniAudio`. São 20 efeitos / 24 WAVs: motor, três canhões, preparação
do canhão, assovio, passagem da esquadrilha, três registros de voz, mecanismos da
escotilha, deploy, cracks, burst e três variantes de dano. CommandWhoosh,
FlutterLoop e YolkWhoosh foram importados, mas permanecem desligados por padrão.

`GusiniAudio` centraliza o áudio em um pool de **dez AudioSources**, sem criar uma
fonte por pássaro. `GusiniAudioConfig` guarda clips, ganhos, pitch, cooldown e
limites por efeito; pode ser ajustado no Inspector. O orçamento linear máximo é
0,82 para as vozes do Gusini, preservando headroom deste conjunto. Tiros/burst
reduzem temporariamente motor, passagem e grasnado para 65% do ganho local. Este
limite não é uma promessa sobre a soma de toda a música/outros efeitos do jogo.

Cada ataque de esquadrilha toca **um único grasnado Mid de 190 ms**, compartilhado
pelas três formações (quinze pássaros). A sequência musical foi removida. Low e
High ficam desativados; não há agendamento de notas nem áudio individual nos
pássaros. O Mid usa ganho 0,09, pitch fixo 1 e cooldown de dois segundos.
`CueSquadron` é chamado uma vez por ataque, antes do loop de formações. Aguarda
o assovio terminar mais 30 ms, toca uma vez e elimina o agendamento. A passagem
coletiva fica em 0,04 e o assovio em 0,22. As animações dos bicos continuam visuais.
Pausa congela o efeito pendente; interromper combate o descarta imediatamente.

Canhões alternam 01/02/03; o som de preparar toca somente no primeiro tiro da
rajada. Hits alternam três variantes com cooldown de 70 ms e máximo de duas
vozes. Os ovos simultâneos compartilham o drop/deploy coincidente; bursts dentro
de 35 ms compartilham um transiente com pequeno reforço de ganho. Cada ovo mantém
seus cracks nos eventos próprios. O som do burst termina no pool do boss, mesmo
depois de o sprite do ovo desaparecer. Interromper combate para motor, grasnado,
loops e caudas imediatamente.

Eventos de áudio adicionados, preservando todas as curvas/timings visuais e eventos
de gameplay: Shoot/OnShootWindup em 80 ms; EggDrop/OnHatchLatch em 80 ms,
OnHatchOpen em 180 ms e OnHatchClose em 490 ms; OnCrack1 e OnCrack2 no início de
seus clips. Tempos de boss são nativos e seguem o playback 2×. Disparo, assovio,
drop, deploy e burst reaproveitam os pontos já existentes.

Importação pelo Editor: one-shots PCM / Decompress On Load, loops Vorbis 0,7 /
Compressed In Memory, preload ligado, sem background load. Mono preservado;
somente o flyby mantém estéreo. Todos usam `GameAudio/Effects`, respeitando volume
geral, efeitos e AudioListener.pause.

`GusiniAudioValidation` integra o menu de validação do boss e confere catálogo,
importação, camadas opcionais e a restrição a um único registro curto e baixo.
O teste de regressão do efeito curto observa duas chamadas completas, conferindo
um grasnado para cada três formações, ausência de Low/High, intervalo após o
assovio, pausa do efeito pendente, canhões e descarte ao interromper combate.
As 24 cópias de WAV foram comparadas com a fonte na integração original.

Limite de avaliação: não há escuta crítica disponível nesta sessão. AudioRenderer
retornou captura silenciosa neste ambiente, inclusive fora de batchmode, apesar
de o transporte nativo avançar. Portanto, não foi validado o pico do mix completo
do jogo por captura. A antiga prévia musical foi descartada como referência do
comportamento atual. O equilíbrio com a música precisa de escuta no jogo.

Bullets, formação/gansinhos, ovos e gemas são descendentes da raiz rastreada por
GusiniCombat. StopBattle, morte do boss/alvo e OnDisable desativam a raiz
imediatamente antes de Destroy. Descarregar a cena também a destrói. Não há busca
global contínua de ameaças. Animator, physics, voo, bico e fuse usam tempo escalado.

## Desenvolvimento e validação

Menu **Crocodilo → Boss → Open Gusini Phase 1 test** abre
`Assets/Scenes/Tests/GusiniBossTest.unity`; Play inicia diretamente a luta.
BossDevelopmentStart e StartBossForDevelopment existem somente no Editor. A cena
não foi adicionada ao build e o jogo normal continua chegando ao boss pela onda 10.
Trocar bossPrefab dessa cena para ExecutorV9 permite testar o legado.

**Crocodilo → Boss → Validate Gusini Phase 1 assets** confere os 50 imports/pivôs,
13 clips, tempos variáveis/eventos, ausência de curvas de Transform, estados,
prefabs novos/legado, scripts/referências e links nas três cenas. A validação usa
RuntimeManifest e não altera assets.

Na integração original, foram executados testes na Unity **6000.6.4f1**, em Play Mode real via batchmode com
Direct3D 11: **122 verificações do combate + 11 verificações adicionais**,
além de um teste dedicado em Play Mode do retrato genérico do HUD.
Após o aumento de dificuldade, uma nova execução real de Play Mode passou em
**87 verificações**: cinco tiros por Burst com intervalos menores que 340 ms,
balas a 7,5, três formações de cinco pássaros por chamada, movimento dos grupos
a 7,2, dois ovos no mesmo frame, destinos aleatórios centrais separados, chegada
antes do burst e seis gemas a 6 unidades/s. Também verificou playback 2×,
patrulha acelerada, pausa da trajetória diagonal, despawn e cancelamento dos grupos
agendados na morte do Player. Scripts temporários dessa validação foram removidos.
Foram observados todos os frames finais de Idle/Shoot/SquadronCall/EggDrop,
Flight A/B, bicos e todos os nove estágios visuais do ovo. Capturas renderizadas
pela câmera foram inspecionadas: muzzle, escotilha, deploy, formação e burst.
As curvas/pivôs/timestamps também passaram na validação nativa do Editor.

Verificado em execução: entrada e invulnerabilidade; primeiro Burst com exatamente
três releases; eventos nos sprites esperados e rejeição de duplicatas; dez padrões
para conferir as sacolas/ausência de repetição; V estável e cinco gansinhos;
despawn; canto independente; desaceleração do ovo; três gemas com direções corretas;
pausa de boss/bullets/ovo/fuse/formação/voo/bico; dano físico de 1 por bullet,
gansinho e gema; destruição de gansinho por Projectile do Player; dano branco no
Visual; HUD/nome/barra; vitória e pontos; derrota por morte real do Player;
cleanup imediato por StopBattle/OnDisable/morte; unload de cena. Executor também
foi instanciado em Play Mode, atacou e acionou enrage após receber dano.

`CrocodiloWindowsBuild.ValidateProject` passou: compilação, scripts de todos os
prefabs, cenas de build, catálogo de cartas, pools/probabilidades e mixer.
O validador do Gusini verificou referências de objetos serializados dos nove prefabs
(oito novos e Executor). As 50 imagens tiveram SHA-256 comparado com o ZIP.
Os scripts temporários de geração e de Play Mode foram removidos de Assets após
os testes; evidências e cópias ficam somente em Logs, ignorado pelo Git.

Limites: não foi produzido um novo .exe nesta tarefa; o equilíbrio de dificuldade
ainda precisa de avaliação humana com as melhorias obtidas nas nove hordas.
O áudio já tem clips atribuídos; a limitação de escuta/captura está descrita na
seção de áudio. Nenhum ajuste manual é necessário para iniciar a luta. Refine os
valores do Inspector após jogar. Fases 2/3 continuam futuras.

## Arquivos criados e alterados

Scripts novos: `Assets/Scripts/Controllers/IBossCombatController.cs`,
`GusiniCombat.cs`, `GusiniSquadron.cs`, `GusiniSquadronGoose.cs`,
`GusiniParachuteEgg.cs`, `BossDevelopmentStart.cs`;
`Assets/Scripts/Presentation/GusiniPresentation.cs`;
`Assets/Editor/GusiniBossTools.cs`.
Áudio: `GusiniAudio.cs`, `GusiniAudioConfig.cs`, `GusiniAudioValidation.cs`,
`Assets/Data/GusiniAudio.asset` e os 24 WAVs finais. Integração altera apresentação,
combate, gansinho, formação, ovo e adiciona seis eventos sonoros aos clips existentes.

Prefabs novos em `Assets/Prefabs/Boss`: Gusini, GusiniBullet, GusiniSquadron,
GusiniSquadronGoose, GusiniParachuteEgg, GusiniYolkLeft, GusiniYolkUpLeft,
GusiniYolkDownLeft. Também novos: cena GusiniBossTest, 50 sprites/13 clips/5
controllers nos diretórios GusiniPhase1 e os oito JSONs fonte + RuntimeManifest
fora de Assets. Metas foram gerados pelo Editor.

Alterados: `Boss.cs`, `BossCombat.cs`, `BossPresentation.cs`, `BossHUD.cs`,
`WavesUI.cs`, `WaveController.cs`; ExecutorV9.prefab recebe DisplayName e referência
ao seu retrato original; BossHUD.prefab liga o retrato genérico e preserva seu
aspecto; PlayerMovementTest e StageOneWaves somente trocam bossPrefab; este Docs/Boss.md.
Player, dash, cartas, projeto legado e alterações locais anteriores foram
preservados. Não foi feito commit.

# Executor V-9 (legado preservado)

As seções seguintes documentam a implementação anterior e seus valores históricos.
O prefab e os ataques continuam disponíveis; para usá-lo, configure ExecutorV9
como bossPrefab. As cenas principais agora usam Gusini.

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

## Gusini Fase 2

A integração atual inclui a transformação F1 → F2, quatro ataques e barra corrompida. A morte da F1 não encerra a run. Veja [Gusini Fase 2](GusiniFase2.md) para comportamento, assets e validação. O Executor descrito nas seções anteriores continua preservado.
