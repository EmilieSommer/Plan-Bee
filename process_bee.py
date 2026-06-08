from PIL import Image
import sys
import os

input_path = "/Users/emiliesommerfreltoft/.gemini/antigravity/brain/d7a0190b-2ad8-4b89-affc-ab44545e32c7/bee_spritesheet_1780900721067.png"
output_path = "Assets/04_Sprites/Bees/PNGS/Bee_WalkSheet.png"

if not os.path.exists(input_path):
    print("Input not found!")
    sys.exit(1)

img = Image.open(input_path).convert("RGBA")
data = img.getdata()

new_data = []
for item in data:
    r, g, b, a = item
    if (r > 240 and g > 240 and b > 240) or (abs(r-204) < 10 and abs(g-204) < 10 and abs(b-204) < 10) or (abs(r-g)<5 and abs(g-b)<5 and r>180):
        new_data.append((255, 255, 255, 0))
    else:
        new_data.append(item)
        
img.putdata(new_data)

w, h = img.size
frame_w = w // 2
frame_h = h // 2

strip = Image.new("RGBA", (frame_w * 4, frame_h))

frames = [
    img.crop((0, 0, frame_w, frame_h)),
    img.crop((frame_w, 0, w, frame_h)),
    img.crop((0, frame_h, frame_w, h)),
    img.crop((frame_w, frame_h, w, h))
]

for i, frame in enumerate(frames):
    strip.paste(frame, (i * frame_w, 0))
    
strip.save(output_path)
print("Processed successfully!")
