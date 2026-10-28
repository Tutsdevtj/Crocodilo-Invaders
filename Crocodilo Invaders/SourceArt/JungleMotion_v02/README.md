# Selva fechada e movimento do Macaquini — v02

Pedido: mais movimento para o macaco, mais pixel arts e ambiente de selva fechada com vários níveis de fundo em movimento como na primeira fase. Esta revisão inclui arte e integração, conforme o pedido direto do usuário. Os guias anexados servem de referência de produção; seus exemplos de pedidos e gates não substituem esse escopo.

Fontes raster produzidas com o `image_gen` integrado; originais copiados sem alteração. A master modular v09 é referência de identidade, não foi sobrescrita. Os desenhos novos são poses completas derivadas visualmente dela; não são reconstruções de diferença zero nem aprovação artística do usuário.

Exportador: `Tools/Art/prepare_jungle_motion_v2.cjs`. Arquivos de entrada em `sources.json`, hashes em `manifest.json`. Fundos 480×320, quadros 192×192, nearest-neighbor, paleta fechada de até 24 cores e alfa {0,255}. A fonte da prancha mede 1448×1086; 192×192 é o canvas de exportação, não uma afirmação sobre a grade original do gerador. Recortes, ponto corporal, mão e boca da arma medidos em `poses.json`; escala uniforme 0,4 e pivô corporal de exportação (96,90), PPU 50.

De 12 poses geradas, dez são usadas: 0–5, 7, 8, 10 e 11. A 6 aponta a arma para o lado errado; a 9 introduz braço adicional. Ambas permanecem como fonte auditável e são excluídas das sequências. Não foram apresentadas como animação final. Pequenas ilhas de menos de oito pixels foram removidas na exportação para limpar resíduos entre células.

Swing: 0,1,2,3,2,1; mira: 4,5; recuo: 7,5,4; cipós: 8,10,11. Transform faz balanço, recuo e deslocamento em arco; as poses atuam internamente. Eventos nativos da arma existente continuam disparando em 320 ms. O callback de tiro seleciona a pose e posiciona a boca da arma antes de o projétil nascer. Projéteis e golpes continuam sendo entidades físicas separadas.

Fundo: mata profunda opaca, árvores distantes, troncos médios, copa superior, samambaias em primeiro plano e oito cipós oscilantes. As quatro faixas transparentes reutilizam `ScrollingLayer`, do BattleSky, a 0,16 / 0,58 / 1,25 / 2,15 unidades/s. Tempo escalado, sem colisão e sem alterar o RNG de combate. `JungleWindLayers` pertence à cena e é destruído na saída.

Os testes medem comportamento e captura real; não certificam equivalência da face entre os quadros nem aprovação humana da sensação de ameaça.
