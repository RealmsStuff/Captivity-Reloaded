# Public asset-slot catalog

This searchable catalog is generated from `CoreAssetSlotBinder.cs`, packaged Core weapon definitions, and `core-clothing.json`. Regenerate it with `Tools/Documentation/Generate-AssetSlotCatalog.ps1`.

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
| `core:sprite/arm-lower-2/image` | legacy-presentation | Placed/background sprite ArmLower_2 | runtime baseline required |
| `core:sprite/arm-upper-18/image` | legacy-presentation | Placed/background sprite ArmUpper_18 | runtime baseline required |
| `core:sprite/arm-upper-20/image` | legacy-presentation | Placed/background sprite ArmUpper_20 | runtime baseline required |
| `core:sprite/blush/image` | legacy-presentation | Placed/background sprite Blush | runtime baseline required |
| `core:sprite/butt-15/image` | legacy-presentation | Placed/background sprite Butt_15 | runtime baseline required |
| `core:sprite/butt-2/image` | named-core-sprite | Core sprite Butt_2 | runtime baseline required |
| `core:sprite/butt-5/image` | named-core-sprite | Core sprite Butt_5 | runtime baseline required |
| `core:sprite/butt-6/image` | named-core-sprite | Core sprite Butt_6 | runtime baseline required |
| `core:sprite/butt/image` | named-core-sprite | Core sprite Butt | runtime baseline required |
| `core:sprite/chest-5/image` | named-core-sprite | Core sprite Chest_5 | runtime baseline required |
| `core:sprite/chest-7/image` | named-core-sprite | Core sprite Chest_7 | runtime baseline required |
| `core:sprite/eyelid/image` | legacy-presentation | Placed/background sprite Eyelid | runtime baseline required |
| `core:sprite/face/image` | legacy-presentation | Placed/background sprite face | runtime baseline required |
| `core:sprite/foot/image` | legacy-presentation | Placed/background sprite Foot | runtime baseline required |
| `core:sprite/hand-16/image` | legacy-presentation | Placed/background sprite Hand_16 | runtime baseline required |
| `core:sprite/hand-2/image` | legacy-presentation | Placed/background sprite Hand_2 | runtime baseline required |
| `core:sprite/head-8/image` | legacy-presentation | Placed/background sprite Head_8 | runtime baseline required |
| `core:sprite/head1/image` | legacy-presentation | Placed/background sprite Head1 | runtime baseline required |
| `core:sprite/head2/image` | legacy-presentation | Placed/background sprite Head2 | runtime baseline required |
| `core:sprite/head3/image` | legacy-presentation | Placed/background sprite Head3 | runtime baseline required |
| `core:sprite/head4/image` | legacy-presentation | Placed/background sprite Head4 | runtime baseline required |
| `core:sprite/hip-3/image` | legacy-presentation | Placed/background sprite Hip_3 | runtime baseline required |
| `core:sprite/hip-5/image` | legacy-presentation | Placed/background sprite Hip_5 | runtime baseline required |
| `core:sprite/lady-statue/image` | legacy-presentation | Placed/background sprite LadyStatue | runtime baseline required |
| `core:sprite/leg-lower-11/image` | legacy-presentation | Placed/background sprite LegLower_11 | runtime baseline required |
| `core:sprite/leg-lower-15/image` | legacy-presentation | Placed/background sprite LegLower_15 | runtime baseline required |
| `core:sprite/leg-upper-1/image` | legacy-presentation | Placed/background sprite LegUpper_1 | runtime baseline required |
| `core:sprite/leg-upper-3/image` | legacy-presentation | Placed/background sprite LegUpper_3 | runtime baseline required |
| `core:sprite/medic-body/image` | legacy-presentation | Placed/background sprite MedicBody | runtime baseline required |
| `core:sprite/neck-5/image` | legacy-presentation | Placed/background sprite Neck_5 | runtime baseline required |
| `core:sprite/neck-8/image` | legacy-presentation | Placed/background sprite Neck_8 | runtime baseline required |
| `core:sprite/painting/image` | legacy-presentation | Placed/background sprite painting | runtime baseline required |
| `core:sprite/rontgen/image` | legacy-presentation | Placed/background sprite rontgen | runtime baseline required |
| `core:sprite/torso-lower-0/image` | named-core-sprite | Core sprite TorsoLower_0 | runtime baseline required |
| `core:sprite/torso-lower-8/image` | legacy-presentation | Placed/background sprite TorsoLower_8 | runtime baseline required |
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
