# CROCODILO INVADERS — BASE IMPROVISADA
## Bíblia de produção de pixel art, animações e integração do cenário

**Versão:** 1.0 — 18/10/2026  
**Estado:** conceito visual aprovado; artes de produção ainda serão criadas  
**Referência visual principal:** `base_militar_ao_pôr_do_sol.png` (imagem de conceito aprovada)  
**Cena Unity prevista:** `Assets/Scenes/BaseImprovisada.unity` / nome da cena **`BaseImprovisada`** (sem underline no nome técnico).  
**Objetivo deste documento:** saber exatamente **o que pedir ao Claude para desenhar**, **o que animar**, **o que pode ser reutilizado**, **o que é cenário estático** e **o que fica bloqueado para a progressão**. Não é instrução para criar tudo de uma vez.

---

## 0. O conceito que estamos preservando

### Sentimento da cena

A base é um **refúgio improvisado de sobreviventes após a destruição da base principal**. A guerra ainda está acontecendo ao longe; o pessoal se reorganiza, cuida dos feridos, remenda aviões e tenta reerguer sua força aérea. A ambientação mistura:

- **Crepúsculo roxo/laranja no horizonte**: cor e contraste arcade, visual bonito e memorável.
- **Luzes quentes e acolhedoras perto das pessoas**: esperança, sobrevivência, solidariedade.
- **Destroços, fumaça e ruínas distantes**: consequências reais dos acontecimentos anteriores.
- **Hangar grande mas maltratado, oficina improvisada**: o coração emocional e mecânico da base.
- **Obras paradas, tapumes e caminhos bloqueados**: promessas de expansão, sem liberar tudo imediatamente.
- **Vida discreta**: soldados conversando, um médico, um civil e um soldado feridos; poucos personagens de cada vez.

**Regra de mundo, inegociável:** todos os soldados, médicos, guardas e civis genéricos são **HUMANOS**. Os personagens importantes B. Crocodilo e Goosini são os antropomórficos; não povoar a base com soldados-crocodilo nem gansos genéricos.

**Regra de história:** Goosini está resgatado e trabalhando na oficina; nesta cena ele é um **ganso aviador não corrompido**, com **capacete/óculos de aviador, cachecol vermelho** e referências ao visual roxo. O avião dele deve continuar **danificado** (avarias e resíduos verdes inativos da corrupção); não usar a aeronave intacta como se nada tivesse acontecido.

### O que não fazer

- Não entregar uma única pintura enorme como cenário jogável. **Separar fundo, parallax, construção, tiles, props, NPCs e FX em assets independentes.**
- Não copiar a quantidade enorme de pessoas do concept literalmente. É um *moodboard*, não a contagem definitiva de NPCs.
- Não substituir a estética do jogo por militarismo realista, marrom monocromático ou pós-apocalipse sombrio demais. **Continua sendo um arcade 2D cartunesco com personalidade.**
- Não construir todas as instalações funcionais desde o começo.
- Não gerar pixels e animações automaticamente em massa antes de aprovar o **master** de cada família de assets.

### Legenda das tabelas

| Coluna | Significado |
|---|---|
| **P0** | Indispensável na primeira base jogável com identidade visual |
| **P1** | Polimento forte: trazer depois que a primeira versão funcionar |
| **P2** | Guardar para desbloqueios, próximos capítulos ou decoração tardia |
| **Estático** | Uma arte ou conjunto de tiles, sem frames de animação |
| **Loop** | Pequena animação que se repete, com frames especificados |
| **One-shot** | Animação breve disparada quando algo acontece |
| **Mov. Unity** | Sprite estático que se move/oscila por script; não requer spritesheet de movimento |
| **Estados** | Duas ou mais variantes visuais, trocadas conforme progresso |

---

## 1. Organização do espaço: como o concept vira um mapa jogável

### Layout visual, da esquerda para a direita

```text
FUNDO DISTANTE        Montanhas / base destruída / fumaça / céu de pôr do sol
                       ↔ aviões distantes e holofotes

CAMADA DE CENÁRIO    [ENFERMARIA] --- [HANGAR / GOOSINI] --- [ACAMPAMENTO] --- [OBRAS FECHADAS]
                                         bancada / aeronaves        fogueira         tapumes

CAMADA JOGÁVEL       ========= CHÃO E CAMINHO CONTÍNUO PARA O CROCODILO ==============

PRIMEIRO PLANO      sucata escura / caixotes parciais / pedras / grades baixas
```

O conceito aprovado tem visual de **dois ou três níveis**, escadas e plataformas. Isso é ótimo para *profundidade*, mas **não tornar obrigatória a navegação vertical na V1**: o personagem a pé está começando com movimentação horizontal, rig e idle. Primeiro, uma rota principal contínua e transitável entre enfermeira/feridos, hangar, acampamento e portão de expansão. Passarelas superiores, subsolo e plataformas podem existir inicialmente como **cenografia inacessível**; depois ganham colisões/navegação quando pulo ou escadas estiverem implementados.

### Zonas e conteúdo inicial

