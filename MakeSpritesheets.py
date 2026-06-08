import os
from PIL import Image

def make_spritesheet(input_path, output_path, frame_size=32):
    img = Image.open(input_path).convert("RGBA")
    
    # Calculate how much to pad to make it fit in frame_size x frame_size
    w, h = img.size
    
    # Create the 4 frames
    frames = []
    
    # Frame 1: 100% width
    f1 = Image.new("RGBA", (frame_size, frame_size), (0,0,0,0))
    f1.paste(img, ((frame_size - w) // 2, (frame_size - h) // 2))
    frames.append(f1)
    
    # Frame 2: 85% width
    w2 = int(w * 0.85)
    img2 = img.resize((w2, h), Image.NEAREST)
    f2 = Image.new("RGBA", (frame_size, frame_size), (0,0,0,0))
    f2.paste(img2, ((frame_size - w2) // 2, (frame_size - h) // 2))
    frames.append(f2)
    
    # Frame 3: 70% width
    w3 = int(w * 0.70)
    img3 = img.resize((w3, h), Image.NEAREST)
    f3 = Image.new("RGBA", (frame_size, frame_size), (0,0,0,0))
    f3.paste(img3, ((frame_size - w3) // 2, (frame_size - h) // 2))
    frames.append(f3)
    
    # Frame 4: 85% width (same as frame 2)
    frames.append(f2)
    
    # Combine into spritesheet
    sheet = Image.new("RGBA", (frame_size * 4, frame_size), (0,0,0,0))
    for i, frame in enumerate(frames):
        sheet.paste(frame, (i * frame_size, 0))
        
    sheet.save(output_path)
    print("Saved", output_path)

input_dir = "Assets/04_Sprites/Bees/PNGS"
for f in os.listdir(input_dir):
    if f.endswith(".png") and not f.endswith("Sheet.png"):
        out_name = f.replace(".png", "_WalkSheet.png")
        # For the queen, let's use a bigger frame size since she is 35x35
        img = Image.open(os.path.join(input_dir, f))
        size = 32
        if img.size[0] > 32 or img.size[1] > 32:
            size = max(img.size[0], img.size[1])
            # round up to multiple of 2
            if size % 2 != 0: size += 1
            
        make_spritesheet(os.path.join(input_dir, f), os.path.join(input_dir, out_name), frame_size=size)
