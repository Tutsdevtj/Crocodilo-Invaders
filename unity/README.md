# Crocodilo Invaders — Unity / WebGL

Port jogável do arquivo `Gaming portugol/Crocodilo Invaders.por`. O projeto Portugol permanece intacto na pasta original. As imagens e os sons foram copiados para `Assets/Resources`; os GIFs foram convertidos em sequências PNG para a Unity.

## Abrir e jogar

1. No Unity Hub, escolha **Add project from disk** e selecione esta pasta `unity/`.
2. Abra com **Unity 2022.3 LTS** ou versão posterior. Se o Hub pedir atualização do projeto, aceite.
3. Aguarde a importação. O script de editor cria `Assets/Scenes/Jogo.unity` e configura a cena no Build Settings automaticamente.
4. Abra `Assets/Scenes/Jogo.unity` (ou use **Crocodilo > Preparar cena**) e pressione **Play**.

Se o projeto não aceitar `Input.GetKey`, em **Edit > Project Settings > Player > Active Input Handling** selecione **Both** ou **Input Manager (Old)**.

## Gerar WebGL

Instale **WebGL Build Support** para a versão de Unity usada no Hub. No editor, clique em **Crocodilo > Build WebGL**. A saída será `unity/Builds/WebGL/`, já com `index.html`, `style.css`, `app.js` e a pasta `Build/`. A Unity pode solicitar a troca de plataforma e recompilar os assets antes do build. Para testar, use **Build And Run** no Build Settings ou sirva a pasta gerada em um servidor HTTP local; abrir `index.html` diretamente via `file://` não executa corretamente o WebGL.

O template da página está em `Assets/WebGLTemplates/Crocodilo/`. Edite esses arquivos para mudar o site e gere o WebGL novamente. Para publicar, envie **todo o conteúdo** de `Builds/WebGL/` para uma hospedagem estática compatível com os arquivos WebGL da Unity. O projeto ativa o fallback de descompressão para facilitar a publicação em serviços estáticos sem configurar os cabeçalhos de compressão. O site é a página que a Unity gera no build; não é preciso programar outro carregador em HTML/JS.

## Controles

- **↑ ↓ ← →**: mover
- **Espaço**: tiro normal; cada tiro carrega o especial
- **C**: tiro especial com a barra cheia
- **Enter**: selecionar opção ou voltar ao menu
- **Esc**: voltar ao menu durante o jogo ou sair da tela Sobre
- **Mouse**: selecionar opção do menu e voltar das telas finais

Ao eliminar 15 inimigos, aparece o Executor V-9. Caixas de vida recuperam até 6 pontos. No navegador, a opção **Sair** não fecha a aba por restrição do WebGL; use o menu do navegador para sair.

## Organização

- `Assets/Scripts/CrocodiloGame.cs`: jogo, menus, HUD, colisões e áudio.
- `Assets/Editor/`: importação de texturas, criação da cena e build WebGL.
- `Assets/WebGLTemplates/Crocodilo/`: página HTML, CSS e JS do build WebGL.
- `Assets/Resources/Art` e `Audio`: cópia dos recursos originais.
- `Tools/importar_assets.py`: recria a cópia dos recursos a partir do Portugol (requer Pillow).

O build, a pasta `Library` e os arquivos locais do editor ficam fora do Git. O código usa a GUI 2D da Unity com escala para a proporção 1200×720; assim a arte original mantém as posições e os controles funcionam no editor e no WebGL. O port ajusta o ciclo de atualização para 60 passos por segundo e corrige estados de projéteis que causavam colisões sem disparo. O jogo original não tinha uma tela de derrota; esta versão mostra pontuação e permite retornar ao menu.
