import os
import colorsys
import uuid
from PIL import Image, ImageDraw

def rgb_to_hsv(r, g, b):
    return colorsys.rgb_to_hsv(r/255.0, g/255.0, b/255.0)

def hsv_to_rgb(h, s, v):
    r, g, b = colorsys.hsv_to_rgb(h, s, v)
    return int(r*255), int(g*255), int(b*255)

def shift_image(img, season):
    pixels = img.load()
    width, height = img.size
    new_img = Image.new('RGBA', (width, height))
    new_pixels = new_img.load()
    
    for x in range(width):
        for y in range(height):
            r, g, b, a = pixels[x, y]
            if a == 0:
                continue
            h, s, v = rgb_to_hsv(r, g, b)
            
            if season == 'Autumn':
                # Shift green towards orange/brown (Hue ~ 0.08 to 0.12)
                h = 0.10 + (h - 0.25) * 0.2
                s *= 0.8
                v *= 0.85
            elif season == 'Winter':
                # Shift to light blue/grey, drop saturation heavily, boost lightness
                h = 0.55
                s *= 0.15
                v = min(1.0, v * 1.5 + 0.3)
            elif season == 'Spring':
                # Vibrant bright green
                h = 0.28 + (h - 0.25) * 0.5
                s = min(1.0, s * 1.3)
                v = min(1.0, v * 1.1)
                
            h = h % 1.0
            s = max(0.0, min(1.0, s))
            v = max(0.0, min(1.0, v))
            nr, ng, nb = hsv_to_rgb(h, s, v)
            new_pixels[x, y] = (nr, ng, nb, a)
            
    return new_img

base_dir = 'Assets/05_Tiles/Grass/'
seasons = ['Autumn', 'Winter', 'Spring']
for s in seasons:
    os.makedirs(f"{base_dir}{s}/Tufts", exist_ok=True)

# 1. Process Base Variations
summer_files = ['Summer_v1_f1.png'] + [f'Summer_v1_var{i}.png' for i in range(1, 6)]
for sf in summer_files:
    sf_path = f"{base_dir}Summer/{sf}"
    if not os.path.exists(sf_path):
        continue
    img = Image.open(sf_path).convert('RGBA')
    
    for s in seasons:
        new_img = shift_image(img, s)
        new_name = sf.replace('Summer', s)
        new_img.save(f"{base_dir}{s}/{new_name}")

print("Base tiles shifted.")

# 2. Process Tufts (For Autumn and Spring. Winter gets snow tufts)
for i in range(1, 8):
    for f in [1, 2]:
        tf_name = f'Summer_tuft_{i}_f{f}.png'
        tf_path = f"{base_dir}Summer/Tufts/{tf_name}"
        if not os.path.exists(tf_path):
            continue
        img = Image.open(tf_path).convert('RGBA')
        
        for s in ['Autumn', 'Spring']:
            new_img = shift_image(img, s)
            new_name = tf_name.replace('Summer', s)
            new_img.save(f"{base_dir}{s}/Tufts/{new_name}")

print("Tufts shifted.")

# 3. Generate Autumn Leaves
def generate_autumn_leaf(name, c1, c2):
    img = Image.new('RGBA', (8, 8), (0,0,0,0))
    draw = ImageDraw.Draw(img)
    # Simple maple leaf shape
    draw.point((4, 2), fill=c1)
    draw.point((3, 3), fill=c2)
    draw.point((4, 3), fill=c1)
    draw.point((5, 3), fill=c2)
    draw.point((2, 4), fill=c2)
    draw.point((3, 4), fill=c1)
    draw.point((4, 4), fill=c1)
    draw.point((5, 4), fill=c1)
    draw.point((6, 4), fill=c2)
    draw.point((3, 5), fill=c2)
    draw.point((4, 5), fill=c1)
    draw.point((5, 5), fill=c2)
    # Stem
    draw.point((4, 6), fill=(100, 50, 20, 255))
    img.save(f"{base_dir}Autumn/Tufts/{name}.png")

