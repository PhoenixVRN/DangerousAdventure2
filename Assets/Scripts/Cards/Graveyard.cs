using System.Collections.Generic;
using UnityEngine;

public class Graveyard : MonoBehaviour
{
	[Header("Runtime store of fallen adventurers (by id)")]
	public List<string> adventurerIds = new List<string>();

	[Header("Optional debug: keep last removed GameObjects")]
	public int keepLastObjects = 0;
	public List<GameObject> lastRemovedObjects = new List<GameObject>();

	public void AddAdventurer(CardDefinition adventurer)
	{
		if (adventurer == null || adventurer.adventurerData == null)
			return;
		adventurerIds.Add(adventurer.adventurerData.id);
		if (keepLastObjects > 0)
		{
			lastRemovedObjects.Add(adventurer.gameObject);
			while (lastRemovedObjects.Count > keepLastObjects)
			{
				lastRemovedObjects.RemoveAt(0);
			}
		}
	}

	public bool TryPopLast(out string id)
	{
		id = null;
		if (adventurerIds == null || adventurerIds.Count == 0)
			return false;
		int last = adventurerIds.Count - 1;
		id = adventurerIds[last];
		adventurerIds.RemoveAt(last);
		return !string.IsNullOrEmpty(id);
	}

	public bool TryPopByClass(AdventurerClass adventurerClass, AdventurerCardsConfig config, out string id)
	{
		id = null;
		if (adventurerIds == null || adventurerIds.Count == 0 || config == null)
			return false;
		for (int i = adventurerIds.Count - 1; i >= 0; i--)
		{
			var candidateId = adventurerIds[i];
			var entry = config.GetByIdOrNull(candidateId);
			if (entry != null && entry.adventurerClass == adventurerClass)
			{
				adventurerIds.RemoveAt(i);
				id = candidateId;
				return true;
			}
		}
		return false;
	}
}


