# Macaquini — vozes com eco de selva v26

**Estado atual:** experimento cancelado pelo usuário. Este documento registra o teste histórico; as vozes e a integração foram retiradas do runtime. Os arquivos rejeitados estão em `SourceArt/MacaquiniAudioRejected`. O jogo usa o áudio anterior restaurado.

Vocalizações de macaco cartunescas integradas aos eventos nativos da transformação, preservando os sons de rasgo. As três edições finais usam como fonte **Monkey Sounds**, por **AntumDeluge**, no [OpenGameArt](https://opengameart.org/content/monkey-sounds), sob CC0 1.0. São imitações vocais humanas, conforme descrição do autor. Originais, licença e parâmetros de edição estão em `SourceArt/MacaquiniJungleVoice_v26`.

| Momento da transformação | Chamada | Ganho local |
| --- | --- | --- |
| Início | Esforço/chamada curta | 0,44 |
| 1,75 s — retirada do topo | Segunda chamada | 0,32 |
| 3,25 s — esforço para rasgar | Chamada mais grave | 0,48 |
| 5,5 s — exibição de força | Grito final com cauda | 0,62 |

Tratamento: corte, remoção de DC, filtragem, pitch moderadamente mais grave e reflexões estéreo alternadas a 170/290/430/670/1030/1470 ms, com difusão curta e escura. O eco está nos arquivos, sem reverb global que afete música ou outros efeitos. Sem loop; o grito termina suavemente após a troca de forma.

Runtime: `MacaquiniJungleVoice`, instalado por `MacaquiniTransformationPresentation.Awake`, três AudioSources 2D no mixer **Effects**, respeitando `AudioListener.pause`. Controles contra eventos duplicados; fim da batalha, cancelamento e desativação interrompem as caudas. A nova voz suprime o growl/rugido sintético da transformação quando os três arquivos estão disponíveis; o som antigo permanece como fallback. Sem alteração estrutural de cena/prefab.

Assets: `Assets/Resources/Audio/MacaquiniJungleVoiceV26`, três WAVs PCM estéreo 44,1 kHz pré-carregados. Receita: `Tools/Audio/prepare_macaquini_jungle_voice_v26.py`. Prévia das vozes e captura real em `Docs/ArtPreviews/MacaquiniJungleVoice_v26`.

Validação nativa `MacaquiniJungleVoiceV26Validation.Run`: **31 verificações, zero erros**, Unity 6000.6.4f1, 27/10/2026. Inclui integridade PCM/estéreo, eco, sincronização dos quatro momentos, ausência de repetições, pausa real, conclusão natural, reinício e cancelamento durante a transformação. Mix real capturado no AudioListener: 9,429 s, estéreo 48 kHz, pico **0,4213**, RMS **0,0403**, **zero amostras em clipping**. Resultados em `validation.json` e `mix-quality.json`; log `Logs/JungleStage/JungleVoice26.log`. Checkout: 2481 GUIDs válidos.

Build: `MacaquiniJungleVoiceV26Tools.Build`, saída `Builds/Windows-Macaquini-Eco-Selva-v26/MacaquiniSelva.exe`. Na cena de teste, **K** esgota a primeira barra e inicia a transformação; **R** reinicia para repetir. Combate v25 e voadores v24 preservados.

Entrega concluída: build Windows `Succeeded` e versão v26 aberta para teste direto. Log do player em `Logs/JungleStage/JungleVoice26-player.log`. Sem commit/push.
