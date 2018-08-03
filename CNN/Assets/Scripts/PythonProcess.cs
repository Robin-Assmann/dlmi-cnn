using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using System.Threading;
using System.Diagnostics;

//Pythonprozess als Thread
public class PythonProcess : ThreadedProcess
{
    public Process p;
    public string move;

    protected override void ThreadFunction()
    {
		p.Start ();
		//Output des Pythonskripts wird ausgelesen
		move = p.StandardOutput.ReadToEnd ();
		//Prozess wird nach dem Ende geschlossen
		p.WaitForExit ();
		p.Close ();
    }
    protected override void OnFinished()
    {
       //Bewgung wird nach dem Prozess durchgeführt
		Manager.manager.MovePlayer (int.Parse(move));
    }
}
