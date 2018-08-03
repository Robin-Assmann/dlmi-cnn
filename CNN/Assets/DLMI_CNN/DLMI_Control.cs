using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using System.Net;
using System.Net.Sockets;


public class DLMI_Control : MonoBehaviour {

	//RenderBild der zweiten Kamera, welches den Spiel-Bildschirm in kleinerer Auflösung aufnimmt
	[SerializeField]
	private RenderTexture display;

	//Standardwerte, irrelevant bei systematischem Ansatz
	private int train_count = 200;
	private int test_count = 100;
	public float speed = -2.0f;
	public float wait_time = 0.3f; 
	public int lane_count = 7;
	public int height = 64;
	public int width = 64;

	//Pfad zum Python Interpreter und Bilder-Ordner
	private string pythonPath;
	//ShellProzess für das Pythonskript
	public Process p;
	//integer für die Bennenung der erstellten Bilder
	private int standCount, rightCount, leftCount;
	//boolean für wenn ein Bild abgespeichert wurde
	public bool picture_Taken = false;

	[SerializeField]
	//sind bereits Testdaten vorhanden?
	private bool test_done = false;
	[SerializeField]
	//sind bereits Trainingsdaten vorhanden?
	private bool train_done = false;

	//DLMI_Control Klasse als statische Variabel, welche von überall abgerufen werden kann
	public static DLMI_Control controller;

    bool streaming = false;

	public GameObject Player, Background;

	// Pfad zum Python Interpreter
	[SerializeField]
	private string python = @"C:\Users\Achilleus\Anaconda2\python.exe";

	//Name des Pythonscriptes
	[SerializeField]
	public string scriptName = "testpythonscript.py";

	//Name des Pythonscriptes
	[SerializeField]
	private string dataPath = @"C:\Desktop";

	//Name des Bilddatenordners
	[SerializeField]
	private string folderPath = @"B:\CNN_DataSets";

	[SerializeField]
	//Soll ein Testlauf durchgeführt werden, oder zB nur Debugging
	private bool testing = false;

	public DataContainer data;

	//Objekte aus der Szene
	public Sprite distortion;
	public GameObject points, distract;
	public Sprite pre_Block, pre_Play1, pre_Play2, pre_Play3, pre_Pos1, pre_Pos2, pre_Pos3, pre_Neg1, pre_Neg2, pre_Neg3, pre_Border1, pre_Border2, pre_Border5, pre_Border10;
	public GameObject pre_Positive, pre_Negative;
	public Text StandCount, RightCount, LeftCount, RatioCount;
	public Slider Ratio;
	public InputField Counter, RandomSeed, ArrayPosition;


	public float xOff, yOff, xSpace, ySpace;
	public bool makeStep = false;
	public bool pictureTaken = false;
	public bool playerMoved = false;

	public List<GameObject> forms;
	public List<int> SpawnOrder;
	PythonProcess pro;

	int count = 0;
	public bool allDone = false;
	public bool doneColoring = false;

	public int pictureCount =0;

	public Camera cam;
	public GameObject renderDisplay;
	public List<int> Combinations;
	string current = "";

	void Awake(){

		//Laden aller XML Daten
		this.data = DataContainer.Load (Path.Combine (Application.dataPath, "Data.xml"));

		//Zuordnung von Spieler und Hintergrund mit Tag
		Player = GameObject.FindGameObjectWithTag ("Player");
		Background = GameObject.FindGameObjectWithTag ("Background");

		//Initialisierung der Bildanzahl pro Klasse
		standCount= rightCount= leftCount =0;

		//Erstellung neuer Listen
		forms = new List<GameObject> ();
		Combinations = new List<int> ();


		DontDestroyOnLoad (gameObject);
		controller = this;	

		//Aufruf der Initialiseriungs-Methoden
		InitInfo ();
		InitBackground();
		InitBlocks ();
		InitSpawn ();
		InitPlayer ();
		this.InitiateProcess (python);

		//Render und Texturen
		cam.GetComponent<Camera> ().targetTexture = new RenderTexture (data.Parameters.PictureWidth, data.Parameters.PictureHeight, 16);
		renderDisplay.GetComponent<RawImage> ().texture = cam.GetComponent<Camera> ().targetTexture;
		display = cam.GetComponent<Camera> ().targetTexture;
	}

