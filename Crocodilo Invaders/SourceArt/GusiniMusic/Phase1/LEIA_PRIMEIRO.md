# Gusini — música da Fase 1

**Escolha: Phantasy Boss Battle Encounter, de Ansimuz (Luis Zuno).** Gratuita, CC0, com WAV original. Não houve geração ou consumo de créditos. Nenhum projeto Unity foi alterado.

## Por que esta opção

A seleção usa como referência o pacote real `Gusini_Audio_Phase1.zip`: motor de avião a hélice, canhão curto de desenho animado, mecanismos pequenos, assobio de comandante, coral de gansos e ovo de paraquedas. A proposta musical é sustentar o combate arcade e a personalidade fanfarrona desse chefe. A faixa é chiptune de batalha de chefe, do mesmo autor de **Boss Fight 2**, escolhida para a Fase 2; essa autoria e linguagem retro oferecem um ponto de continuidade entre as fases.

Essa é uma recomendação editorial baseada na descrição do autor e no pacote de SFX, não uma afirmação de análise auditiva: o agente não realizou escuta crítica. Confira a prévia com os efeitos antes de aprovar o equilíbrio e a adequação ao personagem.

## O que usar

- **`Musica/MUS_Gusini_Phase1_Loop.wav`**: WAV PCM16 estéreo, 48 kHz, **66 segundos**. Nome original `BossBattle.wav`, bytes preservados; não é conversão de MP3.
- `Musica/BossBattle.ogg`: OGG original do autor, para importação conforme a plataforma.
- `Musica/BossBattle.mp3`: MP3 original para audição.
- `Musica/public-license.txt`: permissão original do autor, mantida sem alteração.
- **`Previas/Mix_Fase1_com_SFX_30s.wav`**: 30 s de música com os SFX reais da F1. Ordem de ataques ilustrativa, não captura do jogo. Não usar como áudio de gameplay.
- `Previas/Conferencia_Loop_Emenda_em_6s.wav`: últimos 6 s seguidos dos primeiros 6 s, com a emenda em 6,0 s. Fades apenas nas bordas externas da prévia; não escondem o ponto interno.
- `music_manifest.json`: duração, origem, hashes, níveis e eventos usados na montagem.

## Integração sugerida

Dispare a música ao começar o combate da Fase 1, depois do diálogo. Use uma fonte dedicada 2D, `loop=true`, separada do grupo SFX. O autor confirma que a faixa é loopada na própria página. Janela do WAV: sample 0 até **3.168.000 exclusivo**, sem corte editorial aplicado à faixa original. Faça a escuta da emenda no alvo final antes de concluir a integração.

Na montagem de exemplo, música usa ganho 0,28; motor 0,18; os efeitos seguem os volumes sugeridos no manifesto da F1. Estes ganhos são pontos de partida, não uma mixagem aprovada dentro da Unity. A prévia respeita a cadência de 430 ms dos três tiros e a defasagem de 140/280 ms do coral, mas o jogo deve disparar os arquivos separados em seus próprios eventos.

Mantenha o assobio e os ataques perigosos legíveis acima da música. Reduza a trilha durante diálogo e, se necessário, durante telégrafos. Ao zerar a barra da F1, faça a transição pelo fluxo existente e preserve a pausa de falsa morte **690–1330 ms** no clip de transformação. A trilha da Fase 2 pode começar após terminar a transformação, em **4100 ms**, conforme as instruções do pacote da F2. Nenhuma mudança de código ou assets Unity foi feita nesta entrega.

## Fonte e direitos

Página do autor: https://opengameart.org/content/phantasy-boss-battle-encounter

A publicação indica **CC0**, permite uso pessoal/comercial e modificações; crédito opcional. O autor confirma o loop em resposta de 20/03/2022. O ZIP inclui também permissão de redistribuição. Consulte `LICENCAS.md` e o texto original incluído. Sugestão voluntária: “Music: Phantasy Boss Battle Encounter by Ansimuz (CC0).”
