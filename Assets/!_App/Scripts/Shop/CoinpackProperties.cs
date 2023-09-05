using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace CookingStar
{
	public class CoinpackProperties : MonoBehaviour
	{
		public int itemIndex;
		public float itemPrice;
		public int itemValue;

		//GameObjects
		public Text valueLabel;
		public Text priceLabel;

		void Start()
		{
			valueLabel.text = itemValue + " Coins";
			priceLabel.text = "$" + itemPrice;
		}

	}
}