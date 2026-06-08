import os
from PIL import Image

def make_spritesheet(input_path, output_path, frame_size=32):
    img = Image.open(input_path).convert("RGBA")
    w, h = img.size
    frames = []
    
    f1 = Image.new("RGBA", (frame_size, frame_size), (0,0,0,0))
    f1.paste(img, ((frame_size - w) // 2, (frame_size - h) // 2))
    frames.append(f1)
    
    w2 = int(w * 0.85)
    img2 = img.resize((w2, h), Image.NEAREST)
    f2 = Image.new("RGBA", (frame_size, frame_size), (0,0,0,0))
    f2.paste(img2, ((frame_size - w2) // 2, (frame_size - h) // 2))
    frames.append(f2)
    
    w3 = int(w * 0.70)
    img3 = img.resize((w3, h), Image.NEAREST)
    f3 = Image.new("RGBA", (frame_size, frame_size), (0,0,0,0))
    f3.paste(img3, ((frame_size - w3) // 2, (frame_size - h) // 2))
    frames.append(f3)
    
    frames.append(f2)
    
    sheet = Image.new("RGBA", (frame_size * 4, frame_size), (0,0,0,0))
    for i, frame in enumerate(frames):
        sheet.paste(frame, (i * frame_size, 0))
        
    sheet.save(output_path)
    print("Saved", output_path)

files = ["Assets/04_Sprites/Bees/RobbeBee.png", "Assets/04_Sprites/Bees/Wasp.png"]
for f in files:
    if os.path.exists(f):
        out_name = f.replace(".png", "_WalkSheet.png")
        img = Image.open(f)
        size = 32
        if img.size[0] > 32 or img.size[1] > 32:
            size = max(img.size[0], img.size[1])
            if size % 2 != 0: size += 1
            
        make_spritesheet(f, out_name, frame_size=size)
