using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace CookingStar
{
    public class ShopItemProperties : MonoBehaviour
    {
        public int itemIndex;
        public int itemPrice;
        public Text priceTag;
        public Image icon;
        public bool canBuyMultipleTimes = false;

        void Awake()
        {
            priceTag.text = "" + itemPrice;

            //if we already purchased this item...
            if (PlayerPrefs.GetInt("shopItem-" + itemIndex) == 1)
            {
                priceTag.text = "Purchased";
            }
        }
    }
}