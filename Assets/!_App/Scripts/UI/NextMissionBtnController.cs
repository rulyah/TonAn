using __App.Scripts.Generic;
using UnityEngine;

namespace __App.Scripts.UI
{
    public class NextMissionBtnController : MonoBehaviour
    {
        public void NextMission()
        {
            var currentMissionId = PlayerPrefs.GetInt("careerLevelID");
            var nextMission = MissionManager.instance.GetMissionById(currentMissionId + 1);
            MissionManager.instance.SelectLevel(nextMission);
        }
    }
}