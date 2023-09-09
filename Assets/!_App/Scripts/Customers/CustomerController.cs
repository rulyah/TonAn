using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using __App.Scripts.Generic;

namespace CookingStar
{
    public class CustomerController : MonoBehaviour
    {
        /// <summary>
        /// This class handles every thing related to a customer, including the wishlist, ingredients, patience and animations.
        /// </summary>

        [Header("Customer Settings")]
        public float customerPatience = 30.0f;              //seconds (default = 30 or whatever you wish)
        public int customerNeeds;                           //Product ID which the customer wants. if left to 0, it randomly chooses a product.
                                                            //set anything but 0 to override it.
                                                            //max limit is the length of the availableProducts array.    

        public int customerSidereqNeeds = -1;               //Notice! tutorial customers should be able to ask for specific sidereq items (coffee)
        public float customerMoveSpeed = 3f;
        public float askingSideRequestChance = 0.4f;
        public float sideReqWithNoMainOrderChance = 0.2f;

        [Space(20)]
        public GameObject positionDummy;                    //dummy to indicate where items should be displayed over customer's head

        //*** Customer Moods ***//
        //We use different materials for each mood
        /* Currently we have 4 moods: 
         [0]Defalut
         [1]Bored 
         [2]Satisfied
         [3]Angry
         we change to appropriate material whenever needed. 
        */
        public Material[] customerMoods;
        public GameObject customerBody;
        private int moodIndex;
        internal int mySeat;                        //Do not modify this.
        internal Vector3 destination;               //Do not modify this.
        private GameObject gameController;          //Do not modify this.
        public bool isCloseEnoughToDelivery;        //Do not modify this.
        public bool isCloseEnoughToCandy;           //Do not modify this. 
        public bool isCloseEnoughToSideRequest;     //Do not modify this.
        public GameObject[] availableProducts;      //list of all available product to choose from
        public GameObject[] availableIngredients;   //List of all available ingredients to cook above products
        public GameObject[] availableSideReqs;      //List of all available side-request items

        //Customer variables
        private string customerName;                //random name
        private int productIngredients;             //ingredients of the choosen product
        private int[] productIngredientsIDs;        //IDs of the above ingredients
        internal float currentCustomerPatience;     //current patience of the customer
        private bool isOnSeat;                      //is customer on his seat?
        private int customerSideReq;                //-1 means no side-request. anything but -1 points to a side request.
        private bool sideRequestIsFulfilled;        //flag to let us know if we have delivered the side-request
        private bool mainOrderIsFulfilled;          //flag to let us know if we have delivered the main order
        private bool isOnlyAskingForDrink;          //flag to let us know if this customer only wants a drink (side-req) and no main food

        //Patience bar GUI items and vars
        private bool patienceBarSliderFlag;
        internal float leaveTime;
        private float creationTime;
        private bool isLeaving;
        public GameObject requestBubble;            //reference to (gameObject) bubble over customers head
        public GameObject money3dText;              //UI text over customers head after successful delivery

        //New patience Bar
        public GameObject patienceBarMain;
        public GameObject patienceBarFgPivot;

        //Colored patience bar
        public Renderer patienceBarBody;
        public Material[] availablePatienceBarColors;

        //New visuals
        public GameObject visualGfx;
        public GameObject coinsGfx;

        //Transforms
        private Vector3 startingPosition;

        //All about more than 1 plate goes here:
        private GameObject[] serverPlates;          //all available plates inside the game
        private float[] distanceToPlates;           //distance to all available plates
        private GameObject target;                  //which plate is nearest? that will be the main target

        //Special character adjustment
        private float clientOffsetY = -0.3f;
        private float ySineDivision = 14f;

        [Space(20)]
        public int tutorialCustomerID = 0;          //Not needed in the main game. This is only needed in tutorial mode


