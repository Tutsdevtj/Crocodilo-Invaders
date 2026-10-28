# Macaquini — voadores animados v24

Substitui o sapo rejeitado pelo usuário por uma **mariposa esporadora**. O morcego recebe animação contínua de asas. A campanha e a cena de teste carregam exclusivamente os novos desenhos de `Assets/Resources/MacaquiniHelpersV24`; as fontes v23 permanecem como histórico.

- **Morcego emboscador:** oito desenhos completos de voo, asas altas → abertas → baixas → retorno, 16 quadros/s. Bate asas também ao entrar, anunciar o bote, atacar, recuperar e sair. Mantém 2 HP, aviso de 0,85 s, destino travado, bote de 0,42 s e 150 pontos.
- **Mariposa esporadora:** oito desenhos próprios, 24 quadros/s, corpo roxo, asas marfim/turquesa e bolsa de esporos verde. Paira e deriva levemente, sem cipó. Um círculo verde cresce antes de lançar a esfera destrutível; a esfera preserva o aviso e as quatro sementes. Mantém o papel de pressão à distância, 3 HP e 250 pontos.
- Mantidos o limite de três ajudantes, as ondas espaçadas, os intervalos de esquiva, o rasante que atinge aliados e a limpeza na transformação/morte. A segunda forma lutadora não recebe estes voadores.

Arte criada com a ferramenta integrada `image_gen`. Fontes e prompts completos em `SourceArt/MacaquiniHelpers_v24`; exportador `Tools/Art/prepare_macaquini_helpers_v24.py`. Dezesseis PNGs nativos 128×128, alfa binário, paleta compartilhada de até 48 cores por espécie, Point/64 PPU/sem mipmaps/compressão. Densidade uniforme, registro por um ponto anatômico (olho), margens verificadas; o recorte não recentraliza pela silhueta das asas. A mariposa teve os quadros de retorno reordenados para evitar inversão da batida.

Prévia animada com as mesmas artes e velocidades do jogo: `Docs/ArtPreviews/MacaquiniHelpers_v24/batida-de-asas.gif`. Folha de conferência: `voadores-8-quadros.png` nessa pasta. Capturas reais da luta também nessa pasta.

Validação nativa `MacaquiniHelpersV24Validation.Run`: **40 verificações, zero erros**, em 27/10/2026, Unity 6000.6.4f1. Observa os oito sprites efetivamente trocados por cada SpriteRenderer em Play Mode, asas congeladas na pausa, ausência do sapo/cipó, ataques, dano, pontuação, tiro real contra esfera, rasante e transformação. Relatório `Docs/ArtPreviews/MacaquiniHelpers_v24/validation.json`, log `Logs/JungleStage/Helpers24.log`. Ainda requer avaliação humana do visual e balanceamento.

`MacaquiniHelpersV24Validation.RunAndBuild` valida e gera o build; `MacaquiniHelpersV24Tools.Build` gera somente o build. Saída `Builds/Windows-Macaquini-Voadores-v24/MacaquiniSelva.exe`, cena de teste da primeira forma. F8 chama os ajudantes; F1/F2/F3 forçam rajada/rasante/descarga, I alterna proteção, H cura e R reinicia. Cena de observação mantém 240 HP; isso não modifica a vida da campanha.

Entrega concluída: build Windows `Succeeded`, inicialização do executável conferida em segundo plano sem exceções (dispositivo gráfico nulo; conferência visual vem do Play Mode). Checkout: 2469 GUIDs válidos. ZIP completo `Builds/Pacotes/Macaquini-Fase1-Voadores-v24-Windows.zip`, com prévia GIF, 190 arquivos, 114147840 bytes, CRC e conteúdo de cada arquivo conferidos por SHA256. Hash do pacote: `0c13c349f1769e00abbfcaee41f102710559ef21a7a7fa4b67624fc50fc08967`. Startup em `Logs/JungleStage/Helpers24-startup.log`. Sem commit/push.
