# Conquistas v01

Sistema local integrado ao Unity ativo. Acesso pelo botão CONQUISTAS do menu principal e do menu de pausa. Painel uGUI com quatro medalhas por página, filtros Todas / Desbloqueadas / Pendentes, progresso, data de desbloqueio e total. Navegação por mouse, teclado e controle; Voltar/Esc/B devolve o foco ao menu de origem. Abrir pela pausa mantém o jogo pausado.

O popup no canto inferior direito não captura cliques, foco ou input, nem pausa a partida. Cada conquista aparece uma única vez, em fila: entrada de 0,22 s, exibição de 3 s, saída de 0,2 s e intervalo de 0,18 s. Cartas, pausa, diálogos, transformações e finais seguram a apresentação. Avisos pendentes sobrevivem à troca de cena. Pausar um aviso em andamento também congela seu tempo e seu áudio.

## Catálogo

| ID estável | Nome | Condição |
| --- | --- | --- |
| first_wave | Primeiro Expediente | Concluir a primeira onda. |
| sport_mode | Modo Esportivo | Dez dashes que efetivamente evitem dano, no máximo um registro por dash. |
| boss_special | Bafo de Respeito | Especial causar dano a um boss. Disparar ou acertar com tiro normal não conta. |
| clean_wave | Casco Intacto | Concluir uma onda observada desde o anúncio sem receber dano. |
| gusini_defeated | Ganso Depenado | Derrota final do Gusini; a primeira forma não conta. |
| macaquini_defeated | Descascou o Problema | Derrota final do Macaquini. |
| sapeneu_defeated | Borracha Furada | Derrotar o Sapeneu Cururu. |
| all_card_kinds | Carteira Assinada | Aplicar os cinco tipos distintos de cartas, entre partidas. |
| first_mythic | Hoje Eu Tô Mítico | Aplicar uma carta mítica. |
| last_tooth | Por Um Dente | Derrotar um boss estando vivo e com até 10% da vida máxima. **Oculta.** |

Antes do desbloqueio, a última conquista exibe somente `CONQUISTA OCULTA`, interrogação e uma mensagem genérica. Nome, descrição, ícone real e condição/progresso não aparecem. Não há botão para revelar antecipadamente. Depois, a medalha e seu popup revelam os detalhes. Conta no total de dez conquistas.

