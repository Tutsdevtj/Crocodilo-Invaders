"""Copia os assets do Portugol e transforma GIFs em quadros PNG para a Unity.

Uso: python unity/Tools/importar_assets.py
Requer Pillow: pip install Pillow
"""

from pathlib import Path
from shutil import copy2
from PIL import Image


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "Gaming portugol"
DEST = ROOT / "unity" / "Assets" / "Resources"


def main():
    art = DEST / "Art"
    audio = DEST / "Audio"
    art.mkdir(parents=True, exist_ok=True)
    audio.mkdir(parents=True, exist_ok=True)

    for path in sorted(SOURCE.iterdir()):
        suffix = path.suffix.lower()
        if suffix == ".png":
            copy2(path, art / path.name)
        elif suffix == ".mp3":
            copy2(path, audio / path.name)
        elif suffix == ".gif":
            folder = art / "Anim" / path.stem
            folder.mkdir(parents=True, exist_ok=True)
            with Image.open(path) as gif:
                for index in range(gif.n_frames):
                    gif.seek(index)
                    gif.convert("RGBA").save(folder / f"{index:03}.png")
    print("Assets importados em", DEST)


if __name__ == "__main__":
    main()
