using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardInteraction : MonoBehaviour, IPointerClickHandler
{
	public static System.Action<CardInteraction> CardClicked;
	public static System.Action<CardDefinition, bool> AdventurerSelected;
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
			// Если активен режим свитка — выбор/снятие выбора этой карты для реролла
			if (RerollController.IsActive)
			{
				RerollController.ToggleSelection(_definition);
				CardClicked?.Invoke(this);
				return;
			}
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
			// Dungeon card clicked — либо боёвка, либо выбор для реролла (если активен свиток)
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
		if (_definition != null && _definition.kind == CardKind.Adventurer)
		{
			AdventurerSelected?.Invoke(_definition, _selected);
		}
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
		// Сообщаем подписчикам (например, CardInfoPopup), что авантюрист дизелектнут — попап закроется
		if (_definition != null && _definition.kind == CardKind.Adventurer)
		{
			AdventurerSelected?.Invoke(_definition, false);
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


