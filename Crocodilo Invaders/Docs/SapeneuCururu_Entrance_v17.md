# Abertura exclusiva do Sapeneu Cururu v17

A décima rodada continua sendo a rodada do boss, depois das nove hordas existentes. O anúncio original ONDA 10 é preservado e passa a conduzir uma apresentação exclusiva do lago:

1. Aviso `PERIGO NO LAGO`, escurecimento leve, barras de cinema e a mensagem de que algo está se mexendo.
2. Faixa de água animada com nome completo, `O DONO DESTE BREJO`, retrato do Sapeneu com o pneu e detalhes de vitória-régia.
3. O boss real nasce submerso e emerge até a posição de combate em 1,7 s, com ondulações e respingo de chegada.
4. Após 0,5 s de assentamento, a apresentação sai e o combate recebe controles e hitboxes ativos.

O retrato usa a arte já aprovada; a faixa é uma Graphic uGUI com ondas e corrente, sem novo bitmap. O aviso dura 1,05 s e a revelação 2,3 s; o tempo total, incluindo ONDA 10, é aproximadamente 6,6 s. Dois efeitos sonoros originais sintetizados (água e coaxo) são roteados ao canal Effects e obedecem à pausa e ao volume existente.

`WavesUI` chama a apresentação apenas se a cena possui LakeBossEntrance e o boss é Sapeneu. Os outros bosses mantêm WaveAnnouncer/BossAlertTheme. O token de anúncio original continua pertencendo ao WavesUI; é liberado ao fim do retrato para instanciar o boss. Durante a emergência, Boss.HoldCombat impede ataques e o jogador fica com gameplay desativado e proteção por dono. A chegada usa EnemyMovement.CompleteScriptedArrival para finalizar a entrada antes de liberar a luta. Cancelamento libera o token e a proteção; fim da run e troca de cena descartam a apresentação.

Canvas Overlay/240, acima do HUD e abaixo da pausa/300. Tempo escalado; pausa congela aviso, movimento, água e trilha de entrada. Não altera composição de hordas, cartas, progressão ou mecânicas da luta. Entrada DEV usa o mesmo caminho do anúncio da rodada do boss.

Instalação nativa nas cenas StageTwoLake e SapoDoPneuTest por LakeBossEntranceTools.Install, também chamado no instalador completo. Logs/restauração em `Logs/LagoDoPneu/v17`; capturas e vídeo em `Docs/ArtPreviews/SapeneuCururu_Entrance_v17`. Fontes dos sons em `SourceArt/LagoDoPneu_Entrance_v17`.

Protótipo local. Não commitar, fazer push ou atualizar PR até o usuário finalizar.

Validação Play Mode pela entrada real DEV: **22 checks, zero erros**, cobrindo ONDA 10 antes do aviso, espera até revelar o nome, legibilidade, proteção contra dano, pausa, surgimento submerso, ausência de ataque antecipado, liberação dos controles/hitboxes, continuidade das moscas/folhas, desativação da apresentação sem travar o token e encerramento durante o título sem spawn atrasado. Capturas revisadas e vídeo decodificado sem erros.

Build Windows `Windows-SapeneuCururu-Entrance-v17` concluída; inicialização do executável sem exceções. ZIP `Builds/Pacotes/CrocodiloInvaders-SapeneuCururu-Entrance-v17-TESTE-LOCAL-Windows.zip`: 191 arquivos verificados por hash, 124838037 bytes, SHA256 `8d9daeea22d074947dbb967a62654fac67792318a59094f4e6adc24ee1e47700`. HEAD `f16c05ec6783d96cf50a4ef2acb6e940f8ac2137` e index vazio preservados.
