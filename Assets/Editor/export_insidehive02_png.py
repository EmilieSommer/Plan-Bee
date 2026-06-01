#!/usr/bin/env python3
"""
Export InsideHive02 SVGs → PNGs for Unity tilemap.
Run from anywhere:  python3 Assets/Editor/export_insidehive02_png.py
Output: Assets/05_Tiles/InsideHive/Inside_Hive_02/Variants/
"""

import xml.etree.ElementTree as ET
import re, os, sys
from PIL import Image

# Locate project root (two levels above this script's own dir if run from root,
# or just use the explicit path)
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
PROJECT_ROOT = os.path.dirname(os.path.dirname(SCRIPT_DIR))  # Editor/../.. = project root
BASE = os.path.join(PROJECT_ROOT, "Assets", "05_Tiles", "InsideHive", "Inside_Hive_02")
OUT  = os.path.join(BASE, "Variants")

# (svg_relative_to_BASE, output_png_name)
# Naming convention: letters = which borders are VISIBLE on that sprite
EXPORTS = [
    # 16 cardinal variants
    ("base_components/Inside_Hive.svg",                             "IH2_floor.png"),
    ("Borders/Inside_Hive_Border_Top.svg",                          "IH2_T.png"),
    ("Borders/Inside_Hive_Border_Bottom.svg",                       "IH2_B.png"),
    ("Borders/Inside_Hive_Border_Left.svg",                         "IH2_L.png"),
    ("Borders/Inside_Hive_Border_Right.svg",                        "IH2_R.png"),
    ("Borders/Inside_Hive_Border_T+B.svg",                          "IH2_TB.png"),
    ("Borders/Inside_Hive_Border_L+R.svg",                          "IH2_LR.png"),
    ("Borders/Inside_Border/Inside_Hive_Border_Top_Left.svg",       "IH2_TL.png"),
    ("Borders/Inside_Border/Inside_Hive_Border_Top_Right.svg",      "IH2_TR.png"),
    ("Borders/Inside_Border/Inside_Hive_Border_Bot_Left.svg",       "IH2_BL.png"),
    ("Borders/Inside_Border/Inside_Hive_Border_Bot_Right.svg",      "IH2_BR.png"),
    ("Borders/Inside_Hive_Border_T+B+L.svg",                        "IH2_TBL.png"),
    ("Borders/Inside_Hive_Border_T+B+R.svg",                        "IH2_TBR.png"),
    ("Borders/Inside_Hive_Border_T+L+R.svg",                        "IH2_TLR.png"),
    ("Borders/Inside_Hive_Border_B+L+R.svg",                        "IH2_BLR.png"),
    ("Borders/Inside_Hive_Border_T+B+L+R.svg",                      "IH2_TBLR.png"),
    # 8 diagonal end-cap variants
    ("Borders/Outside_Border/Inside_Hive_Border_Out_Top_Left.svg",  "IH2_EndTop_L.png"),
    ("Borders/Outside_Border/Inside_Hive_Border_Out_Top_Right.svg", "IH2_EndTop_R.png"),
    ("Borders/Outside_Border/Inside_Hive_Border_Out_Bot_Left.svg",  "IH2_EndBot_L.png"),
    ("Borders/Outside_Border/Inside_Hive_Border_Out_Bot_Right.svg", "IH2_EndBot_R.png"),
    ("Borders/Outside_Border/Inside_Hive_Border_Out_Left_Top.svg",  "IH2_EndLeft_T.png"),
    ("Borders/Outside_Border/Inside_Hive_Border_Out_Left_Bot.svg",  "IH2_EndLeft_B.png"),
    ("Borders/Outside_Border/Inside_Hive_Border_Out_Right_Top.svg", "IH2_EndRight_T.png"),
    ("Borders/Outside_Border/Inside_Hive_Border_Out_Right_Bot.svg", "IH2_EndRight_B.png"),
]

def hex_to_rgba(s):
    s = s.lstrip('#')
    if len(s) == 6:
        r, g, b = int(s[0:2],16), int(s[2:4],16), int(s[4:6],16)
        return (r, g, b, 255)
    return None

def convert(svg_path, png_path):
    tree = ET.parse(svg_path)
    root = tree.getroot()
    vb = root.get('viewBox','')
    if vb:
        parts = vb.split()
        W, H = int(float(parts[2])), int(float(parts[3]))
    else:
        W = int(float(root.get('width', 36)))
        H = int(float(root.get('height', 36)))

    img = Image.new('RGBA', (W, H), (0, 0, 0, 0))
    px  = img.load()

    for path_el in root.iter('{http://www.w3.org/2000/svg}path'):
        d    = path_el.get('d', '')
        fill = path_el.get('fill', '')
        if not fill.startswith('#'):
            continue
        color = hex_to_rgba(fill)
        if not color:
            continue
        nums = re.findall(r'[\d.]+', d)
        if len(nums) >= 2:
            x = int(float(nums[0]))
            y = int(float(nums[1]))
            if 0 <= x < W and 0 <= y < H:
                px[x, y] = color

    img.save(png_path)
    return W, H

os.makedirs(OUT, exist_ok=True)
print(f"Output: {OUT}\n")

ok = missing = 0
for svg_rel, png_name in EXPORTS:
    svg_path = os.path.join(BASE, svg_rel)
    png_path = os.path.join(OUT, png_name)
    if os.path.exists(svg_path):
        W, H = convert(svg_path, png_path)
        print(f"  ✓  {png_name:28s}  ({W}x{H})")
        ok += 1
    else:
        print(f"  ✗  MISSING SVG: {svg_rel}")
        missing += 1

print(f"\n{ok} exported, {missing} missing.")
if missing == 0:
    print("All done! Refresh Unity (Assets > Refresh or Ctrl+R) to import the PNGs.")
