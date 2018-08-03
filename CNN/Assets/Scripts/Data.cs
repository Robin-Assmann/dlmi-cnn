using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Klasse mit allen XML Variablen
public class Data{
	public string FileName { get; set;}
	public string FolderPath { get; set;}
	public bool PythonUsed { get; set;}
	public string PythonPath { get; set;}
	public string PythonScriptName { get; set;}

	public int BackgroundNoise { get; set;}
	public Color32 BackgroundColor { get; set;}
	public Color32 NoiseColor { get; set;}
	public int Distraction { get; set;}
	public Color32 DistractionColor { get; set;}

	public int NumVerticalLanes { get; set;}
	public int NumHorizontalLanes{ get; set;}
	public int Distance { get; set;}
	public int xSpacePercent { get; set;}
	public int ySpacePercent { get; set;}

	public Color32 PositiveColor{ get; set;}
	public Color32 NegativeColor{ get; set;}
	public int Form { get; set;}
	public float FormSize { get; set;}
	public int BorderSize { get; set;}

	public float Playersize { get; set;}
	public Color32 PlayerColor{ get; set;}
	public int StartLane { get; set;}

	public int Spawn { get; set;}
	public string SpawnMuster { get; set;}
	public float SpawnRatio { get; set;}
	public bool SpawnSystematic { get; set;}

	public int TestCount { get; set;}
	public int TrainCount { get; set;}
	public int PictureHeight { get; set;}
	public int PictureWidth { get; set;}
	public bool Iteration { get; set;}

	public Data(){
	
	}
}
