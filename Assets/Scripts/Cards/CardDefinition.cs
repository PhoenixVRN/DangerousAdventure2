using UnityEngine;

public class CardDefinition : MonoBehaviour
{
	[Header("Identity")]
	public string id;
	public string displayName;

	[Header("Kind")]
	public CardKind kind;

	[Header("Visuals")]
	public Sprite icon;
	public Sprite backgroundSprite;

	[Header("Data Links")]
	public AdventurerCardsConfig.AdventurerCardEntry adventurerData;
	public DungeonCardsConfig.DungeonCardEntry dungeonData;
}


