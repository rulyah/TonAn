using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CookingStar
{
	public class CandyMover : MonoBehaviour
	{
		/// <summary>
		/// This class manages user inputs (drags and touches) on Candy/Salad items.
		/// </summary>

		private bool canBeDragged;

		void Awake()
		{
			canBeDragged = true;
		}

		void Update()
		{
			if (Input.GetMouseButton(0) && canBeDragged)
			{
				FollowInputPosition();
			}

			if (!Input.GetMouseButton(0) && Input.touches.Length < 1)
			{
				canBeDragged = false;
				CandyController.canDeliverCandy = false;
				CheckCorrectPlacement();
			}
		}

		void CheckCorrectPlacement()
		{
			bool delivered = false;

			//if we are giving this candy to a customer (candy is close enough)
			GameObject[] availableCustomers = GameObject.FindGameObjectsWithTag("customer");

			//if there is no customer in shop, delete the candy.
			if (availableCustomers.Length < 1)
			{
				Destroy(gameObject);
				return;
			}

			GameObject theCustomer = null;
			for (int cnt = 0; cnt < availableCustomers.Length; cnt++)
			{
				if (availableCustomers[cnt].GetComponent<CustomerController>().isCloseEnoughToCandy)
				{
					//we know that just 1 customer is always nearest to the candy. so "theCustomer" is unique.
					theCustomer = availableCustomers[cnt];
					delivered = true;
				}
			}

			//if customer got the candy..
			if (delivered)
			{
				//deliver the candy and let the customers know he got a candy.
				theCustomer.GetComponent<CustomerController>().ReceiveCandy();
				CandyController.availableCandy--;

				if (TutorialManager.currentTutorialStep == 10)
				{
					TutorialManager.instance.DisableHelperElements();
					TutorialManager.instance.UpdateTutorialStep(11);
				}
			}

			Destroy(gameObject);
		}

		/// <summary>
		/// Follow players mouse or finger position on screen.
		/// </summary>
		private Vector3 _Pos;
		public void FollowInputPosition()
		{
			_Pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			_Pos = new Vector3(_Pos.x, _Pos.y, -0.5f);
			transform.position = _Pos + new Vector3(0, 0, 0);
		}

	}
}