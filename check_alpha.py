from PIL import Image
import sys

def check_alpha(img_path):
    try:
        img = Image.open(img_path).convert("RGBA")
    except Exception as e:
        print(f"Error opening {img_path}: {e}")
        return
    
    width, height = img.size
    pixels = img.load()
    
    top_alpha = sum(pixels[x, y][3] for x in range(width) for y in range(height//2))
    bottom_alpha = sum(pixels[x, y][3] for x in range(width) for y in range(height//2, height))
    left_alpha = sum(pixels[x, y][3] for x in range(width//2) for y in range(height))
    right_alpha = sum(pixels[x, y][3] for x in range(width//2, width) for y in range(height))
    
    print(f"File: {img_path}")
    print(f"Top: {top_alpha}, Bottom: {bottom_alpha}")
    print(f"Left: {left_alpha}, Right: {right_alpha}")

check_alpha("Assets/05_Tiles/Brood/Brood_Overlay/inside.png")
