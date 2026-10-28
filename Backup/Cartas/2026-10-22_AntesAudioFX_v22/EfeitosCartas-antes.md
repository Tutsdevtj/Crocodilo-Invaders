# Efeitos externos das cartas

Implementação de 21/10/2026 no prefab e fluxo reais da recompensa, em todas as
cinco famílias. As imagens aprovadas permanecem as mesmas. Foil e holografia
dentro da arte continuam para uma etapa posterior.

| Raridade | Revelação e presença |
| --- | --- |
| Comum | Estalo curto de pixels, sem aura permanente. |
| Rara | Halo azul discreto, faíscas e ponto de luz circulando o contorno. |
| Épica | Energia violeta segmentada, mais partículas e explosão maior. |
| Lendária | Raios dourados, estrelas, halo mais forte e impulso na entrada. |
| Mítica | Coroa carmesim, explosão maior, relâmpagos e impulso mais intenso. |

A intensidade inicial se dissipa; o contorno permanece animado. Passar o mouse
ou selecionar pelo teclado reforça a aura. Confirmar uma escolha dispara outro
impacto. A carta mais rara da oferta revela por último; as posições e os atalhos
1/2/3 continuam correspondendo às mesmas escolhas.

Os centros das cartas ficam separados por 350 unidades na referência 1200×720,
com vão de 100 entre faces de 250 (antes 50). Em proporções mais estreitas, o
conjunto se ajusta à largura disponível para preservar a margem dos efeitos.
O layout se adapta ao redimensionar a janela, sem recriar o prefab.

`CardRarityFeedback` mantém o relógio não escalado e congela explicitamente com
a pausa. `CardOptionView` usa esse mesmo relógio para impulso, tremor e respiração.
`CardRarityParticles` desenha uma malha reutilizada por carta, dimensionada pela
face real, sem objetos por partícula ou alvos de clique. Todos os efeitos são
limpos ao virar para o verso, rerolar, fechar a recompensa ou desativar o objeto.
Na v13, quatro stingers próprios passam a acompanhar as revelações da rara à
mítica. Veja [AjustesCartas_v13.md](AjustesCartas_v13.md).

## Validação repetível

`CardEffectsPlayValidation.Run` executa a cena PlayerMovementTest em Play Mode:
ofertas fixas das cinco raridades, pausa/retomada, rerolls, escolha, nova oferta
e retorno ao menu. Captura o jogo em `Logs/CardEffects`, incluindo três míticas
em 1200×720, 800×600 e 1920×1080. Precisa de renderização ativa, sem `-nographics`,
e processo em lote sem `-quit` (o validador encerra o Editor ao concluir).

O teste de dados de `Tools/Validation/CardDevelopment` complementa esse teste com
1464 verificações, incluindo 200 rerolls DEV e o limite normal de um reroll.

Resultado atual: 94 checks em Play Mode passaram com zero erros do jogo, incluindo
as 25 cartas no ciclo completo, os sons por raridade e a apresentação do verso. Capturas
conferidas nas três resoluções. O indexador Search do Editor 6000.6 pode registrar
uma exceção de inicialização em lote; o validador distingue essa falha do Editor
das exceções do jogo. O player não inclui esse indexador.

O teste encontrou e motivou duas correções de limpeza: `Player` respeita
componentes já destruídos na saída, e `CardRewardController.OnDisable` sempre
restaura o relógio, mesmo se a UI ou as ondas já foram destruídas.
