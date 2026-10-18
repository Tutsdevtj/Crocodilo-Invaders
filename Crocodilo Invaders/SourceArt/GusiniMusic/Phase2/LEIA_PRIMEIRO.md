# Gusini — diálogo RPG + música Fase 2

Pacote gratuito, sem consumo de créditos de geração. Sons de terceiros com autoria e licenças registradas em `LICENCAS.md`. Nenhum projeto Unity foi alterado.

## Som das letras no balão

Use os WAVs em `Dialogo/Prontos/`. São cinco variantes de **50 ms, mono, PCM16, 48 kHz**, editadas a partir do pacote CC0 “5 Blip UI sound effects!” de FunnyDude. Os originais de 250 ms/44,1 kHz estão em `Dialogo/Originais/`. A edição corta o início para 50 ms, remove DC, aplica fades de 1/5 ms, ajusta pico para −12 dBFS e reamostra para 48 kHz. Não foi gerado áudio novo por osciladores ou IA.

Comece experimentando **SFX_Dialogo_TextBlip_04.wav**, de centro espectral mais baixo, e compare com **01**, mais alto. A escolha é uma indicação técnica para teste; o agente não realizou escuta crítica. Ouça `Previas/Comparacao_Dialogo_01_a_05.wav`: variantes 01 a 05, cada uma com 18 repetições a cada 80 ms; os timecodes estão no manifesto.

Na Unity, ligue o blip ao momento em que uma letra é revelada pelo typewriter. Sugestão inicial: uma repetição a cada **duas letras**, com intervalo mínimo de **60–80 ms**. Ignore espaços, quebras de linha e pontuação; ao pular/revelar todo o texto, não dispare todos os sons de uma vez. Pare ao fechar o diálogo. Use uma fonte dedicada 2D, com concorrência limitada; evite empilhar dezenas de `PlayOneShot`. Variação pequena de pitch, se desejada, deve preservar a leitura. Guarde UI/diálogo em grupo próprio do mixer.

## Música da Fase 2

**Boss Fight 2 — Ansimuz (Luis Zuno)**. O autor descreve como música em loop de chiptune/synthwave para chefes. Foi escolhida como opção para o lado eletrônico da corrupção alienígena; a adequação musical ao personagem precisa da sua audição. Não é composição exclusiva do Gusini.

- `Musica/Boss_Fight_2.wav`: original PCM16 estéreo, 48 kHz, **88,615396 s**.
- `Musica/Boss_Fight_2.ogg`: original Vorbis, útil para música no jogo.
- `Musica/Boss_Fight_2.mp3`: original para audição.
- `Musica/public-license.txt`: permissão distribuída pelo autor, preservada.

Os três arquivos musicais foram mantidos sem alteração. A página do autor os oferece como loop. A janela informada é do sample **0 ao 4.253.539 exclusivo** no WAV. `Previas/Emenda_Musica_8seg.wav` junta os últimos 8 s aos primeiros 8 s: a emenda acontece em **8,0 s** nessa prévia. Os fades só nas bordas externas da prévia não escondem a emenda interna. O encaixe harmônico/rítmico deve ser ouvido antes da aprovação; não se declara uma verificação auditiva feita pelo agente.

Para integrar, comece a música quando a transformação terminar, em **4100 ms**, ao entrar no combate da Fase 2. Preserve o silêncio da falsa morte em **690–1330 ms**. Use AudioSource 2D exclusivo para música e habilite loop na faixa escolhida; confira no alvo final. Música longa pode usar importação/streaming conforme a memória e plataforma do projeto. O volume original chega perto de 0 dBFS: comece com a música mais baixa no AudioMixer e ajuste em conjunto com os SFX, mantendo avisos e tiros legíveis. Fade curto ao entrar/sair; pare na morte/fim da luta. Não embuta a música nos SFX de transformação.

## Fontes

- Blips: https://opengameart.org/content/5-blip-ui-sound-effects
- Música: https://opengameart.org/content/boss-fight-2
- CC0: https://creativecommons.org/publicdomain/zero/1.0/

O pacote específico “Character Speaking Blip Sounds” de Chris’ Tutorials foi encontrado com indicação CC0, mas seu download retornou 403 e não foi incluído. A alternativa efetivamente entregue é de FunnyDude, com originais e derivações identificados. Não se alega origem nem download do pacote indisponível.
