# Public asset-slot catalog

This searchable catalog is generated from `CoreAssetSlotBinder.cs`, `CoreLegacyVisualSpriteCatalog.cs`, packaged Core weapon definitions, and `core-clothing.json`. Regenerate it with `Tools/Documentation/Generate-AssetSlotCatalog.ps1`.

A slot is registered only when its baseline Unity sprite or renderer is available. This matters for legacy weapons and enemy rigs whose prefabs do not all expose the same pieces. An unknown target is rejected instead of guessing a private Unity name.

## Enemy semantic slots

For a Core enemy ID `core:enemy/<enemy>`, the binder publishes the following suffixes when that named renderer exists:

- `core:enemy/<enemy>/body/arm-lower`
- `core:enemy/<enemy>/body/arm-upper`
- `core:enemy/<enemy>/body/butt`
- `core:enemy/<enemy>/body/chest`
- `core:enemy/<enemy>/body/foot-left`
- `core:enemy/<enemy>/body/foot-right`
- `core:enemy/<enemy>/body/hand`
- `core:enemy/<enemy>/body/head`
- `core:enemy/<enemy>/body/hips`
- `core:enemy/<enemy>/body/leg-lower`
- `core:enemy/<enemy>/body/leg-upper`
- `core:enemy/<enemy>/body/neck`
- `core:enemy/<enemy>/body/penis`
- `core:enemy/<enemy>/body/torso-lower`

Paired limbs intentionally share semantic slots except for left and right feet. Use the in-game validation result to confirm availability on a particular legacy enemy.

## Exact registered slot IDs