        void Awake()
        {
            target = null;
            serverPlates = GameObject.FindGameObjectsWithTag("serverPlate");
            distanceToPlates = new float[serverPlates.Length];

            requestBubble.SetActive(false);
            patienceBarMain.SetActive(false);

            customerSideReq = -1;
            isCloseEnoughToDelivery = false;
            isCloseEnoughToCandy = false;
            isCloseEnoughToSideRequest = false;
            sideRequestIsFulfilled = false;
            mainOrderIsFulfilled = false;
            isOnlyAskingForDrink = false;

            isOnSeat = false;
            currentCustomerPatience = customerPatience;
            moodIndex = 0;
            leaveTime = 0;
            isLeaving = false;
            creationTime = Time.time;
            startingPosition = transform.position;
            gameController = GameObject.FindGameObjectWithTag("GameController");

            Init();
            StartCoroutine(GoToSeat());
        }


        /// <summary>
        /// Here we will initialize all customer related variables.
        /// </summary>
        private GameObject productImage;
        private GameObject sideReq;
        void Init()
        {
            //we give this customer a nice name
            customerName = "Customer_" + Random.Range(100, 10000);
            gameObject.name = customerName;

            //choose a product for this customer
            if (customerNeeds == 0)
            {
                //for freeplay mode, customers can choose any product. But in career mode,
                //they have to choose from products we allowed in CareerLevelSetup class.
                if (PlayerPrefs.GetString("gameMode") == "CAREER")
                {
                    int totalAvailableProducts = PlayerPrefs.GetInt("availableProducts");
                    customerNeeds = PlayerPrefs.GetInt("careerProduct_" + Random.Range(0, totalAvailableProducts).ToString());
                    customerNeeds -= 1; //Important. We count the indexes from zero, while selecting the products from 1.
                                        //so we subtract a unit from customerNeeds to be equal to main AvailableProducts array.
                }
                else
                {
                    customerNeeds = Random.Range(0, availableProducts.Length);
                }
            }

            //also let's give this customer a chance to wish a side-request.
            if (askingSideRequestChance > Random.value)
                customerSideReq = Random.Range(0, availableSideReqs.Length);
            else
                customerSideReq = -1;

            //give the customers a chance to ask for no main food, and just ask for a drink (side-req)
            //with 20% chance, we create a customer that only wants a drink.
            if (sideReqWithNoMainOrderChance > Random.value) //sideReqWithNoMainOrderChance
            {
                isOnlyAskingForDrink = true;    //only wants a drink
                mainOrderIsFulfilled = true;    //we set this flag to true instantly, as this customer does not have any main order.

                if (customerSidereqNeeds == -1)
                    customerSideReq = Random.Range(0, availableSideReqs.Length);    //we force him to order a side-req!
                else
                    customerSideReq = customerSidereqNeeds;

                print(customerName + " only wants a drink! (no main order)");
            }

            //debug customer's wish	
            if (!isOnlyAskingForDrink)
            {
                print(customerName + " would like a " + availableProducts[customerNeeds].name + " and can wait for " + customerPatience + " seconds");
                if (customerSideReq != -1)
                    print(customerName + " also likes a " + availableSideReqs[customerSideReq].name);
            }

            //get and show product's image
            productImage = Instantiate(availableProducts[customerNeeds], positionDummy.transform.position, Quaternion.Euler(0, 0, 0)) as GameObject;
            productImage.name = "customerNeeds";
            productImage.transform.localScale *= 0.4f;
            productImage.transform.parent = requestBubble.transform;
            productImage.transform.localPosition = new Vector3(-0.07f, productImage.transform.localPosition.y, productImage.transform.localPosition.z); //Adjust X pos

            //if customer wants a side-request, show it
            if (customerSideReq != -1)
            {
                sideReq = Instantiate(availableSideReqs[customerSideReq], positionDummy.transform.position + new Vector3(0, -0.175f, 0), Quaternion.Euler(0, 0, 0)) as GameObject;

                //move the main order a little up! to make some space for the side-request
                productImage.transform.position = new Vector3(productImage.transform.position.x - 0.05f, productImage.transform.position.y + 0.25f, productImage.transform.position.z);
                productImage.transform.localScale *= 0.8f;
                sideReq.transform.localScale = new Vector3(0.7f, 0.7f, 0.001f);
                sideReq.transform.localScale *= 0.7f;
                sideReq.transform.parent = requestBubble.transform;
                sideReq.transform.localPosition = new Vector3(-0.05f, sideReq.transform.localPosition.y - 0.15f, sideReq.transform.localPosition.z); //Reset X pos

                //if this side-req item needs to be processed, show it (ask it) with processed image :)
                if (sideReq.GetComponent<SideRequestMover>().beforeAfterMat.Length > 1)
                    sideReq.GetComponent<SideRequestMover>().SetMaterial(1);

                sideReq.GetComponent<SideRequestMover>().enabled = false;
            }

            //we need to hide the image of main order, if this customer only wants a side-req (drink)
            if (isOnlyAskingForDrink)
            {
                sideReq.transform.position = positionDummy.transform.position;
                sideReq.transform.localPosition = new Vector3(-0.02f, sideReq.transform.localPosition.y, sideReq.transform.localPosition.z); //Reset X pos
                sideReq.transform.localScale *= 1.5f;

                productImage.SetActive(false);
            }

            //get product ingredients
            productIngredients = availableProducts[customerNeeds].GetComponent<ProductManager>().totalIngredients;
            //print(availableProducts[customerNeeds].name + " has " + productIngredients + " ingredients.");
            productIngredientsIDs = new int[productIngredients];
            for (int i = 0; i < productIngredients; i++)
            {
                productIngredientsIDs[i] = availableProducts[customerNeeds].GetComponent<ProductManager>().ingredientsIDs[i];
                //print(availableProducts[customerNeeds].name + " ingredients ID[" + i + "] is: " + productIngredientsIDs[i]);
            }
        }


