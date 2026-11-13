# Hangar / guarda-roupa — MVP v01 (10/11/2026)

Primeira versão do guarda-roupa de skins. Só cosmético: colisão, tempos, eventos de tiro e
balanceamento continuam os mesmos para qualquer skin.

## Como funciona para o jogador

- No menu principal, o botão **Hangar** ocupa o lugar do antigo **Online**, que não fazia nada.
- A tela tem uma prévia animada da nave (quadros Idle), setas para trocar e **EQUIPAR**.
  Controles: setas ou A/D (gatilhos LB/RB e direcional no controle), Enter/Espaço/A para equipar
  e Esc/B para voltar. Também funciona com o mouse.
- Aba **NAVE** ativa; a aba **PILOTO · EM BREVE** fica desabilitada e mostra o próximo passo.
- Conteúdo inicial:
  1. **Bombardiro Original**: padrão, sempre disponível.
  2. **Bombardiro Crocs**: disponível desde o início. Desde a v02 é uma Crocs a jato (asas, deriva, turbina) com capacete Crocs e jibbitz de guerra; ver `SourceArt/CrocsSkin_v02/README.md`.
  3. **Protótipo Secreto**: aparece como silhueta, "EM BREVE"; ligado à conquista "Ganso Depenado"
     para quando ganhar arte.
- A escolha é salva em `wardrobe.json` no `persistentDataPath` (escrita atômica + `.bak`,
  mesmo padrão das conquistas). A partida seguinte já começa com a nave escolhida.

## Arquitetura

| Peça | Papel |
| --- | --- |
| `Scripts/Entities/SkinCatalog.cs` | `SkinSlot` (Ship, Pilot), `SkinDefinition` e o ScriptableObject `Resources/Wardrobe/SkinCatalog.asset` |
| `Scripts/Persistence/Wardrobe.cs` | `WardrobeStore` (JSON) e a fachada `Wardrobe`: desbloqueio por conquista, item equipado com fallback ao padrão, `ApplyShip` |
| `Scripts/Presentation/PlayerPresentation.cs` | No `Awake`, chama `Wardrobe.ApplyShip(animator)` antes do Animator começar |
| `Scripts/Presentation/WardrobeView.cs` | Tela montada em runtime (uGUI), cores e fonte do painel de conquistas |
| `MainMenuController.ShowHangar` | Abre o Hangar, Esc fecha, foco volta para o botão |
| `Editor/WardrobeTools.cs` | `Install` (importa a palavra, gera o catálogo a partir dos clipes Idle, troca o botão Online) e `Validate` |
| `Editor/WardrobePlayValidation.cs` | Teste real em Play Mode, com saves isolados em `Logs/Wardrobe` |
| `Tools/Art/prepare_hangar_wordmark_v01.cjs` | Desenha `Assets/Art/MainMenu/Hangar.png` em grade de pixels, no estilo das outras palavras do menu |

O `Player.prefab` continua com o `Player.controller` original. A skin é aplicada só em runtime,
por escolha do jogador. Uma skin removida, sem conquista ou "em breve" nunca quebra o jogo:
`Wardrobe.Equipped` volta para o original.

`AchievementIconTools.Validate` agora aceita a dependência da Crocs apenas pelo catálogo
`Resources/Wardrobe`. Cenas e outros Resources continuam sem referenciá-la.

## Como adicionar uma skin de nave

1. Crie um `AnimatorOverrideController` de `Player.controller` com os cinco clipes (o
   `CrocsSkinTools` é o modelo: mesmas durações, eventos de release e pivô 0,45/⅓).
2. Acrescente uma entrada em `WardrobeTools.BuildCatalog` (id estável, nome, descrição, controller,
   clipe Idle para a prévia e, se quiser, `unlockAchievement`).
3. Rode `Crocodilo/Wardrobe/Install Hangar` e depois `Validate Hangar`.

## Próximos passos (fora do MVP)

- **Piloto/personagem:** hoje o piloto está desenhado dentro dos quadros da nave. Trocar só um dos
  dois exige separar os 26 quadros em camadas. O personagem também envolve os retratos dos
  diálogos, do HUD e do painel Tab.
- Variações de cor via shader de troca de paleta, para ter várias skins sem arte nova.
- Skins de tiro, do rastro do dash e do personagem a pé da base.

## Validação

`WardrobePlayValidation.Run`, partindo do MainMenu real: abre pelo botão, prévia animada, troca,
equipa e persiste, teaser bloqueado, regra de desbloqueio por conquista, foco ao fechar e a skin
aplicada na cena de gameplay e restaurada. Resultado e capturas em `Docs/ArtPreviews/Wardrobe_v01`.
