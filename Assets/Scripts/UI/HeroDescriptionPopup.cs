using System.Collections;
using TMPro;
using UnityEngine;

public class HeroDescriptionPopup : MonoBehaviour
{
	[SerializeField] private GameObject panelRoot;
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private TMP_Text messageText;
	[SerializeField] private float showDurationSeconds = 5f;
	[SerializeField] private float fadeDurationSeconds = 0.35f;

	private Coroutine _running;

	private void Awake()
	{
		if (panelRoot == null)
			panelRoot = gameObject;
		if (canvasGroup == null)
			canvasGroup = panelRoot.GetComponent<CanvasGroup>();
		if (canvasGroup == null)
			canvasGroup = panelRoot.AddComponent<CanvasGroup>();
		HideImmediate();
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
		if (!isSelected)
			return;
		string text = def != null && def.adventurerData != null ? def.adventurerData.description : string.Empty;
		Show(text);
	}

	public void Show(string message)
	{
		if (messageText != null)
			messageText.text = message;
		if (_running != null)
		{
			StopCoroutine(_running);
			_running = null;
		}
		if (panelRoot != null)
			panelRoot.SetActive(true);
		_running = StartCoroutine(ShowThenFade());
	}

	private IEnumerator ShowThenFade()
	{
		float t = 0f;
		while (t < fadeDurationSeconds)
		{
			t += Time.unscaledDeltaTime;
			float k = Mathf.Clamp01(t / fadeDurationSeconds);
			if (canvasGroup != null)
				canvasGroup.alpha = k;
			yield return null;
		}
		if (canvasGroup != null)
			canvasGroup.alpha = 1f;
		float wait = Mathf.Max(0f, showDurationSeconds);
		float elapsed = 0f;
		while (elapsed < wait)
		{
			elapsed += Time.unscaledDeltaTime;
			yield return null;
		}
		t = 0f;
		while (t < fadeDurationSeconds)
		{
			t += Time.unscaledDeltaTime;
			float k = 1f - Mathf.Clamp01(t / fadeDurationSeconds);
			if (canvasGroup != null)
				canvasGroup.alpha = k;
			yield return null;
		}
		HideImmediate();
		_running = null;
	}

	public void HideImmediate()
	{
		if (canvasGroup != null)
			canvasGroup.alpha = 0f;
		if (panelRoot != null)
			panelRoot.SetActive(false);
	}
}



