using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using __App.Scripts.Generic;
using Unity.Collections;

namespace CookingStar
{
    public class ProductMover : MonoBehaviour
    {
        /// <summary>
        /// This class manages user inputs (drags and touches) on ingredients panel,
        /// and update main queue array accordingly.
        /// </summary>

        public Renderer shape;

        //Internal variables - Not editable
        internal int factoryID;               //actual ingredient ID to be served to customer
        internal bool needsProcess;           //does this ingredient should be processed before delivering to customer?

        ///only required for ingredients that needs process!
        /// ************************************************
        private bool processFlag;           //will be set to true the first time this ingredient enters a processor
        private bool isProcessed;           //process has been finished
        private bool isOverburned;          //burger stayed for too long on the grill
        private bool isProcessing;          //process is being done
        internal string processorTag;         //tag of the processor game object
        public Material[] beforeAfterMat;   //index[0] = raw    
                                            //index[1] = processed
                                            //index[2] = overburned

        //Private flags.
        internal GameObject target;                 //target object for this ingredient (deliveryPlate, a processor machine, etc...)
        private bool canGetDragged;               //are we allowed to drag this plate to customer?
        private GameObject[] serverPlates;        //server plate game objects (there can be more than 1 serverplate)
        private float[] distanceToPlates;         //distance to all available serverPlates
        private GameObject grill;                 //grill machine to process burgers
        private GameObject trashBin;              //reference to trashBin object
        private bool isFinished;                  //are we done with positioning and processing the ingredient on the plate?
        private float minDeliveryDistance = 1.1f; //Minimum distance to deliveryPlate required to land this ingredint on plate.
        private Vector3 tmpPos;                   //temp variable for storing player input position on screen
        private RaycastHit hitInfo;
        private Ray ray;
        public GameObject money3dText;            //3d text mesh
        private float ingredientOverlayOffset = 0.18f;      //Y direction
        private float offsetY = 0.55f;
        private float scaleUpRatio = 1.5f;
        private Vector3 initialScale;


        void Start()
        {
            canGetDragged = true;
            isFinished = false;         //!Important : we use this flag to prevent ingredients to be draggable after placed on the plate.
            isProcessed = false;
            isProcessing = false;
            isOverburned = false;
            processFlag = false;

            //find possible targets
            serverPlates = GameObject.FindGameObjectsWithTag("serverPlate");
            grill = GameObject.FindGameObjectWithTag("grill");
            trashBin = GameObject.FindGameObjectWithTag("trashbin");

            distanceToPlates = new float[serverPlates.Length];

            if (needsProcess)
                target = grill;
            else
                target = serverPlates[0];   //Temporary - we need to edit this in update loop to set correct serverplate for each ingredient!

            initialScale = transform.localScale;
        }


        /// <summary>
        /// Takes an array and return the lowest number in it, 
        /// with the optional index of this lowest number (position of it in the array)
        /// </summary>
        /// <param name="_array"></param>
        /// <returns></returns>
        Vector2 FindMinInArray(float[] _array)
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
            if (needsProcess)
                target = grill;
            else if (!needsProcess && isOverburned)
            {
                target = trashBin;
            }
            else
            {
                for (int i = 0; i < serverPlates.Length; i++)
                {
                    distanceToPlates[i] = Vector3.Distance(serverPlates[i].transform.position, gameObject.transform.position);
                    //find the correct (nearest plate) target
                    target = serverPlates[(int)FindMinInArray(distanceToPlates).y];

                    //debug
                    //print ("Distance to serverPlate-" + i.ToString() + " : " + distanceToPlates[i]);
                    //print ("Target is: " + target.name);
                }
            }

            //if dragged
            if (Input.GetMouseButton(0) && canGetDragged)
            {
                FollowInputPosition();
            }

            //if released and doesn't need process
            if ((!Input.GetMouseButton(0) && Input.touches.Length < 1) && !isFinished && !needsProcess)
            {
                transform.localScale = initialScale;

                canGetDragged = false;
                CheckCorrectPlacement();
            }

