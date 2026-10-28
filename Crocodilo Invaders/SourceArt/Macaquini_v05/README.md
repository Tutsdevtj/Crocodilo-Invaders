# Macaquini v05 — macaco dentro da banana

Revisão solicitada: manter o acabamento detalhado e dar ao personagem uma anatomia e um rosto reconhecíveis de macaco. Focinho curto, orelhas arredondadas, pelagem castanha, mãos com dedos e unhas normais. O estudo v03 de lobisomem foi rejeitado; esta versão usa novas fontes e não exporta a arte rejeitada.

Fontes originais e prompts preservados nesta pasta. Exportador: Tools/Art/prepare_macaquini_v5.cjs. Corpos e membros em canvas320×384, quatro mãos96×96; paleta compartilhada32cores, alfa binário, filtro Point, sem compressão nem mipmaps. layout.json registra pivôs, ossos e hashes. Os dois fragmentos superiores recompõem exatamente os pixels da casca fechada antes de se abrirem.

Primeira forma: cabeça e tronco dentro da banana. Ao esgotar180HP, a arma fica recolhida e a mão livre agarra a própria casca enquanto a outra sustenta o corpo no cipó. O clip nativo anima a abertura por4,22s, com peças extraídas da casca original. Segunda forma: cabeça e peito expostos, parte inferior da banana permanece e mais180HP. Ameaças são canceladas na transição, sem pontuação ou abate intermediário; leque passa de5para7projéteis com o mesmo aviso.

Braços articulados por ombro, cotovelo e punho; pegada fixa no mundo e troca antes de ultrapassar alcance máximo ou mínimo. Quatro desenhos de mão mostram abertura/fechamento. Arma orgânica original v04 com12quadros, sockets de recuo e release320ms. HUD usa a mesma hierarquia, moldura e animações do Gusini, com nome e medalhão próprios. Fundo da selva v02 aprovado preservado.

Validação e pacote final: Docs/Fase2Macaquini_v05.md. Os checks medem execução, contatos e progressão; aprovação artística é do usuário.
