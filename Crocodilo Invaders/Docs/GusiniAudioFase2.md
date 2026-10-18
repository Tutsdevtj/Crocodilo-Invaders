# Gusini — áudio da Fase 2

Pacote fornecido em 18/10/2026: `Gusini_Audio_Phase2.zip`. Foram importados **somente os 29 masters de Phase2/** para `Assets/Audio/Boss/GusiniPhase2`. Os WAVs estão idênticos aos arquivos do ZIP, verificados por SHA-256. Candidatos A/B, montagens de preview e instruções antigas de geração não são recursos de gameplay nem foram executados.

Fontes e mapa de eventos: `SourceArt/GusiniAudioPhase2`. O README antigo do pacote descreve um plano ainda pendente; o estado atual está em `README_AUDIO_UNITY.md`, no manifesto atualizado e nos próprios masters. Os oito efeitos opcionais não existem no pacote e não ganharam placeholders.

## Importação, configuração e mixagem

- Masters mono PCM24, 48 kHz; Unity: PCM, Decompress On Load, preload habilitado, sem carregamento em background. Nenhum corte, normalização, conversão ou alteração dos WAVs foi feito na integração.
- Configuração própria: `Assets/Data/GusiniPhaseTwoAudio.asset`, usando `GusiniAudioConfig`. IDs da F2 acrescentados **depois** dos 20 IDs da F1; o asset, GUIDs, clips e valores aprovados da F1 foram preservados.
- `GusiniAudio` continua com **dez fontes compartilhadas**, no mixer Effects, spatialBlend 0 e um pequeno pan. As entidades não criam AudioSources independentes. O orçamento de ganho continua 0,82; efeitos importantes têm prioridade sobre ornamentos.
- Transformação e F2 usam `BeginPhaseTwo`: remove as caudas e o motor F1, sem começar um loop novo. O motor danificado opcional não veio no pacote.
- Ganhos iniciais: normalmente 48% da sugestão do manifesto, avisos principais 55%, pops/poofs 25%, ticks 30%. São valores artísticos por som, preservados ao ajustar os sliders Master/Effects.
- Pops: até três vozes; poofs e avisos/mergulhos: até duas; demais sons: uma. Pequeno cooldown evita empilhamento de transientes simultâneos.
- **Sem retorno do coral:** Low/High da F2 estão importados, mas desativados. Só o líder pode emitir um honk Mid por ataque inteiro, no instante de abertura do bico, ganho 0,07 e pitch fixo. Os bicos restantes continuam animados visualmente. Mantém a decisão anterior do usuário, mesmo que a nota do pacote proponha cinco vozes.

## Gatilhos

Os eventos do Animator usam os clips nativos existentes; os relógios das entidades são independentes. Não são executadas montagens longas com múltiplos eventos embutidos.

| Família | Sincronismo |
| --- | --- |
| Transformação | TR01 golpe final 0; TR02 motor 160 ms; TR03 primeiro pulso 1330; TR04 mutação 1510; TR05 pico 2530; TR06 rugido 3230. O trecho 690–1330 ms não recebe efeito de boss. A música existente não é alterada. |
| Energia | EN01 no início da carga; EN02 em cada release real da bola; EN03 ao acertar/dissipar cada bola. O primeiro clip tem a carga adicional de 300 ms já implementada; o som segue seus eventos. |
| Ovo | EG01 no deploy, 250 ms após nascer. EG02 em +200/+550/+820/+1000 ms da descida; avanço por limiar, uma vez cada, com pitch/ganho levemente crescentes. EG03 nas duas rachaduras, EG04 uma vez para as cinco gemas. Hatch/open e drop continuam reaproveitando F1. Ovo permanece indestrutível. |
| Esquadrilha | SQ01 no call_start 170 ms; SQ02 uma vez no aparecimento dos cinco marcadores. Mid no OnQuack do líder, sem repetição. SQ06 no início do flash de cada mergulhador; SQ07 400 ms depois; SQ08 somente quando HP do gansinho acaba. |
| Tentáculos | TE01 no começo; TE02 aos 190 ms. TE03 uma vez no início de cada aviso de 900 ms; TE04 no lash_hit. TE05 aos 160 ms do tentáculo de faixa, **uma voz para as duas faixas simultâneas**. TE06 no resist_start; TE07 uma única vez ao abrir, mesmo com loop F4/F5; TE08 ao fechar, após a exposição completa de 2 s. |

Não há áudio novo de barra, retração, carga completa, motor ou dano no ovo. O HUD não decide sons de combate.

## Pausa e limpeza

O `PauseController` já congela `Time.timeScale` e `AudioListener.pause`. Os timers de entidade e eventos param com o jogo; as fontes respeitam o listener. Encerrar a luta, morrer ou descarregar a cena interrompe todo o pool. Sons curtos ligados a uma entidade viva são cancelados se o dono for destruído/desativado; poofs, estouros e dissipações pertencem à raiz da ameaça e podem terminar sua cauda depois de desaparecer o sprite.

## Validação

- `GusiniPhaseTwoAudioTools.Install`: importação e ligação nativa no prefab; modifica assets. Editor fechado antes de executar por batch.
- `GusiniPhaseTwoAudioTools.Validate`: 29 masters/formatos/hashes/importers, config separada, IDs F1 estáveis e limites de vozes.
- `GusiniPhaseTwoPlayValidation.RunAudio`: PlayerLoop, Animator e Physics2D reais; acrescenta verificações de áudio ao teste de combate. Confere eventos, silêncio, três releases, ticks/rachaduras, pares de faixas sem áudio duplicado, aviso de 900 ms, janela de 2000 ms, mergulhos e encerramento das vozes.
- `CrocodiloWindowsBuild.ValidateProject` inclui a configuração de áudio F2; a validação anterior da F1 continua ativa.

Validação na Unity 6000.6.4f1: **83 verificações de combate/áudio passaram**, incluindo os seis eventos da transformação e o silêncio, relógio de amostras congelado na pausa, 300 ms entre tiros, deploy/pulsos/rachaduras, avisos e mergulhos, pares de faixas sem som duplicado, janela do núcleo, morte e unload. Validações de F1/Executor, formatos/hashes e configuração de build também passaram. O checkout passou com 1481 GUIDs de assets válidos.

Logs locais: `Logs/GusiniPhaseTwoAudioInstall.log` e `Logs/GusiniPhaseTwoAudioPlayFinal.log`. Verificações automáticas de sincronização e ganho não representam escuta crítica; os volumes são uma primeira mixagem para avaliar ouvindo uma run real.
