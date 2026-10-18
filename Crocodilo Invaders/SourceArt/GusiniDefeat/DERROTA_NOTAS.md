# Gusini: derrota final (Fase 2 → Gusini recuperado)

A Fase 2 cai e **o parasita é destruído, não o Gusini**.

- **Duração:** **1,9 s**, em 18 frames. Depois entra o loop do Gusini exausto.
- **Canvas e pivô:** **256×196**, pivô **(120,118)**, os mesmos de todo o boss (idle, ataques, transformação).
- **Peças:** recompostas a partir das **peças aprovadas** da F1 e da F2, sem redesenhar nada.

## Roteiro
| Trecho | Frames | ms | O que se vê |
|---|---|---|---|
| **Golpe** | F0 | 0–60 | Flash branco: HP da F2 zerou (`defeat_hit`) |
| **O alien perde o controle** | F1–F6 | 60–490 | Ele cambaleia (tremor de 1 px nas peças). O verde **falha e volta** em ritmo irregular (apaga → estoura → apaga → pulso com anel e raios), a corrupção morre (verde vira cinza) e o **módulo fica em brasa branca** (`core_unstable`, `core_critical`) |
| **O parasita estoura** | F7–F9 | 490–680 | Explosão **localizada no módulo**: núcleo branco, anel verde e 8 raios. Depois o anel abre, **pedaços escuros da carapaça voam** e sobe fumaça verde. Fica um buraco queimado onde o parasita estava (`parasite_burst`) |
| **A corrupção se desfaz** | F10–F13 | 680–1030 | **O rosto do Gusini volta primeiro** (`face_recovered`). Flash branco, e a F1 aparece com as veias apagadas sumindo e a fumaça verde se dissipando (`corruption_fading`) |
| **Gusini salvo, exausto** | F14–F17 | 1030–1900 | Só sobra fumaça **normal** de motor. A nave afunda e o nariz cai, o cachecol murcha e o olhar fica pesado, olhando para baixo, com gota de suor (`gusini_recovered`, `defeated_hold`) |
| **Loop** | `final/gusini_p1_defeated_idle_0-3` | 120 ms cada | Flutua 1 px, fumaça do motor e da fuselagem, faísca de vez em quando |

## v2: nave avariada (correção)
A v1 deixava a nave **limpa demais**, com cara de "consertada". Na v2, o Gusini volta, mas **a nave não volta perfeita**. Veja `previews/o_que_mudou_v1_v2.png`: o mapa em vermelho mostra tudo o que foi danificado.

**Marca do alien morto, na base do leme, onde ficava o parasita:**
- cratera com mordida na silhueta e pétalas de metal rasgado para fora;
- poça de resíduo verde-escuro com escorridos e borrifo radial do estouro;
- respingos espalhados pela fuselagem e pelas asas;
- fumaça cinza saindo da cratera.

**O verde agora é resto, não corrupção ativa:**
- usa só os tons escuros (`#14523E` e `#1E7846`);
- tem **um único pixel** de brilho fraco;
- não tem aura, pulso nem veias.

**Danos da luta:**
- **Fuselagem:** 3 rachaduras com aresta clara, arranhões, 2 amassados, chamuscados, rastro de fuligem e um **painel arrancado** com fios vermelhos e amarelos expostos.
- **Leme:** **lascado** no canto de cima e com uma rachadura.
- **Asa de trás:** ponta **rasgada**.
- **Asa da frente:** ponta **entortada 2 px para baixo** (assimetria), **furo**, ponta esfiapada e borda lascada.
- **Canhão:** **cano caído 1 px**, chamuscado e amassado.
- **Motor:** bocal chamuscado e rachado.

**Por que ainda voa:** as asas, o motor e o cockpit continuam inteiros. A nave está avariada, não destruída.

**Onde isso aparece:** a nave avariada entra a partir do F12, quando a Fase 1 aparece, e segue no loop final.

## Gusini pós-batalha
É a **Fase 1 aprovada**, com duas variações derivadas das peças aprovadas:

- **Cabeça cansada:** pálpebra caída até a metade do olho, pupila baixa e gota de suor.
- **Nave avariada:** todas as peças com danos (ver v2 acima).

**Postura de derrotado:**
- nave 7 px mais baixa;
- nariz para baixo (inclinação 5);
- cabeça baixa;
- cachecol murcho;
- fumaça contínua.

Ele está vivo e reconhecível, mas claramente sem condições de lutar. Compare em `previews/fase1_vs_recuperado_x2.png`.

## Arquivos
```
frames/gusini_defeat_00..17.png        composição final (use estes)
layers/sprite/ · layers/fx/            personagem e efeitos separados por frame
final/gusini_p1_defeated_idle_0-3.png  loop pós-batalha (+ _sprite_ / _fx_ separados, + sheet)
fx/fx_parasite_pop_0-1 · fx_parasite_debris_0-3 · fx_alien_smoke_0-4   FX soltos (80x64, centro do estouro em (40,48))
gusini_defeat_sheet.png (+_x2)         spritesheet
gusini_defeat.json                     frames, durações, eventos e sugestões de Unity
previews/gusini_derrota_x3.gif · gusini_derrota_em_tela.gif · quadros_chave_x2.png · fase1_vs_recuperado_x2.png
                                       o_que_mudou_v1_v2.png · gusini_recuperado_final_x4.png
```

## Sugestões para a Unity (não fazem parte dos sprites)
- **Tremor de tela:** 6 px por 120 ms em `defeat_hit`; 4 px por 150 ms em `parasite_burst`.
- **Câmera lenta (opcional):** timeScale 0.4 por 180 ms no `parasite_burst`. É o momento de maior satisfação.
- **Barra de vida da F2:** esconder no `parasite_burst`.
- **Depois do `defeated_hold`:** loop `defeated_idle` e a cena de diálogo. Se quiser o boss saindo de cena, desça devagar (~20 px/s).

## Revisão
- **O alien foi derrotado?** Sim. O verde falha, o núcleo entra em brasa e estoura; a carapaça sai voando em pedaços e sobra só um buraco queimado.
- **O Gusini voltou ao normal?** Sim. O rosto volta antes do corpo, e depois disso não aparece mais nenhum verde: só fumaça cinza de motor.
- **Curto e impactante?** 1,9 s. O estouro é o pico, com 190 ms de explosão legível. Não é uma segunda transformação.
- **Parece pós-batalha?** Sim: nave afundada e inclinada, olhar cansado, cachecol murcho e fumaça contínua.
- **A nave ainda está danificada?** Sim: cratera, peças lascadas e tortas, rachaduras e painel arrancado. Não está bonitinha.
- **O verde é resíduo e não corrupção?** Sim: só manchas e respingos escuros, sem brilho ativo, aura ou veias.
