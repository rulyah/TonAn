using __App.Scripts.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace __App.Scripts.Generic
{
    public class MenuLoader : MonoBehaviour
    {
        private void Start()
        {
            if(SceneManager.GetActiveScene().name == "Menu") UIController.instance.LoadMenu();
        }
    }
}