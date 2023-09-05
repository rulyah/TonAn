using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

namespace CookingStar
{
    public class MoneyController : MonoBehaviour
    {
        private int availableMoney;
        public Text playerCoinUI;

        void Awake()
        {
            availableMoney = PlayerPrefs.GetInt("PlayerMoney");
            playerCoinUI.text = "" + availableMoney;
        }

        public void SelectCoinPack(CoinpackProperties cpp)
        {
            cpp.gameObject.GetComponent<Button>().interactable = false;
            StartCoroutine(SelectCoinPackCo(cpp));
        }

        public IEnumerator SelectCoinPackCo(CoinpackProperties cpp)
        {
            switch (cpp.itemIndex)
            {
                case 1:
                    //add the purchased coins to the available user money
                    availableMoney += cpp.itemValue;
                    //save new amount of money
                    PlayerPrefs.SetInt("PlayerMoney", availableMoney);
                    //play sfx
                    SfxPlayer.instance.PlaySfx(2);
                    //Wait
                    yield return new WaitForSeconds(0.4f);
                    //Reload the level
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    break;

                case 2:
                    availableMoney += cpp.itemValue;
                    PlayerPrefs.SetInt("PlayerMoney", availableMoney);
                    SfxPlayer.instance.PlaySfx(2);
                    yield return new WaitForSeconds(0.4f);
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    break;

                case 3:
                    availableMoney += cpp.itemValue;
                    PlayerPrefs.SetInt("PlayerMoney", availableMoney);
                    SfxPlayer.instance.PlaySfx(2);
                    yield return new WaitForSeconds(0.4f);
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    break;
            }
        }

    }
}