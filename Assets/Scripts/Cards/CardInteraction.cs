using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardInteraction : MonoBehaviour, IPointerClickHandler
{
	public static System.Action<CardInteraction> CardClicked;
	[SerializeField] private float scaleMultiplier = 1.2f;
	[SerializeField] private float tweenDuration = 0.12f;

	private CardDefinition _definition;
	private static CardInteraction _currentlySelected;
	private bool _selected = false;
	private Vector3 _baseScale;
	private Coroutine _tween;

	public static CardInteraction CurrentSelected => _currentlySelected;
	public CardDefinition Definition => _definition;

	private void Awake()
	{
		_definition = GetComponent<CardDefinition>();
		_baseScale = transform.localScale;
	}

	private void OnEnable()
	{
		_baseScale = transform.localScale;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (_definition == null)
			return;
		if (_definition.kind == CardKind.Adventurer)
		{
			if (_currentlySelected != null && _currentlySelected != this)
			{
				_currentlySelected.ForceDeselect(true);
			}
			if (_selected)
			{
				ForceDeselect(true);
			}
			else
			{
				SetSelected(true);
			}
		}
		else
		{
			// Dungeon card clicked — сообщаем системе боя
		}
		CardClicked?.Invoke(this);
	}

	private void SetSelected(bool value)
	{
		_selected = value;
		if (_selected)
			_currentlySelected = this;
		else if (_currentlySelected == this)
			_currentlySelected = null;
		Vector3 targetScale = _selected ? _baseScale * scaleMultiplier : _baseScale;
		StartTweenScale(targetScale);
	}

	public void ForceDeselect(bool animate)
	{
		if (!_selected && _currentlySelected != this)
			return;
		_selected = false;
		if (_currentlySelected == this)
			_currentlySelected = null;
		if (animate)
		{
			StartTweenScale(_baseScale);
		}
		else
		{
			if (_tween != null)
			{
				StopCoroutine(_tween);
				_tween = null;
			}
			transform.localScale = _baseScale;
		}
	}

	private void StartTweenScale(Vector3 targetScale)
	{
		if (_tween != null)
			StopCoroutine(_tween);
		_tween = StartCoroutine(TweenScaleRoutine(targetScale));
	}

	private IEnumerator TweenScaleRoutine(Vector3 targetScale)
	{
		float t = 0f;
		Vector3 startScale = transform.localScale;
		while (t < tweenDuration)
		{
			t += Time.unscaledDeltaTime;
			float k = Mathf.Clamp01(t / tweenDuration);
			transform.localScale = Vector3.Lerp(startScale, targetScale, k);
			yield return null;
		}
		transform.localScale = targetScale;
		_tween = null;
	}

	private void OnDisable()
	{
		if (_currentlySelected == this)
			_currentlySelected = null;
	}
}


