using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CookingStar
{
    public class InitLoader : MonoBehaviour
    {
        void Awake()
        {
            if (!GameObject.FindGameObjectWithTag("MusicPlayer"))
                SceneManager.LoadScene("Init");
        }
    }
}