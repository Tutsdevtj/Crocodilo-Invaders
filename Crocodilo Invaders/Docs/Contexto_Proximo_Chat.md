# Crocodilo Invaders — contexto para o próximo chat

Atualizado em 18/10/2026. Este arquivo resume o chat anterior; não é um pedido para refazer as implementações. Aguarde a próxima solicitação do usuário.

## Projeto e preferências

- Usuário: Arthur; responder em português, de forma direta e pouco técnica. Pediu economia de tokens e validação proporcional, sem baterias extensas para mudanças simples.
- Projeto Unity ativo: `C:/Users/Arthu/OneDrive/Desktop/Crocodilo-Invaders/Crocodilo Invaders`.
- Raiz Git: pasta pai `C:/Users/Arthu/OneDrive/Desktop/Crocodilo-Invaders`.
- Unity **6000.6.4f1**, C#, Input System. Não confundir com os projetos legados `unity/` e `Gaming portugol/`.
- Guia geral: `../agent.md`. Algumas fotografias antigas do Git nesse guia estão desatualizadas; conferir o estado real.
- Branch `main`; último commit observado: **9a38652 — Integrate Gusini phase two, audio and chapter cinematics**.
- Há trabalho local importante, modificado e não rastreado, **sem commit**. Não descartar. O pedido da tarefa de dash foi não commitar; não fazer commit/push sem novo pedido.
- O Editor estava aberto neste projeto no fim do chat. Não fechar nem reescrever cenas/prefabs abertos sem considerar mudanças do usuário.

## Jogo já existente

Shooter aéreo 2D em pixel art. Run: abertura por rádio → nove hordas, com cartas entre elas → Gusini F1 → falsa derrota/diálogo/transformação → F2 → resgate/diálogo final → transição para `BaseImprovisada` (aviso enquanto a cena não existir).

Gameplay real: `Assets/Scenes/Tests/PlayerMovementTest.unity`. Teste direto do boss: `Assets/Scenes/Tests/GusiniBossTest.unity`. Player real: `Assets/Prefabs/Player.prefab`.

Controles: setas, Espaço segurado para tiro, C especial, ambos os Shifts para dash, Esc pausa. Preserve inputs, animações com release por Animation Events, HUD, cartas e progressão existentes.

O kit de teste do boss já foi aumentado a pedido do usuário: 240 HP, tiro normal ×4, especial ×3, cadência ×2,5, movimento ×1,4, dash distância ×1,3/cooldown ×0,4/proteção ×1,4, especial inicialmente carregado e recarga em cinco tiros. Só na cena de teste e no Editor; não remover nem aplicar às runs normais.

## Dash implementado neste chat — concluído

Pedido original completo: `C:/Users/Arthu/.codex/attachments/86e13cf7-114a-413d-9d81-5dd44e6168a6/Texto colado.txt`.
Pacote usado: `C:/Users/Arthu/Downloads/Dash_VFX.zip`. Documentação atual: `Docs/PlayerDash.md`.

- Base: **2,2 unidades / 0,16 s**, cooldown **1,1 s**, proteção **0,13 s**; configuráveis no Inspector.
- Curva de velocidade: `(0; 2) → (0,65; 0,8) → (1; 0,2)`. Integral normalizada em 256 intervalos preserva distância total com curvas/FPS diferentes. Impulso inicial e cauda curta.
- `PlayerMovement` continua sendo o único escritor da velocidade do Rigidbody; sem teleporte. Preservados diagonais normalizadas, direção travada, última direção e upgrades.
- Somente o Rigidbody do player mudou para **Continuous**, após comprovar atravessamento de parede fina com Discrete. Física global e colliders preservados.
- `Health.SetInvulnerable(owner, bool)` permite proteções independentes; terminar dash não remove outro escudo.
- Eventos: `DashStarted`, `InvulnerabilityEnded`, `DashEnded`, `DashCancelled`.
- **22 PNGs originais**, sem redesenhar: ignição 8, rastro 6, fim 8. Importação Point/PPU100, pivôs do pacote, orientações por flips/90°.
- `PlayerDashVFX`: ignição/fim em mundo, rastro segue centro visual, seis afterimages em pool (~30 ms, vida 130 ms), silhueta ciano/sombra/contorno. Indicador de um pixel somente durante proteção do dash, mantendo nave visível.
- Pausa, morte, bloqueio de gameplay, disable e unload limpam/congelam corretamente. Não alterados tiros, bosses ou waves nesta parte.
- Ferramentas: `Assets/Editor/PlayerDashTools.cs` e `PlayerDashPlayValidation.cs`.
- Testes anteriores: bateria completa **284 verificações**; rodada extra visual/input **63**; capturas em `Logs/DashVFX/`. Não foi um playtest manual de run completa.

