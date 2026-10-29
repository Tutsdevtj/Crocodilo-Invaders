# HUD, moscas e arroto v13

A Saperereca é um enfeite pequeno de 44 × 39, sentado sobre o topo do pneu do retrato, na posição (386,110) da HUD, desenhado à frente da arte. Ao sair do HUD, cresce durante o salto e entra na arena com escala 1,65. Os pontos de apoio foram afastados dos pontos do Sapeneu para acomodar o tamanho; a origem das gotas e o contato do salto acompanham a nova escala.

Ajuste visual posterior: moscas com escala 0,65, incluindo o pulso do aviso. O pacote ZIP citado ao fim registra a build anterior a esse ajuste pontual; os scripts e as capturas atuais incluem o ajuste.

A barra de vida mantém a altura da água e reduz sua largura proporcionalmente ao HP, deixando um canal escuro vazio. A ponta direita tem um menisco animado e libera um jato horizontal mais grosso, com núcleo claro e gotas, ao perder vida. A origem acompanha a ponta que recua; volume e duração respondem à vida perdida. A placa do nome ganhou 222 unidades úteis de texto, 11 pontos e uma única linha, com fundo de madeira e borda verde.

As moscas usam uma reserva de três objetos, com voo ambiente visível e uma única escolhida por vez para o aviso mais agitado. A mira trava a posição real antes do disparo. A primeira pode surgir atrás da nave; as seguintes aparecem em pontos do mapa, sem reposicionar uma atrás do jogador em todo ataque. Cada slot entra uma vez por leva: não há respawn automático de mosca comida no meio da sequência. O início introduz uma mosca, outra 1,35 s depois e a terceira com mais 3,1 s. Sequências escolhem apenas moscas presentes.

Ao comer todas as presentes, a reposição fica suspensa. Depois de recolher a língua, Sapeneu arrota por 2,4 s usando o novo sprite e animação de corpo. O dano recebido dobra e a HUD exibe `ARROTO · DANO x2`. Ele e a parceira não atacam durante a abertura. Ao terminar, o multiplicador e a pose originais retornam. As moscas começam outra leva depois de 3,5 s, aparecendo uma por vez; a primeira já usa os pontos do mapa. Se a metade da vida for cruzada durante o arroto, a entrada da fúria espera essa abertura terminar. A pausa congela todos os tempos e o fim do combate restaura o multiplicador.

Fonte e prompt do sprite em `SourceArt/SapeneuCururu_Burp_v13`; importador `SapeneuBurpTools.Install`. O instalador completo também importa a pose. Capturas/vídeo em `Docs/ArtPreviews/SapeneuCururu_Burp_v13`; logs e backups em `Logs/LagoDoPneu/v13`.

Protótipo local: não commitar, fazer push ou atualizar PR até o usuário finalizar. Dificuldade depende de playtest humano.

Validação final pela entrada real DEV: **31 checks, zero erros**. Cobre enfeite assentado, nome em uma linha, reserva limitada, voo ambiente, largura/altura da água e jato, arroto natural ao limpar moscas, pose registrada, dano dobrado real, aviso na HUD, pausa, ausência de ataques/reposição durante a janela, restauração da defesa, surgimento gradual sem perseguir a nave, crescimento da parceira, apoio no combate e limpeza na derrota. Capturas revisadas visualmente e MP4 decodificado sem erros.

Build Windows `Windows-SapeneuCururu-Burp-v13` concluída no Unity 6000.6.4f1, inicialização sem exceções. Pacote `Builds/Pacotes/CrocodiloInvaders-SapeneuCururu-Burp-v13-TESTE-LOCAL-Windows.zip`: 191 arquivos verificados por hash, 128268771 bytes, SHA256 `95c080e515d40b5ef17920c429e40e26736dd5e5e64773916d4803bf03bf1443`. HEAD e index preservados; nenhuma publicação.
