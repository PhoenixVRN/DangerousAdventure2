using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardInfoPopup : MonoBehaviour
{
	[SerializeField] private GameObject panelRoot;
	[SerializeField] private TMP_Text titleText;
	[SerializeField] private TMP_Text descriptionText;
	[SerializeField] private Image iconImage;
	[SerializeField] private Image backgroundImage;

	private void OnEnable()
	{
		CardInteraction.AdventurerSelected += OnAdventurerSelected;
	}

	private void OnDisable()
	{
		CardInteraction.AdventurerSelected -= OnAdventurerSelected;
	}

	private void OnAdventurerSelected(CardDefinition def, bool isSelected)
	{
		if (isSelected)
		{
			Show(def);
		}
		else
		{
			Hide();
		}
	}

	public void Show(CardDefinition def)
	{
		if (panelRoot != null) panelRoot.SetActive(true);
		if (def != null && def.adventurerData != null)
		{
			if (titleText != null) titleText.text = def.displayName;
			if (descriptionText != null) descriptionText.text = def.adventurerData.description;
			if (iconImage != null) iconImage.sprite = def.icon;
			if (backgroundImage != null) backgroundImage.sprite = def.backgroundSprite;
		}
	}

	public void Hide()
	{
		if (panelRoot != null) panelRoot.SetActive(false);
	}
}


