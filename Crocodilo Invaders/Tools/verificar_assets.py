"""Verifica os recursos copiados para o projeto Unity sem precisar do editor."""

from pathlib import Path
import re


UNITY = Path(__file__).resolve().parents[1]
ART = UNITY / "Assets" / "Resources" / "Art"
AUDIO = UNITY / "Assets" / "Resources" / "Audio"
CODE = (UNITY / "Assets" / "Scripts" / "CrocodiloGame.cs").read_text(encoding="utf-8")

images = set(re.findall(r'DrawImage\("([^"]+)"', CODE))
animations = set(re.findall(r'DrawAnimation\("([^"]+)"', CODE))
sounds = set(re.findall(r'(?:PlaySound|PlayMusic)\("([^"]+)"', CODE))

missing = [str(ART / (name + ".png")) for name in images if not (ART / (name + ".png")).is_file()]
missing += [str(ART / "Anim" / name) for name in animations if not any((ART / "Anim" / name).glob("*.png"))]
missing += [str(AUDIO / (name + ".mp3")) for name in sounds if not (AUDIO / (name + ".mp3")).is_file()]

print(f"{len(images)} imagens, {len(animations)} animações e {len(sounds)} sons referenciados diretamente")
if missing:
    for path in missing:
        print("FALTANDO:", path)
    raise SystemExit(1)
print("Todas as referências diretas existem.")
