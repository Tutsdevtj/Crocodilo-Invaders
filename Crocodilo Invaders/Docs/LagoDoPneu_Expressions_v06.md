# Sapo do Pneu — piscadas e respiração

O sapo pisca por 0,13 segundo em intervalos de 3,25 a 4,1 segundos, com uma piscada dupla ocasional. O rosto dentro do pneu na HUD acompanha a mesma piscada. Uma respiração discreta varia a largura em 1,2% e a altura em 2,4% durante a flutuação; preparação, salto e impacto mantêm suas poses próprias.

As duas expressões de olhos fechados foram editadas com image_gen a partir das masters aprovadas do boss e da HUD. Os originais e os prompts estão em `SourceArt/LagoDoPneu_Expressions_v06`. Somente os recortes dos olhos são exibidos pelo Unity: corpo, pneu e ambiente não trocam de desenho durante a piscada. A pequena diferença de largura da master da HUD é compensada por coordenadas normalizadas. Nenhum PNG foi alterado depois da geração.

`LakeFrogExpressions` controla o relógio e os olhos do boss. `LakeBossBar` acompanha o mesmo estado no retrato. Os recortes seguem o movimento e a cor do sprite; não modificam colisores, trajetórias ou janelas de ataque. Pausa e término da luta congelam ou encerram a expressão.

Prévia real em `ArtPreviews/LagoDoPneu_Expressions_v06`: `hud-normal.png`, `gameplay.png`, `blink.png` e `lago-expressions-v06.mp4`.

Validação pelo menu real: **53 verificações, zero erros**, incluindo repetição das piscadas, fechamento/reabertura sincronizados, pausa com os olhos fechados, respiração e preservação do sprite base. Regressão DEV dos demais bosses após a integração da HUD: 44 verificações sem erros.

Build Windows atual: `Builds/Windows-LagoDoPneu-Expressions-v06/CrocodiloInvaders.exe`.

Build Windows concluída pelo Unity e inicialização de 10 segundos sem exceções. Pacote: `Builds/Pacotes/CrocodiloInvaders-LagoDoPneu-Expressions-v06-Windows.zip`.
