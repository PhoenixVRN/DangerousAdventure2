using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoundManager : MonoBehaviour
{
	[SerializeField] private CardDealer dealer;
	[SerializeField] private CombatSystem combat;
	[SerializeField] private int startingAdventurers = 7;
	[SerializeField] private TMP_Text roundText;
	[SerializeField] private Button nextRoundButton;
	[SerializeField] private GoldManager goldManager;
	[SerializeField] private bool autoStartOnLoad = false;

	private bool roundCleared = false;

	public int CurrentRound { get; private set; } = 1;

	private void Start()
	{
		if (autoStartOnLoad)
		{
			StartRound(1, initial: true);
		}
	}

    public void StartRound(int round, bool initial = false)
    {
        CurrentRound = Mathf.Max(1, round);
        if (dealer == null) return;
		roundCleared = false;
		SetNextRoundButton(false);
        if (initial)
        {
            dealer.DealAdventurersExact(startingAdventurers, clear: true);
        }

        // Делаем одну раздачу для текущего раунда
        int dungeonCount = GetDungeonCountForRound(CurrentRound);
        dealer.DealDungeonExact(dungeonCount, clear: true);
        RefreshUI();

        // Если накопилось 3+ драконов — входим в режим боя с драконами
        if (TryEnterDragonBattle())
        {
            return;
        }

		// Если нет обычных врагов (остались только сундуки/поушены/драконы<3) — сразу даём кнопку Next Round
		if (ShouldAllowImmediateNextRound())
		{
			roundCleared = true;
			SetNextRoundButton(true);
            // На всякий случай — дублируем проверку на следующий кадр
            StartCoroutine(DeferredEvaluateAfterDeal());
			return;
		}

        // Если есть обычные враги — играем этот раунд (кнопку не показываем)
        if (HasAnyActiveEnemies())
        {
            return;
        }
        // Нет обычных врагов — сразу даём кнопку Next Round (независимо от того, сундуки это, поушены или драконы отдельно)
        roundCleared = true;
        SetNextRoundButton(true);
        StartCoroutine(DeferredEvaluateAfterDeal());
        return;

    }

    private System.Collections.IEnumerator DeferredEvaluateAfterDeal()
    {
        // Ждём кадр, чтобы все инстанциирования/лейауты применились, затем переоцениваем
        yield return null;
        if (TryEnterDragonBattle())
            yield break;
        if (ShouldAllowImmediateNextRound())
        {
            roundCleared = true;
            SetNextRoundButton(true);
            yield break;
        }
        if (!HasAnyActiveEnemies())
        {
            roundCleared = true;
            SetNextRoundButton(true);
        }
    }

    public bool TryEnterDragonBattle()
    {
        var dealerField = typeof(CardDealer).GetField("dragonParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var dragonParent = dealerField != null ? (Transform)dealerField.GetValue(dealer) : null;
        if (dragonParent == null)
            return false;
        if (dragonParent.childCount < 3)
            return false;

        // Переключаем состояние игры → фон сменится через BackgroundStateImage
        GameManager.Instance?.StartDragonBattle();

        // Очищаем поле врагов
        var dungeonField = typeof(CombatSystem).GetField("dungeonParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var dungeonParent = dungeonField != null ? (Transform)dungeonField.GetValue(combat) : null;
        if (dungeonParent != null)
        {
            for (int i = dungeonParent.childCount - 1; i >= 0; i--)
            {
                var child = dungeonParent.GetChild(i);
                if (Application.isPlaying) Destroy(child.gameObject); else DestroyImmediate(child.gameObject);
            }
        }

        // Переносим всех драконов на поле врагов
        if (dungeonParent != null)
        {
            var toMove = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < dragonParent.childCount; i++)
            {
                toMove.Add(dragonParent.GetChild(i));
            }
            for (int i = 0; i < toMove.Count; i++)
            {
                toMove[i].SetParent(dungeonParent, worldPositionStays: false);
            }
        }

        roundCleared = false;
        SetNextRoundButton(false);
        RefreshUI();
        return true;
    }

	public void OnEnemyCleared()
	{
		// Сначала проверяем вход в режим драконьей битвы (например, после реролла)
		if (TryEnterDragonBattle())
			return;
		// Проверяем: остались ли враги (кроме драконов и сундуков)
		if (combat == null) return;
		// Исключение: если открыта панель воскрешения — не уходим в таверну и не показываем переходы автоматически
		var resPanelField = typeof(CombatSystem).GetField("resurrectionPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var resPanel = resPanelField != null ? (ResurrectionPanelController)resPanelField.GetValue(combat) : null;
		if (resPanel != null && resPanel.IsOpen)
		{
			return;
		}
		// Если в обычном бою закончились герои — уходим в таверну
		if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.DragonBattle)
		{
			var dealerType = typeof(CardDealer);
			var factoryField = dealerType.GetField("factory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var factory = factoryField != null ? (CardFactory)factoryField.GetValue(dealer) : null;
			Transform adventurerParent = null;
			if (factory != null)
			{
				var fType = typeof(CardFactory);
				var advField = fType.GetField("adventurerParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				adventurerParent = advField != null ? (Transform)advField.GetValue(factory) : null;
			}
			if (adventurerParent != null && adventurerParent.childCount == 0)
			{
				roundCleared = false;
				SetNextRoundButton(false);
				GameManager.Instance.EnterTavern();
				return;
			}
		}
		var cleared = !HasAnyActiveEnemies();
        if (cleared)
        {
			// Если это был режим боя с драконами — после победы уходим в таверну
			if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.DragonBattle)
			{
				roundCleared = false;
				SetNextRoundButton(false);
				// Показать уведомление о победе
				ToastController.Instance?.Show("Вы победили!", 3f);
				GameManager.Instance.EnterTavern();
				return;
			}
            if (!roundCleared)
            {
                roundCleared = true;
                SetNextRoundButton(true);
            }
        }
        else
        {
            int potionsOnly = PotionsOnlyCount();
            if ((CurrentRound == 1 && potionsOnly >= 1) || potionsOnly == 1)
            {
                if (!roundCleared)
                {
                    roundCleared = true;
                    SetNextRoundButton(true);
                }
            }
            else if (HasAnyChests())
            {
                // Разрешаем переход при наличии только сундуков/драконов
                if (!roundCleared)
                {
                    roundCleared = true;
                    SetNextRoundButton(true);
                }
            }
        }
	}

    private bool HasAnyActiveEnemies()
	{
		// Используем CombatSystem.dungeonParent
		var parentField = typeof(CombatSystem).GetField("dungeonParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var parent = parentField != null ? (Transform)parentField.GetValue(combat) : null;
		if (parent == null) return false;
        // В режиме битвы с драконами считаем их активными врагами
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.DragonBattle)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var def = parent.GetChild(i).GetComponent<CardDefinition>();
                if (def == null || def.dungeonData == null) continue;
                if (def.dungeonData.cardType == DungeonCardType.Dragon)
                    return true;
            }
            return false;
        }
		for (int i = 0; i < parent.childCount; i++)
		{
			var def = parent.GetChild(i).GetComponent<CardDefinition>();
			if (def == null || def.dungeonData == null) continue;
			// Любые instant-эффекты не считаем активными врагами (например, поушены)
			if (def.dungeonData.isInstantEffect)
				continue;
			var t = def.dungeonData.cardType;
			// Поушены не считаем активными врагами
			if (t != DungeonCardType.Dragon && t != DungeonCardType.Chest && t != DungeonCardType.Potion)
				return true;
		}
		return false;
	}

    private bool HasAnyChests()
    {
        var parentField = typeof(CombatSystem).GetField("dungeonParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var parent = parentField != null ? (Transform)parentField.GetValue(combat) : null;
        if (parent == null) return false;
        for (int i = 0; i < parent.childCount; i++)
        {
            var def = parent.GetChild(i).GetComponent<CardDefinition>();
            if (def == null || def.dungeonData == null) continue;
            if (def.dungeonData.cardType == DungeonCardType.Chest)
                return true;
        }
        return false;
    }

    private bool HasAnyDragons()
    {
        var field = typeof(CardDealer).GetField("dragonParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var dragonParent = field != null ? (Transform)field.GetValue(dealer) : null;
        return dragonParent != null && dragonParent.childCount > 0;
    }

	private void RefreshUI()
	{
		if (roundText != null)
		{
			roundText.text = CurrentRound.ToString();
		}
	}

    private int PotionsOnlyCount()
    {
        var parentField = typeof(CombatSystem).GetField("dungeonParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var parent = parentField != null ? (Transform)parentField.GetValue(combat) : null;
        if (parent == null) return 0;
        int potions = 0;
        for (int i = 0; i < parent.childCount; i++)
        {
            var def = parent.GetChild(i).GetComponent<CardDefinition>();
            if (def == null || def.dungeonData == null) continue;
            var t = def.dungeonData.cardType;
			if (t == DungeonCardType.Potion || def.dungeonData.isInstantEffect)
                potions++;
            else if (t != DungeonCardType.Dragon)
                return 0; // найден сундук или обычный враг → не только поушены
        }
        return potions;
    }

	private bool ShouldAllowImmediateNextRound()
	{
		// Разрешаем кнопку Next Round, если на столе НЕТ обычных врагов
		// Обычный враг = любая карта подземелья, которая НЕ сундук, НЕ поушен (и не instant), и НЕ дракон.
		var parentField = typeof(CombatSystem).GetField("dungeonParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var parent = parentField != null ? (Transform)parentField.GetValue(combat) : null;
		if (parent == null) return true; // пусто → можно переходить
		for (int i = 0; i < parent.childCount; i++)
		{
			var def = parent.GetChild(i).GetComponent<CardDefinition>();
			if (def == null || def.dungeonData == null) continue;
			if (def.dungeonData.isInstantEffect) continue; // мгновенные эффекты не считаем врагами
			var t = def.dungeonData.cardType;
			if (t != DungeonCardType.Chest && t != DungeonCardType.Potion && t != DungeonCardType.Dragon)
			{
				return false; // найден обычный враг
			}
		}
		return true;
	}

	public void OnNextRoundButtonClicked()
	{
		if (!roundCleared)
			return;
        // Награда за пройденный раунд: +CurrentRound золота
        goldManager?.AddGold(CurrentRound);
		StartRound(CurrentRound + 1);
		roundCleared = false;
		SetNextRoundButton(false);
	}

    private void SetNextRoundButton(bool enabled)
	{
		if (nextRoundButton == null)
			return;
        nextRoundButton.gameObject.SetActive(enabled);
        nextRoundButton.interactable = enabled;
        var cg = nextRoundButton.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
            cg.blocksRaycasts = enabled;
            cg.interactable = enabled;
        }
	}

    private int GetDungeonCountForRound(int round)
    {
        // 1→1, 2→2, ..., 7→7, 8→7, 9→7, 10→7
        if (round <= 7) return Mathf.Max(1, round);
        return 7;
    }
}


