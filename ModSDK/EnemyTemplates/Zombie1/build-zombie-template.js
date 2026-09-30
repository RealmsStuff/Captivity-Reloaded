// Captivity Reloaded Zombie I inherited-rig template generator for LibreSprite.
// Copy this script to LibreSprite's scripts directory and run it from the
// Scripts menu. Change projectRoot when using a different repository checkout.

const projectRoot = "C:/Users/pepom/Downloads/ytivitpac/Captivity-Reloaded";
const outputRoot = projectRoot + "/ModSDK/EnemyTemplates/Zombie1/";
const sourceRoot = outputRoot + "source-parts/";
const outputSource = outputRoot + "zombie-1-template.aseprite";
const outputAtlas = outputRoot + "zombie-1-template.png";

const parts = [
  { slot: "body/torso-lower", file: "torso-lower.png", column: 0, row: 0 },
  { slot: "body/butt", file: "butt.png", column: 1, row: 0 },
  { slot: "body/hips", file: "hips.png", column: 2, row: 0 },
  { slot: "body/chest", file: "chest.png", column: 3, row: 0 },
  { slot: "body/neck", file: "neck.png", column: 0, row: 1 },
  { slot: "body/head", file: "head.png", column: 1, row: 1 },
  { slot: "body/arm-upper", file: "arm-upper.png", column: 2, row: 1 },
  { slot: "body/arm-lower", file: "arm-lower.png", column: 3, row: 1 },
  { slot: "body/hand", file: "hand.png", column: 0, row: 2 },
  { slot: "body/leg-upper", file: "leg-upper.png", column: 1, row: 2 },
  { slot: "body/leg-lower", file: "leg-lower.png", column: 2, row: 2 },
  { slot: "body/foot-left", file: "foot-left.png", column: 3, row: 2 },
  { slot: "body/foot-right", file: "foot-right.png", column: 0, row: 3 }
];

function buildTemplate() {
  // LibreSprite currently ignores NewFile's scripted width/height arguments,
  // so create the document and explicitly resize it before adding artwork.
  app.command.NewFile();

  const templateDocument = app.activeDocument;
  templateDocument.sprite.resize(128, 128);
  if (templateDocument.sprite.width !== 128 || templateDocument.sprite.height !== 128) {
    throw new Error("Could not create the required 128x128 Zombie I atlas canvas.");
  }

  for (let index = 0; index < parts.length; ++index) {
    const part = parts[index];

    app.open(sourceRoot + part.file);
    const sourceImage = app.activeImage;
    const sourcePixels = [];
    for (let y = 0; y < 32; ++y) {
      for (let x = 0; x < 32; ++x) {
        sourcePixels.push(sourceImage.getPixel(x, y));
      }
    }
    app.activeDocument.close();

    if (index > 0) {
      app.command.DuplicateLayer();
    }

    const layer = app.activeSprite.layer(app.activeLayerNumber);
    layer.name = part.slot;
    const targetImage = app.activeImage;
    if (!targetImage) {
      throw new Error("LibreSprite created no image for " + part.slot);
    }

    targetImage.clear(app.pixelColor.rgba(0, 0, 0, 0));
    for (let y = 0; y < 32; ++y) {
      for (let x = 0; x < 32; ++x) {
        targetImage.putPixel(
          part.column * 32 + x,
          part.row * 32 + y,
          sourcePixels[y * 32 + x]);
      }
    }
  }

  templateDocument.sprite.commit();
  templateDocument.sprite.saveAs(outputSource, false);
  templateDocument.sprite.saveAs(outputAtlas, true);

  console.log("Created " + outputSource);
  console.log("Exported " + outputAtlas);
}

function onEvent(event) {
  if (event === "init") {
    buildTemplate();
  }
}
