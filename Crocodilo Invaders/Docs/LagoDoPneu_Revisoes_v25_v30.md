# Lago do Pneu — revisões v25 a v30

Estado do jogo desta entrega: **v30**, com pneu voador, fúria/avisos da dupla, piranhas residentes e saltos altos, perseguição/chinelada dos mosquitos, chegada dos bichos por folhas e cenário opaco.

## Organização

1. Boss: ataque de pneu, posição do Sapeneu, expressão/aviso de fúria, olhos e tiros da Saperereca; arte e importadores nativos.
2. Piranhas: nado residente, perseguição, botes mais altos e frequentes, dano e pausa.
3. Hordas: mosquitos suaves e registrados, perseguição, morte por chinelada, fila de ataques, besouros e entrada dos bichos nas folhas.
4. Ambiente: remoção da transparência adicional nas camadas e adereços.
5. Validações e documentação desta série, incluindo fontes e prévias versionadas.
6. Roteiro Água parada, proposto e ainda não integrado ao jogo.

## Conferência

A versão v30 passou por 107 verificações nativas: 22 de atores/efeitos, 10 de piranhas e 75 do percurso completo. A build Windows foi concluída e iniciou por oito segundos sem erros. A organização em commits não altera o comportamento do jogo.

Validações específicas:
- LakeSwampActionsValidation.Run
- LakePiranhaHighLeapValidation.RunFromMenu
- LakeReworkFullRunValidation.RunFromMenu

LakeReworkJourneyValidation acompanha a perseguição atual, inclusive a troca de lado do jogador. Os demais validadores e documentos preservam os testes e entregas da evolução v25–v29.

## Referências

- [Entrega atual e detalhes v30](LakeSwampActions_v30.md)
- [Prévia atual v30](ArtPreviews/LakeSwampActions_v30/lago-acoes-v30.mp4)
- [Pneu v25](Sapeneu_TireAttack_v25.md)
- [Revisão do encontro v26](LagoDoPneu_Rework_v26.md)
- [Suavidade dos insetos v27](LakeMosquitoSmooth_v27.md)
- [Fundo opaco v28](LakeOpaqueBackground_v28.md)
- [Registro e continuidade v29](LakeMosquitoContinuity_v29.md)
- [Roteiro Água parada](Story/LagoDoPneu_AguaParada_v01.md)

As prévias anteriores são registros das respectivas versões. Menções a trabalho sem commit nesses documentos descrevem a entrega original; o usuário autorizou sua organização em seis commits em 31/10/2026. Builds, ZIPs e logs continuam fora do Git.

