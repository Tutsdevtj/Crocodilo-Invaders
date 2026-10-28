# Bancada de animação — Macaquini v08

Alternativa solicitada para teste: cada pose já contém corpo inteiro, dois pés/pernas, rabo, rosto, arma e cipó. O componente troca apenas o sprite completo; não encaixa nem gira membros separados. A campanha não foi substituída por este protótipo.

56desenhos únicos:16de balanço por forma,8de disparo por forma e8de transformação. Cinco clipes Unity prontos em Assets/Animations/Boss/MacaquiniFullFrames_v08. No balanço, os desenhos intermediários voltam em ordem inversa para uma repetição contínua. As ações retornam ao balanço e o rasgo termina na segunda forma.

A bancada abre diretamente, sem hordas, morte ou gravação de progresso. Permite clicar em cada quadro, pausar e mudar a velocidade para examinar o cipó, o apoio das mãos, o rosto, os pés e o rabo. A selva em planos de parallax é a mesma já aprovada. A pausa congela também o cenário.

Controles: Espaço pausa/reproduz; setas quadro anterior/próximo;1/2formas;Bbalanço;Adisparo;Trasgar;Escsair. Os mesmos controles aparecem como botões. A tira de miniaturas seleciona poses exatas. Velocidade0,25x–2x; ataque pode repetir ou voltar ao balanço.

Fontes, prompts e tabela das etapas: [SourceArt/Macaquini_v08](../SourceArt/Macaquini_v08/README.md). Exportador Tools/Art/prepare_macaquini_frames_v8.cjs; alfa binário e paleta40cores, sem interpolação artificial de peças. [Referência anterior com IK v07](ArtPreviews/Macaquini_v07/macaquini-ik-v07.mp4) para comparação.

Execução final nativa:23checks e zero erros de gameplay, todos os56desenhos vistos no PlayerLoop. Testados raycast do mouse, botões, pausa, passos, seleção de miniatura, formas, velocidade, repetição, rasgo e retorno ao balanço. Capturas em800×600,1200×720e1920×1080. Checkout2090GUIDs válidos. Build Unity6000.6.4f1 concluída:120144315bytes; executável inicializado8s sem exceções. Logs em Logs/JungleStage com Frames-v8.

[Vídeo real do teste](ArtPreviews/Macaquini_v08/macaquini-quadros-completos-v08.mp4), capturado no PlayerLoop e decodificado integralmente sem erros. [Imagem da bancada](ArtPreviews/Macaquini_v08/preview_half.png).

Pacote: Builds/Pacotes/Macaquini-Teste-Quadros-Completos-v08-Windows.zip,65396956bytes;324arquivos comparados por SHA256 com a build. Hash:882B23C332CBA9D70D62964DA60D9B3722D6EA65363ADC45817253D1F79889BF. Extraia e execute MacaquiniQuadros.exe. Inclui a pasta Arte com56PNGs/metadados, cinco clipes .anim, prefab/controlador, quatro folhas originais, prompts e manifesto. A cena e o executável são independentes da campanha, que continua com o boss v07.
