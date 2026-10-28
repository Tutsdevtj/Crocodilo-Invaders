# Tropinha do Macaquini — conceito v01

**Integrado à versão jogável v22 em 24/10/2026.** A prévia e os seis recortes agora usam os mesmos sprites de pixel art do jogo, com transparência binária e margens seguras. O registro de produção, em 64 PPU, está em `SourceArt/MacaquiniFighter_v22/layout.json`; as descrições abaixo preservam o estudo inicial. Implementação e testes: [Macaquini lutador v22](../../MacaquiniFighter_v22.md).

Criada a pedido do usuário em **24/10/2026**. Proposta de inimigos pequenos lutadores que acompanham o Macaquini, cumprindo a função de reforços dos gansinhos do Gusini, com identidade e golpes próprios da selva. Conceito visual e de comportamento; não integrado à campanha ou ao prefab do boss.

Abra [a prévia animada](preview.html). O arquivo é independente, contém as imagens e permite escolher o grupo ou um ajudante. A nave demonstra uma esquiva possível. As poses e os deslocamentos são ilustrativos, ainda sem quadros intermediários e colisões finais.

## Identidade

Macacos menores e ágeis, com aproximadamente 30–40% da altura do boss no estudo. Pelos, orelhas e rabos de macaco; olhos verdes ligam a tropinha à infecção alienígena. Fibras amarelas de banana aparecem como faixas, tornozeleiras e cinturão. O Macaquini continua sendo a figura grande e ameaçadora; os ajudantes têm silhuetas próprias, sem copiar sua anatomia musculosa ou sua roupa completa.

## Papéis e ataques

| Ajudante | Reconhecimento | Ataque | Como responder |
| --- | --- | --- | --- |
| **Batedor** | Magro, pelo cobre, orelhas grandes, faixas amarelas nos punhos | Recolhe o punho, trava uma reta e atravessa com um soco de passagem. A rota não acompanha a nave depois do aviso. | Mudar de altura; acertá-lo durante a preparação ou depois do soco. |
| **Acrobata** | Pelo castanho escuro, membros compridos, faixa na cabeça e nas tornozelas | Entra pelo alto, recolhe os joelhos e faz um único chute em arco preso ao cipó pela mesma mão. Alcance local, sem varrer a arena inteira. | Recuar para fora do arco; atacá-lo depois da passagem. |
| **Rolador** | Pelo cinza-azulado, corpo compacto, tufo alto, cinturão amarelo, rabo enrolado | Mostra a intenção se fechando numa bola e avança numa cambalhota em curva curta. Não fica quicando ou perseguindo o jogador. | Sair da curva, usando a altura livre; atacar quando ele se desenrolar. |

Ponto de partida sugerido para um protótipo: Batedor e Acrobata com 2 HP, Rolador com 3 HP; todos causam 1 de dano por investida, no máximo uma vez. Avisos de 0,65–0,9 s e recuperação visível. São valores de partida para testar, não uma promessa de equilíbrio final.

## Entrada e coordenação

**Revisão de cor em 24/10/2026:** Rolador mudou de avermelhado para cinza-azulado nas duas poses, a pedido do usuário, para se distinguir do Batedor cobre. Olhos verdes, pele clara e cinturão amarelo preservados. As quatro poses do Batedor e do Acrobata permanecem idênticas na prévia. Folha anterior em `macaquinhos-conceitos_v01.png`; revisão em `macaquinhos-conceitos_v02.png` e na folha principal.

- Macaquini chama os reforços com um grito e folhas se mexendo nas bordas. Entram pelos cipós e galhos, com poses e rotas de entrada coerentes com a selva.
- Pequenos grupos de 2–3; limite inicial de 3 ajudantes vivos. Combinações variam, sem uma formação idêntica obrigatória.
- Só um ajudante inicia uma investida de cada vez. Os demais guardam posição ou se aproximam sem causar dano.
- Cada ajudante faz sua investida, fica exposto e sai pela borda ou por um cipó, como uma passagem de reforços; não cria uma camada permanente de inimigos.
- Na transformação, no chute do boss e no pêndulo, suspender as investidas dos ajudantes. Retomar após a recuperação, respeitando a recarga de 1,1 s do dash e preservando ao menos uma rota de fuga.
- As hordas anteriores podem apresentar os tipos individualmente. Na luta do boss, a tropinha recombina padrões já conhecidos em vez de ensinar tudo de uma vez.

## Referência e fontes

O projeto atual usa grupos `GusiniSquadron` para passagens em formação e `GusiniCorruptedGoose` para ajudantes de 2 HP com mergulho de direção travada. A proposta reutiliza o princípio de reforços curtos e legíveis, com uma apresentação e golpes diferentes.

`macaquinhos-conceitos.png` é a folha original criada com `image_gen.imagegen`, tomando o boss apenas como referência de estilo e espécie. Na folha: Batedor, Acrobata e Rolador da esquerda para a direita; preparação em cima e ataque embaixo. Os seis recortes em `concept_pose_00.png` a `concept_pose_05.png` e o registro em `layout.json` permitem retomar o estudo sem depender da conversa. `preview-fragment.html` preserva a prévia editável e suas imagens embutidas.

Conceito anterior do boss lutador guardado separadamente em [MacaquiniFighter_v01](../MacaquiniFighter_v01/README.md).
