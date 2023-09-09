using System.Collections.Generic;
using CookingStar;
using UnityEngine;

namespace __App.Scripts.UI
{
    public class ScreensManager : MonoBehaviour
    {
        [SerializeField] private GameObject _parent;
        private List<UIScreen> _screens = new ();
        private List<UIScreen> _activeScreens = new ();
        
        public static ScreensManager instance;
        
        private void Awake()
        {
            DontDestroyOnLoad(this);
            instance = this;
            var screens = Resources.LoadAll<UIScreen>("UIScreens");
            foreach (var screen in screens)
            {
                var screenObject = Instantiate(screen,_parent.transform);
                _screens.Add(screenObject);
            }
        }
        
        private void Start()
        {
            var screen = GetScreenByType(ScreensTypes.MENU);
            screen.Show();
            _activeScreens.Add(screen);
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