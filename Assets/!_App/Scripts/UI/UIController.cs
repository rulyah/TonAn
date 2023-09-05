using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace CookingStar
{
    public class UIController : MonoBehaviour
    {
        public static UIController instance;

        private void Awake()
        {
            instance = this;
        }

        public void LoadShop()
        {
            SfxPlayer.instance.PlaySfx(0);
            StartCoroutine(LoadSceneDelayed("Shop"));
        }

        public void LoadCareerMode()
        {
            SfxPlayer.instance.PlaySfx(0);
            PlayerPrefs.SetString("gameMode", "CAREER");
            PlayerPrefs.SetInt("IsTutorial", 0);
            StartCoroutine(LoadSceneDelayed("LevelSelection"));
        }

        public void LoadFreePlay()
        {
            SfxPlayer.instance.PlaySfx(0);
            PlayerPrefs.SetString("gameMode", "FREEPLAY");
            PlayerPrefs.SetInt("IsTutorial", 0);
            StartCoroutine(LoadSceneDelayed("Game"));
        }

        public void LoadTutorial()
        {
            SfxPlayer.instance.PlaySfx(0);
            PlayerPrefs.SetString("gameMode", "FREEPLAY");
            PlayerPrefs.SetInt("AvailableCandy", PlayerPrefs.GetInt("AvailableCandy", 0) + 3);  //Add +3 prize candy for tutorial
            PlayerPrefs.SetInt("IsTutorial", 1);
            StartCoroutine(LoadSceneDelayed("Game"));
        }

        public void LoadCoinPack()
        {
            SfxPlayer.instance.PlaySfx(0);
            StartCoroutine(LoadSceneDelayed("BuyCoinPack"));
        }

        public void LoadMenu()
        {
            SfxPlayer.instance.PlaySfx(0);
            StartCoroutine(LoadSceneDelayed("Menu"));
        }

        public IEnumerator LoadSceneDelayed(string sceneToLead, float delay = 0.25f)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(sceneToLead);
        }

        public void Pause()
        {
            SfxPlayer.instance.PlaySfx(0);
            PauseManager.instance.PauseGame();

            //use new UI
            DisplayPausePanel();
        }

        public void Unpause()
        {
            SfxPlayer.instance.PlaySfx(0);
            PauseManager.instance.UnPauseGame();

            //use new UI
            HidePausePanel();
        }

        public void Restart()
        {
            SfxPlayer.instance.PlaySfx(0);

            Unpause();
            SceneManager.LoadScene("Game");
        }

        public void LoadMenuFromGameScene()
        {
            SfxPlayer.instance.PlaySfx(0);

            Unpause();
            SceneManager.LoadScene("Menu");
        }

        public void DisplayPausePanel()
        {
            GameObject pausePanel = GameObject.FindGameObjectWithTag("PausePanelUI");
            pausePanel.transform.GetChild(0).gameObject.SetActive(true);
        }

        public void HidePausePanel()
        {
            GameObject pausePanel = GameObject.FindGameObjectWithTag("PausePanelUI");
            pausePanel.transform.GetChild(0).gameObject.SetActive(false);
        }
    }
}