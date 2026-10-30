# Pássaros distantes v18

Pequenos bandos de três a cinco silhuetas atravessam o céu do lago lentamente, alternando direção e altura entre passagens. Há um intervalo de sete a onze segundos depois de um bando sair completamente, evitando acumular pássaros na tela.

As silhuetas usam cinco pontos com asas animadas e posições dos pontos ajustadas à grade de pixels da câmera. Envergadura aproximada de 10–14 pixels em 720p, cor azul/cinza discreta e alpha 0,62. Sorting Default/-225, depois das nuvens (-230) e antes da margem distante (-220), muito atrás dos personagens e das moscas. Não têm colisão, dano ou som.

Reserva fixa de cinco LineRenderers, compartilhando o material já existente do cenário. Não há novas texturas ou assets de pássaros detalhados; são silhuetas simples apropriadas à distância. Movimento e asas usam tempo escalado e congelam na pausa. Mudança de cena remove os objetos junto ao cenário.

`LakeDistantBirdsTools.Install` adiciona o componente ao LakeBackdrop de StageTwoLake e SapoDoPneuTest pelo Unity; o instalador completo também chama essa etapa. Capturas/gravação em `Docs/ArtPreviews/LagoDoPneu_Birds_v18`; observações de movimento/pausa e log em `Logs/LagoDoPneu/v18`.

Alteração local, sem commit, push ou atualização da PR. Build v17 anterior não inclui esta ambientação.
