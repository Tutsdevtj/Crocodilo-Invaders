"""Sintetiza os sons do lago e o vento do ceu. Nenhum sample externo.

Uso, a partir da pasta do projeto Unity:
    python Tools/Audio/prepare_lake_audio_v34.py
"""
from pathlib import Path
import wave

import numpy as np

SR = 44100
rng = np.random.default_rng(3401)
OUT = Path("Assets/Audio/Lake")
SKY = Path("Assets/Resources/Audio/Ambience")


def t_axis(seconds):
    return np.arange(int(SR * seconds)) / SR


def band_noise(count, low, high):
    spectrum = np.fft.rfft(rng.standard_normal(count))
    freq = np.fft.rfftfreq(count, 1 / SR)
    spectrum *= (freq >= low) & (freq <= high)
    out = np.fft.irfft(spectrum, count)
    return out / max(1e-9, np.abs(out).max())


def sweep(t, start, end, curve=1.0):
    """Senoide com frequencia indo de start a end ao longo de t."""
    u = (t / t[-1]) ** curve
    freq = start + (end - start) * u
    return np.sin(2 * np.pi * np.cumsum(freq) / SR)


def env(t, attack, decay):
    return np.minimum(1, t / max(attack, 1e-4)) * np.exp(-np.maximum(0, t - attack) / decay)


