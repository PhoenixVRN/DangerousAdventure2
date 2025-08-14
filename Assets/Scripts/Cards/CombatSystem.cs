using System.Collections.Generic;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    // removed duplicate field
	[SerializeField] private Transform dungeonParent;
	[SerializeField] private Graveyard graveyard;
	[SerializeField] private RoundManager roundManager;
	[SerializeField] private GoldManager goldManager;
	[SerializeField] private CardFactory factory;
	[SerializeField] private Transform adventurerParent;
	[SerializeField] private ResurrectionPanelController resurrectionPanel;

	// Запоминаем какие классы уже убивали драконов в текущей битве с драконами
	private readonly HashSet<AdventurerClass> _dragonKillersUsed = new HashSet<AdventurerClass>();

	private void OnEnable()
	{
		CardInteraction.CardClicked += OnCardClicked;
		if (GameManager.Instance != null)
			GameManager.Instance.OnStateChanged += OnGameStateChanged;
	}

	private void OnDisable()
	{
		CardInteraction.CardClicked -= OnCardClicked;
		if (GameManager.Instance != null)
			GameManager.Instance.OnStateChanged -= OnGameStateChanged;
	}

	private void OnGameStateChanged(GameManager.GameState previous, GameManager.GameState next)
	{
		if (next == GameManager.GameState.DragonBattle)
		{
			_dragonKillersUsed.Clear();
		}
		else if (previous == GameManager.GameState.DragonBattle && next != GameManager.GameState.DragonBattle)
		{
			_dragonKillersUsed.Clear();
		}
	}

	private void OnCardClicked(CardInteraction interaction)
	{
		if (interaction == null)
			return;
		var def = interaction.Definition;
		if (def == null)
			return;

		// Логика: если щёлкнули по врагу при выбранном солдате — выполняем атаку
		if (def.kind == CardKind.Dungeon)
		{
			var selected = CardInteraction.CurrentSelected;
			if (selected == null)
				return;
			var attacker = selected.Definition;
			if (attacker == null || attacker.kind != CardKind.Adventurer)
				return;

			// Если активирован режим свитка — позволяем выбирать СВОИХ или ВРАГОВ (кроме драконов)
			if (RerollController.IsActive)
			{
				RerollController.ToggleSelection(def);
				// Принудительно держим выделение свитка, чтобы не закрывался попап
				return;
			}

			// Ограничение драконов в режиме DragonBattle: каждый класс может убить только одного дракона
			if (def.dungeonData != null && def.dungeonData.cardType == DungeonCardType.Dragon)
			{
				if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.DragonBattle)
				{
					var cls = attacker.adventurerData != null ? attacker.adventurerData.adventurerClass : AdventurerClass.Warrior;
					if (_dragonKillersUsed.Contains(cls))
					{
						Debug.Log($"[Combat] DragonBattle rule: class {cls} already used to kill a dragon.");
						return;
					}
				}
			}

			// Блокируем атаку сундуков, если на поле есть другие враги (кроме драконов)
			if (def.dungeonData != null && def.dungeonData.cardType == DungeonCardType.Chest)
			{
				if (HasNonDragonEnemies())
				{
					Debug.Log("[Combat] Chest is locked until all non-dragon enemies are cleared.");
					return;
				}
			}
			// Если цель сундук — начисляем награду (round * 1.5), затем выполняем разрушение
			if (def.dungeonData != null && def.dungeonData.cardType == DungeonCardType.Chest)
			{
				int baseValue = Mathf.Max(1, roundManager != null ? roundManager.CurrentRound : 1);
				int reward = Mathf.RoundToInt(baseValue * 1.5f);
				goldManager?.AddGold(reward);
			}
			bool targetIsDragon = def.dungeonData != null && def.dungeonData.cardType == DungeonCardType.Dragon;
			var usedClass = attacker.adventurerData != null ? attacker.adventurerData.adventurerClass : AdventurerClass.Warrior;
			ResolveAttack(attacker, def);
			SendAdventurerToGraveyard(selected);
			selected.ForceDeselect(true);
			if (targetIsDragon && GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.DragonBattle)
			{
				_dragonKillersUsed.Add(usedClass);
			}
			// Сообщаем RoundManager'у после уничтожения объектов, на следующий кадр
			if (roundManager != null)
			{
				StartCoroutine(NotifyClearedNextFrame());
			}
		}
	}

	private void Update()
	{
		// Условие поражения в режиме боя с драконами: героев не осталось (кроме Свитка), а драконы ещё есть
		if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.DragonBattle)
		{
			var adventurersLeft = CountAdventurersExcludingScroll();
			bool dragonsRemain = AnyDragonsOnField();
			if (adventurersLeft == 0 && dragonsRemain)
			{
				GameManager.Instance.EnterTavern();
				// показать тост "вы проиграли" на 3 секунды
				ToastController.Instance?.Show("Вы проиграли", 3f);
			}
		}
	}

	private int CountAdventurersExcludingScroll()
	{
		if (adventurerParent == null) return 0;
		int count = 0;
		for (int i = 0; i < adventurerParent.childCount; i++)
		{
			var def = adventurerParent.GetChild(i).GetComponent<CardDefinition>();
			if (def == null || def.adventurerData == null) continue;
			if (def.adventurerData.adventurerClass == AdventurerClass.Scroll) continue;
			count++;
		}
		return count;
	}

	private bool AnyDragonsOnField()
	{
		if (dungeonParent == null) return false;
		for (int i = 0; i < dungeonParent.childCount; i++)
		{
			var def = dungeonParent.GetChild(i).GetComponent<CardDefinition>();
			if (def == null || def.dungeonData == null) continue;
			if (def.dungeonData.cardType == DungeonCardType.Dragon)
				return true;
		}
		return false;
	}

	private void ResolveAttack(CardDefinition attacker, CardDefinition target)
	{
		var a = attacker.adventurerData;
		var d = target.dungeonData;
		if (a == null || d == null)
			return;

		bool killAllOfType = false;
		if (a.killsAllOfTypes != null)
		{
			for (int i = 0; i < a.killsAllOfTypes.Length; i++)
			{
				if (a.killsAllOfTypes[i] == d.cardType)
				{
					killAllOfType = true;
					break;
				}
			}
		}

		if (killAllOfType)
		{
			KillAllOfType(d.cardType);
		}
		else
		{
			KillSingle(target);
		}

		// Пьём зелья ТОЛЬКО если целью был поушен
		if (d.cardType == DungeonCardType.Potion)
		{
			TryDrinkAllPotions(attacker);
		}
	}

	private void TryDrinkAllPotions(CardDefinition usingAdventurer)
	{
		if (dungeonParent == null) return;
		// Считаем количество зелий на поле
		int potions = 0;
		for (int i = 0; i < dungeonParent.childCount; i++)
		{
			var def = dungeonParent.GetChild(i).GetComponent<CardDefinition>();
			if (def != null && def.dungeonData != null && def.dungeonData.cardType == DungeonCardType.Potion)
			{
				potions++;
			}
		}
		if (potions <= 0) return;

		// Уничтожаем все зелья
		var toDestroy = new List<GameObject>();
		for (int i = 0; i < dungeonParent.childCount; i++)
		{
			var child = dungeonParent.GetChild(i);
			var def = child.GetComponent<CardDefinition>();
			if (def != null && def.dungeonData != null && def.dungeonData.cardType == DungeonCardType.Potion)
				toDestroy.Add(child.gameObject);
		}
		DestroyList(toDestroy);

		// Использованный приключенец уходит на кладбище (сделает внешний код), а тут — открываем панель выбора
		if (resurrectionPanel != null)
		{
			resurrectionPanel.Open(potions);
		}
	}

	private bool HasNonDragonEnemies()
	{
		if (dungeonParent == null)
			return false;
		for (int i = 0; i < dungeonParent.childCount; i++)
		{
			var child = dungeonParent.GetChild(i);
			var def = child.GetComponent<CardDefinition>();
			if (def == null || def.dungeonData == null)
				continue;
			var t = def.dungeonData.cardType;
			// Поушены не блокируют сундук
			if (t != DungeonCardType.Chest && t != DungeonCardType.Dragon && t != DungeonCardType.Potion)
				return true;
		}
		return false;
	}

	private void SendAdventurerToGraveyard(CardInteraction selected)
	{
		if (selected == null) return;
		var def = selected.Definition;
		if (def == null || def.adventurerData == null) return;
		graveyard?.AddAdventurer(def);
		DestroyOne(selected.gameObject);
	}

	private void KillAllOfType(DungeonCardType type)
	{
		if (dungeonParent == null)
			return;
		var toDestroy = new List<GameObject>();
		for (int i = 0; i < dungeonParent.childCount; i++)
		{
			var child = dungeonParent.GetChild(i);
			var def = child.GetComponent<CardDefinition>();
			if (def != null && def.dungeonData != null && def.dungeonData.cardType == type)
			{
				toDestroy.Add(child.gameObject);
			}
		}
		DestroyList(toDestroy);
	}

	private void KillSingle(CardDefinition target)
	{
		if (target == null)
			return;
		DestroyOne(target.gameObject);
	}

	private void DestroyOne(GameObject go)
	{
		if (go == null) return;
		if (Application.isPlaying) Destroy(go); else DestroyImmediate(go);
	}

	private void DestroyList(List<GameObject> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			DestroyOne(list[i]);
		}
	}

	private System.Collections.IEnumerator NotifyClearedNextFrame()
	{
		yield return null; // дождаться конца кадра, чтобы Destroy() применился
		roundManager?.OnEnemyCleared();
	}
}


