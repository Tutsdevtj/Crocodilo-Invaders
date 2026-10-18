# GOOSINI — Boss Alert

**Selecionado tecnicamente: B1**, candidato B, variante 1 do ElevenLabs. `goosini_boss_alert_final.wav`: **2,000 s, WAV PCM16 estéreo, 48 kHz, pico −3 dBFS**. É idêntico ao candidato B editado. Os sons são exportações reais do ElevenLabs Sound Effects, sem síntese substituta ou placeholders.

## Comparação

- **A2:** alternativa de alerta arcade; a energia grave não destaca uma revelação tardia com o mesmo contraste de B1. Mantida como opção sem priorizar a textura de ganso.
- **B1 — selecionado:** subida e pico grave próximos da entrada desejada do nome; eventos/acento tardio e dissipação medíveis. A decisão usa timing e contraste espectral, não simplesmente o mais alto ou com mais graves.
- **C1:** impacto grave mais cedo e queda de energia mais rápida; opção mais curta em duração ativa, apesar de o arquivo ter 2 s.

**Não houve escuta crítica pelo agente**, limitação informada antes da autorização. Não se afirma que os bipes são exatamente dois/três ou que o detalhe de ganso ficou adequado: esses aspectos, ausência de sons indesejados, perigo percebido e fadiga dependem da sua audição. A seleção B1 é técnica e a aprovação sonora final permanece pendente. Ouça a comparação e os originais.

## Sincronismo medido

| Evento | Tempo real no final |
|---|---:|
| alert_start | 0.0 ms |
| first_reveal_attack | 240.0 ms |
| reveal_animation_start | 240.0 ms |
| impact_core | 315.0 ms |
| impact_peak | 349.0 ms |
| final_decay_start | 1296.0 ms |
| active_end_minus40db | 1429.0 ms |
| file_end | 2000.0 ms |

Os eventos foram recalculados sobre o WAV final. Picos espectrais usam janelas de 20 ms a cada 1 ms; considere aproximadamente ±10 ms para os cues, não precisão semântica de um sample. `first_reveal_attack` e `reveal_animation_start` usam a mesma borda medida: são um ponto acústico e uma sugestão de animação, não dois sons diferentes. Use a rampa para começar a entrada do nome e o pico grave para o slam. O pico RMS total do alerta inicial é distinto do pico grave da revelação.

O resultado não reproduz exatamente todas as divisões do prompt: a dissipação começa antes de 1,35 s e a cauda já está muito baixa antes do fim do arquivo. Os valores acima registram isso; não se preencheu o efeito com som artificial, nem se deslocaram os cues para copiar o briefing. `active_end_minus40db` é um limiar técnico, não o fim audível garantido. Use `file_end` para liberar o diálogo com margem segura. Os JSON/CSV acompanham os valores e métodos para a integração.

## Edição, arquivos e reprodução

Remoção de DC por canal e de silêncio inicial quando necessário; final limitado a 2 s (originais 2,08 s), fade-in de 0,5 ms e fade-out de 10 ms; pico −3 dBFS. Estéreo preservado. Sem EQ, compressor, distorção, reverb, pitch-shift, time-stretch ou música adicionada aos quatro WAVs principais. Os originais são PCM16/48 kHz, sem conversão de MP3.

- `goosini_boss_alert_final.wav`: B1 selecionado.
- `goosini_boss_alert_candidate_A.wav`, `_B.wav`, `_C.wav`: A2/B1/C1, todos 2 s.
- `originais/A/1..4.wav`, B e C: 12 WAVs originais preservados. Foram apenas três gerações; a interface cria quatro opções por clique.
- `previas/01_Comparacao_A_B_C.wav`: A duas vezes, B duas vezes, C duas vezes, com pausas. RMS ativo igualado em −18 dBFS para reduzir viés de volume.
- `previas/02_Final_sobre_Musica_das_Ondas.wav`: demonstração com o tema existente “Going Up”, de Ansimuz, CC0. O alerta toca de 2 a 4 s e a música é abaixada. Não usar essa montagem como asset de gameplay. Fonte musical: https://opengameart.org/content/going-up-adventure-chiptune .

**Versões divididas não entregues:** não há fronteira silenciosa entre alerta e revelação; os elementos do mix se sobrepõem. Separá-los exigiria cortar conteúdo ativo. Não foram criados falsos stems ou arquivos vazios. A versão completa é a recomendada.

## Unity

Toque uma única instância do final ao entrar `PERIGO!`, depois que `ONDA 10` sair. Sugestão inicial: volume **0,70**, AudioSource 2D estéreo (`spatialBlend=0`), grupo UI/Boss Alert, sem loop. Para este one-shot, PCM/Decompress On Load são pontos de partida. Mantenha um relógio comum para o SFX e a animação; use os cues medidos para começar a entrada e marcar o impacto do nome. Evite tocar um novo som a cada frame/letra/partícula.

Abaixe/pare o tema das ondas ao apresentar o alerta, preserve o final e só inicie os diálogos depois do efeito. A trilha própria do boss entra quando a luta começar. As prévias são testes separados. Nenhum projeto Unity foi alterado.

## Prompts e créditos

`prompts_used.json` contém os três textos exatos enviados, compactados para 450 caracteres e com a sequência temporal explicitada. Configuração: 2,1 s, influência 80%, loop e melhoria automática desligados.

**84 créditos consumidos de 90 autorizados**, saldo 39.494 → 39.410. Três gerações, sem tentativas adicionais ou mudança de plano. `credit_record.json`, `comparison_and_provenance.json`, `TECHNICAL_QA.json` e `SHA256SUMS.txt` documentam a entrega. O uso dos sons gerados segue os termos aplicáveis à conta ElevenLabs.
