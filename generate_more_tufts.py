import os
from PIL import Image

tufts_dir = 'Assets/05_Tiles/Grass/Summer/Tufts/'

# We have tuft 1 and tuft 2
# Tuft 1
t1_f1 = Image.open(tufts_dir + 'Summer_tuft_1_f1.png').convert('RGBA')
t1_f2 = Image.open(tufts_dir + 'Summer_tuft_1_f2.png').convert('RGBA')

# Tuft 2
t2_f1 = Image.open(tufts_dir + 'Summer_tuft_2_f1.png').convert('RGBA')
t2_f2 = Image.open(tufts_dir + 'Summer_tuft_2_f2.png').convert('RGBA')

# Tuft 3 (Tuft 1 Flipped Horizontally)
t1_f1.transpose(Image.FLIP_LEFT_RIGHT).save(tufts_dir + 'Summer_tuft_3_f1.png')
t1_f2.transpose(Image.FLIP_LEFT_RIGHT).save(tufts_dir + 'Summer_tuft_3_f2.png')

# Tuft 4 (Tuft 2 Flipped Horizontally)
t2_f1.transpose(Image.FLIP_LEFT_RIGHT).save(tufts_dir + 'Summer_tuft_4_f1.png')
t2_f2.transpose(Image.FLIP_LEFT_RIGHT).save(tufts_dir + 'Summer_tuft_4_f2.png')

# Tuft 5 (Tuft 1 Flipped Vertically)
t1_f1.transpose(Image.FLIP_TOP_BOTTOM).save(tufts_dir + 'Summer_tuft_5_f1.png')
t1_f2.transpose(Image.FLIP_TOP_BOTTOM).save(tufts_dir + 'Summer_tuft_5_f2.png')

# Tuft 6 (Tuft 2 Flipped Vertically)
t2_f1.transpose(Image.FLIP_TOP_BOTTOM).save(tufts_dir + 'Summer_tuft_6_f1.png')
t2_f2.transpose(Image.FLIP_TOP_BOTTOM).save(tufts_dir + 'Summer_tuft_6_f2.png')

# Tuft 7 (Tuft 1 Flipped Both)
t1_f1.transpose(Image.FLIP_LEFT_RIGHT).transpose(Image.FLIP_TOP_BOTTOM).save(tufts_dir + 'Summer_tuft_7_f1.png')
t1_f2.transpose(Image.FLIP_LEFT_RIGHT).transpose(Image.FLIP_TOP_BOTTOM).save(tufts_dir + 'Summer_tuft_7_f2.png')

print("Generated 5 new animated tufts successfully!")
