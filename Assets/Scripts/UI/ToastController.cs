using System.Collections;
using TMPro;
using UnityEngine;

public class ToastController : MonoBehaviour
{
	public static ToastController Instance { get; private set; }

	[SerializeField] private GameObject panelRoot;
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private TMP_Text messageText;

	private Coroutine _autoHide;

	private void Awake()
	{
		Instance = this;
		if (panelRoot == null) panelRoot = gameObject;
		if (canvasGroup == null) canvasGroup = panelRoot.GetComponent<CanvasGroup>();
		if (canvasGroup == null) canvasGroup = panelRoot.AddComponent<CanvasGroup>();
		HideImmediate();
	}

	public void Show(string message, float durationSeconds = 3f)
	{
		if (messageText != null) messageText.text = message;
		if (panelRoot != null) panelRoot.SetActive(true);
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 1f;
			canvasGroup.blocksRaycasts = false;
			canvasGroup.interactable = false;
		}
		if (_autoHide != null)
		{
			StopCoroutine(_autoHide);
			_autoHide = null;
		}
		if (durationSeconds > 0f)
		{
			_autoHide = StartCoroutine(AutoHide(durationSeconds));
		}
	}

	private IEnumerator AutoHide(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		HideImmediate();
	}

	public void HideImmediate()
	{
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 0f;
			canvasGroup.blocksRaycasts = false;
			canvasGroup.interactable = false;
		}
		if (panelRoot != null) panelRoot.SetActive(false);
	}
}



