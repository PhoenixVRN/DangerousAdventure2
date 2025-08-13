using System;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	public enum GameState
	{
		Boot,
		MainMenu,
		Tavern,
		RunInit,
		DealHand,
		EnemyBattle,
		DragonBattle,
		ChooseContinue,
		ExitRun,
		Graveyard,
		GameSummary,
		Pause
	}

	public event Action<GameState, GameState> OnStateChanged;

	[SerializeField]
	private bool persistAcrossScenes = true;

	[Header("UI References")]
	[SerializeField] private Button startGameButton; // назначь вручную в инспекторе
	[SerializeField] private UnityEngine.UI.Image backgroundImage; // фон (назначишь Image на сцене)

	public GameState CurrentState { get; private set; } = GameState.Boot;

	public int CurrentLevel { get; private set; } = 0;
	public int CurrentDelveIndex { get; private set; } = 0; // 0..2 для трёх делвов по умолчанию

	private GameState _stateBeforePause = GameState.Boot;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		if (persistAcrossScenes)
		{
			DontDestroyOnLoad(gameObject);
		}
		SetStateInternal(GameState.Boot);
	}

	// Публичные команды переходов. Не создают объектов и не трогают UI — только меняют состояние и счётчики.

	public bool GoToMainMenu()
	{
		return ChangeState(GameState.MainMenu);
	}

	public bool EnterTavern()
	{
		return ChangeState(GameState.Tavern);
	}

	// Назначь этот метод на OnClick у кнопки "Start Game" в инспекторе
	public void OnStartGameButtonClicked()
	{
		if (startGameButton != null)
		{
			startGameButton.gameObject.SetActive(false);
		}
		EnterTavern();
	}

	public bool StartRun()
	{
		CurrentLevel = 0;
		return ChangeState(GameState.RunInit);
	}

	public bool StartDealHand()
	{
		return ChangeState(GameState.DealHand);
	}

	public bool StartEnemyBattle(int level)
	{
		CurrentLevel = Mathf.Max(level, 1);
		return ChangeState(GameState.EnemyBattle);
	}

	public bool StartDragonBattle()
	{
		return ChangeState(GameState.DragonBattle);
	}

	public bool ChooseContinue()
	{
		return ChangeState(GameState.ChooseContinue);
	}

	public bool ExitRun()
	{
		var changed = ChangeState(GameState.ExitRun);
		if (changed)
		{
			CurrentDelveIndex = Mathf.Max(0, CurrentDelveIndex);
		}
		return changed;
	}

	public bool EnterGraveyard()
	{
		return ChangeState(GameState.Graveyard);
	}

	public bool ShowSummary()
	{
		return ChangeState(GameState.GameSummary);
	}

	public bool PauseGame()
	{
		if (CurrentState == GameState.Pause)
			return false;
		_stateBeforePause = CurrentState;
		return ChangeState(GameState.Pause);
	}

	public bool ResumeFromPause()
	{
		if (CurrentState != GameState.Pause)
			return false;
		return ChangeState(_stateBeforePause);
	}

	public bool NextLevel()
	{
		if (CurrentState != GameState.ChooseContinue && CurrentState != GameState.EnemyBattle)
			return false;
		CurrentLevel = Mathf.Max(1, CurrentLevel + 1);
		return ChangeState(GameState.EnemyBattle);
	}

	public bool NextDelveOrSummary(int totalDelves = 3)
	{
		// Вызывать после ExitRun/Graveyard в зависимости от исхода
		if (CurrentDelveIndex + 1 < totalDelves)
		{
			CurrentDelveIndex++;
			CurrentLevel = 0;
			return ChangeState(GameState.Tavern);
		}
		return ChangeState(GameState.GameSummary);
	}

	public bool ChangeState(GameState newState)
	{
		if (newState == CurrentState)
		{
			return false;
		}
		var previous = CurrentState;
		SetStateInternal(newState);
		Debug.Log($"[GameManager] State: {previous} → {newState}");
		OnStateChanged?.Invoke(previous, newState);
		return true;
	}

	private void SetStateInternal(GameState newState)
	{
		CurrentState = newState;
	}
}