	//Überprüfung des PythonProzesses
	void Update(){

		if (pro != null) {
			//Falls der PythonProzess eine Rückgabe geliefert hat
			if (pro.Update ()) {
				pro = null;
			}
		}
	}

	#region Init

	//Initialisierung der XML Info Daten
	void InitInfo(){

		this.python = data.Parameters.PythonPath;
		this.folderPath = data.Parameters.FolderPath;
		this.scriptName = data.Parameters.PythonScriptName;
		this.height = data.Parameters.PictureHeight;
		this.width = data.Parameters.PictureWidth;
	}

	//Initialisierung des Hintergrunds
	void InitBackground(){

		//Gibt es Ablenkungen
		if (data.Parameters.Distraction != 0) {
			distract.SetActive (true);
			distract.GetComponent<SpriteRenderer> ().color = data.Parameters.DistractionColor;
		}
	
		//Gibt es Hintergrundrauschen
		if (data.Parameters.BackgroundNoise != 0) {
			points.SetActive (true);
			points.GetComponent<SpriteRenderer> ().color = data.Parameters.NoiseColor;
		}
		// Einstellung der Hintergrundfarbe
		Background.GetComponent<SpriteRenderer> ().color = data.Parameters.BackgroundColor;
	
	}

	//Initialisierung des Spielers
	void InitPlayer(){
		int form = DLMI_Control.controller.data.Parameters.Form;
		//Änderung des Spieleraussehens
		switch (form) {
		case 0:
			//Ändern der Randdicke
			switch (data.Parameters.BorderSize) {
			case 0:
				Player.GetComponent<SpriteRenderer> ().sprite = pre_Block;
				break;
			case 1:
				Player.GetComponent<SpriteRenderer> ().sprite = pre_Border1;
				break;
			case 2:
				Player.GetComponent<SpriteRenderer> ().sprite = pre_Border2;
				break;
			case 5:
				Player.GetComponent<SpriteRenderer> ().sprite = pre_Border5;
				break;
			case 10:
				Player.GetComponent<SpriteRenderer> ().sprite = pre_Border10;
				break;
			default:
				Player.GetComponent<SpriteRenderer> ().sprite = pre_Block;
				break;
			}
			break;
		case 1:
			Player.GetComponent<SpriteRenderer>().sprite = pre_Play1;
			break;
		case 2:
			Player.GetComponent<SpriteRenderer>().sprite = pre_Play2;
			break;
		case 3:
			Player.GetComponent<SpriteRenderer>().sprite = pre_Play3;
			break;
		default:
			Player.GetComponent<SpriteRenderer>().sprite = pre_Block;
			break;
		}
		//Einstellung der Spielerfarbe
		Player.GetComponent<SpriteRenderer> ().color = data.Parameters.PlayerColor;
		int pos = 0;
		if (!data.Parameters.SpawnSystematic) {
			pos = DLMI_Control.controller.data.Parameters.StartLane;
		}
		//Positionierung des Spielers
			Player.GetComponent<Transform> ().position = new Vector3 (-5 + (0.5f * xOff + (xOff + xSpace) * pos) / 10, -5 + this.yOff / 20, 0);
			float y = yOff;
			if (yOff > xOff)
				y = xOff;
			Player.GetComponent<Transform> ().localScale = new Vector3 (y / 10 * data.Parameters.Playersize / 100, y / 10 * data.Parameters.Playersize / 100, y / 10 * data.Parameters.Playersize / 100);

	}

