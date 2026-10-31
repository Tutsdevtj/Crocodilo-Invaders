# Lago — fundo opaco v28

Corrigida a transparência adicional das camadas de vegetação durante a travessia. No começo da fase, duas faixas eram desenhadas com cerca de 66% e 39% de opacidade, deixando ver imagens sobrepostas através das árvores. Agora todos os renderers do fundo e suas cópias usam opacidade integral. Os adereços da margem também passam de 95% para 100%.

A transparência natural dos recortes PNG é preservada. Rolagem, paralaxe, vento, água e camadas mantêm seu movimento. Sem nova arte, alteração de combate, commit ou atualização da PR.

Conferência visual por captura nativa em movimento, na cena real StageTwoLake. Auxiliar temporário arquivado em Logs/LagoDoPneu/v28/CaptureHelper. Vídeo e capturas em Docs/ArtPreviews/LakeOpaqueBackground_v28.


Conferência nativa: 22 renderers/cópias do fundo com alpha mínimo 1; adereços com alpha 1; deslocamento da camada 4 observado em 7,84 unidades. Captura em movimento sem erros e vídeo decodificado integralmente.

Build Windows-LagoDoPneu-Opaque-v28 concluída com sucesso, conteúdo informado pela Unity: 207224196 bytes. Pacote local inclui jogo e prévia.
