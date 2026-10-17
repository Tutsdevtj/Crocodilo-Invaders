# Crocodilo — roupa relaxada da base, v01

Sprite base de corpo inteiro, em três quartos voltado à direita. Um frame estático; sem animações ou integração automática ao jogo.

## Arquivos de entrega

- `crocodilo-base-relaxado-32.png`: PNG nativo transparente, 32×32.
- `crocodilo-base-relaxado-32.aseprite`: arquivo editável, um frame.
- `crocodilo-base-relaxado-preview-x8.png`: ampliação inteira 8×, 256×256.
- `contextos-preview-x8.png`: revisão sobre fundos claro, escuro e intermediário.
- `validation.json`: medidas reais e verificações técnicas.
- `concept.png`: proposta gerada em alta resolução, preservada como fonte de trabalho; não é a sprite nativa.
- `portrait-reference.png`: referência fornecida.
- `prompt.txt`: prompt usado com a ferramenta integrada de geração de imagens.

## Decisões de design

Preservados: espécie, focinho projetado, pele verde-escura, mandíbula clara e olhar âmbar. Roupa proposta: boné de tecido verde-oliva, camisa solta de manga curta e gola aberta, calça marrom folgada e botas simples. Braços baixos, pés separados e cauda apoiando a silhueta. A troca do capacete por boné e os detalhes do corpo são propostas de design, pois a referência só mostra o busto.

Luz convencionada do alto à esquerda; contorno de um pixel lógico. Paleta proposta com até 16 cores, informada pelas cores da referência, sem alegar extração exata. Corpo condensado para favorecer a leitura em 32×32.

## Produção e revisão

Usada a ferramenta integrada ImageGen para a proposta visual. O arquivo gerado mede 1254×1254 e não apresentou uma grade nativa 32×32. A primeira redução automática foi inspecionada e perdeu leitura no rosto e nas roupas. No Aseprite, o rosto foi reconstruído na grade nativa; contornos, gola, braços, separação das pernas e botas foram retocados. Cores foram organizadas por material. O resultado nativo foi revisado ampliado em três fundos e verificado tecnicamente.

Silhueta revisada: focinho, cabeça, cauda, duas pernas e braços distintos. Construção revisada: cabeça ligada ao pescoço, mangas/antebraços ligados, pés apoiados. As cores e contornos são pixels inteiros, sem antialiasing ou alpha parcial.

O chão e a caixa visível constam em `validation.json`; pivô proposto (16,31), com coordenadas a partir de zero. A posição no jogo pode exigir outro pivô. O teste na cena real da base permanece pendente, pois esta tarefa solicitou somente a arte. Aseprite contém uma camada de personagem; não há rig ou animações.
