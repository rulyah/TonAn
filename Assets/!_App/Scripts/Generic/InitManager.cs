using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace CookingStar
{
	public class InitManager : MonoBehaviour
	{
		IEnumerator Start()
		{
			//PlayerPrefs.DeleteAll();
			yield return new WaitForSeconds(0.1f);
			SceneManager.LoadScene("Menu");
		}

	}
}