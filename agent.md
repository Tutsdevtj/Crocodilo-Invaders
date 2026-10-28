**Macaquini v21 e entrega no fork — 23/10/2026:** segunda fase da selva integrada com jetpack na primeira forma, transformação física da banana, peito claro e olhos verdes preservados, HUD próprio por forma, pêndulo que exige dash e chute com volta ao fundo. V21 espelha o desenho durante a passagem distante e adiciona arco de pressão, três rastros de vento e onda de soltura. Validação nativa v21: 90 verificações sem erros; campanha v20: 93 sem erros; checkout: 2416 GUIDs válidos. Build e vídeo em Builds/Windows-Macaquini-Chute-v21 e Docs/ArtPreviews/MacaquiniKick_v21. Usuário autorizou commits separados e push somente ao fork Picolii/Crocodilo-Invaders, na branch existente codex/cartas-expressoes-v12; não abrir PR para Tutsdevtj/Crocodilo-Invaders. Builds, caches Python e preferências locais do editor não devem ser versionados.

**Teste de quadros completos v08 — 23/10/2026 (bancada separada; campanha ainda v07):** usuário pediu testar animação com pixel arts completas já montadas, incluindo cipó, pernas, pés, rabo e rosto, evitando peças separadas. image_gen integrado gerou quatro folhas em SourceArt/Macaquini_v08 com prompts e manifesto; exportador prepare_macaquini_frames_v8.cjs extrai componentes inteiros/registro pelo topo do cipó,384x384/alfa binário/paleta40cores.56desenhos distintos:16balanço por forma,8ataque por forma,8rasgo. MacaquiniFrameAnimation troca um único SpriteRenderer, sem rig/IK; balanço16cels em ida/volta3.75s, disparo1.32s, rasgo4.22s. Cinco clipes nativos .anim em Assets/Animations/Boss/MacaquiniFullFrames_v08 (uma trilhaSpriteRenderer.m_Sprite). Novo prefabMacaquiniFullFramesPreview e cenaTests/MacaquiniFrameAnimationTest fora dos buildsettingsda campanha. Controles Canvas/novoInputSystem:Espaçopausa,setaspasso,1/2formas,Bbalanço,Adisparo,Trasgo,Escsair;miniaturas clicáveis,velocidade.25x–2x,repetiçãoataque. Selva aprovada reutilizada. Validação final23checks zeroerros,56poses noPlayerLoop,mouse raycast,pausa congela fundo,3res;2090GUIDs válidos. Vídeo Docs/ArtPreviews/Macaquini_v08/macaquini-quadros-completos-v08.mp4 real24s semáudio, decodificado semerros. Build independente Windows-Macaquini-Quadros-v08/MacaquiniQuadros.exe120144315bytes, startup8s semexceções. ZIP Builds/Pacotes/Macaquini-Teste-Quadros-Completos-v08-Windows.zip65396956bytes,324arquivos verificadosSHA256, hash882B23C332CBA9D70D62964DA60D9B3722D6EA65363ADC45817253D1F79889BF; inclui PNGs/metas, clipes, prefab/controlador e fontes/prompts em Arte. Docs/Fase2MacaquiniQuadros_v08.md. NÃO afirmar que substituiu boss da campanha ou aprovação artística. Semcommit.

**Macaquini v07 — 23/10/2026 (atual):** usuário achou mãos v06 minúsculas e pediu aumentá-las e suavizar/anatomizar movimento, pesquisando biblioteca/recurso. Mãos0.80→1.35(+68.75%), PalmContact=.13*HandScale para arma/cipó/rasgo. Boss continua escala1.10. Pacote oficial com.unity.2d.animation16.0.1 instalado via PackageManager.Client.Add; solver UnityEngine.U2D.IK.Limb.Solve efetivamente integrado no lugar do cálculo customizado, arrays reutilizados. Alcance da mão em curva cúbica externa com easing quintic, destino antecipado/fixo durante fechamento; respiração contínua, roll/mira amortecidos, ombros interpolados na transformação. EnemyMovement.smoothDashes defaultfalse ativado só no Macaquini: SmoothDamp de velocidade/freio próximo do destino. Mesma arte/fundo/HUD/arma/duas formas. Motion-v7 passou38checks semerro, pegadas0.0000, cotovelo158graus, arco3.46u; campanha93 e Gusini49 também passaram (180verificações). Checkout2021GUIDs válidos, diffcheck limpo. Vídeo real com áudio Docs/ArtPreviews/Macaquini_v07/macaquini-ik-v07.mp4 decodificado sem erros. Build Windows-Macaquini-v07 concluída169131441bytes, bibliotecaIK presente; startup8s sem exceções. ZIP Builds/Pacotes/CrocodiloInvaders-Macaquini-DuasFormas-v07-Windows.zip100010890bytes,190arquivos verificadosSHA256, hash31086A3E342B8041054E80825C2AEC95C898CC1F9818D03E5AF1B1B9AA68265B. Detalhes em Docs/Fase2Macaquini_v07.md e Logs/JungleStage. Ferramenta Editor MacaquiniAnimationPackageTools preservada para instalar viaClientAPI, nunca editar manifest à mão. Sem commit.

**Macaquini v06 — 22/10/2026 (atual):** usuário aprovou direção v05 e pediu apenas corrigir emendas no ombro/cotovelo, proporção das mãos e reduzir tamanho total. Boss escala1.25→1.10 (-12%), mãos0.8(-20%), contato local0.13→0.104 inclusive arma/cipó/rasgo. Quatro malhas de membros arredondadas e quatro sobreposições da pelagem original da master (ombros seguem tronco, cotovelos antebraço), sem nova arte nem mudança no fundo aprovado. MacaquiniSpriteMesh aplica geometria em Start com SpriteMeshType.Tight e vértices em pixels: OverrideGeometry não funciona no instalador/Awake; tentativas anteriores descartadas. Sprites temporários destruídos ao sair. Overlaps ocultos no prefab até recortados. Captura nativa38checks passou sem erro de gameplay, oito contornos confirmados, contatos0.0000, cotovelo154graus;2020GUIDs válidos, diffcheck limpo. Vídeo real com áudio Docs/ArtPreviews/Macaquini_v06/macaquini-encaixes-v06.mp4 decodificado sem erros. Build Windows-Macaquini-v06 concluída168456579bytes; startup8s sem exceções; ZIP Builds/Pacotes/CrocodiloInvaders-Macaquini-DuasFormas-v06-Windows.zip99759403bytes,180arquivos verificadosSHA256, hash4C0CEF00C059BEAE44E54AA5272F630A9616E30ABBAF1ACD8925AFECA1181BFA. Detalhes em Docs/Fase2Macaquini_v06.md/Logs/JungleStage. Campanha93checks e Gusini49 permanecem referência v05/v04, sem repetir testes de regras/HUD inalterados. Sem commit.

