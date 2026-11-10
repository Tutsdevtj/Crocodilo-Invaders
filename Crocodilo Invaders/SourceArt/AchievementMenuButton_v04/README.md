# Medalha do menu de conquistas — v04

Novo botão artístico que acompanha o troféu de leaderboard. Na revisão v05, a medalha fica no canto inferior esquerdo e o troféu permanece à direita. Medalha dourada com estrela, louros e fitas ciano; plaquinha azul-marinho, ciano, dourado e texto bitmap CONQUISTAS, referenciada diretamente pela arte existente `Assets/Art/Leaderboard/MenuButton.png`.

Arte criada com `image_gen` integrado. Fonte imutável em `master.png`; prompt completo e referência em `generation.json`. Exportação usada pelo jogo: `Assets/Art/AchievementsMenu/achievements_medal.png`, 128×128 com transparência binária, Point/sem compressão/mipmaps. Receita reproduzível: `Tools/Art/prepare_achievement_menu_v04.cjs`; recorte e hashes em `manifest.json`.

`AchievementMenuButtonTools.Install` edita o botão existente por APIs nativas do Editor, sem recriar o menu ou perder seu callback. Mesmo tamanho do troféu, com âncora inferior esquerda e margem espelhada (35,28). Participa da mesma animação de entrada via MenuIntro/CanvasGroup e usa o mesmo feedback de cor do leaderboard. Navegação: medalha ↔ menu central ↔ leaderboard; o retorno do painel restaura o foco da medalha.

O instalador de conquistas e o reinstalador de leaderboard reaplicam esse posicionamento artístico. O rótulo TMP do retângulo antigo fica desativado, porque o texto agora faz parte da arte. Popup compacto, painel, dez ícones e skin Crocs arquivada mantidos.

Conferência real em Play Mode: `AchievementMenuButtonPreview.Run`; capturas em `Docs/ArtPreviews/AchievementMenuButton_v04` (1200×720, 800×600 e 1920×1080). Valida entrada, raycast/clique, teclado/controle, abertura/retorno de conquistas e funcionamento do leaderboard. Saves de teste isolados.

Build separado: `Builds/Windows-Achievements-MenuMedal-v04/CrocodiloInvaders.exe`; log `Logs/AchievementMenuMedal-v04-Build.log`. Sem commit/push nesta tarefa.

Resultado final: 12 verificações de Play Mode / zero erros; build Windows concluído com sucesso (212.490.511 bytes reportados), startup de oito segundos sem exceções no log. Checkout validado com 2719 GUIDs únicos.

## Posição e acesso à fase 2 — v05 (05/11/2026)

A medalha de conquistas fica no canto inferior esquerdo, espelhando a margem e o tamanho do leaderboard à direita. O atalho público da fase 2 foi removido; o acesso de teste fica em DEV > FASE 2 · LAGO DO PNEU. DevelopmentStageSession mantém o fluxo completo de ondas, sem registrar conquistas ou progresso de campanha. Navegação por teclado/controle passa pelo menu central entre os dois cantos.

AchievementMenuButtonPreview.Run: 20 verificações / zero erros; capturas em Docs/ArtPreviews/AchievementMenuButton_v05 nas três resoluções, incluindo DEV e início da fase completa. Build separado Windows-Achievements-Left-v05; log Logs/AchievementMenuLeft-v05-Build.log. Esta revisão substitui a posição da v04; a arte permanece a mesma.

Resultado v05: build Windows concluído com sucesso (212.489.348 bytes reportados); executável permaneceu ativo por oito segundos, sem exceções ou erros no log de inicialização.
