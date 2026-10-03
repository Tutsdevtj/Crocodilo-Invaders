# Editar o jogo na Unity

Abra **Assets/Scenes/JogoEditavel.unity** no Project. A cena contém os objetos antes de apertar Play. Ela é a cena usada no build WebGL. O arquivo antigo `Jogo.unity` é a cena do port anterior e não deve ser usado para editar ou gerar o jogo novo.

## Ver e selecionar os objetos

1. Saia do Play Mode e abra a aba **Scene** em modo **2D**.
2. Use **Crocodilo > Prévia no editor > Fase**, **Menu** ou **Chefe**. A prévia não muda a tela inicial da partida.
3. Na **Hierarchy**, expanda **Crocodilo Invaders — Configuração**. Selecione um objeto e aperte **F** para enquadrá-lo na Scene.
4. Altere o **Inspector** e salve a cena com **Ctrl+S**. Alterações feitas durante Play são descartadas quando você para o jogo.

## O que editar

| Mudança | Onde fica |
| --- | --- |
| Posição inicial do jogador | Fase > Personagens > Jogador: Transform |
| Velocidade e vida do jogador | Jogador: CrocodiloVisual > Move Speed / Max Health |
| Imagem estática do jogador | Jogador: SpriteRenderer > Sprite / Color |
| Tamanho e posição da colisão | Personagem ou projétil: BoxCollider2D > Edit Collider, Size e Offset |
| Posição de combate dos inimigos | Fase > Personagens > Inimigo 1 / 2: Transform |
| Local de nascimento dos inimigos e chefe | Fase > Pontos de nascimento > Spawn ...: Transform |
| Vida, velocidade e altura aleatória de respawn | Inimigo: CrocodiloVisual > Max Health, Move Speed e Respawn Y |
| Frames e ritmo das animações | CrocodiloVisual > Frames / Frame Seconds |
| Fundo, chão, nuvens e sol | Fase > Cenário: SpriteRenderer e Transform |
| Nome do jogador e chefe | Canvas — Menu e HUD > HUD > Layout 1200 x 720: textos Nome jogador / HUD do chefe > Nome chefe |
| Fonte, cor, tamanho e posição do HUD | Objetos Text/Image: Inspector e RectTransform |
| Prefixos “Pontos” e “Inimigos” | CrocodiloSceneView > Points Prefix / Kills Prefix |
| Botões do menu | Canvas — Menu e HUD > Menu > Layout 1200 x 720 > Jogar / Sobre |
| Botões e textos da pausa | Canvas — Menu e HUD > Pausa > Layout 1200 x 720 |
| Aviso, duração, intervalo e faixas do laser | Objeto Configuração: CrocodiloGame > Laser do chefe |
| Espessura do efeito e da faixa de dano | Fase > Lasers do chefe: CrocodiloLaserView > Visual Thickness / Damage Thickness |
| Quantos inimigos antes do chefe, cura e limites de movimento | Objeto Configuração: CrocodiloGame > Partida |
| Música, sons e volume | CrocodiloSceneView > Audio Clips; objetos Música / Efeitos sonoros > AudioSource |

**Move Speed é medido em pixels por segundo.** A arte usa **100 pixels por unidade da Unity**: X = 1 equivale a 100 pixels, Y = -3,19 equivale a 319 pixels abaixo do início da fase. O eixo Y da Unity cresce para cima.

Os objetos animados usam a lista **Frames**; mude os frames para trocar a animação. Objetos estáticos usam o sprite definido no **SpriteRenderer**. A colisão considera o BoxCollider2D, a escala e a posição do objeto. Ela é calculada pela simulação do jogo, sem depender de Rigidbody2D ou de colisões físicas.

O **Transform** dos inimigos define a posição de combate. O campo **Spawn Point** aponta para outro objeto que define onde entram na tela. O inimigo 1 sobe até a posição de combate; o inimigo 2 mantém a altura de nascimento/respawn. O chefe também entra pelo seu marcador até a posição salva na cena.

O fundo ajusta o tamanho para preencher a tela e a câmera enquadra a área de jogo automaticamente. Se quiser enquadrar manualmente, desative **Fit Camera To Screen** e/ou **Fill Screen With Background** no CrocodiloSceneView. O chão é um SpriteRenderer em modo **Tiled**, com largura ajustada durante o scroll.

## Prefabs

Os arquivos em **Assets/Prefabs** são modelos editáveis do jogador, inimigo, chefe, tiros, mísseis, lasers e efeitos. Dê duplo clique no prefab para editá-lo. Alterações nele chegam às instâncias que não tenham overrides desse campo; alterações feitas em uma instância da cena valem para aquela instância.

Tiros inimigos e efeitos aparecem durante a partida a partir desses prefabs. Ficam em **Fase > Projéteis > Em voo e efeitos**, separados dos inimigos; a morte do atirador mantém o tiro em voo.

## Jogar e gerar o WebGL

- Pressione **Play** para testar. Para entrar direto na fase durante seus ajustes, marque **Start In Gameplay** no CrocodiloGame. Desmarque para iniciar pelo menu.
- Salve a cena e use **Crocodilo > Build WebGL**. O build usa a cena salva, os componentes e os prefabs editados.
- Teste a saída com **Build And Run** ou um servidor HTTP. Abrir `index.html` como `file://` não carrega o WebGL corretamente.

**Crocodilo > Criar ou abrir cena editável** abre a cena existente. O gerador só cria a estrutura quando o arquivo está ausente; ele não reconstrói o cenário ao apertar Play nem sobrescreve seus ajustes ao abrir o projeto.

O menu reaproveita os desenhos de título, Jogar e Sobre em **mainMenu.png**, por recortes **RawImage > UV Rect**. O material **Arte menu original** deixa o céu desses recortes transparente para mostrar **background.png**, sem modificar os arquivos de imagem. O recorte de Sair não é usado. Mova os botões pelo RectTransform; seus eventos Button > On Click estão salvos na cena.

Durante a partida, **Esc** abre a pausa com **Continuar** e **Voltar ao menu**. Use **Crocodilo > Prévia no editor > Pausa** para editar esse Canvas sem Play. A simulação para enquanto a pausa está aberta, inclusive projéteis, lasers, animações e áudio; Esc/Enter ou Continuar retomam a mesma partida.
