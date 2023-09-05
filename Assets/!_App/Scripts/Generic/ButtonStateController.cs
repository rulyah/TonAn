using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CookingStar
{
    public class ButtonStateController : MonoBehaviour
    {
        public Sprite[] availableStates;
        public GameObject buttonImage;
        public Text labelTextUI;
        private Image r;
        private int currentState;
        public string prefsCode = "";

        void Start()
        {
            //currentState = 1;
            currentState = PlayerPrefs.GetInt(prefsCode, 1);

            if (buttonImage)
            {
                r = buttonImage.GetComponent<Image>();
                r.sprite = availableStates[currentState];
            }

            if (labelTextUI)
            {
                labelTextUI.text = (currentState == 1) ? "On" : "Off";
            }
        }

        public void ChangeButtonState()
        {
            if (currentState == 0)
                currentState = 1;
            else
                currentState = 0;

            PlayerPrefs.SetInt(prefsCode, currentState);

            if (buttonImage)
                r.sprite = availableStates[currentState];

            if (labelTextUI)
                labelTextUI.text = (currentState == 1) ? "On" : "Off";
        }

        public void ChangeSoundState()
        {
            FbMusicPlayer.instance.ToggleSound();
        }


        public void ChangeMusicState()
        {
            FbMusicPlayer.instance.ToggleMusic();
        }

    }
}