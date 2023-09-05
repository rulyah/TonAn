using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CookingStar
{
    public class FastDestroy : MonoBehaviour
    {
        public float delay = 0.5f;

        void Start()
        {
            Destroy(gameObject, delay);
        }
    }
}