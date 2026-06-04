import os

b_names = {
    0: "Center", 1: "Wall_Top", 2: "Wall_Bottom", 4: "Wall_Left", 8: "Wall_Right",
    3: "Tunnel_Horizontal", 12: "Tunnel_Vertical",
    5: "Corner_TopLeft", 9: "Corner_TopRight", 6: "Corner_BottomLeft", 10: "Corner_BottomRight",
    7: "DeadEnd_Top", 11: "DeadEnd_Bottom", 13: "DeadEnd_Left", 14: "DeadEnd_Right",
    15: "Isolated"
}

def get_name(i):
    # Overlay: Bit=1 means WALL.
    wT = (i & 1) != 0
    wB = (i & 2) != 0
    wL = (i & 4) != 0
    wR = (i & 8) != 0
    count = wT + wB + wL + wR
    if count == 0: return "Center"
    if count == 4: return "Isolated"
    if count == 1:
        if wT: return "Wall_Top"
        if wB: return "Wall_Bottom"
        if wL: return "Wall_Left"
        if wR: return "Wall_Right"
    if count == 2:
        if wT and wB: return "Tunnel_Horizontal"
        if wL and wR: return "Tunnel_Vertical"
        if wT and wL: return "Corner_TopLeft"
        if wT and wR: return "Corner_TopRight"
        if wB and wL: return "Corner_BottomLeft"
        if wB and wR: return "Corner_BottomRight"
    if count == 3:
        if not wB: return "DeadEnd_Top"
        if not wT: return "DeadEnd_Bottom"
        if not wR: return "DeadEnd_Left"
        if not wL: return "DeadEnd_Right"

all_files = []
for root, dirs, files in os.walk("Assets/05_Tiles"):
    for f in files:
        if f.endswith(".png"):
            all_files.append(os.path.join(root, f))

for type_name, hint in [("InsideHive", "Inside"), ("Brood", "Brood")]:
    print(f"--- {type_name} ---")
    for i in range(16):
        overlayName = get_name(i)
        found = False
        for path in all_files:
            name = os.path.basename(path).replace(".png", "")
            if (name == overlayName or name.endswith("_" + overlayName)) and hint in path and "overlay" in path.lower():
                print(f"Mask {i} ({overlayName}) -> {path}")
                found = True
                break
        if not found:
            print(f"Mask {i} ({overlayName}) -> NOT FOUND")
