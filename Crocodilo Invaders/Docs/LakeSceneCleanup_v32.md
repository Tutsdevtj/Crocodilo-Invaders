# Lago do Pneu — cenário e fluxo v32

O canal fixo e o berçário de folhas destoavam da rolagem do lago. O usuário também rejeitou os diálogos de Água parada. Ambos foram retirados das cenas da campanha e do teste do boss.

- Removidos LakeStoryController e LakeStoryWorld das duas cenas, com referência narrativa do WaveController vazia.
- Não surgem captação, entrada do canal, bloqueio, berçário ou escoamento da história.
- A fase começa na onda 1 sem diálogo e mantém hordas/cartas, peixes, piranhas, vegetação e paralaxe.
- O anúncio da onda 10 e a apresentação própria do Sapeneu continuam. Após a entrada, o combate é liberado diretamente.
- A fúria e a entrada da Saperereca seguem pelo aviso e pelo temporizador do combate. Ao vencer, a fase retorna à base pelo fluxo normal, sem cena do canal.
- Fontes, retratos e roteiro v31 ficam arquivados; não foi criado outro roteiro nesta revisão. A tentativa inicial de apenas reposicionar os adereços foi descartada após o esclarecimento do usuário.

LakeStoryTools.RemoveFromScenes aplica a remoção com APIs nativas do Unity. A suíte LakePresentationCleanupValidation verificou campanha (11 checks) e boss (15 checks), com 0 erros: progressão, controles, rolagem, ausência dos adereços/diálogos, entrada do boss, fúria, Saperereca e retorno à base. Save de teste isolado. Capturas em Docs/ArtPreviews/LakeSceneCleanup_v32.

Publicada na PR #26, sobre a base com conquistas, cartas limpas e status no Tab; a suíte acima foi repetida nessa base.

Build Windows-LakeSceneCleanup-v32 concluída (212376878 bytes) na base anterior, inicialização por 8 s sem exceções detectadas.
