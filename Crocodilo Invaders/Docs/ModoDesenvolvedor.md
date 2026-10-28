# Modo desenvolvedor — teste de cartas

No menu inicial, clique em **DEV**, discreto no canto superior direito, ou pressione **F8**.
Selecione a geração e clique **Iniciar teste de cartas**. O loading normal carrega a
mesma cena da partida; confirme com Enter quando estiver pronto.

## Opções

- **Sorteio normal:** usa exatamente o catálogo, pesos e geração da partida.
- **Raridade fixa:** as três ofertas usam a raridade escolhida, mantendo três tipos distintos.
- **Ciclo das 25 cartas:** percorre todas as combinações de tipo/raridade; nove ofertas
  mostram a coleção inteira. O ciclo continua, sem limite de sessão.
- **Progresso após a onda 1, 5 ou 9:** define o progresso usado pelos pesos do sorteio normal.

## Testar na partida

A primeira oferta abre automaticamente. O teste utiliza `WaveController.WaveCleared`
e os componentes reais `CardRewardController`, `CardRewardView`, `CardOptionView`,
`UpgradeCardCatalog.Artwork` e `PlayerRunUpgrades` — não existe uma segunda galeria de cartas.
Imagens de Resources, entrada/virada, áudio, partículas, hover, comparação de atributos,
congelamento, pausa, seleção e aplicação dos bônus seguem o caminho normal.

Escolha com mouse, navegação normal ou teclas 1/2/3. Rerolls ficam disponíveis
sem limite no modo DEV, tanto pelo botão quanto pela tecla R. No jogo normal,
continua sendo um por oferta. Durante a animação ou pausa o comando fica bloqueado.
Depois da escolha, o tempo e o controle do Croc voltam e os bônus
ficam aplicados para experimentar movimento, tiros, especial e vida na cena real.
A sessão mantém a fase entre ondas, sem iniciar hordas ou boss automaticamente.

Abra **DEV · CARTAS [F8]** para gerar a próxima oferta, mudar modo/raridade/progresso,
zerar bônus e reiniciar o ciclo, ou voltar ao menu. Gerar e zerar ficam bloqueados
durante uma oferta, transição ou pausa. Alterar o modo durante uma oferta pronta
afeta as próximas gerações, incluindo o reroll; não substitui as cartas já visíveis.

O teste não prepara uma sessão de ranking, não persiste configurações de debug e
não altera o catálogo. Voltar ao menu encerra a sessão. Iniciar Jogar pelo botão
normal continua com ondas, sorteios e ranking normais.

## Implementação

`MainMenuController` cria `DeveloperMenuView` em runtime; não é preciso executar
um gerador de cenas ou reconfigurar o prefab do menu. `GameplayLoadingController`
encaminha somente sessões de desenvolvimento para `DevelopmentCardTest`.
`DevelopmentCardSession` mantém configurações apenas em memória e cai no gerador
original quando inativo. As cenas e os prefabs de recompensa não foram modificados.

Branch: `codex/cartas-expressoes-v12`. Foils/holográficos novos continuam adiados.

## Validação

O teste de dados em `Tools/Validation/CardDevelopment` compila o código real do
catálogo, pesos e sessão de desenvolvimento em um host .NET com tipos de dados
Unity mínimos. Não emula renderização, eventos de interface nem Play Mode.
São 1464 verificações de isolamento da sessão, equivalência do sorteio normal,
raridades fixas, entradas reais do catálogo, três tipos distintos, sequência
completa e encerramento, incluindo 200 rerolls no controlador de recompensa real,
bloqueio durante animação/pausa e preservação do limite na partida normal.
Coroutines/UI são colaboradores mínimos sem execução neste teste de lógica.
Verificação de checkout/GUIDs e novo buildUnity também passaram.

Para repetir (requer .NET 10, sem pacotes externos):

```powershell
dotnet restore Tools/Validation/CardDevelopment/CardDevelopment.csproj --configfile Tools/Validation/CardDevelopment/NuGet.Config
dotnet run --no-restore --project Tools/Validation/CardDevelopment/CardDevelopment.csproj
```

O projeto foi compilado na Unity6000.6.4f1 em 21/10/2026. Pacote atual
`Builds/Pacotes/CrocodiloInvaders-DEV-Cartas-v15-Windows.zip`: extrair e abrir o EXE, sem Unity.
Inclui [efeitos externos progressivos](EfeitosCartas.md), espaçamento maior e
adaptação à janela. A v13 corrige textos e inclui verso novo e quatro sons próprios.
Na v14 as 25 aberturas e bordas internas são recompostas sem faixas da arte antiga.
Na v15 as frentes e o verso recebem transparência externa, sem o fundo preto opaco.
O validador `CardEffectsPlayValidation.Run` passou 94 checks, cobrindo as 25 cartas
na cena real: cinco raridades, pausa/retomada, reroll, escolha, nova oferta e
retorno ao menu. Capturas reais conferidas em 1200×720, 800×600 e 1920×1080.
Foram corrigidas referências destruídas e a restauração do relógio na saída
durante a recompensa. Ainda vale testar manualmente o acesso pelo menu, os três
modos, acúmulo/reset dos bônus e uma partida normal após sair do teste.
