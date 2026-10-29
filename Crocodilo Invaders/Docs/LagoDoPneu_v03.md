# Lago do Pneu v03 — rolagem e gotas

A fase 2 agora tem rolagem contínua para a esquerda. Nuvens, floresta, margem, água e primeiro plano usam velocidades distintas em unidades por segundo: 0,12; 0,28; 0,52; 0,86; 0,24; 0,43; 1,22. O céu permanece como fundo de cor. A rolagem usa três tiles por camada em posições adjacentes e alterna espelhamento nas bordas. São oito camadas e 22 SpriteRenderers, preservando os arquivos de arte anteriores. Isso garante encontro da mesma borda de textura sem lacunas e mantém o deslocamento ao completar uma largura de tela. O relógio escalado congela a rolagem na pausa.

As gotas receberam uma nova arte em teal, azul-petróleo e aqua clara, baseada na água da cena. A massa arredondada vai na frente da velocidade; a ponta fica atrás. A rotação foi corrigida em 180 graus tanto ao nascer como durante toda a trajetória, incluindo a queda. Há deformação leve conforme a velocidade e um rastro curto, contínuo, na mesma família de cores dos reflexos. Dano, arcos e quantidade de projéteis usam o fluxo existente.

## Arquivos

- Assets/Scripts/LakeStage/LakeBackdrop.cs: tiles e parallax contínuo.
- Assets/Scripts/LakeStage/LakeDroplet.cs: orientação pela massa arredondada, deformação e rastro.
- Assets/Art/LakeStage/gota-lago-v03.png: sprite novo.
- SourceArt/LagoDoPneu_Gameplay_v03/gota-lago-source.png e prompt.txt: fonte original e prompt completo, gerados com image_gen integrado.
- LakeStageTools.UpgradeScrollingAndDrops atualiza as cenas e o prefab pelo Unity, usando UpgradeWaterMotion para preservar a apresentação v02.
- Restauração anterior: Logs/LagoDoPneu/v03/restore.

## Entrega e verificação

Vídeo real: Docs/ArtPreviews/LagoDoPneu_v03/lago-v03.mp4. Captura: gameplay.png na mesma pasta. Build: Builds/Windows-LagoDoPneu-v03/CrocodiloInvaders.exe. Pacote completo: Builds/Pacotes/CrocodiloInvaders-LagoDoPneu-v03-Windows.zip.

O teste de Play Mode parte do menu de produção e verifica deslocamento para a esquerda, velocidades relativas, encontro dos tiles, pausa, uso da arte nova, orientação da frente arredondada na subida/queda e a luta completa. Resultado final e integridade do pacote são registrados em Logs/LagoDoPneu/v03. As versões anteriores permanecem disponíveis.

## Resultado final

40 verificações de Play Mode passaram sem erros, incluindo orientação da parte arredondada ao nascer, na subida e na queda. Build Windows final concluído com 178556712 bytes reportados pelo Unity. MP4 decodificado integralmente sem erros. Jogador iniciou sem exceções. ZIP com 192 arquivos verificados por hash, 112089156 bytes, SHA256 2D271C24A5159665AC1B2A241A3F842043A37E17156E846808A8CD0581414311. Logs finais em v03/play-final.log, build-final.log e delivery.json. Sem commit/push.
