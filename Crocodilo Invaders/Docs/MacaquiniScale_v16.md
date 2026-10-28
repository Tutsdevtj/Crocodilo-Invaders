# Macaquini — escala uniforme v16

O salto de tamanho vinha da escala de importação dos desenhos. Os dois blocos de voo usavam aproximadamente 78,91 e 70,69 pixels por unidade, causando uma diferença súbita de cerca de 12% entre quadros quase iguais. A etapa de busca do cipó interpolava de cerca de 84,44 até 58,80 pixels por unidade. O ciclo final usava 60; havia ainda uma ampliação adicional de 6,5% no corpo durante o grito.

As 98 poses completas agora compartilham 80 pixels por unidade: 32 de voo, 42 da transformação e 24 no cipó. Pivôs registrados no mesmo ponto médio dos pés, com posição local comum. Posições do disparo e da ligação do cipó foram convertidas para o mesmo registro. Escala do corpo permanece constante; força e emoção continuam nas poses, no rosto, no som e nos efeitos.

Nenhum PNG foi redesenhado ou reamostrado. GUIDs, resolução, cores, detalhes e nitidez foram preservados. As diferenças naturais entre poses permanecem; o tamanho de um pixel no mundo não muda de acordo com o quadro. A câmera da luta não usa zoom dinâmico.

Registro reproduzível: `Tools/Art/register_macaquini_scale_v16.cjs` e `SourceArt/MacaquiniScale_v16/registration.json`. Instalação pela API nativa: `MacaquiniScaleV16Tools.Install`. Validação acompanha PPU e escala mundial durante voo, rasgo e cipó, além dos testes existentes de movimento, combate, pausa, transformação e HUD.

Teste: `Builds/Windows-Macaquini-Escala-v16/MacaquiniSelva.exe`. J reduz vida, K transforma, I ativa proteção e R reinicia. Evidências: `Docs/ArtPreviews/MacaquiniScale_v16`.
