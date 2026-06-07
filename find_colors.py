import os
from PIL import Image
from collections import Counter

img_path = 'Assets/05_Tiles/Grass/Summer/Summer_v1_f1.png'
original_img = Image.open(img_path).convert('RGBA')
width, height = original_img.size
pixels = original_img.load()

colors = []
for x in range(width):
    for y in range(height):
        if pixels[x,y][3] > 0:
            colors.append(pixels[x,y])

counter = Counter(colors)
most_common = counter.most_common(20)

print("Top 20 most common colors:")
for c, count in most_common:
    print(f"Color: {c}, Count: {count}")

