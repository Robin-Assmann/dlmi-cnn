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
using System;
using System.Net;
using System.Net.Sockets;


public class DLMI_Control : MonoBehaviour {

	//RenderBild der zweiten Kamera, welches den Spiel-Bildschirm in kleinerer Auflösung aufnimmt
	[SerializeField]
	private RenderTexture display;

	[SerializeField]
	private int train_count = 200;

	[SerializeField]
	private int test_count = 100;

	[SerializeField]
	public float speed = -2.0f;

	[SerializeField]
	public float wait_time = 0.3f; //0.3 = fast; 0.6 = normal; 1.2 = slow

	[SerializeField]
	public int lane_count = 7;

	[SerializeField]
	public int height = 64;

	[SerializeField]
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
	private bool test_done = false;
	[SerializeField]
	private bool train_done = false;

	//DLMI_Control Klasse als statische Variabel, welche von überall abgerufen werden kann
	public static DLMI_Control controller;

    bool streaming = false;

	NamedPipeServerStream pipeServers;

	public GameObject Player;

	// Pfad zum Python Interpreter
	[SerializeField]
	private string python = @"C:\Users\Achilleus\Anaconda2\python.exe";

	//Name des Pythonscriptes
	[SerializeField]
	public string scriptName = "testpythonscript.py";

	//Name des Pythonscriptes
	[SerializeField]
	private string dataPath = @"C:\Desktop";

	[SerializeField]
	private string folderPath = @"B:\CNN_DataSets\autoplay_bilder";

	[SerializeField]
	private bool testing = false;

	void Awake(){
		print (speed);
		Player = GameObject.FindGameObjectWithTag ("Player");
		standCount= rightCount= leftCount =0;

		//Verhindern, dass dieses Skript zerstört wird 
		//Objekt auf dem dieses Skript liegt wird beim Laden einer neuen Szene nicht zerstört
		DontDestroyOnLoad (gameObject);
		controller = this;	
	}

	void Start(){

		//Shell Prozess wird mit dem Pfad zum Python Interpreter initialisiert
		this.InitiateProcess (python);
		//StartCoroutine (PlayerRoutine ());
	}

	#region Player

	IEnumerator PlayerRoutine(){
	
		WaitForSeconds wait = new WaitForSeconds (2.0f);

		while (true) {
		
			MovingPlayer ();

			yield return wait;
		}
	}

	public void MovingPlayer(){
	
		int i = UnityEngine.Random.Range (-2, 3);
		Manager.manager.MovePlayer (i);
	
	}
	#endregion

	#region Picture

	public void TakePicture_PythonInteraction(){

		//Nebenroutine zum Bild abspeichern und Shell Prozess
		StartCoroutine (IE_TakePicture_PythonInteraction ());
	}

	private IEnumerator IE_TakePicture_PythonInteraction(){

		//Bild abspeichern
		DLMI_Control.controller.TakePicture ();

		//Warten bis das Bild abgespeichert ist, sonst kommt es beim Pythonskript zu Komplikationen
		yield return new WaitUntil (() => DLMI_Control.controller.picture_Taken);
		//picture_Taken Wert wird zurückgesetzt, damit der nächste Ablauf wieder korrekt läuft
		DLMI_Control.controller.SwitchPictureTaken ();

		//das Skript "scriptName" wird als Shellprozess gestartet und der Output in einem Textfeld angezeigt
		//string output = DLMI_Control.controller.StartProcess ();
	}

	public void TakePicture(){

		//Falls noch kein Ordner für die Bilder besteht, wird einer erstellt
		if (!Directory.Exists (folderPath)) {
			Directory.CreateDirectory (folderPath);
		}

		//Starten der Bild-Speicher Routine
		StartCoroutine (SavePicture ());	
	}

	private IEnumerator SavePicture(){

		//Auf das Ende des Frames warten, damit das Bild fertig gerendert hat
		yield return new WaitForEndOfFrame ();
		//Neue Textur zum einlesen des Renderbilds
		Texture2D tex = new Texture2D (64,64);

		//Aktive Renderbild wird mit dem RenderBild der zweiten Kamera ersetzt => führt bei zu schnellen Aufrufen zu ruckeln
		RenderTexture nowactive = RenderTexture.active;
		RenderTexture.active = display;
		//das jetzt aktuelle Renderbild wird Pixel für Pixel eingelesen
		tex.ReadPixels (new Rect (0, 0, 64, this.height), 0, 0);

		//Fertige Textur wird in ein byte-Array encoded
		byte[] bytes = tex.EncodeToPNG ();
		print (bytes.Length);

		//Bild wird in dem vorgesehenem Ordner als .png Datei abgelegt
		File.WriteAllBytes (Application.dataPath + "/Pictures/Test_Picture_" + standCount + ".png", bytes);
		//Alte Renderbild wird zurückgesetzt
		RenderTexture.active = nowactive;
		//Textur wird gelöscht / Müll beseitigung
		UnityEngine.Object.Destroy (tex);
		//Bennenungs-Integer wird erhöht, damit die bereits erstellten Bilder nicht überschrieben werden
		standCount++;
		//Bild gilt als abgespeichert
		SwitchPictureTaken ();
	}