A [documentação Steamworks](https://partner.steamgames.com/doc/features/achievements/ach_guide) descreve conquistas spoiler com nome/descrição ocultos até desbloqueio ou revelação manual. Aqui a revelação manual foi omitida conforme o pedido do usuário. Não há integração ao SDK/serviço Steam; os IDs estáveis permitem uma ponte posterior sem mudar as condições locais.

## Integração e persistência

- `AchievementCatalog`: os dez IDs e condições.
- `AchievementStore`: `achievements.json` versionado em `Application.persistentDataPath`, escrita temporária/atômica, backup, recuperação e proteção contra versões futuras. Falha de disco reverte o estado em memória e não emite popup falso.
- `Achievements`: publica progresso e desbloqueio somente depois de salvar, com idempotência e bloqueio de sessões DEV.
- `AchievementRuntime` / `AchievementTracker`: um prefab persistente, inscrição nos eventos reais do player, ondas e bosses; sem varredura de objetos em cada frame. Cenas de teste não registram conquistas; a cena de produção `PlayerMovementTest` continua elegível.
- `AchievementView` / `AchievementRow`: popup e painel. Ocultação aplicada na apresentação, sem tooltip ou botão de spoiler.
- Eventos adicionais sem alterar as regras existentes: `Health.DamagePrevented`, `PlayerRunUpgrades.CardApplied`, `PlayerCombat.SpecialProjectileCreated`, `Projectile.DamagedTarget`.
- Prefab de inicialização: `Assets/Resources/Achievements/Achievements.prefab`.
- Instalador idempotente: `AchievementTools.Install`; usa APIs nativas do Editor para inserir os botões nos prefabs existentes.

Nenhuma skin foi bloqueada ou recompensa de gameplay adicionada. A skin Crocs está guardada e desativada conforme o pedido mais recente; a nave original está equipada.

## Áudio e arte

Som original gerado por `Tools/Audio/generate_achievement_chime.cjs`: clique metálico e três notas crescentes, mono PCM16/44,1 kHz, 0,64 s, pico 0,48 antes do ganho local 0,42. Uma única fonte, roteada pelo mixer Effects e Master. Não copia o som da Steam. Receita em `SourceArt/Achievements_v01/audio.json`.

Fontes e ícones reutilizam a arte do próprio jogo. Moldura gerada em grade, azul-marinho/marfim/dourado, Point/sem mipmaps/compressão. Nada de arte ilustrada externa. Os 26 sprites da skin Crocs e as animações de combate permanecem intactos.

## Validação

`AchievementPlayValidation.Run` primeiro testa a persistência com diretórios isolados (incluindo backup corrompido, versão futura, duplicação e falha de disco), depois exercita menu, pausa, ondas, recompensas, dashes protegidos, cartas, colisão real de especial com boss e as duas formas do Gusini. Usa saves separados em `Logs/Achievements` e não desbloqueia as conquistas do jogador.

Capturas reais e resultado final em `Docs/ArtPreviews/Achievements_v01`; log em `Logs/Achievements-Play.log`. Conferência visual em 1200×720, 800×600 e 1920×1080. O teste também verifica geometria visível dos nomes e reprodução nativa do AudioSource.

Build Windows validado: `Builds/Windows-Achievements-v01/CrocodiloInvaders.exe`. Conferir o log `Logs/Achievements-Build.log` para o resultado. Sem commit/push nesta tarefa.

Resultado verificado: 30 verificações de dados + 66 de Play Mode + 12 de encaixe visual das dez descrições = **108 verificações / zero erros**. As duas primeiras execuções de preparação identificaram um tipo de exceção de save não coberto e o bloqueio normal da animação de entrada do menu; ambos tratados antes da rodada final. Revisão visual corrigiu altura dos títulos, posição do botão para não sobrepor DEV e espaço de duas linhas nas descrições do popup. Relatórios `validation.json` e `visual-validation.json`; o som foi verificado em reprodução no AudioSource nativo e pela receita PCM, sem alegação de escuta crítica manual.

Build final concluído com sucesso: 212.565.337 bytes reportados. Inicialização headless por oito segundos sem exceções/erros no log. Checkout validado: 2.704 GUIDs únicos e todos os assets com metadados. Diff de código e dos dois prefabs de menu sem erros de whitespace.

## Popup compacto — revisão v02 (05/11/2026)

Pedido do usuário: menor e mais limpo, próximo ao toast da Steam. Popup passa de 390×116 para **300×68** (55% menos área), com fundo cinza-escuro liso, ícone 40×40, legenda discreta de 12 e nome de 18. Margens inferior/direita de 12. A condição completa fica no painel; o toast mostra somente indicação de desbloqueio e nome. Configuração idempotente em AchievementTools.ConfigureCompactToast / CompactPopup.

Validação focada: 13 verificações visuais / zero erros, incluindo os dez nomes sem recorte. Gravação nativa da primeira onda e duas conquistas em save isolado: 225 quadros / 11,779 s, desbloqueio real e zero erros. Vídeo com áudio em `Docs/ArtPreviews/Achievements_v01/conquista-popup-compacto.mp4`, imagem `popup-compacto.png`. Build separado `Builds/Windows-Achievements-Compact-v02/CrocodiloInvaders.exe`; log `Logs/Achievements-Compact-Build.log`.


## Ícones exclusivos v03 e Crocs arquivada — 05/11/2026

Dez ícones próprios em Assets/Resources/Achievements/Icons_v03 substituem os recortes provisórios. Fontes/prompts/hashes em SourceArt/AchievementIcons_v03. AchievementIconTools.Install restaura a nave original, configura importação e liga os dez sprites por ID; Validate verifica todas as cenas habilitadas e Resources sem dependências da Crocs. Os 26 sprites e as animações da skin ficam salvos sem uso ativo; instalador da skin não equipa mais automaticamente. Prévias reais em Docs/ArtPreviews/AchievementIcons_v03.

## Acesso artístico no menu — v04 (05/11/2026)

O acesso às conquistas virou uma medalha dourada em pixel art, com a mesma família de plaquinha do troféu de leaderboard. Os dois ficam lado a lado no canto inferior direito, alinhados pela base. O retângulo do canto superior foi substituído no mesmo objeto, preservando callback e retorno de foco. A medalha participa do MenuIntro e usa cores de foco/pressionamento do leaderboard; esquerda/direita navegam entre os dois. Reinstalar conquistas ou leaderboard preserva essa integração.

Fonte/prompt/receita em SourceArt/AchievementMenuButton_v04; sprite nativo Assets/Art/AchievementsMenu/achievements_medal.png (128x128 transparente/Point). Instalador AchievementMenuButtonTools.Install. Validação de Play Mode: 12 verificações / zero erros, incluindo raycast, clique, submit de teclado/controle, foco restaurado e leaderboard funcionando; capturas reais em 1200x720, 800x600 e 1920x1080 em Docs/ArtPreviews/AchievementMenuButton_v04. A captura aguarda o fade nativo de foco estabilizar. Build Windows-Achievements-MenuMedal-v04; log Logs/AchievementMenuMedal-v04-Build.log.

## Posição e acesso à fase 2 — v05 (05/11/2026)

A medalha de conquistas fica no canto inferior esquerdo, espelhando a margem e o tamanho do leaderboard à direita. O atalho público da fase 2 foi removido; o acesso de teste fica em DEV > FASE 2 · LAGO DO PNEU. DevelopmentStageSession mantém o fluxo completo de ondas, sem registrar conquistas ou progresso de campanha. Navegação por teclado/controle passa pelo menu central entre os dois cantos.

AchievementMenuButtonPreview.Run: 20 verificações / zero erros; capturas em Docs/ArtPreviews/AchievementMenuButton_v05 nas três resoluções, incluindo DEV e início da fase completa. Build separado Windows-Achievements-Left-v05; log Logs/AchievementMenuLeft-v05-Build.log. Esta revisão substitui a posição da v04; a arte permanece a mesma.

Resultado v05: build Windows concluído com sucesso (212.489.348 bytes reportados); executável permaneceu ativo por oito segundos, sem exceções ou erros no log de inicialização.


## Conferência do PR sobre a main atual

Branch `codex/conquistas-crocs-menu` preparada sobre `Tutsdevtj/main` em `4299e4c`, preservando as atualizações do personagem do hub. Na Unity 6000.6.4f1: 30 verificações de persistência + 66 de Play Mode + 20 de menu/DEV = **116 verificações, zero erros durante as suítes**. Checkout: 2717 GUIDs únicos; 925 arquivos locais anteriores preservados por SHA256. Relatório `Docs/ArtPreviews/AchievementMenuButton_v05/pr-validation.json`.

A Unity apresentou `ArgumentOutOfRangeException` no índice interno `UnityEditor.Search.SearchDatabase` durante o startup do Editor na cópia nova. As suítes foram iniciadas depois dessa etapa com um auxiliar temporário, removido do PR. O build Windows v05 descrito acima é anterior à preparação da branch; a branch atual foi compilada e exercitada no Editor, sem gerar outro executável Windows.

Commits novos com author e committer **Picolii / bernardo.p2106@gmail.com**. Skin Crocs guardada sem uso; mudanças locais anteriores de Lago e Macaquini ficaram fora deste PR.

## Correções da revisão de código — 09/11/2026

- Macaquini e Sapeneu ficam com `released: false` no catálogo: as duas fases só abrem pelo DEV, que não grava conquistas. Saem da lista e do total, que passa a 8. Ao liberar uma fase ao público, basta retirar esse argumento.
- Popup movido para o canto superior direito, sem cobrir a barra do boss.
- Mudar a meta de uma conquista não invalida mais o save: desbloqueios continuam válidos e progresso acima da nova meta fica a um passo de desbloquear.
- Falha de gravação mantém a conquista em memória e tenta salvar de novo na próxima alteração, na troca de cena e ao fechar o jogo.

Validação: 36 verificações de dados + 66 de Play Mode, zero erros.
