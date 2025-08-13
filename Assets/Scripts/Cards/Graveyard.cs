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
}