| Zona | Estado na V1 | O que o jogador vê | Interatividade inicial |
|---|---|---|---|
| **A. Enfermaria** | Montada às pressas | Tenda médica, cama, caixas, humano ferido e médico | Opcional: diálogo curto |
| **B. Hangar central** | Aberto, danificado | Hangar, Goosini, aeronave avariada, ferramentas, luzes, bancada | Goosini conversa; a loja vem depois |
| **C. Acampamento** | Pequeno e vivo | Fogueira, 2 humanos conversando, suprimentos | Diálogo ambiental opcional |
| **D. Comunicação provisória** | Cenográfico | Antena/rádio e caixa de equipamento | Posteriormente missões/ranking |
| **E. Setor de expansão** | **Fechado** | Tapume, lona, fita de isolamento, sucata, cadeado/placa | Não passa; pode exibir mensagem de construção |
| **F. Plataformas elevadas/subsolo** | **Cenográficas** | Torres, escadas, passarelas, sala inferior iluminada | Sem necessidade de acesso na V1 |

> **Decisão de produção:** entregar primeiro A+B+C+portão de E. As demais zonas podem usar silhuetas de cenário. Um cenário menor, cheio de pequenos detalhes coerentes, é melhor que um mapa imenso e vazio.

### População-alvo da primeira cena

**Em cena, além do jogador Croc e de Goosini:**
- 1 médico humano;
- 1 soldado humano ferido;
- 1 civil humano ferido;
- 2 soldados humanos conversando;
- 1 sentinela humana em local distante/elevado **opcional**.

Ou seja: **5 humanos essenciais + 1 sentinela opcional**, não 20 bonecos. Figurantes adicionais para outras zonas só quando forem abertas. Não é necessário todo humano andar pelo HUB.

---

## 2. Direção técnica: escala, pixel art, camadas e exportação

### 2.1 Primeiro validar escala com o personagem existente

O rig aprovado de B. Crocodilo tem **master de 96 × 184 pixels**. Isso **não define automaticamente** o tamanho dos tiles ou a altura dele na tela. Antes de criar tilesets e NPCs finais:

1. Abrir a cena Unity de blockout e colocar o prefab do Croc na escala real do jogo.
2. Escolher o enquadramento da câmera e validar a leitura do Croc em uma captura real de gameplay.
3. Desenhar uma **folha-teste de escala**: Croc, um humano, porta do hangar, caixa e três tiles de piso.
4. Só então congelar `Pixels Per Unit`, tamanho de tile e alturas relativas.

**Proposta inicial, não decisão irrevogável:** tiles de **32×32 pixels nativos** (ou 16×16 se o teste mostrar vantagem), objetos maiores compostos por múltiplos tiles. A escala de todos os personagens deve ser comparada com o Croc no mundo Unity, não com a resolução absoluta dos PNGs. Não reduzir sprites até virarem borrões; sempre Point Filter e escala coerente.

### 2.2 Parâmetros de arte

- Perspectiva: **side view de platformer 2D**, com profundidade no cenário, não top-down/isométrico.
- Texturas: **pixel art nativa**, clusters legíveis, sem blur, anti-aliasing ou textura pintada de alta resolução simulando pixels.
- Luz: cenas de entardecer, contraste quente/frio; postes e oficinas iluminarão pontos de interesse.
- Paleta compartilhada e estável: roxos/azuis escuros do fundo, laranjas/âmbar de janelas/fogueira, oliva/lona, cinza azulado do metal, vermelho discreto em alertas. Não forçar uma paleta rígida antes da folha-teste.
- Assets do cenário são **modulares**, sem texto embutido e sem personagem “pintado no fundo”.
- Plano de fundo sem colisão. **Colisores são configurados na Unity**, a partir do blockout, não de efeitos decorativos.
- Exportar PNG com alfa real. Não usar o azul do concept como preenchimento dos sprites.
- Evitar dependência obrigatória de URP/Light2D se o projeto não estiver configurado para isso. O brilho pode começar com sprites/overlays aditivos controlados na Unity.

### 2.3 Parallax recomendado

| Camada | Conteúdo | Movimento sugerido da câmera | Arte separada? |
|---|---|---|---|
| BG0 | Gradiente do céu, sol distante | 0–0,05× | Sim |
| BG1 | Nuvens e faixas de cor | 0,08–0,15× | Sim |
| BG2 | Montanhas/serras distantes | 0,18–0,25× | Sim |
| BG3 | Ruínas/base destruída, fumaça, fachos distantes | 0,25–0,40× | Sim, fumaça separada |
| BG4 | Torres, postes e árvores/pedras distantes | 0,45–0,65× | Sim |
| WORLD | Chão, hangar, props, luzes, humanos, Goosini | 1× | Sim |
| FG | Sucata/grades próximas e silhuetas decorativas | 1,05–1,15× (sutil) | Sim |

Os fatores são **valores de partida para ajuste no Play Mode**, não leis da física; o parallax precisa respeitar os limites da câmera e não revelar bordas vazias. Para repetição horizontal, bordas de cada faixa devem ser compatíveis; caso contrário, usar segmentos longos com margens extras.

---

## 3. CATÁLOGO COMPLETO DE ASSETS PARA PEDIR AO CLAUDE

### PACOTE 00 — Guia de estilo e escala (a fazer PRIMEIRO)

| ID | Elemento / arquivos sugeridos | Animação | Prioridade | Critério de aprovação |
|---|---|---|---|---|
| STYLE_01 | `base_palette.png` + paleta editável | Estático | P0 | Laranja quente vs. roxo frio, coerente com Crocodilo Invaders |
| STYLE_02 | `base_scale_sheet.png` (Croc existente, humano, caixa, porta, tile) | Estático | P0 | Proporções aprovadas na câmera real da Unity |
| STYLE_03 | `base_material_test.png` (pedra, lona, metal, madeira, luz) | Estático | P0 | Pixel density uniforme |
| STYLE_04 | `base_layering_mockup.png` (amostra cenário montado) | Estático | P0 | Não achatar a arte em uma imagem só |

