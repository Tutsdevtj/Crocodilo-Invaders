# Barra do Gusini — Fase 1

`BossHUD.prefab` contém a nova barra específica do Gusini e a apresentação
legada do Executor. `BossHUD` escolhe a versão ao receber `BossStarted`, assina
o `Health` real e remove os vínculos anteriores ao trocar de boss ou terminar
a partida. A nova barra não muda combate, atributos, dano ou recompensas.

Arte original de `BossBar_Gusini_F1.zip`: 29 PNGs x2 em
`Assets/Art/UI/GusiniBossBarF1`, sem redimensionamento. Importados com PPU 100,
Filter Point, Compression None e sem mipmaps. GIFs, estados compostos, previews
e ferramentas do ZIP ficam fora dos assets do jogo. Layout e notas originais
estão em `SourceArt/GusiniBossBarF1` como referência do pacote.

O painel tem 476×100 unidades no canto inferior direito, a (-16,14), em Canvas
1200×720. Usa os ornamentos e a plaqueta GUSINI do pacote. A vida restante fica
ancorada à direita: o preenchimento esvazia da esquerda para a direita.

`BossBarFX` apresenta:

- Entrada de 1,3 s: moldura desliza com fade, medalhão e cachecol descem com
  pequeno quique, nome entra em seguida e a vida carrega entre 0,5 e 1,1 s.
  Um brilho e um flash encerram a entrada. Dano recebido nesse intervalo fica
  guardado e é aplicado ao terminar a carga, usando a vida precisa atual.
- Dano imediato no preenchimento, flash de 120 ms, spark de três frames de
  50 ms na borda da vida e deslocamento de até 2 unidades por 100 ms. Flash/spark
  são limitados a um a cada 80 ms; o rastro acumula o dano, segura 400 ms e
  drena a 0,8 barra/s. Três Images formam um pool fixo para os sparks.
- Cachecol em loop de quatro frames de 120 ms. Brilho ocioso a cada 4,5 s,
  varrendo a vida durante 550 ms, limitado por máscara à área restante. Um
  glint alterna entre medalhão e bico ao terminar cada varredura.
- Vida crítica em até 30%: preenchimento vermelho, contorno e medalhão
  pulsando juntos em 800 ms, cachecol acelerado para 80 ms/frame e shimmer
  desligado. Recuperar vida desfaz esse estado e sincroniza o rastro.
- Tempo de gameplay: entrada, cachecol, pulsos, lag e FX congelam com a pausa.
  Desativação/troca de boss descarta os efeitos anteriores. Após o fim da luta,
  reativar a HUD não faz reaparecer uma barra de boss morto ou de partida encerrada.

O fallback do Executor conserva nome dinâmico, portrait, sprites de vida e o
callback de enrage. A HUD do jogador permanece a mesma da integração v2.

Menus no Editor: `Crocodilo → HUD → Validate Gusini F1 bar` verifica layout,
referências, preenchimentos, pool e importação. `Rebuild Gusini F1 bar` remonta
a apresentação nova a partir do layout, preservando a variante antiga.

Validação na Unity 6000.6.4f1: **35 verificações em Play Mode** passaram,
incluindo entrada, dano fracionário durante a carga, dano rápido, borda do fill,
flash/shake/lag, cachecol, shimmer/máscara/glint, pausa, vida crítica, recuperação,
troca para Executor e de volta, morte e encerramento. Bindings das cenas
PlayerMovementTest, StageOneWaves e GusiniBossTest conferidos sem salvar alterações.
Capturas da partida inspecionadas em 1200×720, 1920×1080 e 800×600.

## Fase 2

A mesma barra agora recebe `BossBarPhaseTwo` e os sprites corrompidos. A F1 fica vazia durante a derrota aparente; a revelação inicia tomada, flash e recarga no lugar. Ver [Gusini Fase 2](GusiniFase2.md) para timings, animações, importação e validação atuais.
