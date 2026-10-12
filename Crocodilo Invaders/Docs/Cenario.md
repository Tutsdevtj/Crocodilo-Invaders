# Nuvens e sol

`SkyEnvironment.prefab`, dentro de `Map` na cena `PlayerMovementTest`, reúne o sol e o gerador de nuvens. As artes são as originais do port, copiadas para `Art/Environment` e importadas como sprites. As texturas em `Resources/Art` e a infraestrutura do port permanecem intactas.

## Responsabilidades

- `ScrollingLayer`: move os elementos visuais. **Repeating Layer** continua repetindo o chão; **Single Pass** move uma instância e a remove somente depois que ela sai completamente da câmera.
- `ScreenObjectSpawner`: escolhe quando e onde instanciar os objetos, com altura, escala e velocidade aleatórias. A variação visual usa um gerador separado dos sorteios de gameplay.
- `ViewportAnchor`: mantém o alinhamento do elemento em relação à câmera. O modo **Viewport** e a opção **Anchor Horizontal** posicionam o sol em coordenadas proporcionais à tela, sem depender da resolução.
- `Animator`: reproduz os 29 frames do sol original, com 0,08 segundo por frame, em loop.

## Ajustes iniciais

No filho `Clouds` do prefab `SkyEnvironment`, o `ScreenObjectSpawner` usa:

- **Interval Range:** 3 a 5 segundos entre nuvens.
- **Viewport Height Range:** 0,60 a 0,93. Zero representa a parte inferior da câmera; um representa a parte superior.
- **Speed Range:** 0,8 a 1,3 unidades por segundo para a esquerda.
- **Scale Range:** 1,15 a 1,5.
- **Initial Visible Count:** duas nuvens já visíveis no início.
- **Initial Delay:** 0,8 segundo para a primeira nova nuvem entrar pela direita.
- **Max Alive:** seis nuvens. O limite conta os filhos do objeto gerador; esse objeto deve conter apenas as instâncias que gera.

O prefab `Cloud` usa o mesmo `ScrollingLayer` do chão, configurado como **Single Pass**. Seu sprite está em um filho `Visual`, referenciado em **Segments**. O componente também aceita velocidade negativa para atravessar da esquerda para a direita.

O prefab `AnimatedSun` usa posição de viewport `(0,50; 0,795)`: centralizado horizontalmente, na região superior. Sua escala inicial é 1,4. O fundo usa ordem -100, o sol -90, as nuvens -80 e o chão -50, mantendo os personagens e tiros à frente.

Nuvens, rolagem e animação do sol respeitam a pausa. Os elementos do céu não têm colliders nem participam do combate.
