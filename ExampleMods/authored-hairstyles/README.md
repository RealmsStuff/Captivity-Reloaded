# Authored Hairstyle

Adds **Moving Ponytail** to the Hair wardrobe tab. It is derived directly from the Core brown ponytail at its native 64×64 canvas and 32 pixels per unit.

The front/scalp pixels and ponytail pixels live on separate full-size layers. Their pivots and offsets reconstruct the original hairstyle at rest, while only the ponytail receives bounded sway physics. This avoids resizing, atlas cropping, and generated replacement artwork.
