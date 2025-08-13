using UnityEngine;

public class CardFactory : MonoBehaviour
{
	[SerializeField] private GameObject cardPrefab;
	[SerializeField] private Transform defaultParent;
	[SerializeField] private Transform adventurerParent;
	[SerializeField] private Transform dungeonParent;
	[SerializeField] private AdventurerCardsConfig adventurerCards;
	[SerializeField] private DungeonCardsConfig dungeonCards;
	[SerializeField] private float fallbackSpacing = 3f;

	private int _noParentAdventurerCount = 0;
	private int _noParentDungeonCount = 0;

	public GameObject SpawnAdventurer(string id, Transform parent = null)
	{
		if (adventurerCards == null)
		{
			Debug.LogError("AdventurerCardsConfig not assigned in CardFactory");
			return null;
		}
		if (!adventurerCards.TryGetById(id, out var data))
		{
			Debug.LogError($"Adventurer card id not found: {id}");
			return null;
		}
		return CreateFromAdventurer(data, parent);
	}

	public GameObject SpawnDungeon(string id, Transform parent = null)
	{
		if (dungeonCards == null)
		{
			Debug.LogError("DungeonCardsConfig not assigned in CardFactory");
			return null;
		}
		if (!dungeonCards.TryGetById(id, out var data))
		{
			Debug.LogError($"Dungeon card id not found: {id}");
			return null;
		}
		return CreateFromDungeon(data, parent);
	}

	private GameObject CreateFromAdventurer(AdventurerCardsConfig.AdventurerCardEntry data, Transform parent)
	{
		var chosenParent = parent != null ? parent : (adventurerParent != null ? adventurerParent : defaultParent);
		int childIndexBefore = chosenParent != null ? chosenParent.childCount : _noParentAdventurerCount;
		var go = Instantiate(cardPrefab, chosenParent);
		var def = go.GetComponent<CardDefinition>();
		var view = go.GetComponent<CardView>();
		if (def == null || view == null)
		{
			Debug.LogError("Card prefab must contain CardDefinition and CardView components");
			return go;
		}
		def.id = data.id;
		def.displayName = data.displayName;
		def.kind = CardKind.Adventurer;
		def.icon = data.icon;
		def.backgroundSprite = data.background;
		def.adventurerData = data;
		def.dungeonData = null;
		view.Bind(def);
		ApplyLayoutOrFallback(chosenParent, go.transform, childIndexBefore, true);
		return go;
	}

	private GameObject CreateFromDungeon(DungeonCardsConfig.DungeonCardEntry data, Transform parent)
	{
		var chosenParent = parent != null ? parent : (dungeonParent != null ? dungeonParent : defaultParent);
		int childIndexBefore = chosenParent != null ? chosenParent.childCount : _noParentDungeonCount;
		var go = Instantiate(cardPrefab, chosenParent);
		var def = go.GetComponent<CardDefinition>();
		var view = go.GetComponent<CardView>();
		if (def == null || view == null)
		{
			Debug.LogError("Card prefab must contain CardDefinition and CardView components");
			return go;
		}
		def.id = data.id;
		def.displayName = data.displayName;
		def.kind = CardKind.Dungeon;
		def.icon = data.icon;
		def.backgroundSprite = data.background;
		def.adventurerData = null;
		def.dungeonData = data;
		view.Bind(def);
		ApplyLayoutOrFallback(chosenParent, go.transform, childIndexBefore, false);
		return go;
	}

	private void ApplyLayoutOrFallback(Transform parent, Transform spawned, int indexBefore, bool isAdventurer)
	{
		if (parent == null || spawned == null)
		{
			// No parent: place in world with incremental offset
			int idx = indexBefore;
			if (isAdventurer)
				_noParentAdventurerCount = idx + 1;
			else
				_noParentDungeonCount = idx + 1;
			var wp = spawned.position;
			wp.x = idx * fallbackSpacing;
			spawned.position = wp;
			return;
		}
		var layout = parent.GetComponent<HorizontalCardLayout>();
		if (layout != null)
		{
			layout.Rebuild();
			return;
		}
		// Fallback: simple left-to-right by index
		int newIndex = indexBefore; // spawned is appended at the end
		var p = spawned.localPosition;
		p.x = newIndex * fallbackSpacing;
		p.y = 0f;
		p.z = 0f;
		spawned.localPosition = p;
	}
}


