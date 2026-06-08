from PIL import Image

input_path = "Assets/04_Sprites/Bees/PNGS/Bee_WalkSheet.png"
img = Image.open(input_path)

img = img.resize((128, 32), resample=Image.NEAREST)
img.save(input_path)
print("Resized successfully!")
