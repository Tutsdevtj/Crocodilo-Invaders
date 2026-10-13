# Loading da partida

`Jogar` cria o prefab `Assets/Prefabs/UI/GameplayLoading.prefab` sobre o menu.
O patrulheiro original aparece como uma silhueta branca, sobre fundo preto,
com os pontos de `LOADING...` animados e uma flutuação leve.
Não há imagens novas nem pacotes externos.

A entrada dura 0,12 segundo. A cena de gameplay é carregada com
`SceneManager.LoadSceneAsync`; sua ativação ocorre atrás da tela preta.
O estado pronto só aparece depois da ativação, dos primeiros frames de
inicialização e do carregamento dos clipes de áudio associados às fontes da cena.

Durante a preparação e a confirmação:

- `Time.timeScale` fica em zero e `AudioListener.pause` pausa o áudio;
- os comandos do player e o EventSystem da fase ficam desabilitados;
- o painel provisório `INICIAR` fica oculto;
- nenhuma onda começa antes da confirmação;
- o ENTER que selecionou `Jogar` precisa ser solto antes de uma nova confirmação.

Quando pronta, a tela pede **Pressione ENTER para jogar**. ENTER ou ENTER do
teclado numérico faz um fade de 0,45 segundo. Ao terminar, os estados de tempo,
áudio e comandos são restaurados, `WaveController.StartRun()` começa a primeira
onda e a tela persistente é destruída. Uma nova partida repete esse fluxo.

Uma fase sem WaveController mostra uma mensagem de erro e permite voltar ao
menu. Uma referência de cena inexistente é rejeitada antes de congelar o jogo.

## Responsabilidades

- `MainMenuController`: solicita o loading pelo prefab configurado e mantém Sair.
- `GameplayLoadingController`: troca de cena, bloqueio e restauração de controles,
  confirmação e início da partida.
- `GameplayLoadingView`: textos, reticências, flutuação e fades em tempo não escalado.
- `UISilhouette.shader` e `LoadingSilhouette.mat`: tornam branco o sprite existente,
  preservando sua transparência e sem alterar o PNG.
- `WavesUI.SuppressStartPanel`: oculta o botão provisório durante a transição.

Abrir PlayerMovementTest diretamente continua permitindo o início provisório;
a confirmação pelo loading pertence ao fluxo iniciado pelo MainMenu.

## Peso do projeto

A auditoria anterior a este loading encontrou 433 arquivos novos, incluindo
141 PNGs e 225 metadados. Cerca de 24 mil linhas eram `.meta`, 13 mil eram
prefabs serializados e 1.900 eram C# novo. As pastas Library, Temp, Logs e o
backup de cache estão ignorados pelo Git. O número de linhas de diff não mede
o custo de executar o jogo.

Foi ajustada a resolução máxima de importação de textos e logos dos menus para
1024 e dos diamantes para 512. Fundo, nuvens e moldura mantêm sua configuração.
Os PNGs, GUIDs, recortes e referências continuam os mesmos. Textos pequenos
não precisam carregar uma textura com mais de 2 mil pixels para aparecer na UI.

Na medição com `Profiler.GetRuntimeMemorySizeLong` em uma cópia de validação
na Unity, as texturas do MainMenu passaram de 134,1 para 71,9 MiB; as da pausa,
de 110,8 para 33,8 MiB. Isso reduz em aproximadamente 57% o consumo reportado
dessas texturas nesse ambiente. Não é uma medição de FPS nem do consumo total
de RAM de um build. Loading organiza a transição; quedas de FPS durante o combate
precisam ser analisadas separadamente no Profiler.

Referências técnicas:
- [Unity: LoadSceneAsync](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/scenemanagement/scenemanager/loadsceneasync)
- [Unity: allowSceneActivation](https://docs.unity.cn/6000.0/Documentation/ScriptReference/AsyncOperation-allowSceneActivation.html)
