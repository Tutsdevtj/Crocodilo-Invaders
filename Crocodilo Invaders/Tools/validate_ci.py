"""Fast checkout checks; Unity performs compilation and gameplay-data checks at build time."""
from pathlib import Path
import re
import subprocess
import sys

project = Path(sys.argv[1]).resolve() if len(sys.argv) > 1 else Path(__file__).resolve().parents[1]
required = [
    "ProjectSettings/ProjectVersion.txt", "Packages/manifest.json",
    "Assets/Scenes/MainMenu.unity", "Assets/Scenes/Tests/PlayerMovementTest.unity",
    "Assets/Data/UpgradeCards.asset", "Assets/Resources/Audio/GameAudio.mixer",
    "Assets/Prefabs/UI/MainMenu.prefab", "Assets/Prefabs/UI/PauseMenu.prefab",
    "Assets/Prefabs/UI/AudioOptions.prefab", "Assets/Prefabs/UI/CardRewards.prefab",
    "Assets/Editor/CrocodiloWindowsBuild.cs",
]
for name in required:
    path = project / name
    assert path.is_file() and path.stat().st_size > 1, f"Missing or empty required file: {name}"
scenes = re.findall(r"^    path: (.+)$", (project / "ProjectSettings/EditorBuildSettings.asset").read_text(encoding="utf-8-sig"), re.M)
assert scenes[:2] == ["Assets/Scenes/MainMenu.unity", "Assets/Scenes/Tests/PlayerMovementTest.unity"], "Incorrect starting scenes"
guids = {}
for meta in (project / "Assets").rglob("*.meta"):
    match = re.search(r"^guid: ([a-f0-9]{32})$", meta.read_text(encoding="utf-8-sig"), re.M)
    assert match, f"Invalid asset metadata: {meta.relative_to(project)}"
    assert match[1] not in guids, f"Duplicate GUID: {meta.relative_to(project)} and {guids.get(match[1])}"
    guids[match[1]] = meta.relative_to(project)
for asset in (project / "Assets").rglob("*"):
    if asset.is_file() and asset.suffix != ".meta" and asset.name != ".DS_Store":
        assert Path(str(asset) + ".meta").exists(), f"Asset has no metadata: {asset.relative_to(project)}"
tracked = subprocess.check_output(["git", "ls-files", "-z"], cwd=project).decode().split("\0")
for name in filter(None, tracked):
    assert not re.search(r"(^|/)(Library|Temp|Obj|Logs|Builds|UserSettings)/", name), f"Generated files are tracked: {name}"
    path = project / name
    if path.is_file():
        assert path.stat().st_size < 100 * 1024 * 1024, f"File exceeds GitHub's size limit: {name}"
        assert path.suffix.lower() not in {".ulf", ".alf"}, f"Unity license file must not be committed: {name}"
print(f"CHECKOUT VALIDATION PASSED: {len(guids)} asset GUIDs; scenes, required files and tracked-file checks.")
