// Captivity Reloaded inherited-clothing template generator for LibreSprite.
// Copy this script to LibreSprite's scripts directory and run it from Scripts.
// Change projectRoot when using a different repository checkout.

const projectRoot = "C:/Users/pepom/Downloads/ytivitpac/Captivity-Reloaded";
const templatesRoot = projectRoot + "/ModSDK/ClothingTemplates/";

const templates = [
  {
    directory: "DefaultShirt",
    output: "default-shirt-template",
    columns: 3,
    rows: 1,
    parts: [
      ["piece/shirt-spine", "shirt-spine.png"],
      ["piece/shirt-chest", "shirt-chest.png"],
      ["icon", "icon.png"]
    ]
  },
  {
    directory: "LabCoat",
    output: "lab-coat-template",
    columns: 4,
    rows: 4,
    parts: [
      ["piece/shirt-spine", "shirt-spine.png"],
      ["piece/shirt-chest", "shirt-chest.png"],
      ["piece/shirt-r-arm-upper", "shirt-r-arm-upper.png"],
      ["piece/shirt-r-arm-lower", "shirt-r-arm-lower.png"],
      ["piece/shirt-l-arm-upper", "shirt-l-arm-upper.png"],
      ["piece/shirt-l-arm-lower", "shirt-l-arm-lower.png"],
      ["piece/r-leg-upper", "r-leg-upper.png"],
      ["piece/l-leg-upper", "l-leg-upper.png"],
      ["piece/r-leg-lower", "r-leg-lower.png"],
      ["piece/l-leg-lower", "l-leg-lower.png"],
      ["piece/hips", "hips.png"],
      ["piece/butt", "butt.png"],
      ["icon", "icon.png"]
    ]
  },
  {
    directory: "HazmatSuit",
    output: "hazmat-suit-template",
    columns: 5,
    rows: 5,
    parts: [
      ["piece/r-leg-lower-shoes", "r-leg-lower-shoes.png"],
      ["piece/head", "head.png"],
      ["piece/l-foot", "l-foot.png"],
      ["piece/r-hand", "r-hand.png"],
      ["piece/r-arm-upper", "r-arm-upper.png"],
      ["piece/neck", "neck.png"],
      ["piece/l-leg-lower", "l-leg-lower.png"],
      ["piece/r-leg-upper", "r-leg-upper.png"],
      ["piece/chest", "chest.png"],
      ["piece/spine", "spine.png"],
      ["piece/r-leg-lower", "r-leg-lower.png"],
      ["piece/l-leg-lower-shoes", "l-leg-lower-shoes.png"],
      ["piece/r-arm-lower", "r-arm-lower.png"],
      ["piece/hips", "hips.png"],
      ["piece/mask", "mask.png"],
      ["piece/butt", "butt.png"],
      ["piece/r-foot", "r-foot.png"],
      ["piece/l-hand", "l-hand.png"],
      ["piece/l-arm-lower", "l-arm-lower.png"],
      ["piece/l-arm-upper", "l-arm-upper.png"],
      ["piece/l-leg-upper", "l-leg-upper.png"],
      ["icon", "icon.png"]
    ]
  }
];

function buildTemplate(template) {
  const outputRoot = templatesRoot + template.directory + "/";
  const sourceRoot = outputRoot + "source-parts/";
  app.command.NewFile();
  const templateDocument = app.activeDocument;
  // 32x32 is the Core source-art size, not a runtime or renderer limit. Authors
  // can set cellWidth/cellHeight on a copied descriptor for skirts, capes, etc.
  const cellWidth = template.cellWidth || 32;
  const cellHeight = template.cellHeight || 32;
  const width = template.columns * cellWidth;
  const height = template.rows * cellHeight;
  templateDocument.sprite.resize(width, height);
  if (templateDocument.sprite.width !== width || templateDocument.sprite.height !== height) {
    throw new Error("Could not create the required canvas for " + template.output);
  }

  for (let index = 0; index < template.parts.length; ++index) {
    const slot = template.parts[index][0];
    const file = template.parts[index][1];
    app.open(sourceRoot + file);
    const sourceImage = app.activeImage;
    const sourcePixels = [];
    const copyWidth = Math.min(sourceImage.width, cellWidth);
    const copyHeight = Math.min(sourceImage.height, cellHeight);
    for (let y = 0; y < copyHeight; ++y) {
      for (let x = 0; x < copyWidth; ++x) sourcePixels.push(sourceImage.getPixel(x, y));
    }
    app.activeDocument.close();

    if (index > 0) app.command.DuplicateLayer();
    const layer = app.activeSprite.layer(app.activeLayerNumber);
    layer.name = slot;
    const targetImage = app.activeImage;
    if (!targetImage) throw new Error("LibreSprite created no image for " + slot);
    targetImage.clear(app.pixelColor.rgba(0, 0, 0, 0));
    const column = index % template.columns;
    const row = Math.floor(index / template.columns);
    for (let y = 0; y < copyHeight; ++y) {
      for (let x = 0; x < copyWidth; ++x) {
        targetImage.putPixel(column * cellWidth + x, row * cellHeight + y, sourcePixels[y * copyWidth + x]);
      }
    }
  }

  templateDocument.sprite.commit();
  templateDocument.sprite.saveAs(outputRoot + template.output + ".aseprite", false);
  templateDocument.sprite.saveAs(outputRoot + "assets/clothing/" + template.output + ".png", true);
  templateDocument.close();
  console.log("Created " + template.output);
}

function onEvent(event) {
  if (event !== "init") return;
  for (let index = 0; index < templates.length; ++index) buildTemplate(templates[index]);
}
