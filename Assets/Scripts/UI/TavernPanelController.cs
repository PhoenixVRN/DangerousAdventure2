using UnityEngine;

public class TavernPanelController : MonoBehaviour
{
	[SerializeField] private GameObject panelRoot;
	[SerializeField] private CanvasGroup canvasGroup;

	private void Awake()
	{
		// Автоподхват корня панели
		if (panelRoot == null)
			panelRoot = gameObject;
		// Автоподхват/создание CanvasGroup
		if (canvasGroup == null)
			canvasGroup = panelRoot.GetComponent<CanvasGroup>();
		if (canvasGroup == null)
			canvasGroup = panelRoot.AddComponent<CanvasGroup>();
		// Закрыть по умолчанию
		Close();
	}

	private void OnEnable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnStateChanged += HandleStateChanged;
			ApplyForState(GameManager.Instance.CurrentState);
		}
		else
		{
			// Если менеджера нет — держим панель закрытой
			Close();
		}
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
			GameManager.Instance.OnStateChanged -= HandleStateChanged;
	}

	private void HandleStateChanged(GameManager.GameState previous, GameManager.GameState next)
	{
		ApplyForState(next);
	}

	private void ApplyForState(GameManager.GameState state)
	{
		if (state == GameManager.GameState.Tavern)
			Open();
		else
			Close();
	}

	public void Open()
	{
		if (panelRoot != null) panelRoot.SetActive(true);
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 1f;
			canvasGroup.interactable = true;
			canvasGroup.blocksRaycasts = true;
		}
	}

	public void Close()
	{
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 0f;
			canvasGroup.interactable = false;
			canvasGroup.blocksRaycasts = false;
		}
		if (panelRoot != null) panelRoot.SetActive(false);
	}
}


