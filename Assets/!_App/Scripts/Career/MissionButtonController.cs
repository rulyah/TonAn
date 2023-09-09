using System;
using System.Collections;
using System.Collections.Generic;
using __App.Scripts.Career;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CookingStar
{
    public class MissionButtonController : MonoBehaviour
    {
        [Header("Mission Settings")]
        public int missionID = 1;
        public string missionName = "Mission Name";
        public int missionTarget = 1000;
        public int missionTime = 120;
        public int missionReward = 150;
        public bool canUseCandy = true;

        //Level location (main BG image)
        public enum environments
        {
            Environment_1 = 0,
            Environment_2 = 1,
        }
        public environments levelLocation = environments.Environment_1;
        public int[] availableProductsInMission;

        [Header("Mission Components/UI")]
        public Button missionButton;
        public Text missionNumberUI;
        public Text missionNameUI;
        public Text missionTargetUI;
        public Text missionTimeUI;
        public Text missionPrizeUI;
        public Image missionStarsUI;

        [Header("Resources")]
        public Sprite[] availableMissionStars;

        //Private data
        private float levelSavedTime = 0;   //time record for this level
        private float timeDifference = 0;

        private void Awake()
        {
            missionButton.interactable = false;
        }

        void Start()
        {
            missionNumberUI.text = "Mission " + missionID;
            missionNameUI.text = "" + missionName;
            missionTargetUI.text = "" + missionTarget;
            missionTimeUI.text = "" + missionTime;
            missionPrizeUI.text = "" + missionReward;

            if (CareerMapManager.userLevelAdvance >= missionID - 1)
            {
                //this level is open
                missionButton.interactable = true;

                //grant stars
                levelSavedTime = PlayerPrefs.GetFloat("Level-" + missionID.ToString(), missionTime);
                timeDifference = missionTime - levelSavedTime;
                if (timeDifference > 60)
                {
                    //3-stars
                    missionStarsUI.sprite = availableMissionStars[3];
                }
                else if (timeDifference <= 60 && timeDifference > 30)
                {
                    //2-stars
                    missionStarsUI.sprite = availableMissionStars[2];
                }
                else if (timeDifference <= 30 && timeDifference > 0)
                {
                    //1-stars
                    missionStarsUI.sprite = availableMissionStars[1];

                }
                else if (timeDifference <= 0)
                {
                    //onlu happens if this is the first time we want to play this level
                    //0-star
                    missionStarsUI.sprite = availableMissionStars[0];
                }
            }
            else
            {
                //level is locked
                missionButton.interactable = false;
                missionNameUI.text = "Locked";
                //set 0-star image
                missionStarsUI.sprite = availableMissionStars[0];

            }
        }


        public void SelectMission()
        {
            if (!CareerMapManager.canTap)
                return;
            CareerMapManager.canTap = false;

            //Sfx
            SfxPlayer.instance.PlaySfx(0);

            //save the game mode
            PlayerPrefs.SetString("gameMode", "CAREER");
            PlayerPrefs.SetInt("careerLevelID", missionID);

            //save level prize
            PlayerPrefs.SetInt("careerPrize", missionReward);

            //save mission variables
            PlayerPrefs.SetInt("careerGoalBallance", missionTarget);
            PlayerPrefs.SetInt("careerAvailableTime", missionTime);

            int availableProducts = availableProductsInMission.Length;
            PlayerPrefs.SetInt("availableProducts", availableProducts); //save the length of availableProducts
            for (int j = 0; j < availableProducts; j++)
                PlayerPrefs.SetInt("careerProduct_" + j.ToString(), availableProductsInMission[j]);

            PlayerPrefs.SetInt("canUseCandy", Convert.ToInt32(canUseCandy));

            //set level location
            PlayerPrefs.SetInt("levelLocation", (int)levelLocation);

            Invoke("LoadSceneDelayed", 0.5f);
        }


        public void LoadSceneDelayed()
        {
            SceneManager.LoadScene("Game");
        }
    }
}