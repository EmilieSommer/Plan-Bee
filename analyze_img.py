from PIL import Image

img = Image.open("/Users/emiliesommerfreltoft/.gemini/antigravity/brain/d7a0190b-2ad8-4b89-affc-ab44545e32c7/media__1780506650552.png")
w, h = img.size
print(f"Image size: {w}x{h}")

# convert to RGB
img = img.convert("RGB")

# Sample colors horizontally across the middle to find grid size
colors = []
for x in range(w):
    colors.append(img.getpixel((x, h//2)))

# find transitions to calculate pixel size of a tile
transitions = []
last_c = colors[0]
for x in range(1, w):
    c = colors[x]
    if c != last_c:
        # if diff > threshold
        if sum(abs(c[i]-last_c[i]) for i in range(3)) > 10:
            transitions.append(x)
        last_c = c

print(f"Transitions at middle: {transitions}")
if len(transitions) >= 2:
    print(f"Approx tile width: {transitions[1] - transitions[0]}")
