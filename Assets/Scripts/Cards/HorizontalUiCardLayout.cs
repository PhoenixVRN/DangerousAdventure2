using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class HorizontalUiCardLayout : MonoBehaviour
{
	[SerializeField] private float spacing = 16f;
	[SerializeField] private RectOffset padding;
	[SerializeField] private TextAnchor childAlignment = TextAnchor.MiddleCenter;
	[SerializeField] private bool controlChildSize = false;
	[SerializeField] private Vector2 childSize = new Vector2(300, 400);

	private HorizontalLayoutGroup _group;

	private void OnEnable()
	{
		Ensure();
	}

	private void OnValidate()
	{
		Ensure();
	}

	private void Ensure()
	{
		if (padding == null)
		{
			padding = new RectOffset(8, 8, 8, 8);
		}
		if (_group == null)
		{
			_group = GetComponent<HorizontalLayoutGroup>();
			if (_group == null)
				_group = gameObject.AddComponent<HorizontalLayoutGroup>();
		}
		_group.spacing = spacing;
		_group.padding = padding;
		_group.childAlignment = childAlignment;
		_group.childControlWidth = controlChildSize;
		_group.childControlHeight = controlChildSize;
	}

	public void ApplyChildSize(LayoutElement element)
	{
		if (element == null)
			return;
		if (!controlChildSize)
			return;
		element.preferredWidth = childSize.x;
		element.preferredHeight = childSize.y;
	}
}