### Áudio do dash — concluído

Usuário forneceu `C:/Users/Arthu/Downloads/Crocodilo_Invaders_Dash_SFX/originais/A/1.wav`.

- Copiado sem alterar bytes para `Assets/Audio/Player/SFX_Player_Dash.wav` e ligado no prefab em `PlayerDashVFX.Dash Sound`.
- Estéreo, 48 kHz, 0,4865 s; PCM, preload e Decompress On Load.
- Uma reprodução no início bem-sucedido; cooldown não repete. Fonte 2D, mixer **Effects**, volume local **0,25** ajustável em `Dash Volume`.
- Validação nativa de input/VFX/áudio passou com **69 verificações**; log `Logs/PlayerDashAudio.log`. SHA-256 idêntico ao fornecido. Sem escuta crítica.

## Música das ondas — última implementação, concluída

Pedido: integrar `C:/Users/Arthu/Downloads/Crocodilo_Invaders_Musica_Ondas.zip` nas ondas antes do boss, com validação curta para economizar tokens.

- Master original **Going Up — adventure Chiptune**, Ansimuz / Luis Zuno, pacote registra CC0.
- Asset: `Assets/Resources/Audio/Music/MUS_Crocodilo_Ondas_Loop.wav`, 55 s, estéreo/48 kHz; bytes originais conferidos por SHA-256. Streaming, mesma configuração das músicas do Gusini.
- Novo `Assets/Scripts/Presentation/WaveMusic.cs`.
- `WaveController.Awake` adiciona `WaveMusic` se ausente, após validar o controlador. Não foi necessário editar cenas abertas. A faixa é carregada via Resources.
- Começa no anúncio da primeira horda, depois da abertura; loop contínuo entre ondas e durante cartas, sem reiniciar a cada horda.
- Fonte 2D dedicada, mixer **Music**, volume **0,28**, entrada 0,6 s. `AudioListener.pause` respeitado; fade usa tempo não escalado para funcionar durante cartas.
- Fade de **0,9 s** no anúncio do boss; `BossStarted` garante parada. Também para em fim de run/disable/unload. `GusiniMusic` e suas faixas F1/F2 preservados.
- Proveniência, licença original e manifestos em `SourceArt/WaveMusic`; créditos em `Docs/Creditos.md`. Previews/MP3/OGG não foram importados como assets de gameplay.
- Conferência curta: compilação C# com referências da Unity passou (somente avisos antigos de APIs obsoletas), hash original e `git diff --check`. **Sem teste em Play Mode ou escuta da música das ondas**, conforme escopo econômico solicitado.

## Arquivos locais pendentes de commit

Modificados: `Assets/Prefabs/Player.prefab`, `Assets/Scripts/Controllers/PlayerDash.cs`, `WaveController.cs`, `Assets/Scripts/Entities/Health.cs`, `Assets/Shaders/SpriteHitFlash.shader`, `Docs/PlayerDash.md`, `Docs/Creditos.md`, `../agent.md`.

Novos, com os respectivos `.meta` onde aplicável:

- `Assets/Art/Player/DashVFX/`
- `Assets/Audio/Player/`
- `Assets/Resources/Audio/Music/`
- `Assets/Scripts/Presentation/PlayerDashVFX.cs` e `WaveMusic.cs`
- `Assets/Shaders/DashAfterimage.shader` e `Assets/Materials/DashAfterimage.mat`
- `Assets/Editor/PlayerDashTools.cs` e `PlayerDashPlayValidation.cs`
- `SourceArt/DashVFX/` e `SourceArt/WaveMusic/`
- Este resumo: `Docs/Contexto_Proximo_Chat.md`

Não há tarefa de implementação pendente do chat anterior. As implementações estão locais; o próximo trabalho depende do novo pedido do usuário. Não presumir autorização nova para commit, push, balanceamento adicional ou testes extensos.