	public void SwitchPictureTaken(){
	
		//Toggeln des boolean Wertes "picture_Taken"
		this.picture_Taken = !this.picture_Taken;
	}


	public void SavePicture(byte[] bytes, string name){
		if (bytes.Length == 0) {
			return;
		}
		if (testing) {
			if (!train_done) {
				switch (name) {

				case "stand":
					if (standCount < train_count) {
						File.WriteAllBytes (this.folderPath + "/64x64_train_200/stand/stand_64x64_" + standCount + ".png", bytes);
						standCount++;
					}
					break;
				case "left":
					if (leftCount < train_count) {
						File.WriteAllBytes (this.folderPath + "/64x64_train_200/left/left_64x64_" + leftCount + ".png", bytes);
						leftCount++;
					}
					break;
				case "right":
					if (rightCount < train_count) {
						File.WriteAllBytes (this.folderPath + "/64x64_train_200/right/right_64x64_" + rightCount + ".png", bytes);
						rightCount++;
					}
					break;
				}

				if ((standCount >= train_count) && (leftCount >= train_count) && (rightCount >= train_count)) {
					train_done = true;
					print ("train done");
					standCount = leftCount = rightCount = 0;
				}

			} else if (!test_done) {
				switch (name) {

				case "stand":
					if (standCount < test_count) {
						File.WriteAllBytes (this.folderPath + "/64x64_test_100/stand/stand_64x64_" + standCount + ".png", bytes);
						standCount++;
					}
					break;
				case "left":
					if (leftCount < test_count) {
						File.WriteAllBytes (this.folderPath + "/64x64_test_100/left/left_64x64_" + leftCount + ".png", bytes);
						leftCount++;
					}
					break;
				case "right":
					if (rightCount < test_count) {
						File.WriteAllBytes (this.folderPath + "/64x64_test_100/right/right_64x64_" + rightCount + ".png", bytes);
						rightCount++;
					}
					break;
				}

				if ((standCount >= test_count) && (leftCount >= test_count) && (rightCount >= test_count)) {
					test_done = true;
					print ("all done");
				}
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

//	public IEnumerator StartProcess(byte[] pic){
//		print ("instance");
//		Process new_p = new Process();
//		new_p.StartInfo = p.StartInfo;
//		string output = "";
//		//Prozess startet das übergebene Pythonskript und übergibt als Argument den Bilder-Ordner Pfad
//		//p.StartInfo.Arguments = scriptName + " " + this.folderPath;
//		//string i = GetPictures().ToString();
//
//		new_p.StartInfo.Arguments = scriptName +" "+ pic;
//		new_p.Start ();
//		yield return new WaitForSeconds(0.9f);
//		print ("endsleep");
//		//Output des Pythonskripts wird ausgelesen
//		output = new_p.StandardOutput.ReadToEnd ();
//		print (output);
//		//Prozess wird nach dem Ende geschlossen
//		new_p.WaitForExit ();
//		new_p.Close ();
//
//		//Output wird zurückgegeben
//		yield return output;
//
//	}

	#endregion

	public byte[] GetPictures(){

		//Neue Textur zum einlesen des Renderbilds
		Texture2D tex = new Texture2D (this.width, this.height);

		//Aktive Renderbild wird mit dem RenderBild der zweiten Kamera ersetzt => führt bei zu schnellen Aufrufen zu ruckeln
		RenderTexture nowactive = RenderTexture.active;
		RenderTexture.active = display;
		//das jetzt aktuelle Renderbild wird Pixel für Pixel eingelesen
		tex.ReadPixels (new Rect (0, 64-this.height, this.width, this.height), 0, 0);
		//Fertige Textur wird in ein byte-Array encode
		//tex.Resize(this.width, this.height);
		byte[] bytes = tex.EncodeToPNG ();

		//Alte Renderbild wird zurückgesetzt
		RenderTexture.active = nowactive;
		//Textur wird gelöscht / Müll beseitigung
		UnityEngine.Object.Destroy (tex);

		return bytes;
	}

	public void WriteBytes(byte[] picture, int move, int res){

		//System.Text.Encoding.UTF8.GetString(picture)

		if (!File.Exists (dataPath))
			File.WriteAllText (dataPath,"Pic"  + "," + move +"," + res + Environment.NewLine);
		else 
			File.AppendAllText (dataPath, "Pic" + "," + move +"," + res + Environment.NewLine);
	}

    public void ToggleStreaming() {

        streaming = !streaming;
    }
}
