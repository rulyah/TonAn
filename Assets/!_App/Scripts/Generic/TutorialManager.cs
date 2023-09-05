using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CookingStar
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager instance;
        public static int currentTutorialStep;
        public static bool tutorialIsFinished;

        public GameObject tutorialPlane;
        public GameObject finger;
        public GameObject tutorialTextPanel;


        private void Awake()
        {
            instance = this;
            currentTutorialStep = 0;
            tutorialIsFinished = false;
            tutorialPlane.SetActive(true);
            tutorialPlane.GetComponent<Renderer>().enabled = false;
            finger.SetActive(false);
            tutorialTextPanel.SetActive(false);
        }


        void Start()
        {
            if (!MainGameController.isTutorial)
                this.gameObject.SetActive(false);
        }


        public void UpdateTutorialStep(int step, float delay = 1)
        {
            currentTutorialStep = step;
            print("currentTutorialStep: " + currentTutorialStep);

            if (step == 0)
            {
                StartCoroutine(RunTutorialStep_0(delay));
            }
            else if (step == 1)
            {
                StartCoroutine(RunTutorialStep_1(delay));
            }
            else if (step == 2)
            {
                StartCoroutine(RunTutorialStep_2(delay));
            }
            else if (step == 3)
            {
                StartCoroutine(RunTutorialStep_3(delay));
            }
            else if (step == 4)
            {
                StartCoroutine(RunTutorialStep_4(delay));
            }
            else if (step == 5)
            {
                StartCoroutine(RunTutorialStep_5(delay));
            }
            else if (step == 6)
            {
                StartCoroutine(RunTutorialStep_6(delay));
            }
            else if (step == 7)
            {
                StartCoroutine(RunTutorialStep_7(delay));
            }
            else if (step == 8)
            {
                StartCoroutine(RunTutorialStep_8(delay));
            }
            else if (step == 9)
            {
                StartCoroutine(RunTutorialStep_9(delay));
            }
            else if (step == 10)
            {
                StartCoroutine(RunTutorialStep_10(delay));
            }
            else if (step == 11)
            {
                StartCoroutine(RunTutorialStep_11(delay));
            }
        }

        public IEnumerator RunTutorialStep_0(float _d)
        {
            yield return new WaitForSeconds(_d);

            tutorialPlane.GetComponent<Renderer>().enabled = true;
            finger.SetActive(true);

            GameObject dest = GameObject.Find("Ingredient-01");
            SetPosition(dest);

            finger.GetComponent<Animator>().Play("Finger_Step-0");
        }

        public IEnumerator RunTutorialStep_1(float _d)
        {
            yield return new WaitForSeconds(_d);

            tutorialPlane.GetComponent<Renderer>().enabled = true;
            finger.SetActive(true);

            GameObject dest = GameObject.Find("Ingredient-02");
            SetPosition(dest);

            finger.GetComponent<Animator>().Play("Finger_Step-1");
        }

        public IEnumerator RunTutorialStep_2(float _d)
        {
            yield return new WaitForSeconds(_d);

            tutorialPlane.GetComponent<Renderer>().enabled = true;
            finger.SetActive(true);

            GameObject dest = GameObject.Find("Ingredient-Type-02");
            SetPosition(dest);

            finger.GetComponent<Animator>().Play("Finger_Step-2");
        }

        public IEnumerator RunTutorialStep_3(float _d)
        {
            yield return new WaitForSeconds(_d);

            tutorialPlane.GetComponent<Renderer>().enabled = true;
            finger.SetActive(true);

            GameObject dest = GameObject.Find("Ingredient-05");
            SetPosition(dest);

            finger.GetComponent<Animator>().Play("Finger_Step-3");
        }

        public IEnumerator RunTutorialStep_4(float _d)
        {
            yield return new WaitForSeconds(_d);

            tutorialPlane.GetComponent<Renderer>().enabled = true;
            finger.SetActive(true);

            GameObject dest = GameObject.Find("Ingredient-04");
            SetPosition(dest);

            finger.GetComponent<Animator>().Play("Finger_Step-4");
        }

        public IEnumerator RunTutorialStep_5(float _d)
        {
            yield return new WaitForSeconds(_d);

            tutorialPlane.GetComponent<Renderer>().enabled = true;
            finger.SetActive(true);

            GameObject dest = GameObject.Find("Ingredient-10");
            SetPosition(dest);

            finger.GetComponent<Animator>().Play("Finger_Step-5");
        }


        public IEnumerator RunTutorialStep_6(float _d)
        {
            yield return new WaitForSeconds(_d);

            tutorialPlane.GetComponent<Renderer>().enabled = true;
            finger.SetActive(true);

            GameObject dest = GameObject.Find("serverPlate-1");
            SetPosition(dest);

            finger.GetComponent<Animator>().Play("Finger_Step-6");
        }

        public IEnumerator RunTutorialStep_7(float _d)
        {
            yield return new WaitForSeconds(_d);

            tutorialPlane.GetComponent<Renderer>().enabled = true;
            finger.SetActive(true);

            GameObject dest = GameObject.Find("SideRequest-04");
            SetPosition(dest);

            finger.GetComponent<Animator>().Play("Finger_Step-7");
        }

        public IEnumerator RunTutorialStep_8(float _d)
        {
            yield return new WaitForSeconds(_d);

            tutorialPlane.GetComponent<Renderer>().enabled = true;
            finger.SetActive(true);

            GameObject dest = GameObject.Find("CoffeeMakerTutorialDummy");
            SetPosition(dest);

            finger.GetComponent<Animator>().Play("Finger_Step-8");
        }

        public IEnumerator RunTutorialStep_9(float _d)
        {
            yield return new WaitForSeconds(_d);
            tutorialTextPanel.SetActive(true);
        }

        public IEnumerator RunTutorialStep_10(float _d)
        {
            yield return new WaitForSeconds(_d);

            tutorialPlane.GetComponent<Renderer>().enabled = true;
            finger.SetActive(true);

            GameObject dest = GameObject.Find("CandyController");
            SetPosition(dest);

            finger.GetComponent<Animator>().Play("Finger_Step-10");
        }

        public IEnumerator RunTutorialStep_11(float _d)
        {
            yield return new WaitForSeconds(_d);

            tutorialPlane.GetComponent<Renderer>().enabled = true;
            finger.SetActive(true);

            GameObject dest = GameObject.Find("SideRequest-03");
            SetPosition(dest);

            finger.GetComponent<Animator>().Play("Finger_Step-11");
        }


        public void SetPosition(GameObject _dest)
        {
            tutorialPlane.transform.localPosition = new Vector3(
                _dest.transform.position.x,
                _dest.transform.position.y,
                tutorialPlane.transform.position.z);
        }


        public void DisableHelperElements(bool _tp = false, bool _finger = false)
        {
            tutorialPlane.GetComponent<Renderer>().enabled = _tp;
            finger.SetActive(_finger);
        }


        public void ClickOnTpNextButton()
        {
            TutorialManager.instance.tutorialTextPanel.SetActive(false);
            TutorialManager.instance.DisableHelperElements();
            TutorialManager.instance.UpdateTutorialStep(10);
        }


        public void SpawnThirdCustomer()
        {
            StartCoroutine(SpawnThirdCustomerCo());
        }
        public IEnumerator SpawnThirdCustomerCo()
        {
            yield return new WaitForSeconds(5.0f);
            MainGameController.isTutorialCustomerPresentInScene = false;    //here we tell MGC that second tutorial customer is served and we can create the third customer
        }


        public void FinishIt()
        {
            StartCoroutine(FinishItCo());
        }
        public IEnumerator FinishItCo()
        {
            yield return new WaitForSeconds(3.0f);
            TutorialManager.tutorialIsFinished = true;
        }

    }

}