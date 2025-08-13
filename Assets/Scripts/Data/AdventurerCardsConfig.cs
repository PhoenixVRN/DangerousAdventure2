using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Sunduk/Adventurer Cards List", fileName = "AdventurerCards")]
public class AdventurerCardsConfig : ScriptableObject
{
	[System.Serializable]
	public class AdventurerCardEntry
	{
		[Header("Identity")]
		public string id; // уникальный ключ (например, warrior_basic)
		public string displayName;
		[TextArea]
		public string description;

		[Header("Visuals")]
		public Sprite icon;
		[Tooltip("Фон карточки (рамка/бекграунд)")]
		public Sprite background;

		[Header("Gameplay")]
		public AdventurerClass adventurerClass;
		[Tooltip("Приоритет применения (меньше — раньше). Нужен для авто‑подсказок и разрешения эффектов.")]
		public int priority = 0;
		[Tooltip("Одноразовая ли карта при применении (обычно да, у Свитка тоже да)")]
		public bool discardAfterUse = true;

		[Header("Combat Rules")]
		[Tooltip("Режим точечного убийства: None, SpecificType (killsSingleType), Any (для Вора)")]
		public SingleKillMode singleKillMode = SingleKillMode.None;
		[Tooltip("Тип, против которого карта убивает ровно 1 цель, если выбран режим SpecificType")]
		public DungeonCardType killsSingleType = DungeonCardType.None;
		[Tooltip("Список типов подземелья, которых эта карта убивает всех в ряду (AoE по типу). Например: Воин — Goblin; Клирик — Skeleton; Маг — Spirit; Паладин — по дизайну.")]
		public DungeonCardType[] killsAllOfTypes;

		// Tuning weights removed — uniform random is used
	}

	[Header("Cards")]
	public List<AdventurerCardEntry> cards = new List<AdventurerCardEntry>();

	private Dictionary<string, AdventurerCardEntry> _idToCard;

	private void OnEnable()
	{
		BuildIndex();
	}

	public void BuildIndex()
	{
		_idToCard = new Dictionary<string, AdventurerCardEntry>();
		for (int i = 0; i < cards.Count; i++)
		{
			var entry = cards[i];
			if (string.IsNullOrEmpty(entry?.id))
				continue;
			_idToCard[entry.id] = entry;
		}
	}

	public bool TryGetById(string id, out AdventurerCardEntry entry)
	{
		if (_idToCard == null)
			BuildIndex();
		return _idToCard.TryGetValue(id, out entry);
	}

	public AdventurerCardEntry GetByIdOrNull(string id)
	{
		TryGetById(id, out var entry);
		return entry;
	}
}



