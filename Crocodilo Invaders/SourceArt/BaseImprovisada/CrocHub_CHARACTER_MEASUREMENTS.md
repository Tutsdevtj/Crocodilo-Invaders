# B. Crocodilo (HUB, simplificado) — Medidas e análise técnica

As **medidas** foram feitas no master nativo e são valores exatos. As **decisões** são escolhas artísticas, marcadas como tal. Estimativas estão indicadas como estimativas.

## 1. Resolução

| Item | Valor |
|---|---|
| Imagem fornecida | 1024×1536, RGB, sem alfa, fundo preto (0–4 em cada canal) |
| Tamanho médio do "pixel" na imagem (medido) | ≈16,6 px na horizontal e ≈16,35 px na vertical |
| Variação real das células (medida) | 12 a 20 px; a grade da imagem gerada **não é regular** |
| Grade inferida | 62 × 95 células |
| Personagem na grade (medido) | **42 × 69 px nativos** |
| Canvas final (decisão) | **48 × 72 px**: 3 px de margem lateral e 2 px no topo (o idle sobe 1 px) |
| Fator de escala aproximado | ≈16,5× (não inteiro e irregular, por isso a grade não foi um resize simples) |

### Método para inferir a grade

1. Calculei o perfil de bordas (soma das diferenças entre pixels vizinhos) em x e em y.
2. A análise de Fourier do perfil achou o período dominante: 16,6 px em x e 16,35 px em y.
3. Como as células variam de 12 a 20 px, escolhi as linhas da grade por programação dinâmica. Ela seleciona linhas de borda com espaçamento entre 12 e 21 px que maximizam a força das bordas, com uma penalidade pequena para espaçamentos longe do período médio. Assim a grade segue as irregularidades da imagem em vez de impor um passo fixo.
4. Cada célula virou 1 pixel nativo, pela mediana da região central da célula. Usei 50% interno, para ignorar as bordas borradas.
5. O resultado foi conferido sobreposto à imagem original (o recorte do rosto encaixa nos blocos) e em `previews/original_vs_master.png`.

### Separação do fundo

- O fundo tem valores de 0 a 4. O contorno do personagem é (8–13, 10–15, 12–19): é escuro, mas é outra cor.
- Só as células com soma RGB ≤ 15 viraram transparentes. Os contornos ficaram 100% preservados, e as aberturas entre braço e corpo e entre as pernas ficaram com transparência real.
- Sem halo: como o fundo foi removido célula a célula, não sobra borda escura.

### Limpeza (decisão)

- 126 pixels isolados, ruído da geração, foram trocados pela cor dominante dos vizinhos.
- Ficaram protegidos: contorno, olho, dentes e fivela.
- Duas cores raras sem função visual foram fundidas com a vizinha mais próxima.

## 2. Silhueta (medida, em px nativos; % da altura total de 69)

| Medida | px | % da altura |
|---|---|---|
| Altura total (topo do capacete → sola) | 69 (linhas 2–70) | 100% |
| Largura total (luva da frente → ponta do focinho) | 42 (x 3–44) | 61% |
| Cabeça + capacete (topo → queixo, linha 33) | 32 | 46% |
| Capacete (linhas 2–15) | 14 | 20% |
| Largura do capacete (linha 10) | 24 | — |
| Comprimento do focinho (canto da boca x≈19 → ponta x 44) | ≈26 (estimativa: o canto da boca não é um ponto nítido) | 38% da largura |
| Olho | x 21–24, y 16–18 (8 px amarelos + pupila de contorno) | — |
| Ombros (linha 35) | 31 | 45% |
| Tronco (linha 38, sem as luvas) | 34 | — |
| Cintura / cinto | linhas 41–47 | 57–65% a partir do topo |
| Largura no cinto (linha 41, com braços) | 35 | — |
| Pernas (virilha → sola) | 20 (linhas 51–70) | 29% |
| Largura das duas pernas (linha 56) | 22 | — |
| Botas | 10 (linhas 61–70) | 14% |
| Linha do chão | y = 71 (a última linha opaca é a 70) | — |

**Proporção:** a cabeça tem quase metade da altura (46%), o que dá cerca de **2,2 cabeças de altura**. É um personagem bem cartunesco e compacto, como pedido.