**Macaquini v05 — 22/10/2026 (estado atual):** usuário rejeitou rosto de lobisomem e esclareceu macaco preso numa banana que rasga metade na segunda forma; depois pediu acabamento detalhado com rosto mais reconhecível de macaco. Novas fontes image_gen em SourceArt/Macaquini_v05, exportador prepare_macaquini_v5.cjs, sprites320x384, mãos96x96, paleta32cores/alfa binário. Braços reais com dois ossos, quatro pegadas e troca de cipó por limites mínimo/máximo de alcance; pegada limitada a7.6y para enquadramento. Banana orgânica original v04 com sockets/recoil por quadro. HUD reutiliza BossBarFX do Gusini com nome/medalhões próprios, sem barra genérica. Health180+180, MacaquiniPhaseTwo intercepta primeiro esgotamento, anima rasgo nativo4.22s com fragmentos da própria casca e mão seguindo contato, revela macaco e conserva casca inferior; pontuação só na morte final, leque5→7 com aviso preservado. Fundo aprovado da selva v02 intacto. Arte v03 rejeitada arquivada fora de Assets; v04 é base anterior. Campanha93checks e movimento37checks passaram, zero erros de gameplay; erros de sockets0.0000, cotovelo157.7graus, arco3.89u; clip de rasgo efetivamente animado;2010GUIDs válidos. Regressão Gusini49 já passou após última mudança no HUD compartilhado v04. Prévia real com áudio em Docs/ArtPreviews/Macaquini_v05/macaquini-duas-formas-v05.mp4, decodificação verificada. Build Windows-Macaquini-v05 concluída168258788bytes; startup8s sem exceções. ZIP Builds/Pacotes/CrocodiloInvaders-Macaquini-DuasFormas-v05-Windows.zip99578307bytes,180arquivos verificadosSHA256, hashFEEE54FBD8A5DEC4D130DBF10CAC2649C140B99FF8B2A794A77BA7BACA125850. Detalhes em Docs/Fase2Macaquini_v05.md e Logs/JungleStage. NÃO retornar a arte v02/v03 ou afirmar aprovação artística humana. Sem commit.

**Selva fechada e Macaquini v02 — 22/10/2026 (pedido posterior):** usuário pediu mais movimento, novas pixel arts e fundo em vários níveis como a fase 1. Estado atual substitui o primeiro retrabalho descrito abaixo: dez poses completas usadas (0–5,7,8,10,11; 6 aponta errado e 9 tem membro extra, ambas excluídas), novas fontes image_gen em SourceArt/JungleMotion_v02, exportador Tools/Art/prepare_jungle_motion_v2.cjs. Renderer Visual/ActionDrawings substitui visual modular aberto; módulos preservados ocultos para sockets/eventos. Pivô corporal e mão/boca da arma medidos; OnShot escolhe pose e socket antes de gerar tiro. Recover alterna âncoras via crista a 6,8u/s; captura mediu 4,05u verticais. Selva agora fechada com mata opaca, árvores distantes próprias, troncos médios, copa, samambaias e oito cipós; quatro faixas transparentes reutilizam ScrollingLayer/BattleSky a .16/.58/1.25/2.15u/s, 30 folhas, tempo escalado. Fundo usa Default com ordens negativas, mantendo combate legível. Checkouts1954GUIDs; PlayMode fase completa80checks e movimento26checks, zero erros de gameplay; cobertura em 800x600/1200x720/1920x1080, dez poses observadas, pausa e limpeza. Teste da vespa passou a esperar velocidade física após FixedUpdate (três ticks do Editor não garantiam física); gameplay da vespa intacto. Build Windows-StageTwo-v02 aprovado, startupheadless8s, vídeo real com áudio em Docs/ArtPreviews/JungleMotion_v02/macaquini-selva-fechada-v02.mp4. ZIP v02 Selva-Fechada em Builds/Pacotes,180arquivos verificadosSHA; detalhes e limites em Docs/Fase2SelvaMovimento_v02.md. Master/fontes anteriores e pacote anterior preservados; não afirmar aprovação artística nem transformação integrada. Sem commit.
**Brilho sem música e volume por oferta v25 — 22/10/2026:** usuário pediu tirar a música de fundo, deixar somente brilho e evitar estouro com múltiplas míticas. Dois loops6.4s substituídos por glints inarmônicos curtos/irregulares, sem pad/acordes/melodia/batida; volumes .24/.28, uma fonte compartilhada por oferta. CardRewardView.Bind conta cartas >=Legendary; CardRarityAudio.ConfigureOverlap aplica 1/N por carta alta (2=.5,3=1/3), sem mudar ganhos das outras. Nova receita ativa Tools/Audio/prepare_card_audio_v25.py recria7sons v24+2brilhos; manifesto completo9 em SourceArt/CardAudio_v25_Sparkle. Picos simulados1/2/3míticas .3208/.3097/.2557; limite125combinações .7012; captura real pico.0963. Suíte PlayMode122checks/errors0 inclui duas míticas+comum, ganho3míticas e única cintilação. Checkout1890GUIDs, importação, buildWindows e startup8s passaram. Vídeo real Docs/ArtPreviews/CardAudio_v25/brilho-sem-musica-volume-controlado.mp4 (789frames30fps,5rerolls/3s,3 e2míticas); script temporário só Builds. RAR5 CRC/conteúdo aprovados Builds/Pacotes/CrocodiloInvaders-DEV-BrilhoSemMusica-v25-Windows.rar; SHA256SUMS-v25.txt. Docs AudioCartasBrilho_v25.md/EfeitosCartas.md e empacotador atualizados. Sem commit novo.

**Sons originais v24 — 22/10/2026:** usuário pediu remover todos os pacotes de áudio adicionados e criar sons próprios no estilo do jogo. Removidos SourceArt/CardAudio_v22_CC0 (3 pacotes/ZIPs/originais), SourceArt/CardAudio_v23_Reward, geradores v22/v23 e créditos Kenney do build de trabalho. Nove WAVs ativos agora são síntese própria (pulso arcade suave, cristais FM, pad maior, shuffle de ruído com seed), sem samples externos. Receita autônoma Tools/Audio/prepare_card_audio_v24.py (NumPy+wave); manifesto/procedência SourceArt/CardAudio_v24_Original. Metas/GUIDs/mixer e controles preservados. Reveal0.26/.62/.94/1.40/1.95s; loops6.4s. Mistura3míticas+tesouro pico.7453. Importação, PlayMode118checks/errors0, checkout1890GUIDs e build Windows passaram; startup8s sem exceções listadas. Vídeo real Docs/ArtPreviews/CardAudio_v24/sons-originais-5-rerolls.mp4 (782frames/30fps, 5rerolls/3s de espera). RAR5 CRC/conteúdo aprovados Builds/Pacotes/CrocodiloInvaders-DEV-SonsOriginais-v24-Windows.rar; checksumSHA256SUMS-v24.txt. Pacotes/vídeos anteriores são históricos com sons anteriores. Docs ativos EfeitosCartas.md e AudioCartasOriginais_v24.md. Sem commit novo.

