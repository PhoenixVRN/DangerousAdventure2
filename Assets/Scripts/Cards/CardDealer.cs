using System.Collections.Generic;
using UnityEngine;

public class CardDealer : MonoBehaviour
{
	[SerializeField] private CardFactory factory;
	[SerializeField] private AdventurerCardsConfig adventurerCards;
	[SerializeField] private DungeonCardsConfig dungeonCards;
	[SerializeField] private Transform dragonParent; // отдельный контейнер для драконов
	[SerializeField] private DragonCounter dragonCounter;

	[SerializeField] private int adventurerDealCount = 5;
	[SerializeField] private int dungeonDealCount = 5;
	[SerializeField] private bool clearBeforeDeal = true;
	[SerializeField] private bool autoDealOnStart = true;
	// чистый рандом без ограничений на дубликаты

	private void Start()
	{
		if (autoDealOnStart)
		{
			DealTest();
		}
	}

	public void DealTest()
	{
		DealRandom();
	}

	public void DealRandom()
	{
		if (factory == null)
		{
			Debug.LogError("CardDealer: CardFactory is not assigned");
			return;
		}
		if (adventurerCards == null || dungeonCards == null)
		{
			Debug.LogError("CardDealer: Cards configs are not assigned");
			return;
		}

		if (clearBeforeDeal)
		{
			ClearChildren(GetAdventurerParent());
			ClearChildren(GetDungeonParent());
		}

		for (int i = 0; i < adventurerDealCount; i++)
		{
			var a = WeightedPick(adventurerCards.cards);
			if (a != null)
			{
				factory.SpawnAdventurer(a.id);
			}
		}
		int dragons = 0;
		for (int i = 0; i < dungeonDealCount; i++)
		{
			var d = WeightedPick(dungeonCards.cards);
			if (d != null)
			{
				if (d.cardType == DungeonCardType.Dragon && dragonParent != null)
				{
					factory.SpawnDungeon(d.id, dragonParent);
					dragons++;
				}
				else
				{
					factory.SpawnDungeon(d.id);
				}
			}
		}
		if (dragonCounter != null)
			dragonCounter.ResetCount();
		if (dragonCounter != null)
			dragonCounter.Increment(); // set at least once, then add (below)
		if (dragonCounter != null)
		{
			// корректно выставим итоговое значение
			dragonCounter.CurrentCount.ToString(); // noop to avoid warning
			for (int i = 1; i < dragons; i++) dragonCounter.Increment();
		}
	}

	public void DealAdventurersExact(int count, bool clear)
	{
		if (factory == null || adventurerCards == null)
			return;
		if (clear) ClearChildren(GetAdventurerParent());
		for (int i = 0; i < count; i++)
		{
			var a = WeightedPick(adventurerCards.cards);
			if (a != null) factory.SpawnAdventurer(a.id);
		}
	}

	public void DealDungeonExact(int count, bool clear)
	{
		if (factory == null || dungeonCards == null)
			return;
		if (clear) ClearChildren(GetDungeonParent());
		int dragons = 0;
		for (int i = 0; i < count; i++)
		{
			var d = WeightedPick(dungeonCards.cards);
			if (d == null) continue;
			if (d.cardType == DungeonCardType.Dragon && dragonParent != null)
			{
				factory.SpawnDungeon(d.id, dragonParent);
				dragons++;
			}
			else
			{
				factory.SpawnDungeon(d.id);
			}
		}
		if (dragonCounter != null)
		{
			dragonCounter.ResetCount();
			for (int i = 0; i < dragons; i++) dragonCounter.Increment();
		}
	}

	private Transform GetAdventurerParent()
	{
		var type = factory.GetType();
		var field = type.GetField("adventurerParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		return field != null ? (Transform)field.GetValue(factory) : null;
	}

	private Transform GetDungeonParent()
	{
		var type = factory.GetType();
		var field = type.GetField("dungeonParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		return field != null ? (Transform)field.GetValue(factory) : null;
	}

	private void ClearChildren(Transform parent)
	{
		if (parent == null)
			return;
		var toDestroy = new List<GameObject>();
		for (int i = parent.childCount - 1; i >= 0; i--)
		{
			var child = parent.GetChild(i);
			toDestroy.Add(child.gameObject);
		}
		for (int i = 0; i < toDestroy.Count; i++)
		{
			var go = toDestroy[i];
			if (Application.isPlaying)
				Object.Destroy(go);
			else
				Object.DestroyImmediate(go);
		}
	}

	private AdventurerCardsConfig.AdventurerCardEntry WeightedPick(List<AdventurerCardsConfig.AdventurerCardEntry> list)
	{
		if (list == null || list.Count == 0)
			return null;
		// Uniform random among non-null entries
		var candidates = new List<int>();
		for (int i = 0; i < list.Count; i++) if (list[i] != null) candidates.Add(i);
		if (candidates.Count == 0) return null;
		int idx = candidates[Random.Range(0, candidates.Count)];
		return list[idx];
	}

    // Removed unique pick for adventurers; keeping file clean

	private DungeonCardsConfig.DungeonCardEntry WeightedPick(List<DungeonCardsConfig.DungeonCardEntry> list)
	{
		if (list == null || list.Count == 0)
			return null;
		// Uniform random among non-null entries
		var candidates = new List<int>();
		for (int i = 0; i < list.Count; i++) if (list[i] != null) candidates.Add(i);
		if (candidates.Count == 0) return null;
		int idx = candidates[Random.Range(0, candidates.Count)];
		return list[idx];
	}

}


