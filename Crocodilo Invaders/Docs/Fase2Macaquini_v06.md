# Macaquini v06 — encaixes e proporções

Ajuste pontual sobre a arte v05 aceita pelo usuário: boss12%menor (escala1,25→1,10), mãos20%menores e emendas dos dois braços corrigidas no ombro e no cotovelo.

As extremidades dos quatro segmentos usam contornos arredondados. Sobreposições de pelagem nos ombros acompanham o tronco, enquanto as dos cotovelos acompanham o antebraço. Os recortes reaproveitam os pixels da master aprovada; não há nova identidade visual. Os quatro desenhos de mão mantêm a mesma escala e o contato foi recalibrado de0,13para0,104unidades locais, incluindo o rasgo da casca.

MacaquiniSpriteMesh restaura as oito malhas no Start usando SpriteMeshType.Tight. A API OverrideGeometry exige coordenadas de pixel e o ciclo do player; não deve ser chamada pelo instalador nem no Awake. O prefab guarda os contornos separadamente dos sprites nativos. Cada instância libera seus sprites temporários ao ser destruída. As sobreposições ficam ocultas no prefab até receberem seu recorte, evitando mostrar a textura inteira no Editor.

Fundo, duas formas, arma orgânica e HUD permanecem como na v05. Histórico da fase completa e da integração: Fase2Macaquini_v05.md.

Validação final:38checks de movimento e captura, zero erros de gameplay; oito malhas arredondadas confirmadas em execução. Contatos no cipó, arma e casca com erro0,0000; cotovelo variando154graus. Ambas as formas e o rasgo observados, pausa e enquadramento em800×600,1200×720e1920×1080. Metadados2020GUIDs válidos e diff sem erros de whitespace. A campanha93checks da v05 continua como referência; esta alteração visual não mudou regras de combate nem a HUD compartilhada.

Prévia com áudio: [vídeo](ArtPreviews/Macaquini_v06/macaquini-encaixes-v06.mp4), [primeira forma](ArtPreviews/Macaquini_v06/forma1.png), [segunda forma](ArtPreviews/Macaquini_v06/forma2.png). Vídeo decodificado integralmente sem erros. Build Unity6000.6.4f1 concluída:168456579bytes, startup do executável8s sem exceções de gameplay. Logs em Logs/JungleStage, sufixo v6.

Pacote: Builds/Pacotes/CrocodiloInvaders-Macaquini-DuasFormas-v06-Windows.zip,99759403bytes;180arquivos comparados por SHA256 com a build. Hash do ZIP:4C0CEF00C059BEAE44E54AA5272F630A9616E30ABBAF1ACD8925AFECA1181BFA. Extraia tudo, execute CrocodiloInvaders.exe e escolha FASE2–SELVA. Pacotes anteriores preservados.