**Áudio de recompensa v23 — 22/10/2026:** usuário rejeitou o clima de derrota da v22. Nove WAVs substituídos preservando GUIDs/referências: sinos CC0 glass_001 afinados, frases ascendentes em Dó maior, acordes C/F/G/C e arpejos cintilantes nos loops lendário/mítico (6,4s); reroll tick_001. Sem vinhetas NES/explosões. Receita `Tools/Audio/prepare_card_audio_v23.py`, manifesto/origens `SourceArt/CardAudio_v23_Reward`, docs `Docs/AudioCartasRecompensa_v23.md`. Backup anterior na raiz `Backup/Cartas/2026-10-22_AntesAudioRecompensa_v23`. Importação e 118 checks reais de Play Mode passaram (errors=0). Prévia real com áudio: `Docs/ArtPreviews/CardAudio_v23/recompensas-tesouro-5-rerolls.mp4`, 5 rerolls/3s de espera, 778 frames/30fps. Script temporário de captura preservado só em Builds. O RAR v22 continua histórico com áudios anteriores; não foi gerado novo pacote nesta revisão.

# Crocodilo Invaders — contexto para agentes de código e arte

**Retrabalho visual da selva após crítica do usuário (22/10/2026, posterior à integração inicial):** Macaquini deixou a montagem estática: seis módulos articulados nos pivôs, arma filha do braço com pegada e muzzle próprios, mão elevada acompanhada pelo cipó, balanço/pernas/cauda, recuo e trocas de posição via EnemyMovement.DashTo. Escala 1,15×; materiais de flash existentes, carga/impacto/rastros, cinco SFX próprios PCM no mixer Effects. Fundo agora tem deslocamento suave, oito cipós em três planos e partículas de folhagem com tempo escalado; arte nativa nova `vine_cluster.png`, reutilizada nos golpes texturizados. O usuário rejeitou a falta de movimento/encaixes/emoção da primeira versão; não apresentar a antiga pose parada como resultado final nem afirmar aprovação artística. `JungleStagePlayValidation.Showcase` captura vídeo/áudio reais em 1200×720 e confere pausa/rig/vento + pixels do boss nas três resoluções. Fase completa passou 60 checks; Gusini passou 49 checks. Vídeo e relatório em `Docs/Fase2Selva.md`. O indexador Unity Search 6000.6 dá exceção esporádica de startup; filtro restrito ao stack SearchDatabase foi alinhado no validador do Gusini com os validadores de cartas/selva, sem ignorar erros do player.

**Fase 2 da campanha — Selva/Macaquini (22/10/2026):** usuário confirmou nova fase com Macaquini e bichos da selva. Integração em `Assets/Scenes/StageTwoJungle.unity`, nove hordas de morcegos/vespas e boss na décima, acesso `FASE 2 · SELVA` no menu com nome/loading existentes; capítulo do Gusini preservado. Prefab `Macaquini` usa os seis módulos v09 e arma orgânica v04 sem redesenho; uma forma de combate nesta primeira integração, com salvas, leque e cipós em duas de três faixas. `MacaquiniCombat` implementa o contrato existente; eventos da arma disparam em 320 ms. `BossHUD` ganha barra genérica sem mudar a específica do Gusini. `WaveController.completeChapterOnBossDefeat` é opt-in e falso nas cenas anteriores; `Chapter2.asset` registra só `chapter2_completed` e retorna à base. Bichos reutilizam Enemy/ArtilleryCombat/InterceptorCombat. Fundo estático Default/-110, pois BackgroundSky fica acima de Default neste projeto e cobriria o combate. Fontes/prompts/manifestos em `SourceArt/JungleStage_v01`, exportador `Tools/Art/prepare_jungle_stage.cjs`. Teste isolado `MacaquiniBossTest`, fora do build, F1/F2/F3 forçam ataques e H cura; não aplica kit do Gusini. Ferramentas nativas `JungleStageTools` e `JungleStagePlayValidation`. Transformação/segunda forma, resgate narrativo, áudio próprio, parallax e terminal de missões continuam para etapas posteriores. Estado e validações finais em `Docs/Fase2Selva.md`.

**Áudio CC0 e FX graduais v22 — 22/10/2026:** nove WAVs de cartas preparados com samples dos pacotes oficiais Kenney Interface/Sci-fi/Music Jingles (CC0), sem osciladores. Sources/licenças/hashes em SourceArt/CardAudio_v22_CC0; gerador Tools/Audio/prepare_card_audio_v22.py (soundfile0.13.1 em Builds/AudioDependencies e NumPy). CardAudioPackageTools.Install/Validate configura/importa por APIs públicas e valida loops/referências/PCM; Validate chamado no build. CardRewardView tem dois canais de tesouro com crossfade, um loop da maior raridade revelada (6.4s) compartilhado por oferta, volumes .32/.40, herdando o mixer existente; pausa/fade/escolha/reroll/disable/menu limpos. Sons comum/escolha/reroll agora CC0, quatroRevealGUIDs preservados. FX: comum sem aura, rara4faíscas+traço, épicaenergia, lendáriaestrelas, míticaarcoscurtos emmalha; ThunderRed vertical não é mais esticado. Alfa protegido na vertical para comparações; artes25v21/metas intactas. Backup prévio Backup/Cartas/2026-10-22_AntesAudioFX_v22. PlayMode118checks/erros0 incluindo avanço real do áudio e saída de cena com tesouro; checkout1890GUIDs e buildUnity6000.6.4f1 passaram. Capturas em Docs/ArtPreviews/CardAudioFX_v22, startup8s sem exceções. RAR5 verdadeiro testadoCRC/conteúdo, Builds/Pacotes/CrocodiloInvaders-DEV-AudioFX-v22-Windows.rar (87716663bytes), licenças incluídas. Tools/PackageDev.ps1 suportaRAR com Rar.exe oficial extraído em Builds/PackagingTools, sem instalação global; essa ferramenta não é redistribuída no jogo. Sem commit nesta etapa. Ver Docs/AudioEfeitosCartas_v22.md.

**Coleção de cartas v21 — 22/10/2026:** as 25 frentes foram substituídas por sistemas instalados na nave sem personagem, com raridades progressivas, lendárias full art e míticas com relevo ilustrado. Ataque mítico mantém a borda esquerda inteira, só a bala cruza a direita. Fontes finais em SourceArt/CardCollection_v21_Integration/Sheets; exportador ativo Tools/Art/apply_ship_cards_v21.cjs (--apply/--check), PNGs360×536RGBA com fundo externo transparente e metadados originais preservados. Backup na raiz Git, fora da Unity: Backup/Cartas/2026-10-22_Anteriores_v21, 1.055 arquivos/331.808.858 bytes verificados por SHA256. 01_EmUsoNoJogo preserva a coleção em uso; 02_ModelosEHistorico recebeu fontes/prévias/geradores/guias antigos, removidos do projeto ativo. Não executar geradores históricos para a coleção atual. Verso/catálogo/valores/áudio/FX mantidos; foil/relevo dinâmico não implementado. Exportação/check e checkout1884GUIDs passaram; Play Mode94checks/erros0 passou, todas25capturadas/conferidas1200×720 e míticas800×600/1920×1080. BuildWindowsUnity6000.6.4f1 passou e executável iniciou8s headless sem exceções. ZIPnovo Builds/Pacotes/CrocodiloInvaders-DEV-Cartas-v21-Windows.zip (116987034bytes); anteriores preservados. Ver Docs/CartasIntegracao_v21.md.

