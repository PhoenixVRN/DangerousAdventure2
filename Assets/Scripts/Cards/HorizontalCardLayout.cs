using System.Collections.Generic;
using UnityEngine;

public class HorizontalCardLayout : MonoBehaviour
{
	public enum HorizontalAlignment
	{
		Left,
		Center,
		Right
	}

	[SerializeField] private float spacing = 1.5f;
	[SerializeField] private HorizontalAlignment alignment = HorizontalAlignment.Center;
	[SerializeField] private Vector3 origin = Vector3.zero;
	[SerializeField] private bool autoRebuildOnEnable = true;
	[SerializeField] private bool autoRebuildOnChildrenChanged = true;

	private readonly List<Transform> _children = new List<Transform>();

	private void OnEnable()
	{
		if (autoRebuildOnEnable)
			Rebuild();
	}

	private void OnTransformChildrenChanged()
	{
		if (autoRebuildOnChildrenChanged)
			Rebuild();
	}

	[ContextMenu("Rebuild Layout")]
	public void Rebuild()
	{
		_children.Clear();
		for (int i = 0; i < transform.childCount; i++)
		{
			var child = transform.GetChild(i);
			if (child.gameObject.activeSelf)
				_children.Add(child);
		}

		int count = _children.Count;
		if (count == 0)
			return;

		float totalWidth = spacing * (count - 1);
		float startX;
		switch (alignment)
		{
			case HorizontalAlignment.Left:
				startX = origin.x;
				break;
			case HorizontalAlignment.Right:
				startX = origin.x - totalWidth;
				break;
			case HorizontalAlignment.Center:
			default:
				startX = origin.x - totalWidth * 0.5f;
				break;
		}

		for (int i = 0; i < count; i++)
		{
			var child = _children[i];
			var p = child.localPosition;
			p.x = startX + i * spacing;
			p.y = origin.y;
			p.z = origin.z;
			child.localPosition = p;
		}
	}

	private void OnValidate()
	{
		if (!Application.isPlaying)
		{
			Rebuild();
		}
	}
}


