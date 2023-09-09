using System.Collections;
using System.Collections.Generic;
using CookingStar;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace __App.Scripts.UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private List<UIScreen> _screens;
        [Space(10)]
        [Header("Can be null")] 
        [SerializeField] private TextMeshProUGUI _moneyCount;
        
        private List<UIScreen> _activeScreens = new ();
        
        public static UIController instance;
        
        private void Awake()
        {
            instance = this;
        }

        /*public void LoadShop()
        {
            SfxPlayer.instance.PlaySfx(0);
            StartCoroutine(LoadSceneDelayed("Shop"));
        }*/

        public void LoadCareerMode()
        {
            SfxPlayer.instance.PlaySfx(0);
            PlayerPrefs.SetString("gameMode", "CAREER");
            PlayerPrefs.SetInt("IsTutorial", 0);
            //StartCoroutine(LoadSceneDelayed("LevelSelection"));
            StartCoroutine(LoadScreenDelayed(ScreensTypes.SELECT_LEVEL));
        }

        /*public void LoadFreePlay()
        {
            SfxPlayer.instance.PlaySfx(0);
            PlayerPrefs.SetString("gameMode", "FREEPLAY");
            PlayerPrefs.SetInt("IsTutorial", 0);
            StartCoroutine(LoadSceneDelayed("Game"));
        }*/

        /*public void LoadTutorial()
        {
            SfxPlayer.instance.PlaySfx(0);
            PlayerPrefs.SetString("gameMode", "FREEPLAY");
            PlayerPrefs.SetInt("AvailableCandy", PlayerPrefs.GetInt("AvailableCandy", 0) + 3);  //Add +3 prize candy for tutorial
            PlayerPrefs.SetInt("IsTutorial", 1);
            StartCoroutine(LoadSceneDelayed("Game"));
        }*/

        /*public void LoadCoinPack()
        {
            SfxPlayer.instance.PlaySfx(0);
            StartCoroutine(LoadSceneDelayed("BuyCoinPack"));
        }*/

        public void LoadMenu()
        {
            SfxPlayer.instance.PlaySfx(0);
            if (SceneManager.GetActiveScene().name != "Menu")
            {
                StartCoroutine(LoadSceneDelayed("Menu"));
            }
            else
            {
                StartCoroutine(LoadScreenDelayed(ScreensTypes.MENU));
                var money = GetPlayerMoneyCount();
                SetPlayerMoneyCount(money);
            }
        }

        public void SetPlayerMoneyCount(int count)
        {
            _moneyCount.text = count.ToString();
        }
        
        public int GetPlayerMoneyCount()
        {
            return PlayerPrefs.GetInt("PlayerMoney", 0);
        }
        
        public void LoadSettings()
        {
            SfxPlayer.instance.PlaySfx(0);
            StartCoroutine(LoadScreenDelayed(ScreensTypes.SETTINGS));
        }

        public IEnumerator LoadSceneDelayed(string sceneToLead, float delay = 0.25f)
        {
            CloseLastScreen();
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(sceneToLead);
        }
        
        public IEnumerator LoadScreenDelayed(ScreensTypes screensTypes, float delay = 0.25f)
        {
            yield return new WaitForSeconds(delay);
            CloseLastScreen();
            ShowScreen(screensTypes);
        }

        public void Pause()
        {
            SfxPlayer.instance.PlaySfx(0);
            PauseManager.instance.PauseGame();
            DisplayPausePanel();
        }

        public void Unpause()
        {
            SfxPlayer.instance.PlaySfx(0);
            PauseManager.instance.UnPauseGame();
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
            //GameObject pausePanel = GameObject.FindGameObjectWithTag("PausePanelUI");
            //pausePanel.transform.GetChild(0).gameObject.SetActive(true);
            ShowScreen(ScreensTypes.PAUSE);
        }

        public void HidePausePanel()
        {
            //GameObject pausePanel = GameObject.FindGameObjectWithTag("PausePanelUI");
            //pausePanel.transform.GetChild(0).gameObject.SetActive(false);
            HideScreen(ScreensTypes.PAUSE);
        }
        
        public UIScreen GetScreenByType(ScreensTypes screenType)
        {
            return _screens.Find(screen => screen.screenType == screenType);
        }
        
        public void ShowScreen(ScreensTypes screenType)
        {
            var screen = GetScreenByType(screenType);
            screen.Show();
            _activeScreens.Add(screen);
        }
        
        public void HideScreen(ScreensTypes screenType)
        {
            var screen = GetScreenByType(screenType);
            screen.Hide();
            _activeScreens.Remove(screen);
        }
        
        public void CloseLastScreen()
        {
            if (_activeScreens.Count > 0)
            {
                var screen = _activeScreens[^1];
                screen.Hide();
                _activeScreens.Remove(screen);
            }
        }
        
    }
}