**Transparência externa v15 — 21/10/2026:** usuário apontou fundo preto atrás das molduras. PNGs26 eram alpha255total. Tools/Art/build_card_alpha_v15.py aplica apenasalpha0 ao matteescuroexternoconectadoàs bordas, cominterior protegido e1pixeldecontorno preservado. PixelsRGBtodos e conteúdoopacointacto verificados; fontesv14/versov13 preservados. --applyatômico porarquivo(via stagingeos.replace), --check26hash+metas; atualverificadorativo, nãoaplicarv14/v13poisrestauramopacidade. SourceArt/CardCollection_v15_Transparency/manifest.json epranchasquadriculadas/PNGs emDocs/ArtPreviews/CardCollection_v15. Primeiraaplicação tevefalhatransienteaoabrirSpecialDamage/Mythic.png; interrompidoapenasbatchdetestepróprio eaplicaçãorepetidaatômicaverificadaantesdenovoteste. Aplicadonas25frentes+verso180x268, semnovosrefs/material/códigoUI. PlayMode94checks errors0/ciclo25; capturasfrentes/versoconferidascenáriovisívelnoscantos/recortes, textos/sons/FXinalterados. Checkout1884GUIDspassou. PacoteatualBuilds/Pacotes/CrocodiloInvaders-DEV-Cartas-v15-Windows.zip; anteriorespreservados. Docs/TransparenciaCartas_v15.md. Mesmabranchsemcommit.

**Molduras corrigidas v14 — 21/10/2026:** usuário identificou ilustração antiga nas bordas internas (recorte da Rajada rara) e pediu validar todas. Causa: composição v06-v12 preenchia só132×160 em(24,48), menor que janela real, deixando tiras antigas emlaterais/topo/base. NovoTools/Art/build_card_frames_v14.py recompõe TODA abertura/aro chanfrado nas25cartas a partir de um só recortev12; contorno pixelart porraridade, projeções existentes sómíticas recebemmesmotransform para manterconexão. UsaPillow sobre fontes/códigoeditáveis, semnovaIA/alteraçãoartcompleta. Labels v13bytes/metadadospreservados, verso/sons/FXinalterados. Assertpixelaperture==tile novo e aro apenaspaleta semfundovelho, mudanças restritasmaskrim/projection. Fonte/manifesto/máscaras emSourceArt/CardCollection_v14_FrameRepair; prévia5pranchas eantes/depois/zoom emDocs/ArtPreviews/CardCollection_v14. --applyaplica, --checkconfere25hash/labels/metas; este éoverificadorativo (v12/v13restauramdefeitoantigo). Pranchas25conferidas, depoisTODAS9capturas reais vistas. CardEffectsPlayValidation agora percorreAllCards9ofertas, verificaSprite/catálogo evisited25, mantendooutraschecagens:94checks errors0. CapturasLogs/CardEffects/all_cards_0..8_1200x720.png mais800x600/1920x1080. Checkout1884GUIDspassou. BuildUnity6000.6.4f1sucesso163126719bytes; pacoteBuilds/Pacotes/CrocodiloInvaders-DEV-Cartas-v14-Windows.zip, anteriorespreservados. Docs/RevisaoMolduras_v14.md eDocsativosatualizados. Mesmabranchcodex/cartas-expressoes-v12semcommit.

**Ajustes finos v13 — 21/10/2026:** pedido para textos cortados, sons distintos por raridade e verso claramente diferente da frente. Textos25 recompostos em Pillow/FreeType sobre as basesv12; Pango/Sharp cortava primeira coluna de glifos. Canvas completo/padding preserva letras; atributo e bônus sobem4px. Gerador verifica pixels fora das placas idênticos. Arte/expressõesv12/molduras/valores intactos. Novo verso viaimage_gen nativo, simétrico com selo do Croc e escamas/metal, sem retrato/atributos/texto; finalAssets/Art/Cards/Back.png180×268, identidade do sprite preservada. Fonte e prompt emSourceArt/CardCollection_v13_FineTuning, antigosBack/sons preservados. Gerador Tools/Art/build_card_fine_tuning_v13.py (--check sem mutação) e manifesto/hashes; esse é o verificador atual: apply_card_collection_v12.cjs restaura os textos antigos, não executar para coleção atual. Quatro WAVs originais44.1kHzPCM: rara0.42s dois sinais, épica0.74s subida, lendária1.04s fanfarra, mítica1.42s grave/subida/acorde. RMSGain cresce; mix3míticas200ms pico0.763 sem clipping. CardRarityAudio temcampo rare e ganhos.60/.72/.84/.96, pitch1; somgenéricoView sócomum. CardFineTuningTools.Install viaPrefabUtility/SerializedObject: importPCMprememória semforceToMono normalizador e refsrares3slots. YAMLreordenado peloUnity, comparação porblocos/IDs confirmou apenas3refsrare lógicas novas. Validate verifica25faces/3versos/4sons/RMS. PlayMode57checks errors0 incluindo clipsnos3slots everso, pausa/reroll/choose/menu; capturasemLogs/CardEffects conferidas1200×720/800×600/1920×1080. Checkout1884GUIDs passou. Build163282862bytes emUnity6000.6.4f1. PacotenovoBuilds/Pacotes/CrocodiloInvaders-DEV-AjustesCartas-v13-Windows.zip; anteriorespreservados. Docs/AjustesCartas_v13.md registra tudo. Semescuta humana deáudio afirmada; validação técnica dePCM/referências. RerollinfinitoDEV/FXexternos mantidos, foilinterno segueposterior. Mesmabranchsemcommit.

**Efeitos externos progressivos — 21/10/2026:** implementados a pedido do usuário. CardRarityParticles mantém uma malha por carta dimensionada pela face: comum estalo curto, rara faíscas/ponto azul, épica coroa violeta, lendária raios/estrelas douradas, mítica coroa carmesim/explosão/relâmpagos. CardRarityFeedback usa relógio único pausável; CardOptionView ganha impulso/tremor na revelação e escolha, aura reforçada no foco. CardRewardView revela a mais rara por último sem mudar slots/atalhos; centros350/vão100 em1200×720 e escala responsiva nas janelas estreitas. Efeitos limitados na vertical para proteger textos. Áudios existentes com altura/volume por raridade. Artes25v12 intactas e foil/holografia interna ainda adiada. Validador novo CardEffectsPlayValidation.Run passou44checks reais (cinco raridades, pausa, reroll, escolha, nova oferta, menu) com capturas conferidas em1200×720/800×600/1920×1080. Indexador Search do Editor6000.6 lança ocasional exceção de startup em lote, separada de erros do player no validador. Corrigidos problemas encontrados na saída: Player respeita componentes destruídos; CardRewardController.OnDisable sempre restaura congelamento mesmo quando ondas/UI já destruídas. Teste.NET1464checks e checkout1882GUIDs passaram. BuildUnity6000.6.4f1 sucesso163202029bytes. Pacote novo `Builds/Pacotes/CrocodiloInvaders-DEV-Efeitos-Windows.zip`; anteriores preservados. Docs/EfeitosCartas.md descreve implementação e teste. Play Mode assíncrono exige batch sem-quit: unity run adiciona-quit e encerra antes da suíte; iniciar Editor oculto sem-quit, validador encerra ao concluir. Mesma branchcodex/cartas-expressoes-v12, semcommit.