**Regra:** nenhum lote gigante de sprites antes de aprovar STYLE_02.

### PACOTE 01 — Céu, horizonte e parallax

| ID | Asset | Animação | Prioridade | Observação |
|---|---|---|---|---|
| BG_01 | Céu dusk roxo/rosa/laranja e gradiente | Estático | P0 | Arquivo independente, largura para scroll |
| BG_02 | Sol baixo próximo ao horizonte | Estático | P0 | Não prender às montanhas |
| BG_03 | Nuvens longas distantes (3–4 variantes) | Mov. Unity | P0 | Deslizar muito devagar |
| BG_04 | Silhuetas de montanhas/serras | Estático | P0 | Faixas repetíveis/segmentadas |
| BG_05 | Cidade/base destruída distante em silhueta | Estático | P0 | História contada pelo cenário |
| BG_06 | Prédios rachados, antenas e torres distantes | Estático | P0 | Módulos para variar horizonte |
| BG_07 | Colunas de fumaça sobre as ruínas (2 variantes) | Loop, 6–8 quadros | P1 | Movimento lento, não esconder céu |
| BG_08 | Avião minúsculo em silhueta (2 modelos) | Mov. Unity | P1 | Translação; propulsão/fumaça opcionais |
| BG_09 | Holofote distante/feixe de procura | Mov. Unity | P1 | Giro/varredura lenta, overlay |
| BG_10 | Árvores, rochas e postes distantes | Estático | P1 | Parallax intermediário |
| BG_11 | Faixa de primeiro plano: metal retorcido, trilhos, entulho | Estático | P1 | Silhueta escura, não encobrir o player |
| BG_12 | Faixa de rio/água costeira ou terreno baixo sob o acampamento | Estático + loop sutil opcional | P2 | Só se for compatível com o mapa final |

### PACOTE 02 — Terreno/tileset de plataforma e estrutura

| ID | Asset | Animação | Prioridade | Observação |
|---|---|---|---|---|
| TILE_01 | Solo principal: centro, bordas, cantos, topo, fundo | Estático | P0 | Autotile/tiles manuais, evitar gaps |
| TILE_02 | Pedra rachada e terra compactada (variações) | Estático | P0 | Material jogável de base |
| TILE_03 | Piso de concreto/aeródromo gasto | Estático | P0 | Interior e frente do hangar |
| TILE_04 | Plataforma de aço e rebites | Estático | P0 | Uso modular no hangar |
| TILE_05 | Vigas verticais e suportes de metal | Estático | P0 | Repetíveis em várias larguras |
| TILE_06 | Escadas, corrimãos e passarelas | Estático | P1 | Inicialmente decorativas; depois navegáveis |
| TILE_07 | Paredes subterrâneas / corte de rocha | Estático | P1 | Usar em níveis inferiores futuros |
| TILE_08 | Grades, cercas, arame e postes | Estático | P0 | Também servem em áreas bloqueadas |
| TILE_09 | Cantoneiras, rachaduras, sujeira, ferrugem | Estático | P1 | Overlays separados de colisão |
| TILE_10 | Entulho removível e montes de pedra | Estados | P0 | Intacto e removido/desbloqueado |

**Exportação:** atlas(s) com grade fixa + PNGs isolados se necessário; índice dos tiles; pivot e célula; indicar quais peças são apenas decoração. Nunca incluir colisões “desenhadas” na imagem.

### PACOTE 03 — Hangar central (o ponto focal)

| ID | Asset | Animação | Prioridade | Observação |
|---|---|---|---|---|
| HANG_01 | Estrutura de arco metálico principal (segmentos) | Estático | P0 | Construção modular por partes |
| HANG_02 | Colunas, treliças, suportes e travessas | Estático | P0 | Devem encaixar no arco |
| HANG_03 | Chapas de telhado, remendos e lona verde/ocre | Estático | P0 | Danos reparáveis mais tarde |
| HANG_04 | Interior traseiro: painéis, paredes e vigas | Estático | P0 | Separado da frente do hangar |
| HANG_05 | Piso de oficina com manchas de óleo | Estático | P0 | Sobrepor ao TILE_03 |
| HANG_06 | Porta/cortina de hangar aberta e fechada | Estados | P1 | Não animar abrir sem gameplay |
| HANG_07 | Lona/estandarte com emblema Croc/equipe | Estático ou loop 3–4 quadros | P1 | Símbolo da unidade, NÃO espécie dos humanos |
| HANG_08 | Corrente e gancho de guindaste | Estático + Mov. Unity opcional | P1 | Movimento sutil de balanço |
| HANG_09 | Armários, gavetas e painéis de ferramenta | Estático | P0 | Modular, 2–3 variantes |
| HANG_10 | Bancada mecânica principal | Estático | P0 | Espaço para trabalhar o Goosini |
| HANG_11 | Carrinho de ferramentas vermelho, caixa de peças | Estático | P0 | 2 variantes reutilizáveis |
| HANG_12 | Escada portátil, cavalete, suporte de hélice | Estático | P0 | Peças independentes |
| HANG_13 | Luminárias pendentes industriais (2 modelos) | Estático + flicker Unity | P0 | Separar lâmpada e halo |
| HANG_14 | Fiação elétrica, extensões e cabos pendurados | Estático | P1 | Não preencher toda a tela de ruído |
| HANG_15 | Sinalização de reparo/perigo em placas pequenas | Estático | P1 | Sem texto baked; usar ícones, texto Unity quando precisar |

### PACOTE 04 — Aeronaves e mecânica do Goosini

