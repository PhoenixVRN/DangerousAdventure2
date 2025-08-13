using UnityEngine;
using TMPro;

public class DragonCounter : MonoBehaviour
{
	[SerializeField] private TMP_Text counterText;
	public int CurrentCount { get; private set; } = 0;

	public void ResetCount()
	{
		CurrentCount = 0;
		RefreshUI();
	}

	public void Increment()
	{
		CurrentCount++;
		RefreshUI();
	}

	public void Decrement(int amount = 1)
	{
		CurrentCount = Mathf.Max(0, CurrentCount - Mathf.Max(0, amount));
		RefreshUI();
	}

	private void RefreshUI()
	{
		if (counterText != null)
		{
			counterText.text = CurrentCount.ToString();
		}
	}
}


