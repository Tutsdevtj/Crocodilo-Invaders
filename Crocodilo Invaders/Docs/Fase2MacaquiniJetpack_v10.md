# Macaquini: jetpack e transformação — v10

A primeira forma voa com dois motores montados na casca da banana. A segunda usa o cipó. A vida, a pontuação, a barra original do Gusini e a progressão do capítulo continuam nos controladores existentes.

## Ataques da primeira forma

- **Rajada aérea:** mira travada com aviso, três tiros da arma-banana orgânica enquanto o boss muda de altura. Os Animation Events originais liberam cada projétil.
- **Rasante incandescente:** anuncia uma diagonal, cruza a arena de jetpack e deixa uma faixa quente temporária. Há tempo para sair da linha antes da ignição e uma recuperação após a passagem.
- **Descarga dos reservatórios:** lança dois pequenos reservatórios de combustível para cima da arena. Eles anunciam duas colunas estreitas de fogo, separadas por um corredor seguro. Cada coluna só pode causar um acerto por ativação.

Os três ataques aparecem em sequência na abertura; depois entram em sacos embaralhados que evitam repetição consecutiva. A segunda forma retorna à rajada, ao leque de sete bananas e aos cipós da selva.

## Dano e transformação

Os jatos acompanham a velocidade e a inclinação do corpo. Depois de perder aproximadamente um quarto da vida, o motor começa a falhar e fumar. Com mais dano surgem rachaduras brilhantes e faíscas, a emissão de fumaça aumenta e o som oscila com a perda de potência.

O primeiro esgotamento limpa as ameaças e inicia uma sequência invulnerável de 6,2 segundos: retirar o topo, revelar o rosto, segurar as duas bordas, abrir a casca com as duas mãos, soltar a casca com o jetpack e alcançar o cipó. A arma fica temporariamente guardada no quadril. A casca inferior permanece no corpo. Não há pontos nem abate nessa transição; só a derrota final concede os 40000 pontos.

## Arte e integração

- 16 desenhos completos de voo e 16 células de transformação em `Assets/Art/JungleStage/Macaquini_v10`.
- As 24 imagens aprovadas de balanço/disparo da segunda forma foram copiadas da v08 sem alterar os arquivos PNG.
- Chamas têm quatro quadros por motor, ancorados nos bocais. Fumaça e faíscas usam duas tiras adicionais de quatro quadros, com variação de tamanho e transparência ao longo da vida das partículas.
- Um único SpriteRenderer desenha o personagem completo. O rig antigo fica desativado; os membros não são encaixados durante a animação.
- Recorte por célula preserva o topo lançado e o conjunto descartado. A transformação usa margem de 512 px e registro pelo quadril; o voo usa 384 px. RGB nativo preservado, sem redução de paleta, filtro Point e importação sem compressão.
- A célula 10 da folha de transformação contém um jetpack duplicado na imagem gerada e foi excluída da seleção de produção, prolongando a célula seguinte. A fonte é mantida para rastreabilidade.
- Fontes e prompts: `SourceArt/Macaquini_v10`. Os anexos do usuário orientaram os gestos; as referências visuais usadas para criar o personagem foram os sprites do próprio projeto.

Instalação: `MacaquiniJetpackTools.Install`. Validação nativa: `MacaquiniJetpackValidation.Run`. Build Windows de teste: `MacaquiniJetpackTools.Build`.

## Teste jogável

Cena `Assets/Scenes/Tests/MacaquiniJetpackTest.unity`, separada da lista de cenas da campanha. O prefab real `Assets/Prefabs/Boss/Macaquini.prefab` também é usado pela fase da selva.

Controles normais para mover e atirar. F1/F2/F3 forçam os três ataques da forma atual; J remove 45 HP; K esgota a forma atual; I alterna proteção do jogador; H cura o jogador; R reinicia a cena; ESC usa a pausa normal. O jogador começa com 240 HP somente nesta bancada. O teste usa uma pasta própria de progresso e cancela o envio ao ranking.

O build de teste inclui as cenas normais para permitir o fluxo após a derrota final. Sem commit ou publicação automática.

## Verificação final

Passaram 25 verificações nativas do jetpack, 93 da campanha da selva e 49 da regressão do Gusini. O teste usa Animation Events reais, deslocamento do Rigidbody2D, dano dentro e fora das faixas, pausa, duas barras de vida, transformação, descarte do motor e limpeza das ameaças. O validador do checkout conferiu 2162 GUIDs sem duplicação e os metadados dos assets.

Vídeo real e capturas: `Docs/ArtPreviews/Macaquini_v10`. Build: `Builds/Windows-Macaquini-Jetpack-v10/MacaquiniJetpack.exe`.
