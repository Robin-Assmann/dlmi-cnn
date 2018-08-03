using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageControl : MonoBehaviour {

	void Awake () {

		//3 zufällige Werte für Rot/Grün/Blau Anteile
		float r = Random.value;
		float g = Random.value;
		float b = Random.value;

		//Erstellen einer neuen Farbe
		Color newColor = new Color (r, g, b, 1.0f);

		//Block bekommt die neue Farbe als Hintergrundfarbe zugewiesen
		GetComponent<Image> ().color = newColor;
	}

	void Update () {

		//Position des Blocks wird pro Frame im y-Wert verschoben
		transform.position += new Vector3 (0.0f, -100.0f * Time.deltaTime, 0.0f);
	}
}
