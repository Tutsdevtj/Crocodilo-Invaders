# Fase 2 — Selva e Macaquini

**Experimento v08:** [bancada com56quadros completos, balanço, disparo e rasgo](Fase2MacaquiniQuadros_v08.md). Cena/executável separados para avaliação; o boss da campanha continua na v07.

**Ajuste v07:** [mãos maiores e movimento com IK oficial do Unity](Fase2Macaquini_v07.md), com alcance em curva, inclinação e velocidade suavizadas.

**Ajuste v06:** [encaixes arredondados de ombro/cotovelo, mãos20%menores e boss12%menor](Fase2Macaquini_v06.md), mantendo a direção visual da v05.

**Atualização v05:** Macaquini recebeu nova arte detalhada de macaco, braços articulados, arma-banana original e duas formas: preso na banana e parcialmente libertado após rasgar a casca. Veja [Macaquini v05](Fase2Macaquini_v05.md). O fundo aprovado da [selva v02](Fase2SelvaMovimento_v02.md) permanece. As descrições abaixo registram a integração inicial e o primeiro retrabalho.

Implementação inicial de 22/10/2026, autorizada pelo usuário: nova fase da campanha com Macaquini e bichos da selva. Reutiliza combate, cartas, pausa, loading, ranking e retorno à base existentes. O capítulo do Gusini continua separado.

Retrabalho na mesma data, após o usuário rejeitar a montagem estática: rig articulado, encaixe da arma e do cipó nas mãos, balanço de corpo/pernas/cauda, trocas de posição mais fortes, carga/recuo/impacto e cenário com vento. A prévia real com som está em `Docs/ArtPreviews/JungleStage/macaquini-selva-em-movimento.mp4`; aprovação da sensação de ameaça continua sendo avaliação do usuário.

## Como jogar

No menu principal, use **FASE 2 · SELVA**, informe o nome e confirme o loading com Enter. A seleção abre `Assets/Scenes/StageTwoJungle.unity`, incluída no build depois das cenas existentes. **Jogar** continua levando ao capítulo do Gusini. Nesta versão, a fase 2 tem acesso direto, sem exigir desbloqueio nem terminal de missões na base.

No Editor: **Crocodilo → Fase 2 → Open jungle stage** abre a fase completa. **Open Macaquini boss test** abre `Assets/Scenes/Tests/MacaquiniBossTest.unity` e começa direto no anúncio do boss. A cena de teste não entra no build. F1 força salvas, F2 força leque, F3 força cipós e H cura o player. O kit especial de 240 HP do teste do Gusini não é aplicado ao Macaquini; seus controles e atributos básicos continuam os do player normal.

## Hordas e cartas

São nove hordas fixas, com uma escolha de carta após cada uma. A geração de ofertas, bônus aditivos, raridades, reroll e pausa são os sistemas existentes. As composições da selva são separadas dos pools do capítulo 1.

| Horda | Morcegos | Vespas | Total | Máximo vivo |
| --- | ---: | ---: | ---: | ---: |
| 1 | 4 | 0 | 4 | 3 |
| 2 | 6 | 0 | 6 | 3 |
| 3 | 8 | 2 | 10 | 4 |
| 4 | 10 | 4 | 14 | 4 |
| 5 | 12 | 6 | 18 | 4 |
| 6 | 14 | 8 | 22 | 6 |
| 7 | 16 | 10 | 26 | 6 |
| 8 | 18 | 12 | 30 | 6 |
| 9 | 20 | 14 | 34 | 6 |
| 10 | — | — | Macaquini | 1 boss |

Morcegos têm 5 HP e anunciam um leque de três bananas por 1 s, a 3,5 unidades/s. Reusam `ArtilleryCombat`. Vespas têm 4 HP e anunciam a investida por 850 ms, travando o destino antes de avançar a 8 unidades/s. Reusam `InterceptorCombat`. Ambos usam `Enemy`, `EnemyMovement`, `Health`, `EnemySpawner` e `RunScore`; a apresentação própria mostra a mira e o feedback de dano. Os sprites são estáticos, com deslocamento de voo em Transform, sem fingir que há spritesheets de asas.

## Macaquini

