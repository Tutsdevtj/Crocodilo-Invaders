# Gusini — Fase 2

Implementação do pacote `Gusini_Fase2_Pacote`, recebido em 17/10/2026. Somente essa pasta foi usada como fonte; anexos anteriores foram desconsiderados. O boss tem **duas fases**; não há F3.

## Fluxo e integração

**Kit de teste do Editor:** ao dar Play em `GusiniBossTest`, `BossDevelopmentStart` aplica 240 HP cheios, dano normal ×4, especial ×3, cadência ×2,5 (0,18 s), movimento ×1,4, dash com distância ×1,3/cooldown ×0,4/i-frames ×1,4 (limitados à duração do dash). Especial começa carregado e recarrega com cinco tiros. Os valores principais ficam no Inspector do componente. Aplica depois do reset da run e somente nessa cena; o prefab do player e as partidas normais conservam seus atributos.

O mesmo `Assets/Prefabs/Boss/Gusini.prefab` continua ligado a PlayerMovementTest, StageOneWaves e GusiniBossTest. A F1 mantém os 258 HP, ataques, velocidade 2× e sons aprovados. `GusiniPhaseTwo` só assume quando a vida da F1 acaba.

`Health.Depleted` permite interceptar o esgotamento antes de `Died`. O controlador torna a entidade invulnerável e chama `RestoreNextPhase(320)`; esse método só aceita uma entidade esgotada e invulnerável. Não muda a semântica de Heal nem revive jogadores. A HUD observa o estado da transição e conserva a barra F1 vazia mesmo com o reservatório interno da F2 já preparado. O último tiro da F1 não transfere excesso de dano à F2.

A transformação dura **4,1 s**, com todos os frames e eventos originais, na mesma posição do boss. Cancela pendências e remove ameaças da F1 imediatamente. Entrada, patrulha e hitboxes ficam suspensas; o player pode continuar se movendo. Golpe final e pico (2,53 s) dão shake curto na câmera. Em 160 ms, oferece uma caixa de cura de 6 HP perto do jogador se ele precisar e não houver outra ativa; reutiliza o spawner, seus limites e o cooldown após remoção. Não cria caixas duplicadas nem cura automaticamente.

Revelação em **2,82 s**: a barra sofre tomada verde no lugar, flash de silhueta F1/F2, troca de peças, carga entre +0,40 e +0,98 s e glint final. A IA só começa após o fim completo do clip, em 4,1 s. A F2 tem **320 HP**, editáveis no componente, Animator em **1×**, idle estático do último sprite da transformação e bob feito pela Unity.

Apenas a derrota da F2 emite `Boss.Defeated`: uma recompensa de 40.000 pontos e um abate, seguidos do fluxo de resultado existente. Sem arte de morte própria no pacote, a pose final desaparece por fade. Nenhuma terceira forma ou arte grotesca foi criada.

## Ataques

Primeiro ciclo: Energia → Ovo → Esquadrilha → Tentáculos. Ciclos seguintes embaralham os três ataques comuns, sem reposição, e terminam com Tentáculos. Recuperação padrão de 0,8 s, editável. O boss segura a posição ao atacar e patrulha na recuperação.

- **Rajada:** três bolas nos eventos de 490/790/1090 ms, com 300 ms entre releases. A primeira rajada segura a carga máxima por mais 300 ms. Spawn no deslocamento nativo (-138,-24), escala do boss, crescimento de 180 ms, loop de 70 ms e pop de 250 ms. Cada bola avança a 185 px/s e tem velocidade vertical independente de 5,6/6,25/6,9 unidades/s, alternando a direção inicial e refletindo perto das bordas. Hitbox circular de 23 px, dano 3. Não pode ser abatida.
- **Ovo:** um ovo no evento de 400 ms, deslocamento (-26,-34), atrás do boss no primeiro frame. Sem Health ou hitbox de alvo, não pode ser abatido. Queda 250 ms, deploy 60/70/80 ms com POP, descida 1,15 s com pulsos originais, rachaduras 200/160 ms e estouro 120 ms. Deriva 15 px/s e descida 23 px/s. Cinco gemas nas direções cima, diagonal superior esquerda, esquerda, diagonal inferior esquerda e baixo; 145 px/s, raio de 4 px, dano 2. Sprites já orientados, sem rotação.
- **Esquadrilha:** cinco marcadores na borda em 300 ms; cinco gansinhos criados fora da tela em 580 ms. O líder entra aproximadamente em 1,2 s; marcadores dos outros permanecem conforme os atrasos do V. Duas variantes originais, 2 HP, sem pontos de inimigo comum; dano de contato 2. V aberto, avanço de 130 px/s, bob ±8 px/0,8 s defasado. Abaixo de 60% do HP da F2, slots 1 e 4 param a 70% da largura, fixam a direção para o player, piscam 400 ms e mergulham em linha reta a 280 px/s. Não perseguem após fixar a mira.
- **Tentáculos v2:** início de 610 ms; três golpes, cada um com dois avisos de 900 ms e uma faixa segura limpa. Faixa segura: meio → baixo → topo. Wind-up F0/F1 em loop; `lash_hit` inicia os tentáculos de faixa, com seus 530 ms e hitboxes nativas por frame. Pivô na borda direita, escala horizontal para a largura da câmera; vertical equivalente a 360 px nativos de tela. Espinhos ficam fora do retângulo de dano. Cada golpe causa 4 uma única vez; i-frames também consomem o contato. Após o terceiro, resistência com F4/F5 em loop por **2 s** e multiplicador de dano recebido **×2** no boss inteiro durante a exposição do núcleo. Preserva dano fracionário; volta a ×1 ao fechar ou cancelar.

