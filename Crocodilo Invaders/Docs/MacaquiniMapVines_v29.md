# Macaquini — cipós que atravessam o mapa v29

Aplica aos ataques de cipó o acabamento aprovado na ligação da mão na v28.

- **Cipós horizontais (`JungleVineHazard`):** faixa de casca contínua entre as duas pontas, com textura Point repetida. Retirados os sprites sobrepostos e as rotações/oscilações independentes que abriam emendas. O aviso fino dourado permanece separado e inofensivo; a casca aparece ao ativar o ataque. Mesmos tempos, raio de dano 0,18, dois de dano e um acerto por ataque. A cápsula segue a direção do segmento.
- **Pêndulo (`MacaquiniPendulumVine`):** corpo inteiro em uma malha contínua, com a mesma casca. Retirada a pilha de sprites e o núcleo luminoso durante a passagem ativa. Preservados pivô, ângulo, varredura, retorno, aviso direcional, trilha e dois de dano por contato. Casca mais discreta durante a preparação e opaca na passagem ativa.
- **Mão:** extensão v28 preservada. A malha de ligação atualiza a ponta após a escolha da pose em LateUpdate, evitando atraso de um quadro no pêndulo. A inspeção do validador v28 distingue agora a ligação da mão das malhas dos ataques.

`MacaquiniVineRibbon` aceita tanto encaixe de pose quanto caminho com largura fixa, reutilizando `Assets/Resources/Art/MacaquiniVineV28/vine_bark.png`. Não foi criada arte nova nem alterados sprites originais/prefabs/cenas. UV por distância evita esticar a casca ao longo do mapa. Pausa, cancelamento e fim da batalha congelam/limpam as malhas.

Validação `MacaquiniMapVinesV29Validation.Run`: **22 verificações, zero erros**, Unity 6000.6.4f1, 27/10/2026. Inclui aviso sem dano, faixa contínua e UV repetida, colisão real horizontal, dano único, corredor seguro, pausa, expiração, pêndulo com dano real, ligação da mão, padrão nativo com duas faixas e uma livre e limpeza. Capturas em `Docs/ArtPreviews/MacaquiniMapVines_v29`, log `Logs/JungleStage/MapVines29.log`.

Build `MacaquiniMapVinesV29Tools.Build`: `Builds/Windows-Macaquini-Cipos-Mapa-v29/MacaquiniSelva.exe`, teste da segunda forma. F4 força o pêndulo; F5 testa o chute e R reinicia. Sem mudanças no áudio restaurado ou no combate aprovado da v25. Sem commit/push.

Entrega: build Windows `Succeeded`, checkout validado e executável v29 aberto para teste local. Log do player em `Logs/JungleStage/MapVines29-player.log`.
