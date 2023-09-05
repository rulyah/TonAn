using UnityEngine;
using System.Collections;

namespace CookingStar
{
    public class TrashBinController : MonoBehaviour
    {
        /// <summary>
        /// This class handles everything related to TrashBin.
        /// </summary>

        internal bool canDelete = true;
        private GameObject[] deliveryPlates;        //all available deliveryPlates inside the game
        private float[] distanceToPlates;           //distance to all available plates
        private GameObject target;
        public Texture2D[] state;                   //Textures for open/shut states
        internal bool isCloseEnoughToTrashbin;      //This is used to let other classes know that player is intended to send the item to trashbin.
        private Renderer r;


        void Awake()
        {
            target = null;
            r = GetComponent<Renderer>();
            deliveryPlates = GameObject.FindGameObjectsWithTag("serverPlate");
            distanceToPlates = new float[deliveryPlates.Length];
            isCloseEnoughToTrashbin = false;
            r.material.mainTexture = state[0];
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

            //return the Vector2(minimum population, index of this minVal in the argument Array)
            return (new Vector2(minval, lowestIndex));
        }


        void Update()
        {
            for (int i = 0; i < deliveryPlates.Length; i++)
            {
                distanceToPlates[i] = Vector3.Distance(deliveryPlates[i].transform.position, gameObject.transform.position);
                //find the correct (nearest blender) target
                target = deliveryPlates[(int)FindMinInArray(distanceToPlates).y];
            }

            //check if player wants to move the order to trash bin
            if (target.GetComponent<PlateController>().canDeliverOrder)
            {
                CheckDistanceToDelivery();
            }
        }


        /// <summary>
        /// If player is dragging the deliveryPlate, check if maybe he wants to trash it.
        /// we do this by calculation the distance of deliveryPlate and trashBin.
        /// </summary>
        private float myDistance;
        void CheckDistanceToDelivery()
        {
            myDistance = Vector3.Distance(transform.position, target.transform.position);
            if (myDistance < 1.0f)
            {
                isCloseEnoughToTrashbin = true;
                //change texture
                r.material.mainTexture = state[1];
            }
            else
            {
                isCloseEnoughToTrashbin = false;
                //change texture
                r.material.mainTexture = state[0];
            }
        }


        /// <summary>
        /// Allow other controllers to update the animation state of this trashbin object
        /// by controlling it's door state.
        /// </summary>
        public void UpdateDoorState(int _state)
        {
            if (_state == 1)
                r.material.mainTexture = state[1];
            else
                r.material.mainTexture = state[0];
        }


        /// <summary>
        /// Activate using trashbin again, after a few seconds.
        /// </summary>
        /// <returns></returns>
        IEnumerator Reactivate()
        {
            yield return new WaitForSeconds(0.25f);
            canDelete = true;
        }
    }
}