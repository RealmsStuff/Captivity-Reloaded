# Prey / Bunny Girls Overhaul

Faithful `.capmod` source conversion of Draco66Electro's legacy **Prey Mod
(Bunny Girls)** for Captivity v1.0.4.

The three supplied Unity archives were compared with Captivity v1.0.5b by
decoded object content. `globalgamemanagers.assets` and `resources.assets`
contain no material changes. `sharedassets0.assets` contains 158 changed
texture records: 154 authored visual replacements and four records containing
only texture-compression noise.

The 154 authored images are represented without replacing gameplay code:

- all four player skin palettes, including the bunny body, head, ears, limbs,
  torso, chest, hips, feet and lower eyelids;
- Core hair, hats, shirts, armor, underwear, pants and the Playboy Bunny set;
- the Zombie I, Zombie II, Zombie III and Zombie Grabber artwork touched by the
  original texture replacement;
- Betsy and the placed/background character pieces used by the shipped stages;
- the changed Shack painting, FER X-ray, Space Station medic, Hub face, Jungle
  statue and other background artwork.

The pack intentionally does not add the unrelated experimental Green Stalker,
stage, challenge, attacks or animations from `prey-green-zombie`. Those remain
an authoring demonstration and were never part of the supplied legacy release.

`conversion-report.json` records every legacy path ID, recovered source name,
public target and excluded noise record. Rebuild the source conversion with:

```powershell
python Tools/convert-prey-bunny-girls.py `
  --prey "..\Mods to possible features\Prey Mod (Bunny Girls)\sharedassets0.assets" `
  --baseline "..\Captivity the original\Captivity v1.0.5b\Captivity_Data\sharedassets0.assets"
```

This source is ready for isolated in-game visual testing. It is not a public
redistribution grant; the original author's permission still needs to be
recorded before catalog publication.
