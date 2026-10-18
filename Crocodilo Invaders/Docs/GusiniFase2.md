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

Primeiro ciclo: Energia → Ovo → Esquadrilha → Tentáculos. Ciclos seguintes embaralham os três ataques comuns, sem reposição, e terminam com Tentáculos. Recuperação padrão de 0,8 s, editável. O boss segura a posição ao atacar e patrulha na recuperação. Desde o ajuste de 18/10/2026, a patrulha da F2 é 50% mais rápida (2,4 → 3,6 unidades/s antes da frenagem/reação a acertos), com multiplicador editável no componente da fase. A F1 conserva sua velocidade; encerrar a F2 remove o multiplicador.

- **Rajada:** três bolas nos eventos de 490/790/1090 ms, com 300 ms entre releases. A primeira rajada segura a carga máxima por mais 300 ms. Spawn no deslocamento nativo (-138,-24), escala do boss, crescimento de 180 ms, loop de 70 ms e pop de 250 ms. Cada bola avança a 185 px/s e tem velocidade vertical independente de 5,6/6,25/6,9 unidades/s, alternando a direção inicial e refletindo perto das bordas. Hitbox circular de 23 px, dano 3. Não pode ser abatida.
- **Ovos:** três ovos no evento de 400 ms, começando no deslocamento (-26,-34) e espaçados inicialmente 32 px para a esquerda, atrás do boss no primeiro frame. Quantidade e espaçamento inicial editáveis no componente da fase. Sem Health ou hitbox de alvo, não podem ser abatidos. Queda de 250 ms e deploy de 60/70/80 ms com POP. Cada ovo então se desloca suavemente até um ponto aleatório da arena, com margem de 0,8 unidade das bordas e separação desejada de 2,2 unidades entre pontos (ambas se adaptam em arenas pequenas). Até 64 tentativas por ponto; se o espaço for insuficiente, escolhe o candidato mais afastado. Viagem de 0,65–0,77 s para o primeiro, acrescida de 0,55 s por ovo seguinte. Só ao chegar começa a sequência original de pulsos de 1,15 s, rachaduras de 200/160 ms e estouro de 120 ms. Os três estouram em locais distintos e momentos separados, sem teleporte. Cada ovo libera cinco gemas (15 no total) nas direções cima, diagonal superior esquerda, esquerda, diagonal inferior esquerda e baixo; **420 px/s** (antes: 145), raio de 4 px, dano 2. Sprites já orientados, sem rotação. Áudio continua limitado pelo pool compartilhado; a pausa congela o trajeto e os relógios individuais.
- **Esquadrilha:** cinco marcadores na borda em 300 ms; cinco gansinhos criados fora da tela em 580 ms. Marcadores acompanham os atrasos reais do V e a nova velocidade de entrada. Duas variantes originais, 2 HP, sem pontos de inimigo comum; dano de contato 2. V aberto, avanço de **180 px/s** (antes: 130), bob ±8 px/0,8 s defasado. Durante toda a F2, inclusive com vida cheia, slots 1 e 4 param a 70% da largura, fixam a direção para o player, piscam 400 ms e mergulham em linha reta a **600 px/s** (antes: 400 no primeiro ajuste; originalmente 280 e somente abaixo de 60% de vida). Não perseguem após fixar a mira. Rigidbody2D dos gansinhos usa Continuous.
- **Tentáculos v2:** início de 610 ms; três golpes, cada um com dois avisos de 900 ms e uma faixa segura limpa. Faixa segura: meio → baixo → topo. Wind-up F0/F1 em loop; `lash_hit` inicia os tentáculos de faixa, com seus 530 ms e hitboxes nativas por frame. Pivô na borda direita, escala horizontal para a largura da câmera; vertical equivalente a 360 px nativos de tela. Espinhos ficam fora do retângulo de dano. Cada golpe causa 4 uma única vez; i-frames também consomem o contato. Após o terceiro, resistência com F4/F5 em loop por **2 s** e multiplicador de dano recebido **×2** no boss inteiro durante a exposição do núcleo. Preserva dano fracionário; volta a ×1 ao fechar ou cancelar.

Animações e direções dos projéteis preservam a fonte; os trajetos/tempos individuais dos ovos e as velocidades refletem o rebalanceamento pedido em 18/10/2026. Avaliar a dificuldade jogando uma run com cartas reais.

Primeira validação do rebalanceamento de 18/10/2026: compilação e integração nativas da Unity passaram, assim como 45 verificações em Play Mode, antes da segunda aceleração e dispersão dos ovos. Vida base de 20 HP aplicada no prefab do player; kit de teste de 240 HP preservado. Sem playtest manual de uma run completa ou nova avaliação da mixagem.

Segunda rodada: **49 verificações em Play Mode passaram**, com compilação e integração nativas. Confirmados 15 projéteis a 420 px/s, três estouros em pontos internos separados por pelo menos 2,15 unidades na arena de teste e intervalos de pelo menos 0,4 s; mergulho medido a 600 px/s, mira travada e limpeza de ameaças. Log: `Logs/GusiniEggScatterBalance.log`. A mixagem não foi reavaliada nesta rodada.

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
