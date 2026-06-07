import os
import colorsys
import uuid
from PIL import Image

def rgb_to_hsv(r, g, b):
    return colorsys.rgb_to_hsv(r/255.0, g/255.0, b/255.0)

def hsv_to_rgb(h, s, v):
    r, g, b = colorsys.hsv_to_rgb(h, s, v)
    return int(r*255), int(g*255), int(b*255)

def shift_to_autumn(img):
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
            
            # Map green (h ~ 0.3) to orange/red (h ~ 0.05 - 0.1)
            # We can just force the hue to a warm orange/red and slightly lower saturation
            # Let's make it an average orange: 0.08
            # Add slight variation based on original hue or brightness to keep shading
            h_new = 0.06 + (v * 0.04) # 0.06 to 0.1 (Red to Orange-Yellow)
            s_new = min(1.0, s * 0.8)
            v_new = min(1.0, v * 0.9)
            
            nr, ng, nb = hsv_to_rgb(h_new, s_new, v_new)
            new_pixels[x, y] = (nr, ng, nb, a)
            
    return new_img

leaf_dir = 'Assets/05_Tiles/Grass/Leaf/'
out_dir = 'Assets/05_Tiles/Grass/Autumn/Tufts/'
os.makedirs(out_dir, exist_ok=True)

leaves = ['Group 293.png', 'Group 294.png', 'Group 297.png', 'Group 298.png']

for leaf in leaves:
    leaf_path = os.path.join(leaf_dir, leaf)
    if os.path.exists(leaf_path):
        img = Image.open(leaf_path).convert('RGBA')
        
        # We will make 2 color variations for each leaf: one orange, one more red
        img_orange = shift_to_autumn(img)
        
        # Red variant
        pixels = img_orange.load()
        img_red = Image.new('RGBA', img_orange.size)
        red_pixels = img_red.load()
        for x in range(img_orange.width):
            for y in range(img_orange.height):
                r, g, b, a = pixels[x, y]
                if a != 0:
                    h, s, v = rgb_to_hsv(r, g, b)
                    h = 0.02 # Red
                    r, g, b = hsv_to_rgb(h, s, v)
                red_pixels[x, y] = (r, g, b, a)
                
        out_name1 = f"Autumn_{leaf.replace('.png', '')}_Orange.png"
        out_name2 = f"Autumn_{leaf.replace('.png', '')}_Red.png"
        
        img_orange.save(os.path.join(out_dir, out_name1))
        img_red.save(os.path.join(out_dir, out_name2))
        print(f"Generated autumn variations for {leaf}")

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

for f in os.listdir(out_dir):
    if f.endswith('.png'):
        ensure_meta_file(os.path.join(out_dir, f))
