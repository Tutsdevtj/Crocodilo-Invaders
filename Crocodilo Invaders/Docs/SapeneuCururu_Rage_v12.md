# Sapeneu Cururu: fúria e Saperereca v12

O boss foi renomeado para **Sapeneu Cururu** no prefab, cena de teste, barra de vida e menu DEV. Os nomes internos antigos de classes e caminhos permanecem para preservar referências existentes.

**Saperereca, a mulher do sapo**, começa como um pequeno sapo animado no HUD, com flor de vitória-régia e piscadas. Na metade exata da vida de Sapeneu (100/200 HP), a fúria dispara uma única vez, mesmo se ele for curado depois. Ele interrompe o ataque atual, limpa as gotas anteriores, treme e ganha uma cor quente mais agressiva. A parceira salta do ponto em que estava no HUD, segue um arco de 1,25 s, sai da interface e aterrissa na superfície como personagem do mundo. O nome dela aparece durante a entrada.

Ela alterna entre dois pontos na metade direita do lago. Cada salto é anunciado por 0,65 s e segue um arco de 0,9 s. A mira da rajada fica fixa no início do aviso; a aterrissagem emite três gotas com a mesma arte e física da água existente. Salto e gotas causam 1 HP e respeitam dash/invulnerabilidade. Um intervalo compartilhado de 0,65 s entre seus contatos evita dano simultâneo da rajada. Sapeneu aguarda o salto e as gotas dela antes de começar um novo golpe. Em fúria, mantém quatro linguadas e sete gotas por impacto.

A parceira não cria outra barra, não bloqueia tiros e compartilha a duração do encontro. Usa Rigidbody2D cinemático, movimento em FixedUpdate e o teste varrido LakeHit já existente. Recorte na água e reflexo usam LakeWaterContact. Pausa congela movimento, HUD e entrada; derrota limpa personagem, anúncio e projéteis, inclusive durante o salto da interface.

O mato e as duas repetições agora ficam na última sorting layer do mundo, LakeForeground/32000, acima de todos os elementos do cenário e personagens. A interface original usa Screen Space Overlay, desenhada depois do mundo, e preserva sua ordem relativa original. Para capturar o vídeo com uma câmera, o teste usa temporariamente a layer LakeHUD, imediatamente depois de LakeForeground; ao encerrar, retorna a Overlay.

## Instalação e arte

`SapeneuRageTools.Install` importa seis quadros nativamente, cria os sprites e material, atualiza o prefab e as cenas sem reconstruir a campanha e registra a sorting layer pelo SerializedObject do Unity. `LakeStageTools.Install` chama essa atualização ao fim. Fonte, prompts e hashes em `SourceArt/SapeneuCururu_Rage_v12`.

## Validação

Teste Play Mode pela entrada real DEV > BOSS FINAL > SAPENEU CURURU: **32 checks, zero erros**. Testa limite exato de 50%, HUD e poses, salto de entrada, pausa durante a transição, instância única, mira travada, alternância, dano real de gota, quatro linguadas/sete gotas, passagem repetida pela metade sem duplicar e derrota durante entrada. Regressão da água: **24 checks, zero erros**, incluindo peixes, piranhas, pausa, dano, fuga e limpeza. Total final desta rodada: **56 checks, zero erros**. Logs e backups em `Logs/LagoDoPneu/v12`; capturas e vídeo em `Docs/ArtPreviews/SapeneuCururu_Rage_v12`. MP4 final decodificado sem erros.

Referências consultadas: skill `create-2d-physics/references/legacy-2d-physics.md`, [Rigidbody2D.MovePosition](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/rigidbody2d/moveposition) e [ScreenPointToLocalPointInRectangle](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/recttransformutility/screenpointtolocalpointinrectangle). Implementação segue as APIs 6000.6 e os componentes existentes.

**Não commitar, fazer push ou atualizar PR até o usuário finalizar.** Dificuldade final depende de playtest humano.

Build `Windows-SapeneuCururu-Rage-v12` concluída pelo Unity 6000.6.4f1; inicialização do executável sem exceções. ZIP `Builds/Pacotes/CrocodiloInvaders-SapeneuCururu-Saperereca-v12-TESTE-LOCAL-Windows.zip`: 191 arquivos verificados por hash, 124658992 bytes, SHA256 `32250062ba410945d797cdf76c37a699ee0fee941540e755a2d6f59e7fc3fe20`. HEAD `f16c05ec6783d96cf50a4ef2acb6e940f8ac2137` e index vazio preservados.
