# Lago do Pneu v02 — água e contato do boss

A atualização adiciona movimento à fase 2 já integrada. Oito camadas e arte mestre aprovadas continuam como fontes. As alterações são de materiais, animação de ambiente e contato com a superfície.

## Resultado visual

- A superfície move faixas de reflexos em velocidades diferentes e tem 22 pequenas cristas percorrendo a água. As áreas submersas recebem variação discreta de luz.
- Vegetação intermediária, taboas e primeiro plano têm balanço crescente da raiz para a ponta. Nuvens se movem lentamente em uma faixa limitada, preservando o encaixe das bordas.
- A altura de flutuação do sapo passa de 0,78 para 0,43 unidade acima da linha d'água. Pés e parte inferior do pneu ficam submersos; o recorte segue a altura da água em coordenadas do mundo.
- O pneu tem uma borda de contato na frente e outra atrás, rolagem suave e reflexo comprimido, ondulado e desvanecido pela profundidade. Reflexo e contato desaparecem durante a saída da água.
- A decolagem e a aterrissagem geram ondas que se expandem pela superfície. Respingos têm fitas curvas com espessura variável e partículas menores. Gotas pequenas produzem efeitos proporcionais ao impacto.
- O mesmo relógio escalado controla água, vento e reflexo, mantendo a pausa coerente. Aviso, tiros, dano, dash e quantidade de gotas usam os fluxos da fase 2.

## Arquivos

Shaders: Assets/Shaders/LakeLayerMotion.shader e LakeWaterContact.shader. Materiais nativos correspondentes em Assets/Materials. LakeBackdrop aplica os materiais; LakeSurfaceMotion cria as cristas; LakeBossWaterContact encaixa a apresentação do sapo; LakeSurfaceRipple e LakeWaterFX cuidam de impactos. LakeStageTools.UpgradeWaterMotion atualiza as cenas e o prefab existentes pelo Editor, sem reconstruir a campanha.

Restauração da v01 em Logs/LagoDoPneu/v02/restore. A versão Windows v01 anterior permanece disponível.

## Verificação

Play Mode iniciando no menu real: 31 verificações, 0 erros, incluindo água/vento mudando, reflexo e menisco na flutuação, saída da água, pausa, salto, dano e dash, fúria e fim de capítulo. Log: Logs/LagoDoPneu/v02/play-final.log.

Gravação real da luta em Docs/ArtPreviews/LagoDoPneu_v02/lago-v02.mp4 (sem áudio) e captura gameplay.png. Decodificação completa do MP4 verificada. Comparação de frames da gravação confirmou alterações em 3400 pixels da região de água, 12909 da vegetação e 940 das nuvens, em áreas separadas do jogador e do boss; detalhes em Logs/LagoDoPneu/v02/motion-pixels.json.

Build Windows atualizado em Builds/Windows-LagoDoPneu-v02; pacote completo em Builds/Pacotes/CrocodiloInvaders-LagoDoPneu-v02-Windows.zip. Acesso: FASE 2 · LAGO DO PNEU ou DEV > BOSS FINAL > SAPO DO PNEU. Resultados de build e de integridade do pacote são registrados nos logs v02 após a conclusão.

## Entrega verificada

Build Windows v02 concluído pelo pipeline oficial (178565514 bytes reportados pelo Unity). Inicialização do jogador sem exceções. Pacote com 192 arquivos conferidos por SHA256, 110983961 bytes. Hash do ZIP: 7AF739EBF1FBECDB84D3EADF8CFBDF8336A4C6E3CD8649CC2DD126A4D824E811. Inclui prévia MP4 e captura na pasta Previa. Relatórios em Logs/LagoDoPneu/v02/delivery.json e validation.json. Sem commit/push.