        /// <summary>
        /// After this customer has been instantiated by MainGameController,
        /// it starts somewhere outside game scene and then go to it's position (seat)
        /// with a nice animation. Then asks for it's order.
        /// </summary>
        private float timeVariance;
        IEnumerator GoToSeat()
        {
            //print(gameObject.name + " spawn Position: " + transform.position);
            timeVariance = Random.value;
            while (!isOnSeat)
            {
                transform.position = new Vector3(transform.position.x + (Time.deltaTime * customerMoveSpeed),
                                                 startingPosition.y + clientOffsetY + (Mathf.Sin((Time.time + timeVariance) * 10) / ySineDivision),
                                                 transform.position.z);

                if (transform.position.x >= destination.x)
                {
                    isOnSeat = true;
                    patienceBarSliderFlag = true; //start the patience bar
                    requestBubble.SetActive(true);
                    patienceBarMain.SetActive(true);

                    //Tutorial
                    if (MainGameController.isTutorial)
                    {
                        if (tutorialCustomerID == 1)
                            TutorialManager.instance.UpdateTutorialStep(0);
                        else if (tutorialCustomerID == 2)
                            TutorialManager.instance.UpdateTutorialStep(7);
                        else if (tutorialCustomerID == 3)
                            TutorialManager.instance.UpdateTutorialStep(9);
                    }

                    yield break;
                }
                yield return 0;
            }
        }


        /// <summary>
        /// Takes an array and return the lowest number in it, 
        /// with the optional index of this lowest number (position of it in the array)
        /// </summary>
        /// <param name="_array"></param>
        /// <returns></returns>
        Vector2 findMinInArray(float[] _array)
        {
            int lowestIndex = -1;
            float minval = 1000000.0f;
            for (int i = 0; i < _array.Length; i++)
            {
                if (_array[i] < minval)
                {
                    minval = _array[i];
                    lowestIndex = i;
                }
            }
            return (new Vector2(minval, lowestIndex));
        }


        void Update()
        {
            for (int i = 0; i < serverPlates.Length; i++)
            {
                distanceToPlates[i] = Vector3.Distance(serverPlates[i].transform.position, gameObject.transform.position);
                //find the correct (nearest plate) target
                target = serverPlates[(int)findMinInArray(distanceToPlates).y];
                //print (gameObject.name + " target plate is: " + target.name);
            }

            if (patienceBarSliderFlag)
                StartCoroutine(patienceBar());

            //Manage customer's mood by changing it's material
            UpdateCustomerMood();

            //Update patience bar color
            UpdatePatienceBarColor();
        }


