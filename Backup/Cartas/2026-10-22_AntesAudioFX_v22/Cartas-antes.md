# Melhorias entre ondas

Depois de cada uma das nove hordas, o jogo congela e oferece três cartas. Escolher
uma aplica o bônus imediatamente e libera a próxima onda após a saída da interface.
A nona escolha prepara o player para o boss da onda 10. A vitória final não abre
outra recompensa. Novas partidas começam sem os bônus da partida anterior.

Cada oferta contém três tipos diferentes, sorteados entre os cinco buffs. A raridade
é sorteada independentemente por carta. Os pesos interpolam conforme a progressão:

| Raridade | Primeira recompensa | Meio da run | Última recompensa antes do boss |
| --- | ---: | ---: | ---: |
| Comum | 70% | 50% | 30% |
| Raro | 24% | 28% | 32% |
| Épico | 5% | 15% | 25% |
| Lendário | 0,9% | 5,45% | 10% |
| Mítico | 0,1% | 1,55% | 3% |

`CardRewardController` captura `RewardProgress` ao abrir a escolha: 0 após a onda 1,
0,5 após a onda 5 e 1 após a onda 9. Usa `(onda concluída − 1) / (número de
recompensas − 1)`, com proteção para runs curtas. O catálogo recebe apenas esse
valor normalizado, sem conhecer WaveController.

Não há garantia de carta rara ou mítica por partida. O reroll usa o progresso capturado na abertura,
sem melhorar artificialmente as chances. Há **um reroll gratuito por escolha**;
ele troca as três ofertas e renova somente na próxima onda concluída. O botão
fica indisponível depois do uso e durante as animações, evitando gastos ou escolhas
duplicadas.

## Coleção atual v21 — sistemas da nave — 22/10/2026

As 25 frentes agora mostram os sistemas instalados na nave, sem o croc:
Ataque, Rajada, Blindagem, Propulsão e Especial. Comum simples em oliva/aço,
rara azul, épica violeta, lendária dourada full art e mítica carmesim full art
com relevo ilustrado e sobreposição localizada na moldura. No Ataque mítico
a borda esquerda permanece inteira e somente a bala atravessa a borda direita.

Sprites individuais em `Assets/Resources/Cards/{família}/{raridade}.png`,
360×536 RGBA, sobre grade lógica 180×268. As cinco pranchas aprovadas ficam em
`SourceArt/CardCollection_v21_Integration/Sheets`. O exportador separa as
silhuetas, remove o fundo externo conectado e padroniza as proporções sem
cortar a arte. Molduras vizinhas que se tocam na prancha são separadas pelo
vão de menor custo. Prévia da transparência em quadriculado:
`Docs/ArtPreviews/CardCollection_v21`.

Aplicação: `node Tools/Art/apply_ship_cards_v21.cjs --apply`.
Conferência sem mutação: `node Tools/Art/apply_ship_cards_v21.cjs --check`.
GUIDs, spriteIDs/fileIDs, recortes 360×536, PPU200, filtro Point, valores,
catálogo, verso, sons e efeitos externos preservados. Foil/holografia e
relevo desta arte são estáticos; não há nova implementação de material dinâmico.

Backup fora do projeto Unity, ao lado de `Gaming portugol`:
`Backup/Cartas/2026-10-22_Anteriores_v21`, na raiz Git. Contém uma cópia das
25 frentes/metadados anteriores em `01_EmUsoNoJogo` e fontes, prévias, prompts,
geradores e guias históricos em `02_ModelosEHistorico`. Cada arquivo foi
conferido por SHA-256. Os históricos antigos foram retirados da pasta do jogo;
o manifesto do backup registra os caminhos de origem e destino.

Teste no menu inicial → DEV ou F8 → Iniciar teste de cartas. O modo ciclo25
exibe a coleção inteira usando a recompensa normal, com rerolls infinitos no DEV.
Detalhes de integração: [CartasIntegracao_v21.md](CartasIntegracao_v21.md).
Histórico completo de arte e documentação anterior preservado no backup.

## Valores das cartas

Os valores do catálogo correspondem aos números impressos nas 25 cartas:

| Buff | Comum | Raro | Épico | Lendário | Mítico |
| --- | ---: | ---: | ---: | ---: | ---: |
| Dano de ataque | 5% | 10% | 15% | 20% | 30% |
| Velocidade de ataque | 5% | 10% | 15% | 20% | 30% |
| Vida máxima | 10% | 15% | 20% | 30% | 40% |
| Movimento | 3% | 5% | 8% | 10% | 15% |
| Dano especial | 5% | 10% | 15% | 20% | 30% |

