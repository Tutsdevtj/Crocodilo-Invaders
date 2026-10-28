# Macaquini e selva fechada — revisão v02

Implementado em 22/10/2026 a pedido do usuário: mais movimento, novos desenhos em pixel art e ambiente de selva fechada com múltiplas camadas móveis como na primeira fase.

## Resultado jogável

A fase continua em `Assets/Scenes/StageTwoJungle.unity`, acessível por **FASE 2 · SELVA** no menu. Nove hordas e nove escolhas de carta precedem o Macaquini. A cena isolada `Assets/Scenes/Tests/MacaquiniBossTest.unity`, fora do build, pode ser aberta por **Crocodilo → Fase 2 → Open Macaquini boss test**. F1/F2/F3 forçam salvas, leque e cipós; H cura o jogador.

O corpo visível agora usa desenhos completos no renderer `Visual/ActionDrawings`. Os módulos anteriores permanecem como estrutura de sockets e animação de eventos, com seus renderers desligados. Isso elimina juntas abertas entre peças durante as poses. A master v09 e as fontes anteriores foram preservadas.

Foram produzidas 12 poses, das quais dez são utilizadas: 0–5, 7, 8, 10 e 11. A pose 6 aponta a arma para o lado errado; a 9 tem um membro extra; ambas foram excluídas das sequências. Balanço usa seis passos entre quatro desenhos; mira alterna dois; recuo e chicotada têm desenhos próprios. Movimento global fica no Transform e a mudança interna de pose fica no sprite. Não se afirma reconstrução idêntica à master nem aprovação artística do usuário.

Cada quadro possui pivô corporal e sockets medidos para mão e boca da arma em `SourceArt/JungleMotion_v02/poses.json`. O callback de disparo atualiza a pose e a boca da arma antes de criar o projétil. O cipó acompanha o ponto da mão. Carga, recuo, flash de dano, efeitos e cinco sons próprios existentes foram mantidos.

A recuperação alterna entre (5,5; 4,1) e (2,6; 0,6), passando por uma crista 0,55 unidade acima do ponto mais alto. Velocidade 6,8 unidades/s. No ensaio de 16 segundos, a posição física percorreu **4,05 unidades na vertical**. Os avisos e eventos dos ataques continuam separados do movimento: tiros liberados em 320 ms; cipós avisam por 1,1 s antes do dano. Vida, colisões, pontuação e corredores seguros foram preservados.

## Ambiente

A imagem de clareira foi substituída por mata fechada. As artes são separadas por transparência, e não apenas deslocamentos da mesma placa opaca.

| Plano | Movimento | Ordem Default |
| --- | --- | ---: |
| Mata profunda opaca | deriva lenta com margem de cobertura | -110 |
| Árvores distantes, arte própria | rolagem a 0,16 unidade/s | -104 |
| Troncos médios, arte própria | rolagem a 0,58 unidade/s | -96 |
| Copa e lianas suspensas | rolagem a 1,25 unidade/s | -65 |
| Samambaias e raízes próximas | rolagem a 2,15 unidades/s | 21 |
| Oito cipós adicionais | oscilação independente em três profundidades | -70 / -35 / 20 |

As quatro faixas reutilizam `ScrollingLayer`, o mesmo componente do BattleSky. Tiles extras cobrem telas largas, sem espelhamento alternado. Trinta pequenos elementos de folhagem derivam no vento. Cenário decorativo não tem colisões nem consome o RNG do combate. Tempo escalado congela rolagem, vento e poses na pausa. Objetos auxiliares pertencem à cena e são limpos na saída.

## Arte e evidências

Gerador utilizado: **image_gen integrado**. Fontes originais, prompts completos, recortes e hashes: `SourceArt/JungleMotion_v02/`. Exportador reproduzível: `Tools/Art/prepare_jungle_motion_v2.cjs`. Fundos 480×320; quadros 192×192; paleta de até 24 cores e alfa binário, verificados automaticamente. Importação Point, Full Rect, sem mipmap/compressão; PPU 100 nos fundos e 50 nos quadros. A prancha fonte mede 1448×1086: o canvas de exportação não é apresentado como resolução nativa do gerador.

Prancha dos dez desenhos usados: `Docs/ArtPreviews/JungleMotion_v02/poses-usadas.png`. Vídeo real de Play Mode, com áudio: `Docs/ArtPreviews/JungleMotion_v02/macaquini-selva-fechada-v02.mp4`. Capturas 800×600, 1200×720 e 1920×1080 na mesma pasta. O vídeo apresenta a luta real, não uma animação de mockup.

## Validação realizada

- Importação/compilação e `JungleStageTools.Install`: aprovadas; cena e prefabs atualizados por APIs nativas.
- `JungleStagePlayValidation.Run`: **80 checks, zero erros de gameplay**. Percorreu as nove hordas/164 bichos, nove cartas, ataques, dano, pausa, conclusão, menu e limpeza. Verificou cobertura contínua de cada faixa nas três resoluções.
- `JungleStagePlayValidation.Showcase`: **26 checks, zero erros de gameplay**. Confirmou velocidades distintas, mudança real de desenhos, pausa, dez poses vistas e exclusão das duas inconsistentes, amplitude física e boss visível nas capturas.
- O primeiro ensaio completo encontrou um problema do teste: ler velocidade da vespa após três ticks do Editor podia anteceder FixedUpdate. O teste passou a esperar a velocidade física real; comportamento da vespa não foi alterado.
- `Tools/validate_ci.py`: **1954 GUIDs**, referências e metadados aprovados.
- Build Windows Unity 6000.6.4f1 aprovado, saída de 168321370 bytes; inicialização headless real por oito segundos sem as exceções de runtime pesquisadas. Vídeo/áudio finais decodificados integralmente sem erro.

Logs em `Logs/JungleStage/Install-v2-final.log`, `Play-v2-final.log`, `Motion-v2-final.log` e `Build-v2.log`. O filtro já existente para a exceção intermitente de startup do indexador Unity Search permanece restrito ao stack SearchDatabase; não esconde falhas de gameplay.

Build Windows separado: `Builds/Windows-StageTwo-v02/CrocodiloInvaders.exe`. Pacote completo: `Builds/Pacotes/CrocodiloInvaders-Fase2-Selva-Fechada-v02-Windows.zip`. Extrair tudo e manter EXE, pasta de dados e DLLs juntos. A versão anterior foi preservada.

Pacote de 99646869 bytes; 180 arquivos descompactados virtualmente e comparados ao build por SHA-256. Hash do ZIP: `6BBEB893D51991F20668BA15F735A8BFEF7CBD8FD7DA996B5409A8C5684876B7`, também salvo no arquivo `.zip.sha256` ao lado.

Estas verificações não substituem a avaliação humana de dificuldade e sensação da luta. Transformação/segunda forma e cinemática de libertação continuam fora desta revisão. Nenhum commit foi criado.
