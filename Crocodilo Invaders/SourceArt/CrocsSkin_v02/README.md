# Crocs skin v02: Crocs a jato

Pedido do usuário (10/11/2026): refazer a Crocs com asas de jato e trocar o capacete por um no estilo
Crocs, com jibbitz de guerra presos nos furos.

- Base: o casco v01 (`SourceArt/CrocsSkin_v01/shoe-native.png`) e os pixels do piloto dos 26 quadros de
  produção, com piscadas, recuo e expressões de dano preservados.
- Desenhado em código, na grade nativa de 160×120, por `Tools/Art/prepare_crocs_skin_v02.cjs`. Não houve
  geração por IA.
- Novo (v02.1, encaixe refeito): asa enflechada parafusada na lateral do sapato (base com parafusos), com roundel de estrela; asa do lado oposto saindo de trás do calcanhar e
  deriva vertical no calcanhar; turbina com chama que varia por quadro (Idle curta, Fly/Special longa,
  Hurt falhando).
- Capacete de borracha estilo Crocs: domo verde com sete furos, aba mais longa na frente, rebite da tira
  no lado e jibbitz de guerra (estrela, plaqueta de identificação, granada, bomba). No sapato: medalha,
  bala e estrela.
- O capacete acompanha a cabeça por correlação da silhueta em cada quadro (`head` no manifest); o casco
  segue a regra de deslocamento vertical da v01.
- Removidos: clarões do avião, faíscas azuis do especial e a porta frontal da v01.

`helmet-native.png` e `jet-shoe-native.png` são as camadas soltas. Para regenerar, rode
`node Tools/Art/prepare_crocs_skin_v02.cjs` e depois o Unity (os PNGs mantêm GUIDs, pivô 0,45/⅓ e 100 PPU).
