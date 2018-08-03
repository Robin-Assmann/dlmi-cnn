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

	[SerializeField]
	private GameObject prefab_Block, prefab_Triangle, obj_spawnPosition, text_Output, obj_spawnLeft, obj_spawnRight;
	public GameObject Player;

	public static Manager manager;

	public int edge =3;
	private int SpawnCount = 0;
	public int PlayerPosition;

	void Start(){

		manager = this;
		edge = (DLMI_Control.controller.data.Parameters.NumVerticalLanes - 1) / 2;
		Player = GameObject.FindGameObjectWithTag ("Player");
		PlayerPosition = DLMI_Control.controller.data.Parameters.StartLane;
	}

	//Funktion zum manuellen Bewegen des Spielers
	void Update(){

		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			MovePlayer (-1);
		}

		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			MovePlayer (1);
		}
	}

	//Zufälliges Spawnen von Blöcken
	private IEnumerator SpawnBlocksRandom(){
	
		int delay = DLMI_Control.controller.data.Parameters.NumHorizontalLanes-1;
		do {

			//Bewegung des Hintergrunds (Rauschen oder Ablenkung)
			if (DLMI_Control.controller.data.Parameters.BackgroundNoise == 2) {
				MoveBackground (true);
			}
			if (DLMI_Control.controller.data.Parameters.Distraction == 2) {
				MoveBackground (false);
			}

			for (int i = DLMI_Control.controller.forms.Count - 1; i >= 0; i--) {
				DLMI_Control.controller.forms [i].GetComponent<MoveObject> ().MakeStep ();
				yield return new WaitForSeconds (0.001f);
			}

			//Zufällige Auswahl der Formart
			if (UnityEngine.Random.value > DLMI_Control.controller.data.Parameters.SpawnRatio) {
				Spawn (DLMI_Control.controller.pre_Negative, -5);
			} else {
				Spawn (DLMI_Control.controller.pre_Positive, 5);
			}

			yield return new WaitForSeconds (0.01f);

			//Save Picture und Bewegung des Spielers
			if (delay == 0) {
				DLMI_Control.controller.Decider (DLMI_Control.controller.GetPictures (), PlayerPosition);
				yield return new WaitUntil (() => DLMI_Control.controller.pictureTaken);
				DLMI_Control.controller.pictureTaken = false;

				yield return new WaitUntil (() => DLMI_Control.controller.playerMoved);
				DLMI_Control.controller.playerMoved = false;
			} else {
				delay--;
			}
		} while (!DLMI_Control.controller.allDone);
	}

	//Instanzieren und Positionieren von zufälligen gespawnten Blöcken
	void Spawn(GameObject pre_Block, int value){
	
		//Instanz
		GameObject block = Instantiate (pre_Block) as GameObject;
		block.transform.SetParent (obj_spawnPosition.transform);

		//Positionierung
		int x = 0;
		if (DLMI_Control.controller.data.Parameters.Spawn == 0) {
			x = UnityEngine.Random.Range (0, DLMI_Control.controller.data.Parameters.NumVerticalLanes);
		} else {
			x = DLMI_Control.controller.SpawnOrder [SpawnCount];
			SpawnCount++;
			if (SpawnCount == DLMI_Control.controller.SpawnOrder.Count)
				SpawnCount = 0;
		}
		float s = (0.5f * DLMI_Control.controller.xOff + (DLMI_Control.controller.xOff + DLMI_Control.controller.xSpace) * x) / 10;
		block.transform.localPosition = new Vector2 (-5+s, 5 - DLMI_Control.controller.yOff/20);

		//Setzen der Variablen und Hinzufügen des neuen Blocks zur Liste
		block.GetComponent<BlockValue> ().value = value;
		block.GetComponent<MoveObject> ().position = x;
		float y = DLMI_Control.controller.yOff;
		if (DLMI_Control.controller.yOff > DLMI_Control.controller.xOff)
			y = DLMI_Control.controller.xOff;
		float o = DLMI_Control.controller.data.Parameters.FormSize/100;
		block.GetComponent<Transform>().localScale = new Vector3 (y / 10 *o, y / 10 *o, y / 10 *o);
		DLMI_Control.controller.forms.Add (block);
	}

	//Funktion zum Bewegn des Hintergrunds (zB bei bewegtem Verrauschen)
	public void MoveBackground(bool x){
		if (x) {
			DLMI_Control.controller.points.GetComponent<Transform> ().position = new Vector3 (UnityEngine.Random.Range (-5, 5), UnityEngine.Random.Range (-5, 5), 0);
		} else {
			DLMI_Control.controller.distract.GetComponent<Transform> ().position = new Vector3 (UnityEngine.Random.Range (-5, 5), UnityEngine.Random.Range (-5, 5), 0);
		}
	}

	//Funktion zum Bewegen des Spielers wird im systematischen Ansatz genutzt
	public void MovePlayer(int i){
		PlayerPosition += i;
		if (PlayerPosition > DLMI_Control.controller.data.Parameters.NumVerticalLanes-1) {
			PlayerPosition = DLMI_Control.controller.data.Parameters.NumVerticalLanes-1;
			DLMI_Control.controller.playerMoved = true;
			return;
		} else {
			if (PlayerPosition < 0) {
				PlayerPosition = 0;
				DLMI_Control.controller.playerMoved = true;
				return;
			}
		}
		Player.transform.position = new Vector3 (Player.transform.position.x + ((DLMI_Control.controller.xOff+DLMI_Control.controller.xSpace)*i)/10, Player.transform.position.y, 0.0f);
	
		DLMI_Control.controller.playerMoved = true;
	}

	//Starten des Spawns
	public void StartSpawning(){
		if(!DLMI_Control.controller.data.Parameters.SpawnSystematic)
			StartCoroutine (SpawnBlocksRandom ());
		else
			StartCoroutine (SpawnBlocksSystematic ());

	}

	//Funktion zum systematischen Spawnen von Blöcken
	IEnumerator SpawnBlocksSystematic(){
		int pic = 0;
		int maxpic = (int)(Mathf.Pow((DLMI_Control.controller.data.Parameters.NumVerticalLanes*2), DLMI_Control.controller.data.Parameters.NumHorizontalLanes))*3;
		int[] zero = new int[DLMI_Control.controller.data.Parameters.NumHorizontalLanes];
		for (int i = 0; i < zero.Length; i++) {
			zero[i] = 0;
		}

		//Instanziieren, Positionierung und Bestimmung der Art der Blöcke
		for (int i = DLMI_Control.controller.data.Parameters.NumHorizontalLanes-1; i >=0; i--) {

			GameObject block = Instantiate (DLMI_Control.controller.pre_Positive) as GameObject;
			block.transform.SetParent (obj_spawnPosition.transform);
			float s = (0.5f * DLMI_Control.controller.yOff + (DLMI_Control.controller.yOff + DLMI_Control.controller.ySpace) * i) / 10;
			block.transform.localPosition = new Vector2 ( -5 + DLMI_Control.controller.xOff/20, 5 - s);
			block.GetComponent<BlockValue> ().value = 5;
			block.GetComponent<MoveObject> ().position = 0;

			float y = DLMI_Control.controller.yOff;
			if (DLMI_Control.controller.yOff > DLMI_Control.controller.xOff)
				y = DLMI_Control.controller.xOff;
			float o = DLMI_Control.controller.data.Parameters.FormSize/100;
			block.GetComponent<Transform>().localScale = new Vector3 (y / 10 *o, y / 10 *o, y / 10 *o);
			DLMI_Control.controller.forms.Add (block);
		}


		//Routine der systematischen Bilderstellung
		string startingString = "";
		for (int i = 0; i < zero.Length; i++) {
			startingString = startingString + zero [i];
		}
		bool ignorefirst = true;
		do {
			zero = DLMI_Control.controller.NextStep (zero);
			StartCoroutine (DLMI_Control.controller.Coloring ());
			yield return new WaitUntil (() => DLMI_Control.controller.doneColoring);
			DLMI_Control.controller.doneColoring = false;
			string s ="";
			for (int i = 0; i < zero.Length; i++) {
				s = s + zero [i];
			}
			if(s.Equals(startingString) &&!ignorefirst){
				break;}
			else
				ignorefirst= false;
		} while (true);

		DLMI_Control.controller.SetMaximum ();
	}
}