def fade_edges(x, seconds=.004):
    n = min(int(SR * seconds), len(x) // 2)
    ramp = np.linspace(0, 1, n)
    x = x.copy()
    x[:n] *= ramp
    x[-n:] *= ramp[::-1]
    return x


def bloops(duration, count, level):
    out = np.zeros(int(SR * duration))
    for _ in range(count):
        at = int(rng.uniform(.02, duration * .7) * SR)
        t = t_axis(rng.uniform(.03, .07))
        tone = sweep(t, rng.uniform(600, 1000), rng.uniform(1300, 2000), .6) * env(t, .002, .018)
        end = min(len(out), at + len(tone))
        out[at:end] += tone[:end - at] * level
    return out


def splash_small():
    t = t_axis(.24)
    return band_noise(len(t), 1400, 7000) * env(t, .003, .05) * .55 + bloops(.24, 3, .4)


def splash_big():
    t = t_axis(.75)
    body = band_noise(len(t), 220, 5200) * env(t, .006, .17)
    sub = np.sin(2 * np.pi * 62 * t) * env(t, .004, .09)
    return body * .75 + sub * .5 + bloops(.75, 8, .3)


def spit():
    t = t_axis(.15)
    air = band_noise(len(t), 900, 5200) * env(t, .002, .03)
    pop = sweep(t, 420, 210) * env(t, .001, .022)
    return air * .75 + pop * .55


def tongue_lash():
    t = t_axis(.2)
    count = len(t)
    low = band_noise(count, 500, 1800)
    high = band_noise(count, 1800, 6000)
    u = t / t[-1]
    whoosh = (low * (1 - u) + high * u) * np.sin(np.pi * u) ** 1.5
    snap_at = int(count * .68)
    snap = np.zeros(count)
    click = band_noise(count - snap_at, 1500, 8000) * np.exp(-np.arange(count - snap_at) / (SR * .008))
    snap[snap_at:] = click
    return whoosh * .55 + snap * .55


def gulp():
    t = t_axis(.19)
    tone = sweep(t, 540, 150, .7)
    return (tone + .3 * np.sin(2 * np.arcsin(np.clip(tone, -1, 1)))) * np.sin(np.pi * t / t[-1]) ** .7 * .7


def burp():
    t = t_axis(.62)
    freq = 92 - 26 * (t / t[-1]) + 5 * np.sin(2 * np.pi * 17 * t)
    phase = np.cumsum(freq) / SR
    saw = 2 * (phase - np.floor(phase + .5))
    spectrum = np.fft.rfft(saw)
    spectrum[np.fft.rfftfreq(len(saw), 1 / SR) > 760] = 0
    tone = np.fft.irfft(spectrum, len(saw))
    wobble = .72 + .28 * np.sin(2 * np.pi * 21 * t)
    shape = np.minimum(1, t / .03) * np.minimum(1, (t[-1] - t) / .09)
    return tone / max(1e-9, np.abs(tone).max()) * wobble * shape * .9


def boss_jump():
    t = t_axis(.42)
    u = t / t[-1]
    air = band_noise(len(t), 260, 1700) * np.sin(np.pi * u) ** 2
    lift = sweep(t, 130, 320) * env(t, .02, .14)
    return air * .55 + lift * .4


def boss_land():
    t = t_axis(.58)
    thump = sweep(t, 74, 38) * env(t, .003, .16)
    body = band_noise(len(t), 70, 420) * env(t, .003, .07)
    return thump * .95 + body * .5


def tire_throw():
    t = t_axis(.46)
    u = t / t[-1]
    spin = .6 + .4 * np.sin(2 * np.pi * 11 * t)
    return band_noise(len(t), 320, 2500) * np.sin(np.pi * u) ** 1.4 * spin * .75


def tire_bounce():
    t = t_axis(.26)
    boing = sweep(t, 185, 105) * env(t, .002, .07)
    thud = band_noise(len(t), 60, 340) * env(t, .002, .02)
    return boing * .85 + thud * .45


def enemy_hit():
    t = t_axis(.085)
    return sweep(t, 950, 480) * env(t, .001, .02) * .6 + band_noise(len(t), 1200, 5000) * env(t, .001, .01) * .3


def enemy_die():
    t = t_axis(.25)
    wet = band_noise(len(t), 300, 2100) * env(t, .002, .05)
    drop = sweep(t, 430, 90) * env(t, .002, .07)
    return wet * .55 + drop * .7


def slap():
    t = t_axis(.12)
    crack = band_noise(len(t), 1800, 9000) * env(t, .001, .012)
    body = band_noise(len(t), 150, 900) * env(t, .001, .03)
    return crack * .8 + body * .55


def boss_hit():
    t = t_axis(.1)
    return sweep(t, 190, 120) * env(t, .001, .03) * .8 + band_noise(len(t), 300, 1600) * env(t, .001, .012) * .3


def periodic_noise(count, low, high, slope=0.0):
    """Ruido periodico: o fim emenda exatamente no comeco."""
    freq = np.fft.rfftfreq(count, 1 / SR)
    gain = ((freq >= low) & (freq <= high)) * (np.maximum(freq, 1) ** slope)
    phase = np.exp(2j * np.pi * rng.random(len(freq)))
    out = np.fft.irfft(gain * phase, count)
    return out / max(1e-9, np.abs(out).max())


def lfo(count, cycles, phase=None):
    phase = rng.uniform(0, 2 * np.pi) if phase is None else phase
    return np.sin(2 * np.pi * cycles * np.arange(count) / count + phase)


def place(buffer, at, event, gain_left, gain_right):
    index = (at + np.arange(len(event))) % buffer.shape[0]
    buffer[index, 0] += event * gain_left
    buffer[index, 1] += event * gain_right


def lake_ambience():
    seconds = 28
    count = SR * seconds
    stereo = np.zeros((count, 2))
    for channel in range(2):
        water = periodic_noise(count, 120, 1100, -.7)
        swell = .68 + .2 * lfo(count, 3) + .12 * lfo(count, 7)
        stereo[:, channel] += water * swell * .42
    for _ in range(26):  # agua batendo na margem
        t = t_axis(rng.uniform(.25, .7))
        lap = band_noise(len(t), 260, 1500) * np.sin(np.pi * t / t[-1]) ** 2
        pan = rng.uniform(.25, .75)
        place(stereo, rng.integers(count), lap * rng.uniform(.12, .24), 1 - pan, pan)
    for _ in range(46):  # grilos distantes, em frases curtas
        pulse = t_axis(.022)
        base = rng.uniform(3900, 4700)
        note = np.sin(2 * np.pi * base * pulse) * np.sin(np.pi * pulse / pulse[-1])
        at = int(rng.integers(count))
        pan = rng.uniform(.1, .9)
        level = rng.uniform(.02, .05)
        for repeat in range(rng.integers(3, 6)):
            place(stereo, at + repeat * int(SR * .046), note * level, 1 - pan, pan)
    for _ in range(9):  # sapos bem ao fundo
        length = rng.uniform(.14, .24)
        t = t_axis(length)
        base = rng.uniform(170, 260)
        tone = sweep(t, base * 1.18, base) * (np.sign(np.sin(2 * np.pi * 34 * t)) * .5 + .5)
        tone *= np.sin(np.pi * t / t[-1])
        pan = rng.uniform(.15, .85)
        at = int(rng.integers(count))
        level = rng.uniform(.035, .07)
        for repeat in range(rng.integers(1, 3)):
            place(stereo, at + repeat * int(SR * (length + .09)), tone * level, 1 - pan, pan)
    return stereo


def sky_ambience():
    seconds = 24
    count = SR * seconds
    stereo = np.zeros((count, 2))
    for channel in range(2):
        low = periodic_noise(count, 70, 520, -.8)
        high = periodic_noise(count, 500, 2600, -.5)
        gust = .62 + .25 * lfo(count, 2) + .13 * lfo(count, 5)
        whistle = .35 + .3 * lfo(count, 3)
        stereo[:, channel] = low * gust * .55 + high * whistle * .14
    return stereo


def voiced(t, f0_start, f0_end, low, high, rate):
    """Voz de sapo: pulsos graves filtrados e picotados pela garganta."""
    freq = f0_start + (f0_end - f0_start) * (t / t[-1])
    phase = np.cumsum(freq) / SR
    saw = 2 * (phase - np.floor(phase + .5))
    spectrum = np.fft.rfft(saw)
    bins = np.fft.rfftfreq(len(saw), 1 / SR)
    spectrum *= (bins >= low) & (bins <= high)
    tone = np.fft.irfft(spectrum, len(saw))
    gate = .25 + .75 * np.clip(np.sin(2 * np.pi * rate * t) * 3 + .6, 0, 1)
    return tone / max(1e-9, np.abs(tone).max()) * gate * np.maximum(0, np.sin(np.pi * t / t[-1])) ** .6


def syllables(*parts, gap=.05):
    silence = np.zeros(int(SR * gap))
    out = []
    for part in parts:
        out += [part, silence]
    return np.concatenate(out[:-1])


def frog_croak():
    return syllables(voiced(t_axis(.13), 185, 160, 340, 1500, 38), voiced(t_axis(.18), 205, 150, 340, 1500, 34))


def frog_die():
    return voiced(t_axis(.3), 230, 70, 300, 1300, 30)


def saperereca_croak():
    return syllables(voiced(t_axis(.1), 320, 280, 700, 2600, 46), voiced(t_axis(.17), 350, 250, 700, 2600, 42), gap=.04)


def sapeneu_grunt():
    return voiced(t_axis(.44), 98, 68, 110, 720, 23)


def mosquito_buzz():
    """Loop de exatamente 2 s: todos os parciais fecham o ciclo."""
    seconds, base = 2, 540
    count = SR * seconds
    t = np.arange(count) / SR
    wobble = np.sin(2 * np.pi * 3 * t) * .9 + np.sin(2 * np.pi * 7 * t) * .35
    out = np.zeros(count)
    for harmonic, gain in zip(range(1, 8), (1, .7, .55, .4, .28, .2, .12)):
        out += gain * np.sin(2 * np.pi * base * harmonic * t + harmonic * wobble)
    return out * (.82 + .18 * np.sin(2 * np.pi * 2 * t))


def fly_buzz():
    t = t_axis(.55)
    freq = 225 + 70 * np.sin(np.pi * t / t[-1])
    phase = np.cumsum(freq) / SR
    tone = sum(g * np.sin(2 * np.pi * phase * h) for h, g in zip(range(1, 7), (1, .8, .6, .4, .25, .15)))
    return tone * np.sin(np.pi * t / t[-1]) ** .8


def dragonfly_dash():
    t = t_axis(.42)
    u = t / t[-1]
    wings = np.clip(np.sin(2 * np.pi * 42 * t) * 4, -1, 1) * .5 + .5
    return band_noise(len(t), 300, 2600) * wings * np.sin(np.pi * u) ** 1.2


def beetle_chitter():
    duration = .32
    out = np.zeros(int(SR * duration))
    for index in range(9):
        click_t = t_axis(.012)
        click = band_noise(len(click_t), 1800, 6500) * np.exp(-click_t / .004) + np.sin(2 * np.pi * 420 * click_t) * np.exp(-click_t / .006) * .4
        at = int((.01 + index * .034) * SR)
        out[at:at + len(click)] += click * (.7 + .3 * rng.random())
    return out


def beetle_shell():
    t = t_axis(.1)
    ring = sum(g * np.sin(2 * np.pi * f * t) for f, g in ((1900, 1), (2870, .7), (4100, .4))) * np.exp(-t / .022)
    return ring * .6 + band_noise(len(t), 200, 900) * np.exp(-t / .012) * .45


def beetle_die():
    duration = .32
    out = np.zeros(int(SR * duration))
    for _ in range(9):
        at = int(rng.uniform(0, .2) * SR)
        crack_t = t_axis(.03)
        crack = band_noise(len(crack_t), 900, 5500) * np.exp(-crack_t / .008)
        out[at:at + len(crack)] += crack * rng.uniform(.4, 1)
    t = t_axis(duration)
    return out * .75 + np.sin(2 * np.pi * 90 * t) * np.exp(-t / .06) * .6


def piranha_chomp():
    bite_t = t_axis(.05)
    bite = band_noise(len(bite_t), 700, 3500) * np.exp(-bite_t / .012) + sweep(bite_t, 320, 150) * np.exp(-bite_t / .018) * .7
    return np.concatenate([bite, np.zeros(int(SR * .025)), bite * .8])


def write(path, samples, peak, loop=False):
    samples = np.asarray(samples, dtype=float)
    samples = samples * (peak / max(1e-9, np.abs(samples).max()))
    if samples.ndim == 1 and not loop:
        samples = fade_edges(samples)
    channels = 1 if samples.ndim == 1 else samples.shape[1]
    path.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(path), "wb") as out:
        out.setnchannels(channels)
        out.setsampwidth(2)
        out.setframerate(SR)
        out.writeframes((np.clip(samples, -1, 1) * 32767).astype("<i2").tobytes())
    rms = float(np.sqrt(np.mean(samples ** 2)))
    print(f"{path.name:28} {samples.shape[0] / SR:6.2f}s pico {np.abs(samples).max():.3f} rms {rms:.3f}")


