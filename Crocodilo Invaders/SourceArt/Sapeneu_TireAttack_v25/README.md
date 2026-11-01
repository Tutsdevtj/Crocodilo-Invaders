# Pneu arremessado — v25

Master original gerada com image_gen: `tire-master.png`. Prompt integral em `prompt.txt`. Referências: arte existente do Sapeneu e captura do Lago do Pneu.

O importador nativo `LakeTireArtTools.Install` identifica o conteúdo pelo alfa, cria um Sprite como subasset de `Assets/Resources/LakeTireV25/art.asset` e registra largura de 1,15 unidade. Textura Point, sem mipmaps ou compressão. O PNG de produção mantém a master inteira; o recorte é feito pelo Sprite, sem alterar pixels da imagem.

O pneu emerge junto à mão do boss durante 0,85 s, com recorte pela linha da água. A mira trava ao começar a preparação. Depois o pneu segue em linha reta girando, a 10,5 unidades/s (12,5 na fúria), causa 2 HP uma única vez e continua seu percurso. Dash consome o contato sem dano posterior. Ao atingir a água, produz um splash e desaparece. Pausa, fúria e encerramento do combate são tratados pelo controlador.

O ataque entra no ciclo natural a cada terceiro golpe; os primeiros ataques de língua e salto continuam presentes. A Saperereca espera o pneu terminar antes de atacar.
