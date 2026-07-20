# pyright: reportAttributeAccessIssue=false
import shutil
import random
from pathlib import Path
import yaml
from PIL import Image, ImageEnhance
import numpy as np
import os

BASE_DIR   = Path("/teamspace/studios/this_studio/AI/PlantGrowthStage-CV-Model")
TOMATO_DIR = BASE_DIR / "data/specific/tomato"
COTTON_DIR = BASE_DIR / "data/specific/cotton"
OUT_DIR    = BASE_DIR / "data/combined_v3"

CLASSES = [
    "Tomato_Early_Vegetative",    # 0
    "Tomato_Flowering_Initiation",# 1
    "Tomato_Unripe",              # 2
    "Tomato_Ripe",                # 3
    "Cotton_Early_Stage",         # 4  (Blossom + Bud)
    "Cotton_Mid_Stage",           # 5  (Early Boll)
    "Cotton_Late_Stage",          # 6  (Matured Boll)
]

SPLITS = ["train", "valid", "test"]

# Cotton remap: old_id → new_id
COTTON_REMAP = {0: 4, 1: 4, 2: 5, 3: 6}

def make_dirs():
    for split in SPLITS:
        (OUT_DIR / split / "images").mkdir(parents=True, exist_ok=True)
        (OUT_DIR / split / "labels").mkdir(parents=True, exist_ok=True)
    print("Dirs created")

def augment_image(img_path: Path, out_dir: Path, lbl_path: Path, lbl_out_dir: Path, n: int):
    try:
        img = Image.open(img_path).convert("RGB")
    except:
        return

    for i in range(n):
        aug_img = img.copy()

        # Random brightness
        factor = random.uniform(0.7, 1.3)
        aug_img = ImageEnhance.Brightness(aug_img).enhance(factor)

        # Random contrast
        factor = random.uniform(0.8, 1.2)
        aug_img = ImageEnhance.Contrast(aug_img).enhance(factor)

        # Random flip
        if random.random() > 0.5:
            aug_img = aug_img.transpose(Image.FLIP_LEFT_RIGHT)
            flipped = True
        else:
            flipped = False

        # Random rotation
        angle = random.uniform(-15, 15)
        aug_img = aug_img.rotate(angle, fillcolor=(114, 114, 114))

        # Save image
        aug_name = f"aug{i}_{img_path.name}"
        aug_img.save(out_dir / aug_name)

        if lbl_path.exists():
            shutil.copy(lbl_path, lbl_out_dir / f"aug{i}_{lbl_path.name}")

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

def copy_cotton_with_augmentation():
    total = 0
    aug_total = 0

    for split in SPLITS:
        img_dir = COTTON_DIR / split / "images"
        lbl_dir = COTTON_DIR / split / "labels"
        if not img_dir.exists():
            continue

        images = list(img_dir.glob("*.jpg")) + \
                 list(img_dir.glob("*.png")) + \
                 list(img_dir.glob("*.jpeg"))

        for img in images:
            lbl_path = lbl_dir / (img.stem + ".txt")
            if not lbl_path.exists():
                continue

            with open(lbl_path) as f:
                lines = f.readlines()

            new_lines = []
            skip = False
            for line in lines:
                parts = line.strip().split()
                if not parts:
                    continue
                old_id = int(parts[0])
                if old_id not in COTTON_REMAP:
                    skip = True  # class 4 (Split) → skip
                    break
                parts[0] = str(COTTON_REMAP[old_id])
                new_lines.append(" ".join(parts))

            if skip or not new_lines:
                continue

            dst_img = OUT_DIR / split / "images" / f"cotton_{img.name}"
            dst_lbl = OUT_DIR / split / "labels" / f"cotton_{img.stem}.txt"

            shutil.copy(img, dst_img)
            with open(dst_lbl, "w") as f:
                f.write("\n".join(new_lines) + "\n")

            total += 1

            if split == "train":
                augment_image(
                    img, OUT_DIR / "train" / "images",
                    dst_lbl, OUT_DIR / "train" / "labels",
                    n=4
                )
                aug_total += 4

    print(f"Cotton: {total} original + {aug_total} augmented")

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
    print("\nCombined V3 Stats:")
    for split in SPLITS:
        n = len(list((OUT_DIR / split / "images").glob("*")))
        print(f"   {split}: {n} images")
    print(f"\n   Classes ({len(CLASSES)}):")
    for i, name in enumerate(CLASSES):
        print(f"   {i}: {name}")

if __name__ == "__main__":
    random.seed(42)
    make_dirs()
    copy_tomato()
    copy_cotton_with_augmentation()
    write_yaml()
    stats()
    print("\nCombined V3 ready!")