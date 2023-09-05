using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace CookingStar
{
	public class PauseManager : MonoBehaviour
	{
		public static PauseManager instance;
		public static bool isPaused;
		private float savedTimeScale;

		void Awake()
		{
			instance = this;
			isPaused = false;
			Time.timeScale = 1.0f;
		}

		public void PauseGame()
		{
			print("Game is Paused...");
			isPaused = true;
			savedTimeScale = Time.timeScale;
			Time.timeScale = 0;
			AudioListener.volume = 0;
		}

		public void UnPauseGame()
		{
			print("Unpause");
			isPaused = false;
			Time.timeScale = savedTimeScale;
			AudioListener.volume = 1.0f;
		}

	}
}