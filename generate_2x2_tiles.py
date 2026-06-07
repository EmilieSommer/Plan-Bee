import os
from PIL import Image

def slice_into_2x2(img_path, output_prefix):
    original_img = Image.open(img_path).convert('RGBA')
    
    # Scale 2x nearest neighbor so it becomes 64x64 (a 2x2 grid of 32x32 tiles)
    scaled_img = original_img.resize((64, 64), Image.NEAREST)
    
    # Slice into four 32x32 tiles
    # Top Left
    tl = scaled_img.crop((0, 32, 32, 64)) # Unity origin is bottom-left, but PIL origin is top-left.
    # Actually, PIL crop is (left, upper, right, lower)
    # TL in PIL is (0, 0, 32, 32)
    tl = scaled_img.crop((0, 0, 32, 32))
    tl.save(f"{output_prefix}_TL.png")
    
    # Top Right
    tr = scaled_img.crop((32, 0, 64, 32))
    tr.save(f"{output_prefix}_TR.png")
    
    # Bottom Left
    bl = scaled_img.crop((0, 32, 32, 64))
    bl.save(f"{output_prefix}_BL.png")
    
    # Bottom Right
    br = scaled_img.crop((32, 32, 64, 64))
    br.save(f"{output_prefix}_BR.png")
    
    print(f"Scaled and sliced {img_path} into 4 tiles.")

# 1. Slice the original tile
slice_into_2x2('Assets/05_Tiles/Grass/Summer/Summer_v1_f1.png', 'Assets/05_Tiles/Grass/Summer/Summer_v1_f1')

# 2. Re-generate the 5 variations at 32x32 (the beautiful pure pixel ones) using the script we still have!
import subprocess
subprocess.run(['python3', 'generate_5_variations.py'])

# 3. Scale and slice the 5 variations!
for i in range(1, 6):
    var_path = f'Assets/05_Tiles/Grass/Summer/Summer_v1_var{i}.png'
    output_prefix = f'Assets/05_Tiles/Grass/Summer/Summer_v1_var{i}'
    slice_into_2x2(var_path, output_prefix)

print("All 2x2 sliced variations generated successfully!")
