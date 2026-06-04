from PIL import Image
import sys

img = Image.open("Assets/05_Tiles/InsideHive/InsideHive_Overlay/inside_Wall_Bottom.png").convert("RGBA")
w, h = img.size

top_pixels = 0
bottom_pixels = 0

for x in range(w):
    for y in range(h):
        r, g, b, a = img.getpixel((x, y))
        if a > 0:
            if y < h // 2:
                top_pixels += 1 # PIL y=0 is top
            else:
                bottom_pixels += 1

print(f"Top half non-transparent pixels: {top_pixels}")
print(f"Bottom half non-transparent pixels: {bottom_pixels}")
