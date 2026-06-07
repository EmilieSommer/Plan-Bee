import os
from PIL import Image
from collections import Counter
import random

img_path = 'Assets/05_Tiles/Grass/Summer/Summer_v1_f1.png'
if not os.path.exists(img_path):
    print("Error: Could not find original tile.")
    exit(1)

original_img = Image.open(img_path).convert('RGBA')
width, height = original_img.size
if width != 32 or height != 32:
    original_img = original_img.resize((32, 32), Image.NEAREST)
    width, height = 32, 32

pixels = original_img.load()

# Find the base color (most common color in the image)
all_colors = []
for x in range(width):
    for y in range(height):
        all_colors.append(pixels[x, y])

base_color = Counter(all_colors).most_common(1)[0][0]

# We only need 1 pixel border to maintain perfect seamless tiling!
# This gives us a massive 30x30 interior to create unique patterns!
border_size = 1
visited = set()
stamps = []

# Extract stamps
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
            
            # If the stamp is too huge (e.g. takes up half the screen), it will look too similar.
            # We can break it into smaller pieces, but for now we just use it.
            min_x = min(p[0] for p in stamp_pixels)
            min_y = min(p[1] for p in stamp_pixels)
            normalized_stamp = [((p[0] - min_x), (p[1] - min_y), p[2]) for p in stamp_pixels]
            stamp_width = max(p[0] for p in normalized_stamp) + 1
            stamp_height = max(p[1] for p in normalized_stamp) + 1
            stamps.append((normalized_stamp, stamp_width, stamp_height))

# Generate 3 variations
for var_idx in range(1, 4):
    new_img = Image.new('RGBA', (width, height))
    new_pixels = new_img.load()
    
    # Fill with base color
    for x in range(width):
        for y in range(height):
            new_pixels[x, y] = base_color
            
    # Copy 1-pixel borders perfectly
    for x in range(width):
        for y in range(height):
            if x < border_size or x >= width - border_size or y < border_size or y >= height - border_size:
                new_pixels[x, y] = pixels[x, y]

    # Place random stamps in the interior
    # Use more stamps since the border is smaller now
    num_stamps_to_place = random.randint(6, 12)
    placed_stamps = []
    
    for _ in range(num_stamps_to_place * 5): # attempts
        if not stamps: break
        stamp_data, sw, sh = random.choice(stamps)
        
        # Randomly flip the stamp horizontally or vertically to create new shapes!
        flip_h = random.choice([True, False])
        flip_v = random.choice([True, False])
        
        modified_stamp = []
        for sx, sy, color in stamp_data:
            nsx = (sw - 1 - sx) if flip_h else sx
            nsy = (sh - 1 - sy) if flip_v else sy
            modified_stamp.append((nsx, nsy, color))
        
        # Pick a random position
        px = random.randint(border_size, width - border_size - sw)
        py = random.randint(border_size, height - border_size - sh)
        
        # Check overlap
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

    out_path = f'Assets/05_Tiles/Grass/Summer/Summer_v1_f1_var{var_idx}.png'
    new_img.save(out_path)
    print(f"Saved distinct variation to {out_path}")
