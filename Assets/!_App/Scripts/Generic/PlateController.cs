using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using __App.Scripts.Generic;

namespace CookingStar
{
    public class PlateController : MonoBehaviour
    {
        /// <summary>
        /// This calss will manages all things related to the deliveryPlate, including
        /// handing it over to the customers, trashing it, drag and release events, etc...
        /// </summary>

        internal bool canDeliverOrder;  //Static variable to let customers know they can receive their orders
        internal bool isReadyToServe;   //if we are done and have picked up the final product to deliver it to customers

        //Private flags
        private Vector3 initialPosition;
        private GameObject trashbin;

        //Money fx
        public GameObject money3dText;

        //Delivery arrays
        internal bool deliveryQueueIsFull;                                      //delivery queue can accept 6 ingredients. more is not acceptable.
        internal int deliveryQueueItems;                                        //number of items in delivery queue
        internal List<int> deliveryQueueItemsContent = new List<int>();         //conents of delivery queue

        //component cache
        private Renderer r;

        //Using different images for each plate. we need a way to center items on each plate
        public float itemPositionAdjustment = 0;


        void Awake()
        {
            canDeliverOrder = true;
            isReadyToServe = false;

            initialPosition = transform.position;
            trashbin = GameObject.FindGameObjectWithTag("trashbin");

            //clear & reset the delivery arrays
            deliveryQueueIsFull = false;
            deliveryQueueItems = 0;
            deliveryQueueItemsContent.Clear();

            r = GetComponent<Renderer>();
        }


        void Update()
        {
            //no more ingredient can be picked
            if (deliveryQueueItems >= MainGameController.maxSlotState)
                deliveryQueueIsFull = true;
            else
                deliveryQueueIsFull = false;

            ManageDeliveryDrag();
        }


