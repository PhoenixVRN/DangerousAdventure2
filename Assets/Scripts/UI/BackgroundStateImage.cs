using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BackgroundStateImage : MonoBehaviour
{
	[Header("Background Sprites")]
	[SerializeField] private Sprite mainMenuSprite;
	[SerializeField] private Sprite tavernSprite;
	[SerializeField] private Sprite enemyBattleSprite;
	[SerializeField] private Sprite dragonBattleSprite;
	[SerializeField] private Sprite graveyardSprite;
	[SerializeField] private Sprite defaultSprite;

	private Image _image;

	private void Awake()
	{
		_image = GetComponent<Image>();
	}

	private void OnEnable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnStateChanged += HandleStateChanged;
			ApplySpriteForState(GameManager.Instance.CurrentState);
		}
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnStateChanged -= HandleStateChanged;
		}
	}

	private void HandleStateChanged(GameManager.GameState previous, GameManager.GameState next)
	{
		ApplySpriteForState(next);
	}

	private void ApplySpriteForState(GameManager.GameState state)
	{
		Sprite spriteToUse = defaultSprite;
		switch (state)
		{
			case GameManager.GameState.MainMenu:
				spriteToUse = mainMenuSprite ? mainMenuSprite : defaultSprite;
				break;
			case GameManager.GameState.Tavern:
				spriteToUse = tavernSprite ? tavernSprite : defaultSprite;
				break;
			case GameManager.GameState.EnemyBattle:
				spriteToUse = enemyBattleSprite ? enemyBattleSprite : defaultSprite;
				break;
			case GameManager.GameState.DragonBattle:
				spriteToUse = dragonBattleSprite ? dragonBattleSprite : defaultSprite;
				break;
			case GameManager.GameState.Graveyard:
				spriteToUse = graveyardSprite ? graveyardSprite : defaultSprite;
				break;
			default:
				spriteToUse = defaultSprite;
				break;
		}

		// Без инстансов/создания — только замена спрайта существующего Image.
		_image.sprite = spriteToUse;
		_image.enabled = spriteToUse != null; // если спрайта нет — скрыть фон
	}
}


