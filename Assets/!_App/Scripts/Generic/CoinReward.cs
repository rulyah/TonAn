using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CookingStar
{
    public class CoinReward : MonoBehaviour
    {
        public Transform[] coins;
        private Transform targetPoint;
        private Animator anim;

        private void Awake()
        {
            targetPoint = GameObject.FindGameObjectWithTag("DummyCoinCollectionTarget").GetComponent<Transform>();
            anim = GetComponent<Animator>();
        }

        void Start()
        {
            StartCoroutine(MoveCoins());
        }

        public IEnumerator MoveCoins()
        {
            yield return new WaitForSeconds(1f);
            anim.enabled = false;

            foreach (Transform c in coins)
            {
                yield return new WaitForSeconds(0.2f);
                StartCoroutine(MoveCoinCo(c));
            }

            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }

        public IEnumerator MoveCoinCo(Transform _c)
        {
            Vector3 startPos = _c.position;
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 2.8f;
                _c.position = new Vector3(
                    Mathf.SmoothStep(startPos.x, targetPoint.position.x, t),
                    Mathf.SmoothStep(startPos.y, targetPoint.position.y, t),
                    startPos.z);
                yield return 0;
            }

            if (t >= 1)
            {
                SfxPlayer.instance.PlaySfx(3);
            }
        }
    }
}