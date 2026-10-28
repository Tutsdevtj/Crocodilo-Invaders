# Macaquini — infecção e continuidade v13

Correção dos três pontos apontados após a v12: rosto fechado com leitura alienígena, piscadas/reaparecimento da roupa durante o rasgo e troca indevida de braço na busca pelo cipó.

A casca continua amarela. A expressão passa a pertencer à banana possuída: fissuras escuras, tecido e veios verdes, olhos em fendas e uma pequena abertura orgânica que pulsa. Não há dentes humanos alternando com um sorriso de desenho. O conceito segue a definição já registrada em `BananaPossessora.md`: a banana foi infectada e possui o macaco, como a corrupção do Goosini. Os retratos do Goosini foram usados apenas para coerência da infecção; a identidade e a anatomia do Macaquini foram preservadas.

São 32 desenhos de voo na ordem cronológica da pulsação. A v12 ordenava os sprites pela proximidade dos pés, o que podia alternar expressões sem continuidade. Agora a sequência facial não é embaralhada. O registro usa os centros dos pés e metadados individuais de pivô/PPU; não se apoia na borda da casca, que muda durante o rasgo. A média combinada de deslocamento dos pés é 0,97 px, com máximo de 4,11 px na escala de referência de 80 PPU.

A transformação tem 42 desenhos completos e tempos explícitos. O topo é retirado; a mão desce ao peito; as duas mãos abrem a casca; o conjunto com jetpack é lançado; ele grita e busca o cipó com o braço que aparece à direita da imagem, o mesmo que permanece segurando o cipó na segunda forma. Um quadro de busca com o braço oposto e outro com a mão cortada foram rejeitados. Uma primeira folha de rasgo que cortava as pernas também não foi importada.

O progresso visual é monotônico: uma atualização temporariamente menor da curva do Animator não pode selecionar um desenho anterior nem levantar o corpo novamente. A queda do corpo usa esse mesmo progresso protegido. Os eventos nativos de soltura avançam a pose até seu marco, evitando mostrar uma peça simultaneamente na mão e no mundo. A inclinação brusca durante o rasgo foi reduzida e o flash de dano não colore a transformação invulnerável.

A extensão do cipó começa no próprio desenho em que a mão fecha a pegada. A primeira pose da segunda forma é determinística e o registro dos últimos desenhos converge gradualmente para o ciclo do cipó preservado da v11. As peças continuam caindo com gravidade até o limite inferior do mapa, com os mesmos cuidados de pausa e cancelamento da v12. Ataques, HUD e regras de vida/pontuação continuam nos controladores existentes.

A escala final usa a largura opaca da pose completa, em vez da área de cor dos pés, que variava entre os estilos de sombreamento. O último desenho da transformação e o primeiro do cipó têm largura visível de 5,0167 unidades antes da escala do objeto; razão 1,0000. O instalador rejeita diferença acima de 0,5%. Isso evita encolher durante a busca e aumentar abruptamente na entrada da segunda forma.

Arte criada com o image_gen integrado a partir dos sprites do projeto. Fontes, prompts, motivos das exclusões e manifesto: `SourceArt/Macaquini_v13`. Exportador: `Tools/Art/prepare_macaquini_v13.cjs`. Os PNGs preservam RGB e resolução nativa, com transparência binária; importação Point sem compressão ou mipmaps. Os ajustes de escala são metadados de PPU, sem redesenhar ou reamostrar os pixels.

Instalação: `MacaquiniContinuityV13Tools.Install`. Validação e gravação: `MacaquiniJetpackValidation.Run`. Build: `MacaquiniContinuityV13Tools.Build`. O prefab real da campanha mantém o mesmo GUID. Evidências em `Docs/ArtPreviews/Macaquini_v13` e logs em `Logs/JungleStage/*Continuity-v13*`.

O teste jogável abre diretamente no boss. K esgota a forma para testar a transformação; J retira 45 HP; F1/F2/F3 forçam ataques; I alterna proteção; H cura; R reinicia; ESC pausa. Progresso e ranking do teste permanecem isolados da campanha principal. A avaliação estética final continua sendo do usuário.

Validação: 51 verificações do boss/continuidade e 93 da campanha, sem erros de gameplay; 2367 GUIDs válidos no checkout. O teste força um reset temporário da curva e confirma que desenho e posição não voltam para trás; monitora corpo visível/opaco e chegada ao último quadro sem regressão. A gravação inclui pausas intencionais dos testes de pausa. `hand_continuity.png` contém recortes de quadros reais de alcance, pegada e segunda forma; `recording_segments.json` registra os instantes e arquivos de origem.
