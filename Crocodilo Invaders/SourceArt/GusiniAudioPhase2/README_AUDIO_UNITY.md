# Gusini — áudio da Fase 2

29 efeitos prioritários P0 gerados no ElevenLabs e exportados em WAV, com dois candidatos A/B preservados por efeito. Os oito opcionais P1 estão adiados; não há placeholders. Nenhum projeto Unity foi alterado.

Os pilotos TR04 B e TE04 B tiveram a identidade aprovada pelo usuário. Os outros A/B foram comparados por medidas de duração, energia preservada no corte e conteúdo de alta frequência. **Não foi realizada escuta crítica pelo agente.** A aprovação sonora individual e a escuta final no jogo continuam pendentes; a comparação técnica não demonstra, por si só, fidelidade tímbrica aos prompts. Consulte `CANDIDATE_COMPARISON.json`, especialmente `needs_tail_listening`.

## Arquivos

- `Phase2/`: somente os 29 masters separados, com os nomes exatos do manifesto.
- `_candidates/<id>/A.wav` e `B.wav`: exportações originais, sem alteração.
- `_previews/AB_TODOS_P0.wav`: comparação A/B de todos os efeitos, com pausas de 700 ms. Os timecodes estão no JSON ao lado.
- `_previews/01_Transformacao.wav` a `05_Tentaculos_3_ciclos.wav`: montagens somente para conferência; não importar como clips de gameplay. `SEQUENCE_CUES.json` distingue eventos reais de offsets ilustrativos.
- `audio_manifest.json`: fontes, seleção, durações medidas, formato, status e gatilhos.
- `AUDIO_EVENT_MAP.md`: mapa de sincronismo original; prevalece para os relógios das entidades.
- `CREDIT_ESTIMATE.json`: orçamento aprovado e consumo observado.
- `missing_or_deferred.md`: opcionais e revisões pendentes.

## Formato e edição

Originais: **WAV PCM16 estéreo, 48 kHz**. Masters: **WAV PCM24 mono, 48 kHz**. A edição em 24 bits não acrescenta resolução à fonte de 16 bits. Não foi usado MP3 ou conversão de áudio com perdas para alegar qualidade lossless.

Limpeza editorial: média L/R para mono, remoção de DC/silêncio inicial, corte de cauda para a janela do evento, fade-in de 0,5 ms, fade-out de 8 ms e pico de −3 dBFS. Não há time-stretch, efeitos sintetizados, substituições artificiais ou sequências embutidas nos masters. Os pilotos aprovados foram preservados. Cortes relevantes de cauda ficam sinalizados no relatório para escuta.

## Integração na Unity

Importe apenas `Phase2/` para uma pasta exclusiva da Fase 2. Preserve os áudios da Fase 1. Sugestão inicial: `Decompress On Load`, `Preload Audio Data`, Compression Format = PCM para preservar transientes e `spatialBlend=0` no jogo 2D. Se a posição tiver função de gameplay, ajuste a espacialização na integração. Direcione as fontes a um grupo SFX do AudioMixer e ajuste o volume no contexto do jogo; os valores no manifesto são pontos de partida.

Dispare cada arquivo em seu evento de gameplay/Animation Event; não use `Update()` por frame para repetir avisos. Os relógios do ovo, do canto de cada ganso e do tentáculo de faixa são independentes do clip do boss. Consulte o mapa completo para cada callback.

| Família | Sincronismo essencial |
|---|---|
| Transformação | TR01=0, TR02=160, TR03=1330, TR04=1510, TR05=2530, TR06=3230 ms. Preserve o silêncio 690–1330 ms. IA só após 4100 ms. |
| Rajada | EN01=0; o mesmo EN02 em 490/790/1090 ms. EN03 na colisão/dissipação de cada projétil. Não disparar a prévia de três tiros. |
| Ovo | EG01 no deploy, cerca de 250 ms após spawn, conforme gameplay. Descida começa após os 210 ms de deploy; EG02 em +200/+550/+820/+1000 ms da descida. EG03 nas duas rachaduras; EG04 uma vez para as cinco gemas. |
| Esquadrilha | SQ01=170 ms; SQ02 uma vez para todos os marcadores (~300 ms). Gansos nascem fora da tela em 580 ms; honks somente no `OnQuack` do bico (+120 ms do canto). SQ06 precede SQ07 em 400 ms; SQ08 na derrota. |
| Tentáculos v2 | TE01 no início, TE02 em +190 ms. Cada TE03 é seguido por TE04 **900 ms depois**, em três ciclos. TE05 em +160 ms da entidade de faixa, uma vez para as duas faixas. TE06 após três golpes, TE07 em +370 ms da resistência; TE08 **2000 ms após abrir**. |

Evite dobrar as duas faixas simultâneas dos tentáculos. Comece limitando honks a três vozes e poofs a duas; reduza ornamentação para preservar avisos. Interrompa as fontes ao pausar, morrer ou destruir a entidade. Diver só nos gansos designados pelo gameplay; não acrescentar homing. Ovo é indestrutível, sem áudio de dano de ovo.

Os oito `deferred` não possuem clip. Não os preencha com placeholders. OP03/OP04/OP05/OP08 podem reutilizar hatch/drop/flyby/hit aprovados da F1, se a integração demonstrar benefício. Motor, confirmação de carga, retração e barra são ornamentais e permanecem ausentes nesta entrega.

## Conferência antes de integrar

Ouça as cinco prévias e o A/B. Confira a identificação orgânica/elétrica, o reconhecimento dos gansos e a naturalidade dos cortes. A verificação automática confirmou o formato, arquivos distintos, durações, picos e intervalos dos eventos; não substitui essa escuta. A transformação tem silêncio real entre 690 e 1330 ms; rajada tem espaçamento de 300 ms; tentáculos preservam aviso de 900 ms e exposição de 2000 ms. Os offsets entre ciclos de tentáculos e entre gansos nas prévias são exemplos, não uma mudança de gameplay.

## Créditos e direitos

O teto aprovado foi **350 créditos no total, incluindo os 20 dos pilotos**. `CREDIT_ESTIMATE.json` registra o saldo inicial/final e o consumo efetivo observado. A interface gera quatro variantes por lote; foram exportadas e comparadas as variantes 1/2 como A/B; 3/4 permanecem no histórico do provedor. Nenhum plano ou pagamento foi alterado.

Plano observado: Starter. Política oficial consultada em 18/10/2026: planos pagos incluem licença comercial para serviços não-Beta, sujeita a direitos de propriedade intelectual, legislação e termos aplicáveis: https://help.elevenlabs.io/hc/en-us/articles/13313564601361-Can-I-publish-the-content-I-generate-on-the-platform . O registro está no manifesto; não se acrescenta garantia jurídica por edição de formato.

Referências de importação: https://docs.unity.com/en-us/engine/7000.0/manual/audio/reference/class-audio-clip e https://docs.unity.com/en-us/engine/6000.0/manual/audio/files/compression . Ajuste ao alvo/versão do seu projeto; estas são sugestões de integração, sem alterações no projeto.
