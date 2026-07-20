from pathlib import Path
import shutil

COTTON_DIR = Path("/teamspace/studios/this_studio/AI/PlantGrowthStage-CV-Model/data/specific/cotton")
SPLITS = ["train", "valid", "test"]

def seg_to_bbox(points: list[float]) -> tuple[float, float, float, float]:
    """convert polygon points to bounding box"""
    xs = points[0::2]
    ys = points[1::2]
    x_min, x_max = min(xs), max(xs)
    y_min, y_max = min(ys), max(ys)
    x_center = (x_min + x_max) / 2
    y_center = (y_min + y_max) / 2
    width    = x_max - x_min
    height   = y_max - y_min
    return x_center, y_center, width, height

def fix_labels(split: str):
    lbl_dir = COTTON_DIR / split / "labels"
    fixed = 0
    skipped = 0

    for lbl_file in lbl_dir.glob("*.txt"):
        with open(lbl_file) as f:
            lines = f.readlines()

        new_lines = []
        changed = False

        for line in lines:
            parts = line.strip().split()
            if not parts:
                continue

            class_id = parts[0]
            values   = list(map(float, parts[1:]))

            if len(values) == 4:
                # box - leave it
                new_lines.append(line.strip())

            elif len(values) >= 6 and len(values) % 2 == 0:
                # segmentation polygon - convert to box
                x_c, y_c, w, h = seg_to_bbox(values)
                new_lines.append(f"{class_id} {x_c:.6f} {y_c:.6f} {w:.6f} {h:.6f}")
                changed = True

            else:
                skipped += 1
                continue

        with open(lbl_file, "w") as f:
            f.write("\n".join(new_lines) + "\n")

        if changed:
            fixed += 1

    print(f"  {split}: {fixed} files converted, {skipped} lines skipped")

def main():
    print("Converting segmentation - bounding boxes...")
    for split in SPLITS:
        fix_labels(split)
    print("Done!")

if __name__ == "__main__":
    main()