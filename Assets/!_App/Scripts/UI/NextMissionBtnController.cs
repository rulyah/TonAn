using __App.Scripts.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace __App.Scripts.UI
{
    public class NextMissionBtnController : MonoBehaviour
    {
        public void NextMission()
        {
            var currentMissionId = PlayerPrefs.GetInt("careerLevelID");
            var nextMissionId = currentMissionId + 1;
            
            if(nextMissionId >= MissionManager.instance._missionCount) SceneManager.LoadScene("Menu");
            else
            {
                var nextMission = MissionManager.instance.GetMissionById(nextMissionId);
                MissionManager.instance.SelectLevel(nextMission);
            }
        }
    }
}