| ID | Asset | Animação | Prioridade | Observação |
|---|---|---|---|---|
| PLANE_01 | Avião roxo do Goosini, **avariado após Fase 2**, estacionado | Estático em peças | P0 | Reutilizar referência da versão já existente; manter cratera, resíduos verdes apagados |
| PLANE_02 | Capô/carenagem aberta, painel removido, peças separadas | Estados | P0 | Avião em manutenção, composição modular |
| PLANE_03 | Hélice da aeronave parada + 2–4 frames de giro lento | Loop opcional | P1 | Não deixar motor funcionando pleno enquanto avariado |
| PLANE_04 | Trem de pouso, cavaletes, asas e acessórios | Estático | P0 | Separáveis se precisarem trocar o estado de reparo |
| PLANE_05 | Avião cinza do B. Crocodilo em repouso | Estático | P1 | Só se couber sem lotar o hangar |
| TOOL_01 | Chave inglesa, martelo, chave de fenda, alicate | Estático (peças) | P0 | Versões na mão do Goosini e sobre a mesa |
| TOOL_02 | Motor desmontado, cilindro, tubulações, hélice extra | Estático | P0 | Kit de componentes mecânicos |
| TOOL_03 | Peças pequenas: porcas, engrenagens, parafusos | Estático | P1 | 1 folha reutilizável |
| FX_01 | Faíscas de solda / impacto de ferramenta | One-shot, 4–6 quadros | P0 | Pouco frequentes, arco curto |
| FX_02 | Fumaça do motor/peça quente | Loop, 4–6 quadros | P1 | Muito sutil |
| FX_03 | Brilho momentâneo de metal | One-shot, 2–3 quadros | P1 | Opcional, não virar confete |

**Atenção:** o Goosini fica **reparando o avião ou uma peça sobre a bancada**, não dirigindo o avião dentro do hangar. O estado visual da aeronave pode melhorar por progressão de capítulo sem obrigar a refazer toda a estrutura do hangar.

### PACOTE 05 — Enfermaria e sobreviventes feridos

| ID | Asset | Animação | Prioridade | Observação |
|---|---|---|---|---|
| MED_01 | Tenda médica improvisada: teto/lona/postes/parede | Estático | P0 | Construída em módulos, símbolo de cruz separado |
| MED_02 | Bandeira de primeiros socorros, cruz vermelha estilizada | Estático ou loop sutil | P0 | Elemento de leitura rápida |
| MED_03 | Maca de campanha e cama (2 modelos) | Estático | P0 | Humanos ficam como entidades separadas |
| MED_04 | Lençóis/cobertores, almofadas e rolos de pano | Estático | P0 | 3 variantes pequenas |
| MED_05 | Caixas de remédio, maleta, kit de primeiros socorros | Estático | P0 | Props reutilizáveis |
| MED_06 | Soro/suporte, bandagens e frascos | Estático | P1 | Pequenos e discretos |
| MED_07 | Luminária quente e lanterna da enfermaria | Estático + luz Unity | P0 | Deve iluminar rosto de NPC sem dificultar leitura |
| MED_08 | Cadeira/carrinho e divisória de lona | Estático | P1 | Montagem modular |

**Direção narrativa:** feridos são humanos, representados com respeito e sem gore; atmosfera triste, mas não grotesca. O posto médico é um lugar de cuidado e reconstrução.

### PACOTE 06 — Acampamento, área social e suprimentos

| ID | Asset | Animação | Prioridade | Observação |
|---|---|---|---|---|
| CAMP_01 | Fogueira/barril aquecido, metal e lenha | Loop, 6–8 quadros | P0 | Foco quente da área social |
| CAMP_02 | Brasas e faíscas ocasionais | One-shot, 3–4 quadros | P1 | Baixa quantidade |
| CAMP_03 | Fumaça discreta da fogueira | Loop, 6 quadros | P1 | Sem cobrir NPCs |
| CAMP_04 | Banco, caixote usado como assento, tambor | Estático | P0 | 3–4 variantes reutilizáveis |
| CAMP_05 | Mesa de campanha pequena | Estático | P1 | Para cartas, canecas, alimentos |
| CAMP_06 | Tenda simples, lona pendente e cordas | Estático | P0 | Parecer remendada |
| CAMP_07 | Pilhas de caixotes e mantimentos | Estático | P0 | Variações e módulos separados |
| CAMP_08 | Sacos de areia e barris | Estático | P0 | Também servem a portões e torres |
| CAMP_09 | Canecas, tigelas, garrafa térmica, cobertor | Estático | P1 | Detalhes de vida cotidiana |
| CAMP_10 | Gerador pequeno com caixa de energia | Estático + loop 2–3 opcional | P1 | Justifica fios e iluminação |
| CAMP_11 | Pontos de luz: postes, cordões de lâmpadas | Estático + flicker Unity | P0 | Âmbar; iluminar seletivamente |
| CAMP_12 | Quadros de avisos/documentos sem texto fixo | Estático | P1 | UI adicionada no jogo |

### PACOTE 07 — Segurança, comunicação e áreas bloqueadas