**Reroll infinito no DEV — 21/10/2026:** usuário pediu rerolar sem limite no modo desenvolvedor. CardRewardController.HasUnlimitedRerolls vinculado somente a DevelopmentCardSession.IsActive: não consome cota noDEV, mantém bloqueios duranteReveal/Choose/pausa e limite1 na partida normal. CardRewardView mostra `REROLL INFINITO · DEV [R]`, botão/navegação disponíveis quandoReady mesmo comremaining0. HintDEV atualizado. Teste .NET inclui controladorreal com colaboradores/coroutines semexecução:200rerolls seguidos, concorrência, pausa, limite normal e isolamento;1464checks passaram. BuildUnity6000.6.4f1 validado. Novo pacote `Builds/Pacotes/CrocodiloInvaders-DEV-RerollInfinito-Windows.zip`; anterior preservado. PackageDev.ps1 aceitaPackageName eLEIA-ME atualizado. Semnovofoil/FX.

**Build Windows DEV entregue — 21/10/2026:** usuário pediu arquivo para extrair/rodar sem Unity. Build exato6000.6.4f1 compilado e validado com CrocodiloWindowsBuild.Build, saída `Builds/Windows-DEV/CrocodiloInvaders.exe`,163197933bytes do player. `Builds/Pacotes/CrocodiloInvaders-DEV-Windows.zip`115151485bytes, SHA256SUMS eLEIA-ME; empacotador `Builds/PackageDev.ps1` exclui debugDoNotShip e verifica EXE/UnityPlayer/Assembly-CSharp com os tiposDEV. Inclui25cartasv12 e DEV/F8, sem novosFX/foil. Inicialização real do EXE em headless10s semexceções; interaçãoUI ainda não conferida visualmente. CLIatualizada1.0.0-beta.14; Editor6000.6.4f1 preparado em `Builds/UnityToolchain/6000.6.4f1` via instalador oficial silenciosoRunAsInvoker em pasta local (instalador/UAC convencional bloqueado). Pasta padrãoUnityHub restauradaC:/ProgramFiles/Unity/Hub/Editor. UsoCLI`unity run --editor-path <local>/Editor/Unity.exe` evitaEditorantigo6000.3. Scripts agora compilados, validadorescenas/prefabs/cartas/chances/mixer passaram. LogsWindowsDEV-Build/Startup emLogs. PacoteZIPpoisRARnãoinstalado. BuildseToolchainignoradospeloGit. Mesmabranchcodex/cartas-expressoes-v12 semcommit.

**Estudo de efeitos externos das cartas — 21/10/2026:** usuário pediu estudar e apresentar ideia, sem implementar agora. Proposta em `Docs/EstudoEfeitosCartas.md`: comum impacto seco/sem aura; rara aro azul com ponto de luz; épica energia violeta segmentada; lendária halo/brasas douradas; mítica coroa carmesim e estalo/arcos curtos só na revelação, repouso contido. Estudo inspecionou CardRarityFeedback/Particles/Audio, CardOptionView e CardRewards.prefab: já existem aura/partículas/sons desdeépica, raios míticos e referências3slots; evoluir sistema existente em vez de duplicar. Partículas usam coordenadasfixas e malhaUI; dimensionar contorno relativo ao Face. Arte360×536 versusFace250×402 com PreserveAspect0 requer conferirproporção antes de efeitos. Testar3míticas simultâneas, pausas/reroll/foco/saída noDEV e medirCanvas/overdraw. Fonte técnicaUnity citada no estudo. Nenhum código/prefab/material alterado neste estudo; foil/holográfico interno segueposterior.

**Modo desenvolvedor de cartas — 21/10/2026:** usuário pediu acesso discreto no menu e teste no mesmo fluxo do jogo. `MainMenuController` instancia `DeveloperMenuView` runtime (DEV no topo direito/F8), configura `DevelopmentCardSession` em memória e carrega a cena normal pelo loading existente. Na confirmação, `GameplayLoadingController` inicia `DevelopmentCardTest` em vez de StartRun. WaveController fornece StartCardDevelopment/RequestDevelopmentCardReward: fase Intermission mantida, WaveCleared real, pausa/input/reveal/reroll/escolha/bônus originais; após escolher fica livre para testar o Croc na cena sem novas hordas/boss automáticos. PainelF8 com sorteio normal/raridade fixa/ciclo25, onda1/5/9, nova oferta, resetbônus/ciclo e menu. Não prepara ranking; Menu.Awake encerra sessão. Com sessão inativa a geração cai exatamente em catalog.RollOffers. Cenas/prefabs/shaders intactos. Novos arquivos DevelopmentCardSession/Test, DeveloperMenuView/DevelopmentUI; metadados presentes. Teste .NET10 em Tools/Validation/CardDevelopment: 860checks de catálogo/pesos/sessão passaram; checkout1881GUIDs passou. Compilação/visual de UI e Play Mode ainda dependem de Unity6000.6.4f1 (somente6000.3.25f1 instalada). Docs/ModoDesenvolvedor.md descreve acesso/arquitetura/validação. Mesma branch codex/cartas-expressoes-v12, sem commit.

**Coleção v12 integrada — 21/10/2026:** usuário confirmou expressões nos demais tipos e pediu branch/aplicar no jogo, deixando foils Unity para depois. Branch `codex/cartas-expressoes-v12` criada a partir de main com trabalho local preservado. 25 frentes v12 copiadas para `Assets/Resources/Cards/{família}/{raridade}.png`, 360×536; sprite Card cobre tudo, PPU200 para dimensão nominal equivalente à antiga180×268/PPU100; Point, sem mipmaps/compressão, limite1024. GUID/spriteID/fileID, catálogo/valores/caminhos Resources e verso preservados. Aplicador/verificador `Tools/Art/apply_card_collection_v12.cjs` (`--check` verifica sem mutação), backup dos arquivos anteriores e manifesto em `SourceArt/CardCollection_v12_Integration`. Verificação25imagens/hashes/metadados/catálogo passou. Materiais/shaders e código de gameplay/UI intactos; novos foils/holográficos dinâmicos explicitamente adiados. Sem Play Mode: projeto6000.6.4f1, somente Editor6000.3.25f1 detectado; Pipeline sem instância acessível. Docs/Cartas.md atualizado. Alterações locais sem commit nesta etapa.

