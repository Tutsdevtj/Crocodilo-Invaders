# Crocodilo Invaders — música das ondas

**Escolha: Going Up — adventure Chiptune, de Ansimuz (Luis Zuno).** É uma sugestão para o combate das ondas antes do boss, mantendo a linguagem retro das duas músicas de chefe já selecionadas, do mesmo autor. A página a descreve como tema de aventura e como loop contínuo. A adequação e o contraste com o boss devem ser aprovados pela sua escuta; o agente não realizou escuta crítica.

## Arquivos

- **Musica/MUS_Crocodilo_Ondas_Loop.wav**: WAV original PCM16 estéreo, 48 kHz, **55.000000 s**. Somente o nome mudou; os bytes são os mesmos de `Going Up.wav` do autor.
- Musica/Going_Up.ogg e Going_Up.mp3: formatos originais, sem conversão.
- Musica/public-license.txt: permissão original do autor.
- Previas/Emenda_do_Loop_em_6s.wav: últimos 6 s seguidos dos primeiros 6 s; emenda em 6,0 s. Fades apenas nas bordas externas, sem esconder o ponto interno.
- **Previas/Ondas_para_Goosini_Fase1.wav**: primeiros 12 s da faixa de ondas, fade-out de 1 s, pausa ilustrativa de 0,4 s, entrada de 12 s do tema aprovado da F1, “Phantasy Boss Battle Encounter”. Montagem para audição, não um arquivo de gameplay.

## Integração sugerida

Use uma fonte 2D dedicada à música e loop na faixa de ondas. Inicie após o diálogo inicial/quando começar a primeira onda. Mantenha a reprodução contínua entre ondas para evitar recomeçar a introdução a cada spawn; em menus de melhoria, pode reduzir o grupo Music e restaurar no combate. Comece testando volume 0,25–0,30 e ajuste junto dos tiros e dash.

Na chegada do boss, faça fade-out de cerca de 0,8–1,0 s, abaixe/pare a trilha durante a cena e diálogo e inicie a música própria do chefe quando a luta começar. O intervalo de 0,4 s da prévia é só demonstração; a cena real decide sua duração. Não empilhe as três trilhas. Ao morrer, reiniciar ou trocar de cena, evite deixar uma segunda fonte tocando por cima.

O autor informa que a música é seamless. Janela original: sample 0 até **2,640,000 exclusivo** no WAV. Não foi aplicado corte, compressão, EQ, time-stretch ou crossfade à faixa principal; confira a emenda no jogo antes de aprovar. As prévias ficam separadas do master e não devem substituir a música nas ondas.

## Fonte e licença

Gratuita, **CC0**, sem crédito obrigatório, conforme a publicação: https://opengameart.org/content/going-up-adventure-chiptune . O texto original incluído permite uso pessoal/comercial, modificação e redistribuição. Referência CC0: https://creativecommons.org/publicdomain/zero/1.0/ . Crédito voluntário: “Going Up by Ansimuz (CC0).”

O trecho do boss na prévia também é de Ansimuz e CC0: https://opengameart.org/content/phantasy-boss-battle-encounter . Não é composição nova ou exclusiva do projeto. **Zero créditos de geração gastos. Nenhum projeto Unity alterado.**
