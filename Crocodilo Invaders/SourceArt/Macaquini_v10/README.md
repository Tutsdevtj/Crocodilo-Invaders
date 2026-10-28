# Fontes v10

flight_source.png: 4×4 quadros completos do Macaquini aprovado com jetpack, sem cipó.

transform_source.png: 4×4 etapas de retirada do topo, abertura com duas mãos, descarte conjunto da casca e jetpack, alcance e pegada do cipó. A célula 10 tem um erro de duplicação do pack na fonte e não é selecionada em produção.

Exportação reproduzível: `Tools/Art/prepare_macaquini_jetpack_v10.cjs`. Os valores de registro do quadril e as posições medidas da boca da arma ficam no script/manifest. A exportação mantém o RGB nativo; apenas normaliza a transparência para pixel art e remove pequenos fragmentos isolados que invadiram a célula vizinha.

As folhas v09 anteriores mostram uma transformação ainda pendurada e foram superadas pela decisão de usar jetpack na primeira forma. Não são referenciadas pelo prefab.
