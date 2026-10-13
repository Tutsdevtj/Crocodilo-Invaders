# Crocodilo Invaders

Projeto Unity **6000.6.4f1**, com nove hordas, melhorias por cartas e o boss na décima onda.

Abra esta pasta **Crocodilo Invaders/** pelo Unity Hub e a cena
`Assets/Scenes/MainMenu.unity`. Jogar solicita o nome e abre o loading; pressione Enter quando estiver
pronto para iniciar. A cena PlayerMovementTest também tem um botão provisório de início.

- Setas: movimento; Espaço: tiro contínuo; C: especial carregado.
- Esc: pausa. Opções ajusta volume geral, efeitos e música, com valores salvos.
- O troféu no canto inferior direito abre o leaderboard local. Partidas concluídas
  são salvas ao voltar ao menu, com sete resultados por página.
- Cada horda oferece três cartas, com um reroll gratuito e bônus acumulados na partida.
- Mouse ou setas + Enter escolhem cartas; 1–3 escolhem diretamente; R faz o reroll.
- A décima onda traz o Executor V-9, com ataques variados e reforços interceptores.

## Executável Windows

Use **Crocodilo → Build Windows** no Editor. A saída é `Builds/Windows/`.
Distribua a pasta inteira: o `.exe` depende da pasta `_Data` e das DLLs ao lado dele.

Após configurar a ativação da Unity nos secrets do repositório, cada push/merge em
`main` gera e publica um ZIP Windows em **GitHub Releases**. Veja
[Build Windows e ativação](Docs/BuildWindows.md) para a configuração, os checks e a execução manual.
Cache da Unity, logs e builds ficam fora do Git.

## Documentação

- [Ondas](Docs/Ondas.md), [inimigos](Docs/Inimigos.md) e [boss](Docs/Boss.md).
- [Cartas e raridades](Docs/Cartas.md), [cura](Docs/Cura.md) e [créditos](Docs/Creditos.md).
- [Menu](Docs/Menu.md), [loading](Docs/Loading.md), [pausa](Docs/Pausa.md) e [áudio](Docs/OpcoesAudio.md).
- [Leaderboard e salvamento local](Docs/Leaderboard.md).

A cena `Assets/Scenes/Jogo.unity` e `CrocodiloGame.cs` preservam o port anterior.
O projeto Portugol e a pasta `unity/` antiga continuam na raiz do repositório.
