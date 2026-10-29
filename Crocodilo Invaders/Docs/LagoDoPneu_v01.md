# Lago do Pneu — fase 2, v01

Cena: Assets/Scenes/StageTwoLake.unity. Entrada no menu principal: FASE 2 · LAGO DO PNEU. DEV > BOSS FINAL > SAPO DO PNEU inicia diretamente a luta sem ranking. Cena auxiliar do Editor: Assets/Scenes/Tests/SapoDoPneuTest.unity.

## Combate

SapoDoPneuCombat implementa IBossCombatController e reutiliza Boss, Health, EnemyMovement para entrada e WaveController para progresso. Depois da entrada, somente o controlador do sapo escreve a posição do Rigidbody2D. Física 2D existente do projeto (Rigidbody2D/Collider2D), sem novo pacote.

Sequência: flutuar 2,4 s; antecipar 0,9 s com marcador na água; saltar por 1,45 s entre duas posições do lado direito; comprimir no impacto 0,25 s; recuperar por 0,7 s. O destino fica travado antes da decolagem. Altura limitada pela câmera. A pose mestre aprovada usa squash/stretch para antecipação, subida e impacto; esta versão não é uma spritesheet desenhada quadro a quadro.

Cada impacto lança cinco gotas azuis em trajetórias balísticas a partir da superfície, com núcleo azul e destaque ciano/branco. As gotas sobem, caem e desaparecem ao voltar à água; respingos e partículas são apenas decorativos. Abaixo de 40% de vida, descanso menor e sete gotas. Dano de gota: 1; contato com o boss durante salto: 2, com intervalo de 0,8 s. As consultas de cápsula cobrem o percurso entre passos físicos; gotas atingindo um dash invulnerável são consumidas. Derrota ou morte do jogador encerra o combate e remove as ameaças.

## Cenário e progressão

Oito PNGs aprovados copiados para Assets/Art/LakeStage, sem alterar fontes. LakeBackdrop mantém registro dos offsets da prévia e linha d'água a 28% da altura do viewport. Movimento ambiente pequeno e limitado por camada evita exibir emendas de tiles ainda não finalizadas. Não há rolagem horizontal infinita nesta primeira versão.

Nove hordas, cartas, inimigos menores existentes, controles, loading, pausa e HUD do jogador vêm da cena funcional da campanha. BossHUD cria uma variante da barra original para o sapo, com retrato e nome próprios. Sem redesenho das barras do jogador.

Chapter2Lake registra lago_do_pneu_completed e chapter2_completed, com retorno à BaseImprovisada pelo ChapterFlow existente. A cena StageTwoJungle, Macaquini e acesso dele pelo DEV foram preservados.

## Fontes e validação

Arte original do sapo: SourceArt/SapoDoPneu_Master_v01. Cenário: SourceArt/LagoDoPneu_Layers_v01 (prompts, manifesto e offsets). Gota gerada com image_gen integrado, baseada na referência do lago e acabamento do Bombardiro. Nenhuma licença externa presumida. Sprite novo em Assets/Art/LakeStage/gota.png.

Instalador nativo: Assets/Editor/LakeStageTools.cs. Teste real de Play Mode: Assets/Editor/LakeStagePlayValidation.cs. Logs e ponto de restauração dos arquivos compartilhados em Logs/LagoDoPneu. Resultados finais são registrados em validation.json; não confundir a presença do script de teste com um teste aprovado.

Referências de implementação consultadas: unity-cli/SKILL.md, create-2d-physics/SKILL.md e references/legacy-2d-physics.md, ui-ugui/SKILL.md; documentação Unity de Rigidbody2D.MovePosition e Physics2D.CircleCast. A consulta varrida segue o padrão OverlapCapsule já usado pelo Macaquini.

- https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Rigidbody2D.MovePosition.html
- https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Physics2D.CircleCast.html

Para testar: abrir a cena pelo Editor, ou gerar jogador Windows com LakeStageTools.BuildWindows. Não há alterações de save real nos testes automáticos. Ajuste fino de dificuldade ainda depende de uma partida humana completa.

## Resultado da integração — 28/10/2026

- Compilação e instalação de assets no Unity 6000.6.4f1 concluídas. Cena e prefab gerados pelo Editor, sem edição manual de YAML.
- Play Mode partindo do menu de produção: 24 verificações, 0 erros (Logs/LagoDoPneu/play-final.log e validation.json).
- Regressão do menu e acesso às duas formas do Gusini/Macaquini: 44 verificações, 0 erros (Logs/LagoDoPneu/menu-regression.log; resultado original em Logs/DeveloperMenu/validation.json).
- Captura real da luta: Docs/ArtPreviews/LagoDoPneu_v01/gameplay.png. Fonte e nome da barra conferidos visualmente.
- Windows: Builds/Windows-LagoDoPneu-v01/CrocodiloInvaders.exe. Usar a pasta completa ou o ZIP, não somente o EXE.

Verificações automáticas não substituem uma partida humana completa para ajustar dificuldade. Animação atual do sapo usa a arte mestre aprovada, movimento físico e deformação de pose; não foi prometida animação desenhada com novos frames.

Build Windows concluído pelo pipeline CrocodiloWindowsBuild, 178555875 bytes reportados pelo Unity. Teste de inicialização do jogador permaneceu ativo sem exceções. ZIP completo em Builds/Pacotes/CrocodiloInvaders-LagoDoPneu-v01-Windows.zip: 190 arquivos conferidos por SHA256, 107722814 bytes. SHA256 do pacote: A01BD1E5C68605C108EF061B57D54F0DCE4ED7DD494F6449818C406348A97E47. Relatório local em Logs/LagoDoPneu/delivery.json.
