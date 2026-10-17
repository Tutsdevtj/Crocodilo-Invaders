# HUD do jogador — cockpit v2

`Assets/Prefabs/UI/PlayerHUD.prefab` usa as camadas originais de `HUD_Player_v2.zip`.
O Canvas continua em 1200×720, com escala por resolução e match 0,5. O cockpit
ocupa 364×112 unidades no canto inferior esquerdo, a 16 unidades das bordas.
Pontos e inimigos conservam seus objetos, posições e conexão ao `RunScore`.
As instâncias nas cenas continuam referenciando o mesmo prefab e componente.

`PlayerHUD` assina os eventos de `Health` e `PlayerCombat`. `PlayerHUDCockpit`
apenas apresenta esses valores; não altera dano, carga, custos ou atributos.

- HP usa o valor preciso normalizado, inclusive dano fracionário e mudanças na
  vida máxima. O rastro segura 350 ms e cai a 0,9 barra/s; cura o sincroniza
  imediatamente e produz um flash de 250 ms.
- Abaixo ou em 25% de vida, a blindagem e o emblema ficam vermelhos, o mostrador
  ganha rachaduras e o alerta pulsa a cada 0,9 s.
- SP tem preenchimento contínuo, onda e máscara do tamanho exato da carga.
  A lâmpada/fill mudam abaixo de 1/3, a partir de 1/3 e ao chegar a 100%.
  Ganhos de carga produzem flash de 200 ms e bolhas por 350 ms; no máximo três
  bolhas usam objetos já existentes no prefab.
- Especial cheio toca ignição de três frames de 100 ms, emite 6–9 centelhas e
  inicia chama de seis frames de 80 ms. A turbina gira, o brilho pulsa e surgem
  centelhas esparsas a 2,5/s. Ao gastar, quatro frames de 100 ms encerram a chama.
- Todos os efeitos usam tempo de gameplay e congelam durante a pausa. As
  centelhas usam um pool de nove Images e um sorteio visual separado do gameplay.
  Reativar/revincular a HUD sincroniza os valores sem repetir a ignição.

Foram importados apenas **51 PNGs x2**, sem redimensionar seus pixels, em
`Assets/Art/UI/PlayerHUDv2`. Filter Point, Compression None, sem mipmaps e PPU 100.
As composições, GIFs e ferramentas do ZIP não entram nos assets do jogo. Layout
e notas originais estão em `SourceArt/PlayerHUDv2`, como referência do pacote.
O prefab e os scripts da HUD do boss não fazem parte desta alteração.

No Editor, `Crocodilo → HUD → Validate Player cockpit` confere as referências,
preenchimentos, layout e importação. `Rebuild Player cockpit` remonta apenas
a arte do painel a partir do layout original, mantendo os contadores.

Validação realizada na Unity 6000.6.4f1: **42 verificações em Play Mode**, incluindo
dano fracionário, rastro de dano, cura, HP crítico, vida máxima, carga por disparo
real, uso real do especial, ignição/chama/desligamento, pools, pausa, contadores,
reativação, vínculo e dano letal. Referências de jogador/score/HUD conferidas em
PlayerMovementTest, StageOneWaves e GusiniBossTest. Capturas do jogo inspecionadas
em 1200×720, 1920×1080 e 800×600. O teste não salva alterações nas cenas.
