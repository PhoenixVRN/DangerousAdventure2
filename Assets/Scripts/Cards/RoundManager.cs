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

	private bool roundCleared = false;

	public int CurrentRound { get; private set; } = 1;

	private void Start()
	{
		StartRound(1, initial: true);
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
        // Диллим врагов и если выпали только драконы — переходим к следующему раунду.
        int safety = 0;
        while (safety++ < 20)
        {
            int dungeonCount = GetDungeonCountForRound(CurrentRound);
            dealer.DealDungeonExact(dungeonCount, clear: true);
            RefreshUI();
            if (HasAnyActiveEnemies())
            {
                // есть обычные враги — играем этот раунд
                break;
            }
            // если есть сундуки и нет обычных врагов — даём выбор (кнопка Next Round)
            if (HasAnyChests())
            {
                roundCleared = true;
                SetNextRoundButton(true);
                break;
            }
            // Спец-правило для 1-го раунда: если выпали только драконы (и нет обычных врагов/сундуков), даём кнопку Next Round
            if (CurrentRound == 1 && HasAnyDragons())
            {
                roundCleared = true;
                SetNextRoundButton(true);
                break;
            }
            // если остались только поушены —
            // • в любом раунде при ровно 1 поушене — даём кнопку
            // • в 1-м раунде при >=1 поушене — тоже даём кнопку
            int potionsOnly = PotionsOnlyCount();
            if ((CurrentRound == 1 && potionsOnly >= 1) || potionsOnly == 1)
            {
                roundCleared = true;
                SetNextRoundButton(true);
                break;
            }
            // если только драконы или никого — следующий раунд
            CurrentRound++;
        }
    }

	public void OnEnemyCleared()
	{
		// Проверяем: остались ли враги (кроме драконов и сундуков)
		if (combat == null) return;
		var cleared = !HasAnyActiveEnemies();
        if (cleared)
        {
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
		for (int i = 0; i < parent.childCount; i++)
		{
			var def = parent.GetChild(i).GetComponent<CardDefinition>();
			if (def == null || def.dungeonData == null) continue;
			var t = def.dungeonData.cardType;
			if (t != DungeonCardType.Dragon && t != DungeonCardType.Chest)
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
            if (t == DungeonCardType.Potion)
                potions++;
            else if (t != DungeonCardType.Dragon)
                return 0; // найден сундук или обычный враг → не только поушены
        }
        return potions;
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


