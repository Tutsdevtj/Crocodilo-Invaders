# Opções de áudio

O botão **Opções** do MainMenu e o da pausa abrem o mesmo prefab:
`Assets/Prefabs/UI/AudioOptions.prefab`. O painel, as barras, o trilho e o diamante
usam cópias dos PNGs fornecidos pelo usuário, com filtro Point e limite 1024.
Os arquivos originais permanecem intactos. Os ícones e os textos do painel fornecido
são mantidos; as barras e os seletores desenhados nele são cobertos por trechos
do próprio fundo e substituídos por sliders interativos. Um fundo opaco impede
que o conteúdo do menu apareça através das áreas semitransparentes da arte.

Há três controles, de 0 a 100%:

- **Volume geral:** afeta música e efeitos juntos.
- **Efeitos sonoros:** tiros, impactos, HUD, botões, cura, avisos e revelações das cartas.
- **Músicas:** menu, gameplay e boss.

Os valores são aplicados imediatamente e são compartilhados entre as cenas. O padrão
é 100% nos três controles, preservando o equilíbrio dos volumes já definidos nas fontes.
O sistema usa `Assets/Resources/Audio/GameAudio.mixer`, com grupos Master, Effects e
Music e parâmetros expostos `MasterVolume`, `EffectsVolume` e `MusicVolume`.
Cada valor normalizado é convertido por `20 × log10(valor)`; zero usa o piso de
silêncio de -80 dB. O volume próprio de cada AudioSource, seus fades e os volumes
de PlayOneShot continuam funcionando sem serem sobrescritos pelas opções.

O botão e os sliders funcionam por mouse e controle. Arraste o diamante ou clique
no trilho. Setas cima/baixo alternam controles e o botão de voltar; esquerda/direita
ajustam um ponto percentual. O diamante selecionado cresce levemente e ganha brilho.
Ajustar efeitos ou geral toca uma amostra curta, respeitando os dois volumes;
mudar música usa a música já tocando, sem reiniciá-la.

Esc ou o botão de voltar fecha a tela. No MainMenu, devolve o foco ao botão Opções.
Na pausa, volta ao menu de pausa e mantém a simulação e a música paradas; um novo
Esc retoma o jogo. O preview de efeitos funciona na pausa. Abrir opções sobre a
pausa das cartas mantém também a escolha bloqueada; voltar dela retorna à escolha
congelada, sem liberar a próxima onda. O painel entra em 0,16 s e sai em 0,12 s,
com fade e escala, usando tempo não escalado.

## Responsabilidades e persistência

- `GameAudioSettings`: singleton iniciado automaticamente, estado dos três canais,
  mixer e persistência. Não depende de um objeto colocado manualmente na cena.
- `AudioOptionsView`: apresenta valores, recebe sliders, toca a amostra e anima a tela.
- `AudioSliderFeedback`: foco visual de cada seletor.
- `MainMenuController` e `PauseMenuView`: abrem a tela e restauram seus próprios menus.

Os valores são salvos em PlayerPrefs nas chaves `Crocodilo.Audio.Master.v1`,
`Crocodilo.Audio.Effects.v1` e `Crocodilo.Audio.Music.v1`. A escrita em disco acontece
ao fechar a tela, desativá-la, suspender o app ou encerrar o jogo, e apenas se houve
mudança. Não grava a cada movimento do mouse. Valores inválidos voltam ao padrão;
valores fora da faixa são limitados a 0–1.

A [documentação de AudioMixer.SetFloat](https://docs.unity3d.com/cn/current/ScriptReference/Audio.AudioMixer.SetFloat.html)
recomenda aplicar os parâmetros em Start ou depois. O serviço carrega os valores
em Awake e aplica o mixer em Start. A [documentação de PlayerPrefs.Save](https://docs.unity3d.com/ja/current/ScriptReference/PlayerPrefs.Save.html)
avisa que a gravação pode causar pequenas interrupções; por isso ela fica nos
pontos de saída da tela, e não durante o arraste.

Os AudioSources dos prefabs atuais são ligados ao mixer, incluindo player, inimigos,
lasers, boss, HUD, pausa e cartas. O `CrocodiloGame.cs` antigo também liga suas fontes
criadas dinamicamente, preservando sua lógica anterior. Para fontes novas, configure
Output no Inspector com Music ou Effects, ou use `GameAudioSettings.Route`.

## Validação

Teste em Play Mode: botão habilitado, entrada modal, foco, setas, arraste real do
mouse, limites 0/100, parâmetros independentes do mixer, preservação dos volumes
originais e persistência após recriar o serviço. Percorre MainMenu, loading, gameplay,
opções na pausa, duas etapas de Esc, pausa sobre cartas e retorno ao MainMenu.
Verifica também a ligação de todos os AudioSources atuais e dos dois canais do boss.
As preferências de teste usam outra identificação de aplicação para preservar os
valores reais do usuário. Capturas e logs ficam na pasta ignorada `Logs`; a compilação
do projeto completo é conferida separadamente.
