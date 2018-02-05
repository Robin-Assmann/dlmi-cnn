using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using System.Threading;
using System.Diagnostics;

public class PythonProcess : ThreadedProcess
{
    public Process p;  // arbitary job data
	public GameObject s;
	public byte[] picture;
    public string move; // arbitary job data

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
        // This is executed by the Unity main thread when the job is finished
		Manager.manager.MovePlayer (int.Parse(move));
		s.transform.parent.GetComponent<MoveObject> ().LastMove = int.Parse(move);
		s.transform.parent.GetComponent<MoveObject> ().CriticalPicture = picture;
    }
}
