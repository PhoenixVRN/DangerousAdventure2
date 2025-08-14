using UnityEngine;
using UnityEngine.UI;

public class StartMenuController : MonoBehaviour
{
	[SerializeField] private GameObject panelRoot;
	[SerializeField] private CanvasGroup canvasGroup;
	[SerializeField] private Button startGameButton;

	private void Awake()
	{
		if (panelRoot == null)
			panelRoot = gameObject;
		if (canvasGroup == null)
			canvasGroup = panelRoot.GetComponent<CanvasGroup>();
		if (canvasGroup == null)
			canvasGroup = panelRoot.AddComponent<CanvasGroup>();
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
			Open();
		}
		if (startGameButton != null)
		{
			startGameButton.onClick.RemoveAllListeners();
			startGameButton.onClick.AddListener(() => GameManager.Instance?.OnStartGameButtonClicked());
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
		if (state == GameManager.GameState.MainMenu)
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