| ID | Asset | Animação | Prioridade | Observação |
|---|---|---|---|---|
| SEC_01 | Portão improvisado de grades e madeira | Estados | P0 | Fechado, parcialmente aberto, removido |
| SEC_02 | Tapumes de obra, cavaletes listrados, placas com ferramenta | Estados | P0 | Reutilizar em vários setores |
| SEC_03 | Sucata empilhada, pedras, pallets bloqueando passagem | Estados | P0 | Separar objetos para sumir no desbloqueio |
| SEC_04 | Lona de construção e armação incompleta | Estados | P0 | Vazio → em obras → estrutura pronta |
| SEC_05 | Cadeado, correntes, sinalização de acesso restrito | Estático | P1 | Decoração do bloqueio |
| SEC_06 | Sirene/luz de segurança vermelha | Loop 2 quadros / Unity | P1 | Uso esporádico; não poluir a cena |
| SEC_07 | Torre de observação e plataforma de vigia | Estático | P1 | Cenográfica, sentinela opcional |
| SEC_08 | Rádio, antena, pratos/peças de comunicação | Estático | P1 | Estação provisória, não missão completa ainda |
| SEC_09 | Holofotes de perímetro | Estático + Mov. Unity | P1 | Apenas poucos, direcionados para longe |
| SEC_10 | Cercas com arame farpado, lona furada | Estático | P1 | Primeiro plano e limites laterais |
| SEC_11 | Estado de instalação ativa/reparada | Estados | P2 | Somente nas expansões futuras |

**Regras dos bloqueios:** indicar claramente que algo pode ser desbloqueado no futuro, mas não parecer uma porta de missão funcional na V1. O objeto bloqueador e sua decoração devem ser separados dos assets de cenário; a colisão liga/desliga via Unity, não pelo desenho.

### PACOTE 08 — Personagens HUMANOS (NPCs genéricos)

| ID | Personagem | Arte obrigatória | Animação | Prioridade |
|---|---|---|---|---|
| NPC_H01 | **Médico/enfermeira humano(a)** | Master de corpo inteiro, faixa/crachá de primeiros socorros | Idle de 3–4 quadros; gesto de cuidar one-shot 4 quadros | P0 |
| NPC_H02 | **Soldado humano ferido** | Master sentado/deitado, capacete/curativo, roupa suja | Respiração discreta 2–4 quadros | P0 |
| NPC_H03 | **Civil humano ferido** | Master em maca, roupa civil diferente de soldado | Respiração discreta 2–4 quadros | P0 |
| NPC_H04 | **Soldado humano conversando A** | Master de corpo inteiro, militar comum | Gesto de conversa 3–4 quadros, loop lento | P0 |
| NPC_H05 | **Soldado humano conversando B** | Variação genuína de uniforme/cabeça, outra postura | Gestos de resposta 3–4 quadros | P0 |
| NPC_H06 | **Sentinela humana distante** | Master pequeno com capacete e postura vigilante | Estático ou 2 quadros de atenção | P1 |
| NPC_H07 | Carregador de suprimentos humano | Master, mãos/caixa | Andar com caixa 6 quadros | P2 |
| NPC_H08 | Civil ajudante humano | Master, roupa civil | Idle + gesto de ajuda | P2 |

**Restrições muito importantes:**

1. Todos os oito acima são **humanos, nunca crocodilos ou gansos**.
2. **Não produzir NPCs H07/H08 na primeira leva.** Reservados para crescimento da base.
3. Personagens do mesmo grupo podem compartilhar paleta ou esqueleto, mas devem ter **variações visíveis de cabeça, rosto e postura** para não parecerem clones.
4. Usar o **Croc existente na cena Unity para calibrar a escala**, e não redesenhar um novo Croc.
5. Feridos podem animar muito pouco. Um loop simples de respiração já comunica vida; menos é mais.
6. O par de soldados conversando precisa ter *timing desencontrado*, coordenado no Unity, para não gesticularem em sincronia.
7. NPCs não precisam de ciclo completo de caminhada se forem permanecer no mesmo posto. Economizar produção.

### PACOTE 09 — Goosini mecânico (NPC importante)

| ID | Asset | Animação | Prioridade | Observação |
|---|---|---|---|---|
| GUS_N01 | Master corpo inteiro do Goosini como mecânico | Estático-base | P0 | Ganso branco, capacete roxo/óculos azuis, cachecol vermelho; fiel aos portraits |
| GUS_N02 | Módulos de cabeça, pescoço, tronco, asas/braços, pernas | Rig modular | P0 | Construir para trabalho na bancada |
| GUS_N03 | Idle de oficina | Loop 4–6 quadros | P0 | Olhar peça, respirar, ajustar postura |
| GUS_N04 | Trabalhar com chave inglesa | Loop 6–8 quadros | P0 | Ferramenta separada e encaixada à asa/mão |
| GUS_N05 | Martelar/reparar peça | One-shot 4–6 quadros | P1 | 1–2 impactos, acompanhados por FX separado |
| GUS_N06 | Pausar trabalho e olhar para Croc | One-shot 3–4 quadros | P1 | Para início de diálogo |
| GUS_N07 | Expressão de orgulho/irritação engraçada | Variante localizada | P2 | Para conversas futuras |

**Planejamento de comportamento:** `WorkIdle → WorkLoop → LookAtPlayer → Dialogue → WorkLoop`. Não começar por todas essas animações: aprovar master, modularizar, depois fazer `WorkIdle` e um único loop com chave inglesa. Nenhuma corrupção visual ativa; Goosini está recuperado.

### PACOTE 10 — Iluminação e FX de ambiente

