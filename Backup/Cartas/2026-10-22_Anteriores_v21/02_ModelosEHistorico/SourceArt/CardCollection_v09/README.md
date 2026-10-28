# Coleção completa de melhorias — v09

Pedido: salvar o modelo final de Ataque e criar as outras famílias no mesmo padrão.
O modelo aprovado e fontes foram arquivados em CardsModel_Approved_v08_20261021.
Ataque foi copiado desse arquivo sem mudar nenhuma base, máscara ou texto por hash.

Quatro novos atlas originais gerados por IA usam Croc e os masters aprovados como
referência de identidade/estilo. Cada atlas tem grade 3×2; cinco variantes comuns,
raras, épicas, lendárias e míticas, e uma célula vazia. Sem molduras, números ou
foil embutido. Os cinco recortes nativos de cada família ficam junto do atlas.
Não são cartas do jogo até a etapa de integração.

- AttackSpeed: armas de disparo rápido e munição, de canhão duplo a gatlings
  energizadas e múltiplos rotores. Título RAJADA; atributo CADÊNCIA.
- MaximumHealth: placas e suporte de vida cada vez mais reforçados, preservando
  a cruz médica. Título BLINDAGEM; atributo VIDA MÁXIMA.
- MoveSpeed: o avião do Croc com propulsores, turbinas e afterburners crescentes.
  Título PROPULSÃO; atributo MOVIMENTO.
- SpecialDamage: emissor de energia, capacitores, anéis de foco e esfera de plasma.
  Título ESPECIAL; atributo DANO ESPECIAL.

As míticas têm a expressão de boca aberta e equipamentos mais exagerados. Só
elas sobrepõem partes de equipamentos à moldura. Cada grupo tem recortes próprios;
o compositor remove partículas isoladas e exige ligação com a área interna da arte.
Os modelos abaixo da mítica têm toda a arte contida na janela.

Frames e acabamentos usam o mesmo modelo aprovado: comum fosca; rara reflexo
neutro no aro; épica arte com reflexo neutro e moldura foil; lendária moldura
neutra/fundo holográfico/Croc protegido; mítica prismático na arte, escovado na
moldura e realce no personagem. Tipografia v08 idêntica em tamanhos e margens.
Porcentagens conferidas contra UpgradeCards.asset, sem alterar regras ou valores.

Exportar e validar:

```powershell
node 'Crocodilo Invaders/Tools/Art/build_card_collection_v09.cjs'
node 'Crocodilo Invaders/Tools/Art/verify_card_collection_v09.cjs'
```

Entregas em `Docs/ArtPreviews/CardCollection_v09`: 25 cartas PNG 360×536, camadas
base/materiais/texto, cinco pranchas por raridade, prancha das 25, comparação das
míticas, prévia interativa com seletor de família e relatório de conferência.
O arquivo HTML é um fragmento autossuficiente com todos os dados embutidos; ele
reutiliza cinco contextos WebGL ao trocar a família. PNGs conservam pixels sem
perda; só a cópia inline das bases usa WebP para manter a prévia abaixo de 1 MB.

Validação: 25 valores corretos; shader e seleção das cinco famílias; comum fosca;
reflexos ativos e desligáveis; Croc lendário protegido; leitura responsiva em
736/360 px; Ataque aprovado inalterado por hash. Conferência visual das quatro
novas pranchas realizada. Sem edição em Assets ou teste de integração em Unity.
`manifest.json` lista valores, arquivos, atlas e hashes dos PNGs.
