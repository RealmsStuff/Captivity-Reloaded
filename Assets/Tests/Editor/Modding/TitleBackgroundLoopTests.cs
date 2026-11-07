using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CaptivityReloaded.Modding.Tests
{
	// The title screen's plank background is a tiled Image that bgAnim slides to the left and then snaps back.
	// The snap only hides when the slide is a whole number of tiles; otherwise the planks jump sideways once a loop.
	public sealed class TitleBackgroundLoopTests
	{
		private const string ScenePath = "Assets/Scenes/Main.unity";
		private const string ClipPath = "Assets/AnimationClip/bgAnim.anim";
		private const string BackgroundName = "titleBgAnimation";

		private Scene m_scene;

		[OneTimeSetUp]
		public void OpenScene()
		{
			m_scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
		}

		[OneTimeTearDown]
		public void CloseScene()
		{
			if (m_scene.IsValid()) EditorSceneManager.CloseScene(m_scene, true);
		}

		[Test]
		public void TheLoopSlidesWholeTiles()
		{
			RectTransform background = FindBackground();
			Component image = background.GetComponents<Component>().Single(component => component.GetType().Name == "Image");
			SerializedObject imageData = new SerializedObject(image);
			Assert.That(imageData.FindProperty("m_Type").enumValueIndex, Is.EqualTo(2), "The background is no longer a tiled Image.");
			Sprite sprite = (Sprite)imageData.FindProperty("m_Sprite").objectReferenceValue;
			float multiplier = imageData.FindProperty("m_PixelsPerUnitMultiplier").floatValue;

			Canvas canvas = background.GetComponentInParent<Canvas>(true).rootCanvas;
			Component scaler = canvas.GetComponents<Component>().Single(component => component.GetType().Name == "CanvasScaler");
			float referencePixelsPerUnit = new SerializedObject(scaler).FindProperty("m_ReferencePixelsPerUnit").floatValue;

			// UI Image.GenerateTiledSprite: one tile is the sprite's size scaled by referencePixelsPerUnit / sprite PPU.
			float tileInParent = sprite.rect.width * referencePixelsPerUnit / sprite.pixelsPerUnit / multiplier * background.localScale.x;
			AnimationCurve x = Curve("m_AnchoredPosition.x");
			float slide = Mathf.Abs(x.Evaluate(Clip().length) - x.Evaluate(0f));
			float tiles = slide / tileInParent;
			Assert.That(tiles, Is.GreaterThanOrEqualTo(1f - 1e-3f), "The loop slides less than one tile.");
			Assert.That(Mathf.Abs(tiles - Mathf.Round(tiles)), Is.LessThan(1e-3f),
				"bgAnim slides " + slide + " units a loop, but one plank tile is " + tileInParent + " units wide: the planks jump when the loop restarts.");
		}

		[Test]
		public void TheBackgroundCoversAnUltrawideScreenThroughTheWholeLoop()
		{
			RectTransform background = FindBackground();
			AnimationCurve x = Curve("m_AnchoredPosition.x");
			AnimationCurve width = Curve("m_SizeDelta.x");
			float length = Clip().length;
			// Canvas units at 32:9 when the canvas matches its 1080-unit reference height.
			float halfScreen = 1080f * 32f / 9f / 2f;
			for (int step = 0; step <= 60; step++)
			{
				float time = length * step / 60f;
				float halfImage = width.Evaluate(time) * background.localScale.x / 2f;
				float centre = x.Evaluate(time);
				Assert.That(centre - halfImage, Is.LessThanOrEqualTo(-halfScreen), "Gap on the left at " + time + " s.");
				Assert.That(centre + halfImage, Is.GreaterThanOrEqualTo(halfScreen), "Gap on the right at " + time + " s.");
			}
		}

		private RectTransform FindBackground()
		{
			RectTransform found = m_scene.GetRootGameObjects()
				.SelectMany(root => root.GetComponentsInChildren<RectTransform>(true))
				.FirstOrDefault(transform => transform.name == BackgroundName);
			Assert.That(found, Is.Not.Null, BackgroundName + " is not in " + ScenePath + ".");
			return found;
		}

		private static AnimationClip Clip()
		{
			AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath);
			Assert.That(clip, Is.Not.Null, ClipPath);
			return clip;
		}

		private static AnimationCurve Curve(string i_property)
		{
			EditorCurveBinding binding = AnimationUtility.GetCurveBindings(Clip()).Single(item => item.propertyName == i_property);
			return AnimationUtility.GetEditorCurve(Clip(), binding);
		}
	}
}
