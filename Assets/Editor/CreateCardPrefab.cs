using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class CreateCardPrefab
{
	[MenuItem("Sunduk/Create/Card Prefab (basic)")]
	public static void Create()
	{
		var root = new GameObject("Card");
		var def = root.AddComponent<CardDefinition>();
		var view = root.AddComponent<CardView>();

		// Visual container
		var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
		canvasGO.transform.SetParent(root.transform, false);
		var canvas = canvasGO.GetComponent<Canvas>();
		canvas.renderMode = RenderMode.WorldSpace;
		var scaler = canvasGO.GetComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

		// Background
		var bgGO = new GameObject("Background", typeof(Image));
		bgGO.transform.SetParent(canvasGO.transform, false);
		var bg = bgGO.GetComponent<Image>();
		bg.rectTransform.sizeDelta = new Vector2(300, 400);

		// Icon
		var iconGO = new GameObject("Icon", typeof(Image));
		iconGO.transform.SetParent(canvasGO.transform, false);
		var icon = iconGO.GetComponent<Image>();
		icon.rectTransform.sizeDelta = new Vector2(256, 256);

		// Title
		var titleGO = new GameObject("Title", typeof(TextMeshProUGUI));
		titleGO.transform.SetParent(canvasGO.transform, false);
		var title = titleGO.GetComponent<TextMeshProUGUI>();
		title.alignment = TextAlignmentOptions.Center;
		title.fontSize = 32;
		title.text = "Card";
		title.rectTransform.anchoredPosition = new Vector2(0, -150);

		// Bind references
		var so = new SerializedObject(view);
		so.FindProperty("backgroundImage").objectReferenceValue = bg;
		so.FindProperty("iconImage").objectReferenceValue = icon;
		so.FindProperty("titleText").objectReferenceValue = title;
		so.ApplyModifiedPropertiesWithoutUndo();

		// Save prefab
		var path = EditorUtility.SaveFilePanelInProject("Save Card Prefab", "Card.prefab", "prefab", "Choose location for the card prefab");
		if (!string.IsNullOrEmpty(path))
		{
			PrefabUtility.SaveAsPrefabAsset(root, path);
		}
		Object.DestroyImmediate(root);
	}

	[MenuItem("Sunduk/Create/Card Prefab (UI layout)")]
	public static void CreateUiLayout()
	{
		var root = new GameObject("Card", typeof(RectTransform));
		var rect = root.GetComponent<RectTransform>();
		rect.sizeDelta = new Vector2(300, 400);

		var def = root.AddComponent<CardDefinition>();
		var view = root.AddComponent<CardView>();

		// LayoutElement for HG compatibility
		var le = root.AddComponent<LayoutElement>();
		le.preferredWidth = 300;
		le.preferredHeight = 400;

		// Background
		var bgGO = new GameObject("Background", typeof(Image));
		bgGO.transform.SetParent(root.transform, false);
		var bg = bgGO.GetComponent<Image>();
		var bgRect = bg.rectTransform;
		bgRect.anchorMin = new Vector2(0, 0);
		bgRect.anchorMax = new Vector2(1, 1);
		bgRect.offsetMin = Vector2.zero;
		bgRect.offsetMax = Vector2.zero;

		// Icon
		var iconGO = new GameObject("Icon", typeof(Image));
		iconGO.transform.SetParent(root.transform, false);
		var icon = iconGO.GetComponent<Image>();
		var iconRect = icon.rectTransform;
		iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.6f);
		iconRect.sizeDelta = new Vector2(256, 256);

		// Title
		var titleGO = new GameObject("Title", typeof(TextMeshProUGUI));
		titleGO.transform.SetParent(root.transform, false);
		var title = titleGO.GetComponent<TextMeshProUGUI>();
		title.alignment = TextAlignmentOptions.Center;
		title.fontSize = 32;
		title.text = "Card";
		var titleRect = title.rectTransform;
		titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 0.1f);
		titleRect.sizeDelta = new Vector2(260, 48);

		// Bind references
		var so = new SerializedObject(view);
		so.FindProperty("backgroundImage").objectReferenceValue = bg;
		so.FindProperty("iconImage").objectReferenceValue = icon;
		so.FindProperty("titleText").objectReferenceValue = title;
		so.ApplyModifiedPropertiesWithoutUndo();

		// Save prefab
		var path = EditorUtility.SaveFilePanelInProject("Save Card Prefab (UI)", "Card_UI.prefab", "prefab", "Choose location for the UI card prefab");
		if (!string.IsNullOrEmpty(path))
		{
			PrefabUtility.SaveAsPrefabAsset(root, path);
		}
		Object.DestroyImmediate(root);
	}
}