	//Initialisierung der Formen
	void InitBlocks(){

		Color bad = DLMI_Control.controller.data.Parameters.NegativeColor;
		Color good = DLMI_Control.controller.data.Parameters.PositiveColor;
		float size = DLMI_Control.controller.data.Parameters.FormSize;
		int form = DLMI_Control.controller.data.Parameters.Form;

		//Änderung des Spieleraussehens
		switch (form) {
		case 0:
			//Ändern der Randdicke
			switch (data.Parameters.BorderSize) {
			case 0:
				pre_Positive.GetComponent<SpriteRenderer> ().sprite = pre_Block;
				pre_Negative.GetComponent<SpriteRenderer> ().sprite = pre_Block;
				break;
			case 1:
				pre_Positive.GetComponent<SpriteRenderer> ().sprite = pre_Border1;
				pre_Negative.GetComponent<SpriteRenderer> ().sprite = pre_Border1;
				break;
			case 2:
				pre_Positive.GetComponent<SpriteRenderer> ().sprite = pre_Border2;
				pre_Negative.GetComponent<SpriteRenderer> ().sprite = pre_Border2;
				break;
			case 5:
				pre_Positive.GetComponent<SpriteRenderer> ().sprite = pre_Border5;
				pre_Negative.GetComponent<SpriteRenderer> ().sprite = pre_Border5;
				break;
			case 10:
				pre_Positive.GetComponent<SpriteRenderer> ().sprite = pre_Border10;
				pre_Negative.GetComponent<SpriteRenderer> ().sprite = pre_Border10;
				break;
			default:
				pre_Positive.GetComponent<SpriteRenderer> ().sprite = pre_Block;
				pre_Negative.GetComponent<SpriteRenderer> ().sprite = pre_Block;
				break;
			}
			break;
		case 1:
			pre_Positive.GetComponent<SpriteRenderer>().sprite = pre_Pos1;
			pre_Negative.GetComponent<SpriteRenderer>().sprite = pre_Neg1;
			break;
		case 2:
			pre_Positive.GetComponent<SpriteRenderer>().sprite = pre_Pos2;
			pre_Negative.GetComponent<SpriteRenderer>().sprite = pre_Neg2;
			break;
		case 3:
			pre_Positive.GetComponent<SpriteRenderer>().sprite = pre_Pos3;
			pre_Negative.GetComponent<SpriteRenderer>().sprite = pre_Neg3;
			break;
		}

		//Einstellung der Formfarben
		pre_Positive.GetComponent<SpriteRenderer> ().color = good;
		pre_Negative.GetComponent<SpriteRenderer> ().color = bad;
	}

	//Initalisierung der XML Spawn Daten
	void InitSpawn(){

		float den = DLMI_Control.controller.data.Parameters.NumHorizontalLanes;
		float xdis = DLMI_Control.controller.data.Parameters.xSpacePercent;
		float ydis = DLMI_Control.controller.data.Parameters.ySpacePercent;
		int lanes = DLMI_Control.controller.data.Parameters.NumVerticalLanes;

		this.yOff = Mathf.Round (100 / (den * ydis/100 + den + 1)*100f)/100f;
		this.ySpace = Mathf.Round ((this.yOff * ydis / 100)*100f)/100f;
		this.xOff = Mathf.Round ((100 / ((lanes - 1) * xdis / 100 + lanes))*100f)/100f;
		this.xSpace = Mathf.Round ((this.xOff * xdis / 100)*100f)/100f;

		string s = data.Parameters.SpawnMuster;
		string[] split = s.Split ("-" [0]);
		for (int i = 0; i < split.Length; i++) {
			SpawnOrder.Add (int.Parse (split [i]));
		}

		this.train_count = data.Parameters.TrainCount;
		this.test_count = data.Parameters.TestCount;
	}
	#endregion

	#region Player
	//Bewegen des Spielers
	public void MovingPlayer(){
	
		//Zufällige Auswahl einer Bewegung nach rechts oder links
		int i = UnityEngine.Random.Range (-2, 3);
		Manager.manager.MovePlayer (i);
	}
	#endregion

	#region Picture

