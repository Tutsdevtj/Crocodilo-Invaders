# Crocodilo Invaders — Kit de implementação da Base Improvisada (v01)

Este pacote consolida assets existentes e organiza um layout proposto para implementação no projeto Unity ativo. **Não é uma cena Unity pronta**. O `blueprint/BaseImprovisada_layout_largo_v01.png` é uma visualização composicional; as artes devem ser montadas como objetos independentes dentro do editor.

## Conteúdo

- `assets/01_terreno/`: sprites PNG individuais extraídos do Lote 01 (terreno, paredes, escadas, chapas, plataformas, cercas). Alguns precisam de revisão de bordas e NÃO estão garantidos como tiles repetíveis.
- `assets/02_suprimentos/`: 10 props individuais transparentes do lote intermediário.
- `assets/03_ambientacao/`: 10 sprites individuais RGBA do Lote 03.
- `assets/04_construcoes/`: 5 construções RGBA (algumas incluem props e fachadas num único desenho). Não estão arquitetonicamente modulares; não simular módulos que não existem.
- `references/backgrounds_opacos/`: imagens de referência de céu, nuvens, montanhas e ruínas. **São RGB sem alpha; NÃO são cinco parallax transparentes exportados.** Não empilhe cegamente. Use uma camada opaca por vez ou crie máscaras manualmente, validando o resultado, e parallax em sprites de primeiro plano.
- `references/concept/`: concept visual e folha de inspiração.
- `blueprint/`: panorama técnico horizontal + layout JSON por zonas.
- `docs/Art_Bible_Referencia.md`: documento legado com histórico e direção de arte. **A menção antiga ao Croc 96×184 está desatualizada**; usar o prefab atual do projeto (o novo master simplificado de 48×72), sem trocar o controlador nem a câmera.

## Cuidados essenciais

- O layout JSON usa `design_pixels`; converter para mundo Unity de acordo com Pixels Per Unit e escala existentes. Não usar esses valores como coordenadas absolutas da cena sem conversão.
- Manter a cena técnica `BaseImprovisada` (sem underscore). Preservar `ChapterFlow` e a transição ao fim do boss.
- O primeiro passeio deve ser **horizontal e contínuo**. Plataformas e escadas superiores são decorações até haver mecânica apropriada.
- Inicialmente só Hangar/Goosini, enfermaria e um pequeno acampamento estão ativos; rádio/comando e expansão devem mostrar barreiras, sem sistemas inventados.
- Civis, médicos e soldados genéricos são **humanos**; Goosini e Croc são os protagonistas antropomórficos. Não gerar NPCs definitivos nesta entrega; reservar anchors.
- Se os assets de alta resolução ficarem incompatíveis com o sprite 48×72, ajustar PPU e eventualmente gerar cópias em resolução apropriada via nearest-neighbor, conferindo perda de detalhes. Não usar bilinear/blur.
- Corpos sólidos de cenário precisam de BoxCollider2D/TilemapCollider2D feitos por geometria de gameplay, não PolygonCollider derivado da silhueta inteira dos PNGs.
- Não reivindicar resultados testados sem executar Play Mode.
