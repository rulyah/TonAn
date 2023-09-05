using UnityEngine;
using System.Collections;

namespace CookingStar
{
	public class TextMeshController : MonoBehaviour
	{
		/// <summary>
		/// This simple class will manage and animate 3dtexts ($Money) which are used when a 
		/// customer is happy and is paying the price. 
		/// </summary>
		/// 
		internal Vector3 startingSize;
		public TextMesh tm;
		internal string myText;

		void Start()
		{
			//start at the default scale.
			startingSize = transform.localScale;
			StartCoroutine(ScaleUpCo());
		}

		IEnumerator ScaleUpCo()
		{
			tm.text = myText;
			while (transform.localScale.x < 1.5f)
			{
				transform.localScale = new Vector3(transform.localScale.x + 0.045f, transform.localScale.y + 0.045f, transform.localScale.z);
				transform.position = new Vector3(transform.position.x, transform.position.y + 0.025f, transform.position.z);
				yield return 0;
			}

			float t = 2;
			while (t > 0)
			{
				t -= Time.deltaTime * 2f;
				transform.position = new Vector3(transform.position.x, transform.position.y + 0.005f, transform.position.z);
				if (t <= 0)
					Destroy(gameObject);
				yield return 0;
			}
		}
	}
}