| Slot ID | Family | Meaning | Availability |
| --- | --- | --- | --- |
| `core:clothing/arm-wrap/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/arm-wrap/piece/r-arm-upper` | clothing | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/bandana/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/bandana/piece/head` | clothing | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/bikini-jewelry/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/bikini-jewelry/piece/shirt-chest` | clothing | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/bikini-marine/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/bikini-marine/piece/shirt-chest` | clothing | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/bikini-orange/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/bikini-orange/piece/shirt-chest` | clothing | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/bikini-pink/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/bikini-pink/piece/shirt-chest` | clothing | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/bikini-white/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/bikini-white/piece/shirt-chest` | clothing | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/black-top/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/black-top/piece/shirt-chest` | clothing | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/christy-skirt/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/christy-skirt/piece/butt` | clothing | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/christy-skirt/piece/hips` | clothing | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/christy-skirt/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/christy-skirt/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/christy-skirt/piece/skirt-hips` | clothing | Garment piece/skirt-hips | registered from the reconstructed garment |
| `core:clothing/crown/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/crown/piece/head` | clothing | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/crying-mask/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/crying-mask/piece/head` | clothing | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/dark-purple-top/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/dark-purple-top/piece/shirt-chest` | clothing | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/dog-collar/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/dog-collar/piece/neck` | clothing | Garment piece/neck | registered from the reconstructed garment |
| `core:clothing/dog-ears/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/dog-ears/piece/head` | clothing | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/dress-ada/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/dress-ada/piece/butt` | clothing | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/dress-ada/piece/chest` | clothing | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/dress-ada/piece/hips` | clothing | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/dress-ada/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/dress-ada/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/dress-ada/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/eye-wrap/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/eye-wrap/piece/head` | clothing | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/feet-jewelry/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/feet-jewelry/piece/l-foot` | clothing | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/feet-jewelry/piece/l-leg-lower` | clothing | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/feet-jewelry/piece/r-foot` | clothing | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/feet-jewelry/piece/r-leg-lower` | clothing | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/glasses-1/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/glasses-1/piece/glasses` | clothing | Garment piece/glasses | registered from the reconstructed garment |
| `core:clothing/glasses-2/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/glasses-2/piece/glasses` | clothing | Garment piece/glasses | registered from the reconstructed garment |
| `core:clothing/golden-arm-bracelet/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/golden-arm-bracelet/piece/r-arm-upper` | clothing | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/hair-2/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-2/piece/hair` | clothing | Garment piece/hair | registered from the reconstructed garment |
| `core:clothing/hair-ada/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-ada/piece/head` | clothing | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/hair-asian/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-asian/piece/hair` | clothing | Garment piece/hair | registered from the reconstructed garment |
| `core:clothing/hair-blonde-bowl-red-line/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-blonde-bowl-red-line/piece/hair` | clothing | Garment piece/hair | registered from the reconstructed garment |
| `core:clothing/hair-blonde-pony-tail/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-blonde-pony-tail/piece/hair` | clothing | Garment piece/hair | registered from the reconstructed garment |
| `core:clothing/hair-brown-big/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-brown-big/piece/hair` | clothing | Garment piece/hair | registered from the reconstructed garment |
| `core:clothing/hair-brown-pony-tail/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-brown-pony-tail/piece/hair` | clothing | Garment piece/hair | registered from the reconstructed garment |
| `core:clothing/hair-default/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-default/piece/hair` | clothing | Garment piece/hair | registered from the reconstructed garment |
| `core:clothing/hair-punk/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-punk/piece/hair` | clothing | Garment piece/hair | registered from the reconstructed garment |
| `core:clothing/hair-white-blonde/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hair-white-blonde/piece/hair` | clothing | Garment piece/hair | registered from the reconstructed garment |
| `core:clothing/hand-jill/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hand-jill/piece/l-hand` | clothing | Garment piece/l-hand | registered from the reconstructed garment |
| `core:clothing/hand-jill/piece/r-hand` | clothing | Garment piece/r-hand | registered from the reconstructed garment |
| `core:clothing/hard-hat/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hard-hat/piece/head` | clothing | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/hat-christy/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hat-christy/piece/head` | clothing | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/hat-jill/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hat-jill/piece/head` | clothing | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/butt` | clothing | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/chest` | clothing | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/head` | clothing | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/hips` | clothing | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/l-arm-lower` | clothing | Garment piece/l-arm-lower | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/l-arm-upper` | clothing | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/l-foot` | clothing | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/l-hand` | clothing | Garment piece/l-hand | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/l-leg-lower` | clothing | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/l-leg-lower-shoes` | clothing | Garment piece/l-leg-lower-shoes | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/mask` | clothing | Garment piece/mask | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/neck` | clothing | Garment piece/neck | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/r-arm-lower` | clothing | Garment piece/r-arm-lower | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/r-arm-upper` | clothing | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/r-foot` | clothing | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/r-hand` | clothing | Garment piece/r-hand | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/r-leg-lower` | clothing | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/r-leg-lower-shoes` | clothing | Garment piece/r-leg-lower-shoes | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/hazmat-suit/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/hockey-mask/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/hockey-mask/piece/head` | clothing | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/jeans-blue/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/jeans-blue/piece/butt` | clothing | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/jeans-blue/piece/hips` | clothing | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/jeans-blue/piece/l-leg-lower` | clothing | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/jeans-blue/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/jeans-blue/piece/r-leg-lower` | clothing | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/jeans-blue/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/jeans-blue/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/jeans-white/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/jeans-white/piece/butt` | clothing | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/jeans-white/piece/hips` | clothing | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/jeans-white/piece/l-leg-lower` | clothing | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/jeans-white/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/jeans-white/piece/r-leg-lower` | clothing | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/jeans-white/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/jeans-white/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/jewelry-ear/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/jewelry-ear/piece/ear` | clothing | Garment piece/ear | registered from the reconstructed garment |
| `core:clothing/jewelry-sleeve/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/jewelry-sleeve/piece/r-arm-lower` | clothing | Garment piece/r-arm-lower | registered from the reconstructed garment |
| `core:clothing/jewelry-sleeve/piece/r-hand` | clothing | Garment piece/r-hand | registered from the reconstructed garment |
| `core:clothing/knight-boots/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/knight-boots/piece/l-foot` | clothing | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/knight-boots/piece/l-leg-lower-armor` | clothing | Garment piece/l-leg-lower-armor | registered from the reconstructed garment |
| `core:clothing/knight-boots/piece/l-leg-lower-stocking` | clothing | Garment piece/l-leg-lower-stocking | registered from the reconstructed garment |
| `core:clothing/knight-boots/piece/l-leg-upper-stocking` | clothing | Garment piece/l-leg-upper-stocking | registered from the reconstructed garment |
| `core:clothing/knight-boots/piece/r-foot` | clothing | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/knight-boots/piece/r-leg-lower-armor` | clothing | Garment piece/r-leg-lower-armor | registered from the reconstructed garment |
| `core:clothing/knight-boots/piece/r-leg-lower-stocking` | clothing | Garment piece/r-leg-lower-stocking | registered from the reconstructed garment |
| `core:clothing/knight-boots/piece/r-leg-upper-stocking` | clothing | Garment piece/r-leg-upper-stocking | registered from the reconstructed garment |
| `core:clothing/knight-breast-plate/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/knight-breast-plate/piece/arm-upper` | clothing | Garment piece/arm-upper | registered from the reconstructed garment |
| `core:clothing/knight-breast-plate/piece/chest` | clothing | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/knight-helmet/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/knight-helmet/piece/head` | clothing | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/knight-pants/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/knight-pants/piece/butt` | clothing | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/knight-pants/piece/hips` | clothing | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/knight-pants/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/knight-pants/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/knight-pants/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/latex-gloves-black/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/latex-gloves-black/piece/l-arm-lower` | clothing | Garment piece/l-arm-lower | registered from the reconstructed garment |
| `core:clothing/latex-gloves-black/piece/l-arm-upper` | clothing | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/latex-gloves-black/piece/l-hand` | clothing | Garment piece/l-hand | registered from the reconstructed garment |
| `core:clothing/latex-gloves-black/piece/r-arm-lower` | clothing | Garment piece/r-arm-lower | registered from the reconstructed garment |
| `core:clothing/latex-gloves-black/piece/r-arm-upper` | clothing | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/latex-gloves-black/piece/r-hand` | clothing | Garment piece/r-hand | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-purple/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-purple/piece/l-arm-lower` | clothing | Garment piece/l-arm-lower | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-purple/piece/l-arm-upper` | clothing | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-purple/piece/l-hand` | clothing | Garment piece/l-hand | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-purple/piece/r-arm-lower` | clothing | Garment piece/r-arm-lower | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-purple/piece/r-arm-upper` | clothing | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-purple/piece/r-hand` | clothing | Garment piece/r-hand | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-red/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-red/piece/l-arm-lower` | clothing | Garment piece/l-arm-lower | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-red/piece/l-arm-upper` | clothing | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-red/piece/l-hand` | clothing | Garment piece/l-hand | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-red/piece/r-arm-lower` | clothing | Garment piece/r-arm-lower | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-red/piece/r-arm-upper` | clothing | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/latex-gloves-dark-red/piece/r-hand` | clothing | Garment piece/r-hand | registered from the reconstructed garment |
| `core:clothing/lingerie-black-lower/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/lingerie-black-lower/piece/hips` | clothing | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/lingerie-black-lower/piece/l-foot` | clothing | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/lingerie-black-lower/piece/l-leg-lower` | clothing | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/lingerie-black-lower/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/lingerie-black-lower/piece/r-foot` | clothing | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/lingerie-black-lower/piece/r-leg-lower` | clothing | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/lingerie-black-lower/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/lingerie-black-lower/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/lingerie-black-upper/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/lingerie-black-upper/piece/chest` | clothing | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/lingerie-white-lower/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/lingerie-white-lower/piece/hips` | clothing | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/lingerie-white-lower/piece/l-foot` | clothing | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/lingerie-white-lower/piece/l-leg-lower` | clothing | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/lingerie-white-lower/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/lingerie-white-lower/piece/r-foot` | clothing | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/lingerie-white-lower/piece/r-leg-lower` | clothing | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/lingerie-white-lower/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/lingerie-white-lower/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/lingerie-white-upper/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/lingerie-white-upper/piece/chest` | clothing | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/pants-default/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/pants-default/piece/belt` | clothing | Garment piece/belt | registered from the reconstructed garment |
| `core:clothing/pants-default/piece/butt` | clothing | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/pants-default/piece/hips` | clothing | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/pants-default/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/pants-default/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/pants-jill/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/pants-jill/piece/butt` | clothing | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/pants-jill/piece/hips` | clothing | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/pants-jill/piece/l-leg-lower` | clothing | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/pants-jill/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/pants-jill/piece/r-leg-lower` | clothing | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/pants-jill/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/pants-jill/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/pants-underwear/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/pants-underwear/piece/butt` | clothing | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/pants-underwear/piece/hips` | clothing | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/pants-underwear/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/pants-underwear/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/pants-underwear/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-body/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-body/piece/butt` | clothing | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-body/piece/chest` | clothing | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-body/piece/hips` | clothing | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-body/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-ears/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-ears/piece/head` | clothing | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-sleeves/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-sleeves/piece/l-arm-lower` | clothing | Garment piece/l-arm-lower | registered from the reconstructed garment |
| `core:clothing/playboy-bunny-sleeves/piece/r-arm-lower` | clothing | Garment piece/r-arm-lower | registered from the reconstructed garment |
| `core:clothing/programming-socks-blue-white/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/programming-socks-blue-white/piece/l-foot` | clothing | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/programming-socks-blue-white/piece/l-leg-lower` | clothing | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/programming-socks-blue-white/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/programming-socks-blue-white/piece/r-foot` | clothing | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/programming-socks-blue-white/piece/r-leg-lower` | clothing | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/programming-socks-blue-white/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/programming-socks-jacky-style/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/programming-socks-jacky-style/piece/l-foot` | clothing | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/programming-socks-jacky-style/piece/l-leg-lower` | clothing | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/programming-socks-jacky-style/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/programming-socks-jacky-style/piece/r-foot` | clothing | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/programming-socks-jacky-style/piece/r-leg-lower` | clothing | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/programming-socks-jacky-style/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/programming-socks-pink-white/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/programming-socks-pink-white/piece/l-foot` | clothing | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/programming-socks-pink-white/piece/l-leg-lower` | clothing | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/programming-socks-pink-white/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/programming-socks-pink-white/piece/r-foot` | clothing | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/programming-socks-pink-white/piece/r-leg-lower` | clothing | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/programming-socks-pink-white/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/scientist-set/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/butt` | clothing | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/hips` | clothing | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/l-leg-lower` | clothing | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/r-leg-lower` | clothing | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/shirt-chest` | clothing | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/shirt-l-arm-lower` | clothing | Garment piece/shirt-l-arm-lower | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/shirt-l-arm-upper` | clothing | Garment piece/shirt-l-arm-upper | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/shirt-r-arm-lower` | clothing | Garment piece/shirt-r-arm-lower | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/shirt-r-arm-upper` | clothing | Garment piece/shirt-r-arm-upper | registered from the reconstructed garment |
| `core:clothing/scientist-set/piece/shirt-spine` | clothing | Garment piece/shirt-spine | registered from the reconstructed garment |
| `core:clothing/security-hat/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/security-hat/piece/shirt-collar` | clothing | Garment piece/shirt-collar | registered from the reconstructed garment |
| `core:clothing/security-pants/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/security-pants/piece/butt` | clothing | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/security-pants/piece/hips` | clothing | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/security-pants/piece/l-leg-lower` | clothing | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/security-pants/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/security-pants/piece/r-leg-lower` | clothing | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/security-pants/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/security-pants/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/security-shirt/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/security-shirt/piece/shirt-chest` | clothing | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/security-shirt/piece/shirt-collar` | clothing | Garment piece/shirt-collar | registered from the reconstructed garment |
| `core:clothing/security-shirt/piece/shirt-l-arm-lower` | clothing | Garment piece/shirt-l-arm-lower | registered from the reconstructed garment |
| `core:clothing/security-shirt/piece/shirt-l-arm-upper` | clothing | Garment piece/shirt-l-arm-upper | registered from the reconstructed garment |
| `core:clothing/security-shirt/piece/shirt-r-arm-lower` | clothing | Garment piece/shirt-r-arm-lower | registered from the reconstructed garment |
| `core:clothing/security-shirt/piece/shirt-r-arm-upper` | clothing | Garment piece/shirt-r-arm-upper | registered from the reconstructed garment |
| `core:clothing/security-shirt/piece/shirt-spine` | clothing | Garment piece/shirt-spine | registered from the reconstructed garment |
| `core:clothing/security-shoes/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/security-shoes/piece/l-foot` | clothing | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/security-shoes/piece/r-foot` | clothing | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/shirt-1/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shirt-1/piece/chest` | clothing | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/shirt-1/piece/l-arm-upper` | clothing | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-1/piece/r-arm-upper` | clothing | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-1/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/shirt-2/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shirt-2/piece/chest` | clothing | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/shirt-2/piece/l-arm-upper` | clothing | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-2/piece/r-arm-upper` | clothing | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-2/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/shirt-3/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shirt-3/piece/chest` | clothing | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/shirt-3/piece/l-arm-upper` | clothing | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-3/piece/r-arm-upper` | clothing | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-3/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/shirt-christy/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shirt-christy/piece/l-arm-lower` | clothing | Garment piece/l-arm-lower | registered from the reconstructed garment |
| `core:clothing/shirt-christy/piece/l-arm-upper` | clothing | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-christy/piece/r-arm-lower` | clothing | Garment piece/r-arm-lower | registered from the reconstructed garment |
| `core:clothing/shirt-christy/piece/r-arm-upper` | clothing | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-christy/piece/shirt-chest` | clothing | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/shirt-christy/piece/shirt-neck` | clothing | Garment piece/shirt-neck | registered from the reconstructed garment |
| `core:clothing/shirt-christy/piece/shirt-spine` | clothing | Garment piece/shirt-spine | registered from the reconstructed garment |
| `core:clothing/shirt-damaged/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shirt-damaged/piece/chest` | clothing | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/shirt-damaged/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/shirt-default/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shirt-default/piece/shirt-chest` | clothing | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/shirt-default/piece/shirt-spine` | clothing | Garment piece/shirt-spine | registered from the reconstructed garment |
| `core:clothing/shirt-jill/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shirt-jill/piece/chest` | clothing | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/shirt-jill/piece/l-arm-upper` | clothing | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-jill/piece/r-arm-upper` | clothing | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/shirt-jill/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/shirt-top-blue/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shirt-top-blue/piece/shirt-chest` | clothing | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/shirt-top-orange/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shirt-top-orange/piece/shirt-chest` | clothing | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/shoes-ada/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shoes-ada/piece/l-foot` | clothing | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/shoes-ada/piece/r-foot` | clothing | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/shoes-christy/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shoes-christy/piece/l-foot` | clothing | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/shoes-christy/piece/r-foot` | clothing | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/shoes-default/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shoes-default/piece/l-foot` | clothing | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/shoes-default/piece/r-foot` | clothing | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/shoes-jill/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shoes-jill/piece/l-foot` | clothing | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/shoes-jill/piece/l-leg-lower` | clothing | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/shoes-jill/piece/r-foot` | clothing | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/shoes-jill/piece/r-leg-lower` | clothing | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-blue/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-blue/piece/butt` | clothing | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-blue/piece/hips` | clothing | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-blue/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-blue/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-blue/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-white/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-white/piece/butt` | clothing | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-white/piece/hips` | clothing | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-white/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-white/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/shorts-jeans-white/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/silver-arm-bracelet/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/silver-arm-bracelet/piece/r-arm-upper` | clothing | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/sports-sneakers-black/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/sports-sneakers-black/piece/l-foot` | clothing | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/sports-sneakers-black/piece/r-foot` | clothing | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/sports-sneakers-white/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/sports-sneakers-white/piece/l-foot` | clothing | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/sports-sneakers-white/piece/r-foot` | clothing | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/stockings-ada/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/stockings-ada/piece/l-foot` | clothing | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/stockings-ada/piece/l-leg-lower` | clothing | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/stockings-ada/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/stockings-ada/piece/r-foot` | clothing | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/stockings-ada/piece/r-leg-lower` | clothing | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/stockings-ada/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/stockings-default/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/stockings-default/piece/l-foot` | clothing | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/stockings-default/piece/l-leg-lower` | clothing | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/stockings-default/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/stockings-default/piece/r-foot` | clothing | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/stockings-default/piece/r-leg-lower` | clothing | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/stockings-default/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/stockings-punk/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/stockings-punk/piece/l-foot` | clothing | Garment piece/l-foot | registered from the reconstructed garment |
| `core:clothing/stockings-punk/piece/l-leg-lower` | clothing | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/stockings-punk/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/stockings-punk/piece/r-foot` | clothing | Garment piece/r-foot | registered from the reconstructed garment |
| `core:clothing/stockings-punk/piece/r-leg-lower` | clothing | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/stockings-punk/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/string-jewelry/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/string-jewelry/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/string-marine/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/string-marine/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/string-orange/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/string-orange/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/string-pink/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/string-pink/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/string-white/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/string-white/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/summer-hat/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/summer-hat/piece/head` | clothing | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/sweater-1/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/sweater-1/piece/chest` | clothing | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/sweater-1/piece/l-arm-lower` | clothing | Garment piece/l-arm-lower | registered from the reconstructed garment |
| `core:clothing/sweater-1/piece/l-arm-upper` | clothing | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/sweater-1/piece/r-arm-lower` | clothing | Garment piece/r-arm-lower | registered from the reconstructed garment |
| `core:clothing/sweater-1/piece/r-arm-upper` | clothing | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/sweater-1/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/sweater-2/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/sweater-2/piece/chest` | clothing | Garment piece/chest | registered from the reconstructed garment |
| `core:clothing/sweater-2/piece/l-arm-lower` | clothing | Garment piece/l-arm-lower | registered from the reconstructed garment |
| `core:clothing/sweater-2/piece/l-arm-upper` | clothing | Garment piece/l-arm-upper | registered from the reconstructed garment |
| `core:clothing/sweater-2/piece/r-arm-lower` | clothing | Garment piece/r-arm-lower | registered from the reconstructed garment |
| `core:clothing/sweater-2/piece/r-arm-upper` | clothing | Garment piece/r-arm-upper | registered from the reconstructed garment |
| `core:clothing/sweater-2/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/tiara/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/tiara/piece/head` | clothing | Garment piece/head | registered from the reconstructed garment |
| `core:clothing/white-top/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/white-top/piece/shirt-chest` | clothing | Garment piece/shirt-chest | registered from the reconstructed garment |
| `core:clothing/yoga-pants-1/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/yoga-pants-1/piece/butt` | clothing | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/yoga-pants-1/piece/hips` | clothing | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/yoga-pants-1/piece/l-leg-lower` | clothing | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/yoga-pants-1/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/yoga-pants-1/piece/r-leg-lower` | clothing | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/yoga-pants-1/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/yoga-pants-1/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:clothing/yoga-pants-2/icon` | clothing | Wardrobe icon | registered from the reconstructed garment |
| `core:clothing/yoga-pants-2/piece/butt` | clothing | Garment piece/butt | registered from the reconstructed garment |
| `core:clothing/yoga-pants-2/piece/hips` | clothing | Garment piece/hips | registered from the reconstructed garment |
| `core:clothing/yoga-pants-2/piece/l-leg-lower` | clothing | Garment piece/l-leg-lower | registered from the reconstructed garment |
| `core:clothing/yoga-pants-2/piece/l-leg-upper` | clothing | Garment piece/l-leg-upper | registered from the reconstructed garment |
| `core:clothing/yoga-pants-2/piece/r-leg-lower` | clothing | Garment piece/r-leg-lower | registered from the reconstructed garment |
| `core:clothing/yoga-pants-2/piece/r-leg-upper` | clothing | Garment piece/r-leg-upper | registered from the reconstructed garment |
| `core:clothing/yoga-pants-2/piece/spine` | clothing | Garment piece/spine | registered from the reconstructed garment |
| `core:enemy-anatomy/sprite/penis` | named-core-sprite | Core sprite Penis | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-0` | named-core-sprite | Core sprite penis_0 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-1` | named-core-sprite | Core sprite penis_1 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-2` | named-core-sprite | Core sprite Penis_2 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-3` | named-core-sprite | Core sprite Penis_3 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-4` | named-core-sprite | Core sprite Penis_4 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-5` | named-core-sprite | Core sprite penis_5 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-6` | named-core-sprite | Core sprite Penis_6 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-7` | named-core-sprite | Core sprite Penis_7 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-base` | named-core-sprite | Core sprite PenisBase | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-base-0` | named-core-sprite | Core sprite PenisBase_0 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-base-1` | named-core-sprite | Core sprite PenisBase_1 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-base-2` | named-core-sprite | Core sprite PenisBase_2 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-base-3` | named-core-sprite | Core sprite PenisBase_3 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-base-4` | named-core-sprite | Core sprite PenisBase_4 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-base-5` | named-core-sprite | Core sprite PenisBase_5 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-base-6` | named-core-sprite | Core sprite PenisBase_6 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-end` | named-core-sprite | Core sprite PenisEnd | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-end-0` | named-core-sprite | Core sprite PenisEnd_0 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-end-1` | named-core-sprite | Core sprite PenisEnd_1 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-end-2` | named-core-sprite | Core sprite PenisEnd_2 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-end-3` | named-core-sprite | Core sprite PenisEnd_3 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-mid` | named-core-sprite | Core sprite PenisMid | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-mid-0` | named-core-sprite | Core sprite PenisMid_0 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-mid-1` | named-core-sprite | Core sprite PenisMid_1 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-middle` | named-core-sprite | Core sprite PenisMiddle | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-rod` | named-core-sprite | Core sprite PenisRod | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-rod-0` | named-core-sprite | Core sprite PenisRod_0 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-tip` | named-core-sprite | Core sprite PenisTip | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-tip-0` | named-core-sprite | Core sprite PenisTip_0 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-tip-1` | named-core-sprite | Core sprite PenisTip_1 | runtime baseline required |
| `core:enemy-anatomy/sprite/penis-tip-2` | named-core-sprite | Core sprite PenisTip_2 | runtime baseline required |
| `core:map-art/usable-vendor/sprite` | map-art | Usable vendor world artwork | registered when the Core template or a Tiled vendor is available |
| `core:player/body/arm-lower/black` | player | Player arm-lower artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/arm-lower/pale` | player | Player arm-lower artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/arm-lower/tan` | player | Player arm-lower artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/arm-lower/white` | player | Player arm-lower artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/arm-upper/black` | player | Player arm-upper artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/arm-upper/pale` | player | Player arm-upper artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/arm-upper/tan` | player | Player arm-upper artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/arm-upper/white` | player | Player arm-upper artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/butt/black` | player | Player butt artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/butt/pale` | player | Player butt artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/butt/tan` | player | Player butt artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/butt/white` | player | Player butt artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/chest/black` | player | Player chest artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/chest/pale` | player | Player chest artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/chest/tan` | player | Player chest artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/chest/white` | player | Player chest artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/ear/black` | player | Player ear artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/ear/pale` | player | Player ear artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/ear/tan` | player | Player ear artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/ear/white` | player | Player ear artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/foot/black` | player | Player foot artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/foot/pale` | player | Player foot artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/foot/tan` | player | Player foot artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/foot/white` | player | Player foot artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/hand/black` | player | Player hand artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/hand/pale` | player | Player hand artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/hand/tan` | player | Player hand artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/hand/white` | player | Player hand artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/head/black` | player | Player head artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/head/pale` | player | Player head artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/head/tan` | player | Player head artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/head/white` | player | Player head artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/hips/black` | player | Player hips artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/hips/pale` | player | Player hips artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/hips/tan` | player | Player hips artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/hips/white` | player | Player hips artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/leg-lower/black` | player | Player leg-lower artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/leg-lower/pale` | player | Player leg-lower artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/leg-lower/tan` | player | Player leg-lower artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/leg-lower/white` | player | Player leg-lower artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/leg-upper/black` | player | Player leg-upper artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/leg-upper/pale` | player | Player leg-upper artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/leg-upper/tan` | player | Player leg-upper artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/leg-upper/white` | player | Player leg-upper artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/neck/black` | player | Player neck artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/neck/pale` | player | Player neck artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/neck/tan` | player | Player neck artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/neck/white` | player | Player neck artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/body/torso-lower/black` | player | Player torso-lower artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/body/torso-lower/pale` | player | Player torso-lower artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/body/torso-lower/tan` | player | Player torso-lower artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/body/torso-lower/white` | player | Player torso-lower artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/face/blush/image` | named-core-sprite | Core sprite Blush_0 | runtime baseline required |
| `core:player/face/eyelid-lower/black` | player | Player face/eyelid-lower artwork for black skin | registered when the shipped skin sprite exists |
| `core:player/face/eyelid-lower/pale` | player | Player face/eyelid-lower artwork for pale skin | registered when the shipped skin sprite exists |
| `core:player/face/eyelid-lower/tan` | player | Player face/eyelid-lower artwork for tan skin | registered when the shipped skin sprite exists |
| `core:player/face/eyelid-lower/white` | player | Player face/eyelid-lower artwork for white skin | registered when the shipped skin sprite exists |
| `core:player/face/mouth/mouth-10` | player-mouth | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-12` | player-mouth | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-2` | player-mouth | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-3` | player-mouth | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-4` | player-mouth | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-5` | player-mouth | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-6` | player-mouth | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-7` | player-mouth | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-8` | player-mouth | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-9` | player-mouth | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-oral-3` | player-mouth | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:player/face/mouth/mouth-tired-2` | player-mouth | Player mouth expression | registered when a scene mouth manager supplies the sprite |
| `core:sprite/ammo-box/image` | legacy-presentation | Legacy Core sprite AmmoBox | runtime baseline required |
| `core:sprite/anaphrodisiac/image` | legacy-presentation | Legacy Core sprite anaphrodisiac | runtime baseline required |
| `core:sprite/android-parts-cabinet/image` | legacy-presentation | Legacy Core sprite AndroidPartsCabinet | runtime baseline required |
| `core:sprite/arm-lower-1/image` | legacy-presentation | Legacy Core sprite ArmLower_1 | runtime baseline required |
| `core:sprite/arm-lower-10/image` | legacy-presentation | Legacy Core sprite ArmLower_10 | runtime baseline required |
| `core:sprite/arm-lower-15/image` | legacy-presentation | Legacy Core sprite ArmLower_15 | runtime baseline required |
| `core:sprite/arm-lower-19/image` | legacy-presentation | Legacy Core sprite ArmLower_19 | runtime baseline required |
| `core:sprite/arm-lower-2/image` | legacy-presentation | Legacy Core sprite ArmLower_2 | runtime baseline required |
| `core:sprite/arm-lower-5/image` | legacy-presentation | Legacy Core sprite ArmLower_5 | runtime baseline required |
| `core:sprite/arm-lower-8/image` | legacy-presentation | Legacy Core sprite ArmLower_8 | runtime baseline required |
| `core:sprite/arm-lower-bloody-0/image` | legacy-presentation | Legacy Core sprite ArmLowerBloody_0 | runtime baseline required |
| `core:sprite/arm-lower-bloody/image` | legacy-presentation | Legacy Core sprite ArmLowerBloody | runtime baseline required |
| `core:sprite/arm-upper-0/image` | legacy-presentation | Legacy Core sprite ArmUpper_0 | runtime baseline required |
| `core:sprite/arm-upper-10/image` | legacy-presentation | Legacy Core sprite ArmUpper_10 | runtime baseline required |
| `core:sprite/arm-upper-16/image` | legacy-presentation | Legacy Core sprite ArmUpper_16 | runtime baseline required |
| `core:sprite/arm-upper-17/image` | legacy-presentation | Legacy Core sprite ArmUpper_17 | runtime baseline required |
| `core:sprite/arm-upper-18/image` | legacy-presentation | Legacy Core sprite ArmUpper_18 | runtime baseline required |
| `core:sprite/arm-upper-20/image` | legacy-presentation | Legacy Core sprite ArmUpper_20 | runtime baseline required |
| `core:sprite/arm-upper-5/image` | legacy-presentation | Legacy Core sprite ArmUpper_5 | runtime baseline required |
| `core:sprite/aspirin/image` | legacy-presentation | Legacy Core sprite Aspirin | runtime baseline required |
| `core:sprite/b-prec/image` | legacy-presentation | Legacy Core sprite BPrec | runtime baseline required |
| `core:sprite/background/image` | legacy-presentation | Legacy Core sprite Background | runtime baseline required |
| `core:sprite/bar-big/image` | legacy-presentation | Legacy Core sprite barBig | runtime baseline required |
| `core:sprite/bar-small/image` | legacy-presentation | Legacy Core sprite barSmall | runtime baseline required |
| `core:sprite/bar/image` | legacy-presentation | Legacy Core sprite Bar | runtime baseline required |
| `core:sprite/barrel-0/image` | legacy-presentation | Legacy Core sprite barrel_0 | runtime baseline required |
| `core:sprite/barrel-1/image` | legacy-presentation | Legacy Core sprite barrel_1 | runtime baseline required |
| `core:sprite/barrel-pusher/image` | legacy-presentation | Legacy Core sprite barrelPusher | runtime baseline required |
| `core:sprite/barrel/image` | legacy-presentation | Legacy Core sprite barrel | runtime baseline required |
| `core:sprite/base-0/image` | legacy-presentation | Legacy Core sprite base_0 | runtime baseline required |
| `core:sprite/base-1/image` | legacy-presentation | Legacy Core sprite base_1 | runtime baseline required |
| `core:sprite/base-10/image` | legacy-presentation | Legacy Core sprite base_10 | runtime baseline required |
| `core:sprite/base-11/image` | legacy-presentation | Legacy Core sprite base_11 | runtime baseline required |
| `core:sprite/base-12/image` | legacy-presentation | Legacy Core sprite base_12 | runtime baseline required |
| `core:sprite/base-13/image` | legacy-presentation | Legacy Core sprite base_13 | runtime baseline required |
| `core:sprite/base-14/image` | legacy-presentation | Legacy Core sprite base_14 | runtime baseline required |
| `core:sprite/base-15/image` | legacy-presentation | Legacy Core sprite base_15 | runtime baseline required |
| `core:sprite/base-16/image` | legacy-presentation | Legacy Core sprite base_16 | runtime baseline required |
| `core:sprite/base-17/image` | legacy-presentation | Legacy Core sprite base_17 | runtime baseline required |
| `core:sprite/base-18/image` | legacy-presentation | Legacy Core sprite base_18 | runtime baseline required |
| `core:sprite/base-19/image` | legacy-presentation | Legacy Core sprite base_19 | runtime baseline required |
| `core:sprite/base-2/image` | legacy-presentation | Legacy Core sprite base_2 | runtime baseline required |
| `core:sprite/base-20/image` | legacy-presentation | Legacy Core sprite base_20 | runtime baseline required |
| `core:sprite/base-21/image` | legacy-presentation | Legacy Core sprite base_21 | runtime baseline required |
| `core:sprite/base-3/image` | legacy-presentation | Legacy Core sprite base_3 | runtime baseline required |
| `core:sprite/base-4/image` | legacy-presentation | Legacy Core sprite base_4 | runtime baseline required |
| `core:sprite/base-5/image` | legacy-presentation | Legacy Core sprite base_5 | runtime baseline required |
| `core:sprite/base-6/image` | legacy-presentation | Legacy Core sprite base_6 | runtime baseline required |
| `core:sprite/base-7/image` | legacy-presentation | Legacy Core sprite base_7 | runtime baseline required |
| `core:sprite/base-8/image` | legacy-presentation | Legacy Core sprite base_8 | runtime baseline required |
| `core:sprite/base-9/image` | legacy-presentation | Legacy Core sprite base_9 | runtime baseline required |
| `core:sprite/base/image` | legacy-presentation | Legacy Core sprite base | runtime baseline required |
| `core:sprite/battery/image` | legacy-presentation | Legacy Core sprite battery | runtime baseline required |
| `core:sprite/bay-bg/image` | legacy-presentation | Legacy Core sprite BayBg | runtime baseline required |
| `core:sprite/bay-wall-left/image` | legacy-presentation | Legacy Core sprite BayWallLeft | runtime baseline required |
| `core:sprite/beam-0/image` | legacy-presentation | Legacy Core sprite Beam_0 | runtime baseline required |
| `core:sprite/beam/image` | legacy-presentation | Legacy Core sprite Beam | runtime baseline required |
| `core:sprite/bed-0/image` | legacy-presentation | Legacy Core sprite Bed_0 | runtime baseline required |
| `core:sprite/bed/image` | legacy-presentation | Legacy Core sprite Bed | runtime baseline required |
| `core:sprite/bg-0/image` | legacy-presentation | Legacy Core sprite bg_0 | runtime baseline required |
| `core:sprite/bg-1/image` | legacy-presentation | Legacy Core sprite bg_1 | runtime baseline required |
| `core:sprite/bg-2/image` | legacy-presentation | Legacy Core sprite bg_2 | runtime baseline required |
| `core:sprite/bg-3/image` | legacy-presentation | Legacy Core sprite bg_3 | runtime baseline required |
| `core:sprite/bg-4/image` | legacy-presentation | Legacy Core sprite bg_4 | runtime baseline required |
| `core:sprite/bg-5/image` | legacy-presentation | Legacy Core sprite bg_5 | runtime baseline required |
| `core:sprite/bg-basement/image` | legacy-presentation | Legacy Core sprite bgBasement | runtime baseline required |
| `core:sprite/bg-blue/image` | legacy-presentation | Legacy Core sprite bgBlue | runtime baseline required |
| `core:sprite/bg-corridor/image` | legacy-presentation | Legacy Core sprite bgCorridor | runtime baseline required |
| `core:sprite/bg-green-animated1/image` | legacy-presentation | Legacy Core sprite BgGreenAnimated1 | runtime baseline required |
| `core:sprite/bg-green-animated10/image` | legacy-presentation | Legacy Core sprite BgGreenAnimated10 | runtime baseline required |
| `core:sprite/bg-green-animated11/image` | legacy-presentation | Legacy Core sprite BgGreenAnimated11 | runtime baseline required |
| `core:sprite/bg-green-animated12/image` | legacy-presentation | Legacy Core sprite BgGreenAnimated12 | runtime baseline required |
| `core:sprite/bg-green-animated13/image` | legacy-presentation | Legacy Core sprite BgGreenAnimated13 | runtime baseline required |
| `core:sprite/bg-green-animated14/image` | legacy-presentation | Legacy Core sprite BgGreenAnimated14 | runtime baseline required |
| `core:sprite/bg-green-animated15/image` | legacy-presentation | Legacy Core sprite BgGreenAnimated15 | runtime baseline required |
| `core:sprite/bg-green-animated16/image` | legacy-presentation | Legacy Core sprite BgGreenAnimated16 | runtime baseline required |
| `core:sprite/bg-green-animated17/image` | legacy-presentation | Legacy Core sprite BgGreenAnimated17 | runtime baseline required |
| `core:sprite/bg-green-animated2/image` | legacy-presentation | Legacy Core sprite BgGreenAnimated2 | runtime baseline required |
| `core:sprite/bg-green-animated3/image` | legacy-presentation | Legacy Core sprite BgGreenAnimated3 | runtime baseline required |
| `core:sprite/bg-green-animated4/image` | legacy-presentation | Legacy Core sprite BgGreenAnimated4 | runtime baseline required |
| `core:sprite/bg-green-animated5/image` | legacy-presentation | Legacy Core sprite BgGreenAnimated5 | runtime baseline required |
| `core:sprite/bg-green-animated6/image` | legacy-presentation | Legacy Core sprite BgGreenAnimated6 | runtime baseline required |
| `core:sprite/bg-green-animated7/image` | legacy-presentation | Legacy Core sprite BgGreenAnimated7 | runtime baseline required |
| `core:sprite/bg-green-animated8/image` | legacy-presentation | Legacy Core sprite BgGreenAnimated8 | runtime baseline required |
| `core:sprite/bg-green-animated9/image` | legacy-presentation | Legacy Core sprite BgGreenAnimated9 | runtime baseline required |
| `core:sprite/bg-ground/image` | legacy-presentation | Legacy Core sprite bgGround | runtime baseline required |
| `core:sprite/bg-lab/image` | legacy-presentation | Legacy Core sprite bgLab | runtime baseline required |
| `core:sprite/bg-office/image` | legacy-presentation | Legacy Core sprite bgOffice | runtime baseline required |
| `core:sprite/bg-purple/image` | legacy-presentation | Legacy Core sprite bgPurple | runtime baseline required |
| `core:sprite/bg-trees/image` | legacy-presentation | Legacy Core sprite BgTrees | runtime baseline required |
| `core:sprite/bg/image` | legacy-presentation | Legacy Core sprite bg | runtime baseline required |
| `core:sprite/bg1/image` | legacy-presentation | Legacy Core sprite bg1 | runtime baseline required |
| `core:sprite/blood-big1/image` | legacy-presentation | Legacy Core sprite BloodBig1 | runtime baseline required |
| `core:sprite/blood-big2/image` | legacy-presentation | Legacy Core sprite BloodBig2 | runtime baseline required |
| `core:sprite/blood-platform/image` | legacy-presentation | Legacy Core sprite BloodPlatform | runtime baseline required |
| `core:sprite/blood-small1/image` | legacy-presentation | Legacy Core sprite bloodSmall1 | runtime baseline required |
| `core:sprite/blood-small2/image` | legacy-presentation | Legacy Core sprite bloodSmall2 | runtime baseline required |
| `core:sprite/blood-small3/image` | legacy-presentation | Legacy Core sprite bloodSmall3 | runtime baseline required |
| `core:sprite/blood-small4/image` | legacy-presentation | Legacy Core sprite bloodSmall4 | runtime baseline required |
| `core:sprite/blood-splashes-wall1/image` | legacy-presentation | Legacy Core sprite BloodSplashesWall1 | runtime baseline required |
| `core:sprite/blood-splashes-wall2/image` | legacy-presentation | Legacy Core sprite BloodSplashesWall2 | runtime baseline required |
| `core:sprite/blood-splashes-wall3/image` | legacy-presentation | Legacy Core sprite BloodSplashesWall3 | runtime baseline required |
| `core:sprite/blueprint/image` | legacy-presentation | Legacy Core sprite blueprint | runtime baseline required |
| `core:sprite/blush-0/image` | legacy-presentation | Legacy Core sprite Blush_0 | runtime baseline required |
| `core:sprite/blush/image` | legacy-presentation | Legacy Core sprite Blush | runtime baseline required |
| `core:sprite/brain-0/image` | legacy-presentation | Legacy Core sprite brain_0 | runtime baseline required |
| `core:sprite/brain-bottle-wood/image` | legacy-presentation | Legacy Core sprite brainBottleWood | runtime baseline required |
| `core:sprite/brain-bottle/image` | legacy-presentation | Legacy Core sprite brainBottle | runtime baseline required |
| `core:sprite/brain-glass-overlay/image` | legacy-presentation | Legacy Core sprite brainGlassOverlay | runtime baseline required |
| `core:sprite/brain-machine-base/image` | legacy-presentation | Legacy Core sprite brainMachineBase | runtime baseline required |
| `core:sprite/breast-1/image` | legacy-presentation | Legacy Core sprite Breast_1 | runtime baseline required |
| `core:sprite/breast-2/image` | legacy-presentation | Legacy Core sprite Breast_2 | runtime baseline required |
| `core:sprite/breast-3/image` | legacy-presentation | Legacy Core sprite Breast_3 | runtime baseline required |
| `core:sprite/breast-4/image` | legacy-presentation | Legacy Core sprite Breast_4 | runtime baseline required |
| `core:sprite/breast-clean/image` | legacy-presentation | Legacy Core sprite BreastClean | runtime baseline required |
| `core:sprite/breast/image` | legacy-presentation | Legacy Core sprite Breast | runtime baseline required |
| `core:sprite/brow-0/image` | legacy-presentation | Legacy Core sprite Brow_0 | runtime baseline required |
| `core:sprite/btn-keypad-idle/image` | legacy-presentation | Legacy Core sprite BtnKeypadIdle | runtime baseline required |
| `core:sprite/btn-keypad-pressed/image` | legacy-presentation | Legacy Core sprite BtnKeypadPressed | runtime baseline required |
| `core:sprite/btn-menu-exit/image` | legacy-presentation | Legacy Core sprite BtnMenuExit | runtime baseline required |
| `core:sprite/btn-menu-top-bar-open/image` | legacy-presentation | Legacy Core sprite BtnMenuTopBarOpen | runtime baseline required |
| `core:sprite/btn-menu/image` | legacy-presentation | Legacy Core sprite BtnMenu | runtime baseline required |
| `core:sprite/btn-pressed/image` | legacy-presentation | Legacy Core sprite BtnPressed | runtime baseline required |
| `core:sprite/bubbles-mask/image` | legacy-presentation | Legacy Core sprite bubblesMask | runtime baseline required |
| `core:sprite/bubbles/image` | legacy-presentation | Legacy Core sprite bubbles | runtime baseline required |
| `core:sprite/buffout/image` | legacy-presentation | Legacy Core sprite buffout | runtime baseline required |
| `core:sprite/bullet-0/image` | legacy-presentation | Legacy Core sprite bullet_0 | runtime baseline required |
| `core:sprite/bullet-1/image` | legacy-presentation | Legacy Core sprite bullet_1 | runtime baseline required |
| `core:sprite/bullet-2/image` | legacy-presentation | Legacy Core sprite bullet_2 | runtime baseline required |
| `core:sprite/bullet-3/image` | legacy-presentation | Legacy Core sprite bullet_3 | runtime baseline required |
| `core:sprite/bullet-4/image` | legacy-presentation | Legacy Core sprite bullet_4 | runtime baseline required |
| `core:sprite/bullet-5/image` | legacy-presentation | Legacy Core sprite bullet_5 | runtime baseline required |
| `core:sprite/bullet-6/image` | legacy-presentation | Legacy Core sprite bullet_6 | runtime baseline required |
| `core:sprite/bullet-7/image` | legacy-presentation | Legacy Core sprite bullet_7 | runtime baseline required |
| `core:sprite/bullet/image` | legacy-presentation | Legacy Core sprite bullet | runtime baseline required |
| `core:sprite/butt-11/image` | legacy-presentation | Legacy Core sprite Butt_11 | runtime baseline required |
| `core:sprite/butt-12/image` | legacy-presentation | Legacy Core sprite Butt_12 | runtime baseline required |
| `core:sprite/butt-15/image` | legacy-presentation | Legacy Core sprite Butt_15 | runtime baseline required |
| `core:sprite/butt-2/image` | named-core-sprite | Core sprite Butt_2 | runtime baseline required |
| `core:sprite/butt-5/image` | named-core-sprite | Core sprite Butt_5 | runtime baseline required |
| `core:sprite/butt-6/image` | named-core-sprite | Core sprite Butt_6 | runtime baseline required |
| `core:sprite/butt-7/image` | legacy-presentation | Legacy Core sprite Butt_7 | runtime baseline required |
| `core:sprite/butt-8/image` | legacy-presentation | Legacy Core sprite Butt_8 | runtime baseline required |
| `core:sprite/butt-9/image` | legacy-presentation | Legacy Core sprite Butt_9 | runtime baseline required |
| `core:sprite/butt/image` | named-core-sprite | Core sprite Butt | runtime baseline required |
| `core:sprite/c-s446/image` | legacy-presentation | Legacy Core sprite CS446 | runtime baseline required |
| `core:sprite/camp-fire/image` | legacy-presentation | Legacy Core sprite CampFire | runtime baseline required |
| `core:sprite/case1/image` | legacy-presentation | Legacy Core sprite Case1 | runtime baseline required |
| `core:sprite/chair-0/image` | legacy-presentation | Legacy Core sprite Chair_0 | runtime baseline required |
| `core:sprite/chair-1/image` | legacy-presentation | Legacy Core sprite Chair_1 | runtime baseline required |
| `core:sprite/chair-2/image` | legacy-presentation | Legacy Core sprite Chair_2 | runtime baseline required |
| `core:sprite/chair/image` | legacy-presentation | Legacy Core sprite Chair | runtime baseline required |
| `core:sprite/challenge-item-hud-border/image` | legacy-presentation | Legacy Core sprite ChallengeItemHudBorder | runtime baseline required |
| `core:sprite/chamber-0/image` | legacy-presentation | Legacy Core sprite chamber_0 | runtime baseline required |
| `core:sprite/chamber/image` | legacy-presentation | Legacy Core sprite chamber | runtime baseline required |
| `core:sprite/checkmark/image` | legacy-presentation | Legacy Core sprite Checkmark | runtime baseline required |
| `core:sprite/chest-11/image` | legacy-presentation | Legacy Core sprite Chest_11 | runtime baseline required |
| `core:sprite/chest-12/image` | legacy-presentation | Legacy Core sprite Chest_12 | runtime baseline required |
| `core:sprite/chest-15/image` | legacy-presentation | Legacy Core sprite Chest_15 | runtime baseline required |
| `core:sprite/chest-16/image` | legacy-presentation | Legacy Core sprite Chest_16 | runtime baseline required |
| `core:sprite/chest-4/image` | legacy-presentation | Legacy Core sprite Chest_4 | runtime baseline required |
| `core:sprite/chest-5/image` | named-core-sprite | Core sprite Chest_5 | runtime baseline required |
| `core:sprite/chest-7/image` | named-core-sprite | Core sprite Chest_7 | runtime baseline required |
| `core:sprite/chest-clean/image` | legacy-presentation | Legacy Core sprite ChestClean | runtime baseline required |
| `core:sprite/circle-saw/image` | legacy-presentation | Legacy Core sprite circleSaw | runtime baseline required |
| `core:sprite/door-bg-0/image` | legacy-presentation | Legacy Core sprite doorBg_0 | runtime baseline required |
| `core:sprite/door-bg/image` | legacy-presentation | Legacy Core sprite doorBg | runtime baseline required |
| `core:sprite/door-closed-0/image` | legacy-presentation | Legacy Core sprite DoorClosed_0 | runtime baseline required |
| `core:sprite/door-closed-left/image` | legacy-presentation | Legacy Core sprite DoorClosedLeft | runtime baseline required |
| `core:sprite/door-closed-right/image` | legacy-presentation | Legacy Core sprite DoorClosedRight | runtime baseline required |
| `core:sprite/door-closed/image` | legacy-presentation | Legacy Core sprite doorClosed | runtime baseline required |
| `core:sprite/door-floor/image` | legacy-presentation | Legacy Core sprite DoorFloor | runtime baseline required |
| `core:sprite/door-hole/image` | legacy-presentation | Legacy Core sprite doorHole | runtime baseline required |
| `core:sprite/door-office/image` | legacy-presentation | Legacy Core sprite DoorOffice | runtime baseline required |
| `core:sprite/door-open-left/image` | legacy-presentation | Legacy Core sprite DoorOpenLeft | runtime baseline required |
| `core:sprite/door-open-right-0/image` | legacy-presentation | Legacy Core sprite DoorOpenRight_0 | runtime baseline required |
| `core:sprite/door-open-right/image` | legacy-presentation | Legacy Core sprite DoorOpenRight | runtime baseline required |
| `core:sprite/door-open/image` | legacy-presentation | Legacy Core sprite doorOpen | runtime baseline required |
| `core:sprite/door-rollable/image` | legacy-presentation | Legacy Core sprite DoorRollable | runtime baseline required |
| `core:sprite/door-roller/image` | legacy-presentation | Legacy Core sprite DoorRoller | runtime baseline required |
| `core:sprite/door-visitor-closed-left/image` | legacy-presentation | Legacy Core sprite DoorVisitorClosedLeft | runtime baseline required |
| `core:sprite/door-visitor-closed-right/image` | legacy-presentation | Legacy Core sprite DoorVisitorClosedRight | runtime baseline required |
| `core:sprite/door-visitor-open-left/image` | legacy-presentation | Legacy Core sprite DoorVisitorOpenLeft | runtime baseline required |
| `core:sprite/door-visitor-open-right/image` | legacy-presentation | Legacy Core sprite DoorVisitorOpenRight | runtime baseline required |
| `core:sprite/door1/image` | legacy-presentation | Legacy Core sprite Door1 | runtime baseline required |
| `core:sprite/door2/image` | legacy-presentation | Legacy Core sprite Door2 | runtime baseline required |
| `core:sprite/double-trigger/image` | legacy-presentation | Legacy Core sprite DoubleTrigger | runtime baseline required |
| `core:sprite/drawing-jenny/image` | legacy-presentation | Legacy Core sprite DrawingJenny | runtime baseline required |
| `core:sprite/drawing-sunny/image` | legacy-presentation | Legacy Core sprite DrawingSunny | runtime baseline required |
| `core:sprite/drop-down-bar/image` | legacy-presentation | Legacy Core sprite DropDownBar | runtime baseline required |
| `core:sprite/drop-down-check-mark/image` | legacy-presentation | Legacy Core sprite DropDownCheckMark | runtime baseline required |
| `core:sprite/dropdown-arrow/image` | legacy-presentation | Legacy Core sprite DropdownArrow | runtime baseline required |
| `core:sprite/ear-broken/image` | legacy-presentation | Legacy Core sprite EarBroken | runtime baseline required |
| `core:sprite/ear/image` | legacy-presentation | Legacy Core sprite Ear | runtime baseline required |
| `core:sprite/eye-white-0/image` | legacy-presentation | Legacy Core sprite EyeWhite_0 | runtime baseline required |
| `core:sprite/eyelid-upper-mask-0/image` | legacy-presentation | Legacy Core sprite EyelidUpperMask_0 | runtime baseline required |
| `core:sprite/eyelid-upper-mask/image` | legacy-presentation | Legacy Core sprite EyelidUpperMask | runtime baseline required |
| `core:sprite/eyelid-upper/image` | legacy-presentation | Legacy Core sprite EyelidUpper | runtime baseline required |
| `core:sprite/eyelid/image` | legacy-presentation | Legacy Core sprite Eyelid | runtime baseline required |
| `core:sprite/eyewhite/image` | legacy-presentation | Legacy Core sprite Eyewhite | runtime baseline required |
| `core:sprite/face/image` | legacy-presentation | Legacy Core sprite face | runtime baseline required |
| `core:sprite/fence-0/image` | legacy-presentation | Legacy Core sprite fence_0 | runtime baseline required |
| `core:sprite/fence-end/image` | legacy-presentation | Legacy Core sprite fenceEnd | runtime baseline required |
| `core:sprite/fence/image` | legacy-presentation | Legacy Core sprite Fence | runtime baseline required |
| `core:sprite/fetus-hud-fill-mask/image` | legacy-presentation | Legacy Core sprite FetusHudFillMask | runtime baseline required |
| `core:sprite/fetus-hud-fill/image` | legacy-presentation | Legacy Core sprite FetusHudFill | runtime baseline required |
| `core:sprite/flash-light/image` | legacy-presentation | Legacy Core sprite FlashLight | runtime baseline required |
| `core:sprite/floor-0/image` | legacy-presentation | Legacy Core sprite floor_0 | runtime baseline required |
| `core:sprite/floor-1/image` | legacy-presentation | Legacy Core sprite floor_1 | runtime baseline required |
| `core:sprite/floor-2/image` | legacy-presentation | Legacy Core sprite floor_2 | runtime baseline required |
| `core:sprite/floor-3/image` | legacy-presentation | Legacy Core sprite floor_3 | runtime baseline required |
| `core:sprite/floor-4/image` | legacy-presentation | Legacy Core sprite floor_4 | runtime baseline required |
| `core:sprite/floor-bend/image` | legacy-presentation | Legacy Core sprite floorBend | runtime baseline required |
| `core:sprite/floor-bg/image` | legacy-presentation | Legacy Core sprite floorBg | runtime baseline required |
| `core:sprite/floor-climable/image` | legacy-presentation | Legacy Core sprite floorClimable | runtime baseline required |
| `core:sprite/floor-climbable/image` | legacy-presentation | Legacy Core sprite floorClimbable | runtime baseline required |
| `core:sprite/floor/image` | legacy-presentation | Legacy Core sprite floor | runtime baseline required |
| `core:sprite/floor1/image` | legacy-presentation | Legacy Core sprite floor1 | runtime baseline required |
| `core:sprite/floorbg-0/image` | legacy-presentation | Legacy Core sprite floorbg_0 | runtime baseline required |
| `core:sprite/floordouble/image` | legacy-presentation | Legacy Core sprite floordouble | runtime baseline required |
| `core:sprite/floorlower/image` | legacy-presentation | Legacy Core sprite floorlower | runtime baseline required |
| `core:sprite/foot-base-0/image` | legacy-presentation | Legacy Core sprite FootBase_0 | runtime baseline required |
| `core:sprite/foot-base-1/image` | legacy-presentation | Legacy Core sprite FootBase_1 | runtime baseline required |
| `core:sprite/foot-base-2/image` | legacy-presentation | Legacy Core sprite FootBase_2 | runtime baseline required |
| `core:sprite/foot-base-3/image` | legacy-presentation | Legacy Core sprite FootBase_3 | runtime baseline required |
| `core:sprite/foot-base/image` | legacy-presentation | Legacy Core sprite FootBase | runtime baseline required |
| `core:sprite/foot-bloody/image` | legacy-presentation | Legacy Core sprite FootBloody | runtime baseline required |
| `core:sprite/foot-end-0/image` | legacy-presentation | Legacy Core sprite FootEnd_0 | runtime baseline required |
| `core:sprite/foot-end-1/image` | legacy-presentation | Legacy Core sprite FootEnd_1 | runtime baseline required |
| `core:sprite/foot-end-2/image` | legacy-presentation | Legacy Core sprite FootEnd_2 | runtime baseline required |
| `core:sprite/foot-end-3/image` | legacy-presentation | Legacy Core sprite FootEnd_3 | runtime baseline required |
| `core:sprite/foot-end/image` | legacy-presentation | Legacy Core sprite FootEnd | runtime baseline required |
| `core:sprite/foot/image` | legacy-presentation | Legacy Core sprite Foot | runtime baseline required |
| `core:sprite/generator/image` | legacy-presentation | Legacy Core sprite Generator | runtime baseline required |
| `core:sprite/ground-seperator/image` | legacy-presentation | Legacy Core sprite groundSeperator | runtime baseline required |
| `core:sprite/ground/image` | legacy-presentation | Legacy Core sprite ground | runtime baseline required |
| `core:sprite/gun-rig/image` | legacy-presentation | Legacy Core sprite gunRig | runtime baseline required |
| `core:sprite/halman/image` | legacy-presentation | Legacy Core sprite halman | runtime baseline required |
| `core:sprite/hand-16/image` | legacy-presentation | Legacy Core sprite Hand_16 | runtime baseline required |
| `core:sprite/hand-18/image` | legacy-presentation | Legacy Core sprite Hand_18 | runtime baseline required |
| `core:sprite/hand-19/image` | legacy-presentation | Legacy Core sprite Hand_19 | runtime baseline required |
| `core:sprite/hand-2/image` | legacy-presentation | Legacy Core sprite Hand_2 | runtime baseline required |
| `core:sprite/hand-4/image` | legacy-presentation | Legacy Core sprite Hand_4 | runtime baseline required |
| `core:sprite/hand-8/image` | legacy-presentation | Legacy Core sprite Hand_8 | runtime baseline required |
| `core:sprite/hand-9/image` | legacy-presentation | Legacy Core sprite Hand_9 | runtime baseline required |
| `core:sprite/hand-broken/image` | legacy-presentation | Legacy Core sprite HandBroken | runtime baseline required |
| `core:sprite/harrington/image` | legacy-presentation | Legacy Core sprite harrington | runtime baseline required |
| `core:sprite/head-11/image` | legacy-presentation | Legacy Core sprite Head_11 | runtime baseline required |
| `core:sprite/head-16/image` | legacy-presentation | Legacy Core sprite Head_16 | runtime baseline required |
| `core:sprite/head-17/image` | legacy-presentation | Legacy Core sprite Head_17 | runtime baseline required |
| `core:sprite/head-18/image` | legacy-presentation | Legacy Core sprite Head_18 | runtime baseline required |
| `core:sprite/head-19/image` | legacy-presentation | Legacy Core sprite Head_19 | runtime baseline required |
| `core:sprite/head-8/image` | legacy-presentation | Legacy Core sprite Head_8 | runtime baseline required |
| `core:sprite/head-fear/image` | legacy-presentation | Legacy Core sprite HeadFear | runtime baseline required |
| `core:sprite/head-turned-off-0/image` | legacy-presentation | Legacy Core sprite HeadTurnedOff_0 | runtime baseline required |
| `core:sprite/head-turned-off/image` | legacy-presentation | Legacy Core sprite HeadTurnedOff | runtime baseline required |
| `core:sprite/head-w-i-p/image` | legacy-presentation | Legacy Core sprite HeadWIP | runtime baseline required |
| `core:sprite/head1/image` | legacy-presentation | Legacy Core sprite Head1 | runtime baseline required |
| `core:sprite/head2/image` | legacy-presentation | Legacy Core sprite Head2 | runtime baseline required |
| `core:sprite/head3/image` | legacy-presentation | Legacy Core sprite Head3 | runtime baseline required |
| `core:sprite/head4/image` | legacy-presentation | Legacy Core sprite Head4 | runtime baseline required |
| `core:sprite/heart-dead/image` | legacy-presentation | Legacy Core sprite HeartDead | runtime baseline required |
| `core:sprite/heart-healthy/image` | legacy-presentation | Legacy Core sprite HeartHealthy | runtime baseline required |
| `core:sprite/hip-3/image` | legacy-presentation | Legacy Core sprite Hip_3 | runtime baseline required |
| `core:sprite/hip-5/image` | legacy-presentation | Legacy Core sprite Hip_5 | runtime baseline required |
| `core:sprite/hips-1/image` | legacy-presentation | Legacy Core sprite Hips_1 | runtime baseline required |
| `core:sprite/hips-7/image` | legacy-presentation | Legacy Core sprite Hips_7 | runtime baseline required |
| `core:sprite/hut-bg/image` | legacy-presentation | Legacy Core sprite HutBg | runtime baseline required |
| `core:sprite/hut-door/image` | legacy-presentation | Legacy Core sprite HutDoor | runtime baseline required |
| `core:sprite/icon-0/image` | legacy-presentation | Legacy Core sprite Icon_0 | runtime baseline required |
| `core:sprite/icon-6/image` | legacy-presentation | Legacy Core sprite Icon_6 | runtime baseline required |
| `core:sprite/icon-7/image` | legacy-presentation | Legacy Core sprite Icon_7 | runtime baseline required |
| `core:sprite/icon-9/image` | legacy-presentation | Legacy Core sprite Icon_9 | runtime baseline required |
| `core:sprite/jacky-door-light/image` | legacy-presentation | Legacy Core sprite JackyDoorLight | runtime baseline required |
| `core:sprite/keypad/image` | legacy-presentation | Legacy Core sprite Keypad | runtime baseline required |
| `core:sprite/knife/image` | legacy-presentation | Legacy Core sprite knife | runtime baseline required |
| `core:sprite/lab/image` | legacy-presentation | Legacy Core sprite Lab | runtime baseline required |
| `core:sprite/lady-statue/image` | legacy-presentation | Legacy Core sprite LadyStatue | runtime baseline required |
| `core:sprite/leg-lower-10/image` | legacy-presentation | Legacy Core sprite LegLower_10 | runtime baseline required |
| `core:sprite/leg-lower-11/image` | legacy-presentation | Legacy Core sprite LegLower_11 | runtime baseline required |
| `core:sprite/leg-lower-13/image` | legacy-presentation | Legacy Core sprite LegLower_13 | runtime baseline required |
| `core:sprite/leg-lower-15/image` | legacy-presentation | Legacy Core sprite LegLower_15 | runtime baseline required |
| `core:sprite/leg-lower-3/image` | legacy-presentation | Legacy Core sprite LegLower_3 | runtime baseline required |
| `core:sprite/leg-lower-4/image` | legacy-presentation | Legacy Core sprite LegLower_4 | runtime baseline required |
| `core:sprite/leg-lower-8/image` | legacy-presentation | Legacy Core sprite LegLower_8 | runtime baseline required |
| `core:sprite/leg-lower-bloody-0/image` | legacy-presentation | Legacy Core sprite LegLowerBloody_0 | runtime baseline required |
| `core:sprite/leg-lower-bloody/image` | legacy-presentation | Legacy Core sprite LegLowerBloody | runtime baseline required |
| `core:sprite/leg-lower-broken/image` | legacy-presentation | Legacy Core sprite LegLowerBroken | runtime baseline required |
| `core:sprite/leg-upper-1/image` | legacy-presentation | Legacy Core sprite LegUpper_1 | runtime baseline required |
| `core:sprite/leg-upper-12/image` | legacy-presentation | Legacy Core sprite LegUpper_12 | runtime baseline required |
| `core:sprite/leg-upper-17/image` | legacy-presentation | Legacy Core sprite LegUpper_17 | runtime baseline required |
| `core:sprite/leg-upper-3/image` | legacy-presentation | Legacy Core sprite LegUpper_3 | runtime baseline required |
| `core:sprite/leg-upper-4/image` | legacy-presentation | Legacy Core sprite LegUpper_4 | runtime baseline required |
| `core:sprite/leg-upper-5/image` | legacy-presentation | Legacy Core sprite LegUpper_5 | runtime baseline required |
| `core:sprite/leg-upper-6/image` | legacy-presentation | Legacy Core sprite LegUpper_6 | runtime baseline required |
| `core:sprite/leg-upper-bloody/image` | legacy-presentation | Legacy Core sprite LegUpperBloody | runtime baseline required |
| `core:sprite/libido-heart/image` | legacy-presentation | Legacy Core sprite LibidoHeart | runtime baseline required |
| `core:sprite/light-switch-off/image` | legacy-presentation | Legacy Core sprite lightSwitchOff | runtime baseline required |
| `core:sprite/lk235/image` | legacy-presentation | Legacy Core sprite lk235 | runtime baseline required |
| `core:sprite/m4-b1/image` | legacy-presentation | Legacy Core sprite M4B1 | runtime baseline required |
| `core:sprite/magazine-0/image` | legacy-presentation | Legacy Core sprite magazine_0 | runtime baseline required |
| `core:sprite/magazine-1/image` | legacy-presentation | Legacy Core sprite magazine_1 | runtime baseline required |
| `core:sprite/magazine-10/image` | legacy-presentation | Legacy Core sprite magazine_10 | runtime baseline required |
| `core:sprite/magazine-11/image` | legacy-presentation | Legacy Core sprite magazine_11 | runtime baseline required |
| `core:sprite/magazine-12/image` | legacy-presentation | Legacy Core sprite magazine_12 | runtime baseline required |
| `core:sprite/magazine-3/image` | legacy-presentation | Legacy Core sprite magazine_3 | runtime baseline required |
| `core:sprite/magazine-4/image` | legacy-presentation | Legacy Core sprite magazine_4 | runtime baseline required |
| `core:sprite/magazine-5/image` | legacy-presentation | Legacy Core sprite magazine_5 | runtime baseline required |
| `core:sprite/magazine-6/image` | legacy-presentation | Legacy Core sprite magazine_6 | runtime baseline required |
| `core:sprite/magazine-7/image` | legacy-presentation | Legacy Core sprite magazine_7 | runtime baseline required |
| `core:sprite/magazine-9/image` | legacy-presentation | Legacy Core sprite magazine_9 | runtime baseline required |
| `core:sprite/magazine/image` | legacy-presentation | Legacy Core sprite magazine | runtime baseline required |
| `core:sprite/medic-body/image` | legacy-presentation | Legacy Core sprite MedicBody | runtime baseline required |
| `core:sprite/mg32/image` | legacy-presentation | Legacy Core sprite mg32 | runtime baseline required |
| `core:sprite/morphine/image` | legacy-presentation | Legacy Core sprite Morphine | runtime baseline required |
| `core:sprite/mouth-oral1/image` | legacy-presentation | Legacy Core sprite MouthOral1 | runtime baseline required |
| `core:sprite/mouth-oral2/image` | legacy-presentation | Legacy Core sprite MouthOral2 | runtime baseline required |
| `core:sprite/mouth-oral3/image` | legacy-presentation | Legacy Core sprite MouthOral3 | runtime baseline required |
| `core:sprite/mouth-tired1/image` | legacy-presentation | Legacy Core sprite MouthTired1 | runtime baseline required |
| `core:sprite/mouth-tired2/image` | legacy-presentation | Legacy Core sprite MouthTired2 | runtime baseline required |
| `core:sprite/mouth10/image` | legacy-presentation | Legacy Core sprite Mouth10 | runtime baseline required |
| `core:sprite/mouth12/image` | legacy-presentation | Legacy Core sprite Mouth12 | runtime baseline required |
| `core:sprite/mouth2/image` | legacy-presentation | Legacy Core sprite Mouth2 | runtime baseline required |
| `core:sprite/mouth3/image` | legacy-presentation | Legacy Core sprite Mouth3 | runtime baseline required |
| `core:sprite/mouth4/image` | legacy-presentation | Legacy Core sprite Mouth4 | runtime baseline required |
| `core:sprite/mouth5/image` | legacy-presentation | Legacy Core sprite Mouth5 | runtime baseline required |
| `core:sprite/mouth6/image` | legacy-presentation | Legacy Core sprite Mouth6 | runtime baseline required |
| `core:sprite/mouth7/image` | legacy-presentation | Legacy Core sprite Mouth7 | runtime baseline required |
| `core:sprite/mouth8/image` | legacy-presentation | Legacy Core sprite Mouth8 | runtime baseline required |
| `core:sprite/mouth9/image` | legacy-presentation | Legacy Core sprite Mouth9 | runtime baseline required |
| `core:sprite/muger/image` | legacy-presentation | Legacy Core sprite muger | runtime baseline required |
| `core:sprite/muzzle-flash1/image` | legacy-presentation | Legacy Core sprite MuzzleFlash1 | runtime baseline required |
| `core:sprite/muzzle-flash2/image` | legacy-presentation | Legacy Core sprite MuzzleFlash2 | runtime baseline required |
| `core:sprite/muzzle-flash3/image` | legacy-presentation | Legacy Core sprite MuzzleFlash3 | runtime baseline required |
| `core:sprite/n-p40/image` | legacy-presentation | Legacy Core sprite NP40 | runtime baseline required |
| `core:sprite/neck-11/image` | legacy-presentation | Legacy Core sprite Neck_11 | runtime baseline required |
| `core:sprite/neck-4/image` | legacy-presentation | Legacy Core sprite Neck_4 | runtime baseline required |
| `core:sprite/neck-5/image` | legacy-presentation | Legacy Core sprite Neck_5 | runtime baseline required |
| `core:sprite/neck-8/image` | legacy-presentation | Legacy Core sprite Neck_8 | runtime baseline required |
| `core:sprite/no-entry/image` | legacy-presentation | Legacy Core sprite noEntry | runtime baseline required |
| `core:sprite/note-keycode/image` | legacy-presentation | Legacy Core sprite noteKeycode | runtime baseline required |
| `core:sprite/note-wall/image` | legacy-presentation | Legacy Core sprite noteWall | runtime baseline required |
| `core:sprite/np9/image` | legacy-presentation | Legacy Core sprite np9 | runtime baseline required |
| `core:sprite/overlay-hole/image` | legacy-presentation | Legacy Core sprite OverlayHole | runtime baseline required |
| `core:sprite/overlay/image` | legacy-presentation | Legacy Core sprite Overlay | runtime baseline required |
| `core:sprite/painting/image` | legacy-presentation | Legacy Core sprite painting | runtime baseline required |
| `core:sprite/penis-0/image` | legacy-presentation | Legacy Core sprite penis_0 | runtime baseline required |
| `core:sprite/penis-1/image` | legacy-presentation | Legacy Core sprite penis_1 | runtime baseline required |
| `core:sprite/penis-2/image` | legacy-presentation | Legacy Core sprite Penis_2 | runtime baseline required |
| `core:sprite/penis-3/image` | legacy-presentation | Legacy Core sprite Penis_3 | runtime baseline required |
| `core:sprite/penis-4/image` | legacy-presentation | Legacy Core sprite Penis_4 | runtime baseline required |
| `core:sprite/penis-5/image` | legacy-presentation | Legacy Core sprite penis_5 | runtime baseline required |
| `core:sprite/penis-6/image` | legacy-presentation | Legacy Core sprite Penis_6 | runtime baseline required |
| `core:sprite/penis-base-0/image` | legacy-presentation | Legacy Core sprite PenisBase_0 | runtime baseline required |
| `core:sprite/penis-base-1/image` | legacy-presentation | Legacy Core sprite PenisBase_1 | runtime baseline required |
| `core:sprite/penis-base-2/image` | legacy-presentation | Legacy Core sprite PenisBase_2 | runtime baseline required |
| `core:sprite/penis-base-3/image` | legacy-presentation | Legacy Core sprite PenisBase_3 | runtime baseline required |
| `core:sprite/penis-base-4/image` | legacy-presentation | Legacy Core sprite PenisBase_4 | runtime baseline required |
| `core:sprite/penis-base-5/image` | legacy-presentation | Legacy Core sprite PenisBase_5 | runtime baseline required |
| `core:sprite/penis-base-6/image` | legacy-presentation | Legacy Core sprite PenisBase_6 | runtime baseline required |
| `core:sprite/penis-base/image` | legacy-presentation | Legacy Core sprite PenisBase | runtime baseline required |
| `core:sprite/penis-end-0/image` | legacy-presentation | Legacy Core sprite PenisEnd_0 | runtime baseline required |
| `core:sprite/penis-end-1/image` | legacy-presentation | Legacy Core sprite PenisEnd_1 | runtime baseline required |
| `core:sprite/penis-end-2/image` | legacy-presentation | Legacy Core sprite PenisEnd_2 | runtime baseline required |
| `core:sprite/penis-end-3/image` | legacy-presentation | Legacy Core sprite PenisEnd_3 | runtime baseline required |
| `core:sprite/penis-end/image` | legacy-presentation | Legacy Core sprite PenisEnd | runtime baseline required |
| `core:sprite/penis-mid-0/image` | legacy-presentation | Legacy Core sprite PenisMid_0 | runtime baseline required |
| `core:sprite/penis-mid-1/image` | legacy-presentation | Legacy Core sprite PenisMid_1 | runtime baseline required |
| `core:sprite/penis-mid/image` | legacy-presentation | Legacy Core sprite PenisMid | runtime baseline required |
| `core:sprite/penis-middle/image` | legacy-presentation | Legacy Core sprite PenisMiddle | runtime baseline required |
| `core:sprite/penis-rod-0/image` | legacy-presentation | Legacy Core sprite PenisRod_0 | runtime baseline required |
| `core:sprite/penis-rod/image` | legacy-presentation | Legacy Core sprite PenisRod | runtime baseline required |
| `core:sprite/penis-tip-0/image` | legacy-presentation | Legacy Core sprite PenisTip_0 | runtime baseline required |
| `core:sprite/penis-tip-1/image` | legacy-presentation | Legacy Core sprite PenisTip_1 | runtime baseline required |
| `core:sprite/penis-tip-2/image` | legacy-presentation | Legacy Core sprite PenisTip_2 | runtime baseline required |
| `core:sprite/penis-tip/image` | legacy-presentation | Legacy Core sprite PenisTip | runtime baseline required |
| `core:sprite/penis/image` | legacy-presentation | Legacy Core sprite Penis | runtime baseline required |
| `core:sprite/pipe-fire/image` | legacy-presentation | Legacy Core sprite pipeFire | runtime baseline required |
| `core:sprite/pistol/image` | legacy-presentation | Legacy Core sprite pistol | runtime baseline required |
| `core:sprite/plate-of-food/image` | legacy-presentation | Legacy Core sprite plateOfFood | runtime baseline required |
| `core:sprite/platform-0/image` | legacy-presentation | Legacy Core sprite platform_0 | runtime baseline required |
| `core:sprite/platform-climable-left/image` | legacy-presentation | Legacy Core sprite platformClimableLeft | runtime baseline required |
| `core:sprite/platform-climable-right/image` | legacy-presentation | Legacy Core sprite platformClimableRight | runtime baseline required |
| `core:sprite/platform-climable/image` | legacy-presentation | Legacy Core sprite platformClimable | runtime baseline required |
| `core:sprite/platform/image` | legacy-presentation | Legacy Core sprite platform | runtime baseline required |
| `core:sprite/poster-wife-bedroom/image` | legacy-presentation | Legacy Core sprite PosterWifeBedroom | runtime baseline required |
| `core:sprite/poster/image` | legacy-presentation | Legacy Core sprite poster | runtime baseline required |
| `core:sprite/psycho/image` | legacy-presentation | Legacy Core sprite psycho | runtime baseline required |
| `core:sprite/pump/image` | legacy-presentation | Legacy Core sprite pump | runtime baseline required |
| `core:sprite/qlock17-a/image` | legacy-presentation | Legacy Core sprite Qlock17A | runtime baseline required |
| `core:sprite/qlock17/image` | legacy-presentation | Legacy Core sprite Qlock17 | runtime baseline required |
| `core:sprite/reload-light/image` | legacy-presentation | Legacy Core sprite reloadLight | runtime baseline required |
| `core:sprite/revolver/image` | legacy-presentation | Legacy Core sprite revolver | runtime baseline required |
| `core:sprite/ronigsberg/image` | legacy-presentation | Legacy Core sprite ronigsberg | runtime baseline required |
| `core:sprite/rontgen/image` | legacy-presentation | Legacy Core sprite rontgen | runtime baseline required |
| `core:sprite/s-i-i-d-21/image` | legacy-presentation | Legacy Core sprite SIID-21 | runtime baseline required |
| `core:sprite/s-s-p-h42/image` | legacy-presentation | Legacy Core sprite SSPH42 | runtime baseline required |
| `core:sprite/saw/image` | legacy-presentation | Legacy Core sprite saw | runtime baseline required |
| `core:sprite/sawed-off/image` | legacy-presentation | Legacy Core sprite SawedOff | runtime baseline required |
| `core:sprite/schockgewehr/image` | legacy-presentation | Legacy Core sprite schockgewehr | runtime baseline required |
| `core:sprite/shack-front-door-roof/image` | legacy-presentation | Legacy Core sprite shackFrontDoorRoof | runtime baseline required |
| `core:sprite/shell/image` | legacy-presentation | Legacy Core sprite shell | runtime baseline required |
| `core:sprite/shitty-table/image` | legacy-presentation | Legacy Core sprite shittyTable | runtime baseline required |
| `core:sprite/shoe-base/image` | legacy-presentation | Legacy Core sprite ShoeBase | runtime baseline required |
| `core:sprite/shoe-end/image` | legacy-presentation | Legacy Core sprite ShoeEnd | runtime baseline required |
| `core:sprite/skirt/image` | legacy-presentation | Legacy Core sprite Skirt | runtime baseline required |
| `core:sprite/sldefender/image` | legacy-presentation | Legacy Core sprite sldefender | runtime baseline required |
| `core:sprite/slide-0/image` | legacy-presentation | Legacy Core sprite slide_0 | runtime baseline required |
| `core:sprite/slide-1/image` | legacy-presentation | Legacy Core sprite slide_1 | runtime baseline required |
| `core:sprite/slide-2/image` | legacy-presentation | Legacy Core sprite slide_2 | runtime baseline required |
| `core:sprite/slide/image` | legacy-presentation | Legacy Core sprite slide | runtime baseline required |
| `core:sprite/smg/image` | legacy-presentation | Legacy Core sprite smg | runtime baseline required |
| `core:sprite/snub/image` | legacy-presentation | Legacy Core sprite snub | runtime baseline required |
| `core:sprite/spine-0/image` | legacy-presentation | Legacy Core sprite Spine_0 | runtime baseline required |
| `core:sprite/spine-2/image` | legacy-presentation | Legacy Core sprite Spine_2 | runtime baseline required |
| `core:sprite/spine-3/image` | legacy-presentation | Legacy Core sprite Spine_3 | runtime baseline required |
| `core:sprite/spine-5/image` | legacy-presentation | Legacy Core sprite Spine_5 | runtime baseline required |
| `core:sprite/spine-9/image` | legacy-presentation | Legacy Core sprite Spine_9 | runtime baseline required |
| `core:sprite/stage/image` | legacy-presentation | Legacy Core sprite Stage | runtime baseline required |
| `core:sprite/stamina-bar-danger/image` | legacy-presentation | Legacy Core sprite StaminaBarDanger | runtime baseline required |
| `core:sprite/stamina-bar-empty/image` | legacy-presentation | Legacy Core sprite StaminaBarEmpty | runtime baseline required |
| `core:sprite/stamina-bar-fill/image` | legacy-presentation | Legacy Core sprite StaminaBarFill | runtime baseline required |
| `core:sprite/status-player-hud-item/image` | legacy-presentation | Legacy Core sprite StatusPlayerHudItem | runtime baseline required |
| `core:sprite/surgery-bed-jacky/image` | legacy-presentation | Legacy Core sprite surgeryBedJacky | runtime baseline required |
| `core:sprite/surgery-bed/image` | legacy-presentation | Legacy Core sprite surgeryBed | runtime baseline required |
| `core:sprite/table-0/image` | legacy-presentation | Legacy Core sprite table_0 | runtime baseline required |
| `core:sprite/table-1/image` | legacy-presentation | Legacy Core sprite table_1 | runtime baseline required |
| `core:sprite/table-right/image` | legacy-presentation | Legacy Core sprite tableRight | runtime baseline required |
| `core:sprite/table-wall/image` | legacy-presentation | Legacy Core sprite tableWall | runtime baseline required |
| `core:sprite/table/image` | legacy-presentation | Legacy Core sprite Table | runtime baseline required |
| `core:sprite/table1/image` | legacy-presentation | Legacy Core sprite Table1 | runtime baseline required |
| `core:sprite/table2/image` | legacy-presentation | Legacy Core sprite Table2 | runtime baseline required |
| `core:sprite/tenelli/image` | legacy-presentation | Legacy Core sprite tenelli | runtime baseline required |
| `core:sprite/tenelliso3/image` | legacy-presentation | Legacy Core sprite tenelliso3 | runtime baseline required |
| `core:sprite/theatre-back-background/image` | legacy-presentation | Legacy Core sprite theatreBackBackground | runtime baseline required |
| `core:sprite/theatre-bg/image` | legacy-presentation | Legacy Core sprite TheatreBg | runtime baseline required |
| `core:sprite/theatre-seat/image` | legacy-presentation | Legacy Core sprite theatreSeat | runtime baseline required |
| `core:sprite/title-bg/image` | legacy-presentation | Legacy Core sprite titleBg | runtime baseline required |
| `core:sprite/torso-lower-0/image` | named-core-sprite | Core sprite TorsoLower_0 | runtime baseline required |
| `core:sprite/torso-lower-8/image` | legacy-presentation | Legacy Core sprite TorsoLower_8 | runtime baseline required |
| `core:sprite/tree-mid/image` | legacy-presentation | Legacy Core sprite treeMid | runtime baseline required |
| `core:sprite/tree-platform-0/image` | legacy-presentation | Legacy Core sprite treePlatform_0 | runtime baseline required |
| `core:sprite/tree-platform/image` | legacy-presentation | Legacy Core sprite treePlatform | runtime baseline required |
| `core:sprite/tree-root/image` | legacy-presentation | Legacy Core sprite treeRoot | runtime baseline required |
| `core:sprite/tv-chair/image` | legacy-presentation | Legacy Core sprite tvChair | runtime baseline required |
| `core:sprite/u-i-check-mark/image` | legacy-presentation | Legacy Core sprite UICheckMark | runtime baseline required |
| `core:sprite/upper-floor/image` | legacy-presentation | Legacy Core sprite upperFloor | runtime baseline required |
| `core:sprite/usable-vendor/image` | legacy-presentation | Legacy Core sprite UsableVendor | runtime baseline required |
| `core:sprite/vegetationfloor/image` | legacy-presentation | Legacy Core sprite vegetationfloor | runtime baseline required |
| `core:sprite/wall-0/image` | legacy-presentation | Legacy Core sprite wall_0 | runtime baseline required |
| `core:sprite/wall-1/image` | legacy-presentation | Legacy Core sprite Wall_1 | runtime baseline required |
| `core:sprite/wall-2/image` | legacy-presentation | Legacy Core sprite Wall_2 | runtime baseline required |
| `core:sprite/wall-3/image` | legacy-presentation | Legacy Core sprite Wall_3 | runtime baseline required |
| `core:sprite/wall-4/image` | legacy-presentation | Legacy Core sprite wall_4 | runtime baseline required |
| `core:sprite/wall-5/image` | legacy-presentation | Legacy Core sprite wall_5 | runtime baseline required |
| `core:sprite/wall-bottom-closed/image` | legacy-presentation | Legacy Core sprite WallBottomClosed | runtime baseline required |
| `core:sprite/wall-light/image` | legacy-presentation | Legacy Core sprite wallLight | runtime baseline required |
| `core:sprite/wall-pillar/image` | legacy-presentation | Legacy Core sprite WallPillar | runtime baseline required |
| `core:sprite/wall-scratches-medic-room/image` | legacy-presentation | Legacy Core sprite WallScratchesMedicRoom | runtime baseline required |
| `core:sprite/wall/image` | legacy-presentation | Legacy Core sprite wall | runtime baseline required |
| `core:sprite/wall2/image` | legacy-presentation | Legacy Core sprite Wall2 | runtime baseline required |
| `core:sprite/wardrobe/image` | legacy-presentation | Legacy Core sprite Wardrobe | runtime baseline required |
| `core:sprite/weapon-vendor/image` | legacy-presentation | Legacy Core sprite WeaponVendor | runtime baseline required |
| `core:sprite/whiteboard/image` | legacy-presentation | Legacy Core sprite whiteboard | runtime baseline required |
| `core:sprite/yard-grass-platform/image` | legacy-presentation | Legacy Core sprite YardGrassPlatform | runtime baseline required |
| `core:stage-art/fer/actor/butt-12` | named-core-sprite | Core sprite Butt_12 | runtime baseline required |
| `core:stage-art/fer/actor/chest-12` | named-core-sprite | Core sprite Chest_12 | runtime baseline required |
| `core:stage-art/fer/actor/chest-15` | named-core-sprite | Core sprite Chest_15 | runtime baseline required |
| `core:stage-art/fer/actor/hand-broken` | named-core-sprite | Core sprite HandBroken | runtime baseline required |
| `core:stage-art/fer/actor/head-11` | named-core-sprite | Core sprite Head_11 | runtime baseline required |
| `core:stage-art/fer/actor/head-17` | named-core-sprite | Core sprite Head_17 | runtime baseline required |
| `core:stage-art/fer/actor/head-18` | named-core-sprite | Core sprite Head_18 | runtime baseline required |
| `core:stage-art/fer/actor/head-turned-off` | named-core-sprite | Core sprite HeadTurnedOff | runtime baseline required |
| `core:stage-art/fer/actor/head-turned-off-alt` | named-core-sprite | Core sprite HeadTurnedOff_0 | runtime baseline required |
| `core:stage-art/fer/actor/leg-lower-bloody` | named-core-sprite | Core sprite LegLowerBloody_0 | runtime baseline required |
| `core:stage-art/fer/actor/leg-upper-17` | named-core-sprite | Core sprite LegUpper_17 | runtime baseline required |
| `core:stage-art/fer/actor/leg-upper-4` | named-core-sprite | Core sprite LegUpper_4 | runtime baseline required |
| `core:stage-art/fer/actor/leg-upper-6` | named-core-sprite | Core sprite LegUpper_6 | runtime baseline required |
| `core:stage-art/fer/actor/leg-upper-bloody` | named-core-sprite | Core sprite LegUpperBloody | runtime baseline required |
| `core:stage-art/fer/actor/spine-3` | named-core-sprite | Core sprite Spine_3 | runtime baseline required |
| `core:stage-art/fer/actor/spine-5` | named-core-sprite | Core sprite Spine_5 | runtime baseline required |
| `core:stage-art/fer/actor/spine-9` | named-core-sprite | Core sprite Spine_9 | runtime baseline required |
| `core:stage-art/fer/effects/blood-big-1` | named-core-sprite | Core sprite BloodBig1 | runtime baseline required |
| `core:stage-art/fer/effects/blood-big-2` | named-core-sprite | Core sprite BloodBig2 | runtime baseline required |
| `core:stage-art/fer/effects/blood-platform` | named-core-sprite | Core sprite BloodPlatform | runtime baseline required |
| `core:stage-art/fer/effects/blood-small-1` | named-core-sprite | Core sprite bloodSmall1 | runtime baseline required |
| `core:stage-art/fer/effects/blood-small-2` | named-core-sprite | Core sprite bloodSmall2 | runtime baseline required |
| `core:stage-art/fer/effects/blood-small-3` | named-core-sprite | Core sprite bloodSmall3 | runtime baseline required |
| `core:stage-art/fer/effects/blood-small-4` | named-core-sprite | Core sprite bloodSmall4 | runtime baseline required |
| `core:stage-art/fer/effects/blood-wall-1` | named-core-sprite | Core sprite BloodSplashesWall1 | runtime baseline required |
| `core:stage-art/fer/effects/blood-wall-2` | named-core-sprite | Core sprite BloodSplashesWall2 | runtime baseline required |
| `core:stage-art/fer/effects/blood-wall-3` | named-core-sprite | Core sprite BloodSplashesWall3 | runtime baseline required |
| `core:weapon-effects/muzzle-flashes/1` | weapon-effect | Core muzzle flash 1 | registered when used by a shipped gun |
| `core:weapon-effects/muzzle-flashes/2` | weapon-effect | Core muzzle flash 2 | registered when used by a shipped gun |
| `core:weapon-effects/muzzle-flashes/3` | weapon-effect | Core muzzle flash 3 | registered when used by a shipped gun |
| `core:weapon/css446/base` | weapon | CSS446 base renderer | registered when the shipped base renderer exists |
| `core:weapon/css446/body` | weapon | CSS446 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/halman-785/base` | weapon | Halman 785 base renderer | registered when the shipped base renderer exists |
| `core:weapon/halman-785/body` | weapon | Halman 785 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/harrington-model-1892/base` | weapon | Harrington Model 1892 base renderer | registered when the shipped base renderer exists |
| `core:weapon/harrington-model-1892/body` | weapon | Harrington Model 1892 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/lk-235/base` | weapon | LK-235 base renderer | registered when the shipped base renderer exists |
| `core:weapon/lk-235/body` | weapon | LK-235 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/m4b1/base` | weapon | M4B1 base renderer | registered when the shipped base renderer exists |
| `core:weapon/m4b1/body` | weapon | M4B1 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/mg32/base` | weapon | MG32 base renderer | registered when the shipped base renderer exists |
| `core:weapon/mg32/body` | weapon | MG32 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/muger-p08/base` | weapon | Muger P08 base renderer | registered when the shipped base renderer exists |
| `core:weapon/muger-p08/body` | weapon | Muger P08 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/np-40/base` | weapon | NP-40 base renderer | registered when the shipped base renderer exists |
| `core:weapon/np-40/body` | weapon | NP-40 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/np-9/base` | weapon | NP-9 base renderer | registered when the shipped base renderer exists |
| `core:weapon/np-9/body` | weapon | NP-9 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/pistol/base` | weapon | Pistol base renderer | registered when the shipped base renderer exists |
| `core:weapon/pistol/body` | weapon | Pistol inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/pistol/slide` | weapon | Pistol slide renderer | registered when the shipped slide renderer exists |
| `core:weapon/qlock-17-a/base` | weapon | Qlock 17-A base renderer | registered when the shipped base renderer exists |
| `core:weapon/qlock-17-a/body` | weapon | Qlock 17-A inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/qlock-17-s/base` | weapon | Qlock 17-S base renderer | registered when the shipped base renderer exists |
| `core:weapon/qlock-17-s/body` | weapon | Qlock 17-S inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/revolver-44/base` | weapon | Revolver .44 base renderer | registered when the shipped base renderer exists |
| `core:weapon/revolver-44/body` | weapon | Revolver .44 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/ronigsberg-543/base` | weapon | Ronigsberg 543 base renderer | registered when the shipped base renderer exists |
| `core:weapon/ronigsberg-543/body` | weapon | Ronigsberg 543 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/sawed-off/base` | weapon | Sawed-Off base renderer | registered when the shipped base renderer exists |
| `core:weapon/sawed-off/body` | weapon | Sawed-Off inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/schockgewehr/base` | weapon | Schockgewehr base renderer | registered when the shipped base renderer exists |
| `core:weapon/schockgewehr/body` | weapon | Schockgewehr inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/sl-defender/base` | weapon | S&L Defender base renderer | registered when the shipped base renderer exists |
| `core:weapon/sl-defender/body` | weapon | S&L Defender inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/snub-revolver-32/base` | weapon | Snub Revolver .32 base renderer | registered when the shipped base renderer exists |
| `core:weapon/snub-revolver-32/body` | weapon | Snub Revolver .32 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/ssph-42/base` | weapon | SSPH-42 base renderer | registered when the shipped base renderer exists |
| `core:weapon/ssph-42/body` | weapon | SSPH-42 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/tenelli-m5/base` | weapon | Tenelli M5 base renderer | registered when the shipped base renderer exists |
| `core:weapon/tenelli-m5/body` | weapon | Tenelli M5 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/tenelli-so3/base` | weapon | Tenelli SO3 base renderer | registered when the shipped base renderer exists |
| `core:weapon/tenelli-so3/body` | weapon | Tenelli SO3 inventory/body sprite | registered when the shipped icon exists |
| `core:weapon/usi/base` | weapon | Usi base renderer | registered when the shipped base renderer exists |
| `core:weapon/usi/body` | weapon | Usi inventory/body sprite | registered when the shipped icon exists |

## Using a slot

Declare the exact slot ID in the manifest `overrides` list, then target its owner in an `assetPatch` and use the suffix relative to that owner in `replacements`. See [Asset patches](../content/asset-patches.md) for examples and resolution rules.
