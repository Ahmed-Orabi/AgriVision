import shutil
import random
from pathlib import Path
import yaml

BASE_DIR     = Path("/teamspace/studios/this_studio/AI/PlantGrowthStage-CV-Model")
TOMATO_DIR   = BASE_DIR / "data/specific/tomato"
COTTON_DIR   = BASE_DIR / "data/specific/cotton"
OUT_DIR      = BASE_DIR / "data/combined"

CLASSES = [
    "Tomato_Early_Vegetative",      # 0
    "Tomato_Flowering_Initiation",  # 1
    "Tomato_Unripe",                # 2
    "Tomato_Ripe",                  # 3
    "Cotton_Cotton_Blossom",        # 4
    "Cotton_Cotton_Bud",            # 5
    "Cotton_Early_Boll",            # 6
    "Cotton_Matured_Cotton_Boll",   # 7
    "Cotton_Split_CottonBoll",      # 8
]

SPLITS = ["train", "valid", "test"]

def make_dirs():
    for split in SPLITS:
        (OUT_DIR / split / "images").mkdir(parents=True, exist_ok=True)
        (OUT_DIR / split / "labels").mkdir(parents=True, exist_ok=True)
    print("Output dirs created")

def copy_tomato():
    total = 0
    for split in SPLITS:
        img_dir = TOMATO_DIR / split / "images"
        lbl_dir = TOMATO_DIR / split / "labels"
        if not img_dir.exists():
            continue
        images = list(img_dir.glob("*"))
        for img in images:
            shutil.copy(img, OUT_DIR / split / "images" / img.name)
            lbl = lbl_dir / (img.stem + ".txt")
            if lbl.exists():
                shutil.copy(lbl, OUT_DIR / split / "labels" / lbl.name)
        total += len(images)
    print(f"Tomato: {total} images copied")

def copy_cotton():
    remap = {0: 4, 1: 5, 2: 6, 3: 7, 4: 8}
    total = 0
    for split in SPLITS:
        img_dir = COTTON_DIR / split / "images"
        lbl_dir = COTTON_DIR / split / "labels"
        if not img_dir.exists():
            continue
        images = list(img_dir.glob("*"))
        for img in images:
            dst_img = OUT_DIR / split / "images" / f"cotton_{img.name}"
            shutil.copy(img, dst_img)
            lbl = lbl_dir / (img.stem + ".txt")
            dst_lbl = OUT_DIR / split / "labels" / f"cotton_{img.stem}.txt"
            if lbl.exists():
                with open(lbl) as f:
                    lines = f.readlines()
                new_lines = []
                for line in lines:
                    parts = line.strip().split()
                    if parts:
                        parts[0] = str(remap.get(int(parts[0]), int(parts[0])))
                        new_lines.append(" ".join(parts))
                with open(dst_lbl, "w") as f:
                    f.write("\n".join(new_lines) + "\n")
        total += len(images)
    print(f"Cotton: {total} images copied & remapped")

def write_yaml():
    cfg = {
        "train": str(OUT_DIR / "train/images"),
        "val":   str(OUT_DIR / "valid/images"),
        "test":  str(OUT_DIR / "test/images"),
        "nc":    len(CLASSES),
        "names": CLASSES,
    }
    with open(OUT_DIR / "data.yaml", "w") as f:
        yaml.dump(cfg, f, allow_unicode=True, sort_keys=False)
    print(f"data.yaml written")

def stats():
    print("\nCombined Dataset Stats:")
    for split in SPLITS:
        n = len(list((OUT_DIR / split / "images").glob("*")))
        print(f"   {split}: {n} images")
    print(f"\n   Classes ({len(CLASSES)}):")
    for i, name in enumerate(CLASSES):
        print(f"   {i}: {name}")

if __name__ == "__main__":
    make_dirs()
    copy_tomato()
    copy_cotton()
    write_yaml()
    stats()
    print("\nCombined dataset ready!")