Os bônus do mesmo atributo somam sobre o valor inicial: 5% + 30% = 35%, portanto
o multiplicador é 1,35. Não se multiplicam entre si. A interface mostra o valor
atual, o valor após escolher e o total acumulado; o rodapé mostra os cinco totais.

Cadência representa tiros por segundo. Com +35%, o intervalo inicial de 0,45 s
passa a `0,45 / 1,35 = 0,333... s`. Dano normal, dano especial e movimento usam
`valor inicial × (1 + bônus total / 100)`. Os projéteis recebem seu dano na criação;
o prefab não é alterado e os tiros que já estão voando mantêm o dano recebido.

A vida máxima é arredondada para cima para preservar HP inteiros: a base de 20 HP
com +25% vira 25 HP (com +3%, vira 21 HP). Só a capacidade acrescentada é preenchida.
Um player com 15/20 passa a 20/25; a carta não cura todo o dano anterior nem revive
um player morto.

O dano conserva frações: um tiro base de 1 com +5% causa 1,05, sem perder o bônus
no arredondamento. `Health` guarda a vida precisa e expõe `Fraction` para barras,
fumaça, avarias e condições de cura. `CurrentHealth` e `HealthChanged(int,int)`
continuam disponíveis para os consumidores existentes; o número exibido arredonda
para cima enquanto a entidade está viva.

## Apresentação e controles

Os versos entram de baixo, com desaceleração e pequenos atrasos entre cartas.
Depois se viram da esquerda para a direita; a troca da imagem acontece no meio
da virada. A entrada dura 0,38 s, cada virada 0,32 s e o atraso entre cartas 0,10 s.
O fundo do jogo fica congelado, desfocado e escurecido. A música continua durante
a escolha; abrir a pausa por cima interrompe música e animações das cartas.

A carta selecionada sobe e cresce levemente. Comuns e raras usam o som existente
de seleção. Épicas, lendárias e míticas recebem os efeitos de raridade descritos
abaixo e sons próprios. A aquisição usa o som de power-up uma vez. A carta escolhida avança
enquanto as outras desaparecem, e a interface sai rapidamente.

- Mouse: passe sobre a carta e clique para escolher.
- Setas esquerda/direita: alternam cartas; Enter confirma.
- Seta para baixo: seleciona o reroll disponível; para cima volta às cartas.
- Teclas 1, 2 e 3: escolhem a carta correspondente; R faz o reroll.
- Controle: navegação e confirmação pelo EventSystem existente.
- Esc: abre a pausa existente, inclusive durante a revelação.

A pausa bloqueia escolhas e rerolls sob sua interface. Continuar volta à escolha,
mantendo o combate congelado. Sair para o menu e morrer cancelam a recompensa e
restauram o relógio da cena; não liberam uma onda após a morte.

### Efeitos das raridades superiores

- Épica: halo e raios de luz roxos, pixels flutuantes e um toque musical curto.
- Lendária: halo dourado mais intenso, mais pixels e um acorde com mais notas.
- Mítica: trovão vermelho nas duas laterais durante a revelação, halo vermelho
  pulsando depois, mais partículas e um som de revelação com impacto grave e textura elétrica.

Os efeitos começam quando a carta vira para a frente, ficam atrás da sua arte e
acompanham seu movimento e transparência. Nenhum Graphic dos efeitos recebe cliques.
O trovão usa os 48 frames de `thunder red/spritesheet-red.png`, fornecido pelo usuário,
copiado sem alterar o PNG. Toca uma vez a 40 fps (1,2 s) e termina; o halo e as
partículas continuam até escolher ou fazer reroll. O arquivo é importado com filtro
Point, sem mipmaps ou compressão. Comuns e raras não recebem esses halos.

O Canvas existente está em Screen Space Overlay. As luzes são desenhadas na UI por
um shader aditivo com coordenadas quantizadas, mantendo o aspecto pixelado e evitando
adicionar câmeras ou iluminação de mundo. Uma camada única de mesh por carta desenha
os pixels flutuantes, sem instanciar um objeto por partícula. O relógio dos efeitos
usa tempo não escalado para funcionar com o combate congelado, mas para explicitamente
quando a pausa é aberta. O som também respeita `AudioListener.pause`.

`CardRarityFeedback` coordena halo, trovão e sua duração; `CardRarityParticles` desenha
as partículas; `CardRarityAudio` toca um dos três sons originais curtos em
`Assets/Audio/CardRewards`. Os sons foram sintetizados para este projeto, não extraídos
de outro jogo. O volume padrão é 0,42 na fonte de cada carta, ajustável no prefab.
A revelação substitui o clique genérico nessas três raridades, para preservar o som
distinto sem somá-lo ao som anterior. Reroll, saída e desativação limpam os efeitos e
interrompem o áudio anterior. Chamadas repetidas de face-up não reiniciam trovão nem som.