Os tempos e direções são da fonte; HP, dano, velocidades e ordem são a primeira passagem de balanceamento escolhida na implementação. Avaliar a dificuldade jogando uma run com cartas reais.

## Apresentação e áudio

`BossBarFX` continua dono da vida, lag, shake, fill com origem à direita e shimmer. `BossBarPhaseTwo` troca peças, faz a tomada e anima olho, falhas do nome e seis partículas reutilizadas. F2: shimmer 3,5 s/450 ms, olho ~2,6 s, cachecol 100 ms; crítico ≤30%, fill ácido, pulso 700 ms e cachecol 70 ms. Carga não ativa o estado crítico.

O pacote visual não inclui áudio. A atualização de 18/10/2026 integra os **29 masters próprios da F2** recebidos em `Gusini_Audio_Phase2.zip`, com configuração separada e o mesmo pool central/mixer Effects. Sem coral: só o honk Mid coletivo fica ativo. Ver [Áudio da Fase 2](GusiniAudioFase2.md) para gatilhos, mixagem, fontes e validação.

Tempo escalado: pausa congela transformação, Animator, movimento, avisos, janela do núcleo, FX e áudio existente. Morte do player, StopBattle, desativação do boss e unload cancelam coroutines, multiplicador e todas as ameaças da raiz pertencente à cena. A F1 conserva seu controlador e o Executor continua separado.

## Assets e ferramentas

- 171 PNGs nativos do boss/ameaças em `Assets/Art/Boss/GusiniPhase2` e 42 PNGs x2 da HUD em `Assets/Art/UI/GusiniBossBarF2`.
- Point, PPU 100, Full Rect, sem compressão/mipmaps, tamanhos originais. Boss: pivot exato (120/256,78/196), equivalente ao pivot em pixels (120,118) medido do topo. Gansinho (19/36,0,5); tentáculo (1,0,5).
- 12 clips e controller em `Assets/Animations/Boss/GusiniPhase2`, curvas somente de sprite em Visual, eventos nativos em milissegundos. Clips de wind-up e núcleo foram separados para manter os loops extras sem alterar os timings originais.
- `Assets/Data/GusiniPhaseTwo.asset` referencia os sprites; runtime não lê JSON ou Downloads.
- Os seis JSONs, sete notas e manifest normalizado ficam em `SourceArt/GusiniPhase2`. Não importamos previews, sheets ampliadas nem executamos scripts do pacote.
- `Tools/import_gusini_phase_two.py`: copia originais selecionados e normaliza fontes. Instalação nativa: `GusiniPhaseTwoTools.Install`. Não executar instalação/rebuild como simples check.
- Menus `Crocodilo/Boss/Validate Gusini Phase 2` e `Validate Phase 2 in Play Mode`; `GusiniPhaseTwoTools.ValidateIntegration` inclui validações F1, áudio, HUD e build.
- Se reconstruir a barra F1 com a ferramenta antiga, rode a instalação F2 depois para recolocar os componentes adicionais.

Capturas e logs locais da validação ficam em `Logs/GusiniPhaseTwo`, fora do versionamento. O teste usa cena e alvo isolados, sem iniciar run nem gravar ranking. Verifica PlayerLoop/Physics2D/Animation Events reais, transição sem vitória, invulnerabilidade, pausa, releases, cinco gemas, dois tiros por ganso, três golpes, multiplicador fracionário por 2 s e limpeza.

Resultado na Unity 6000.6.4f1: **41 verificações de combate + 7 de ciclo da HUD passaram**, incluindo mergulhadores, mira travada, contato em i-frames, morte do player, cancelamento e unload de cena. A HUD reativada durante a transformação ou F2 conserva a fase e a vida precisa, sem repetir entrada/recarga; boss morto não reabre. Validações de integração F1/Executor, áudio, HUD e configuração de build também passaram. Os 213 PNGs foram comparados por SHA-256 com os originais e estão idênticos. Capturas da HUD inspecionadas em 1200×720, 1920×1080 e 800×600; avaliar dificuldade e mixagem em uma run manual continua sendo uma etapa de balanceamento.
