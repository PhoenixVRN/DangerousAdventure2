using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RerollController : MonoBehaviour
{
	public static RerollController Instance { get; private set; }

	[SerializeField] private Button rerollButton;
	[SerializeField] private CardFactory factory;
	[SerializeField] private AdventurerCardsConfig adventurerConfig;
	[SerializeField] private DungeonCardsConfig dungeonConfig;
	[SerializeField] private CardDealer dealer; // для dragonParent
	[SerializeField] private Graveyard graveyard; // куда отправляем использованный свиток
	[SerializeField] private RoundManager roundManager; // чтобы переоценить состояние после реролла

	private readonly HashSet<CardDefinition> _selectedForReroll = new HashSet<CardDefinition>();
	private readonly Dictionary<Transform, Vector3> _originalScales = new Dictionary<Transform, Vector3>();
	private CardDefinition _currentScroll;

	private void Awake()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		CardInteraction.AdventurerSelected += OnAdventurerSelected;
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnStateChanged += OnStateChanged;
		}
		if (rerollButton != null) rerollButton.gameObject.SetActive(false);
		UpdateRerollButton();
	}

	private void OnDisable()
	{
		CardInteraction.AdventurerSelected -= OnAdventurerSelected;
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnStateChanged -= OnStateChanged;
		}
		if (Instance == this) Instance = null;
	}

	private void OnStateChanged(GameManager.GameState previous, GameManager.GameState next)
	{
		if (next == GameManager.GameState.DragonBattle)
		{
			// Принудительно выходим из режима свитка при входе в бой с драконами
			ExitMode();
			if (rerollButton != null) rerollButton.gameObject.SetActive(false);
		}
	}

	public static bool IsActive => Instance != null && Instance._currentScroll != null;

	public static bool IsScroll(CardDefinition def)
	{
		return def != null && def.adventurerData != null && def.adventurerData.adventurerClass == AdventurerClass.Scroll;
	}

	private void OnAdventurerSelected(CardDefinition def, bool isSelected)
	{
		if (!IsScroll(def))
			return;
		if (isSelected)
		{
			EnterMode(def);
		}
		else
		{
			ExitMode();
		}
	}

	private void EnterMode(CardDefinition scroll)
	{
		_currentScroll = scroll;
		_selectedForReroll.Clear();
		ClearVisuals();
		UpdateRerollButton();
	}

	private void ExitMode()
	{
		_currentScroll = null;
		_selectedForReroll.Clear();
		ClearVisuals();
		UpdateRerollButton();
	}

	public static void ToggleSelection(CardDefinition def)
	{
		if (!IsActive || def == null)
			return;
		if (def.dungeonData != null && def.dungeonData.cardType == DungeonCardType.Dragon)
			return; // нельзя выбирать драконов
		if (IsScroll(def))
			return; // сам свиток не рероллим
		if (Instance._selectedForReroll.Contains(def))
		{
			Instance._selectedForReroll.Remove(def);
			Instance.Unmark(def);
		}
		else
		{
			Instance._selectedForReroll.Add(def);
			Instance.Mark(def);
		}
		Instance.UpdateRerollButton();
	}

	private void Mark(CardDefinition def)
	{
		if (def == null) return;
		var t = def.transform;
		if (!_originalScales.ContainsKey(t))
			_originalScales[t] = t.localScale;
		t.localScale = _originalScales[t] * 1.1f;
	}

	private void Unmark(CardDefinition def)
	{
		if (def == null) return;
		var t = def.transform;
		if (_originalScales.TryGetValue(t, out var s))
		{
			t.localScale = s;
			_originalScales.Remove(t);
		}
	}

	private void ClearVisuals()
	{
		foreach (var def in _selectedForReroll)
		{
			Unmark(def);
		}
		_selectedForReroll.Clear();
	}

	private void UpdateRerollButton()
	{
		if (rerollButton != null)
		{
			bool enabled = _selectedForReroll.Count > 0;
			rerollButton.interactable = enabled;
			rerollButton.gameObject.SetActive(enabled);
		}
	}

	public void OnRerollButtonClicked()
	{
		if (!IsActive || factory == null || adventurerConfig == null || dungeonConfig == null)
			return;
		var scroll = _currentScroll; // сохранить ссылку до очистки режима
		var toProcess = new List<CardDefinition>(_selectedForReroll);
		ClearVisuals();
		for (int i = 0; i < toProcess.Count; i++)
		{
			RerollOne(toProcess[i]);
		}
		// После реролла — отправляем свиток в кладбище и удаляем его с поля
		if (scroll != null)
		{
			var interact = scroll.GetComponent<CardInteraction>();
			if (interact != null) interact.ForceDeselect(true);
			graveyard?.AddAdventurer(scroll);
			DestroyOne(scroll.gameObject);
		}
		// Сбросить режим и UI
		ExitMode();
		// Переоценить состояние стола (покажет Next Round при сундуках/поушенах)
		roundManager?.OnEnemyCleared();
	}

	private void RerollOne(CardDefinition def)
	{
		if (def == null) return;
		var parent = def.transform.parent;
		int siblingIndex = def.transform.GetSiblingIndex();
		bool isAdventurer = def.kind == CardKind.Adventurer;
		DestroyOne(def.gameObject);
		if (isAdventurer)
		{
			var entry = PickRandom(adventurerConfig.cards);
			if (entry != null)
			{
				var go = factory.SpawnAdventurer(entry.id, parent);
				if (go != null) go.transform.SetSiblingIndex(siblingIndex);
			}
		}
		else
		{
			var entry = PickRandom(dungeonConfig.cards);
			if (entry != null)
			{
				Transform targetParent = parent;
				if (entry.cardType == DungeonCardType.Dragon && dealer != null)
				{
					var field = typeof(CardDealer).GetField("dragonParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
					var dragonParent = field != null ? (Transform)field.GetValue(dealer) : null;
					if (dragonParent != null) targetParent = dragonParent;
				}
				var go = factory.SpawnDungeon(entry.id, targetParent);
				if (go != null && targetParent == parent)
					go.transform.SetSiblingIndex(siblingIndex);
			}
		}
	}

	private static T PickRandom<T>(List<T> list) where T : class
	{
		if (list == null || list.Count == 0) return null;
		int idx = Random.Range(0, list.Count);
		return list[idx];
	}

	private void DestroyOne(GameObject go)
	{
		if (go == null) return;
		if (Application.isPlaying) Destroy(go); else DestroyImmediate(go);
	}
}