        /// <summary>
        /// If we are starting our drag on deliveryPlate, move the plate with our touch/mouse...
        /// </summary>
        private RaycastHit hitInfo;
        private Ray ray;
        void ManageDeliveryDrag()
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
                if (objectHit.tag == "serverPlate" && objectHit.name == gameObject.name && !IngredientsController.itemIsInHand)
                {
                    StartCoroutine(CreateDeliveryPackage());
                }
            }
        }


        /// <summary>
        /// Check how many customers are available and active in scene
        /// </summary>
        /// <returns></returns>
        public int GetCustomersInScene()
        {
            int customersInScene = 0;
            GameObject[] cis = GameObject.FindGameObjectsWithTag("customer");
            customersInScene = cis.Length;
            //print("customersInScene: " + customersInScene);
            return customersInScene;
        }


        private Vector3 _Pos;
        IEnumerator CreateDeliveryPackage()
        {
            while (canDeliverOrder && deliveryQueueItems > 0)
            {
                //follow mouse or touch
                _Pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                _Pos = new Vector3(_Pos.x, _Pos.y, -0.5f);

                //follow player's finger
                transform.position = _Pos + new Vector3(0, 0, 0);

                //while we are dragging the product, this var will be true. but as soon as we released the drag, it will be false.
                isReadyToServe = true;

                //better to be transparent, when dragged
                r.material.color = new Color(r.material.color.r, r.material.color.g, r.material.color.b, 0.5f);

                //deliver (dragging the plate) is not possible when user is not touching screen
                //so we must decide what we are going to do after dragging and releasing the plate
                if (Input.touches.Length < 1 && !Input.GetMouseButton(0))
                {
                    //we no longer have control over the product
                    isReadyToServe = false;

                    //if we are giving the order to a customer (plate is close enough)
                    GameObject[] availableCustomers = GameObject.FindGameObjectsWithTag("customer");

                    //if there is no customer in shop, take the plate back.
                    if (availableCustomers.Length < 1)
                    {
                        //take the plate back to it's initial position
                        ResetPosition();
                        yield break;
                    }

                    bool delivered = false;
                    GameObject theCustomer = null;
                    for (int cnt = 0; cnt < availableCustomers.Length; cnt++)
                    {
                        if (availableCustomers[cnt].GetComponent<CustomerController>().isCloseEnoughToDelivery)
                        {
                            //we know that just 1 customer is always nearest to the delivery. so "theCustomer" is unique.
                            theCustomer = availableCustomers[cnt];
                            delivered = true;
                        }
                    }

                    //if customer got the delivery
                    if (delivered)
                    {
                        //deliver the order
                        List<int> deliveredProduct = new List<int>();

                        //contents of the delivery which customer got from us.
                        deliveredProduct = deliveryQueueItemsContent;

                        //debug delivery
                        for (int i = 0; i < deliveryQueueItemsContent.Count; i++)
                        {
                            print("Delivery Items ID " + i.ToString() + " = " + deliveryQueueItemsContent[i]);
                        }

                        //let the customers know what he got.
                        theCustomer.GetComponent<CustomerController>().ReceiveOrder(deliveredProduct);

                        //reset main queue
                        deliveryQueueItems = 0;
                        deliveryQueueIsFull = false;
                        deliveryQueueItemsContent.Clear();

                        //destroy the contents of the serving plate.
                        GameObject[] DeliveryQueueItems = GameObject.FindGameObjectsWithTag("deliveryQueueItem");
                        foreach (GameObject item in DeliveryQueueItems)
                        {
                            if (item.transform.parent.gameObject == gameObject)
                                Destroy(item);
                        }

                        if (MainGameController.isTutorial)
                        {
                            //#6 - Dragging plate to customer
                            if (TutorialManager.currentTutorialStep == 6 && this.gameObject.name == "serverPlate-1")
                            {
                                //create the next customer that wants the coffee
                                TutorialManager.instance.DisableHelperElements();
                                StartCoroutine(CreateSecondCustomer());
                            }
                        }

                        //take the plate back to it's initial position
                        ResetPosition();
                    }
                    else
                    {
                        ResetPosition();
                    }
                }

                yield return 0;
            }
        }


        public IEnumerator CreateSecondCustomer()
        {
            print("<b>Tutorial Part #1 is done</b>");
            yield return new WaitForSeconds(5.0f);
            MainGameController.isTutorialCustomerPresentInScene = false;    //here we tell MGC that first tutorial customer is served and we can create the second customer
        }


        public IEnumerator DisplayTutorialFinishPanel()
        {
            print("<b>Tutorial is done</b>");
            yield return new WaitForSeconds(3.0f);
            TutorialManager.tutorialIsFinished = true;
        }


        /// <summary>
        /// Move the plate to it's initial position.
        /// we also check if user wants to trash his delivery, before any other process.
        /// this way we can be sure that nothing will interfere with deleting the delivery. (prevents many bugs)
        /// </summary>
        void ResetPosition()
        {
            //just incase user wants to move this to trashbin, check it here first
            if (trashbin.GetComponent<TrashBinController>().isCloseEnoughToTrashbin)
            {
                //empty plate contents
                SfxPlayer.instance.PlaySfx(1);

                MainGameController.totalMoneyMade -= MainGameController.globalTrashLoss;
                GameObject money3d = Instantiate(money3dText, trashbin.transform.position + new Vector3(-1.5f, 0, -0.8f), Quaternion.Euler(0, 0, 0)) as GameObject;
                money3d.GetComponent<TextMeshController>().myText = "- $" + MainGameController.globalTrashLoss.ToString();

                //Stats
                MainGameController.totalPenalties++;
                print("totalPenalties: " + MainGameController.totalPenalties);

                deliveryQueueItems = 0;
                deliveryQueueIsFull = false;
                deliveryQueueItemsContent.Clear();
                GameObject[] DeliveryQueueItems = GameObject.FindGameObjectsWithTag("deliveryQueueItem");
                foreach (GameObject item in DeliveryQueueItems)
                    if (item.transform.parent.gameObject == gameObject)
                        Destroy(item);
            }

            //take the plate back to it's initial position
            print("Back to where we belong");
            transform.position = initialPosition;
            r.material.color = new Color(r.material.color.r, r.material.color.g, r.material.color.b, 1);
            canDeliverOrder = false;
            StartCoroutine(reactivate());
        }


        /// <summary>
        /// Make the deliveryPlate draggable again.
        /// </summary>
        /// <returns></returns>
        IEnumerator reactivate()
        {
            yield return new WaitForSeconds(0.25f);
            canDeliverOrder = true;
        }

        public void ResetState()
        {
            //print("Plate status resetted!");
            r.material.color = new Color(1, 1, 1, 1);
        }

    }
}