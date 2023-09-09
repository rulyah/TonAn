using UnityEngine;

namespace __App.Scripts.Career
{
	public class CareerMapManager : MonoBehaviour
	{
		static public int userLevelAdvance;
		public static bool canTap;

		void Awake()
		{
			canTap = true; //player can tap on buttons

			if (PlayerPrefs.HasKey("userLevelAdvance"))
				userLevelAdvance = PlayerPrefs.GetInt("userLevelAdvance");
			else
				userLevelAdvance = 0; //default. only level 1 in open.

			//Debug
			userLevelAdvance = 8;
		}
	}
}