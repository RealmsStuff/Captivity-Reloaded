// Captivity Reloaded inherited-weapon template generator for LibreSprite.
// Copy this file to LibreSprite's scripts directory and run it from Scripts.
// Change projectRoot when using a different repository checkout.

const projectRoot = "C:/Users/pepom/Downloads/ytivitpac/Captivity-Reloaded";
const templatesRoot = projectRoot + "/ModSDK/WeaponTemplates/";

const templates = [
  {
    directory: "Pistol", output: "pistol-template", width: 32, height: 32,
    parts: [["body", "body.png"], ["slide", "slide.png"], ["magazine", "magazine.png"], ["base", "base.png"]]
  },
  {
    directory: "TenelliSO3", output: "tenelli-so3-template", width: 48, height: 32,
    parts: [["body", "body.png"], ["base", "base.png"], ["magazine", "magazine.png"], ["shell", "shell.png"]]
  },
  {
    directory: "Revolver44", output: "revolver-44-template", width: 32, height: 32,
    parts: [
      ["body", "body.png"], ["hammer", "hammer.png"], ["chamber", "chamber.png"], ["base", "base.png"],
      ["bullet", "bullet.png"], ["bullet-1", "bullet-1.png"], ["bullet-2", "bullet-2.png"],
      ["bullet-3", "bullet-3.png"], ["bullet-4", "bullet-4.png"], ["bullet-5", "bullet-5.png"]
    ]
  }
];

function buildTemplate(template) {
  const outputRoot = templatesRoot + template.directory + "/";
  const sourceRoot = outputRoot + "assets/weapon/";
  app.command.NewFile();
  const templateDocument = app.activeDocument;
  templateDocument.sprite.resize(template.width, template.height);
  if (templateDocument.sprite.width !== template.width || templateDocument.sprite.height !== template.height) {
    throw new Error("Could not create the required canvas for " + template.output);
  }

  for (let index = 0; index < template.parts.length; ++index) {
    const slot = template.parts[index][0];
    const file = template.parts[index][1];
    app.open(sourceRoot + file);
    const sourceImage = app.activeImage;
    const sourcePixels = [];
    for (let y = 0; y < template.height; ++y) {
      for (let x = 0; x < template.width; ++x) sourcePixels.push(sourceImage.getPixel(x, y));
    }
    app.activeDocument.close();

    if (index > 0) app.command.DuplicateLayer();
    const layer = app.activeSprite.layer(app.activeLayerNumber);
    layer.name = slot;
    const targetImage = app.activeImage;
    if (!targetImage) throw new Error("LibreSprite created no image for " + slot);
    targetImage.clear(app.pixelColor.rgba(0, 0, 0, 0));
    for (let y = 0; y < template.height; ++y) {
      for (let x = 0; x < template.width; ++x) {
        targetImage.putPixel(x, y, sourcePixels[y * template.width + x]);
      }
    }
  }

  templateDocument.sprite.commit();
  templateDocument.sprite.saveAs(outputRoot + template.output + ".aseprite", false);
  templateDocument.close();
  console.log("Created " + template.output);
}

function onEvent(event) {
  if (event !== "init") return;
  for (let index = 0; index < templates.length; ++index) buildTemplate(templates[index]);
}