Prefab: `Assets/Prefabs/Boss/Macaquini.prefab`, com 360 HP e uma forma de combate nesta primeira integração: banana de casca fechada, com membros e rabo do macaco visíveis. A montagem usa os seis módulos da master v09, ampliada 15% na Unity; a arma usa os desenhos orgânicos v04. As fontes não foram redesenhadas. Ombros, quadris e raiz da cauda usam os pivôs nativos: braço de apoio elevado segura o cipó, braço da arma prepara a mira e recua, pernas e cauda acompanham o balanço. A arma é filha do braço e gira em torno da pegada; a boca do disparo acompanha essa hierarquia. O cipó curva entre o galho e a mão, sem dano. Entrada, patrulha e troca de posição a 4,2 unidades/s ficam no `EnemyMovement` existente.

`MacaquiniCombat` implementa `IBossCombatController`; `Boss` mantém entrada, vulnerabilidade, vida e derrota. Primeiro ciclo apresenta salvas → leque → cipós; depois os três padrões são embaralhados sem reposição nem repetição imediata entre ciclos. Há 1,1 s de recuperação, editável no Inspector.

| Ataque | Aviso e execução | Resposta possível |
| --- | --- | --- |
| Salvas | Mira travada por 750 ms, mais 320 ms até o evento da arma; três disparos separados | Sair da posição anunciada; as bananas não perseguem |
| Leque | Mesmo aviso; cinco bananas em arco de 64°, velocidade 6 unidades/s | Recuar para ampliar os espaços ou sair do arco |
| Cipós | Limpa projéteis anteriores; avisa duas das três faixas horizontais por 1,1 s; golpe ativo por 480 ms | Entrar na faixa inteira que permaneceu livre |

Bananas são entidades próprias, com dano 1, vida útil de 4 s e hurtbox menor que a arte. Cipó causa 2 de dano uma vez por alvo e por golpe; sua espessura de dano é menor que a visual. O cipó usa geometria de linha para combinar aviso, alcance e dano, sem arte incorporada no sprite do boss.

O clip de arma mantém os tempos do JSON da v04, com `MacaquiniRelease` em 320 ms e conclusão em 999 ms. O evento de disparo é consumido uma vez; callbacks duplicados não criam projéteis adicionais. Curvas do Animator alteram somente o sprite da arma, sem movimento global. Poses e balanço são aplicados nos Transforms das articulações, mantendo os encaixes. O pacote antigo de salto em cipós, com movimento embutido, não foi importado como deslocamento de gameplay.

Carga animada, aura verde breve, flash na boca, onda curta e rastro de projétil dão antecipação e impacto. Acertos usam o material `SpriteHitFlash` existente nos módulos. Shake curto reutiliza `GusiniPhaseTwoCameraFeedback`, que é um offset aditivo genérico apesar do nome. Cinco sons originais de carga, disparo, chicote, entrada e dano foram sintetizados por `Tools/Audio/prepare_macaquini_audio.cjs`, com PCM e roteamento no mixer Effects. A prévia captura o áudio do próprio jogo por um filtro temporário no listener; não grava áudio do sistema operacional.

## HUD e conclusão

`BossHUD` conserva a apresentação específica do Gusini e adiciona uma barra genérica para outros bosses, exibindo nome e vida real. A selva não carrega diálogos do Gusini nem usa seu retrato.

`WaveController.completeChapterOnBossDefeat`, falso por padrão nas cenas anteriores, permite concluir o capítulo 2 após a duração da derrota. `ChapterFlow` e `ChapterDefinition` existentes registram apenas `chapter2_completed` e retornam à `BaseImprovisada`. Não registra `gusini_rescued`, `chapter1_completed` ou um resgate do Macaquini que ainda não foi representado em uma cena narrativa.

As 164 criaturas dão 800 pontos cada; nove bônus de horda dão 9000; o boss dá 40000 e um abate. Concluir a fase rende 180200 pontos e 165 abates. O ranking existente continua compartilhado entre capítulos; não foi criado filtro de fase.

## Arte e fontes

Novas artes de morcego, vespa e cenário foram feitas com o gerador integrado de imagens. Originais, prompts e hashes ficam em `SourceArt/JungleStage_v01/`. O exportador `Tools/Art/prepare_jungle_stage.cjs` preserva os originais, exporta por nearest-neighbor, quantiza uma paleta fechada e verifica transparência binária. Bichos finais: 96×66 e 80×62; cenário: 480×320. Imports: Point, PPU 100, Full Rect, sem mipmaps/compressão.

