using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

namespace CookingStar
{
    public class ShopController : MonoBehaviour
    {
        public GameObject playerMoney;
        private int availableMoney;
        public GameObject[] totalItemsForSale;  //Reference to items in scene
        public Text playerCoinUI;
        private float reloadSceneDelay = 1f;

        void Awake()
        {
            availableMoney = PlayerPrefs.GetInt("PlayerMoney");
            playerCoinUI.text = "" + availableMoney;

            //Check if we previously purchased these items.
            for (int i = 0; i < totalItemsForSale.Length; i++)
            {
                //format the correct string we use to store purchased items into playerprefs
                string shopItemName = "shopItem-" + totalItemsForSale[i].GetComponent<ShopItemProperties>().itemIndex.ToString();
                if (PlayerPrefs.GetInt(shopItemName) == 1)
                {
                    totalItemsForSale[i].GetComponent<Button>().interactable = false;               //Not clickable anymore
                    totalItemsForSale[i].GetComponent<ShopItemProperties>().icon.enabled = false;
                }
            }
        }


        public void BuyItem(ShopItemProperties sip)
        {
            StartCoroutine(BuyItemCo(sip));
        }

        public IEnumerator BuyItemCo(ShopItemProperties sip)
        {
            if (availableMoney >= sip.itemPrice)
            {
                availableMoney -= sip.itemPrice;
                PlayerPrefs.SetInt("PlayerMoney", availableMoney);

                //If this is a one-time purchase, save it in playerprefs
                if (!sip.canBuyMultipleTimes)
                {
                    string saveName = "shopItem-" + sip.itemIndex.ToString();
                    PlayerPrefs.SetInt(saveName, 1);
                }

                //Special case for salad item
                if (sip.itemIndex == 4)
                {
                    int savedCandy = PlayerPrefs.GetInt("AvailableCandy", 0);
                    PlayerPrefs.SetInt("AvailableCandy", savedCandy + 5);       //hardcoded - 5 candies for each purchase
                }

                SfxPlayer.instance.PlaySfx(2);
                yield return new WaitForSeconds(reloadSceneDelay);
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                SfxPlayer.instance.PlaySfx(1);
            }
        }

    }
}