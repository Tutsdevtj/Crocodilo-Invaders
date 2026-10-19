# Limites da arena de combate

`ArenaViewportBounds2D` em `ArenaBounds` posiciona as faces internas de LeftWall,
RightWall, TopWall e BottomWall nas bordas visíveis da câmera ortográfica. A parede
direita antiga em X = 2,27 foi removida dessa posição: a nave pode atravessar toda a
largura visível. A colisão continua física, inclusive durante o dash.

O componente está nas cenas PlayerMovementTest (produção), StageOneWaves e
GusiniBossTest. A Base Improvisada conserva os limites próprios de plataforma.
Referências à câmera, quatro paredes e collider da nave ficam serializadas na cena.
As paredes e a câmera devem permanecer alinhadas aos eixos e com escala unitária.

Os limites acompanham proporção, posição e tamanho da câmera, atualizando antes da
física e após o feedback visual. Uma janela que fica mais estreita recoloca a nave
dentro do novo viewport uma única vez; movimento e dash continuam escrevendo a
velocidade pelo PlayerMovement. Shake não aciona essa correção de posição.

As referências e a geometria padrão de 1200×720 já estão salvas nas cenas. As
ferramentas locais usadas para instalar/validar foram arquivadas fora de Assets;
não são parte do código entregue. Capturas e logs ficam em `Logs/GameplayCleanup`.

Na limpeza da UI de 18/10/2026, foram removidos os 11 objetos da página antiga
`PauseMenu/Content/Panel/OptionsPage`, incluindo quatro TMP e o botão de retorno
provisório; `optionsPage` e `optionsBackButton` saíram do PauseMenuView. A tela de
áudio atual continua ligada em `optionsPrefab`. Também foram excluídos o template
`NewUXMLTemplate.uxml`, PanelSettings e seu tema, sem dependentes na auditoria nativa.
TextMesh Pro continua necessário para diálogos, cartas, nome, ranking e resultados.

Validação desta alteração: 130 assertions de Play Mode passaram nas três resoluções
citadas; capturas da nave junto à borda direita conferidas em 1200×720 e 800×600.
O teste existente de pausa da Base passou outras 16 assertions, incluindo ESC/Start,
opções, retorno ao menu, recarga da Base e pausa/retomada no combate.

Correção posterior: StartButton/Label e a lógica de início provisório saíram de
WavesUI; o painel agora se chama Result e fica salvo oculto. BossInfo/Name/Bar/Portrait
do Executor foram excluídos de BossHUD, com seu código e referências. A barra atual
do Gusini continua; o painel de resultado aparece somente no fim da run. Abertura
direta das cenas inicia a run sem botão; loading pelo menu e starter do boss conservam
seus próprios pontos de início. Captura fora de Play e logs em `Logs/CurrentCombatUiCleanup`.
