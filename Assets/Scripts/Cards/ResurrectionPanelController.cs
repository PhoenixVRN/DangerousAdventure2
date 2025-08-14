using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResurrectionPanelController : MonoBehaviour
{
	[SerializeField] private GameObject panelRoot;
	[SerializeField] private TMP_Text counterText;
	[SerializeField] private Button warriorBtn;
	[SerializeField] private Button clericBtn;
	[SerializeField] private Button mageBtn;
	[SerializeField] private Button thiefBtn;
	[SerializeField] private Button paladinBtn;
	[SerializeField] private Button scrollBtn; // не используем обработчик для Scroll

	[SerializeField] private Graveyard graveyard;
	[SerializeField] private AdventurerCardsConfig adventurerConfig;
	[SerializeField] private CardFactory factory;
	[SerializeField] private Transform adventurerParent;
	[SerializeField] private Transform previewsParent;
	[SerializeField] private RoundManager roundManager;

	private int remaining = 0;
	private readonly List<string> _reservedIds = new List<string>();
	private readonly List<GameObject> _previewCards = new List<GameObject>();

	public bool IsOpen { get; private set; } = false;

	public void Open(int resurrectCount)
	{
		remaining = Mathf.Max(0, resurrectCount);
		Debug.Log($"[ResurrectionPanel] Open with count={remaining}");
		RefreshCounter();
		if (panelRoot != null) panelRoot.SetActive(true);
		IsOpen = true;
		ClearPreviews();
		_reservedIds.Clear();
		HookButtons(true);
	}

	public void Close()
	{
		Debug.Log("[ResurrectionPanel] Close panel");
		HookButtons(false);
		if (panelRoot != null) panelRoot.SetActive(false);
		IsOpen = false;
		// После закрытия панели — переоценим состояние поля (например, остался только сундук)
		roundManager?.OnEnemyCleared();
	}

	private void HookButtons(bool hook)
	{
		if (hook)
		{
			Debug.Log("[ResurrectionPanel] Hooking buttons");
			if (warriorBtn) warriorBtn.onClick.AddListener(() => OnPick(AdventurerClass.Warrior));
			if (clericBtn) clericBtn.onClick.AddListener(() => OnPick(AdventurerClass.Cleric));
			if (mageBtn) mageBtn.onClick.AddListener(() => OnPick(AdventurerClass.Mage));
			if (thiefBtn) thiefBtn.onClick.AddListener(() => OnPick(AdventurerClass.Thief));
			if (paladinBtn) paladinBtn.onClick.AddListener(() => OnPick(AdventurerClass.Paladin));
			// Scroll не оживляем: не добавляем обработчик
		}
		else
		{
			Debug.Log("[ResurrectionPanel] Unhooking buttons");
			if (warriorBtn) warriorBtn.onClick.RemoveAllListeners();
			if (clericBtn) clericBtn.onClick.RemoveAllListeners();
			if (mageBtn) mageBtn.onClick.RemoveAllListeners();
			if (thiefBtn) thiefBtn.onClick.RemoveAllListeners();
			if (paladinBtn) paladinBtn.onClick.RemoveAllListeners();
			if (scrollBtn) scrollBtn.onClick.RemoveAllListeners();
		}
	}

	private void OnPick(AdventurerClass klass)
	{
		Debug.Log($"[ResurrectionPanel] OnPick class={klass}, remaining(before)={remaining}");
		if (klass == AdventurerClass.Scroll)
			return;
		if (remaining <= 0) return;
        if (graveyard == null || factory == null || adventurerParent == null || adventurerConfig == null)
        {
            Debug.LogError($"[ResurrectionPanel] Missing refs: graveyard={(graveyard!=null)}, factory={(factory!=null)}, adventurerParent={(adventurerParent!=null)}, adventurerConfig={(adventurerConfig!=null)}");
            return;
        }
        // Диагностика: сколько всего карт в кладбище
        Debug.Log($"[ResurrectionPanel] Graveyard total={graveyard.adventurerIds.Count}");

        if (graveyard != null && factory != null && adventurerParent != null && adventurerConfig != null)
		{
            // ЛЮБОЙ павший герой может воскресить другого типа:
            // Списываем любую карту из кладбища и создаём выбранный класс из конфига
            if (graveyard.TryPopLast(out var consumedId))
			{
                Debug.Log($"[ResurrectionPanel] Consume from graveyard id={consumedId} -> resurrect class={klass}");
                var targetId = GetIdForClass(klass);
                if (string.IsNullOrEmpty(targetId))
                {
                    Debug.LogError($"[ResurrectionPanel] No config id for class={klass}");
                    return;
                }
                _reservedIds.Add(targetId);
				// Создаём превью карты в панели
				Transform parentForPreview = previewsParent != null ? previewsParent : (panelRoot != null ? panelRoot.transform : null);
				if (parentForPreview != null)
				{
                    var preview = factory.SpawnAdventurer(targetId, parentForPreview);
					if (preview != null)
					{
						// Отключаем интерактив на превью
						var interact = preview.GetComponent<CardInteraction>();
						if (interact != null) interact.enabled = false;
						var cg = preview.GetComponent<CanvasGroup>();
						if (cg == null) cg = preview.AddComponent<CanvasGroup>();
						cg.blocksRaycasts = false;
						// Поджимаем превью, чтобы влезало в панель
						preview.transform.localScale = Vector3.one * 0.8f;
						_previewCards.Add(preview);
                        Debug.Log($"[ResurrectionPanel] Preview spawned for id={targetId} under={parentForPreview.name}");
					}
				}
				else
				{
					Debug.LogWarning("[ResurrectionPanel] previewsParent/panelRoot is not assigned. Preview will be skipped.");
				}
				remaining--;
				RefreshCounter();
				Debug.Log($"[ResurrectionPanel] remaining(after)={remaining}");
				if (remaining <= 0)
				{
					// Выставляем все зарезервированные карты на наш стол
					for (int i = 0; i < _reservedIds.Count; i++)
					{
						factory.SpawnAdventurer(_reservedIds[i], adventurerParent);
					}
					_reservedIds.Clear();
					ClearPreviews();
					Close();
				}
			}
			else
			{
                Debug.LogWarning($"[ResurrectionPanel] Graveyard empty, cannot resurrect");
			}
		}
	}

	private void RefreshCounter()
	{
		if (counterText != null)
			counterText.text = remaining.ToString();
	}

	private void ClearPreviews()
	{
		Debug.Log($"[ResurrectionPanel] ClearPreviews count={_previewCards.Count}");
		for (int i = 0; i < _previewCards.Count; i++)
		{
			if (_previewCards[i] == null) continue;
			if (Application.isPlaying) Destroy(_previewCards[i]); else DestroyImmediate(_previewCards[i]);
		}
		_previewCards.Clear();
	}

    private string GetIdForClass(AdventurerClass klass)
    {
        if (adventurerConfig == null || adventurerConfig.cards == null) return null;
        for (int i = 0; i < adventurerConfig.cards.Count; i++)
        {
            var e = adventurerConfig.cards[i];
            if (e != null && e.adventurerClass == klass)
                return e.id;
        }
        return null;
    }
}


