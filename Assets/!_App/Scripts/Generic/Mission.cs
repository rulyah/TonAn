using System;

namespace __App.Scripts.Generic
{
    [Serializable]
    public struct Mission
    {
        public int id;
        public string name;
        public int target;
        public int time;
        public int reward;
        public bool canUseCandy;
        public int[] availableProductsInMission;
        public MissionManager.Environments levelLocation;
    }
}