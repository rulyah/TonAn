using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace __App.Scripts.Generic
{
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager instance { get; private set; }

        [SerializeField] private List<Mission> _missionList;
        
        public int _missionCount => _missionList.Count;

        private void Awake()
        {
            if (instance == null)
            {
                DontDestroyOnLoad(gameObject);
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void SelectLevel(Mission mission)
        {
            PlayerPrefs.SetString("gameMode", "CAREER");
            PlayerPrefs.SetInt("careerLevelID", mission.id);

            PlayerPrefs.SetInt("careerPrize", mission.reward);

            PlayerPrefs.SetInt("careerGoalBallance", mission.target);
            PlayerPrefs.SetInt("careerAvailableTime", mission.time);
            
            int availableProducts = mission.availableProductsInMission.Length;
            PlayerPrefs.SetInt("availableProducts", availableProducts);
            for (int j = 0; j < availableProducts; j++)
                PlayerPrefs.SetInt("careerProduct_" + j.ToString(), mission.availableProductsInMission[j]);

            PlayerPrefs.SetInt("canUseCandy", Convert.ToInt32(mission.canUseCandy));

            PlayerPrefs.SetInt("levelLocation", (int)mission.levelLocation);

            SceneManager.LoadScene("Game");
        }

        public Mission GetMissionById(int id)
        {
            return _missionList.Find(n => n.id == id);
        }
        
        public enum Environments
        {
            Environment_1 = 0,
            Environment_2 = 1,
        }
    }
}