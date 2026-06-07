import os
from PIL import Image
from collections import Counter
import random

img_path = 'Assets/05_Tiles/Grass/Summer/Summer_v1_f1.png'
original_img = Image.open(img_path).convert('RGBA')

# Resize original to 64x64 to double the size of all art/tufts
original_img = original_img.resize((64, 64), Image.NEAREST)
width, height = 64, 64
pixels = original_img.load()

# Find base color
all_colors = []
for x in range(width):
    for y in range(height):
        all_colors.append(pixels[x, y])
base_color = Counter(all_colors).most_common(1)[0][0]

border_size = 2 # 1 pixel scaled 2x
visited = set()
stamps = []

# Extract the now 2x larger tufts
for x in range(border_size, width - border_size):
    for y in range(border_size, height - border_size):
        if (x, y) not in visited and pixels[x, y] != base_color:
            stamp_pixels = []
            q = [(x, y)]
            visited.add((x, y))
            while q:
                cx, cy = q.pop(0)
                stamp_pixels.append((cx, cy, pixels[cx, cy]))
                for dx, dy in [(-1,0), (1,0), (0,-1), (0,1), (-1,-1), (1,1), (-1,1), (1,-1)]:
                    nx, ny = cx + dx, cy + dy
                    if border_size <= nx < width - border_size and border_size <= ny < height - border_size:
                        if (nx, ny) not in visited and pixels[nx, ny] != base_color:
                            visited.add((nx, ny))
                            q.append((nx, ny))
            
            min_x = min(p[0] for p in stamp_pixels)
            min_y = min(p[1] for p in stamp_pixels)
            normalized_stamp = [((p[0] - min_x), (p[1] - min_y), p[2]) for p in stamp_pixels]
            sw = max(p[0] for p in normalized_stamp) + 1
            sh = max(p[1] for p in normalized_stamp) + 1
            stamps.append((normalized_stamp, sw, sh))

# Now we generate NEW 32x32 tiles, stamping the 2x larger tufts onto them!
target_size = 32
target_border = 1

stamp_counts = [1, 2, 3, 4, 5] # We place fewer stamps because they are 2x larger!

for i, num_stamps_to_place in enumerate(stamp_counts):
    var_idx = i + 1
    new_img = Image.new('RGBA', (target_size, target_size))
    new_pixels = new_img.load()
    
    # Fill with base color
    for x in range(target_size):
        for y in range(target_size):
            new_pixels[x, y] = base_color
            
    placed_stamps = []
    
    for _ in range(num_stamps_to_place * 5): 
        if not stamps or num_stamps_to_place == 0: break
        stamp_data, sw, sh = random.choice(stamps)
        
        # If the stamp is larger than the interior, skip it
        if sw > target_size - 2*target_border or sh > target_size - 2*target_border:
            continue
            
        flip_h = random.choice([True, False])
        flip_v = random.choice([True, False])
        
        modified_stamp = []
        for sx, sy, color in stamp_data:
            nsx = (sw - 1 - sx) if flip_h else sx
            nsy = (sh - 1 - sy) if flip_v else sy
            modified_stamp.append((nsx, nsy, color))
            
        px = random.randint(target_border, target_size - target_border - sw)
        py = random.randint(target_border, target_size - target_border - sh)
        
        overlap = False
        for px_prev, py_prev, sw_prev, sh_prev in placed_stamps:
            if px < px_prev + sw_prev and px + sw > px_prev and py < py_prev + sh_prev and py + sh > py_prev:
                overlap = True
                break
                
        if not overlap:
            placed_stamps.append((px, py, sw, sh))
            for sx, sy, color in modified_stamp:
                new_pixels[px + sx, py + sy] = color
            if len(placed_stamps) >= num_stamps_to_place:
                break

    out_path = f'Assets/05_Tiles/Grass/Summer/Summer_v1_var{var_idx}.png'
    new_img.save(out_path)
    print(f"Saved 2x upscaled art tile to {out_path}")