generate_autumn_leaf('Autumn_leaf_1', (200, 50, 0, 255), (150, 20, 0, 255)) # Red
generate_autumn_leaf('Autumn_leaf_2', (255, 120, 0, 255), (200, 80, 0, 255)) # Orange
generate_autumn_leaf('Autumn_leaf_3', (255, 200, 0, 255), (220, 150, 0, 255)) # Yellow

# 4. Generate Winter Snow Clumps (Animated)
def generate_snow_tuft(idx):
    for f in [1, 2]:
        img = Image.new('RGBA', (16, 16), (0,0,0,0))
        draw = ImageDraw.Draw(img)
        cx, cy = 8, 8
        shift = 1 if f == 2 else 0
        white = (255, 255, 255, 255)
        grey = (200, 200, 220, 255)
        dark = (150, 150, 180, 255)
        
        if idx == 1:
            points = [(0,1), (1,0), (1,1), (-1,1), (0,0)]
        else:
            points = [(0,1), (1,1), (2,1), (-1,1), (-2,1), (0,0), (1,0), (-1,0)]
            
        for dx, dy in points:
            draw.point((cx+dx+shift, cy+dy), fill=white)
        draw.point((cx+shift, cy+2), fill=grey)
        
        img.save(f"{base_dir}Winter/Tufts/Winter_tuft_{idx}_f{f}.png")

generate_snow_tuft(1)
generate_snow_tuft(2)

print("Leaves and Snow generated.")

# 5. Generate meta files and asset files
def ensure_meta_file(png_path):
    meta_path = png_path + ".meta"
    if not os.path.exists(meta_path):
        guid = uuid.uuid4().hex
        meta_content = f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 12
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipLevelLoadData: 0
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMasterTextureLimit: 0
  useMipmapLimitGroupName: 0
  mipmapLimitGroupName: 
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 32
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 3
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: {uuid.uuid4().hex[:32]}
    internalID: 0
    vertices: []
    indices: []
    edges: []
    weights: []
    tessellationDetail: 0
    hasOutline: 0
  spritePackingTag: 
  pSDRemoveMatte: 0
  pSDShowRemoveMatteOption: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
        with open(meta_path, "w") as f:
            f.write(meta_content)

def create_animated_tile_asset(name, f1_guid, f2_guid, out_dir):
    yaml_content = f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: 13b75c95f34a00d4e8c04f76b73312e6, type: 3}}
  m_Name: {name}
  m_EditorClassIdentifier:
  m_AnimatedSprites:
  - {{fileID: 21300000, guid: {f1_guid}, type: 3}}
  - {{fileID: 21300000, guid: {f2_guid}, type: 3}}
  m_MinSpeed: 1.5
  m_MaxSpeed: 2.5
  m_AnimationStartTime: 0
  m_TileColliderType: 0
"""
    with open(f"{out_dir}{name}.asset", "w") as f:
        f.write(yaml_content)

import re
def get_guid(filepath):
    if not os.path.exists(filepath):
        return None
    with open(filepath, 'r') as f:
        content = f.read()
        match = re.search(r'guid: ([a-f0-9]+)', content)
        if match:
            return match.group(1)
    return None

for s in seasons:
    s_dir = f"{base_dir}{s}/"
    for root, dirs, files in os.walk(s_dir):
        for file in files:
            if file.endswith('.png'):
                ensure_meta_file(os.path.join(root, file))
                
# Generate asset files for tufts in Autumn, Spring, Winter
for s in seasons:
    tufts_d = f"{base_dir}{s}/Tufts/"
    for i in range(1, 8):
        f1_path = f"{tufts_d}{s}_tuft_{i}_f1.png.meta"
        if os.path.exists(f1_path):
            g1 = get_guid(f1_path)
            g2 = get_guid(f"{tufts_d}{s}_tuft_{i}_f2.png.meta")
            if g1 and g2:
                create_animated_tile_asset(f"{s}_tuft_{i}", g1, g2, tufts_d)

print("All meta and asset files generated.")
