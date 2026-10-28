# Integração da coleção de sistemas da nave — 22/10/2026

As 25 cartas de produção foram substituídas pelas artes aprovadas: comuns/raras/épicas v19, lendárias full art v20 e míticas v20 com a borda esquerda do Ataque corrigida. As fontes finais foram reunidas em `SourceArt/CardCollection_v21_Integration/Sheets` antes de retirar as versões anteriores.

O exportador `Tools/Art/apply_ship_cards_v21.cjs` separa as cinco silhuetas de cada prancha, remove apenas o fundo externo conectado e gera os 25 PNGs RGBA. As molduras que se tocam na prancha são separadas por uma faixa mínima seguindo o vão entre cartas. Os painéis de larguras diferentes são normalizados na grade lógica 180×268 e duplicados por vizinho mais próximo para 360×536. A silhueta completa é preservada, com margem transparente, inclusive bala, blindagem e efeitos míticos sobrepostos à borda.

Nenhum `.meta` das cartas foi alterado. Catálogo, atributos, valores, caminhos Resources, verso, áudio, efeitos, escolhas e rerolls mantêm a configuração existente. `--check` verifica 25 hashes, dimensões, transparência externa, metadados e integridade do catálogo e verso.

## Backup e limpeza

Pasta na raiz do repositório: `Backup/Cartas/2026-10-22_Anteriores_v21`, fora de `Crocodilo Invaders`, ao lado de `Gaming portugol`. São 1.055 arquivos, 331.808.858 bytes, conferidos individualmente por SHA-256:

- `01_EmUsoNoJogo`: frentes/metadados anteriores, verso/metadados e guia anterior.
- `02_ModelosEHistorico`: fontes, modelos, prompts, pranchas, geradores e guias históricos, movidos para fora do projeto ativo.

O manifesto registra cada localização original e no backup. Os arquivos do jogador, inimigos e outras tarefas não foram incluídos nessa limpeza. Os geradores antigos não ficam mais junto das ferramentas ativas para evitar restaurar coleções descartadas.

## Verificações

- Exportação e conferência de integridade das 25 cartas: passou.
- Validador do checkout Unity: passou, 1.884 GUIDs.
- Play Mode real: 94 verificações, sem erros, percorrendo e capturando todas as 25 cartas, pausa, reroll infinito, escolha, nova oferta e retorno ao menu.
- Nove capturas de ofertas em 1200×720 conferidas visualmente, cobrindo toda a coleção, além de 800×600 e 1920×1080. Cópias em `Docs/ArtPreviews/CardCollection_v21/InGame`.
- Build Windows na Unity 6000.6.4f1 passou com validação de cenas, prefabs, catálogo e áudio.
- Executável iniciou em headless durante oito segundos sem exceções de carregamento/runtime; encerrado o próprio processo de teste. Não constitui uma partida manual completa.
- Pacote verificado: `Builds/Pacotes/CrocodiloInvaders-DEV-Cartas-v21-Windows.zip`, 116.987.034 bytes, com checksum em `SHA256SUMS.txt`. Build executável em `Builds/Windows-DEV/CrocodiloInvaders.exe`.
