using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

public class Manager: MonoBehaviour{

	//prefab_Block = vorgefertigte Prefab Objekt eines Blockes
	//obj_spawnPosition = Position in der Blöcke erstellt werden
	//text_Output = Textfeld auf dem Python Text angezeigt wird
	[SerializeField]
	private GameObject prefab_Block, prefab_Triangle, obj_spawnPosition, text_Output, obj_spawnLeft, obj_spawnRight;
	public GameObject Player;

	public static Manager manager;

	public int edge =3;

	void Start(){

		manager = this;
		//Startet nebenläufige Routine, welche Blöcke erstellt


		edge = (DLMI_Control.controller.lane_count - 1) / 2;
		StartCoroutine (SpawnBlocks ());
		Player = GameObject.FindGameObjectWithTag ("Player");

	}

	void Update(){

		if (Input.GetKeyDown (KeyCode.LeftArrow)) {

			MovePlayer (-1);
		}

		if (Input.GetKeyDown (KeyCode.RightArrow)) {

			MovePlayer (1);
		}


	}

	private IEnumerator SpawnBlocks(){
	
		//WaitforSeconds wird einmalig initialisiert um Müll zu verhindern
		WaitForSeconds waiting = new WaitForSeconds (DLMI_Control.controller.wait_time);

		while (true) {
			if (UnityEngine.Random.value > 0.3f) {
				//Erstellen eines neuen GameObjects anhand dem Block Prefab
				GameObject block = Instantiate (prefab_Triangle) as GameObject;
				//Neue Block wird dem Spawnpunkt als Kind zugewiesen
				block.transform.SetParent (obj_spawnPosition.transform);
				//Position des Blockes wird auf die y Höhe des Punktes gesetzt/ x-Wert wird zufällig gewählt
				block.transform.localPosition = new Vector2 (UnityEngine.Random.Range (-edge, edge+1), 0.0f);
				block.GetComponent<BlockValue> ().value = -5;
			} else {
				//Erstellen eines neuen GameObjects anhand dem Block Prefab
				GameObject block = Instantiate (prefab_Block) as GameObject;
				//Neue Block wird dem Spawnpunkt als Kind zugewiesen
				block.transform.SetParent (obj_spawnPosition.transform);
				//Position des Blockes wird auf die y Höhe des Punktes gesetzt/ x-Wert wird zufällig gewählt
				block.transform.localPosition = new Vector2 (UnityEngine.Random.Range (-edge, edge+1), 0.0f);

				block.GetComponent<BlockValue> ().value = 5;
				//Verzögerung von 3 Sekunden
			}
			yield return waiting;
		}
	}

	public void MovePlayer(int i){
			
		int pos =(int)(Player.transform.position.x + i);
		if (pos > edge)
			pos = edge;
		if (pos < -edge)
			pos = -edge;
		Player.transform.position = new Vector3 (pos,Player.transform.position.y,0.0f);
	}
}

