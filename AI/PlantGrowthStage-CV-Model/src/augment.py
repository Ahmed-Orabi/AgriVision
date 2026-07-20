import os
import cv2
from pathlib import Path

# Path
TRAIN_DIR = Path("/teamspace/studios/this_studio/AI/PlantGrowthStage-CV-Model/data/combined_v3/train")
IMAGES_DIR = TRAIN_DIR / "images"
LABELS_DIR = TRAIN_DIR / "labels"

# Cotton codes
TARGET_CLASSES = ['4', '5', '6']

def augment_data():
    print("Start increasing... ")
    augmented_count = 0
    
    for label_file in LABELS_DIR.glob("*.txt"):
        with open(label_file, "r") as f:
            lines = f.readlines()
            
        has_cotton = any(line.split()[0] in TARGET_CLASSES for line in lines)
        
        if not has_cotton:
            continue
            
        img_name = label_file.stem + ".jpg"
        img_path = IMAGES_DIR / img_name
        
        if not img_path.exists():
            continue
            
        img = cv2.imread(str(img_path))
        h, w, _ = img.shape
        
        # -----------------------------------------
        # 1. (Horizontal Flip)
        # -----------------------------------------
        img_flip = cv2.flip(img, 1)
        flip_lines = []
        for line in lines:
            parts = line.strip().split()
            cls_id = parts[0]
            x_center, y_center, box_w, box_h = map(float, parts[1:5])
            
            new_x_center = 1.0 - x_center
            flip_lines.append(f"{cls_id} {new_x_center:.6f} {y_center:.6f} {box_w:.6f} {box_h:.6f}\n")
            
        cv2.imwrite(str(IMAGES_DIR / f"aug_flip_{img_name}"), img_flip)
        with open(LABELS_DIR / f"aug_flip_{label_file.name}", "w") as f:
            f.writelines(flip_lines)
            
        # -----------------------------------------
        # 2. (Darkened)
        # -----------------------------------------
        # 30%
        img_dark = cv2.convertScaleAbs(img, alpha=0.7, beta=0)
        
        cv2.imwrite(str(IMAGES_DIR / f"aug_dark_{img_name}"), img_dark)
        with open(LABELS_DIR / f"aug_dark_{label_file.name}", "w") as f:
            f.writelines(lines)
            
        augmented_count += 2
        
    print(f" Finished! Added {augmented_count} new cotton photos with their labels.")

if __name__ == "__main__":
    augment_data()