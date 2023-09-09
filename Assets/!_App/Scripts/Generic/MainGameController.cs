using System.Collections;
using System.Collections.Generic;
using __App.Scripts.UI;
using CookingStar;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace __App.Scripts.Generic
{
	public class MainGameController : MonoBehaviour
	{
		/// <summary>
		/// This class is the main controller of the game and manages customer creation,
		/// time management, money management, game state and win/lose state.
		/// it also manages available seats in your shop.
		/// </summary>

		//Countdown
		public GameObject countdownObject;
		private float delayedStartTime = 4.5f;  //seconds

		//Tutorial System
		public static bool isTutorial;
		[Header("Tutorial Settings")]
		public GameObject[] tutorialCustomers;
		public static bool isTutorialCustomerPresentInScene;
		public GameObject tutorialModeIndicatorUI;

		//Freeplay goal ballance
		public int freeplayGoalBallance = 10000;

		// Mission Variables (for Career mode)
		internal int availableTime;                 //Seconds
		static public bool canUseCandy;

		// Static Variables //
		static public string gameMode;          //game mode setting (freeplay or career)
		static public float gameTime;
		static public int requiredBalance;
		static public int globalTrashLoss = 15; //when player trashes a product or ingredient, we deduct a fixed loss from player balance.
		public static int totalPenalties;       //Keeping track of player penalties

		//public game objects
		[Space(20)]
		public GameObject[] customers;  //list of all available customers (different patience and textures)

		//public variables
		//How many seats are available in this shop?
		//These two array should always be the same size (Length)
		[Space(20)]
		public bool[] availableSeatForCustomers;
		public Vector3[] seatPositions;
		public GameObject[] additionalItems;    //Items that can be purchased via in-game shop.

		//Delivery queue arrays
		static public bool deliveryQueueIsFull;         //delivery queue can accept 6 ingredients. more is not acceptable.
		static public int deliveryQueueItems;               //number of items in delivery queue
		static public List<int> deliveryQueueItemsContent = new List<int>();    //conents of delivery queue

		///game timer vars
		private string remainingTime;
		private int seconds;
		private int minutes;

		//Cutomers
		private int delay;                  //delay between creating a new customer (smaller number leads to faster customer creation)
		private bool canCreateNewCustomer;  //flag to prevent double calls to functions

		//Money and GameState
		static public int totalMoneyMade;
		//private int totalMoneyLost;
		static public bool gameIsFinished;      //Flag
		static public bool gameIsStarted;       //Flag
		private bool _isDoubleMoneyCollect = false;       //Flag

		///////////////////////////////////////
		static public int slotState = 0;                //available slots for product creation (same as delivery queue)
		static public int maxSlotState;             //maximum available slots in delivery queue (set in init)


		[Header("UI Objects")]
		//public GameObject gameoverPanelHolder;
		public Text moneyText;
		public Text missionText;
		public Text timeText;
		public Text missionTargetUI;
		public Text missionTimeUI;
		public Image resultStarsUI;
		public Text tutorialIsCompletedUI;
		public Text endPanelLabelUI;
		public GameObject topUIHolder;
		public GameObject levelTargetUI;
		public GameObject retryButton;
		public GameObject levelTimeHolder;
		public TextMeshProUGUI moneyCount;

		//Shrink game view to be fully viewable on extra thin devices/Screens
		private Camera mainCam;
		private float currentScreenRatio;
		private float ratioDiff;
		private float defaultCamSize = 5.05f;
		private float defaultScreenRatio = 1.6666f;

		[Header("Resources")]
		public Sprite[] availableStarIcons;

		public void Awake()
		{
			Init();

			//New
			//FixCamSizeInThinDevices();
		}


		/*public void FixCamSizeInThinDevices()
		{
			mainCam = Camera.main;
			currentScreenRatio = Screen.height / (Screen.width + 0.001f);
			ratioDiff = currentScreenRatio - defaultScreenRatio;

			print("Screen.height: " + Screen.height);
			print("Screen.width: " + Screen.width);
			print("currentScreenRatio: " + currentScreenRatio);
			print("ratioDiff: " + ratioDiff);

			if (ratioDiff > 0.2f)
			{
				mainCam.orthographicSize = defaultCamSize + (ratioDiff * 2.5f);
			}
		}*/


		public IEnumerator EnableDelayedStart()
		{
			yield return new WaitForSeconds(delayedStartTime);
			gameIsStarted = true;
			Destroy(countdownObject);
		}


		//***************************************************************************//
		// Init everything here.
		//***************************************************************************//
		void Init()
		{
			Application.targetFrameRate = 60;
			slotState = 0;
			maxSlotState = 6;

			//gameoverPanelHolder.SetActive(false);
			totalPenalties = 0;

			deliveryQueueIsFull = false;
			deliveryQueueItems = 0;
			deliveryQueueItemsContent.Clear();

			totalMoneyMade = 0;
			gameIsFinished = false;
			gameIsStarted = false;

			seconds = 0;
			minutes = 0;

			delay = 6; //Optimal value should be between 5 (Always crowded) and 15 (Not so crowded) seconds. 
			canCreateNewCustomer = false;

			//set all seats as available at the start of the game. No seat is taken yet.
			for (int i = 0; i < availableSeatForCustomers.Length; i++)
			{
				availableSeatForCustomers[i] = true;
			}

			//check if player previously purchased these items..
			//ShopItem index starts from 1.
			for (int j = 0; j < additionalItems.Length; j++)
			{
				//format the correct string we use to store purchased items into playerprefs
				string shopItemName = "shopItem-" + (j + 1).ToString(); ;
				if (PlayerPrefs.GetInt(shopItemName) == 1)
				{
					//we already purchased this item
					additionalItems[j].SetActive(true);
				}
				else
				{
					additionalItems[j].SetActive(false);
				}
			}

			//check game mode.
			if (PlayerPrefs.HasKey("gameMode"))
			{
				gameMode = PlayerPrefs.GetString("gameMode");
			}
			else
			{
				gameMode = "FREEPLAY"; //default game mode
			}


			switch (gameMode)
			{
				case "FREEPLAY":
					requiredBalance = freeplayGoalBallance;
					gameTime = 0;
					canUseCandy = true;
					break;

				case "CAREER":
					requiredBalance = PlayerPrefs.GetInt("careerGoalBallance");
					availableTime = PlayerPrefs.GetInt("careerAvailableTime");
					//check if we are allowed to use candy in this career level
					canUseCandy = (PlayerPrefs.GetInt("canUseCandy") == 1) ? true : false;
					break;
			}

			//Tutorial system
			if (PlayerPrefs.GetInt("IsTutorial") == 1)
			{
				isTutorial = true;
				tutorialModeIndicatorUI.SetActive(true);
				countdownObject.SetActive(false);
				levelTimeHolder.SetActive(false);

				gameIsStarted = true;
			}
			else
			{
				isTutorial = false;
				tutorialModeIndicatorUI.SetActive(false);

				//countdown
				countdownObject.SetActive(true);
				StartCoroutine(EnableDelayedStart());
			}
			//print("isTutorial: " + isTutorial);
			isTutorialCustomerPresentInScene = false;
		}


		//***************************************************************************//
		// Starting delay. Optional.
		//***************************************************************************//
		IEnumerator Start()
		{
			//countdown delay
			if (countdownObject.activeSelf)
			{
				//we need additional waiting time
				yield return new WaitForSeconds(delayedStartTime);
			}

			yield return new WaitForSeconds(1f);
			canCreateNewCustomer = true;
		}


		//***************************************************************************//
		// FSM
		//***************************************************************************//
		void Update()
		{
			//no more ingredient can be picked
			if (deliveryQueueItems >= maxSlotState)
				deliveryQueueIsFull = true;
			else
				deliveryQueueIsFull = false;

			if (!gameIsFinished)
			{
				manageClock();
				manageGuiTexts();
				StartCoroutine(CheckGameWinState());
			}

			//create a new customer if there is free seat and game is not finished yet
			if (canCreateNewCustomer && !gameIsFinished)
			{
				if (monitorAvailableSeats() != 0)
				{
					if (isTutorial)
						createCustomer(freeSeatIndex[1]);
					else
						createCustomer(freeSeatIndex[Random.Range(0, freeSeatIndex.Count)]);
				}
				else
				{
					//print("No free seat is available!");
				}
			}


			//Always keep totalMoney positive
			if (totalMoneyMade < 0)
				totalMoneyMade = 0;
		}


		//***************************************************************************//
		// New Customer creation routine.
		//***************************************************************************//
		void createCustomer(int _seatIndex)
		{
			if (isTutorial && isTutorialCustomerPresentInScene)
				return;

			//set flag to prevent double calls 
			//canCreateNewCustomer = false;
			//StartCoroutine(reactiveCustomerCreation());

			//which customer?
			GameObject tmpCustomer = null;

			if (isTutorial)
			{
				if (TutorialManager.currentTutorialStep == 0)
					tmpCustomer = tutorialCustomers[0];
				else if (TutorialManager.currentTutorialStep == 6)
					tmpCustomer = tutorialCustomers[1];
				else if (TutorialManager.currentTutorialStep == 8)
					tmpCustomer = tutorialCustomers[2];

				isTutorialCustomerPresentInScene = true;
			}
			else
			{
				//Spawn a customer
				int rndCustomer = Random.Range(0, customers.Length);
				tmpCustomer = customers[rndCustomer];
			}

			//set flag to prevent double calls 
			canCreateNewCustomer = false;
			StartCoroutine(reactiveCustomerCreation());

			//For debugging, create a certain customer with ID
			//tmpCustomer = customers[1];

			//which seat
			Vector3 seat = seatPositions[_seatIndex];
			//mark the seat as taken
			availableSeatForCustomers[_seatIndex] = false;

			//create customer
			float offset = -5f;
			GameObject newCustomer = Instantiate(tmpCustomer, new Vector3(offset, seat.y, 0.2f), Quaternion.Euler(0, 0, 0)) as GameObject;

			//any post creation special Attributes?
			newCustomer.GetComponent<CustomerController>().mySeat = _seatIndex;
			//set customer's destination
			newCustomer.GetComponent<CustomerController>().destination = seat;
		}


		//***************************************************************************//
		// customer creation is active again
		//***************************************************************************//
		IEnumerator reactiveCustomerCreation()
		{
			yield return new WaitForSeconds(delay);
			canCreateNewCustomer = true;
			yield break;
		}


		//***************************************************************************//
		// check if there is any free seat for customers and if true, return their index(s)
		//***************************************************************************//
		private List<int> freeSeatIndex = new List<int>();
		int monitorAvailableSeats()
		{
			freeSeatIndex = new List<int>();
			for (int i = 0; i < availableSeatForCustomers.Length; i++)
			{
				if (availableSeatForCustomers[i] == true)
					freeSeatIndex.Add(i);
			}

			//debug
			//print("Available seats: " + freeSeatIndex);

			if (freeSeatIndex.Count > 0)
				return -1;
			else
				return 0;
		}


		//***************************************************************************//
		// GUI text management
		//***************************************************************************//
		void manageGuiTexts()
		{
			moneyText.text = "$" + totalMoneyMade.ToString();
			missionText.text = "$" + requiredBalance.ToString();

			if (gameMode == "CHALLENGE")
			{
				missionText.text = "N/A";
			}
		}


		//***************************************************************************//
		// Game clock manager
		//***************************************************************************//
		void manageClock()
		{
			if (gameIsFinished)
				return;

			//countdown system
			if (countdownObject)
			{
				gameTime = availableTime - Time.timeSinceLevelLoad;
				timeText.text = "";
				return;
			}

			if (gameMode == "FREEPLAY")
			{
				gameTime = Time.timeSinceLevelLoad;
				seconds = Mathf.CeilToInt(Time.timeSinceLevelLoad) % 60;
				minutes = Mathf.CeilToInt(Time.timeSinceLevelLoad) / 60;
				remainingTime = string.Format("{0:00} : {1:00}", minutes, seconds);
				timeText.text = remainingTime.ToString();

			}
			else if (gameMode == "CAREER")
			{
				gameTime = availableTime - Time.timeSinceLevelLoad;
				seconds = Mathf.CeilToInt(availableTime - Time.timeSinceLevelLoad) % 60;
				minutes = Mathf.CeilToInt(availableTime - Time.timeSinceLevelLoad) / 60;
				remainingTime = string.Format("{0:00} : {1:00}", minutes, seconds);
				timeText.text = remainingTime.ToString();
			}
		}



		//***************************************************************************//
		// finish the game gracefully
		//***************************************************************************//
		IEnumerator processGameFinish()
		{
			SfxPlayer.instance.PlaySfx(6);

			yield return new WaitForSeconds(1.5f);  //absolutely required.
			print("game is finished");
			//tell all customers to leave, if they are still in the shop :)))
			GameObject[] customersInScene = GameObject.FindGameObjectsWithTag("customer");
			if (customersInScene.Length > 0)
			{
				foreach (var customer in customersInScene)
				{
					customer.GetComponent<CustomerController>().leave();
				}
			}
			//did we reached the level goal?
			if (totalMoneyMade >= requiredBalance)
			{
				print("We beat the mission :))))");
				SfxPlayer.instance.PlaySfx(7);
			}
			else
			{
				print("better luck next time :((((");
				SfxPlayer.instance.PlaySfx(8);
			}

		}


		/// <summary>
		/// Game Win/Lose State
		/// </summary>
		/// <returns></returns>
		IEnumerator CheckGameWinState()
		{
			if (gameIsFinished)
				yield break;

			if (gameMode == "CAREER" && gameTime <= 0 && totalMoneyMade < requiredBalance)
			{
				endPanelLabelUI.text = "Out of Time!";
				print("Time is up! You have failed :(");    //debug the result
				gameIsFinished = true;                      //announce the new status to other classes
				//gameoverPanelHolder.SetActive(true);        //show the endGame plane
				UIController.instance.ShowScreen(ScreensTypes.VICTORY);
				UIController.instance.SetPlayerMoneyCount(totalMoneyMade);
				
				missionTargetUI.text = totalMoneyMade + " of " + requiredBalance;
				missionTimeUI.text = (int)Time.timeSinceLevelLoad + " of " + availableTime;
				resultStarsUI.sprite = availableStarIcons[0];

				SfxPlayer.instance.PlaySfx(6);
				yield return new WaitForSeconds(2.0f);
				SfxPlayer.instance.PlaySfx(8);
			}
			else if (gameMode == "CAREER" && gameTime > 0 && totalMoneyMade >= requiredBalance)
			{
				endPanelLabelUI.text = "Success!";

				//save career progress
				saveCareerProgress();

				//grant the prize
				int levelPrize = PlayerPrefs.GetInt("careerPrize");
				int currentMoney = PlayerPrefs.GetInt("PlayerMoney");
				currentMoney += levelPrize;
				PlayerPrefs.SetInt("PlayerMoney", currentMoney);

				print("Wow, You beat the level! :)");
				gameIsFinished = true;
				//gameoverPanelHolder.SetActive(true);
				UIController.instance.ShowScreen(ScreensTypes.VICTORY);
				UIController.instance.SetPlayerMoneyCount(levelPrize);

				missionTargetUI.text = totalMoneyMade + " of " + requiredBalance;
				missionTimeUI.text = (int)Time.timeSinceLevelLoad + " of " + availableTime;
				resultStarsUI.sprite = availableStarIcons[GetLevelStars()];

				SfxPlayer.instance.PlaySfx(7);

				//save gametime for level stars system
				PlayerPrefs.SetFloat("Level-" + PlayerPrefs.GetInt("careerLevelID").ToString(), Time.timeSinceLevelLoad);
			}
			else if (gameMode == "FREEPLAY" && totalMoneyMade >= requiredBalance)
			{
				endPanelLabelUI.text = "Bravo!";

				print("Wow, You beat the goal in freeplay mode. But You can continue... :)");
				SfxPlayer.instance.PlaySfx(7);

				//gameIsFinished = true; 
				//we can still play in freeplay mode. 
				//there is no end here unless user stops the game and choose exit.
			}
			else if (TutorialManager.tutorialIsFinished)
			{
				//Tutorial mode is completed successfully
				print("<b>Tutorial is finished!!!</b>");
				gameIsFinished = true;
				timeText.text = "00:00";
				SfxPlayer.instance.PlaySfx(4);
				//Show EngGame panel
				//gameoverPanelHolder.SetActive(true);
				UIController.instance.ShowScreen(ScreensTypes.VICTORY);
				//Show Stats
				missionTargetUI.text = "N/A";
				missionTimeUI.text = "N/A";
				resultStarsUI.sprite = availableStarIcons[3];
				endPanelLabelUI.text = "Congratulations!";
				retryButton.SetActive(false);
				tutorialModeIndicatorUI.SetActive(true);
				tutorialIsCompletedUI.gameObject.SetActive(true);
			}
		}


		public int GetLevelStars()
		{
			int stars = 0;
			float remainingTime = availableTime - Time.timeSinceLevelLoad;

			if (remainingTime >= 60)
				stars = 3;
			else if (remainingTime < 60 && remainingTime >= 30)
				stars = 2;
			else if (remainingTime < 30 && remainingTime > 0)
				stars = 1;
			else
				stars = 0;

			return stars;
		}
		
		public void ForceCustomersToLeave()
		{
			GameObject[] customers = GameObject.FindGameObjectsWithTag("customer");
			if (customers.Length > 0)
			{
				foreach (GameObject c in customers)
				{
					StartCoroutine(c.GetComponent<CustomerController>().leave());
				}
			}
		}


		//********************************************************
		// Save user progress in career mode.
		//********************************************************
		void saveCareerProgress()
		{
			int currentLevelID = PlayerPrefs.GetInt("careerLevelID");
			int userLevelAdvance = PlayerPrefs.GetInt("userLevelAdvance");

			//if this is the first time we are beating this level...
			if (userLevelAdvance < currentLevelID)
			{
				userLevelAdvance++;
				PlayerPrefs.SetInt("userLevelAdvance", userLevelAdvance);
			}
		}
		
		public void DoubleReward()
		{
			if(_isDoubleMoneyCollect) return;
			int levelPrize = PlayerPrefs.GetInt("careerPrize");
			int currentMoney = PlayerPrefs.GetInt("PlayerMoney");
			currentMoney += levelPrize;
			PlayerPrefs.SetInt("PlayerMoney", currentMoney);
			levelPrize *= 2;
			UIController.instance.SetPlayerMoneyCount(levelPrize);
			_isDoubleMoneyCollect = true;
		}


		///***********************************************************************
		/// play normal audio clip
		///***********************************************************************
		void playNormalSfx(AudioClip _sfx)
		{
			if (!FbMusicPlayer.globalSoundState)
				return;

			GetComponent<AudioSource>().clip = _sfx;
			if (!GetComponent<AudioSource>().isPlaying)
				GetComponent<AudioSource>().Play();
		}
		
	}
}