| ID | Asset/efeito | Animação | Prioridade | Observação |
|---|---|---|---|---|
| LIGHT_01 | Halo âmbar de luminária, PNG alfa separado | Estático, intensidade via Unity | P0 | Dentro de hangar/enfermaria |
| LIGHT_02 | Halo da fogueira e reflexo no chão | Estático, intensidade via Unity | P0 | Sutil flicker |
| LIGHT_03 | Halo vermelho para luz de alerta | Estático, intensidade via Unity | P1 | Moderado, não vibrar o tempo todo |
| LIGHT_04 | Cone de holofote distante | Estático, rotação Unity | P1 | Camada de fundo |
| FX_04 | Fumaça de destroço distante | Loop 6–8 quadros | P1 | Separar fontes de fumaça |
| FX_05 | Pó flutuando no hangar iluminado | Mov. Unity / partículas discretas | P2 | Não distrair |
| FX_06 | Poeira breve ao caminhar em solo seco | One-shot 3–4 quadros | P2 | Mais tarde, quando a corrida existir |
| FX_07 | Faísca da oficina (reuse FX_01) | One-shot | P0 | Não criar sprite duplicado |
| FX_08 | Luz da lâmpada piscando | Controle Unity | P1 | Não requer spritesheet |
| FX_09 | Tecido de bandeira balançando | Loop 3–4 quadros | P1 | Opcional por performance/tempo |
| FX_10 | Refração de calor do fogo | Opcional; sprite estático ou shader leve | P2 | Não implementar se afetar pixels |

**Iluminação no começo:** preferir halos independentes e pequenas variações via script. Os sprites não podem vir com o brilho pintado sobre todos os objetos vizinhos, pois isso inviabiliza movimentação/ciclo noturno e substituição de props.

### PACOTE 11 — Interface mínima e marcadores de estado (não confundir com cenário)

| ID | Asset | Animação | Prioridade | Observação |
|---|---|---|---|---|
| UI_01 | Ícone de interação `E` / gamepad em pixel art | Pop curto 2–3 quadros opcional | P1 | Pode usar solução temporária TMP |
| UI_02 | Ícone oficina (chave inglesa) | Estático | P1 | Para indicar Goosini interativo |
| UI_03 | Ícone instalação bloqueada/em obras | Estático | P0 | Pequena indicação perto de portão |
| UI_04 | Marcador missão/hangar | Estático | P1 | Quando seleção de missão existir |
| UI_05 | Balão de fala neutro | Estático | P2 | Pode reaproveitar DialogueRunner existente |

**Não encomendar HUD e menus completos agora.** A UI real de missões, módulos e upgrades será planejada quando essas funcionalidades forem implementadas.

---

## 4. Onde usar animação e onde NÃO gastar animação

### Animações essenciais na V1 (menos é mais)

| Elemento | Tipo | Referência de custo visual | Implementação |
|---|---|---|---|
| Goosini trabalhando | Loop modular 6–8 quadros | Alta importância narrativa | Animator/rig do NPC |
| Goosini idle | Loop modular 4–6 quadros | Alta | Animator/rig do NPC |
| Fogueira | Loop 6–8 quadros | Alta | Sprite sheet/Animator |
| Soldados conversando A/B | 3–4 quadros cada, cadência lenta | Média | 2 animações não sincronizadas |
| Soldado ferido / civil ferido | 2–4 quadros cada | Média | Respiração discreta |
| Médico ajudando | 3–4 quadros ou gesto one-shot | Média | Sem deslocamento |
| Faíscas da oficina | 4–6 quadros one-shot | Média | Disparo intermitente com áudio eventual |

### Efeitos que podem ser feitos na Unity SEM spritesheet

- Nuvem deslizando lentamente.
- Aviãozinho distante cruzando o céu.
- Gancho/corrente balançando discretamente.
- Leve variação de intensidade do halo da lâmpada.
- Luz vermelha de alerta piscando.
- Holofote distante girando.
- Objetos de obra alternando estados ao desbloquear.

### Efeitos estáticos sem necessidade de animação

Montanhas, ruínas, quase todos os escombros, telhado, pilares, passarelas, mesas, caixas, tendas, torres, o sol, chão, paredes, placas de madeira, portas de cenário, malas médicas e painéis elétricos.

**Critério:** animar o que comunica vida, movimento de personagem ou mudança narrativa; o resto pode ficar parado sem prejudicar a cena.

---

## 5. Desbloqueios e evolução visual da base

A base deve usar *visual states* (partes substituíveis), não uma imagem inteira diferente por capítulo.

| Local | V1 — logo após resgatar Goosini | Próximo estado desbloqueado | Estado futuro |
|---|---|---|---|
| Hangar | Danificado, improvisado, funciona para missões | Oficina reorganizada, mais equipamentos | Hangar expandido e mais naves |
| Oficina | Bancada simples de Goosini, peças espalhadas | Módulos/upgrade e peças mais avançadas | Oficina profissional de motores |
| Enfermaria | Tenda e macas provisórias | Cobertura melhor, equipamento | Centro médico estruturado |
| Comunicação | Antena precária em posto improvisado | Terminal de missões funcional | Torre de operações |
| Área direita | Tapume + entulho + andaimes | Obra parcial | Nova instalação desbloqueada |
| Subsolo | Acesso fechado e luz distante | Escada/entrada liberada | Laboratório/armazenamento futuro |
| Pontos de luz | Lanternas e fios improvisados | Fiação consolidada | Luzes elétricas consistentes |
| População | Poucos humanos exaustos | Mais moradores e atividade | Base mais movimentada, sem virar multidão |

### Estratégia de estados visuais

Para cada estrutura desbloqueável, pedir separadamente:

1. `facility_<id>_locked.png` — tapume, lona, sucata, terreno vazio.
2. `facility_<id>_building.png` — andaime, ferramentas, materiais, construção.
3. `facility_<id>_open.png` — estrutura funcional.

