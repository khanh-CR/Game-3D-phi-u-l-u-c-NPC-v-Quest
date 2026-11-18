# PSX Dungeon Texture Pack

67 seamless textures built for PS1-era horror, dungeon crawlers and retro FPS.
Baked lighting, real 15-bit colour, ordered dithering — the grain is authentic,
not a filter.

Everything tiles seamlessly in both axes.

## Contents

| Folder | Notes |
|---|---|
| `textures/` | 256x256 — PSX max texture-page size. Use this one. |
| `textures_128/` | 128x128 — chunkier, closer to what shipped on real PS1 discs. |
| `PREVIEW.png` | Labelled contact sheet. |

**Walls (23)** — stonebrick + mossy / cracked / bloody / dark, rubble masonry,
cave rock, red brick, broken plaster, carved runes, wood panel, sandstone block,
hieroglyphs, marble white, marble black, obsidian, prison tally, ivy overgrown,
cobweb stone, scorched, slime, riveted metal, wattle & daub, catacomb skulls,
glazed tile

**Floors (20)** — cobblestone, wet cobblestone, flagstone, dirt & gravel,
checker tile, marble tile, mosaic, wood planks, parquet, brick basketweave,
sewer brick, sand, straw, ash, mud puddles, ice, lava cracks, bone gravel,
sludge water, carpet

**Ceilings (4)** — vaulted stone, brick vault, cave rock, wood beams

**Metal (6)** — rusted plate, copper patina, chain links, iron bars*, grate*,
diamond plate

**Props & trim (8)** — iron-banded wood door, stone door, barrel, crate,
tapestry, stone pillar, stone trim block, plus liquid water and lava

**Decals (3)** — bloodstain*, cobweb*, moss patch*

\* RGBA with a hard 1-bit alpha cutout — no soft edges, matching how PS1
handled this kind of geometry. Decals are meant to be laid over the tiling
textures on a second quad or as a detail pass.

All `wall_`, `floor_`, `ceiling_`, `metal_`, `liquid_` and `trim_` tiles repeat
in both axes. `prop_door_wood_iron` and `prop_door_stone` are one-offs and do
not tile.

## Import settings

**Unity**
- Filter Mode: **Point (no filter)**
- Compression: **None** (or High Quality) — DXT will smear the dither
- Wrap Mode: Repeat
- Generate Mip Maps: on for floors and ceilings, off for walls you'll always be
  close to. Aniso Level 0.
- Max Size 256, sRGB on
- For alpha cutouts and decals use an Unlit/Cutout shader, alpha clip ~0.5

**Godot** — Filter: Nearest, Mipmaps: On, Repeat: Enabled, Compress: Lossless

## Verifying the claims

See `VERIFY.md`, and run `verify.py` against the files you downloaded. The
15-bit palette and the both-axis tiling are both measurable rather than
something you have to take on trust.

**Unreal** — Filter: Nearest, Compression: UserInterface2D (RGBA) to keep the
dither intact, Mip Gen Settings: FromTextureGroup

## Selling the era

Two things do most of the work beyond the textures themselves:

1. **Vertex snapping** — quantise clip-space XY in the vertex shader (snap to
   roughly a 160x120 grid) for the classic wobble.
2. **Affine texture mapping** — drop the perspective divide on UVs so textures
   warp across large triangles. Keep floor polys small or it gets unreadable.

Add short-distance fog and render to a 320x240 target upscaled with point
filtering and it's basically indistinguishable from the real thing.

## Licence

Use in personal and commercial projects, unlimited titles. No credit required,
though it's appreciated. You may not resell or redistribute the textures as-is,
as part of another asset pack, or in any AI training dataset.
