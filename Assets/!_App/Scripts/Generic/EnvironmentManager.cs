using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CookingStar
{
	public class EnvironmentManager : MonoBehaviour
	{
		/// <summary>
		/// This class sets a new texture (material) to background object everytime player loads a new level. 
		/// The actual texture index is set via "LevelSelection" scene.
		/// </summary>

		public string envPrefsKey = "levelLocation";
		public Material[] availableEnvironments;
		private int envID;

		void Awake()
		{
			//Get it from PlayerPrefs
			//envID = PlayerPrefs.GetInt (envPrefsKey, 0);

			//Use a random ID on each play
			envID = Random.Range(0, availableEnvironments.Length);

			//Since we only have two BG arts in this demo, we need to make sure we are not using a bigger index!
			if (envID > 1)
				envID = 1;

			GetComponent<Renderer>().material = availableEnvironments[envID];
		}

	}
}