**Pedir na V1 apenas o estado `locked` dos setores futuros**. `building`/`open` ficam catalogados para quando o sistema for desbloqueado. A Unity pode consultar flags de progresso (`CampaignProgress.Has(...)`) e carregar a variante correta. O jogo já possui `gusini_rescued` e `chapter1_completed` no fluxo de capítulo, mas não inventar flags novas no pacote de arte.

---

## 6. Ordem de produção recomendada: pacotes que devemos realmente pedir

| Ordem | Lote Claude | Entrega aprovada antes de continuar | Estado |
|---:|---|---|---|
| **00** | Style guide + escala com rig real | Comparativo Croc × humano × porta × tile | **Primeiro** |
| **01** | Céu e parallax BG0–BG4 | Camadas separadas, loop/scroll sem bordas vazias | Segunda prioridade |
| **02** | Tileset de solo + pedra + concreto + aço | Chão jogável sem gaps, pixel density aprovada | Terceira |
| **03** | Estrutura modular do hangar | Montagem de exemplo do hangar central | Quarta |
| **04** | Aeronave roxa avariada + oficina/props | Composição com posição de Goosini | Quinta |
| **05** | Goosini mecânico: MASTER único | Só depois: módulos/idle/work | Sexta |
| **06** | Tenda médica + cama + suprimentos | Montagem de enfermaria | Sétima |
| **07** | Set de acampamento + fogueira | Cena pequena de acampamento | Oitava |
| **08** | Humanos genéricos (5, cada um em sublote) | Master de cada função antes dos frames | Nona |
| **09** | Tapumes, portões, entulho, luzes, halos | Pelo menos duas áreas fechadas visualmente | Décima |
| **10** | FX, fumaça, aviões distantes, holofotes | Polimento sem distrair gameplay | Por último |
| **11** | Novas construções e novos humanos | Só quando a campanha liberar | **Não fazer agora** |

### O menor conjunto que já faria o cenário parecer o concept

- Um céu/parallax montado e distante com fumaça;
- Um chão modular de pedra/concreto;
- Hangar central, telhado/estrutura e 3–4 luminárias;
- Avião roxo avariado e bancada;
- Goosini trabalhando;
- Tenda médica com um paciente e médico;
- Fogueira e dois soldados humanos conversando;
- Dois locais bloqueados com tapume;
- Pouca sucata no primeiro plano.

Essa é a **vertical slice visual**. Tudo além disso deve entrar quando a cena estiver interessante e funcional.

---

## 7. Padrão de entrega para TODOS os pacotes do Claude

Cada pedido deve resultar em algo semelhante a:

```text
BaseArt_<LOTE>_v01.zip
├── sprites/
│   ├── static/
│   ├── modular/
│   └── animations/
│       └── <anim_id>/frame_000.png ...
├── tilesets/                  # apenas se necessário
├── previews/
│   ├── composition.png
│   ├── actual_scale.png
│   └── animation.gif          # apenas se animado
├── manifest.json
└── README.md
```

**No `manifest.json`, incluir:**

- Nome/ID único de cada asset.
- Dimensões em pixels **nativos**.
- Paleta e outline usados.
- Coordenadas de pivô (pixels + normalizado).
- `Pixels Per Unit` recomendado **somente depois** do teste de escala na Unity.
- Sorting Layer / ordem de sobreposição sugerida.
- Camada de parallax e fator pretendido.
- Frames, FPS e duração do loop/one-shot (quando animado).
- Direção do personagem (direita/esquerda) e ponto de contato com chão.
- Dependências entre sprites (por exemplo, ferramenta e mão do Goosini).
- Se possui colisão prevista (sim/não), mas **não** gerar polígonos de colisão cegamente a partir da transparência.
- Versões/estados (bloqueado, em obras, liberado) quando aplicável.

**Não fazer:**

- PNG gigante achatado contendo todos os elementos.
- Efeito de luz pintado permanentemente sobre móveis, NPCs e paredes.
- Letras pequenas com texto fictício ilegível integrado nos painéis.
- Sprites em tamanhos/paletas completamente diferentes entre lotes.
- GIF como único arquivo final; é apenas preview.
- Animação independente de cada frame alterando design, rosto, proporção ou roupa.
- Refazer Croc, Goosini ou avião a partir de descrição genérica se já existe asset aprovado.

**Verificação de qualidade:**

1. Abrir PNG sobre fundo xadrez para validar alfa/transparência.
2. Conferir composição em escala real de gameplay, não só com zoom 6×.
3. Verificar clusters, contornos, pixels soltos e costuras entre módulos.
4. Conferir compatibilidade de tiles, cantos e bordas.
5. Verificar se sprites que repetem horizontalmente não deixam emendas.
6. Reproduzir animações em loop e comparar com master.
7. Confirmar que todos os arquivos anunciados no manifesto existem.
8. Validar a composição na Unity após cada lote, antes de encomendar o próximo.

---

## 8. Planejamento de integração na Unity (para o Codex, não para o Claude)

### Estrutura sugerida

```text
Assets/
├── Art/BaseImprovisada/
│   ├── Background/
│   ├── Tilesets/
│   ├── Hangar/
│   ├── Airplanes/
│   ├── Infirmary/
│   ├── Camp/
│   ├── Construction/
│   ├── Characters/
│   └── FX/
├── Animations/BaseImprovisada/
├── Prefabs/BaseImprovisada/
└── Scenes/BaseImprovisada.unity
```

### Camadas de renderização sugeridas

