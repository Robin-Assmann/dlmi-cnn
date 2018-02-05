using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockValue : MonoBehaviour {

	public int value = 0;

	void Start () {

		SpriteRenderer rend = GetComponent<SpriteRenderer> ();

		if (value < 0)
			rend.color = new Color (0.8f, 0.0f, 0.0f);
		else
			rend.color = new Color (0.0f, 0.8f, 0.0f);


	}

}
