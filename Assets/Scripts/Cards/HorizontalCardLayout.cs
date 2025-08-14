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
	[SerializeField] private bool autoScaleToFitParentWidth = true;
	[SerializeField] private float minScale = 0.5f;

	private readonly List<Transform> _children = new List<Transform>();
	private readonly Dictionary<Transform, Vector3> _baseScales = new Dictionary<Transform, Vector3>();

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

		// Запомним базовые масштабы для корректного пересчёта
		for (int i = 0; i < count; i++)
		{
			var t = _children[i];
			if (!_baseScales.ContainsKey(t))
				_baseScales[t] = t.localScale;
		}

		float totalWidth = spacing * (count - 1);
		float childWidth = EstimateChildWidth();
		if (childWidth > 0f)
		{
			totalWidth += childWidth * count;
		}

		float scaleFactor = 1f;
		if (autoScaleToFitParentWidth)
		{
			float parentWidth = EstimateParentWidth();
			if (parentWidth > 0f && totalWidth > 0f)
			{
				scaleFactor = Mathf.Clamp(parentWidth / totalWidth, minScale, 1f);
			}
		}
		float startX;
		switch (alignment)
		{
			case HorizontalAlignment.Left:
				startX = origin.x;
				break;
			case HorizontalAlignment.Right:
				startX = origin.x - (totalWidth * scaleFactor);
				break;
			case HorizontalAlignment.Center:
			default:
				startX = origin.x - (totalWidth * 0.5f * scaleFactor);
				break;
		}

		for (int i = 0; i < count; i++)
		{
			var child = _children[i];
			// Применяем масштаб относительно базового
			if (_baseScales.TryGetValue(child, out var baseScale))
			{
				child.localScale = baseScale * scaleFactor;
			}
			var p = child.localPosition;
			p.x = startX + i * spacing * scaleFactor + (childWidth > 0f ? i * childWidth * scaleFactor : 0f);
			p.y = origin.y;
			p.z = origin.z;
			child.localPosition = p;
		}
	}

	private float EstimateParentWidth()
	{
		var rt = transform as RectTransform;
		if (rt != null)
			return Mathf.Abs(rt.rect.width);
		return 0f;
	}

	private float EstimateChildWidth()
	{
		// Возьмём максимальную ширину среди детей по RectTransform
		float max = 0f;
		for (int i = 0; i < _children.Count; i++)
		{
			var t = _children[i];
			var rt = t as RectTransform;
			float w = 0f;
			if (rt != null)
			{
				w = Mathf.Abs(rt.rect.width);
			}
			max = Mathf.Max(max, w);
		}
		return max;
	}

	private void OnValidate()
	{
		if (!Application.isPlaying)
		{
			Rebuild();
		}
	}
}


