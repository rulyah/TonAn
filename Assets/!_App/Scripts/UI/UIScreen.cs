using __App.Scripts.UI;
using UnityEngine;

namespace CookingStar
{
    public class UIScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        
        public ScreensTypes screenType;
        
        public virtual void Show()
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
        
        public virtual void Hide()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}