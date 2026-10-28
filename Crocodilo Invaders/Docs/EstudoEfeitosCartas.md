# Estudo: efeitos externos das cartas — 21/10/2026

Estudo original preservado. Os efeitos externos foram implementados posteriormente
em 21/10/2026; veja [EfeitosCartas.md](EfeitosCartas.md) para o comportamento atual.
O foil/holográfico interno é uma etapa posterior e permanece separado deste estudo.

## Objetivo visual

A raridade deve ser reconhecível pela cor e pelo movimento ao redor da carta,
antes mesmo de ler o nome. Manter identidade arcade/pixel art militar. Poucas
formas legíveis, contraste controlado, rosto e textos sempre livres. A raridade
define o acabamento nas cinco famílias; o tipo de bônus continua comunicado pela arte.

| Raridade | Enquanto aguarda escolha | Na revelação |
| --- | --- | --- |
| Comum | Sombra curta, sem aura contínua | Pequeno impacto seco e poucos pixels neutros que somem |
| Rara | Contorno azul fino; um ponto de luz percorre o aro lentamente | Dois pulsos azuis curtos pelos cantos; faíscas pontuais |
| Épica | Energia violeta aderida ao contorno; pequenos segmentos intermitentes nos cantos | Descarga violeta curta que se divide ao longo das bordas |
| Lendária | Halo dourado mais definido, com poucas brasas ascendentes | Explosão dourada compacta, seguida de fragmentos de luz que desaceleram |
| Mítica | Coroa de energia carmesim, pulsação irregular e poucas fagulhas; contorno com maior presença | Compressão de luz junto à moldura, estalo vermelho/marfim, dois arcos elétricos curtos e dissipação rápida |

“Mais top” vem de movimento, contraste, ritmo e som, sem manter uma tempestade
de partículas. O pico mítico acontece na revelação; em repouso deve permitir
comparar três cartas e ler os percentuais confortavelmente. Evitar flashes na
tela inteira, névoa cobrindo a carta, raios constantes ou mistura de todas as cores.

No foco por mouse/controle, o contorno ganha definição e a energia se aproxima
da borda; manter o movimento de elevação existente. Ao escolher, breve convergência
de partículas para a carta selecionada durante a animação atual. Encerrar efeitos
das descartadas ao sair. Reroll reinicia os estados, sem empilhar partículas ou som.

## O que já existe no projeto

- `CardOptionView`: foco por mouse/controle, escala/elevação, contorno da raridade,
  flash de revelação e pulsação de escala para lendárias/míticas.
- `CardRarityFeedback`: aura só desde épica, pulso, explosão inicial, ligação com
  partículas/áudio e raios laterais apenas na revelação mítica.
- `CardRarityParticles`: um `MaskableGraphic`, sem GameObject por partícula;
  12/18/24 partículas persistentes por épica/lendária/mítica e 10/14/18 no impulso
  inicial. A trajetória atual é praticamente a mesma, mudando a intensidade.
- `CardRarityAudio`: sons separados de revelação épica/lendária/mítica.
- `CardRewards.prefab`: referências já ligadas para os três slots. Gráficos dos
  efeitos com Raycast Target desligado; áreas externas chegam a 420×550.
- Projeto Built-in, Canvas/uGUI. Não há necessidade demonstrada de migrar para URP,
  adicionar câmera 3D ou instalar plugin de partículas UI para estes efeitos.

Os scripts e as referências do prefab foram inspecionados. Aparência e desempenho
atuais não foram medidos em Play Mode nesta máquina.

## Caminho técnico proposto

1. Evoluir `CardRarityFeedback` e o desenho de `CardRarityParticles`, aproveitando
   o vínculo existente com Bind/Reveal/Hide, escolha e pausa. Evitar um segundo
   sistema paralelo. Parametrizar cor, forma de trajetória, quantidade, largura,
   velocidade e duração por raridade.
2. Tratar repouso, revelação, foco e escolha como intensidades do mesmo efeito.
   Pausa explícita continua interrompendo animação/som; a recompensa usa tempo
   não escalado pois a partida fica congelada durante a escolha.
3. Gerar trajetórias em função do retângulo real da carta. Hoje há coordenadas
   fixas como ±132 e y=-218…228, que precisam ser substituídas por medidas relativas
   ao contorno para funcionar em diferentes resoluções, escalas e proporções.
   Conferir proporção antes: arte v12 é 360×536; o Face do prefab tem área 250×402.
   Preserve Aspect está desligado no prefab inspecionado. Corrigir ou deliberar
   o enquadramento antes de alinhar os efeitos à área efetivamente desenhada.
4. Partículas atrás da carta e próximas ao perímetro; reflexos sobre a moldura
   somente por máscara. Manter placas de texto e comparação de atributos livres.
   Gráficos decorativos não interceptam cliques. Efeito acompanha a mesma hierarquia
   de movimento/hover; desligar na face de verso e no fechamento.
5. Começar com as malhas de UI existentes e pequenas texturas/frames pixelados
   compartilhados quando necessários. Se a atualização das partículas afetar o
   Canvas inteiro, avaliar um sub-Canvas para os elementos animados após medir.
   Não acrescentar múltiplos canvases/materiais por carta sem evidência de ganho.
6. Mais adiante, integrar foil/holografia com as máscaras separadas já aprovadas:
   arte, moldura, Croc e textos. Efeitos externos devem continuar coerentes com
   a paleta mesmo com foil desligado.

## Validação no modo DEV

- Raridade fixa: três épicas, três lendárias e três míticas simultâneas.
- Ciclo25: mesmas regras visuais em todos os tipos, inclusive cortes míticos.
- Foco por mouse e gamepad, virada, reroll, escolha, pausa/retomada e saída ao menu.
- Repetir ofertas para detectar objetos, materiais, partículas ou áudios acumulados.
- Conferir 1200×720, 1920×1080 e 800×600: sem sobreposição nos textos, percentuais,
  cartas vizinhas ou botões. Conferir cliques junto à borda.
- Medir CPU de reconstrução do Canvas, lotes, memória/GC e transparências sobrepostas
  com o Profiler/Frame Debugger. Definir orçamento com base no custo observado,
  sem prometer ganho de desempenho só pela quantidade de partículas.
- Próxima melhoria útil no painel DEV: ligar/desligar apenas os efeitos externos
  e reproduzir a revelação, para comparar leitura e intensidade no cenário real.

## Ordem sugerida

Primeiro prototipar contorno/partículas externos da rara, épica e mítica para
definir os extremos de intensidade; completar comum/lendária e validar as três
cartas simultâneas. Só então trabalhar foil/holografia interna.

## Referências técnicas

- [Unity: otimização de UI](https://unity.com/how-to/unity-ui-optimization-tips)
  — canvases separados conforme frequência de atualização, elementos decorativos
  sem Raycast Target e cuidados com reconstrução de malhas.
- [Unity 6: reduzir trabalho de renderização](https://docs.unity3d.com/6000.0/Documentation/Manual/OptimizingGraphicsPerformance.html)
  — medir custo CPU/GPU e transparências sobrepostas.

Proposta artística autoral baseada na coleção aprovada; as referências acima
fundamentam os cuidados técnicos, não a direção estética.
