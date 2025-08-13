using UnityEngine;
using TMPro;

public class GoldManager : MonoBehaviour
{
	[SerializeField] private TMP_Text goldText;
	public int TotalGold { get; private set; } = 0;

	public void ResetGold(int value = 0)
	{
		TotalGold = Mathf.Max(0, value);
		RefreshUI();
	}

	public void AddGold(int amount)
	{
		if (amount <= 0) return;
		TotalGold += amount;
		RefreshUI();
	}

	private void RefreshUI()
	{
		if (goldText != null)
		{
			goldText.text = TotalGold.ToString();
		}
	}
}


