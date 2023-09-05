using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

namespace CookingStar
{
	public class MenuController : MonoBehaviour
	{
		public Text playerCoinsUI;

		void Awake()
		{
			Time.timeScale = 1.0f;
			playerCoinsUI.text = "" + PlayerPrefs.GetInt("PlayerMoney");
		}

	}
}