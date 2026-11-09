# Lago do Pneu: dificuldade v33

Pedido do usuário: a fase estava fácil demais e alguns ataques não faziam sentido. Os valores abaixo ainda dependem de teste jogando; as validações conferem comportamento, não o equilíbrio.

## Sapeneu Cururu

| Item | Antes | Agora |
| --- | --- | --- |
| Vida | 200 | 320 (fúria continua em 50%) |
| Salto | Pouso escolhido 2,35 s antes de cair | Pouso segue a nave a até 4,5 u/s durante o aviso e a subida, e trava em 55% do voo |
| Língua | Mosca parada no mapa; linha com raio 0,13 | Moscas rondam a nave e a escolhida voa para além dela, então a linha passa pela nave; raio 0,28 |
| Arroto | Ficava exposto na hora | 0,6 s de preparação, leque de 7 gotas (9 na fúria) e só então 1,4 s de dano dobrado |
| Pneu | Linha reta, some na água | Arco que passa pela mira, trava 0,3 s antes do arremesso e quica 2 vezes na água (3 na fúria) |
| Gotas do salto | 3 baixas + 2 altas | Baixas sobem mais, altas alcançam o topo e a esquerda, e uma gota extra mira a nave (6 no normal, 8 na fúria) |
| Pouso | Do meio para a direita | Mantido, por decisão anterior do usuário |

## Fúria e Saperereca

- O descanso real depois de cada golpe cai 40%. Antes, o cálculo anulava o bônus.
- O casal ataca ao mesmo tempo: Sapeneu não espera a parceira, e as gotas no ar não seguram o próximo golpe durante a fúria.
- A mira da Saperereca é calculada 0,3 s antes da rajada, com antecipação de 0,45 s do movimento da nave, e o intervalo entre rajadas caiu de 2,2 s para 1,6 s.

## Hordas

- Inimigos por onda: 10, 13, 16, 18, 20, 22, 24, 27, 30 (antes 6 a 18), mais 0 a 2 mosquitos conforme a composição sorteada.
- Até 9 vivos, com spawn mais rápido.
- A partir da onda 4, até dois ataques simultâneos e 0,85 s entre liberações. Na onda 4 em diante as gotas em voo não seguram o próximo ataque.
- `LakeJourneyTools.ApplyWaves` regrava somente as composições de `StageTwoLake`. A reserva de folhas tem 9 vagas.

## Validação

No Unity 6000.6.4f1: dificuldade 44/0, pneu 30/0, chefe v26 25/0, travessia 10/0, campanha completa 75/0, limpeza da cena 24/0, combate das hordas 11/0, entrada do chefe 22/0, ações do brejo 22/0 e piranhas 10/0.

## Ajustar sem recompilar

Todos os números acima ficam em `Assets/Data/Lake/LakeDifficulty.asset`, separados por seção no Inspector. A vida do boss continua no componente Health do prefab `SapoDoPneu` e as hordas continuam em `wavePools` da cena `StageTwoLake`.

A arte do lago saiu de `Resources` para `Assets/Art/LakeRuntime` e é referenciada por `Assets/Data/Lake/LakeStageArt.asset`, ligado ao `LakeBackdrop` das duas cenas por `LakeStageDataTools.Install`.
