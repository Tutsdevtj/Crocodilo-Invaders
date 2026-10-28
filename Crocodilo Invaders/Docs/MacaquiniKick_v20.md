# Macaquini — chute do cipó e volta pela selva v20

Novo ataque da segunda forma: três arcos de embalo crescentes no cipó durante 2,6 s, mão soltando o cipó, perna estendida num chute que cruza a arena e saída completa da tela. O macaco reaparece pequeno atrás da copa, cruza o fundo durante 2,1 s e retorna em arco para agarrar o cipó no ponto de partida. A escala reduzida é exclusiva da passagem distante; o corpo retorna à escala normal ao finalizar ou cancelar.

A direção fica travada e é avisada por 0,85 s antes da soltura. O pé usa colisão varrida em Physics2D para não atravessar o jogador entre passos. Causa 3 de dano, no máximo uma vez por chute, respeitando a proteção real do dash. Embalo, saída, volta distante e retorno não causam dano. A passagem distante não recebe tiros. As ameaças anteriores são removidas antes da sequência. Pausa congela tudo; cancelar restaura a posição, a escala, a cor, a ordenação e remove a proteção temporária.

Seis poses completas foram criadas com `image_gen.imagegen` a partir do sprite atual `Macaquini_v18/transform_39.png`, preservando identidade, peito claro, olhos verdes, banana rasgada, pelo e rabo. Fonte com alfa em `SourceArt/MacaquiniKick_v20/kick-generated.png`. Recorte e registro reproduzíveis em `Tools/Art/prepare_macaquini_kick_v20.py`: mesma escala de 0,64 para todas as poses, 80 PPU e pivô comum na pelve. Sem montagem de membros separados.

O golpe participa da rotação normal da segunda forma junto do pêndulo. Na arena: K transforma; F5 chama o chute; F4 chama o pêndulo; Shift dá dash; I alterna proteção; H cura; R reinicia.

Implementação: `MacaquiniSwingKick.cs`, integração em `MacaquiniCombat.cs` e `MacaquiniFlightPresentation.cs`. Instalação nativa com `MacaquiniKickV20Tools.Install`; build Windows com `MacaquiniKickV20Tools.Build`. Versões anteriores preservadas.

Validação em Play Mode: 88 verificações da luta sem erros, incluindo dano real do pé, um acerto por passagem, direção travada, pausa, escala real do desenho na passagem distante, proteção temporária, retorno e cancelamento. O teste anterior do pêndulo também atravessa o cipó usando o dash real. Verificação estrutural aprovada com 2415 GUIDs de assets.
