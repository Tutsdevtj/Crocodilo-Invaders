programa
{
	inclua biblioteca Graficos --> g
	inclua biblioteca Teclado --> t
	inclua biblioteca Util --> u
	inclua biblioteca Texto --> tex
	inclua biblioteca Sons --> sm

/*.-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-.
 |                                                                       |
 |      Jogo feito por Arthur Reis e Erik Andriani Vargas                |
 |      para a disciplina Algoritmos e Programação do                    |
 |      Curso de Ciência da Computação                                   |
 |                                                                       |
 !                                                                       !
 :      Matéria lecionada pelo professor                                 :
 :      André Raabe                                                      :
 |                                                                       |
`-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-etf-' */




	inteiro somMenu = sm.carregar_som("menu_music.mp3")
	logico menuS
	funcao audioMenu() {
		inteiro vol = 75
		sm.reproduzir_som(somMenu, verdadeiro)
		sm.definir_volume_reproducao(somMenu, vol)

	}
	inteiro somSelect = sm.carregar_som("select.mp3")
	logico menuInicial = falso
	funcao menu()
	{
		g.iniciar_modo_grafico(falso)
		menuInicial = verdadeiro
		//carregamento e redimensionamento de arquivos externos
		
		inteiro fundoMenu = g.carregar_imagem("mainMenu.png")
		inteiro setaMenu = g.carregar_imagem("seta.png")

		inteiro somSelection = sm.carregar_som("selection.mp3")
		inteiro icon = g.carregar_imagem("icon.png")
		inteiro larguraTela = 1200
		inteiro alturaTela = 720
		g.definir_dimensoes_janela(larguraTela, alturaTela)
		g.definir_titulo_janela("Crocodilo Invaders")
		g.definir_icone_janela(icon)
		fundoMenu = g.redimensionar_imagem(fundoMenu, larguraTela, alturaTela, verdadeiro)

		
		inteiro voltarSobre = 0
		inteiro opcao = 0
		enquanto (menuInicial == verdadeiro) {

			
			g.limpar()
		
			g.desenhar_imagem(0, 0, fundoMenu)
			g.definir_cor(Graficos.COR_BRANCO)
			g.definir_tamanho_texto(12.0)
			
			g.desenhar_texto(20, 655, "Setas direcionais para se mover")
			g.desenhar_texto(20, 670, "Pressione 'ESPAÇO' para Atacar")
			g.desenhar_texto(20, 685, "Pressione 'C' para Ataque Especial")
			g.desenhar_texto(20, 700, "Passar por cima dos powerups para ganhar HP")


			se (t.alguma_tecla_pressionada()) {
				se (t.tecla_pressionada(t.TECLA_SETA_ACIMA)) {
					sm.reproduzir_som(somSelection, falso)
					u.aguarde(200)
					
					opcao = opcao - 1
					
				}
				se (t.tecla_pressionada(t.TECLA_SETA_ABAIXO)) {
					sm.reproduzir_som(somSelection, falso)
					u.aguarde(200)
				
					opcao = opcao + 1
					
				}
			}

			se (opcao <= -1) {
				opcao = 2
			}
			senao se (opcao == 3) {
				opcao = 0
			}

			se (opcao == 0) {
				g.desenhar_imagem(780, 347, setaMenu)
			}
			senao se (opcao == 1) {
				g.desenhar_imagem(780, 457, setaMenu)
			}
			senao se (opcao == 2) {
				g.desenhar_imagem(780, 570, setaMenu)
			}

			se (opcao == 0) {
				se (t.tecla_pressionada(t.TECLA_ENTER)) {
					sm.reproduzir_som(somSelect, falso)
					menuS = falso
					sm.definir_volume_reproducao(somMenu, 0)
					tick()
				}
			}

			se (opcao == 1) {
				se (t.tecla_pressionada(t.TECLA_ENTER)) {
					sm.reproduzir_som(somSelect, falso)
					sobre()
				}
			}

			se (opcao == 2) {
				se (t.tecla_pressionada(t.TECLA_ENTER)) {
					sm.reproduzir_som(somSelect, falso)
					retorne
				}
			}
			g.renderizar()
		}
	}
	
	funcao sobre() {
		logico voltarSobre = verdadeiro
		inteiro fundoSobre = g.carregar_imagem("sobre.png")
		
		enquanto (voltarSobre == verdadeiro) {
			g.desenhar_imagem(0, 0, fundoSobre)
			g.renderizar()

			se (t.tecla_pressionada(t.TECLA_ESC)) {
				voltarSobre = falso
			}
		}
	}

	inteiro win_Menu = g.carregar_imagem("win_menu.png")
	logico isWinMenu = falso

	funcao winMenu() {
		isWinMenu = verdadeiro
		isPlaying = falso
		enquanto (isWinMenu) {
			sm.definir_volume_reproducao(somMenu, 0)
			g.desenhar_imagem(0, 0, win_Menu)
			inteiro branco = g.criar_cor(255, 255,255)
			g.definir_cor(branco)
			g.definir_estilo_texto(falso, verdadeiro, falso)
			g.definir_tamanho_texto(30.0)
			g.desenhar_texto(644, 285, "" + totalPoints)
			g.desenhar_texto(740, 354, "" + enemies_killed)
			g.definir_tamanho_texto(50.0)
			g.desenhar_texto(685, 549, "SS")
			g.definir_tamanho_texto(16.0)
			g.desenhar_texto(900, 700, "Pressione ENTER para sair do jogo")
			
			se (t.tecla_pressionada(t.TECLA_ENTER)){
				sm.reproduzir_som(somSelect, falso)
				retorne
			}
			g.renderizar()
		}
	}

	logico barrier = falso
	inteiro timer_alert = 0
	inteiro movement_bossX = 1700
	inteiro movement_bossY = 90
	inteiro movementX = 100
	inteiro movementY = 319
	funcao movement_main_character() {

		se (t.tecla_pressionada(t.TECLA_SETA_ACIMA)) {
			movementY = movementY - 1
			se (movementY <= 3) {
				movementY = 4
			}
		}
		se (t.tecla_pressionada(t.TECLA_SETA_ABAIXO)) {
			movementY = movementY + 1
			se (movementY >= 450) {
				movementY = 449
			}
		}
		
		se (t.tecla_pressionada(t.TECLA_SETA_ESQUERDA)) {
			movementX = movementX - 1
			se (movementX <= 0) {
				movementX = 1
			}
		}
		se (t.tecla_pressionada(t.TECLA_SETA_DIREITA)) {
			movementX = movementX + 1

			se (movementX >= 650) {
				movementX = 649
				barrier = verdadeiro
				timer_alert = 100
			} senao {
				barrier = falso
			}
		}
	}

	funcao logico verificarColisaoPr(inteiro canto_prmcX, inteiro canto_prmcY, inteiro larguraPrX, inteiro alturaPrX, inteiro canto_prenX, inteiro canto_prenY, inteiro larguraEnX, inteiro alturaEnX) {
		se (canto_prmcX < canto_prenX + larguraEnX e canto_prmcX + larguraPrX > canto_prenX e canto_prmcY < canto_prenY + alturaEnX e canto_prmcY + alturaPrX > canto_prenY) {
			retorne verdadeiro
		}
		senao {
			retorne falso
		}
	}

	inteiro mainC_health = 30
	inteiro mainC_dmg = sm.carregar_som("mainC_dmg.mp3")
	inteiro enemy1_health = 4
	inteiro enemy2_health = 4
	inteiro enemy_dmg = sm.carregar_som("enemy_dmg.mp3")
	inteiro enemy_when_dmg = sm.carregar_som("enemy_when_dmg.mp3")
	logico isEnemyAlive
	logico isEnemy2Alive
	inteiro enemies_killed = 0
	inteiro game_over = sm.carregar_som("gameover.mp3")
	inteiro enemy_death = g.carregar_imagem("enemy_death.gif")
	inteiro xEffect, yEffect
	logico desenhar_efeito_enemy1 = falso
	inteiro timer_efeito_enemy1 = 0
	logico desenhar_efeito_enemy2 = falso
	inteiro timer_efeito_enemy2 = 0
	logico desenhar_hit_sp = falso
	inteiro hit = g.carregar_imagem("hit_effect.gif")
	inteiro timer_hit = 0
	inteiro sp_hit = g.carregar_imagem("sp_hit_effect.gif")
	inteiro sp_dmg = sm.carregar_som("sp_sound.mp3")
	inteiro timer_hit_sp = 0
	logico desenhar_hit = falso

	//boss variáveis
	inteiro boss_hitbox1W = 230
	inteiro boss_hitbox1H = 250
	inteiro boss_hitbox2W = 50
	inteiro boss_hitbox2H = 50
	inteiro boss_hitbox3W = 50
	inteiro boss_hitbox3H = 50

	inteiro boss_explode = g.carregar_imagem("boss_death_effect.gif")
	inteiro boss_death_sound = sm.carregar_som("boss_death_sound.mp3")
	inteiro win_sound = sm.carregar_som("win_sound.mp3")
	inteiro timer_laser_beam = 0
	logico boss_atk = falso
	inteiro xEffectPlus = 700
	inteiro yEffectPlus = 90

	inteiro boss_missile = g.carregar_imagem("boss_missile.gif")
	inteiro numero_misseis = 5
	inteiro missileX[5]
	inteiro missileY[5]
	logico missile_ativo[5]
	inteiro timer_missile_atk = 0

	inteiro laser_trace = g.carregar_imagem("laser_trace.png")
	inteiro laser_beam = g.carregar_imagem("laser_beam.gif")
	// Variações verticais
	inteiro laser_trace_vertical = g.carregar_imagem("laser_trace_vertical.png")
	inteiro laser_beam_vertical = g.carregar_imagem("laser_beam_vertical.gif")

	inteiro timer_laser_principal = 1500
	inteiro timer_laser_vertical_especial = 2000

	inteiro h_laser_estado_0 = 0
	inteiro h_laser_y_0 = 0
	inteiro h_laser_timer_0 = 0

	inteiro h_laser_estado_1 = 0
	inteiro h_laser_y_1 = 0
	inteiro h_laser_timer_1 = 0

	inteiro v_laser_estado_0 = 0
	inteiro v_laser_x_0 = 0
	inteiro v_laser_timer_0 = 0

	inteiro v_laser_estado_1 = 0
	inteiro v_laser_x_1 = 0
	inteiro v_laser_timer_1 = 0

	inteiro h_laser_loops_0 = 0
	inteiro h_laser_loops_1 = 0
	inteiro v_laser_loops_0 = 0
	inteiro v_laser_loops_1 = 0

	inteiro timer_proximo_ataque_laser = 200 
	cadeia proximo_ataque_tipo = "HORIZONTAL"
	
	inteiro som_laser_beam = sm.carregar_som("laser_beam_sound.mp3")
	inteiro missile_sound = sm.carregar_som("bit.mp3")
	logico isBossDead = falso
	funcao boss_effects() {

		//death
		se (desenhar_efeito_enemy1 e isBossDead) {
			sm.reproduzir_som(win_sound, falso)
			g.desenhar_imagem(xEffectPlus, yEffectPlus, boss_explode)
			timer_efeito_enemy1--
			
			se(movement_bossX >= 400 e movement_bossY < 1700) {
				movement_bossX--
				movement_bossY+= 2
				se(movement_bossY <= 1550) {
					sm.reproduzir_som(win_sound, falso)
					isWinMenu = verdadeiro
				}
			}
			
			
			se (timer_efeito_enemy1 <= 0 e movement_bossX <= 500) {
				desenhar_efeito_enemy1 = falso
			}
		}

		se(appearBoss == verdadeiro) {
			se (verificarColisaoPr(projectileX, projectileY, 25, 25, movement_bossX + 150, movement_bossY + 80, boss_hitbox1W, boss_hitbox1H) ou
				verificarColisaoPr(projectileX, projectileY, 25, 25, movement_bossX + 45, movement_bossY + 160, boss_hitbox2W, boss_hitbox2H) ou
				verificarColisaoPr(projectileX, projectileY, 25, 25, movement_bossX + 150, movement_bossY + 160, boss_hitbox3W, boss_hitbox3H)) {

				boss_hpPoints-= 1
				xEffect = projectileX
				yEffect = projectileY
				timer_hit = 100
				desenhar_hit = verdadeiro
				sm.reproduzir_som(enemy_when_dmg, falso)
				
				projectileX = 1800
				se (boss_hpPoints <= 0) {
					isBossDead = verdadeiro
					totalPoints+= 40000
					yPoint = movementY
					desenhar_point = verdadeiro
					timer_point = 100
					desenhar_efeito_enemy1 = verdadeiro
					timer_efeito_enemy1 = 1000
					sm.reproduzir_som(boss_death_sound, falso)
					ppreset++
					enemies_killed+= 10
					iniciar_winmenu = verdadeiro
				}
			}

		//special atk
			se (verificarColisaoPr(SprojectileX, SprojectileY, 25, 25, movement_bossX + 150, movement_bossY + 80, boss_hitbox1W, boss_hitbox1H) ou
				verificarColisaoPr(SprojectileX, SprojectileY, 25, 25, movement_bossX + 45, movement_bossY + 160, boss_hitbox2W, boss_hitbox2H) ou
				verificarColisaoPr(SprojectileX, SprojectileY, 25, 25, movement_bossX + 150, movement_bossY + 160, boss_hitbox3W, boss_hitbox3H)) {

				boss_hpPoints-= 5
				xEffect = SprojectileX
				yEffect = SprojectileY
				timer_hit_sp = 60
				desenhar_hit_sp = verdadeiro
				SprojectileX = 1800
				sm.reproduzir_som(enemy_when_dmg, falso)	
				projectileX = 1500
				se (boss_hpPoints <= 0) {
					isBossDead = verdadeiro
					totalPoints+= 40000
					yPoint = movementY
					desenhar_point = verdadeiro
					timer_point = 100
					desenhar_efeito_enemy1 = verdadeiro
					timer_efeito_enemy1 = 1000
					sm.reproduzir_som(boss_death_sound, falso)
					ppreset++
					enemies_killed+= 10
					iniciar_winmenu = verdadeiro
				}
			}


			
			
			// O chefe só ataca se estiver na tela e vivo
			se (appearBoss e movement_bossX <= 750 e boss_hpPoints > 0) {
   
    se (h_laser_estado_0 == 0 e h_laser_estado_1 == 0 e v_laser_estado_0 == 0 e v_laser_estado_1 == 0) {
        timer_proximo_ataque_laser--
    }

    se (timer_proximo_ataque_laser <= 0 e h_laser_estado_0 == 0 e h_laser_estado_1 == 0 e v_laser_estado_0 == 0 e v_laser_estado_1 == 0) {
        
       
        se (proximo_ataque_tipo == "HORIZONTAL") {
        
            h_laser_estado_0 = 1
            h_laser_estado_1 = 1
            h_laser_timer_0 = 110 
            h_laser_timer_1 = 110
            h_laser_y_0 = u.sorteia(80, 300)
            h_laser_y_1 = u.sorteia(350, 600)
            
    
            proximo_ataque_tipo = "VERTICAL"
 
            timer_proximo_ataque_laser = 1500 
        } 
        
        senao se (proximo_ataque_tipo == "VERTICAL") {
            v_laser_estado_0 = 1
            v_laser_estado_1 = 1
            v_laser_timer_0 = 110 // Duração do traçado
            v_laser_timer_1 = 110
            v_laser_x_0 = u.sorteia(75, 500)
            v_laser_x_1 = u.sorteia(400, 550)
            
            
            proximo_ataque_tipo = "HORIZONTAL"
      
            timer_proximo_ataque_laser = 2000 
        }
    }
}

			se (h_laser_estado_0 == 1) {
				g.desenhar_imagem(0, h_laser_y_0, laser_trace)
				h_laser_timer_0--
				se (h_laser_timer_0 <= 0) {
					h_laser_estado_0 = 2
					h_laser_timer_0 = 150
					
				}
			} senao se (h_laser_estado_0 == 2) {
				sm.reproduzir_som(som_laser_beam, falso)
				g.desenhar_imagem(0, h_laser_y_0 - 125, laser_beam)
				se (verificarColisaoPr(movementX, movementY, 100, 83, 0, h_laser_y_0, 1200, 30)) {
					mainC_health -= 5
					h_laser_estado_0 = 0
				}
				h_laser_timer_0--
				se (h_laser_timer_0 <= 0) {
					h_laser_estado_0 = 0
				}
			}
			
			se (h_laser_estado_1 == 1) {
				g.desenhar_imagem(0, h_laser_y_1, laser_trace)
				h_laser_timer_1--
				se (h_laser_timer_1 <= 0) {
					h_laser_estado_1 = 2
					h_laser_timer_1 = 150
				}
			} senao se (h_laser_estado_1 == 2) {
				sm.reproduzir_som(som_laser_beam, falso)
				g.desenhar_imagem(0, h_laser_y_1 - 125, laser_beam)
				se (verificarColisaoPr(movementX, movementY, 100, 83, 0, h_laser_y_1, 1200, 30)) {
					mainC_health -= 5
					h_laser_estado_1 = 0
				}
				h_laser_timer_1--
				se (h_laser_timer_1 <= 0) {
					h_laser_estado_1 = 0
				}
			}

			se (v_laser_estado_0 == 1) {
				g.desenhar_imagem(v_laser_x_0, 0, laser_trace_vertical)
				v_laser_timer_0--
				se (v_laser_timer_0 <= 0) {
					v_laser_estado_0 = 2
					v_laser_timer_0 = 150
					sm.reproduzir_som(som_laser_beam, falso)
				}
			} senao se (v_laser_estado_0 == 2) {
				g.desenhar_imagem(v_laser_x_0 - 150, -150, laser_beam_vertical)
				se (verificarColisaoPr(movementX, movementY, 100, 83, v_laser_x_0, 0, 30, 720)) {
					mainC_health -= 5
					v_laser_estado_0 = 0
				}
				v_laser_timer_0--
				se (v_laser_timer_0 <= 0) {
					v_laser_estado_0 = 0
				}
			}

			se (v_laser_estado_1 == 1) {
				g.desenhar_imagem(v_laser_x_1, 0, laser_trace_vertical)
				v_laser_timer_1--
				se (v_laser_timer_1 <= 0) {
					v_laser_estado_1 = 2
					v_laser_timer_1 = 150
					sm.reproduzir_som(som_laser_beam, falso)
				}
			} senao se (v_laser_estado_1 == 2) {
				g.desenhar_imagem(v_laser_x_1 - 150, -150, laser_beam_vertical)
				se (verificarColisaoPr(movementX, movementY, 100, 83, v_laser_x_1, 0, 30, 720)) {
					mainC_health -= 5
					v_laser_estado_1 = 0
				}
				v_laser_timer_1--
				se (v_laser_timer_1 <= 0) {
					v_laser_estado_1 = 0
				}
			}
			
			//chuva de míssel
			se (appearBoss e movement_bossX <= 750 e boss_hpPoints > 0) {
				timer_missile_atk--
				se (timer_missile_atk <= 0) {
					timer_missile_atk = 180
			
					se (nao missile_ativo[0]) {
						missile_ativo[0] = verdadeiro
						sm.reproduzir_som(missile_sound, falso)
						missileX[0] = movement_bossX + 50
						missileY[0] = u.sorteia(85, 570)
					} senao se (nao missile_ativo[1]) {
						missile_ativo[1] = verdadeiro
							sm.reproduzir_som(missile_sound, falso)
						missileX[1] = movement_bossX + 50
						missileY[1] = u.sorteia(85, 570)
					} senao se (nao missile_ativo[2]) {
						missile_ativo[2] = verdadeiro
							sm.reproduzir_som(missile_sound, falso)
						missileX[2] = movement_bossX + 50
						missileY[2] = u.sorteia(85, 570)
					} senao se (nao missile_ativo[3]) {
						missile_ativo[3] = verdadeiro
							sm.reproduzir_som(missile_sound, falso)
						missileX[3] = movement_bossX + 50
						missileY[3] = u.sorteia(85, 570)
					} senao se (nao missile_ativo[4]) {
						missile_ativo[4] = verdadeiro
							sm.reproduzir_som(missile_sound, falso)
						missileX[4] = movement_bossX + 50
						missileY[4] = u.sorteia(85, 570)
					}
				}
			}

			se (missile_ativo[0]) {
				missileX[0] -= 3
				
			
				g.desenhar_imagem(missileX[0], missileY[0], boss_missile)
				se (verificarColisaoPr(missileX[0], missileY[0], 30, 15, movementX, movementY, 100, 83)) {
					mainC_health -= 1
					sm.reproduzir_som(mainC_dmg, falso)
					missile_ativo[0] = falso
				}
				se (missileX[0] < -50) {
					missile_ativo[0] = falso
				}
			}

			se (missile_ativo[1]) {
			
				missileX[1] -= 3
				
				g.desenhar_imagem(missileX[1], missileY[1], boss_missile)
				se (verificarColisaoPr(missileX[1], missileY[1], 30, 15, movementX, movementY, 100, 83)) {
					mainC_health -= 1
					sm.reproduzir_som(mainC_dmg, falso)
					missile_ativo[1] = falso
				}
				se (missileX[1] < -75) {
					missile_ativo[1] = falso
				}
			}

			se (missile_ativo[2]) {
				missileX[2] -= 3
		
			
				g.desenhar_imagem(missileX[2], missileY[2], boss_missile)
				se (verificarColisaoPr(missileX[2], missileY[2], 30, 15, movementX, movementY, 100, 83)) {
					mainC_health -= 1
					sm.reproduzir_som(mainC_dmg, falso)
					missile_ativo[2] = falso
				}
				se (missileX[2] < -120) {
					missile_ativo[2] = falso
				}
			}

			se (missile_ativo[3]) {
				missileX[3] -= 3
			
				g.desenhar_imagem(missileX[3], missileY[3], boss_missile)
				se (verificarColisaoPr(missileX[3], missileY[3], 30, 15, movementX, movementY, 100, 83)) {
					mainC_health -= 1
					sm.reproduzir_som(mainC_dmg, falso)
					missile_ativo[3] = falso
				}
				se (missileX[3] < -120) {
					missile_ativo[3] = falso
				}
			}
		
			se (missile_ativo[4]) {
			
				missileX[4] -= 3
			
				g.desenhar_imagem(missileX[4], missileY[4], boss_missile)
				se (verificarColisaoPr(missileX[4], missileY[4], 30, 15, movementX, movementY, 100, 83)) {
					mainC_health -= 1
					sm.reproduzir_som(mainC_dmg, falso)
					missile_ativo[4] = falso
				}
				se (missileX[4] < -75) {
					missile_ativo[4] = falso
				}
			}
		}
	}

	inteiro timerwinmenu = 700
	logico iniciar_winmenu = falso


	funcao colisoes() {

		se(appearBoss == falso) {
			// inimigo 1
			se(movement_enemyX < 1200) {
				se (verificarColisaoPr(projectileX, projectileY, 25, 25, movement_enemyX + 6, movement_enemyY - 37, 65, 58)) {
					xEffect = projectileX
					yEffect = projectileY
					timer_hit = 100
					desenhar_hit = verdadeiro
					sm.reproduzir_som(enemy_when_dmg, falso)
					projectileX = 1300
					enemy1_health--
				
					se (enemy1_health == 0) {
						x_projectile = 900
						xEffect = movement_enemyX
						yEffect = movement_enemyY
						totalPoints+= 800
						yPoint = movementY
						desenhar_point = verdadeiro
						timer_point = 100
						desenhar_efeito_enemy1 = verdadeiro
						timer_efeito_enemy1 = 100
						sm.reproduzir_som(enemy_dmg, falso)
						movement_enemyX = 1400
						movement_enemyY = u.sorteia(300, 950)
						isEnemyAlive = falso
						enemies_killed++
						ppreset++
						
						se (nao isEnemyAlive) {
							enemy1_health = 4
							isEnemyAlive = verdadeiro
						}
					}
				}
			}

			//inimigo 1 special attack colisao
			se(movement_enemyX < 1200) {
				se (verificarColisaoPr(SprojectileX, SprojectileY, 50, 25, movement_enemyX, movement_enemyY - 37, 65, 58)) {
					xEffect = SprojectileX
					yEffect = SprojectileY
					timer_hit_sp = 60
					desenhar_hit_sp = verdadeiro
					SprojectileX = 1300
					enemy1_health -= 4
			
					se (enemy1_health <= 0) {
						xEffect = movement_enemyX
						yEffect = movement_enemyY
						totalPoints+= 2323
						yPoint = movementY
						desenhar_point = verdadeiro
						timer_point = 100
						desenhar_efeito_enemy1 = verdadeiro
						timer_efeito_enemy1 = 100
						sm.reproduzir_som(enemy_dmg, falso)
						movement_enemyX = 1400
						movement_enemyY = u.sorteia(200, 950)
						isEnemyAlive = falso
						enemies_killed++
						ppreset+= 2
			
						se (nao isEnemyAlive) {
							enemy1_health = 4
							isEnemyAlive = verdadeiro
						}
					}
				}
			}
			
			//inimigo 2 special attack colisao
			se(movement_enemy2X < 1200) {
				se (verificarColisaoPr(SprojectileX, SprojectileY, 50, 25, movement_enemy2X, movement_enemy2Y - 37, 65, 58)) {
					xEffect = SprojectileX
					yEffect = SprojectileY
					timer_hit_sp = 60
					desenhar_hit_sp = verdadeiro
					SprojectileX = 1300
					enemy2_health -= 4
			
					se (enemy2_health <= 0) {
						xEffect = movement_enemy2X
						yEffect = movement_enemy2Y
						totalPoints+= 2967
						yPoint = movementY
						desenhar_point = verdadeiro	
						timer_point = 100
						desenhar_efeito_enemy2 = verdadeiro
						timer_efeito_enemy2 = 100
						sm.reproduzir_som(enemy_dmg, falso)
						movement_enemy2X = 1400
						sorteio = verdadeiro
						isEnemy2Alive = falso
						enemies_killed++
						
						se (nao isEnemy2Alive) {
							enemy2_health = 4
							isEnemy2Alive = verdadeiro
						}
					}
				}
			}
			
			// inimigo 2
			se(movement_enemy2X < 1200) {
				se (verificarColisaoPr(projectileX, projectileY, 25, 25, movement_enemy2X, movement_enemy2Y - 37, 65, 58)) {
					xEffect = projectileX
					yEffect = projectileY
					timer_hit = 100
					desenhar_hit = verdadeiro
					sm.reproduzir_som(enemy_when_dmg, falso)
					projectileX = 1300
					enemy2_health--
			
					se (enemy2_health == 0) {
						x_projectile2 = 850
						xEffect = movement_enemy2X
						yEffect = movement_enemy2Y
						totalPoints+= 876
						yPoint = movementY
						desenhar_point = verdadeiro
						timer_point = 100
						desenhar_efeito_enemy2 = verdadeiro
						timer_efeito_enemy2 = 100
						sm.reproduzir_som(enemy_dmg, falso)
						movement_enemy2X = 1400
						sorteio = verdadeiro
						isEnemy2Alive = falso
						enemies_killed++
						ppreset++
						timerpp = 1520
						ppSortY = u.sorteia(50, 500)
						ppX = 1300
			
						se (nao isEnemy2Alive) {
							isEnemy2Alive = verdadeiro
						}
						enemy2_health = 4
					}
				}
			}
		
			// inimigo 2 atira no main character
			se (verificarColisaoPr(x_projectile2, movement_enemy2Y + 10, 25, 25, movementX, movementY, 100, 83)) {
				sm.reproduzir_som(mainC_dmg, falso)
				x_projectile2 = -60
				mainC_health--
			}
		
			// inimigo 1 atira no main character
			se (verificarColisaoPr(x_projectile, movement_enemyY + 10, 25, 25, movementX, movementY, 100, 83)) {
				sm.reproduzir_som(mainC_dmg, falso)
				x_projectile = -60
				mainC_health--
			}
		} 

		// Bloco de colisão dos Power-ups (sempre ativo)
		se (verificarColisaoPr(movementX, movementY, 100, 83, ppX, ppY, 62, 69)) {
			sm.reproduzir_som(powerup_sound, falso)
			ppreset = 0
			ppX = -300
			mainC_health+= 6
			whenhp_up = verdadeiro
			timerhp_up = 100

			se(mainC_health >= 30) {
				mainC_health = 30
			}
		}
	}

	inteiro barrier_alert = g.carregar_imagem("alert.gif")
	funcao effect() {

		se (whenhp_up) {
			g.desenhar_imagem(movementX + 40, movementY - 30, hp_up)
			timerhp_up--

			se (timerhp_up <= 0) {
				whenhp_up = falso
			}
		}

		se (desenhar_hit) {
			g.desenhar_imagem(xEffect + 35, yEffect + 50, hit)
			timer_hit--

			se (timer_hit <= 0) {
				desenhar_hit = falso
			}
		}
		
		se (desenhar_hit_sp) {
			g.desenhar_imagem(xEffect, yEffect, sp_hit)
			timer_hit_sp--

			se (timer_hit_sp <= 0) {
				desenhar_hit_sp = falso
			}
		}
		se (desenhar_efeito_enemy1) {
			g.desenhar_imagem(xEffect, yEffect, enemy_death)
			timer_efeito_enemy1--

			se (timer_efeito_enemy1 <= 0) {
				desenhar_efeito_enemy1 = falso
			}
		}

		se (desenhar_efeito_enemy2) {
			g.desenhar_imagem(xEffect, yEffect, enemy_death)
			timer_efeito_enemy2--

			se (timer_efeito_enemy2 <= 0) {
				desenhar_efeito_enemy2 = falso
			}
		}

		se(barrier) {
			g.desenhar_imagem(movementX + 70, movementY - 40, barrier_alert)
			timer_alert--
			se(timer_alert <= 0) {
				barrier = falso
			}
		}
	}

	inteiro ppreset = 0
	inteiro ppreset2 = 0
	inteiro ppY = 0, ppX = -300
	inteiro health_box = g.carregar_imagem("health_box.gif")
	inteiro timerpp = 0
	inteiro ppSortY = 0
	logico powertrue = falso

	inteiro powerup_sound = sm.carregar_som("powerUp_Sound.mp3")
	funcao powerUp() {
		se(ppreset >= 13) {
			ppY = ppSortY
			se(ppX >= -200) {
				ppX--
				g.desenhar_imagem(ppX, ppY, health_box)
				
				timerpp--
				se(ppX == -200) {
					ppreset = 0
				}
			}
		}
	}

	inteiro boss_hp[13]

	funcao boss_health_bar() {
		g.definir_cor(hitboxColor)
		g.definir_estilo_texto(falso, verdadeiro, falso)
		g.definir_tamanho_texto(16.0)
		boss_hp[0] = g.carregar_imagem("hp_boss.png")
		boss_hp[1] = g.carregar_imagem("hp_boss1.png")
		boss_hp[2] = g.carregar_imagem("hp_boss2.png")
		boss_hp[3] = g.carregar_imagem("hp_boss3.png")
		boss_hp[4] = g.carregar_imagem("hp_boss4.png")
		boss_hp[5] = g.carregar_imagem("hp_boss5.png")
		boss_hp[6] = g.carregar_imagem("hp_boss6.png")
		boss_hp[7] = g.carregar_imagem("hp_boss7.png")
		boss_hp[8] = g.carregar_imagem("hp_boss8.png")
		boss_hp[9] = g.carregar_imagem("hp_boss9.png")
		boss_hp[10] = g.carregar_imagem("hp_boss10.png")
		boss_hp[11] = g.carregar_imagem("hp_boss11.png")
		boss_hp[12] = g.carregar_imagem("hp_boss12.png")
	}

	funcao boss_health_bar_att() {
		g.desenhar_texto(967, 632, "Executor V-9")
		g.desenhar_imagem(1070, 600, b_portrait)

		se (boss_hpPoints > 99) {
			g.desenhar_imagem(745, 650, boss_hp[0])
		}
		senao se (boss_hpPoints > 90) {
			g.desenhar_imagem(745, 650, boss_hp[1])
		}
		senao se (boss_hpPoints > 81) {
			g.desenhar_imagem(745, 650, boss_hp[2])
		}
		senao se (boss_hpPoints > 72) {
			g.desenhar_imagem(745, 650, boss_hp[3])
		}
		senao se (boss_hpPoints > 63) {
			g.desenhar_imagem(745, 650, boss_hp[4])
		}
		senao se (boss_hpPoints > 54) {
			g.desenhar_imagem(745, 650, boss_hp[5])
		}
		senao se (boss_hpPoints > 45) {
			g.desenhar_imagem(745, 650, boss_hp[6])
		}
		senao se (boss_hpPoints > 36) {
			g.desenhar_imagem(745, 650, boss_hp[7])
		}
		senao se (boss_hpPoints > 27) {
			g.desenhar_imagem(745, 650, boss_hp[8])
		}
		senao se (boss_hpPoints > 18) {
			g.desenhar_imagem(745, 650, boss_hp[9])
		}
		senao se(boss_hpPoints > 9) {
			g.desenhar_imagem(745, 650, boss_hp[10])
		}
		senao se(boss_hpPoints > 0) {
			g.desenhar_imagem(745, 650, boss_hp[11])
		}
		senao {
			g.desenhar_imagem(745, 650, boss_hp[12])
		}
	}


	inteiro mchar_health_bar[10]
	inteiro mchar_sp_bar[5]
	inteiro portrait = g.carregar_imagem("portrait.png")
	inteiro hp_up = g.carregar_imagem("hp_up.gif")
	logico whenhp_up = falso
	inteiro timerhp_up = 0
	funcao mainC_health_bar() {
		mchar_health_bar[0] = g.carregar_imagem("hp_bar.png")
		mchar_health_bar[1] = g.carregar_imagem("hp-1.png")
		mchar_health_bar[2] = g.carregar_imagem("hp-2.png")
		mchar_health_bar[3] = g.carregar_imagem("hp-3.png")
		mchar_health_bar[4] = g.carregar_imagem("hp-4.png")
		mchar_health_bar[5] = g.carregar_imagem("hp-5.png")
		mchar_health_bar[6] = g.carregar_imagem("hp-6.png")
		mchar_health_bar[7] = g.carregar_imagem("hp-7.png")
		mchar_health_bar[8] = g.carregar_imagem("hp-8.png")
		
		mchar_sp_bar[0] = g.carregar_imagem("special.png")
		mchar_sp_bar[1] = g.carregar_imagem("special-1.png")
		mchar_sp_bar[2] = g.carregar_imagem("special-2.png")
		mchar_sp_bar[3] = g.carregar_imagem("special-3.png")
		mchar_sp_bar[4] = g.carregar_imagem("special-4.png")
	}

	funcao mainC_health_bar_att() {
		g.desenhar_imagem(20, 600, portrait)

		se(mainC_health > 27) {
			g.desenhar_imagem(140, 650, mchar_health_bar[0])
		}
		senao se(mainC_health > 24 e mainC_health <= 27) {
			g.desenhar_imagem(140, 650, mchar_health_bar[1])
		}
		senao se (mainC_health > 21 e mainC_health <= 24) {
			g.desenhar_imagem(140, 650, mchar_health_bar[2])
		}
		senao se (mainC_health > 18 e mainC_health <= 21) {
			g.desenhar_imagem(140, 650, mchar_health_bar[3])
		}
		senao se (mainC_health > 15 e mainC_health <= 18) {
			g.desenhar_imagem(140, 650, mchar_health_bar[4])
		}
		senao se (mainC_health > 12 e mainC_health <= 15) {
			g.desenhar_imagem(140, 650, mchar_health_bar[5])
		}
		senao se (mainC_health > 5 e mainC_health <= 12) {
			g.desenhar_imagem(140, 650, mchar_health_bar[6])
		}
		senao se (mainC_health >= 1 e mainC_health <= 8) {
			g.desenhar_imagem(140, 650, mchar_health_bar[7])
		}
		senao se (mainC_health <= 0) {
			g.desenhar_imagem(140, 650, mchar_health_bar[8])
			gameover()
		}

		se(SpCharge < 2) {
			g.desenhar_imagem(140, 680, mchar_sp_bar[4])
		}
		senao se(SpCharge >= 2 e SpCharge < 4) {
			g.desenhar_imagem(140, 680, mchar_sp_bar[3])
		}
		senao se (SpCharge >= 4 e SpCharge < 6) {
			g.desenhar_imagem(140, 680, mchar_sp_bar[2])
		}
		senao se (SpCharge >= 6 e SpCharge < 10) {
			g.desenhar_imagem(140, 680, mchar_sp_bar[1])
		}
		senao se (SpCharge >= 10) {
			g.desenhar_imagem(140, 680, mchar_sp_bar[0])
		}
	}

	funcao gameover() {
		sm.definir_volume_reproducao(somMenu, 0)
		gameovermenu()
		reset()
		isPlaying = falso

	}

	funcao gameovermenu() {
		sm.reproduzir_som(game_over, falso)
	

	}

	inteiro larguraHitBox = 100
	inteiro alturaHitBox = 83
	inteiro hitboxColor = g.criar_cor(0, 0, 0)
	funcao main_character_hitbox() {
		g.definir_cor(hitboxColor)
		g.desenhar_retangulo(movementX, movementY, larguraHitBox, alturaHitBox, falso, falso)
		g.desenhar_retangulo(projectileX, projectileY + 45, 25, 25, falso, falso)
	}

	inteiro movement_enemyY = 350
	inteiro x_projectile = 900
	inteiro largura_enemy_hit = x_projectile - 5
	inteiro altura_enemy_hit = movement_enemyY + 10

	funcao enemy_hitbox() {
		g.desenhar_retangulo(movement_enemyX, movement_enemyY, 65, 58, falso, falso)
		g.desenhar_retangulo(x_projectile - 5, movement_enemyY + 10, 25, 25, falso, falso)
	}

	//load arquivos e variaveis globais referentes ao mapa do jogo
	inteiro background = g.carregar_imagem("background.png")
	inteiro ground = g.carregar_imagem("ground.png")
	
	inteiro mainC_projectile = g.carregar_imagem("projectile.png")
	inteiro sun = g.carregar_imagem("sun.gif")
	
	inteiro cloud1 = g.carregar_imagem("cloud.png")
	inteiro cloud2 = cloud1
	inteiro map_cloud_movementX = 2
	inteiro cloud_height1 = 15
	inteiro cloud_height2 = 55
	inteiro x_cloud1 = 400
	inteiro x_cloud2 = 750
	
	inteiro x_ground1 = 0
	inteiro x_ground2 = 1199
	inteiro map_ground_movementX = 1
	funcao drawMap() {
		g.desenhar_imagem(0,0, background)
		g.desenhar_imagem(x_ground1 = x_ground1 - map_ground_movementX, 530, ground)
		g.desenhar_imagem(x_ground2 = x_ground2 - map_ground_movementX, 530, ground)
		g.desenhar_imagem(510,70, sun)
		g.desenhar_imagem(x_cloud1 -= map_cloud_movementX, cloud_height1, cloud1)
		g.desenhar_imagem(x_cloud2 -= map_cloud_movementX, cloud_height2, cloud2)
		g.definir_cor(hitboxColor)
		g.definir_estilo_texto(falso, verdadeiro, falso)
		g.definir_tamanho_texto(16.0)
		g.desenhar_texto(165, 632, "B. Crocodilo")

		se (x_cloud1 == -200) {
			x_cloud1 = 2000
			cloud_height1 = cloud_height1 + 45
			
			se (cloud_height1 > 200) {
				cloud_height1 = 33
			}
		}
		se (x_cloud2 == -200) {
			x_cloud2 = 2000
			cloud_height2 = cloud_height2 + 75
			se (cloud_height2 > 300) {
				cloud_height2 = 78
			}
		}
		se (x_ground1 == -1200) {
			x_ground1 = 0
		}
		se (x_ground2 == 0) {
			x_ground2 = 1200
		}
	}

	// variaveis globais attack
	logico isAttacking = falso
	logico isSpecialAtk = falso
	inteiro SpCharge = 0
	inteiro projectileX = 0
	inteiro projectileY = 0
	inteiro SprojectileY = 0
	inteiro SprojectileX = 0
	inteiro mainC_projectile_sound = sm.carregar_som("mainC_projectile_sound.mp3")
	inteiro Sprojectile = g.carregar_imagem("Sprojectile_effect.gif")
	funcao main_character_attack() {
		se (t.tecla_pressionada(t.TECLA_ESPACO) e nao isAttacking) {
			SpCharge++
			isAttacking = verdadeiro
			sm.reproduzir_som(mainC_projectile_sound, falso)
			projectileY = movementY
			projectileX = movementX
		}

		se (isAttacking) {
			projectileX += 5
			g.desenhar_imagem(projectileX, projectileY + 45, mainC_projectile)

			se (projectileX > 1225) {
				isAttacking = falso
			}
		}

		se(SpCharge >= 10) {
			SpCharge = 10
		}
		se (t.tecla_pressionada(t.TECLA_C) e nao isSpecialAtk e SpCharge >= 10) {
			isSpecialAtk = verdadeiro
			sm.reproduzir_som(sp_dmg, falso)
			SprojectileY = movementY
			SprojectileX = movementX
			SpCharge -= 10
		}

		se(isSpecialAtk) {
			SprojectileX+= 5
			g.desenhar_imagem(SprojectileX, SprojectileY, Sprojectile)
			se(SprojectileX > 2000) {
				isSpecialAtk = falso
			}
		}
	}

	inteiro enemy_projectile = g.carregar_imagem("enemy_projectile.png")
	inteiro movement_enemyX = 1400
	logico is_enemy_attacking = falso
	inteiro enemy_projectile_sound = sm.carregar_som("enemy_projectile_sound.mp3")
	funcao initial_movement_enemy1() {
		movement_enemy2()
		se (movement_enemyX >= 900) {
			movement_enemyX = movement_enemyX - 1
		}
		se (movement_enemyX == 899) {
			se (movement_enemyY != 225) {
				movement_enemyY = movement_enemyY - 1
			}
			se(movement_enemyY == 225 e x_projectile > -200) {
				x_projectile -= 2
				g.desenhar_imagem(x_projectile, movement_enemyY + 10, enemy_projectile)
				is_enemy_attacking = verdadeiro

				se (is_enemy_attacking e x_projectile <= -200) {
					x_projectile = 900
				}
			}
		}

		se (x_projectile == 840) {
			sm.reproduzir_som(enemy_projectile_sound, falso)
		}
	}

	inteiro movement_enemy2X = 1700
	inteiro movement_enemy2Y = 400
	logico sorteio = falso
	inteiro sorteadoY = 0
	inteiro x_projectile2 = 0
	inteiro y_projectile2 = 0
	funcao movement_enemy2() {
		se (sorteio == verdadeiro) {
			sorteadoY = u.sorteia(250, 500)
			movement_enemy2Y = sorteadoY
			sorteio = falso
		}

		se (movement_enemy2X != 850) {
			movement_enemy2X-= 1
		}
		se(movement_enemy2X == 850 e x_projectile2 < -200) {
			x_projectile2 = movement_enemy2X
			y_projectile2 = 400
		}
		
		se (x_projectile2 > -200 e movement_enemy2X == 850) {
			x_projectile2 -= 2
			g.desenhar_imagem(x_projectile2, movement_enemy2Y, enemy_projectile)
		}
		se (x_projectile2 == -200) {
			x_projectile2 = 850
		}
		se (x_projectile2 == 840) {
			sm.reproduzir_som(enemy_projectile_sound, falso)
		}
	}

	inteiro boss_idle = g.carregar_imagem("newboss.gif")
	inteiro timer_boss_start = 1000
	inteiro timer_boss_idle = 0
	logico appearBoss = falso
	logico isBossNormalAtk = falso
	inteiro cont_anim_idle = 0
	inteiro timer_boss_appear = 2500
	inteiro b_portrait = g.carregar_imagem("b_portrait.png")
	inteiro boss_hpPoints = 108
	funcao boss_fight() {
		se (enemies_killed >= 15) {  //QUANTIDADE DE INIMIGOS NECESSÁRIO PARA LIBERAR O BOSS
			enemy1_health = 0
			enemy2_health = 0
			movement_enemy2X = 1500
			movement_enemyX = 1500
			appearBoss = verdadeiro
		}
		
		se(appearBoss) {
			boss_health_bar_att()
			boss_effects()
			timer_boss_appear--
			
			se(timer_boss_start > 0) {
				timer_boss_start--
			}
			se(timer_boss_start == 0) {
				timer_boss_start = 0
				timer_boss_idle = 100
			}
			se(timer_boss_idle > 0) {
				g.desenhar_imagem(movement_bossX, movement_bossY, boss_idle)
				se(timer_boss_idle <= 0) {
					timer_boss_idle = 100
				}
			}
			se(timer_boss_idle < 0) {
				timer_boss_idle = 100
			}
			se(movement_bossX > 750) {
				boss_hpPoints = 108 //Boss não perde vida até chegar na posição idle
				timer_boss_idle--
				movement_bossX--
				se(movement_bossX == 749 e nao isBossNormalAtk) {
				
				}
			}
			g.desenhar_imagem(1070, 600, b_portrait)
		}
	}

	inteiro yPoint = 0
	inteiro totalPoints = 0
	inteiro timer_point = 0
	logico desenhar_point = falso
	inteiro coinUp = g.carregar_imagem("coin.gif")
	funcao points() {
		se (desenhar_point) {
			se(yPoint != 0) {
				yPoint--
			}
			g.desenhar_imagem(movementX + 60, yPoint - 20, coinUp)
			timer_point--

			se (timer_point <= 0) {
				desenhar_point = falso
			}
		}
	}

	funcao reset() {
		mainC_health = 30
		movementX = 100
		movementY = 319
		movement_enemyX = 1400
		movement_enemyY = 550
		movement_enemy2X = 1400
		movement_enemy2Y = 350
		enemy1_health = 4
		enemy2_health = 4
		SpCharge = 0
		enemies_killed = 0
		totalPoints = 0
		appearBoss = falso
		sm.definir_posicao_atual_musica(somMenu, 0)
		sm.definir_volume_reproducao(somMenu, 75)
		boss_hpPoints = 108 
		movement_bossX = 1700
		movement_bossY = 90
		h_laser_estado_0 = 0
		h_laser_estado_1 = 0
		v_laser_estado_0 = 0
		v_laser_estado_1 = 0
		timer_laser_principal = 400
		timer_laser_vertical_especial = 840
	}

	inteiro mainCharacter = g.carregar_imagem("character.png")
	inteiro enemy1 = g.carregar_imagem("enemy1.gif")
	logico isPlaying = falso
	funcao tick() {
		audioMenu()
		isPlaying = verdadeiro
		isEnemyAlive = verdadeiro
		mainC_health_bar()
		boss_health_bar()
		
		enquanto (isPlaying == verdadeiro) {
			drawMap()
			g.desenhar_imagem(movementX, movementY, mainCharacter)
			main_character_attack()
			mainC_health_bar_att()
			movement_main_character()
			colisoes() 
			powerUp()
			points()
			
			se (appearBoss == falso) {
				g.desenhar_imagem(movement_enemyX, movement_enemyY, enemy1)
				g.desenhar_imagem(movement_enemy2X, movement_enemy2Y, enemy1)
				initial_movement_enemy1()
			} 
			boss_fight()	
			effect()

			se(iniciar_winmenu == verdadeiro e timerwinmenu >= 0) {
				timerwinmenu--
			}
			se(timerwinmenu <= 0 e iniciar_winmenu == verdadeiro) { 
				winMenu()
			}
			
			g.renderizar()
			u.aguarde(3)
			
			se (t.tecla_pressionada(t.TECLA_ESC)) {
				reset()
				sm.definir_volume_reproducao(somMenu, 0)
				isPlaying = falso
			}
		}
	}

	funcao inicio()
	{
		menu()
	}
}
/* $$$ Portugol Studio $$$ 
 * 
 * Esta seção do arquivo guarda informações do Portugol Studio.
 * Você pode apagá-la se estiver utilizando outro editor.
 * 
 * @POSICAO-CURSOR = 34954; 
 * @PONTOS-DE-PARADA = ;
 * @SIMBOLOS-INSPECIONADOS = ;
 * @FILTRO-ARVORE-TIPOS-DE-DADO = inteiro, real, logico, cadeia, caracter, vazio;
 * @FILTRO-ARVORE-TIPOS-DE-SIMBOLO = variavel, vetor, matriz, funcao;
 */