        /// <summary>
        /// colored patience bar
        /// </summary>
        public void UpdatePatienceBarColor()
        {
            if (currentCustomerPatience <= customerPatience && currentCustomerPatience > customerPatience / 2f)
            {
                patienceBarBody.material = availablePatienceBarColors[0];
            }
            else if (currentCustomerPatience <= customerPatience / 2f && currentCustomerPatience > customerPatience / 4f)
            {
                patienceBarBody.material = availablePatienceBarColors[1];
            }
            else if (currentCustomerPatience <= customerPatience / 4f && currentCustomerPatience >= 0)
            {
                patienceBarBody.material = availablePatienceBarColors[2];
            }
        }


        /// <summary>
        /// We need these events to be checked at the eend of every frame.
        /// </summary>
        void LateUpdate()
        {

            //check if this customer is close enough to delivery, in order to receive it.
            if (target)
            {
                if (target.GetComponent<PlateController>().isReadyToServe)
                    CheckDistanceToDelivery();
            }

            //check if this customer is close enough to a candy, in order to receive it.
            if (CandyController.canDeliverCandy && gameObject.tag == "customer")
            {
                CheckDistanceToCandy();
            }

            //check if this customer is close enough to a side-request delivery, in order to receive it.
            //we must also check if this customer wants a side-request or not!
            if (SideRequestsController.canDeliverSideRequest && customerSideReq != -1 && gameObject.tag == "customer")
            {
                CheckDistanceToSideRequest();
            }

            //check the status of customers with both main order and side-request, and look if they already received both their orders.
            if (sideRequestIsFulfilled && mainOrderIsFulfilled && !isLeaving)
            {
                Settle();
            }
        }


        /// <summary>
        /// Make the customer react to events by changing it's material (and texture)
        /// </summary>
        void UpdateCustomerMood()
        {
            //if customer has waited for half of his/her patience, make him/her bored.
            if (!isLeaving)
            {
                if (currentCustomerPatience <= customerPatience / 2)
                    moodIndex = 1;
                else
                    moodIndex = 0;
            }

            customerBody.GetComponent<Renderer>().material = customerMoods[moodIndex];
        }


        /// <summary>
        /// Fill customer's patience bar and make it full again.
        /// </summary>
        void FillCustomerPatience()
        {
            currentCustomerPatience = customerPatience;
            patienceBarFgPivot.transform.localScale = new Vector3(1, 1, 100);
        }

        /// <summary>
        /// check distance to delivery
        /// we check if player is dragging the order towards the customer or towards it's
        /// requestBubble. the if the order was close enough to any of these objects,
        /// we se the deliver flag to true.
        /// </summary>
        private float distanceToDelivery;
        private float frameDistanceToDelivery;
        void CheckDistanceToDelivery()
        {
            distanceToDelivery = Vector3.Distance(transform.position, target.transform.position);
            frameDistanceToDelivery = Vector3.Distance(requestBubble.transform.position, target.transform.position);
            //print(gameObject.name + " distance to candy is: " + distanceToDelivery + ".");

            if (distanceToDelivery < 1.1f || frameDistanceToDelivery < 1.1f)
            {
                isCloseEnoughToDelivery = true;
                customerBody.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f);
            }
            else
            {
                isCloseEnoughToDelivery = false;
                customerBody.GetComponent<Renderer>().material.color = new Color(1, 1, 1);
            }
        }


