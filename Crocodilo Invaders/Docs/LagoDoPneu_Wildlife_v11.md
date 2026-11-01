# Água e piranhas v11 — protótipo local

Oito peixes animados cruzam o lago em sentidos alternados. São decorativos, sem colisão, renderizados entre a água base e os reflexos da superfície. O movimento usa tempo escalado, incluindo a pausa.

Quando a parte inferior da nave toca a superfície (+0,08 unidade de tolerância), cinco piranhas se agrupam abaixo dela. Uma agitação na água antecipa os botes por 0,65 s. Elas saltam em sequência, separadas por 0,12 s, abrem a boca na subida e produzem respingos ao atravessar a superfície. Novos grupos surgem a cada 1,65 s enquanto a nave permanecer baixa; reserva limitada a dez piranhas.

A trajetória tem gravidade e mira com pequena antecipação horizontal. A mordida usa cápsula varrida e o filtro Player existente, causa 1 HP e respeita invulnerabilidade/dash. Há um intervalo compartilhado de 0,65 s entre tentativas de dano do cardume. Descer mais fundo continua perigoso. Subir até a parte inferior da nave ficar 0,55 unidade acima da superfície encerra o perigo, cancela os botes pendentes e faz as restantes retornarem sem dano. Não há força artificial aplicada ao controle da nave.

O primeiro plano de vegetação agora usa WorldFront/100, incluindo as duas cópias de rolagem. A HUD permanece acima do mundo. Base da água em -190, peixes decorativos -185, piranhas submersas -182, reflexos -180; piranhas no ar usam 32.

## Integração

`LakeWildlifeTools.Install` importa e registra quatro quadros de peixe e seis de piranha e atualiza StageTwoLake e SapoDoPneuTest sem reconstruir a campanha. O instalador completo também chama essa etapa. Assets/pivôs são criados pelo Editor nativo. PNGs originais, prompts e hashes em `SourceArt/LagoDoPneu_Wildlife_v11`.

## Validação

Play Mode pela entrada real DEV > BOSS FINAL > SAPO DO PNEU: 24 checks, zero erros. Cobre arte, camadas, nado nos dois sentidos, antecipação, pausa, cancelamento ao subir, sequência de botes, boca aberta no ar, invulnerabilidade, dano de 1 HP, intervalo entre mordidas, repetição, fuga, ataques submersos e limpeza no fim do combate. Nesse teste específico, o combate do sapo é desativado em runtime depois da entrada para isolar a origem do dano; prefab e cena preservam o combate ativo. Progresso é redirecionado a pasta de teste.

Regressão da luta completa: 44 checks, zero erros, preservando campo de moscas, linguadas, salto, gotas, dash e limpeza na derrota. Total desta rodada: 68 checks, zero erros.

Capturas e vídeo em `Docs/ArtPreviews/LagoDoPneu_Wildlife_v11`; logs em `Logs/LagoDoPneu/v11`. Dificuldade final ainda depende de playtest humano.

**Não commitar, fazer push ou atualizar PR até o usuário finalizar o protótipo.**

Build Windows `Windows-LagoDoPneu-Wildlife-v11` concluída pelo Unity 6000.6.4f1. Inicialização do executável sem exceções no log. ZIP `Builds/Pacotes/CrocodiloInvaders-LagoDoPneu-Wildlife-v11-TESTE-LOCAL-Windows.zip`: 191 arquivos verificados por hash, 120064053 bytes, SHA256 `3e83159c415caa735fb8e6ffe7ab2826a3b6dcb8b70ce6f8a5453b2259de09ac`. MP4 decodificado sem erros. HEAD e index preservados.