	//Methode zum Abspeichern der Bilder
	public void SavePicture(byte[] bytes, string name){

		//Abbrechen falls keine Bilddaten vorhanden
		if (bytes.Length == 0) {
			return;
		}

		//s entspricht dem Dateipfad des neuen Bildes
		string s = data.Parameters.FileName;

		//Erstellen der neuen Zielordner
		if (!Directory.Exists (this.folderPath+"/"+s+ "/proto")) {
			Directory.CreateDirectory (this.folderPath+"/"+s+ "/proto"+"/stand");
			Directory.CreateDirectory (this.folderPath+"/"+s+ "/proto"+"/right");
			Directory.CreateDirectory (this.folderPath+"/"+s+ "/proto"+"/left");
		}

		//Speichern in Testordner abhängig von den Klassen
		if (testing) {
			if (!train_done) {
				string sx = "train";
				if (data.Parameters.SpawnSystematic) {
					sx = "proto";
				}
				switch (name) {
				case "stand":
					if (standCount < train_count) {
						File.WriteAllBytes (this.folderPath + "/" + s + "/" + sx + "/stand/stand_" + standCount + ".png", bytes);
						standCount++;
					}
					break;
				case "left":
					if (leftCount < train_count) {
						File.WriteAllBytes (this.folderPath + "/" + s + "/" + sx + "/left/left_" + leftCount + ".png", bytes);
						leftCount++;
					}
					break;
				case "right":
					if (rightCount < train_count) {
						File.WriteAllBytes (this.folderPath + "/" + s + "/" + sx + "/right/right_" + rightCount + ".png", bytes);
						rightCount++;
					}
					break;
				}

				//Erhöhung der Testanzahlen
				StandCount.text = standCount + "";
				RightCount.text = rightCount + "";
				LeftCount.text = leftCount + "";

				if ((standCount >= train_count) && (leftCount >= train_count) && (rightCount >= train_count)) {
					train_done = true;
					print ("train done");
					standCount = leftCount = rightCount = 0;
				}		
			//Speichern in Trainingordner abhängig von den Klassen
			} else if (!test_done) {
				switch (name) {

				case "stand":
					if (standCount < test_count) {
						File.WriteAllBytes (this.folderPath+"/"+s+ "/test"+"/stand/stand_64x64_" + standCount + ".png", bytes);
						standCount++;
					}
					break;
				case "left":
					if (leftCount < test_count) {
						File.WriteAllBytes (this.folderPath+"/"+s+ "/test"+"/left/left_64x64_" + leftCount + ".png", bytes);
						leftCount++;
					}
					break;
				case "right":
					if (rightCount < test_count) {
						File.WriteAllBytes (this.folderPath+"/"+s+ "/test"+"/right/right_64x64_" + rightCount + ".png", bytes);
						rightCount++;
					}
					break;
				}

				//Erhöhung der Trainingsanzahlen
				StandCount.text = standCount+"";
				RightCount.text = rightCount+"";
				LeftCount.text = leftCount+"";


				if ((standCount >= test_count) && (leftCount >= test_count) && (rightCount >= test_count)) {
					test_done = true;
					print ("all done");
					allDone = true;
				}
			}
		}
		if (!data.Parameters.SpawnSystematic) {
			//Anbindung eines Pythonprozesses
			if (data.Parameters.PythonUsed) {
				pro = new PythonProcess ();
				Process p = DLMI_Control.controller.p;
				p.StartInfo.Arguments = scriptName + " " + bytes;
				pro.p = p;
				pro.Start ();
				this.pictureTaken = true;
			} else {
			
				//Falls nicht systematisch und keine Pythonanbindung => Bewgung des Spielers manuell
				this.pictureTaken = true;
				int move = UnityEngine.Random.Range (-1, 2);
				Manager.manager.MovePlayer (move);
			}
		}else{
			this.pictureTaken = true;
		}
	}