```text
00_BackgroundSky
10_BackgroundMountains
20_BackgroundRuins
30_BackgroundNear
40_WorldRear
50_PropsRear
60_NPCAndPlayer
70_PropsFront
80_VFX
90_Foreground
100_UI
```

O sorting real será decidido no projeto; os números acima servem apenas para tornar a separação clara. Usar `SortingGroup` no Croc e Goosini se necessário para que seus módulos não intercalem incorretamente com o cenário.

### Objetos e componentes de gameplay

- `BaseImprovisada` é a cena usada pelo fluxo do capítulo, já prevista em `Chapter1.asset`.
- `BaseCrocodilo` (rig e movimento) é independente do player voando.
- Chão e paredes recebem `Collider2D` próprios, independentes das imagens decorativas.
- Componentes de parallax movem apenas camadas BG, nunca o chão jogável.
- Goosini, NPCs genéricos e portões serão prefabs independentes.
- Construções bloqueadas terão lógica por ID de instalação e estado persistido quando implementadas.
- Portões podem ter um Collider2D bloqueando a passagem antes do desbloqueio; a arte não decide sozinha a colisão.
- Luzes decorativas não devem obrigar uma conversão de pipeline de renderização sem avaliação.
- Criar prefabs reutilizáveis para fogueira, luz, caixotes e bandeiras; não duplicar animações em cada cópia.

---

## 9. Prompt-base para abrir CADA novo lote com o Claude

> Estou criando os assets de produção da Base Improvisada de Crocodilo Invaders. Use a imagem de concept aprovada e a Bíblia `Crocodilo_Invaders_Base_Improvisada_Art_Bible.md`. Nesta tarefa faça **APENAS o PACOTE [NOME E IDs]**. O estilo é pixel art arcade cartunesca de platformer lateral 2D, ao pôr do sol, com iluminação âmbar e parallax. **Todos os civis, médicos, guardas e soldados genéricos são HUMANOS; somente B. Crocodilo e Goosini são antropomórficos.** Entregue assets separados com transparência, preview montada, escala testável, manifest e README. Não entregue o cenário achatado. Não gere animações antes de aprovar o master do objeto/personagem. Não faça elementos de pacotes futuros. Para cada asset listado, siga o campo animação/estático do documento. **Comece pelo master/folha de estilo deste lote e aguarde a aprovação antes de produção em massa.**

**Para lotes de personagens, acrescentar:** preservar proporções e silhueta frame a frame; construir o movimento sobre blockout aprovado; manter articulações corretas; nunca preencher espaços vazios entre braços/corpo com cores opacas; não importar a estética visual de personagens secundários sobre Croc e Goosini.

---

## 10. Checklist de aprovação da primeira Base Improvisada

### Aparência

- [ ] O céu tem o pôr do sol roxo/laranja do concept.
- [ ] A profundidade é percebida ao mover a câmera (parallax em múltiplas camadas).
- [ ] O hangar central é claramente o foco da tela.
- [ ] A iluminação âmbar destaca Goosini, enfermaria e fogueira.
- [ ] A guerra é perceptível ao longe sem transformar o HUB em outro combate.
- [ ] As estruturas parecem improvisadas, danificadas, reparáveis.
- [ ] Não há excesso de detalhes em áreas de passagem.

### Pessoas e história

- [ ] **Todos os NPCs genéricos são humanos.**
- [ ] B. Crocodilo mantém o sprite/rig aprovado.
- [ ] Goosini está visualmente reconhecível e não corrompido.
- [ ] A aeronave de Goosini permanece avariada, com resíduos verdes inertes.
- [ ] Há ao menos um humano civil ferido e um soldado humano ferido.
- [ ] Existem dois soldados humanos conversando discretamente.
- [ ] O médico atua na tenda.
- [ ] Nenhuma região tem multidão excessiva.

### Animação e técnica

- [ ] Fogueira funciona em loop.
- [ ] Goosini trabalha com ferramentas em loop sem deformações.
- [ ] NPCs de fundo não ficam todos sincronizados.
- [ ] Luzes podem oscilar sem alterar texturas originais.
- [ ] Não existem frames com buracos ou membros desconectados.
- [ ] Sprites PNG têm transparência real e filtro Point.
- [ ] Assets visuais não precisam de colisores automáticos.
- [ ] Há espaço de caminhada livre para o Croc no plano jogável.

### Progressão

- [ ] Setores futuros estão fechados com tapumes e sucata.
- [ ] Pelo menos duas áreas sugerem expansão sem oferecer interação prematura.
- [ ] Áreas acessíveis são poucas, próximas e úteis.
- [ ] As construções futuras podem ser reveladas trocando prefabs/estados, não redesenhando o mapa inteiro.

---

## 11. Decisão recomendada agora

**Próximo pedido ao Claude: `PACOTE 00 — Style guide + folha de escala`.**

Antes de autorizar o cenário inteiro, colocar lado a lado **o Croc existente de 96×184, um humano genérico, um tile de chão, uma porta e um segmento do hangar** na escala jogável, e comparar com a câmera real do Unity. Assim a gente trava a escala e evita que um NPC fique com 40 pixels de altura enquanto o Croc ocupa metade da tela.

**Depois:** parallax do crepúsculo → tiles jogáveis → hangar → avião avariado + oficina → Goosini → enfermaria → acampamento → humanos → bloqueios → FX.

O objetivo não é gerar 100 sprites imediatamente. É construir um cenário que, mesmo pequeno, já transmita: **“perdemos nossa base, mas ainda estamos vivos e vamos reconstruir.”**
