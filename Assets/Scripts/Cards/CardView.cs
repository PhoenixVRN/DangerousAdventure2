using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardView : MonoBehaviour
{
	[SerializeField] private RectTransform contentRoot;
	[SerializeField] private Image backgroundImage;
	[SerializeField] private Image iconImage;
	[SerializeField] private TMP_Text titleText;

	private CardDefinition _definition;

	public RectTransform ContentRoot => contentRoot;

	public void Bind(CardDefinition definition)
	{
		_definition = definition;
		Refresh();
	}

	public void Refresh()
	{
		if (_definition == null)
			return;
		if (backgroundImage != null)
			backgroundImage.sprite = _definition.backgroundSprite;
		if (iconImage != null)
			iconImage.sprite = _definition.icon;
		if (titleText != null)
			titleText.text = _definition.displayName;
	}
}