	//Methode zum Aufteilen der Bilddaten aus dem Oberordner "proto"
	public void DataSplit(string className){
		
		if (data.Parameters.SpawnSystematic) {
			string s = data.Parameters.FileName;

			//Erstellen der Trainings- Unterordner
			Directory.CreateDirectory (this.folderPath+"/"+s+ "/train"+"/stand");
			Directory.CreateDirectory (this.folderPath+"/"+s+ "/train"+"/right");
			Directory.CreateDirectory (this.folderPath+"/"+s+ "/train"+"/left");

			//Erstellen der Test- Unterordner
			Directory.CreateDirectory (this.folderPath+"/"+s+ "/test"+"/stand");
			Directory.CreateDirectory (this.folderPath+"/"+s+ "/test"+"/right");
			Directory.CreateDirectory (this.folderPath+"/"+s+ "/test"+"/left");


			//Auslesen der Ratio, Count und Seed Daten
			float Ratio = float.Parse (RatioCount.text);
			int Count = int.Parse (Counter.text);

			int ArrayPos = 0;
			if (!ArrayPosition.text.Equals ("")) {
				ArrayPos = int.Parse (ArrayPosition.text);
			}

			float tr = Count * (1 - Ratio);
			float te = Count * Ratio;

			int trainCount = (int)tr;
			int testCount = (int)te;

			int RandomSeedNumber = 1;
			if (!RandomSeed.text.Equals ("")) {
				RandomSeedNumber = int.Parse(RandomSeed.text);
			}
			System.Random rand = new System.Random (RandomSeedNumber);

			//Laden aller Dateipfade
			int o = 0;
			string[] names = Directory.GetFiles (this.folderPath + "/" + data.Parameters.FileName + "/proto" + "/" + className);
			List<string> FileNames = new List<string> (names);

			//Shuffeln der Daten Liste
			List<string> shuffled_FileNames = new List<string>(FileNames);
			int n = shuffled_FileNames.Count;
			int sets = n / trainCount;
			while (n > 1) {
				n--;
				int k = rand.Next (n + 1);
				string value = shuffled_FileNames [k];
				shuffled_FileNames [k] = shuffled_FileNames [n];
				shuffled_FileNames [n] = value;
			}

			//Zufällige Auswahl der Trainingsdaten, Verschiebung in Unterordner
			for (int i = 0; i < trainCount; i++) {

				o = rand.Next (0, FileNames.Count - 1);

				if (File.Exists (this.folderPath + "/" + data.Parameters.FileName + "/train" + "/" + className + "/" + className + "_" + i + ".png"))
					File.Delete (this.folderPath + "/" + data.Parameters.FileName + "/train" + "/" + className + "/" + className + "_" + i + ".png");

				File.Copy (shuffled_FileNames [0+trainCount*ArrayPos], this.folderPath + "/" + data.Parameters.FileName + "/train" + "/" + className + "/" + className + "_" + i + ".png");
				shuffled_FileNames.RemoveAt (0+trainCount*ArrayPos);
			}

			//Zufällige Auswahl der Testdaten, Verschiebung in Unterordner
			for (int i = 0; i < testCount; i++) {
				o = rand.Next (0, shuffled_FileNames.Count - 1);
				if (File.Exists (this.folderPath + "/" + data.Parameters.FileName + "/test" + "/" + className + "/" + className + "_" + i + ".png"))
					File.Delete (this.folderPath + "/" + data.Parameters.FileName + "/test" + "/" + className + "/" + className + "_" + i + ".png");

				File.Copy (shuffled_FileNames [o], this.folderPath + "/" + data.Parameters.FileName + "/test" + "/" + className + "/" + className + "_" + i + ".png");
				shuffled_FileNames.RemoveAt (o);
			}

		}
	
	}

	#endregion

	#region PythonProcess

	public void InitiateProcess(string pyPath){

		//Abspeichern des Bilder-Ordner Pfades und des mit übergebenem Python-Interpreter Pfades
		this.pythonPath = pyPath;

		p = new Process ();
		p.StartInfo.FileName = this.pythonPath;

		//Errors und Outputs werden weitergeleitet
		p.StartInfo.RedirectStandardError = true;
		p.StartInfo.RedirectStandardOutput = true;
		//kein neues Fenster öffnen beim Prozessstart
		p.StartInfo.CreateNoWindow = true;

		//Prozess wird im data Ordner der Anwendung gestartet => hier liegt das Pythonskript
		p.StartInfo.WorkingDirectory = Application.dataPath;
		p.StartInfo.UseShellExecute = false;

	}
	#endregion

	public byte[] GetPictures(){

		//Neue Textur zum einlesen des Renderbilds
		Texture2D tex = new Texture2D (this.width, this.height);

		//Aktive Renderbild wird mit dem RenderBild der zweiten Kamera ersetzt => führt bei zu schnellen Aufrufen zu ruckeln
		RenderTexture nowactive = RenderTexture.active;
		RenderTexture.active = display;
		//das jetzt aktuelle Renderbild wird Pixel für Pixel eingelesen
		tex.ReadPixels (new Rect (0, 0, this.width, this.height), 0, 0);
		//Fertige Textur wird in ein byte-Array encode
		byte[] bytes = tex.EncodeToPNG ();
		//Alte Renderbild wird zurückgesetzt
		RenderTexture.active = nowactive;
		//Textur wird gelöscht / Müll beseitigung
		UnityEngine.Object.Destroy (tex);

		return bytes;
	}

