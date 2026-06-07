import os
import re

tufts_dir = 'Assets/05_Tiles/Grass/Summer/Tufts/'

def get_guid(filepath):
    if not os.path.exists(filepath):
        return None
    with open(filepath, 'r') as f:
        content = f.read()
        match = re.search(r'guid: ([a-f0-9]+)', content)
        if match:
            return match.group(1)
    return None

def create_animated_tile_asset(name, f1_guid, f2_guid):
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
    with open(f"{tufts_dir}{name}.asset", "w") as f:
        f.write(yaml_content)

for i in range(3, 8):
    f1_meta = f"{tufts_dir}Summer_tuft_{i}_f1.png.meta"
    f2_meta = f"{tufts_dir}Summer_tuft_{i}_f2.png.meta"
    
    guid1 = get_guid(f1_meta)
    guid2 = get_guid(f2_meta)
    
    if guid1 and guid2:
        create_animated_tile_asset(f"Summer_tuft_{i}", guid1, guid2)
        print(f"Created asset for Summer_tuft_{i}")
    else:
        print(f"Could not find GUIDs for Summer_tuft_{i}")

for i in range(1, 6):
    f1_meta = f"{tufts_dir}Summer_flower_{i}_f1.png.meta"
    f2_meta = f"{tufts_dir}Summer_flower_{i}_f2.png.meta"
    
    guid1 = get_guid(f1_meta)
    guid2 = get_guid(f2_meta)
    
    if guid1 and guid2:
        create_animated_tile_asset(f"Summer_flower_{i}", guid1, guid2)
        print(f"Created asset for Summer_flower_{i}")
    else:
        print(f"Could not find GUIDs for Summer_flower_{i}")
