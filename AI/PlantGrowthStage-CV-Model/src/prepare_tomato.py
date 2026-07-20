import os
import shutil
import random
from pathlib import Path

# Paths
BASE_DIR    = Path("/teamspace/studios/this_studio/AI/PlantGrowthStage-CV-Model")
KAGGLE_DIR  = BASE_DIR / "data/specific/tomato_kaggle/Tomato_Plant_Stages_Dataset"
ROBFLOW_DIR = BASE_DIR / "data/specific/tomato_roboflow"
COTTON_DIR  = BASE_DIR / "data/specific/cotton"
OUT_DIR     = BASE_DIR / "data/specific/tomato"

# Classes
# class_id → name
# 0: Early_Vegetative
# 1: Flowering_Initiation
# 2: Unripe
# 3: Ripe

SPLITS = ["train", "valid", "test"]

def make_dirs():
    for split in SPLITS:
        (OUT_DIR / split / "images").mkdir(parents=True, exist_ok=True)
        (OUT_DIR / split / "labels").mkdir(parents=True, exist_ok=True)
    print("Output dirs created")

def full_image_label(class_id: int, out_path: Path):
    """bounding box"""
    with open(out_path, "w") as f:
        f.write(f"{class_id} 0.5 0.5 1.0 1.0\n")

def convert_kaggle(split_ratio=(0.7, 0.15, 0.15)):
    """Convert Kaggle classification folders to YOLO format"""
    classes = {
        "Stage1_Early_Vegetative":   0,
        "Stage2_Flowering_Initiation": 1,
    }
    
    for folder, class_id in classes.items():
        images = list((KAGGLE_DIR / folder).glob("*.jpg")) + \
                 list((KAGGLE_DIR / folder).glob("*.png")) + \
                 list((KAGGLE_DIR / folder).glob("*.jpeg"))
        
        random.shuffle(images)
        n = len(images)
        n_train = int(n * split_ratio[0])
        n_valid = int(n * split_ratio[1])
        
        splits_data = {
            "train": images[:n_train],
            "valid": images[n_train:n_train + n_valid],
            "test":  images[n_train + n_valid:],
        }
        
        for split, imgs in splits_data.items():
            for img_path in imgs:
                # copy the photo
                dst_img = OUT_DIR / split / "images" / img_path.name
                shutil.copy(img_path, dst_img)
                # label
                dst_lbl = OUT_DIR / split / "labels" / (img_path.stem + ".txt")
                full_image_label(class_id, dst_lbl)
        
        print(f"Kaggle {folder}: {n} images converted")

def copy_roboflow():
    # Roboflow classes: 0=Ripe, 1=Unripe
    #make it like this : 2=Unripe, 3=Ripe
    remap = {"0": "3", "1": "2"}  # Ripe→3, Unripe→2
    
    for split in SPLITS:
        img_dir = ROBFLOW_DIR / split / "images"
        lbl_dir = ROBFLOW_DIR / split / "labels"
        
        if not img_dir.exists():
            continue
            
        images = list(img_dir.glob("*.jpg")) + \
                 list(img_dir.glob("*.png")) + \
                 list(img_dir.glob("*.jpeg"))
        
        for img_path in images:
            shutil.copy(img_path, OUT_DIR / split / "images" / img_path.name)
            
            lbl_path = lbl_dir / (img_path.stem + ".txt")
            dst_lbl  = OUT_DIR / split / "labels" / (img_path.stem + ".txt")
            
            if lbl_path.exists():
                with open(lbl_path) as f:
                    lines = f.readlines()
                new_lines = []
                for line in lines:
                    parts = line.strip().split()
                    if parts:
                        parts[0] = remap.get(parts[0], parts[0])
                        new_lines.append(" ".join(parts))
                with open(dst_lbl, "w") as f:
                    f.write("\n".join(new_lines) + "\n")
        
        print(f"Roboflow {split}: {len(images)} images copied")

def write_yaml():
    """ data.yaml """
    yaml_content = f"""train: {OUT_DIR}/train/images
val: {OUT_DIR}/valid/images
test: {OUT_DIR}/test/images

nc: 4
names: ['Early_Vegetative', 'Flowering_Initiation', 'Unripe', 'Ripe']
"""
    yaml_path = OUT_DIR / "data.yaml"
    with open(yaml_path, "w") as f:
        f.write(yaml_content)
    print(f"data.yaml written → {yaml_path}")

def stats():
    print("\nFinal Stats:")
    for split in SPLITS:
        n = len(list((OUT_DIR / split / "images").glob("*")))
        print(f"   {split}: {n} images")

if __name__ == "__main__":
    random.seed(42)
    make_dirs()
    convert_kaggle()
    copy_roboflow()
    write_yaml()
    stats()
    print("\nTomato dataset ready!")