	//Funktion zum systematischen Bewegen der Formen
	public int[] NextStep(int[] lastNumber){

		//Positionierung der Form
		for (int i = 0; i < lastNumber.Length; i++) {
			forms [i].GetComponent<Transform> ().localPosition = new Vector2 (-5+((0.5f * xOff + (xOff + xSpace) * lastNumber[i]) / 10),forms [i].GetComponent<Transform> ().localPosition.y);
			forms [i].GetComponent<MoveObject> ().position = lastNumber[i];
		}

		//Erhöhen der Positionierungszahl
		lastNumber[0]++;

		//Darstellung der neuen richtigen Zahl
		for (int u = 0; u < lastNumber.Length; u++) {
			if (lastNumber [u] > data.Parameters.NumVerticalLanes-1) {
				
				if (lastNumber.Length > 1) {
					lastNumber [u] = 0;
					if (u == lastNumber.Length - 1)
						break;
					else
						lastNumber [u + 1]++;
				} else {
					break;
				}
			} else {
				break;
			}
		}
		return lastNumber;

	}

	//Funktion zum systematischen Ändern der Formart
	public IEnumerator Coloring(){

		//Jede Form startet positiv
		for (int i = 0; i < forms.Count; i++) {
		
			forms[i].GetComponent<BlockValue> ().value = 5;
			forms [i].GetComponent<SpriteRenderer> ().sprite = pre_Positive.GetComponent<SpriteRenderer> ().sprite;
			forms [i].GetComponent<SpriteRenderer> ().color = pre_Positive.GetComponent<SpriteRenderer> ().color;
		}
		//Bewegung des Spielers und anschließendes Speichern eines Bildes
		for(int i=0; i<data.Parameters.NumVerticalLanes;i++){
			Player.transform.position = new Vector3 (-5 + ((0.5f * xOff + (xOff + xSpace) * i) / 10), Player.transform.position.y, 0.0f);
			Manager.manager.PlayerPosition = i;
			if (DLMI_Control.controller.data.Parameters.BackgroundNoise == 2) {
				Manager.manager.MoveBackground (true);
			}
			if (DLMI_Control.controller.data.Parameters.Distraction == 2) {
				Manager.manager.MoveBackground (false);
			}
			yield return new WaitForEndOfFrame();
			Decider (DLMI_Control.controller.GetPictures (), i);
			yield return new WaitUntil(()=> pictureTaken);
			pictureCount++;
			this.pictureTaken = false;
		}

		//Durchlaufen aller möglichen Formart Kombinationen
		List<int> offsets = new List<int> ();
		int step = data.Parameters.NumHorizontalLanes - 1;
		while (offsets.Count < data.Parameters.NumHorizontalLanes) {
			for (int i = 0; i < offsets.Count; i++) {
				offsets [i] = i;
			}
			offsets.Add (offsets.Count);
			while (offsets [0] <= (step-(offsets.Count-1))) {
				for (int i = 0; i < forms.Count; i++) {
					forms[i].GetComponent<BlockValue> ().value = 5;
					forms [i].GetComponent<SpriteRenderer> ().sprite = pre_Positive.GetComponent<SpriteRenderer> ().sprite;
					forms [i].GetComponent<SpriteRenderer> ().color = pre_Positive.GetComponent<SpriteRenderer> ().color;
				}
				for (int i = 0; i < offsets.Count; i++) {
					forms[offsets[i]].GetComponent<BlockValue> ().value = -5;
					forms [offsets[i]].GetComponent<SpriteRenderer> ().sprite = pre_Negative.GetComponent<SpriteRenderer> ().sprite;
					forms [offsets[i]].GetComponent<SpriteRenderer> ().color = pre_Negative.GetComponent<SpriteRenderer> ().color;
				
				}

				//Bewegung des Spielers und anschließendes Speichern eines Bildes
				for(int i=0; i<data.Parameters.NumVerticalLanes;i++){
					Player.transform.position = new Vector3 (-5 + ((0.5f * xOff + (xOff + xSpace) * i) / 10), Player.transform.position.y, 0.0f);
					Manager.manager.PlayerPosition = i;

					if (DLMI_Control.controller.data.Parameters.BackgroundNoise == 2) {
						Manager.manager.MoveBackground (true);
					}
					if (DLMI_Control.controller.data.Parameters.Distraction == 2) {
						Manager.manager.MoveBackground (false);
					}
					yield return new WaitForEndOfFrame();
					Decider (DLMI_Control.controller.GetPictures (), i);
					yield return new WaitUntil(()=> pictureTaken);
					pictureCount++;
					this.pictureTaken = false;
				}

				//Erhöhen der Färbungszahl
				offsets [offsets.Count - 1]++;
				int check = step;
				bool next = false;
				for (int u = offsets.Count - 1; u >= 0; u--) {
					if (offsets [u] > check) {
						if (u != 0) {

							offsets [u - 1]++;
							next = true;
							check--;
						} else {
							break;
						}

					} else {
					
						if(next){
							for (int i = u+1; i <offsets.Count; i++) {
								offsets [i] = offsets [i-1]+1;
							}
							break;
						}
					}
				}
			}
		}
		this.doneColoring = true;
	}

