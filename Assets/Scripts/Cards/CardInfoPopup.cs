using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardInfoPopup : MonoBehaviour
{
	[SerializeField] private GameObject panelRoot;
	[SerializeField] private TMP_Text titleText;
	[SerializeField] private TMP_Text descriptionText;
	[SerializeField] private Image iconImage;
	[SerializeField] private Image backgroundImage;
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private float showDurationSeconds = 5f;
	[SerializeField] private float fadeDurationSeconds = 0.35f;

	private Coroutine _fadeRoutine;

	private void Awake()
	{
		if (panelRoot == null)
			panelRoot = gameObject;
		if (canvasGroup == null)
			canvasGroup = panelRoot.GetComponent<CanvasGroup>();
		if (canvasGroup == null)
			canvasGroup = panelRoot.AddComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		if (panelRoot != null) panelRoot.SetActive(false);
	}

	private void OnEnable()
	{
		CardInteraction.AdventurerSelected += OnAdventurerSelected;
	}

	private void OnDisable()
	{
		CardInteraction.AdventurerSelected -= OnAdventurerSelected;
	}

	private void OnAdventurerSelected(CardDefinition def, bool isSelected)
	{
		if (isSelected)
		{
			Show(def);
		}
		else
		{
			Hide();
		}
	}

	public void Show(CardDefinition def)
	{
		if (panelRoot != null) panelRoot.SetActive(true);
		if (def != null && def.adventurerData != null)
		{
			if (titleText != null) titleText.text = def.displayName;
			if (descriptionText != null) descriptionText.text = def.adventurerData.description;
			if (iconImage != null) iconImage.sprite = def.icon;
			if (backgroundImage != null) backgroundImage.sprite = def.backgroundSprite;
		}
		// Сброс и запуск авто‑скрытия с фейдом
		if (_fadeRoutine != null)
		{
			StopCoroutine(_fadeRoutine);
			_fadeRoutine = null;
		}
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 1f;
		}
		_fadeRoutine = StartCoroutine(AutoFadeRoutine());
	}

	public void Hide()
	{
		if (_fadeRoutine != null)
		{
			StopCoroutine(_fadeRoutine);
			_fadeRoutine = null;
		}
		if (canvasGroup != null)
			canvasGroup.alpha = 0f;
		if (panelRoot != null) panelRoot.SetActive(false);
	}

	private System.Collections.IEnumerator AutoFadeRoutine()
	{
		float wait = Mathf.Max(0f, showDurationSeconds);
		float elapsed = 0f;
		while (elapsed < wait)
		{
			elapsed += Time.unscaledDeltaTime;
			yield return null;
		}
		float t = 0f;
		while (t < fadeDurationSeconds)
		{
			t += Time.unscaledDeltaTime;
			float k = 1f - Mathf.Clamp01(t / fadeDurationSeconds);
			if (canvasGroup != null)
				canvasGroup.alpha = k;
			yield return null;
		}
		Hide();
	}
}


