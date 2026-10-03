# Crocodilo Invaders — Unity / WebGL

**[Jogar online](https://crocodilo-invaders.netlify.app/)** · [Projeto no Netlify](https://app.netlify.com/projects/crocodilo-invaders/overview)

Port jogável do arquivo `Gaming portugol/Crocodilo Invaders.por`. O projeto Portugol continua na pasta original. As imagens e os sons foram copiados para `Assets/Resources`; os GIFs foram convertidos em sequências PNG para a Unity.

## Abrir e jogar

1. No Unity Hub, escolha **Add project from disk** e selecione esta pasta `unity/`.
2. Abra com **Unity 2022.3.62f1**, a versão indicada em `ProjectSettings/ProjectVersion.txt`, e instale o módulo WebGL Build Support para essa mesma versão.
3. Aguarde a importação. A cena `Assets/Scenes/JogoEditavel.unity` acompanha o projeto, com personagens, cenário, colliders, prefabs, menu e HUD editáveis; o script de editor configura o Build Settings.
4. Abra `Assets/Scenes/JogoEditavel.unity` (ou use **Crocodilo > Preparar cena**) e pressione **Play**.

Para alterar o jogo na Scene/Inspector, veja **[GUIA_EDICAO.md](GUIA_EDICAO.md)**. Use **Crocodilo > Prévia no editor** para mostrar a fase, o menu ou o chefe sem Play. O gerador de cena só é executado explicitamente quando o arquivo está ausente; entrar em Play não monta a hierarquia.

Se o projeto não aceitar `Input.GetKey`, em **Edit > Project Settings > Player > Active Input Handling** selecione **Both** ou **Input Manager (Old)**.

## Gerar WebGL

Instale **WebGL Build Support** para a versão de Unity usada no Hub. No editor, clique em **Crocodilo > Build WebGL**. A saída será `unity/Builds/WebGL/`, já com `index.html`, `style.css`, `app.js` e a pasta `Build/`. A Unity pode solicitar a troca de plataforma e recompilar os assets antes do build. Para testar, use **Build And Run** no Build Settings ou sirva a pasta gerada em um servidor HTTP local; abrir `index.html` diretamente via `file://` não executa corretamente o WebGL.

O template da página está em `Assets/WebGLTemplates/Crocodilo/`. Edite esses arquivos para mudar o site e gere o WebGL novamente. **Não copie os arquivos gerados para dentro de `WebGLTemplates`**: a Unity já usa esse template para montar a página final em `Builds/WebGL/`. Para publicar, envie **todo o conteúdo** dessa pasta de saída, sem separar `index.html` da pasta `Build/`, para uma hospedagem estática compatível com WebGL. O projeto ativa o fallback de descompressão para facilitar a publicação em serviços estáticos sem configurar os cabeçalhos de compressão.

Para testar no próprio computador, use **Build And Run** no editor ou abra um terminal dentro de `Builds/WebGL/`, execute `py -m http.server 8000` (se tiver Python) e acesse `http://localhost:8000/`. Abrir `index.html` com duplo clique usa `file://` e geralmente impede a Unity de carregar os arquivos do jogo.

### Atualizar no Netlify

1. Salve a cena editada e gere o jogo por **Crocodilo > Build WebGL**.
2. Abra o projeto do jogo no Netlify, em **Deploys** ou **Production deploys**.
3. Envie a pasta **Builds/WebGL** para a área de upload. Também pode compactar **o conteúdo dessa pasta** em um ZIP: `index.html` deve ficar na raiz do ZIP, ao lado de `app.js`, `style.css` e `Build/`.
4. Aguarde o deploy ficar publicado e abra o endereço do projeto.

Essa publicação envia o build compilado, conforme o [fluxo de upload do Netlify](https://docs.netlify.com/start/quickstarts/netlify-drop-quickstart/). Fazer commit/push dos fontes não atualiza esse deploy manual: depois de editar o jogo, gere e envie um novo WebGL. Não envie a pasta do projeto Unity inteira. Os arquivos `.unityweb` usam o fallback de descompressão da Unity; não configure `Content-Encoding: gzip` para eles.

## Controles

- **↑ ↓ ← →**: mover
- **Espaço**: tiro normal; cada tiro carrega o especial
- **C**: tiro especial com a barra cheia
- **Enter**: selecionar opção ou voltar ao menu
- **Esc**: pausar/continuar a partida ou sair da tela Sobre
- **Mouse**: selecionar opções, continuar a partida e voltar ao menu

Ao eliminar 15 inimigos, aparece o Executor V-9. Caixas de vida recuperam até 6 pontos. O menu usa o título e os botões originais de `mainMenu.png`, recortados no Canvas, e o fundo existente `background.png`. Tem apenas **Jogar** e **Sobre**; a opção **Sair** foi removida da exibição, do teclado e do clique. Durante a partida, **Esc** abre **Continuar** e **Voltar ao menu**. A pausa congela a simulação, animações e áudio; continuar mantém a partida e voltar ao menu permite começar outra.

## Organização

- `Assets/Scripts/CrocodiloGame.cs`: simulação, controles, colisões e progressão.
- `Assets/Scripts/CrocodiloSceneView.cs`: referências aos sprites, Canvas, prefabs e áudio salvos na cena.
- `Assets/Scripts/CrocodiloVisual.cs`: animação e dados editáveis de cada personagem/projétil.
- `Assets/Scenes/JogoEditavel.unity`: cena editável e incluída no build.
- `Assets/Prefabs/`: modelos dos personagens, tiros, lasers e efeitos.
- `Assets/Editor/`: importação de texturas, configuração do projeto e build WebGL.
- `Assets/WebGLTemplates/Crocodilo/`: página HTML, CSS e JS do build WebGL.
- `Assets/Resources/Art` e `Audio`: cópia dos recursos originais.
- `Tools/importar_assets.py`: recria a cópia dos recursos a partir do Portugol (requer Pillow).

O build, a pasta `Library` e os arquivos locais do editor ficam fora do Git. O jogo usa SpriteRenderer/BoxCollider2D e Canvas com Text/Image/Button, com uma área de referência de 1200×720 e 100 pixels por unidade da Unity. Os personagens mantêm a proporção, enquanto o fundo e o chão cobrem toda a tela, inclusive Full HD. Os controles funcionam no editor e no WebGL. O jogo original não tinha uma tela de derrota; esta versão mostra pontuação e permite retornar ao menu.

## Tempo, colisões e projéteis

O laço original termina com `u.aguarde(3)`. Na implementação do [Portugol Studio](https://github.com/UNIVALI-LITE/Portugol-Studio/blob/master/core/src/main/java/br/univali/portugol/nucleo/bibliotecas/Util.java), isso chama `Thread.sleep(3)`; o gerador transforma `enquanto` em um `while` Java. A taxa real do original também depende do custo de renderização e do agendamento do sistema operacional. O port usa o intervalo nominal de **3 ms por passo**, acumulando o tempo entre frames da Unity: movimento e contadores não dependem de renderizar a 30, 60 ou 144 FPS. A taxa nominal de movimento do jogador é aproximadamente 333 pixels por segundo, em vez dos antigos 60. As animações continuam usando segundos.

A colisão das naves usa BoxCollider2D ajustados aos limites visíveis da animação `enemy1`, sem o deslocamento antigo de `-37` no eixo Y. Editar esses colliders e a escala do objeto altera a colisão utilizada pela simulação. O especial também colide pela parte visível do efeito. Os tiros inimigos guardam suas próprias coordenadas e continuam avançando e causando dano após a morte/respawn do atirador ou a chegada do chefe; uma nova partida remove os tiros anteriores.

O laser roxo do chefe avisa por **1,5 segundo** antes de disparar e permanece visível por **1,2 segundo**. Esses tempos usam segundos, independentemente do passo de simulação. Os feixes horizontais e verticais cobrem toda a área visível da tela, com o aviso e a faixa de dano no mesmo centro do efeito. Os textos do HUD e da pausa usam fontes brancas. As letras desenhadas nos botões originais foram preservadas junto com a arte.

Para verificar essas correções com a Unity instalada, execute `Tools/verificar_regressoes.ps1` pelo PowerShell. Se necessário, passe `-EditorPath` com o caminho de `Unity.exe`. O script usa uma cópia temporária, entra em Play Mode e testa velocidades em diferentes FPS, colisões, vida dos projéteis, cobertura do fundo, tempo/alinhamento dos lasers e as duas opções do menu. Os casos estão em `Tests/Editor/CrocodiloRegressionChecks.cs`.