	#region Decider

	//Funktionen zur Aktionsbewertung
	public void Decider(byte [] Picture, int PlayerPosition){


		//Falls nächste Form positiv ist
		if (IsPositive(0)) {

			//GetPosition gibt die Position der Form an
			//PlayerPosition entspricht der Position des Spielers
			if (GetPosition(0) > PlayerPosition) {
				SavePicture (Picture, "right");
			}
			if (GetPosition(0) == PlayerPosition) {
				SavePicture (Picture, "stand");
			}
			if (GetPosition(0) < PlayerPosition) {
				SavePicture (Picture, "left");
			}
		} else {
			if (GetPosition (0) != PlayerPosition) {
				SavePicture (Picture, "stand");
				return;
			}

			//Falls ein iterativer Ansatz gewählt wurde werden mehrere Formen in die Entscheidung miteinbezogen
			if (data.Parameters.Iteration) {
				for (int i = 1; i < forms.Count; i++) {

					if (IsPositive (i)) {
						if (GetPosition (i) > PlayerPosition) {
							SavePicture (Picture, "right");
							return;
						}
						if (GetPosition (i) == PlayerPosition) {
							if (PlayerPosition != data.Parameters.NumVerticalLanes) {
								SavePicture (Picture, "right");
								return;
							} else {
								SavePicture (Picture, "left");
								return;
							}
						}
						if (GetPosition (i) < PlayerPosition) {
							SavePicture (Picture, "left");
							return;
						}
					}
					if (i == forms.Count - 1) {
						if (PlayerPosition != 0) {
							SavePicture (Picture, "left");
							return;
						} else {
							SavePicture (Picture, "right");
							return;
						}
					}
				}
			} else {
				if (PlayerPosition != data.Parameters.NumVerticalLanes-1) {
					SavePicture (Picture, "right");
					return;
				} else {
					SavePicture (Picture, "left");
					return;
				}
			}
		}
	}

	//Gibt die Art einer Form
	bool IsPositive(int i){
			if (forms [i].GetComponent<BlockValue> ().value > 0)
				return true;
			else
				return false;
	}

	//Gibt die Position der gwünschten Form
	int GetPosition(int i){
			return DLMI_Control.controller.forms [i].GetComponent<MoveObject> ().position;
	}

	//RatioSlider soll nur in 5% Schritten veränderbar sein
	public void UpdateRatio(Slider slid){
	
		float x = slid.value /20;
		RatioCount.text = x + "";
	}

	//Maxima der Bilddaten
	public void SetMaximum(){
		Counter.text = leftCount + "";
	}

	//Überprüfen ob gewünschte Anzhal im Rahmen der möglichen Bildanzahl liegt
	public void CheckValue(){
	
		if (leftCount == 0)
			return;
		int value = int.Parse (Counter.text);

		if (value > leftCount)
			Counter.text = leftCount + "";
	}
	#endregion
}