**Expressões progressivas v12 — 21/10/2026:** usuário apontou mesma cara do comum ao lendário e pediu raiva/loucura gradual até mítico. Quinze faces intermediárias editadas nas cinco famílias: raro olhar irritado/sobrancelhas fechadas e mandíbula cerrada (preserva boca corrigida v11); épico rosnado aberto/raiva; lendário berro/olhar quase insano. Dez extremos comum confiante/mítico gargalhada aberta preservados por hash. Fontes/faixas editadas/prompts/máscaras/manifesto em `SourceArt/CardCollection_v12_Expressions`; prévia atual `Docs/ArtPreviews/CardCollection_v12/cartas-expressoes-progressivas.html`. Exportador só aplica polígonos localizados e verifica pixels fora intactos. Ferramentas `build_card_collection_v12.cjs` e `verify_card_collection_v12.cjs`. 25 camadas de texto, 25 máscaras e shader anteriores preservados; WebGL/interações/regiões protegidas/responsividade passaram. Sem mudança em Assets/runtime Unity; usar v12 na próxima integração.

**Bocas corrigidas v11 — 21/10/2026:** usuário identificou boca duplicada nas Rajadas rara/épica e pediu revisar as demais. Auditoria visual das 25 encontrou também Propulsão rara. Três edições localizadas via image_gen em `SourceArt/CardCollection_v11_MouthFix`: retirada de dentição humana/sorriso sobreposto e forma bege dentada sob a mandíbula; mandíbula única contínua, dentes pontudos, focinho/olhos/capacete preservados. Compositor aplica somente polígonos de boca/pescoço e verifica pixels fora intactos. Prévia atual `Docs/ArtPreviews/CardCollection_v11/cartas-bocas-corrigidas.html`; ferramentas `build_card_collection_v11.cjs`/`verify_card_collection_v11.cjs`. 22 cartas inteiras, 25 camadas de texto, 25 máscaras e shader v10 preservados; WebGL/interações/regiões protegidas/responsividade passaram. Míticas carmesim intactas, históricos preservados. Sem mudança de Assets/runtime. Usar a coleção v11 para futura integração.

**Míticas carmesim v10 — 21/10/2026:** usuário pediu identidade vermelha e menos poluição, ainda acima das lendárias. Cinco artes revisadas via image_gen em `SourceArt/CardCollection_v10_MythicRed` (atlas/fontes/prompt/manifesto); equipamento carmesim/gunmetal/marfim, Croc verde com capacete oliva/boca aberta, menos peças pequenas e explosões. Composição mostra mais equipamento principal; sobreposição de ombros limitada. Arte prismática vermelha e moldura escovada vermelha com efeitos distintos, menor intensidade no Croc e placas de texto protegidas. Prévia atual `Docs/ArtPreviews/CardCollection_v10/cartas-miticas-vermelhas.html`, PNGs/camadas/pranchas/qa no mesmo diretório. Ferramentas `build_card_collection_v10.cjs` e `verify_card_collection_v10.cjs`. 20 cartas não míticas e 25 camadas de texto idênticas à v09 por hash; WebGL/interações/foil/regiões protegidas/responsividade passaram. v09 e snapshot v08 preservados. Sem alteração em Assets/runtime.

**Coleção completa de cartas v09 — 21/10/2026:** usuário pediu salvar os textos v08 e fazer as demais famílias na mesma pegada. Modelo final imutável em `SourceArt/CardsModel_Approved_v08_20261021` com HTML/camadas/fontes/fontes de arte/código/hashes. Novos 4atlas/20artes em `SourceArt/CardCollection_v09`: Rajada/Cadência (gatlings), Blindagem/Vida (armadura), Propulsão/Movimento (avião/turbinas), Especial (emissor/esfera verde). Cinco raridades cada, míticas mais exageradas com boca aberta. Molduras, regras de foil/holografia e tipografia v08 aprovadas; só míticas sobrepõem equipamentos. Exportações 25PNG/camadas/pranchas/QA e prévia com seletor de família em `Docs/ArtPreviews/CardCollection_v09/cartas-colecao.html`. Scripts `build_card_collection_v09.cjs`, `card_collection_v09.html`, `verify_card_collection_v09.cjs`; v06 exporta helpers de recorte sem executar ao importar. Valores conferidos no catálogo; Ataque inteiro copiado do aprovado por hash; shader/seleção/regiões protegidas/responsividade passaram. Apenas design/exportação, sem mudar Assets/runtime (v03 continua no jogo).

**Encaixe dos textos v08 — 21/10/2026:** usuário apontou tamanhos inconsistentes, desalinhamento e contato com molduras. Atual em `Tools/Art/card_typography_v08.cjs`: posições verticais/centros fixos, fonte e tamanho iguais por função nas cinco raridades, bônus inteiro (+30%) em uma só linha/tamanho; atributo afastado do topo e bônus reduzido/subido, sombra1px. Raridade/atributo16px, título20px, bônus32px na camada360×536. 25combinações conferidas com acentos e caixas seguras; bases e materiais preservados por hash. Preview v06 usa v08, v07 histórica. Snapshot do modelo aprovado intacto; sem mudar Assets/runtime.

**Modelo de cartas aprovado e textos v07 — 21/10/2026:** usuário aprovou e pediu salvar o modelo, depois focar nos textos. Snapshot imutável em `SourceArt/CardsModel_Approved_v06_20261021` com HTML autossuficiente, camadas, prancha e hashes. Prévia editável v06 agora usa `Tools/Art/card_typography_v07.cjs`: Press Start 2P para títulos/números, Pixelify Sans para raridade/atributo; maior legibilidade, bônus numérico destacado, +/% menores, alinhamento/margens e acentos reais. 25 combinações de tipografia cabem nas caixas seguras. Cinco bases/materiais idênticos ao modelo aprovado por hash, snapshot intacto, shader e checks de leitura/regiões protegidas passaram. Preview `cartas-acabamentos.html` atualizado; sem mudança de Assets/runtime.

**Acabamentos finais da prévia v06 — 21/10/2026:** novo ajuste solicitado: épica reflexo branco neutro na arte e foil colorido na moldura; lendária reflexo neutro na moldura, arte anterior idêntica (inclui holografia só do fundo/Croc protegido); mítica dois holográficos diferentes, prismático curvo na arte e escovado diagonal na moldura, Croc destacado. Imagens, textos e recorte do vazamento preservados por hash; comum/rara inalteradas. Máscara G mítica codifica arte=128/moldura=255. Preview `cartas-acabamentos.html` atualizado, checks WebGL e regiões protegidas passaram; arte lendária conferida pixel a pixel. Sem mudança de Assets.

**Recorte mítico da prévia v06 corrigido — 21/10/2026:** usuário pediu corrigir somente o vazamento da arte sobre a moldura. `build_foil_preview_v06.cjs` substitui cortes horizontais rígidos por contornos dos quatro grupos de canhões, seleção de metal/energia, recuperação do traço escuro, remoção de partículas isoladas e sombra de contato de 1px. Elimina blocos de céu e canhões truncados sobre os trilhos. Bases/materiais/textos das outras quatro cartas preservados por hash; imagem fonte, textos, foil e Assets sem mudanças. Preview `cartas-acabamentos.html` atualizado e conferido visualmente.

