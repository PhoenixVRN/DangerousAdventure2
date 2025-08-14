using UnityEngine;
using UnityEngine.UI;

public class DragonGiveUpController : MonoBehaviour
{
	[SerializeField] private Button giveUpButton;

	private void OnEnable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnStateChanged += HandleStateChanged;
			ApplyForState(GameManager.Instance.CurrentState);
		}
		if (giveUpButton != null)
		{
			giveUpButton.onClick.RemoveAllListeners();
			giveUpButton.onClick.AddListener(OnGiveUpClicked);
		}
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
			GameManager.Instance.OnStateChanged -= HandleStateChanged;
		if (giveUpButton != null)
			giveUpButton.onClick.RemoveListener(OnGiveUpClicked);
	}

	private void HandleStateChanged(GameManager.GameState previous, GameManager.GameState next)
	{
		ApplyForState(next);
	}

	private void ApplyForState(GameManager.GameState state)
	{
		if (giveUpButton == null) return;
		bool visible = state == GameManager.GameState.DragonBattle;
		giveUpButton.gameObject.SetActive(visible);
		giveUpButton.interactable = visible;
	}

	public void OnGiveUpClicked()
	{
		GameManager.Instance?.EnterTavern();
		ToastController.Instance?.Show("Вы проиграли", 3f);
	}
}