        /// <summary>
        /// check distance to Candy
        /// </summary>
        private float distanceToCandy;
        private GameObject deliveryCandy;
        void CheckDistanceToCandy()
        {
            //first find the deliveryCandy GameObject
            if (!deliveryCandy)
            {
                //do we already found a candy?
                deliveryCandy = GameObject.FindGameObjectWithTag("deliveryCandy");
            }

            distanceToCandy = Vector3.Distance(transform.position, deliveryCandy.transform.position);
            //print(gameObject.name + " distance to candy is: " + distanceToCandy + ".");

            if (distanceToCandy < 1.1f)
            {
                isCloseEnoughToCandy = true;
                customerBody.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f);
            }
            else
            {
                isCloseEnoughToCandy = false;
                customerBody.GetComponent<Renderer>().material.color = new Color(1, 1, 1);
            }
        }


        /// <summary>
        /// check distance to side-request
        /// </summary>
        private float distanceToSideRequest;
        private float frameDistanceToSideRequest;
        private GameObject deliverySideRequest;
        void CheckDistanceToSideRequest()
        {
            //first find the delivery SideRequest GameObject
            if (!deliverySideRequest)
            {
                deliverySideRequest = GameObject.FindGameObjectWithTag("deliverySideRequest");
            }

            distanceToSideRequest = Vector3.Distance(transform.position, deliverySideRequest.transform.position);
            frameDistanceToSideRequest = Vector3.Distance(requestBubble.transform.position, deliverySideRequest.transform.position);

            if (distanceToSideRequest < 1.1f || frameDistanceToSideRequest < 1.1f)
            {
                isCloseEnoughToSideRequest = true;
                customerBody.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f);
            }
            else
            {
                isCloseEnoughToSideRequest = false;
                customerBody.GetComponent<Renderer>().material.color = new Color(1, 1, 1);
            }
        }


        /// <summary>
        /// Show and animate progress bar based on customer's patience
        /// </summary>
        /// <returns></returns>
        IEnumerator patienceBar()
        {
            patienceBarSliderFlag = false;

            //Tutorial on candy usage
            //we need this customer to portrait very small patience so we can use candy to fill his patience bar
            if (tutorialCustomerID == 3)
            {
                currentCustomerPatience = 140f;
            }

            while (currentCustomerPatience > 0)
            {
                currentCustomerPatience -= Time.deltaTime * Application.targetFrameRate * 0.02f;
                patienceBarFgPivot.transform.localScale = new Vector3(1, currentCustomerPatience / customerPatience, 100);
                yield return 0;
            }

            if (currentCustomerPatience <= 0)
            {
                patienceBarMain.SetActive(false);
                //customer is angry and will leave with no food received.
                StartCoroutine(leave());
            }
        }


        /// <summary>
        /// We need to silently deliver the dish using quick delivery method to see if any customer is looking for this dish
        /// </summary>
        /// <param name="_getOrder"></param>
        public bool ConfirmQuickOrder(List<int> _getOrder)
        {
            int[] myOriginalOrder = productIngredientsIDs;
            List<int> myReceivedOrder = _getOrder;
            if (myOriginalOrder.Length == myReceivedOrder.Count)
            {
                //2.compare two arrays
                bool detectInequality = false;
                for (int i = 0; i < myOriginalOrder.Length; i++)
                {
                    if (myOriginalOrder[i] != myReceivedOrder[i])
                    {
                        detectInequality = true;
                    }
                }
                if (!detectInequality)
                    return true;
                else
                    return false;
            }
            else    //different array length
                return false;
        }


        /// <summary>
        /// receive and check order contents
        /// </summary>
        /// <param name="_getOrder"></param>
        public void ReceiveOrder(List<int> _getOrder)
        {
            //print("Order received. contents are: " + _getOrder);

            //check the received order with the original one (customer's wish).
            int[] myOriginalOrder = productIngredientsIDs;
            List<int> myReceivedOrder = _getOrder;

            //check if the two array are the same, meaning that we received what we were looking for.
            //print(myOriginalOrder + " - " + myReceivedOrder);

            //1.check the length of two arrays
            if (myOriginalOrder.Length == myReceivedOrder.Count)
            {
                //2.compare two arrays
                bool detectInequality = false;
                for (int i = 0; i < myOriginalOrder.Length; i++)
                {
                    if (myOriginalOrder[i] != myReceivedOrder[i])
                    {
                        detectInequality = true;
                    }
                }

                if (!detectInequality)
                    OrderIsCorrect();
                else
                    OrderIsIncorrect(); //different array items

            }
            else    //different array length
                OrderIsIncorrect();
        }


        /// <summary>
        /// If order is delivered correctly
        /// </summary>
        void OrderIsCorrect()
        {
            print("Order is correct.");

            //if the customer just asked for an order with NO sideRequest, we make him settle and leave instantly.
            if (customerSideReq == -1)
            {
                Settle();
            }
            else
            {
                //But if it also wants a side-request, we enter another loop to check if the second wish is fulfilled or not.
                //first hide the main order
                productImage.transform.GetChild(0).gameObject.SetActive(false);
                //set the flag
                mainOrderIsFulfilled = true;
                SfxPlayer.instance.PlaySfx(13);
            }
        }


        /// <summary>
        /// If order is NOT delivered correctly
        /// </summary>
        void OrderIsIncorrect()
        {
            print("Order is not correct.");
            moodIndex = 3;
            SfxPlayer.instance.PlaySfx(1);
            StartCoroutine(leave());
        }


        /// <summary>
        /// Receive a candy/salad to refill the patience bar :)
        /// </summary>
        public void ReceiveCandy()
        {
            //Candy Received!
            print(gameObject.name + " received a candy!");
            SfxPlayer.instance.PlaySfx(13);
            //fill customer's patient
            FillCustomerPatience();

            //reduce 1 candy from inventory and save the new amount
            int remainingCandies = PlayerPrefs.GetInt("AvailableCandy", 0);
            remainingCandies--;
            if (remainingCandies < 0)
                remainingCandies = 0;
            PlayerPrefs.SetInt("AvailableCandy", remainingCandies);
        }


        /// <summary>
        /// Receive a side-request
        /// </summary>
        /// <param name="_ID"></param>
        public void ReceiveSideRequest(int _ID)
        {
            //side-request Received!
            print(gameObject.name + " received a side-request, ID: " + (_ID - 1) + " - Original request ID was: " + customerSideReq);

            //check if this is what we want
            if (customerSideReq == _ID - 1)
            {
                //we subtract 1 from _ID, because we start side-request items from 0, while Ids starts from 1.
                //we received the right side-request, so the side-req is fulfilled. But we should wait for the main order status.
                //first hide the side-request
                sideReq.SetActive(false);
                SfxPlayer.instance.PlaySfx(13);
                sideRequestIsFulfilled = true;
            }
            else
            {
                //we received the wrong side-request
                OrderIsIncorrect();
            }
        }


        /// <summary>
        /// Customer should pay and leave the restaurant.
        /// </summary>
        void Settle()
        {
            moodIndex = 2;  //make him/her happy :)

            //give cash, money, bonus, etc, here.
            float leaveTime = Time.time;
            int remainedPatienceBonus = (int)Mathf.Round(customerPatience - (leaveTime - creationTime));

            //bug fix
            if (remainedPatienceBonus < 0)
                remainedPatienceBonus = 0;

            //if we have purchased additional items for our restaurant, we should receive more tips
            int tips = 0;
            if (PlayerPrefs.GetInt("shopItem-1") == 1) tips += 4;   //if we have chair
            if (PlayerPrefs.GetInt("shopItem-2") == 1) tips += 7;   //if we have music player
            if (PlayerPrefs.GetInt("shopItem-3") == 1) tips += 10;   //if we have flowers

            //to calculate the final money, first check if this order contains a sideRequest or maybe only consists of a sideRequest.
            //as we need to calculate the price of the sideReq items as well.
            int sideReqPrice = 0;
            int mainOrderPrice = 0;

            //if customer is only asking for a drink with no main order
            if (isOnlyAskingForDrink)
            {
                mainOrderPrice = 0;
                sideReqPrice = availableSideReqs[customerSideReq].GetComponent<SideRequestMover>().sideReqPrice;

            }
            else
            {
                //calculate the price of main order
                mainOrderPrice = availableProducts[customerNeeds].GetComponent<ProductManager>().price;

                //if customer also wants a sideRed item, calculate it as well
                if (customerSideReq != -1)
                    sideReqPrice = availableSideReqs[customerSideReq].GetComponent<SideRequestMover>().sideReqPrice;
            }

            //debug
            print("mainOrderPrice: " + mainOrderPrice + " // sideReqPrice: " + sideReqPrice);
            int finalMoney = mainOrderPrice + sideReqPrice + remainedPatienceBonus + tips;
            print("FinalMoney: " + mainOrderPrice + "/" + sideReqPrice + "/" + remainedPatienceBonus + "/" + tips);

            MainGameController.totalMoneyMade += finalMoney;
            GameObject money3d = Instantiate(money3dText, transform.position + new Vector3(0, 0, -0.8f), Quaternion.Euler(0, 0, 0)) as GameObject;
            print("FinalMoney: " + finalMoney);
            money3d.GetComponent<TextMeshController>().myText = "$" + finalMoney.ToString();

            //Create coins gfx
            GameObject coinGfx = Instantiate(coinsGfx, transform.position + new Vector3(0, -0.5f, -0.8f), Quaternion.Euler(0, 0, 0)) as GameObject;

            //Create nice visuals if customer patience is >= 50%
            if (currentCustomerPatience >= customerPatience / 2f)
            {
                StartCoroutine(CreateVisuals());
            }

            SfxPlayer.instance.PlaySfx(2);
            StartCoroutine(leave());
        }


        public IEnumerator CreateVisuals()
        {
            yield return new WaitForSeconds(0.3f);
            GameObject nicejobGfx = Instantiate(visualGfx, transform.position + new Vector3(0, 2f, -0.8f), Quaternion.Euler(0, 0, 0)) as GameObject;
            nicejobGfx.name = "VisualGfx";

            //Sfx
            SfxPlayer.instance.PlaySfx(4);
        }


        /// <summary>
        /// Leave routine with get/set ers and animations.
        /// </summary>
        /// <returns></returns>
        public IEnumerator leave()
        {
            //prevent double animation
            if (isLeaving)
                yield break;
            isLeaving = true;

            //we need to change the tag of this customer, in order to not be able to receive new deliveries.
            gameObject.tag = "Untagged";

            //animate (close) patienceBar
            StartCoroutine(animate(Time.time, patienceBarMain, 0.7f, 0.8f));
            yield return new WaitForSeconds(0.3f);

            //animate (close) request bubble
            StartCoroutine(animate(Time.time, requestBubble, 0.75f, 0.95f));
            yield return new WaitForSeconds(0.4f);

            while (transform.position.x < 10)
            {
                transform.position = new Vector3(transform.position.x + (Time.deltaTime * customerMoveSpeed),
                                                 startingPosition.y + clientOffsetY + (Mathf.Sin(Time.time * 10) / ySineDivision),
                                                 transform.position.z);

                if (transform.position.x >= 6)
                {
                    gameController.GetComponent<MainGameController>().availableSeatForCustomers[mySeat] = true;
                    Destroy(gameObject);
                    yield break;
                }
                yield return 0;
            }
        }


        /// <summary>
        /// Simply animate the customer.
        /// </summary>
        /// <param name="_time"></param>
        /// <param name="_go"></param>
        /// <param name="_in"></param>
        /// <param name="_out"></param>
        /// <returns></returns>
        IEnumerator animate(float _time, GameObject _go, float _in, float _out)
        {
            float t = 0.0f;
            while (t <= 1.0f)
            {
                t += Time.deltaTime * 10;
                _go.transform.localScale = new Vector3(Mathf.SmoothStep(_in, _out, t),
                                                       _go.transform.localScale.y,
                                                       _go.transform.localScale.z);
                yield return 0;
            }
            float r = 0.0f;
            if (_go.transform.localScale.x >= _out)
            {
                while (r <= 1.0f)
                {
                    r += Time.deltaTime * 2;
                    _go.transform.localScale = new Vector3(Mathf.SmoothStep(_out, 0.01f, r),
                                                           _go.transform.localScale.y,
                                                           _go.transform.localScale.z);
                    if (_go.transform.localScale.x <= 0.01f)
                        _go.SetActive(false);
                    yield return 0;
                }
            }
        }
    }
}