using UnityEngine;
using System.Collections;

namespace CookingStar
{
	public class TextureScroller : MonoBehaviour
	{
		private float offset;
		private float damper = 5.0f;

		void LateUpdate()
		{
			offset += damper * Time.deltaTime;
			GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(0, offset));
		}
	}
}