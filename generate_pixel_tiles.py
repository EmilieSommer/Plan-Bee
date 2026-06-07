import os
from PIL import Image
import random

# Load original image
img_path = 'Assets/05_Tiles/Grass/Summer/Summer_v1_f1.png'
if not os.path.exists(img_path):
    print("Error: Could not find original tile at", img_path)
    exit(1)

original_img = Image.open(img_path).convert('RGBA')
width, height = original_img.size

if width != 32 or height != 32:
    print(f"Warning: Original image is {width}x{height}, resizing to 32x32 just in case.")
    original_img = original_img.resize((32, 32), Image.NEAREST)
    width, height = 32, 32

pixels = original_img.load()

# Create 3 variations
for var_idx in range(1, 4):
    new_img = Image.new('RGBA', (width, height))
    new_pixels = new_img.load()
    
    # 1. Copy borders perfectly (2 pixels thick on all sides) to guarantee seamless tiling
    border_size = 3
    for x in range(width):
        for y in range(height):
            if x < border_size or x >= width - border_size or y < border_size or y >= height - border_size:
                new_pixels[x, y] = pixels[x, y]
            else:
                # 2. For the interior, randomly sample a block from the original interior
                # We do this block by block to maintain local structures (like grass clumps)
                pass

    # Fill interior with 4x4 blocks from the original interior
    block_size = 4
    for x in range(border_size, width - border_size, block_size):
        for y in range(border_size, height - border_size, block_size):
            # Pick a random source block from the interior
            src_x = random.randint(border_size, width - border_size - block_size)
            src_y = random.randint(border_size, height - border_size - block_size)
            
            for bx in range(block_size):
                for by in range(block_size):
                    # Ensure we don't go out of bounds
                    if x + bx < width - border_size and y + by < height - border_size:
                        new_pixels[x + bx, y + by] = pixels[src_x + bx, src_y + by]

    # Save the variation
    out_path = f'Assets/05_Tiles/Grass/Summer/Summer_v1_f1_var{var_idx}.png'
    new_img.save(out_path)
    print(f"Saved {out_path}")