SOUNDS = {
    "SFX_Lake_Spit": (spit, .8), "SFX_Lake_SplashSmall": (splash_small, .7), "SFX_Lake_SplashBig": (splash_big, .9),
    "SFX_Lake_TongueLash": (tongue_lash, .85), "SFX_Lake_Gulp": (gulp, .75), "SFX_Lake_Burp": (burp, .9),
    "SFX_Lake_BossJump": (boss_jump, .75), "SFX_Lake_BossLand": (boss_land, .95), "SFX_Lake_TireThrow": (tire_throw, .8),
    "SFX_Lake_TireBounce": (tire_bounce, .85), "SFX_Lake_EnemyHit": (enemy_hit, .7), "SFX_Lake_EnemyDie": (enemy_die, .8),
    "SFX_Lake_Slap": (slap, .85), "SFX_Lake_BossHit": (boss_hit, .75),
}

ANIMALS = {
    "SFX_Lake_FrogCroak": (frog_croak, .85), "SFX_Lake_FrogDie": (frog_die, .85), "SFX_Lake_SapererecaCroak": (saperereca_croak, .85),
    "SFX_Lake_SapeneuGrunt": (sapeneu_grunt, .92), "SFX_Lake_FlyBuzz": (fly_buzz, .7), "SFX_Lake_DragonflyDash": (dragonfly_dash, .8),
    "SFX_Lake_BeetleChitter": (beetle_chitter, .75), "SFX_Lake_BeetleShell": (beetle_shell, .75), "SFX_Lake_BeetleDie": (beetle_die, .85),
    "SFX_Lake_PiranhaChomp": (piranha_chomp, .8),
}

if __name__ == "__main__":
    for name, (build, peak) in SOUNDS.items():
        write(OUT / f"{name}.wav", build(), peak)
    write(OUT / "AMB_Lake_Loop.wav", lake_ambience(), .6)
    write(SKY / "AMB_Sky_Wind_Loop.wav", sky_ambience(), .6)
    # Depois dos anteriores: o gerador aleatorio continua igual e os sons antigos nao mudam.
    for name, (build, peak) in ANIMALS.items():
        write(OUT / f"{name}.wav", build(), peak)
    write(OUT / "SFX_Lake_MosquitoBuzz.wav", mosquito_buzz(), .7, loop=True)
