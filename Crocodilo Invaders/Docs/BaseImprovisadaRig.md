# BaseImprovisada — estado após remoção do personagem

Atualização de 18/10/2026: a implementação anterior do personagem e da câmera de
exploração foi descartada por pedido explícito do usuário. O personagem será
refeito completamente; a câmera será refeita depois.

A cena `Assets/Scenes/BaseImprovisada.unity` permanece no build e como destino de
`Chapter1.asset.nextScene`. Conserva o chão de 60×4 unidades, três colisões estáticas
(chão e duas paredes), fundo e uma câmera ortográfica nativa fixa, sem script.

Foram removidos da cena e de Assets:

- Personagem completo, rig, física e prefab BaseCrocodilo.
- Animações Idle/Run e Animator Controller do personagem a pé.
- Módulos gráficos importados para esse rig e ações Base/Move.
- BaseCharacterController, BaseCameraFraming e suas referências.
- Ferramentas e validadores de setup/rig/Run, para não reconstruírem o sistema antigo.
- Comparativos e traçados gerados para as corridas descartadas.
- Material físico do personagem e estudos antigos de sprites 32×32/64×64 sem uso.

O pixel genérico do chão foi movido, com o mesmo GUID, para
`Assets/Art/Environment/BaseBlockoutSolid.png`. Não depende mais da pasta do personagem.
Os originais do pacote fornecido continuam em `SourceArt/Croc_Rig_Ready`, fora de
Assets. Uma cópia de segurança do material removido está em
`Logs/RemovedBaseSetup-20261018-171046`, ignorada pelo Git e sem uso no jogo.

Na mesma alteração foram excluídos PointsText/KillsText de PlayerHUD e WaveCounter
de WavesUI, incluindo campos, atualizações e overrides obsoletos. O cockpit de
vida/especial, pontuação interna, resultado/ranking e anúncios visuais permanecem.

A validação nativa da limpeza passou na Unity 6000.6.4f1, assim como a validação
estática do cockpit e dos anúncios. Logs: `Logs/BaseAndHudCleanup.log` e
`Logs/BaseAndHudCleanupPlay.log`. **130 verificações de Play Mode passaram**,
além da compilação e conferência de GUIDs/referências do checkout. A rodada inclui a abertura da base
sem o personagem e sem scripts na câmera.

As aprovações e instruções antigas de Idle/Run pertenciam à implementação removida.
Não restaurar aquele rig, controlador ou câmera automaticamente ao iniciar a próxima etapa.

## Cenário v01 (18/10/2026)

A cena recebeu o mapa horizontal completo do kit `BaseImprovisada_Kit_Implementacao_v01`
(ver `Docs/BaseImprovisada_Map.md`). Nessa etapa ainda não havia personagem,
controlador, Idle/Run ou lógica de câmera. Foi colocado um marcador de escala EditorOnly do Croc
simplificado 48×72 no spawn, e a câmera existente mudou apenas o Y (0,9 → 2,4). `Ground`,
`Floor`, `LeftWall` e `RightWall` são os mesmos objetos, redimensionados para o mapa.

## Novo HUB simplificado (18/10/2026, etapa seguinte)

O pedido seguinte substituiu o marcador por `CrocHub.prefab`: corpo físico, VisualRoot
com seis frames do Idle aprovado e CameraTarget estável. Movimento horizontal próprio
e câmera com janela/antecipação/limites, sem restaurar o rig anterior e **sem Run**.
Estado atual, ajustes e testes em `Docs/BaseImprovisadaCamera.md`.
