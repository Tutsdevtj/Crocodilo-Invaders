# Macaquini — movimento natural v11

Esta revisão trata da fluidez do voo e do cipó. A transformação, a roupa e o rosto ficam para os próximos pontos solicitados pelo usuário.

O voo usa 32 desenhos completos, contra 16 anteriormente. O cipó usa 24 desenhos selecionados, contra 16. Os intermediários mantêm as pernas relaxadas, com pequenos ajustes por inércia; o corpo não faz um ciclo de caminhada ou pedalada no ar. Os dois ciclos são reproduzidos em ida e volta a 20 quadros por segundo, com movimento físico e inclinação contínuos entre as imagens.

A versão anterior calculava o índice como `tempo × taxa`, alterando a taxa de 10 para 14 durante arrancadas. Essa troca podia saltar de uma pose para outra sem percorrer os intermediários. A v11 acumula uma fase independente da velocidade. Disparar também não troca mais o corpo inteiro para os oito desenhos antigos de ataque; o movimento segue contínuo, acompanhado de recuo suavizado, flash da arma e som.

A oscilação do corpo foi reduzida e a inclinação tem amortecimento maior. A continuação do cipó usa o ponto superior medido de cada desenho, e os jatos foram reposicionados nos bocais das novas imagens. HUD, ataques, vida das duas formas, pontuação e regras da campanha continuam nos controladores existentes.

## Arte e medição

Quatro folhas de 16 poses foram produzidas a partir dos sprites aprovados do projeto. A seleção usa 56 imagens: 32 de voo e 24 no cipó. Quatro desenhos de fonte com os pés cortados na borda inferior foram rejeitados; outras posturas que desviavam demais do repouso não entram na sequência do cipó.

Os PNGs são registrados pela parte inferior da casca e ordenados por proximidade dos pontos dos pés. Cada imagem continua sendo um personagem completo, sem montagem de membros separados. Exportação em resolução nativa, sem alteração da paleta RGB, com transparência binária, filtro Point e sem compressão. O cipó usa margem transparente adicional de 512 px para conservar a figura inteira.

Como métrica auxiliar, os centros das regiões de pele dos dois pés são comparados entre desenhos vizinhos. A média combinada caiu de 15,55 para 0,88 px no voo e de 12,37 para 1,81 px no cipó. A maior diferença selecionada é 2,02 px no voo e 3,44 px no cipó. Esses números medem a troca entre sprites; o deslocamento do boss pela arena é separado.

Fontes, prompts e manifesto: `SourceArt/Macaquini_v11`. Exportação reproduzível: `Tools/Art/prepare_macaquini_motion_v11.cjs`. Instalação: `MacaquiniMotionV11Tools.Install`. Teste nativo: `MacaquiniJetpackValidation.Run`, que identifica a revisão natural e grava evidências em `Docs/ArtPreviews/Macaquini_v11`. Build: `MacaquiniMotionV11Tools.Build`.

## Teste direto

Cena existente `Assets/Scenes/Tests/MacaquiniJetpackTest.unity`. O prefab real da campanha também foi atualizado.

F1/F2/F3 forçam os ataques da forma atual; J retira 45 HP; K esgota a forma; I alterna proteção; H cura o jogador; R reinicia; ESC pausa. O teste usa progresso isolado da campanha principal e cancela o envio ao ranking.
