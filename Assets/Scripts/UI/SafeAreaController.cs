using UnityEngine;

/// <summary>
/// Подгоняет указанный RectTransform под безопасную область экрана (ноутчи/скругления) на Android/iOS.
/// Повесь на корневой UI-контейнер панели, которая должна учитывать safe area.
/// </summary>
public class SafeAreaController : MonoBehaviour
{
	[SerializeField] private RectTransform target;

	private Rect _lastSafeArea;
	private ScreenOrientation _lastOrientation;
	private Vector2Int _lastResolution;

	private void Awake()
	{
		if (target == null)
			target = transform as RectTransform;
		Apply();
	}

	private void OnEnable()
	{
		Apply();
	}

	private void Update()
	{
		if (Screen.safeArea != _lastSafeArea ||
			Screen.orientation != _lastOrientation ||
			Screen.width != _lastResolution.x ||
			Screen.height != _lastResolution.y)
		{
			Apply();
		}
	}

	private void Apply()
	{
		if (target == null)
			return;
		_lastSafeArea = Screen.safeArea;
		_lastOrientation = Screen.orientation;
		_lastResolution = new Vector2Int(Screen.width, Screen.height);

		var sa = _lastSafeArea;
		var res = new Vector2(Screen.width, Screen.height);
		var anchorMin = sa.position;
		var anchorMax = sa.position + sa.size;
		anchorMin.x /= res.x;
		anchorMin.y /= res.y;
		anchorMax.x /= res.x;
		anchorMax.y /= res.y;

		target.anchorMin = anchorMin;
		target.anchorMax = anchorMax;
		target.offsetMin = Vector2.zero;
		target.offsetMax = Vector2.zero;
	}
}