            //if released and needs process
            if ((!Input.GetMouseButton(0) && Input.touches.Length < 1) && !isFinished && needsProcess)
            {
                transform.localScale = initialScale;

                canGetDragged = false;
                CheckCorrectPlacementOnProcessor();
            }

            //if needs process and process is finished successfully
            if (isProcessed && !isOverburned && !target.GetComponent<PlateController>().deliveryQueueIsFull && !IngredientsController.itemIsInHand)
            {
                ManageSecondDrag();
            }

            //if needs process and process took too long and burger is overburned and must be discarded
            if (/*needsProcess &&*/ isProcessed && isOverburned && /*!target.GetComponent<PlateController>().deliveryQueueIsFull &&*/ !IngredientsController.itemIsInHand)
            {
                ManageDiscard();
            }

            //you can make some special effects like particle, smoke or other visuals when your ingredient in being processed
            if (isProcessing)
            {
                //Special FX here! - make sure to stop, disable ro destroy your FX object after processing is finished.

            }

            //Optional - change target's color when this ingredient is near enough
            if (!processFlag || (isProcessed && target.tag == "serverPlate"))
                ChangeTargetsColor(target);

        }


        /// <summary>
        /// Let the player move the processed ingredient
        /// </summary>
        void ManageDiscard()
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
                if (objectHit.tag == "overburnedIngredient" && objectHit.name == gameObject.name)
                {
                    IngredientsController.itemIsInHand = true;  //we have an ingredient in hand. no room for other ingredients!
                    target = trashBin;                          //we can just deliver this ingredient to trashbin.
                    StartCoroutine(DiscardIngredient());
                }
            }
        }


        /// <summary>
        /// Let the player move the processed ingredient to the delivery plate
        /// </summary>
        void ManageSecondDrag()
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
                if (objectHit.tag == "ingredient" && objectHit.name == gameObject.name)
                {
                    IngredientsController.itemIsInHand = true;      //we have an ingredient in hand. no room for other ingredients!                                                            
                    StartCoroutine(FollowInputTimeIndependent());   //destination for this processed ingredient
                }
            }
        }


        /// <summary>
        /// Change plate's color when dragged ingredients are near enough
        /// </summary>
        private float myDistance;
        void ChangeTargetsColor(GameObject _target)
        {
            if (!IngredientsController.itemIsInHand)    //if nothing is being dragged
                return;
            else if (_target.tag == "serverPlate" && _target.GetComponent<PlateController>().deliveryQueueIsFull)
                return;
            else
            {
                myDistance = Vector3.Distance(_target.transform.position, gameObject.transform.position);
                if (myDistance < minDeliveryDistance)
                {
                    //change target's color to let the player know this is the correct place to release the items.
                    _target.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f);
                }
                else
                {
                    //change target's color back to normal
                    _target.GetComponent<Renderer>().material.color = new Color(1, 1, 1);
                }
            }
        }


        /// <summary>
        /// Check if the ingredients are dragged into the deliveryPlate. Otherwise delete them.
        /// </summary>
        void CheckCorrectPlacementOnProcessor()
        {
            //if there is already an item on the processor, destroy this new ingredient
            if (!target.GetComponent<GrillController>().isEmpty)
            {
                Destroy(this.gameObject);
                target.GetComponent<Renderer>().material.color = new Color(1, 1, 1);
                return;
            }

            //if this ingredient is close enough to it's processor machine, leave it there. otherwise drop and delete it.
            float distanceToProcessor = Vector3.Distance(target.transform.position, gameObject.transform.position);
            print("distanceToProcessor: " + distanceToProcessor);

            if (distanceToProcessor < minDeliveryDistance)
            {
                //close enough to land on processor
                transform.parent = target.transform;
                transform.localPosition = new Vector3(0.33f, 0.14f, 5f);

                //change deliveryPlate's color back to normal
                target.GetComponent<Renderer>().material.color = new Color(1, 1, 1);

                //start processing the raw ingredient
                StartCoroutine(ProcessRawIngredient());

                //Tutorial system
                if (MainGameController.isTutorial)
                {
                    //#1 - Dragging burger to grill
                    if (TutorialManager.currentTutorialStep == 1 && factoryID == 2)
                    {
                        TutorialManager.instance.DisableHelperElements();
                        TutorialManager.instance.UpdateTutorialStep(2, 4f);
                    }
                }
            }
            else
            {
                Destroy(gameObject);
            }

            //Not draggable anymore.
            isFinished = true;
        }


        /// <summary>
        /// /Process raw ingredinet and transform it to a usable ingredient
        /// </summary>
        /// <returns></returns>
        IEnumerator ProcessRawIngredient()
        {
            processFlag = true; //should always remain true!
            isProcessing = true;
            isProcessed = false;
            isOverburned = false;

            float processTime = GrillController.grillTimer;
            float keepWarmTime = GrillController.grillKeepWarmTimer;

            GrillController gc = target.GetComponent<GrillController>();
            gc.isOn = true;
            gc.isEmpty = false;
            SfxPlayer.instance.PlaySfx(10);

            float t = 0.0f;
            while (t < 1)
            {
                t += Time.deltaTime * (1 / processTime);
                yield return 0;
            }

            if (t >= 1)
            {
                isProcessing = false;
                isProcessed = true;
                isOverburned = false;
                needsProcess = false;
                gc.isWarm = true;        //grill has entered the state to keep the burger warm.
                SfxPlayer.instance.PlaySfx(11);

                gameObject.tag = "ingredient";
                gameObject.name = gameObject.name.Substring(0, gameObject.name.Length - 4);
                //GetComponent<Renderer>().material = beforeAfterMat[1];
                shape.material = beforeAfterMat[1];

                //Now check if we pick the fried burger on time, otherwise this burger will get overburned and should be discarded.
                float v = 0.0f;
                while (v < 1)
                {
                    if (!IngredientsController.itemIsInHand)
                        v += Time.deltaTime * (1 / keepWarmTime);

                    //Do not overburn the burger in tutorial mode
                    if (MainGameController.isTutorial)
                    {
                        v = 0;
                    }

                    //print("Time to OverBurn: " + (1 - v));
                    yield return 0;
                }

                //This burger is overburned! so it should be discarded in trashbin.
                //Important: as we are checking this condition independently from main game cycle, we need to double check
                //if this ingredient has been moved to delivery plate or is still waiting on the grill
                //if it was not on the plate, then we are sure that it is burned. otherwise we do not continue the procedure.
                if (v >= 1 && gameObject.tag != "deliveryQueueItem" && !IngredientsController.itemIsInHand)
                {
                    isProcessing = false;
                    isProcessed = true;
                    isOverburned = true;

                    //GetComponent<Renderer>().material = beforeAfterMat[2];        //change the material to overburned burger
                    shape.material = beforeAfterMat[2];                             //change the material to overburned burger

                    GrillController ngc = grill.GetComponent<GrillController>();
                    SfxPlayer.instance.PlaySfx(12);
                    ngc.isEmpty = false;
                    ngc.isWarm = false;
                    ngc.isOn = true;
                    ngc.isOverburned = true;
                    gameObject.tag = "overburnedIngredient";
                    target = trashBin;
                }
            }
        }


        /// <summary>
        /// Check if the ingredients are dragged into the deliveryPlate. Otherwise delete them.
        /// </summary>
        void CheckCorrectPlacement()
        {
            PlateController pc = target.GetComponent<PlateController>(); //cache this component

            //if this ingredient is close enough to serving plate, we can add it to main queue. otherwise drop and delete it.
            float distanceToPlate = Vector3.Distance(target.transform.position, gameObject.transform.position);
            //print("distanceToPlate: " + distanceToPlate);

            if (distanceToPlate < minDeliveryDistance && !pc.deliveryQueueIsFull)
            {
                //close enough to land on plate
                transform.parent = target.transform;
                transform.position = new Vector3(
                    target.transform.position.x,
                    target.transform.position.y + (ingredientOverlayOffset * pc.deliveryQueueItems),
                    target.transform.position.z - (0.2f * pc.deliveryQueueItems + 0.1f));

                //X pos adjustment
                transform.localPosition = new Vector3(transform.localPosition.x + pc.itemPositionAdjustment, transform.localPosition.y, transform.localPosition.z);
                pc.deliveryQueueItems++;
                pc.deliveryQueueItemsContent.Add(factoryID);

                //change deliveryPlate's color back to normal
                target.GetComponent<Renderer>().material.color = new Color(1, 1, 1);

                //Tutorial system
                if (MainGameController.isTutorial)
                {
                    //#0 - Dragging bread to plate
                    if (TutorialManager.currentTutorialStep == 0 && factoryID == 1)
                    {
                        TutorialManager.instance.DisableHelperElements();
                        TutorialManager.instance.UpdateTutorialStep(1);
                    }

                    //#3 - Dragging cheeze to plate
                    if (TutorialManager.currentTutorialStep == 3 && factoryID == 5)
                    {
                        TutorialManager.instance.DisableHelperElements();
                        TutorialManager.instance.UpdateTutorialStep(4);
                    }

                    //#4 - Dragging sliced tomato to plate
                    if (TutorialManager.currentTutorialStep == 4 && factoryID == 4)
                    {
                        TutorialManager.instance.DisableHelperElements();
                        TutorialManager.instance.UpdateTutorialStep(5);
                    }

                    //#5 - Dragging top bread to plate
                    if (TutorialManager.currentTutorialStep == 5 && factoryID == 10)
                    {
                        TutorialManager.instance.DisableHelperElements();
                        TutorialManager.instance.UpdateTutorialStep(6);
                    }
                }

                //we no longer need this ingredient's script (ProductMover class)
                GetComponent<ProductMover>().enabled = false;
            }
            else
            {
                Destroy(gameObject);
            }

            //Not draggable anymore.
            isFinished = true;

            //Make sure all serving plates are back to normnal state
            ResetServingPlatesState();
        }


        public void ResetServingPlatesState()
        {
            GameObject[] servingPlates = GameObject.FindGameObjectsWithTag("serverPlate");
            foreach (GameObject sp in serverPlates)
            {
                sp.GetComponent<PlateController>().ResetState();
            }
        }


        /// <summary>
        /// Follow players mouse or finger position on screen.
        /// </summary>
        private Vector3 _Pos;
        void FollowInputPosition()
        {
            _Pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Custom offset. these objects should be in front of every other GUI instances.
            _Pos = new Vector3(_Pos.x, _Pos.y, -0.5f);
            //follow player's finger
            transform.position = _Pos + new Vector3(0, 0 + offsetY, 0);
            transform.localScale = initialScale * scaleUpRatio;
        }


        /// <summary>
        /// Follow players mouse or finger position on screen.
        /// This is an IEnumerator and run independent of game main cycle
        /// </summary>
        /// <returns></returns>
        IEnumerator FollowInputTimeIndependent()
        {
            while (IngredientsController.itemIsInHand || target.tag == "serverPlate")
            {
                tmpPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                tmpPos = new Vector3(tmpPos.x, tmpPos.y, -0.5f);
                transform.position = tmpPos + new Vector3(0, 0 + offsetY, 0);

                //if user release the input, check if we delivered the processed ingredient to the plate or we just release it nowhere!
                if (Input.touches.Length < 1 && !Input.GetMouseButton(0))
                {
                    //if we delivered it to the plate
                    if (Vector3.Distance(target.transform.position, gameObject.transform.position) <= minDeliveryDistance)
                    {
                        print("Landed on Plate in: " + Time.time);
                        GrillController gc = grill.GetComponent<GrillController>();
                        gc.isEmpty = true;
                        gc.isWarm = false;
                        gc.isOn = false;
                        gameObject.tag = "deliveryQueueItem";
                        gameObject.GetComponent<MeshCollider>().enabled = false;

                        transform.position = new Vector3(
                            target.transform.position.x,
                            target.transform.position.y + (ingredientOverlayOffset * target.GetComponent<PlateController>().deliveryQueueItems),
                            target.transform.position.z - (0.2f * target.GetComponent<PlateController>().deliveryQueueItems + 0.1f));

                        transform.parent = target.transform;

                        //X pos adjustment
                        transform.localPosition = new Vector3(transform.localPosition.x + target.GetComponent<PlateController>().itemPositionAdjustment, transform.localPosition.y, transform.localPosition.z);

                        //Tutorial system
                        if (MainGameController.isTutorial)
                        {
                            //#2 - Dragging ready burger to plate
                            if (TutorialManager.currentTutorialStep == 2 && factoryID == 2)
                            {
                                TutorialManager.instance.DisableHelperElements();
                                TutorialManager.instance.UpdateTutorialStep(3);
                            }
                        }

                        target.GetComponent<PlateController>().deliveryQueueItems++;
                        target.GetComponent<PlateController>().deliveryQueueItemsContent.Add(factoryID);
                        //change deliveryPlate's color back to normal
                        target.GetComponent<Renderer>().material.color = new Color(1, 1, 1);
                        //we no longer need this ingredient's script (ProductMover class)
                        GetComponent<ProductMover>().enabled = false;
                        yield break;
                    }
                    else
                    {
                        //if we released it nowhere
                        target = grill;
                        transform.parent = target.transform;
                        transform.position = new Vector3(target.transform.position.x, target.transform.position.y + 0.75f, target.transform.position.z - 0.1f);

                        //X pos adjustment
                        transform.localPosition = new Vector3(0.33f, 0.14f, 5f);
                    }
                }

                yield return 0;
            }
        }


        /// <summary>
        /// Follow players mouse or finger position on screen.
        /// This is an IEnumerator and run independent of game main cycle
        /// </summary>
        /// <returns></returns>
        IEnumerator DiscardIngredient()
        {
            while (IngredientsController.itemIsInHand || target == trashBin)
            {
                print("<b>target: " + target + "</b>");
                TrashBinController tbc = trashBin.GetComponent<TrashBinController>();

                tmpPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                tmpPos = new Vector3(tmpPos.x, tmpPos.y, -0.5f);
                transform.position = tmpPos + new Vector3(0, 0 + offsetY, 0);

                //update trashbin door state
                float tmpDistanceToTrashbin = Vector3.Distance(target.transform.position, gameObject.transform.position);
                print("tmpDistanceToTrashbin: " + tmpDistanceToTrashbin);
                if (tmpDistanceToTrashbin <= minDeliveryDistance)
                    tbc.UpdateDoorState(1);
                else
                    tbc.UpdateDoorState(0);

                //if user release the input, check if we delivered the processed ingredient to the trashbin or we just release it nowhere!
                if (Input.touches.Length < 1 && !Input.GetMouseButton(0))
                {
                    //if we delivered it to the trashbin
                    if (tmpDistanceToTrashbin <= minDeliveryDistance)
                    {
                        SfxPlayer.instance.PlaySfx(14);

                        //Trash loss
                        MainGameController.totalMoneyMade -= MainGameController.globalTrashLoss;
                        GameObject money3d = Instantiate(money3dText,
                                                            trashBin.transform.position + new Vector3(-1.5f, 0, -0.8f),
                                                            Quaternion.Euler(0, 0, 0)) as GameObject;
                        money3d.GetComponent<TextMeshController>().myText = "- $" + MainGameController.globalTrashLoss.ToString();

                        //Stats
                        MainGameController.totalPenalties++;
                        print("totalPenalties: " + MainGameController.totalPenalties);
                        GrillController gc = grill.GetComponent<GrillController>();
                        gc.isEmpty = true;
                        gc.isWarm = false;
                        gc.isOn = false;
                        gc.isOverburned = false;
                        Destroy(gameObject);
                        yield break;
                    }
                    else
                    {
                        //if we released it nowhere
                        target = grill;
                        transform.parent = target.transform;
                        transform.localPosition = new Vector3(0.33f, 0.14f, 5f);
                        //Target should be trashbin again once the burned burger is back at the grill
                        target = trashBin;
                        yield break;
                    }
                }

                yield return 0;
            }
        }

    }
}