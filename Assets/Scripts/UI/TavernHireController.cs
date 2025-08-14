using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TavernHireController : MonoBehaviour
{
	[SerializeField] private CardFactory factory;
	[SerializeField] private AdventurerCardsConfig adventurerConfig;
	[SerializeField] private Transform hireContainer;
	[SerializeField] private Button hireHeroesButton;
	[SerializeField] private Button fightButton;
	[SerializeField] private RoundManager roundManager;
	[SerializeField] private int hireCount = 7;
	[SerializeField] private bool clearBeforeHire = true;

	private void Awake()
	{
		if (fightButton != null)
		{
			fightButton.gameObject.SetActive(false);
			fightButton.interactable = false;
		}
	}

	private void OnEnable()
	{
		if (hireHeroesButton != null)
		{
			hireHeroesButton.onClick.RemoveAllListeners();
			hireHeroesButton.onClick.AddListener(OnHireHeroesClicked);
		}
		if (fightButton != null)
		{
			fightButton.onClick.RemoveAllListeners();
			fightButton.onClick.AddListener(OnFightClicked);
		}
	}

	private void OnDisable()
	{
		if (hireHeroesButton != null) hireHeroesButton.onClick.RemoveAllListeners();
		if (fightButton != null) fightButton.onClick.RemoveAllListeners();
	}

	private void OnHireHeroesClicked()
	{
		if (factory == null || adventurerConfig == null)
			return;
		Transform targetParent = hireContainer != null ? hireContainer : GetFactoryAdventurerParent();
		if (targetParent == null)
			return;
		// Всегда очищаем контейнер найма перед новой раздачей, чтобы не накапливать героев
		for (int i = targetParent.childCount - 1; i >= 0; i--)
		{
			var child = targetParent.GetChild(i);
			if (Application.isPlaying) Destroy(child.gameObject); else DestroyImmediate(child.gameObject);
		}
		for (int i = 0; i < Mathf.Max(0, hireCount); i++)
		{
			var entry = PickRandom(adventurerConfig.cards);
			if (entry == null) continue;
			factory.SpawnAdventurer(entry.id, targetParent);
		}
		if (fightButton != null)
		{
			fightButton.gameObject.SetActive(true);
			fightButton.interactable = true;
		}
	}

	private void OnFightClicked()
	{
		// Спрячем кнопку, чтобы не триггерили повторно
		if (fightButton != null)
		{
			fightButton.interactable = false;
		}
		// Очистим боевой контейнер и перенесём нанятых героев из контейнера таверны
		var battleParent = GetFactoryAdventurerParent();
		if (hireContainer != null && battleParent != null)
		{
			// Если это разные контейнеры — очистим боевой, чтобы не было дублей
			if (hireContainer != battleParent)
			{
				var toRemove = new System.Collections.Generic.List<Transform>();
				for (int i = 0; i < battleParent.childCount; i++)
					toRemove.Add(battleParent.GetChild(i));
				for (int i = 0; i < toRemove.Count; i++)
				{
					var child = toRemove[i];
					if (Application.isPlaying) Destroy(child.gameObject); else DestroyImmediate(child.gameObject);
				}
			}
			var toMove = new System.Collections.Generic.List<Transform>();
			for (int i = 0; i < hireContainer.childCount; i++)
			{
				toMove.Add(hireContainer.GetChild(i));
			}
			for (int i = 0; i < toMove.Count; i++)
			{
				toMove[i].SetParent(battleParent, worldPositionStays: false);
			}
		}
		// Меняем состояние, чтобы фон переключился на боевой
		GameManager.Instance?.StartEnemyBattle(1);
		// Запускаем 1-й раунд без перераздачи наших героев
		roundManager?.StartRound(1, initial: false);
	}

	private static T PickRandom<T>(List<T> list) where T : class
	{
		if (list == null || list.Count == 0) return null;
		int idx = Random.Range(0, list.Count);
		return list[idx];
	}

	private Transform GetFactoryAdventurerParent()
	{
		if (factory == null) return null;
		var fType = typeof(CardFactory);
		var advField = fType.GetField("adventurerParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		return advField != null ? (Transform)advField.GetValue(factory) : null;
	}
}



