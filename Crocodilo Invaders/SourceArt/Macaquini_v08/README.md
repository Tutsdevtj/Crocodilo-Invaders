# Macaquini v08 — teste com quadros completos

Pedido de23/10/2026: experimentar animação em pixel arts completas intercaladas, incluindo pés, pernas, rabo, rosto e cipó, evitando emendas de peças separadas.

Foram geradas quatro folhas com image_gen integrado, preservadas nesta pasta com os respectivos prompts. O exportador Tools/Art/prepare_macaquini_frames_v8.cjs extrai personagens inteiros por componentes conectados e registra cada quadro pelo topo pintado do cipó. Mantém o desenho completo e remove apenas a transparência de exportação; não move membros, não deforma a imagem e não cria poses com rotação de recortes. layout.json registra fontes, pivôs, recortes, escala e SHA256. Os PNGs finais usam canvas384×384, alfa binário e paleta compartilhada40cores.

Fontes e prompts finais: [balanço fechado](closed_source.png) / [prompt](closed_prompt.txt); [balanço exposto](half_source.png) / [prompt](half_prompt.txt); [disparos](attack_source.png) / [prompt](attack_prompt.txt); [rasgo](tear_source.png) / [prompt](tear_prompt.txt).

| Sequência | Desenhos únicos | Reprodução padrão |
|---|---:|---|
| Banana fechada no cipó |16|Ida e volta, ciclo3,75s|
| Macaco parcialmente livre no cipó |16|Ida e volta, ciclo3,75s|
| Disparo da banana fechada |8|Preparação, abertura, recuo e repouso,1,32s|
| Disparo do macaco exposto |8|Preparação, abertura, recuo e repouso,1,32s|
| Rasgo da casca |8|Fecha, contrai, rompe, revela e acomoda,4,22s|

Total56desenhos completos. O balanço percorre os16quadros e os intermediários em ordem inversa, evitando pular diretamente da última pose para a primeira. Pés se recolhem/estendem, dedos mudam, rabo acompanha com atraso, rosto pisca/franze/abre a boca e o cipó muda de curvatura, todos já pintados em cada quadro.

Assets/Art/JungleStage/Macaquini_v08 contém os56PNGs. Assets/Animations/Boss/MacaquiniFullFrames_v08 contém cinco clipes nativos .anim, com uma única trilha SpriteRenderer.m_Sprite. O prefab MacaquiniFullFramesPreview contém um único SpriteRenderer para todo o personagem; não usa o rig modular nem IK. A campanha continua na v07 enquanto o usuário avalia esta alternativa.

A cena Assets/Scenes/Tests/MacaquiniFrameAnimationTest.unity abre uma bancada independente na selva aprovada. Controles de forma, ação, pausa, quadro anterior/próximo, seleção pela tira de miniaturas, velocidade0,25x–2x e repetição do ataque. Espaço pausa; setas avançam/voltam;1/2trocam forma;Bbalanço;Adisparo;Trasgo;Escfecha o executável.

Resultados, vídeo e pacote de teste: [documentação](../../Docs/Fase2MacaquiniQuadros_v08.md). Fontes e prompts foram gerados com a ferramenta integrada image_gen, sem fallback CLI. Não há aprovação artística humana implícita.
