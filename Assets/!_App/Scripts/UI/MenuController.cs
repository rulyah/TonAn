using UnityEngine;
using TMPro;

namespace CookingStar
{
	public class MenuController : MonoBehaviour
	{
		public TextMeshProUGUI playerCoinsUI;

		void Awake()
		{
			Time.timeScale = 1.0f;
			playerCoinsUI.text = "" + PlayerPrefs.GetInt("PlayerMoney");
		}

	}
}