**Molduras padrão comum/mítica — 21/10/2026:** correção da prévia v06 pedida pelo usuário. Comum voltou à moldura industrial completa oliva/aço da família v03, fosca; mítica usa carcaça carmesim completa com presas, títulos/atributos dentro das placas, foil/holografia e canhões sobrepostos apenas a partes dos trilhos. Não usar a moldura retangular lisa comum nem o aro fino mítico da primeira v06. Rara/épica/lendária preservadas por hash; preview `cartas-acabamentos.html` atualizado, sem alterações de Assets.

**Acabamentos por camada v06 — 21/10/2026:** direção solicitada mais recente: comum fosca/moldura lisa; rara reflexo neutro uniforme só no aro da imagem (arte fosca); épica moldura holográfica e arte foil; lendária holografia SÓ no fundo, Croc/arma/armadura protegidos e desenho CONTIDO na moldura; mítica toda holográfica com personagem destacado e arsenal SOBRE a moldura, única que pode rompê-la. Usa mítica multicannon/boca aberta da v05. Prévia sem alterar Assets: fontes/regras em `SourceArt/CardsFoil_v06_Layers`, exportações `Docs/ArtPreviews/CardsFoil_v06`, ferramentas `build_foil_preview_v06.cjs`, `foil_preview_v06.html`, `verify_foil_preview_v06.cjs`. Máscaras R/G/B/A separam reflexo/holografia/personagem/visibilidade. Shader/JS, regiões protegidas e responsividade passaram. Substitui a proposta de lendária full art da v04/v05. V03 segue integrada no jogo.

**Expressão da mítica — 21/10/2026:** `SourceArt/CardsFoil_v05_AltArt/art_4.png` agora é a versão com múltiplos canhões e boca bem aberta/olhos arregalados, em gargalhada/berro insano, por pedido do usuário. Anterior preservada em `art_4_multicannon_closed_mouth.png`; imagem de conferência `Docs/ArtPreviews/CardsFoil_v05/mythic_multicannon_open_mouth.png`. Revisão apenas da imagem, sem alterar outras raridades/Assets ou recompilar prévia interativa.

**Mítica com múltiplos canhões — 21/10/2026:** usuário pediu refazer apenas a imagem da mítica, mais exagerada e cheia de canhões. Fonte atual `SourceArt/CardsFoil_v05_AltArt/art_4.png`: gatling no ombro, três canhões laterais, canhão principal e duas armas inferiores, mantendo rosto/capacete/estilo. Anterior em `art_4_single_cannon_v01.png`; conferência avulsa `Docs/ArtPreviews/CardsFoil_v05/mythic_multicannon.png`. Demais fontes intactas por hash; sem mudança de textos/foil/Assets. Não reexportada a prévia interativa nesta revisão de imagem isolada; próxima execução do exportador v05 usa a nova mítica.

**Prévia de variantes por raridade v05 — 21/10/2026:** usuário pediu artes cada vez mais tunadas desde a rara, mantendo a composição e identidade da mesma carta. Quatro edições do master v04: rara reforço/ciano; épica célula e conduítes violeta; lendária ouro/reator verde; mítica placas pesadas, presas metálicas e dois reatores, a mais equipada. Mesmo Croc/rosto/pose/cenário. Mantém foil/textos/full art v04 e opção desligar foil para comparar arte. Prévia de dano de ataque, sem alterar Assets ou mecânicas. Fontes `SourceArt/CardsFoil_v05_AltArt`, prévias `Docs/ArtPreviews/CardsFoil_v05`, ferramentas `build_foil_preview_v05.cjs`, `foil_preview_v05.html`, `verify_foil_preview_v05.cjs`. V03 continua integrada no jogo.

**Prévia foil/full art — 21/10/2026:** usuário aprovou a direção v03 e pediu textos melhor encaixados e raridades progressivamente foil, com foco no personagem e full art. Protótipo somente, sem mudar Assets: comum fosca; rara foil interno; épica imagem/bordas; lendária arte expandida/foil/relevo; mítica full art texturizada holográfica. Fontes em `SourceArt/CardsFoil_v04_Preview`; prévias em `Docs/ArtPreviews/CardsFoil_v04`; ferramentas `build_foil_preview_v04.cjs`, `foil_preview_v04.html`, `verify_foil_preview_v04.cjs`. Reflexos dinâmicos, rosto com intensidade reduzida, textos protegidos em camada posterior. Verificação WebGL/JS e leitura visual no browser; ainda não é shader/integração de Unity. V03 continua em Assets.

**Cartas Croc v03 — 20/10/2026:** substitui a v02 também rejeitada pelo usuário. Atual: close-ups de combate do Croc e molduras industriais sombreadas com volume, metal, rebites e luzes; comum aço/oliva, raro azul/prata/ciano, épico violeta com conduítes, lendário bronze/ouro com asas e estrela, mítico carmesim com presas marfim. Novas cenas geradas referenciando o retrato do Croc; textos bitmap separados, títulos diretos e sem frases pequenas. 25frentes e verso180×268, GUIDs/fileIDs/catálogo/regras preservados. Exportador atual `Tools/Art/build_croc_cards_v03.cjs`; fonte/manifest `SourceArt/CardsCroc_v03`; prévias `Docs/ArtPreviews/CardsCroc_v03`. V01/v02 históricas, não são a direção atual. Conferência visual, catálogo/importação e checkout passaram; sem Play Mode.

**Cartas Croc v02 — 20/10/2026:** usuário rejeitou os ícones gerados e a moldura uniforme da v01. Atual: 25 frentes compostas com sprites reais de Croc, projéteis, dash, especial e vida; nomes Dente de Aço, Chuva de Bala, Casco Duro, Pé na Tábua e Bafo Atômico. Cinco molduras com silhuetas distintas: comum chapa cinza/oliva, rara reforços azuis/ciano, épica circuitos/cristais roxos, lendária asas/estrelas douradas, mítica mandíbula vermelha com dentes. Verso com retrato original em identificação militar. PNGs180×268, Point, GUIDs/fileIDs/catálogo/regras preservados. Exportador atual `Tools/Art/build_croc_cards_v02.cjs`, manifest/fontes `SourceArt/CardsCroc_v02`, prévias `Docs/ArtPreviews/CardsCroc_v02`; v01 e26originais arquivados em `SourceArt/CardsMilitary_v01`. Conferência visual e catálogo; sem Play Mode nesta revisão.

