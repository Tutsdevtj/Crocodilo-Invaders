# Macaquini v05 — duas formas

O pedido atual é um macaco com acabamento detalhado e rosto claramente de macaco, preso numa banana que ele rasga na segunda forma. A nova arte tem focinho curto, orelhas arredondadas, pelagem castanha e dedos com unhas normais. O estudo v03 rejeitado ficou arquivado fora de Assets; a v04 foi a base funcional anterior.

O corpo começa com cabeça e tronco escondidos pela banana. Ao esgotar180HP, a mão da arma recolhe a banana, agarra a casca e abre dois fragmentos retirados dos próprios pixels do corpo, enquanto a outra mão sustenta o personagem no cipó. A animação nativa dura4,22s e revela cabeça e peito, conservando a casca inferior. Mais180HP completam os360HP totais. Só a morte definitiva concede pontos e conclui o capítulo; o leque passa de5para7bananas com o mesmo aviso.

Os braços têm ombro, cotovelo e punho, sem esticar os sprites. A mão carregada mantém contato fixo no mundo; abre e fecha em quatro desenhos ao trocar de pegada antes de exceder o alcance mínimo ou máximo. A arma é a banana orgânica original v04, com12quadros, recuo compensado e disparo320ms. A HUD reutiliza a hierarquia, moldura, preenchimento e efeitos do Gusini, com nome e medalhões próprios para as duas formas.

O fundo da selva v02 aprovado foi preservado: mata profunda, quatro faixas transparentes em velocidades distintas, oito cipós e folhas em movimento.

Fontes e prompts: SourceArt/Macaquini_v05. Exportador: Tools/Art/prepare_macaquini_v5.cjs. Sprites320×384, mãos96×96, paleta32cores, alfa binário, Point/sem compressão. layout.json contém hashes e pivôs; a recomposição dos fragmentos tem zero diferenças em relação à casca fechada.

Prévia real com áudio: [vídeo](ArtPreviews/Macaquini_v05/macaquini-duas-formas-v05.mp4). Imagens: [primeira forma](ArtPreviews/Macaquini_v05/forma1.png), [segunda forma](ArtPreviews/Macaquini_v05/forma2.png).

Validação de movimento:37checks, zero erros de gameplay; pegada no cipó, arma e casca com erro0,0000unidades, cotovelo variando157,7graus e deslocamento vertical3,89unidades. O teste confirmou que o clip realmente anima a abertura da casca. Capturas em800×600,1200×720e1920×1080; vídeo completo decodificado sem erros. Metadados:2010GUIDs válidos. A regressão do HUD compartilhado do Gusini já passou49assertions na v04, após a última mudança nesse HUD.

Campanha completa:93checks, zero erros de gameplay. Inclui164animais, nove cartas, pausa, transição invulnerável, limpeza de ameaças, leque da segunda forma, pontuação única e progresso exclusivo do capítulo2. Build Windows-Macaquini-v05 concluída no Unity6000.6.4f1:168258788bytes. Executável inicializado por8s sem exceções de gameplay. Logs em Logs/JungleStage.

Pacote: Builds/Pacotes/CrocodiloInvaders-Macaquini-DuasFormas-v05-Windows.zip (99578307bytes). Todos os180arquivos do ZIP tiveram SHA256 comparado com a build; EXE, UnityPlayer e Assembly-CSharp presentes. SHA256 do ZIP: FEEE54FBD8A5DEC4D130DBF10CAC2649C140B99FF8B2A794A77BA7BACA125850. Extraia tudo e execute CrocodiloInvaders.exe; escolha FASE2–SELVA no menu. Pacotes anteriores preservados.

Estas verificações cobrem execução, encaixes, pausa e progressão. A aprovação artística continua com o usuário.
