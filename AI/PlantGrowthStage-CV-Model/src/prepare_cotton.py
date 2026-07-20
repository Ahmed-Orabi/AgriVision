import os
import shutil
from pathlib import Path
import yaml

# Paths
BASE_DIR   = Path("/teamspace/studios/this_studio/AI/PlantGrowthStage-CV-Model")
COTTON_DIR = BASE_DIR / "data/specific/cotton"

def fix_yaml():
    yaml_path = COTTON_DIR / "data.yaml"
    with open(yaml_path) as f:
        cfg = yaml.safe_load(f)

    cfg["train"] = str(COTTON_DIR / "train/images")
    cfg["val"]   = str(COTTON_DIR / "valid/images")
    cfg["test"]  = str(COTTON_DIR / "test/images")

    with open(yaml_path, "w") as f:
        yaml.dump(cfg, f, allow_unicode=True)

    print("Cotton data.yaml fixed")
    print(f"   nc: {cfg['nc']}")
    print(f"   names: {cfg['names']}")

def stats():
    print("\nCotton Stats:")
    for split in ["train", "valid", "test"]:
        n = len(list((COTTON_DIR / split / "images").glob("*")))
        print(f"   {split}: {n} images")

if __name__ == "__main__":
    fix_yaml()
    stats()
    print("\nCotton dataset ready!")