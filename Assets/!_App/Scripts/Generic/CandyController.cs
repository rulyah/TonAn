using UnityEngine;
using System.Collections;
using __App.Scripts.Generic;

namespace CookingStar
{
	public class CandyController : MonoBehaviour
	{
		/// <summary>
		/// This class handles dragging candies to customers, monitoring available candies, and
		/// an option to buy more candies for the shop.
		/// </summary>

		public static bool canDeliverCandy;     //can we give the candy to a customer?
		public static int availableCandy;       //how many candy we have
		public int candyPrice = 20;             //price of candy, if we want to buy more
		public GameObject candyGO;              //reference to candy prefab
		public GameObject ingameCandyIcon;      //child candy object
		public GameObject availableCandyText;   //child text
		private float delayTime = 0.25f;        //after this delay, we let player to be able to choose another ingredient again
		private bool canCreate;                 //cutome flag to prevent double picking
		private Renderer rend;
		private float sizeIncreasePercentOnDrag = 1.1f; //10% Percent 

		void Awake()
		{
			canDeliverCandy = false;
			canCreate = true;
			availableCandy = PlayerPrefs.GetInt("AvailableCandy", 0);
			rend = ingameCandyIcon.GetComponent<Renderer>();
		}

		void Start()
		{
			//check if we are allowed to use candy in this level
			if (!MainGameController.canUseCandy)
				gameObject.SetActive(false);
		}

		void Update()
		{
			//let player give candy to customers
			ManagePlayerDrag();

			//always count available candies
			UpdateAvailableCandies();

			if (availableCandy > 0 || MainGameController.isTutorial)
			{
				//looks like active
				rend.material.color = new Color(rend.material.color.r, rend.material.color.g, rend.material.color.b, 1);
			}
			else
			{
				//make it looks like inactive
				rend.material.color = new Color(rend.material.color.r, rend.material.color.g, rend.material.color.b, 0.5f);
			}
		}


		/// <summary>
		/// Always count available candies and show it in-game.
		/// </summary>
		void UpdateAvailableCandies()
		{
			if (availableCandy < 0)
				availableCandy = 0;

			availableCandyText.GetComponent<TextMesh>().text = "x" + availableCandy.ToString();
		}


		/// <summary>
		/// If player has dragged a candy, make an instance of it, then
		/// follow players touch/mouse position.
		/// </summary>
		private RaycastHit hitInfo;
		private Ray ray;
		void ManagePlayerDrag()
		{
			if (Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Moved)
				ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
			else if (Input.GetMouseButtonDown(0))
				ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			else
				return;

			if (Physics.Raycast(ray, out hitInfo))
			{
				GameObject objectHit = hitInfo.transform.gameObject;
				if (objectHit.tag == "candy" && !IngredientsController.itemIsInHand && (availableCandy > 0 || MainGameController.isTutorial))
				{
					CreateCandy();
				}
			}
		}


		/// <summary>
		/// Create an instance of Candy
		/// </summary>
		void CreateCandy()
		{
			if (canCreate && !MainGameController.gameIsFinished)
			{
				GameObject candy = Instantiate(candyGO, transform.position + new Vector3(0, 0, -1), Quaternion.Euler(0, 0, 0)) as GameObject;
				candy.name = "DeliveryCandy";
				candy.tag = "deliveryCandy";
				canDeliverCandy = true;
				candy.GetComponent<BoxCollider>().enabled = false;
				candy.transform.localScale = new Vector3(sizeIncreasePercentOnDrag, sizeIncreasePercentOnDrag, 0.001f);
				SfxPlayer.instance.PlaySfx(5);
				canCreate = false;
				IngredientsController.itemIsInHand = true; //prevent user from picking an ingredient when trying to deliver a cany!
				StartCoroutine(reactivate());
			}
		}


		/// <summary>
		/// make this ingredient draggable again
		/// </summary>
		/// <returns></returns>
		public IEnumerator reactivate()
		{
			yield return new WaitForSeconds(delayTime);
			canCreate = true;
		}

	}
}