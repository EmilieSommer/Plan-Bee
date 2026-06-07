import os
import random
from PIL import Image, ImageDraw

out_dir = 'Assets/05_Tiles/Grass/Summer/Tufts/'
os.makedirs(out_dir, exist_ok=True)

def generate_flower_frames(name_prefix, flower_type):
    # We create a 16x16 canvas for the overlay
    w, h = 16, 16
    
    # We will generate 2 frames for a subtle wind sway
    for frame in range(1, 3):
        img = Image.new('RGBA', (w, h), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        
        # Wind offset: frame 1 is normal, frame 2 is shifted right by 1 pixel for the top half
        shift_x = 1 if frame == 2 else 0
        
        cx, cy = 8, 8
        
        white = (255, 255, 255, 255)
        off_white = (220, 230, 240, 255)
        yellow = (255, 200, 0, 255)
        dark_yellow = (220, 160, 0, 255)
        green_stem = (100, 150, 20, 255)
        
        if flower_type == 1: # Small Daisy
            # Stem
            draw.point((cx, cy+2), fill=green_stem)
            draw.point((cx+shift_x, cy+1), fill=green_stem)
            # Petals
            draw.point((cx+shift_x, cy-1), fill=white) # Top
            draw.point((cx+shift_x, cy+1), fill=white) # Bottom
            draw.point((cx-1+shift_x, cy), fill=white) # Left
            draw.point((cx+1+shift_x, cy), fill=white) # Right
            # Shading
            draw.point((cx+shift_x, cy+1), fill=off_white)
            # Center
            draw.point((cx+shift_x, cy), fill=yellow)
            
        elif flower_type == 2: # Medium Daisy
            # Stem
            draw.point((cx, cy+3), fill=green_stem)
            draw.point((cx-1, cy+2), fill=green_stem)
            draw.point((cx+shift_x, cy+1), fill=green_stem)
            # Petals (diagonal and straight)
            for dx, dy in [(-1,-1), (1,-1), (-1,1), (1,1), (0,-1), (0,1), (-1,0), (1,0)]:
                draw.point((cx+dx+shift_x, cy+dy), fill=white)
            draw.point((cx-1+shift_x, cy+1), fill=off_white)
            draw.point((cx+1+shift_x, cy+1), fill=off_white)
            draw.point((cx+shift_x, cy+1), fill=off_white)
            # Center
            draw.point((cx+shift_x, cy), fill=yellow)
            
        elif flower_type == 3: # Cluster of 2
            # Stem
            draw.point((cx, cy+2), fill=green_stem)
            draw.point((cx-2, cy+2), fill=green_stem)
            draw.point((cx-1, cy+1), fill=green_stem)
            
            # Flower 1
            f1x, f1y = cx+1+shift_x, cy-1
            draw.point((f1x, f1y-1), fill=white)
            draw.point((f1x, f1y+1), fill=white)
            draw.point((f1x-1, f1y), fill=white)
            draw.point((f1x+1, f1y), fill=white)
            draw.point((f1x, f1y), fill=yellow)
            
            # Flower 2
            f2x, f2y = cx-2+shift_x, cy+1
            draw.point((f2x, f2y-1), fill=white)
            draw.point((f2x, f2y+1), fill=white)
            draw.point((f2x-1, f2y), fill=white)
            draw.point((f2x+1, f2y), fill=white)
            draw.point((f2x, f2y), fill=dark_yellow)
            
        elif flower_type == 4: # Cluster of 3 tiny
            # Stems
            draw.point((cx, cy+2), fill=green_stem)
            draw.point((cx-1+shift_x, cy+1), fill=green_stem)
            draw.point((cx+1+shift_x, cy+1), fill=green_stem)
            # F1
            draw.point((cx+shift_x, cy-2), fill=white)
            draw.point((cx+shift_x, cy-1), fill=yellow)
            # F2
            draw.point((cx-2+shift_x, cy), fill=white)
            draw.point((cx-1+shift_x, cy), fill=yellow)
            # F3
            draw.point((cx+2+shift_x, cy+1), fill=white)
            draw.point((cx+1+shift_x, cy+1), fill=yellow)
            
        elif flower_type == 5: # Star flower
            draw.point((cx, cy+2), fill=green_stem)
            draw.point((cx+shift_x, cy+1), fill=green_stem)
            # Star petals
            draw.point((cx+shift_x, cy-2), fill=white)
            draw.point((cx+shift_x, cy+2), fill=white)
            draw.point((cx-2+shift_x, cy), fill=white)
            draw.point((cx+2+shift_x, cy), fill=white)
            # Inner petals
            draw.point((cx-1+shift_x, cy-1), fill=off_white)
            draw.point((cx+1+shift_x, cy-1), fill=off_white)
            draw.point((cx-1+shift_x, cy+1), fill=off_white)
            draw.point((cx+1+shift_x, cy+1), fill=off_white)
            # Center
            draw.point((cx+shift_x, cy), fill=yellow)
            
        img.save(f"{out_dir}{name_prefix}_f{frame}.png")

for i in range(1, 6):
    generate_flower_frames(f"Summer_flower_{i}", i)
    print(f"Generated frames for Summer_flower_{i}")
