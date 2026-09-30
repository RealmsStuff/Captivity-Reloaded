# LibreSprite templates

LibreSprite generators are available for the Zombie I inherited enemy, the default shirt, scientist/lab-coat set, hazmat suit, pistol, Tenelli SO3, and Revolver .44. Enemy and clothing kits arrange parts in fixed atlas cells. Weapon kits use overlapping named layers because each weapon part is exported as a separate PNG.

Set the selected script's `projectRoot`, copy it into the folder opened by **Scripts -> Open Scripts Folder**, rescan scripts, and run it from LibreSprite's Scripts menu. Output is written back to the SDK template directory.

For the supplied clothing atlas templates, each body part begins in a 32 by 32 cell. That is a Core-art default, not a clipping limit: enlarge the generator's `cellWidth`/`cellHeight` and the matching JSON regions for extended garments. Disable trimming or automatic packing. The JSON rectangles use Unity's bottom-left origin, while LibreSprite displays the canvas from the top left.

For weapon templates, preserve the documented full canvas and export one visible named layer at a time to its matching PNG. Pistol and Revolver .44 parts are 32 by 32. Tenelli SO3 parts are 48 by 32.

The generators are source-controlled instead of requiring binary editor documents, so each template can be recreated and audited whenever its layout changes. LibreSprite uses JavaScript for automation; Aseprite Lua scripts are not directly compatible.
