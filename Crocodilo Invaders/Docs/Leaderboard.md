# Leaderboard local

Jogar abre o campo de nome (até 16 caracteres). Enter ou Iniciar abre o loading existente.
O último nome fica preenchido na próxima partida. Esc/Voltar cancela o formulário.

O troféu no canto inferior direito abre o ranking. São sete resultados por página,
ordenados pela maior pontuação. Empates preservam a ordem de conclusão. Os três
primeiros colocados recebem dourado, prata e bronze. Próxima e a seta `<` navegam
entre páginas; Voltar, X ou Esc retornam ao menu. Nomes repetidos são permitidos:
cada partida concluída tem sua própria entrada.

Vitória e derrota capturam a pontuação final em `WaveController.EndRun`. O botão
Voltar do resultado e o retorno pela pausa salvam antes da troca de cena. Fechar
o aplicativo após o resultado também salva. Abandonar uma partida em andamento
não registra resultado; iniciar outra partida zera a pontuação normalmente.

## Persistência

`LeaderboardStore` usa JSON versionado em `Application.persistentDataPath`:
`leaderboard.json`, `leaderboard.json.bak` e um arquivo temporário durante a escrita.
No Windows, com os nomes atuais do projeto, a pasta é
`%USERPROFILE%/AppData/LocalLow/Arthur Reis/Crocodilo Invaders`.
O arquivo continua disponível entre execuções e atualizações do build.
É um ranking deste computador, sem servidor ou sincronização online.

O arquivo temporário é gravado e sincronizado antes da substituição do principal;
o último principal válido vira backup. Um principal corrompido tenta recuperar
o backup sem sobrescrever essa cópia com dados corrompidos. Uma falha de escrita
mantém o resultado pendente para nova tentativa e apresenta aviso no ranking.
Identificadores de partida impedem duplicações durante tentativas de salvamento.

Os assets originais foram copiados para `Assets/Art/Leaderboard`. Retângulos dos
sprites excluem margens transparentes sem alterar os PNGs. As interfaces são
prefabs editáveis em `Assets/Prefabs/UI/Leaderboard.prefab` e `PlayerName.prefab`.
A fonte Press Start 2P de CodeMan38 está em `Assets/Fonts/PressStart2P`, junto da
licença SIL Open Font License.

Referências: [pasta persistente da Unity](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Application-persistentDataPath.html),
[fonte no Google Fonts](https://github.com/google/fonts/tree/main/ofl/pressstart2p).
