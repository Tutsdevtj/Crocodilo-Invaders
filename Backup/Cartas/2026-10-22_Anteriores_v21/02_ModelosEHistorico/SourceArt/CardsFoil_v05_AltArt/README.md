# Artes alternativas por raridade — prévia v05

**Expressão atual:** por pedido seguinte do usuário, `art_4.png` mantém o arsenal
e agora mostra Croc com boca bem aberta, língua/dentes visíveis, olhos arregalados
e expressão de gargalhada/berro insano. Anterior de boca fechada preservada em
`art_4_multicannon_closed_mouth.png`. Conferência avulsa atual em
`Docs/ArtPreviews/CardsFoil_v05/mythic_multicannon_open_mouth.png`.

## Revisão exclusiva da mítica — múltiplos canhões

Por novo pedido do usuário, `art_4.png` agora traz um arsenal exagerado: canhão
central, gatling sobre o ombro, três canhões laterais e duas armas inferiores,
com reatores verdes, armadura dourada e cápsulas ejetadas. O rosto/capacete e
estilo do Croc permanecem reconhecíveis. Esta revisão substitui somente a imagem
da mítica; `art_1` a `art_3` permanecem idênticas por hash. A versão anterior está
preservada em `art_4_single_cannon_v01.png`. Imagem avulsa de conferência em
`Docs/ArtPreviews/CardsFoil_v05/mythic_multicannon.png`. Não houve mudança de
textos, valores, foil ou arquivos em Assets; a prévia interativa anterior não
foi recomposta nesta entrega de imagem isolada. A próxima exportação v05 usará
a nova imagem mítica.

O usuário pediu evolução da arte a partir da rara, mantendo identidade e composição:
mítica mais tunada, lendária um pouco menos, épica intermediária e rara discreta.
Esta revisão continua como prévia de dano de ataque. Não altera Assets nem gameplay.

`full_art_master.png` é a referência v04 preservada. `art_1.png` a `art_4.png`
são quatro edições independentes desse mesmo master, com preservação explícita do
rosto, capacete, expressão, pose, direção do canhão, enquadramento e cenário.
Novas imagens produzidas com o gerador de imagens; sem texto ou foil embutido.

- Comum: mantém a arte padrão v03.
- Rara: reforço simples no canhão com fenda ciano e ombreira com rebites.
- Épica: dois reforços, célula ciano, conduítes/aberturas violeta e arco elétrico curto.
- Lendária: armadura e arma com reforços dourados, reator verde e estabilizadores;
  disparo energizado mais forte, arte expandida.
- Mítica: canhão e ombreiras com placas pesadas e ouro, presas metálicas, dois
  reatores e conduítes verdes, capacete reforçado e descarga máxima; full art.

As mudanças são cosméticas na carta e não adicionam armas ou mecânicas ao jogo.
Face e expressão continuam as mesmas, sem metamorfose, nova pose ou outro personagem.
Rara/épica usam recorte aproximado ao da comum para manter a escala do Croc legível.
Mantém textos ajustados e a progressão foil v04. A prévia tem uma opção de desligar
o acabamento foil para comparar só as variantes de arte.

Exportar e verificar:

```powershell
node 'Crocodilo Invaders/Tools/Art/build_foil_preview_v05.cjs'
node 'Crocodilo Invaders/Tools/Art/verify_foil_preview_v05.cjs'
```

Prévia e prancha em `Docs/ArtPreviews/CardsFoil_v05`. Conferência visual e testes
de shader/reflexão, controle de foil e responsividade feitos em Edge headless.
A v03 permanece como a versão integrada no jogo; v04/v05 são estudos de acabamento
e variantes por raridade, aguardando a decisão visual do usuário.
