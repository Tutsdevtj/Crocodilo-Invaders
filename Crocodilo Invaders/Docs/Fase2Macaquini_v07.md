# Macaquini v07 — mãos maiores e movimento suave

Revisão de23/10/2026: o usuário considerou as mãos da v06 minúsculas e pediu movimento mais suave e anatômico, incluindo pesquisar uma biblioteca ou recurso adequado.

As mãos passaram de escala0,80para1,35:68,75%maiores em relação à v06. A constante HandScale também calibra o contato entre punho e dedos, usado pelo cipó, pela arma e pela casca. O tamanho geral do boss permanece no ajuste menor aprovado da v06.

Foi instalado pelo Unity Package Manager o pacote oficial com.unity.2d.animation16.0.1. O controlador usa diretamente UnityEngine.U2D.IK.Limb.Solve para calcular o cotovelo a partir do ombro, comprimentos e destino do punho. Os sprites e as sobreposições da pelagem continuam no rig existente; não é apenas um pacote instalado sem uso. Arrays do solver são reutilizados, sem criar arrays por quadro. [Documentação oficial do 2D IK](https://docs.unity3d.com/Packages/com.unity.2d.animation@16.0/manual/2DIK.html).

A troca de pegada agora percorre uma curva cúbica por fora do corpo, com aceleração/desaceleração gradual e destino previsto antes do alcance. O contato permanece fixo durante o fechamento e a sustentação. Removido o arredondamento da respiração para pequenos degraus; inclinação corporal e mira têm amortecimento temporal. Ombros fazem a transição entre as duas formas progressivamente e a entrada/saída do rasgo conserva a inclinação do corpo.

EnemyMovement recebeu uma opção smoothDashes, ativada apenas no prefab do Macaquini: amortecimento da velocidade e freio próximo dos destinos. A configuração padrão dos demais inimigos e do Gusini permanece desativada. Fundo, ataques, vida, arma orgânica e HUD não foram redesenhados.

Captura inicial final:38checks, zero erros de gameplay; contatos no cipó, arma e casca com erro0,0000; oito malhas arredondadas preservadas. Movimento vertical3,46unidades e cotovelo variando158graus. Ambas as formas, rasgo, pausa e enquadramento em800×600/1200×720/1920×1080 conferidos. Metadados2021GUIDs válidos, diff sem erros de whitespace.

[Vídeo real com áudio](ArtPreviews/Macaquini_v07/macaquini-ik-v07.mp4), [primeira forma](ArtPreviews/Macaquini_v07/forma1.png), [segunda forma](ArtPreviews/Macaquini_v07/forma2.png). Vídeo decodificado integralmente sem erros. Campanha93checks e regressão do Gusini49assertions passaram, somando180verificações com a captura. Build Unity6000.6.4f1 concluída:169131441bytes, incluindo Unity.2D.IK.Runtime.dll. Executável inicializado8s sem exceções de gameplay. Logs em Logs/JungleStage, sufixo v7.

Pacote final: Builds/Pacotes/CrocodiloInvaders-Macaquini-DuasFormas-v07-Windows.zip,100010890bytes;190arquivos comparados por SHA256 com a build. Hash do ZIP:31086A3E342B8041054E80825C2AEC95C898CC1F9818D03E5AF1B1B9AA68265B. Extraia tudo e escolha FASE2–SELVA no menu. Pacotes anteriores preservados; não é necessário instalar Unity nem o pacote de animação para jogar.