**Perspectiva (observada):** a cabeça está de perfil, virada para a direita. O tronco, o cinto e os bolsos estão em três quartos, com os dois braços visíveis. O braço da esquerda da imagem é o mais próximo (mais largo, por cima do tronco); o da direita fica atrás. A mesma lógica vale para as pernas.

## 3. Articulações (medidas no sprite; canto superior esquerdo do pixel)

| Ponto | Canvas (x, y) | Como foi medido |
|---|---|---|
| Contato com o chão / pivô geral | (24, 71) | Centro entre as duas botas, na base da sola |
| Centro do quadril | (24, 47) | Meio do cinto, borda inferior |
| Quadril dianteiro | (16, 51) | Topo da coxa da perna da frente |
| Quadril traseiro | (28, 51) | Topo da coxa da perna de trás |
| Cintura (pivô do tronco) | (24, 41) | Linha de cima do cinto |
| Centro do tronco | (24, 36) | Meio da jaqueta |
| Base do pescoço | (25, 33) | Ponta do queixo/gola, onde a cabeça encontra o tronco |
| Ombro dianteiro | (10, 33) | Centro da manga no ombro da frente |
| Ombro traseiro | (35, 35) | Centro da manga no ombro de trás |

## 4. Paleta (medida, depois de agrupar cores quase iguais)

São 20 cores.

| Grupo | Cores (RGB) |
|---|---|
| Contorno | (10,12,16); sombra profunda (25,31,25) |
| Pele e capacete (verdes frios) | (44,63,39) (56,78,46) (65,90,52) (76,104,58) (92,114,65) (138,157,84) |
| Uniforme (oliva) | (87,94,51) (112,115,58) |
| Couro / alças / luvas / botas | (49,44,33) (73,61,42) (98,78,50) |
| Mandíbula / focinho | (157,113,60) (184,135,71) (209,152,71) (239,188,98) |
| Dentes / brilho do olho | (253,233,145) |
| Fivela | (137,127,119) (182,167,152) |

O olho usa os mesmos amarelos da mandíbula ((209,152,71) e (239,188,98)), como na imagem original. A capacete e a pele compartilham a família de verdes frios; o uniforme usa a família oliva.

## 5. Módulos (decisão)

São **7 módulos**, mais 2 overlays de piscada:

| Módulo | Conteúdo | Por que é uma peça |
|---|---|---|
| `head` | capacete + crânio + olho + focinho + mandíbula | É a identidade do personagem; nunca deforma. Uma peça só, sem separar o capacete |
| `torso` | jaqueta, gola, bolsos do peito, alças | Respira (sobe 1 px) |
| `arm_front` | braço da frente inteiro, com luva | Acompanha o ombro; pode ser dividido no cotovelo na etapa da corrida |
| `arm_back` | braço de trás inteiro, com luva | Idem |
| `hips` | cinto, bolsas laterais, fivela, topo da calça | Ponto fixo do idle e futura base do balanço da corrida |
| `leg_front` / `leg_back` | coxa, canela e bota | Já separadas para alternar na corrida; ficam fixas no idle |

**Não separei** capacete, botas, cotovelos, mãos nem mandíbula. Nesta resolução (~2 px por detalhe), essas separações só criariam costuras e contornos quebrados.

### Subpintura oculta (só o necessário para 1 px de movimento)

| Peça | Pixels | Onde |
|---|---|---|
| Quadril | 58 | A jaqueta continua 1–3 linhas sob a borda do tronco |
| Tronco | 42 | Gola e pescoço continuam sob o queixo |
| Tronco | 21 | Lateral sob a borda interna dos braços |
| Automático | 9 | Frestas nas juntas, detectadas quadro a quadro e fechadas na peça de trás |

Remontagem a partir dos módulos: **0 px de diferença** em relação ao master.

## 6. Limitações (observadas)

- A imagem original foi gerada com uma grade irregular. Algumas células tinham dois tons misturados e a mediana escolheu um deles; a leitura geral foi conferida lado a lado com o original.
- O olho tem só 3 linhas de altura. A piscada usa 2 estados (meio fechado e fechado), e é o máximo legível nessa resolução.
