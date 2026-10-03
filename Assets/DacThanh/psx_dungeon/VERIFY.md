# Verifying the claims

Two things get claimed a lot in retro texture packs and are rarely checkable
before you're already importing them. Both are testable here.

Run `python3 verify.py` from this folder (needs numpy and Pillow). It reads the
PNGs you downloaded — nothing else.

## 1. Genuine 15-bit colour

Every texture is quantised to 5 bits per channel at authoring time, with an 8x8
Bayer ordered dither. This is not a grain overlay on full-colour art.

You can confirm it without the script: open any tile in a colour picker and step
through pixels. No channel value falls outside a 32-step ramp.

Measured across all 67 textures:

```
5-bit palette      PASS — no channel exceeds 32 levels
unique colours     min 40, median 135, max 1,101
                   (full-colour art would allow 16,777,216)
```

## 2. Tiles in both axes

65 of 67 wrap in both X and Y. `prop_door_wood_iron` and `prop_door_stone` are
deliberate one-offs and are not meant to tile.

The script's seam test flags roughly 17 textures. **That is expected and is not
a list of broken files.** It compares the wrap-around pixel pair against all 256
adjacent pairs in the image, and flags the seam when it ranks near the top. A
mortar line or plank joint sitting exactly on the tile boundary produces a
genuinely high-contrast pair while wrapping perfectly. Smooth textures like the
marble exaggerate this further, because a tiny difference looks enormous against
a near-zero baseline.

Every flagged texture was tiled 2x2 and inspected before release. If you want to
confirm it yourself, that's the real test — repeat the tile and look for a line.
No statistic substitutes for it.
