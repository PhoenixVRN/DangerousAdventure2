using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Sunduk/Dungeon Cards List", fileName = "DungeonCards")]
public class DungeonCardsConfig : ScriptableObject
{
	[System.Serializable]
	public class DungeonCardEntry
	{
		[Header("Identity")]
		public string id; // уникальный ключ (например, goblin_basic)
		public string displayName;
		[TextArea]
		public string description;

		[Header("Visuals")]
		public Sprite icon;
		[Tooltip("Фон карточки (рамка/бекграунд)")]
		public Sprite background;

		[Header("Gameplay")]
		public DungeonCardType cardType;
		[Tooltip("Особый тег/подтип, если потребуется в будущем (напр., Undead)")]
		public string tag;

		[Header("Effects")]
		[Tooltip("Количество сущностей этого типа, поражаемых при применении соответствующего класса (для справок/UI)")]
		public int typicalGroupSize = 0;
		[Tooltip("Даёт ли эта карта немедленный эффект вместо боя (напр., Зелье)")]
		public bool isInstantEffect;

		// Tuning weights removed — uniform random is used
	}

	[Header("Cards")]
	public List<DungeonCardEntry> cards = new List<DungeonCardEntry>();

	private Dictionary<string, DungeonCardEntry> _idToCard;

	private void OnEnable()
	{
		BuildIndex();
	}

	public void BuildIndex()
	{
		_idToCard = new Dictionary<string, DungeonCardEntry>();
		for (int i = 0; i < cards.Count; i++)
		{
			var entry = cards[i];
			if (string.IsNullOrEmpty(entry?.id))
				continue;
			_idToCard[entry.id] = entry;
		}
	}

	public bool TryGetById(string id, out DungeonCardEntry entry)
	{
		if (_idToCard == null)
			BuildIndex();
		return _idToCard.TryGetValue(id, out entry);
	}

	public DungeonCardEntry GetByIdOrNull(string id)
	{
		TryGetById(id, out var entry);
		return entry;
	}
}