## Pesquisa e escolhas de animação

O [material oficial de Slay the Spire](https://www.megacrit.com/press-kits/slay-the-spire/)
serviu como referência para apresentar as opções de carta com espaço para comparação.
As [notas oficiais de Hades](https://www.supergiantgames.com/blog/hades-updates/)
registram melhorias no destaque de upgrades, sons de recompensas raras e apresentação
dos bônus. Aplicamos destaque de raridade, confirmação sonora e valores comparáveis.
Os tempos, a entrada escalonada e a virada descritos acima são escolhas deste projeto,
não uma reprodução de tempos medidos desses jogos.

As [notas oficiais de Hades sobre áudio de upgrades](https://www.supergiantgames.com/blog/hades-welcome-to-hell-update-patch-notes/)
também registram sons diferentes conforme o tipo de melhoria. Isso motivou separar
o caráter musical das três revelações; os timbres e as cores acima são escolhas próprias.

## Organização e ajustes

- `UpgradeCardCatalog`: dados, chances e sorteio; `Assets/Data/UpgradeCards.asset`
  contém os 25 registros e os campos **Early Run Weights / Late Run Weights**.
  Os cinco pesos são editáveis no Inspector e não precisam somar 100. O catálogo
  interpola os pesos e sorteia pelo total. Pesos negativos, não finitos ou todos
  iguais a zero geram erro claro; pesos zero nunca são sorteados.
- `PlayerRunUpgrades`: acumula porcentagens e aplica os atributos sobre as bases
  capturadas no player da cena, incluindo seus overrides. Reinicia por partida.
- `CardRewardController`: coordena ondas, escolha, reroll, bloqueio de input e pausa.
- `CardRewardView`: anima a interface, sons e textos comparativos.
- `CardOptionView`: apresentação e foco de cada carta.
- `WaveController`: espera explicitamente pela recompensa antes de avançar. O tempo
  `Reward Transition Duration`, padrão 0,35 s, começa após concluir a escolha.

`Assets/Prefabs/UI/CardRewards.prefab` está nas cenas `PlayerMovementTest` e
`StageOneWaves`. Os versos e as frentes usam a mesma área visual de 180×268;
o recorte cobre a tela nativa completa. As texturas são importadas com limite 512,
filtro Point, sem mipmaps ou compressão. O catálogo contém caminhos,
e só carrega as frentes oferecidas, guardando-as para reutilização durante a sessão.
Não há carregamento inicial das 25 texturas nem novas dependências de animação.

## Validação

A validação original em Play Mode cobriu limites das cinco raridades, 5.000
ofertas com tipos distintos, soma de bônus, cadência, dano fracionário em colisão
2D real, dano especial, vida máxima e reset. O fluxo percorre as nove escolhas,
o boss e seus reforços, loading de nova partida, morte durante recompensa e saída
pela pausa. Também verifica navegação por setas/Enter, único reroll por escolha,
bloqueio da onda e suspensão da animação durante a pausa. Imagens de conferência e
logs ficam na pasta ignorada `Logs`; a compilação do projeto completo é verificada
separadamente dos testes.

A progressão atual foi validada no Editor com 100.000 sorteios em cada progresso
0 / 0,5 / 1, mais tipos distintos, pesos somando 10 e configurações inválidas.
Resultados comum/raro/épico/lendário/mítico: início **69,761 / 24,105 / 5,117 /
0,915 / 0,102%**; meio **49,895 / 27,863 / 15,105 / 5,574 / 1,563%**; final
**30,030 / 31,731 / 25,152 / 10,047 / 3,040%**.
O comando **Crocodilo → Validate Roguelite** repete essas verificações; a validação
de build também as executa. Play Mode real percorreu nove recompensas com reroll,
pausa, aplicação única de carta, boss, pontuação e derrota, usando abates e tempo
acelerados para exercitar o fluxo. Isso não avalia o balanceamento de uma partida manual.

Os efeitos têm validação adicional: três auras e sons distintos, reprodução única
do trovão, 48 frames importados, shaders compilados e geometria das partículas
presente no CanvasRenderer. Verifica pausa do relógio e áudio, limpeza ao fazer
reroll e sair, ausência de brilho em comuns/raras e cliques sem interceptação.
As capturas com as três raridades superiores são exemplos de conferência visual,
sem forçar essas cartas no sorteio da partida.