**Arte fase1 em cipós e tiro — 20/10/2026:** `SourceArt/BananaPhase1_VinesShoot_v01/`, 24desenhos160×160/4,08s, corpo de banana fechado, macaco oculto e membros esguios. Salta entre cipós, para, mira/dispara arma orgânicav04, retorna fechando e volta ao outro cipó. Projétil64×32 separado, tiro índice15/2660ms. Atlas8×3, quatro faixas de6frames, JSON/fontes/prompts/QA. GIF em `Docs/ArtPreviews/BananaPhase1_VinesShoot_v01/`. Cipós e arma segurada fazem parte da composição do personagem; trajetória está nos frames, sem rig ou integração de gameplay. Ferramentas `split_banana_phase1_vines.ps1` e `package_banana_phase1_vines.cjs`.

**Arma atual — orgânica v04 (20/10/2026):** usuário pediu “menos tech e mais banana”. `SourceArt/BananaAlienWeapon_v04_Organic/`: casca natural dominante, talo marrom na pegada, veios verdes; retirados colar roxo, câmaras/placas e aleta tech. Arma128×96, projétil64×32, recoil8px, ciclo12posições/1s com retorno reverso. Pegada(86,60), boca(32,39), disparo índice3/320ms. Prévia em `Docs/ArtPreviews/BananaAlienWeapon_v04_Organic/`; ferramenta `Tools/Art/package_banana_alien_weapon_v04.cjs`. Sem encaixe/combate, anteriores preservadas.

**Projétil maior da arma — v03 (20/10/2026):** `SourceArt/BananaAlienWeapon_v03_BigProjectile/` amplia a banana disparada para64×32 (antes32×16), com saída ajustada na prévia. Os 12 frames da arma são idênticos à v02 porhash; ciclo1s/recoil8px preservados. GIF em `Docs/ArtPreviews/BananaAlienWeapon_v03_BigProjectile/`. Ferramenta `Tools/Art/package_banana_alien_weapon_v03.cjs`; versões anteriores preservadas, sem gameplay.

**Arma banana mais poderosa — v02 (20/10/2026):** usuário pediu mais poder. Arte atual em `SourceArt/BananaAlienWeapon_v02_Power/`: corpo/casca mais grossos, nervuras douradas, duas câmaras verdes, empunhadura roxa reforçada, abertura maior e recoil8px (v01=4px). PNGs nativos128×96, projétil32×16, paleta20/alpha binário, 12 posições/1s e pegada(86,60). Prévias em `Docs/ArtPreviews/BananaAlienWeapon_v02_Power/`; ferramenta `Tools/Art/package_banana_alien_weapon_v02.cjs`. V01 preservada; sem dano/combate ou encaixe no macaco implementados.

**Arma separada do macaco — 20/10/2026:** `SourceArt/BananaAlienWeapon_v01/` traz arma alienígena em formato de banana, voltada à esquerda. 12 posições/1 s de abertura dianteira → disparo da fruta separada → recoil até4px à direita → retorno/fechamento. PNGs nativos112×80, projétil24×12, paleta limitada/alpha binário, atlas6×2 e JSON de tempos/pontos. Pegada nominal(72,50), boca(32,34); evento no índice3/320ms. Prévias em `Docs/ArtPreviews/BananaAlienWeapon_v01/`; fontes/prompts e `Tools/Art/package_banana_alien_weapon.cjs` salvos. Feita para encaixar futuramente; sem mãos, rig, prefab ou gameplay implementados.

**Correção de arte mais recente — pixel art v03 (20/10/2026):** usuário rejeitou o acabamento ilustrado da transformação v02. Revisão em `SourceArt/BananaPossessora_Transformacao_v03_PixelArt/`: 24 desenhos redesenhados, 128×128 nativos, paleta fixa de 20 cores mais alpha binário, contornos/planos em pixels; mesmos movimentos e tempos (4,22 s), usando Goosini como referência. GIF nativo, prévia ampliada exatamente4× e player por quadro em `Docs/ArtPreviews/BananaPossessora_Transformacao_v03_PixelArt/`. Exportações finais em `frames/`; folhas-fonte não são assets nativos. V02/poses anteriores preservadas. Sem integração de gameplay. Ferramenta `Tools/Art/package_banana_pixel_v03.cjs`.

**Transformação mais recente — 20/10/2026:** `SourceArt/BananaPossessora_Transformacao_v02_24Frames/` contém 24 desenhos distintos da transformação para a fase 2 (4,22 s): força/raiva aumentam, mão agarra e arranca a ponta superior com o talo, arremessa esse pedaço e rasga a casca pelo meio. A ponta não reaparece; final com macaco furioso de olhos verdes, peito exposto, abas laterais e casca inferior amarela. PNGs RGBA 384×384 alinhados pelos pés, atlas 8×3, JSON de tempos/pivô, fontes/prompts/QA. GIF, WebP e player por frame em `Docs/ArtPreviews/BananaPossessora_Transformacao_v02/`. v01, coleção de poses/cipó e master modular v09 preservados. Sem Animator/prefab/gameplay; ferramentas `split_banana_transformacao_v02.ps1` e `package_banana_transformacao_v02.cjs` em `Tools/Art`.

**Atualização mais recente — banana v09 (19/10/2026):** o usuário aprovou a montagem da conversa e pediu modularidade compatível com a arte existente. Master atual em `SourceArt/BananaPossessora_Fase1_Master_v09_Modular`, seis PNGs nativos, ORA/SVG e layout/pivôs; exportações estáticas em `Assets/Art/Boss/BananaPossessora/Fase1`, Point/PPU 100, sem mipmaps/compressão. Prévia interativa e GIF em `Docs/ArtPreviews/BananaPossessora_v09`. A decisão sucede a v08 descrita abaixo; históricos preservados. Próxima forma definida diretamente pelo usuário: fortalecimento gradual, casca rasgada na metade/semiaberta e macaco parcialmente exposto, furioso, também com olhos verdes. Há seis quadros-chave em `SourceArt/BananaPossessora_Transformacao_v01`; são estudo de transformação, ainda sem intermediários ou módulos separados da forma semiaberta. Sem prefab/Animator/gameplay e sem validação no Editor Unity. Ferramentas `Tools/Art/package_banana_v09.ps1`, `preview_banana_v09.ps1` e `export_banana_preview.cjs`.

**Direção de novo boss (19/10/2026):** a banana infectada por alienígenas possui e aprisiona o macaco que a comeu; Croc deve derrotar a banana para libertá-lo. Estilo baseado somente em Goosini e Croc, desconsiderando outros estudos banana/macaco. Primeira fase definida pelo usuário em `SourceArt/BananaPossessora_Fase1_Master_v08_BaseRetomada`: macaco menos forte, banana esguia como a nova referência, casca AMARELA com olhos e detalhes VERDES, sem rosto espiando e rabo encaixado atrás do quadril. O usuário retomou a referência da v03_Amarela e pediu apenas braços/pernas mais compridos e perna de dentro atrás da ponta (v08). Desconsiderar as alterações de design das v04–v07: preservar banana, encaixes, rosto e rabo da imagem escolhida. Não usar casca verde. A v03 musculosa foi preservada, sem fase atribuída. Masters estáticos modulares, sem integração de gameplay. História, escopo e prévias em `Docs/BananaPossessora.md` (caminhos relativos ao projeto ativo).

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