`runtime_manifest.json` separa os dados de arte/pivôs/hitboxes dos parâmetros iniciais de gameplay. A arma e o projétil são cópias dos exports existentes. O fundo distante usa uma imagem opaca com margem e deslocamento suave; oito cipós de sprite próprio balançam independentemente em três planos e 18 pequenos pontos de folhagem derivam no vento. A arte nova dos cipós e seu prompt ficam nas mesmas pastas de fontes. O centro da arena fica aberto. A ordem do fundo é Default/-110, atrás do combate; as camadas BackgroundSky do HUB ficam acima de Default e não devem ser usadas aqui. O relógio do vento usa tempo escalado e congela junto da luta.

## Validação repetível

- **Crocodilo → Fase 2 → Validate jungle assets** / `JungleStageTools.Validate`: imports, prefabs, scripts, evento nativo, ausência de movimento embutido, nove hordas, dados de capítulo, build e preservação das referências do capítulo 1.
- **Validate jungle in Play Mode** / `JungleStagePlayValidation.Run`: percorre as nove hordas e escolhas reais, conta inimigos/pontos, testa os três padrões, pausa, dano dos cipós, morte/disable, ataques dos bichos e entrada no menu. Usa progresso isolado e não prepara nome para ranking. As hordas são aceleradas para validação funcional; isso não mede dificuldade humana.
- `Tools/validate_ci.py`: metadados, GUIDs e configuração do checkout. Não substitui compilação nem Play Mode.

`JungleStagePlayValidation.Showcase` verifica movimento real de rig/fundo, encaixes na hierarquia, pausa e presença do boss nos pixels em três resoluções, e captura 16 s do PlayerLoop com áudio. O retrabalho passou oito verificações de movimento e 60 da fase completa, sem erros de gameplay. A regressão existente do Gusini passou 49 verificações, incluindo transformação, ameaças, núcleo, pausa e limpeza; validações nativas de assets/build também passaram. O áudio capturado tem 16 s e pico 0,203. Vídeo H.264/AAC de 15,48 s a 20 FPS, com 0,5 s iniciais do fade da pausa removidos. Os golpes de cipó usam segmentos da arte nativa com folhas/casca, em vez das faixas verdes do protótipo; o aviso continua correspondendo à colisão. Logs locais: `Logs/JungleStage/`. Capturas: `Docs/ArtPreviews/JungleStage/Gameplay/`.

O indexador de busca do Editor 6000.6 lançou uma exceção intermitente de startup fora do player. O filtro restrito ao stack `UnityEditor.Search.SearchDatabase` foi alinhado no validador do Gusini com o que os validadores de cartas e selva já fazem; erros de combate continuam fazendo o teste falhar.

## Limites desta versão

Build Windows separado: `Builds/Windows-StageTwo/CrocodiloInvaders.exe`, gerado pela Unity 6000.6.4f1 com `CrocodiloWindowsBuild.Build`, saída total de 167782432 bytes. O build anterior Windows-DEV foi preservado. Inicialização real headless de 8 s não registrou as exceções de runtime pesquisadas; isso não substitui jogar manualmente. A interação do menu foi validada em Play Mode, incluindo seleção da selva e preservação do botão Jogar original.

Pacote completo: `Builds/Pacotes/CrocodiloInvaders-Fase2-Selva-Animada-Windows.zip`, com README e checksum SHA-256 ao lado. A estrutura dos 182 itens foi verificada, incluindo EXE, UnityPlayer.dll e Assembly-CSharp.dll. O executável precisa ser mantido junto da pasta de dados e dependências. Para testar: extrair tudo → abrir o EXE → FASE 2 · SELVA.

Ainda não estão integrados: transformação e segunda forma do Macaquini, cinemática de libertação do macaco, trilha musical própria, falas, árvores do fundo separadas em camadas e terminal de missões no HUB. O balanceamento e a sensação de ameaça requerem avaliação humana com os controles e com diferentes escolhas de cartas. Não houve commit nesta tarefa.
