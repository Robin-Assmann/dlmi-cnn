using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Diagnostics;

public class TriggerPicture : MonoBehaviour {

	PythonProcess pro;

	void OnTriggerEnter(Collider other){

		if (other.tag.Equals ("Player")) {

			byte[] picture = DLMI_Control.controller.GetPictures ();
			transform.parent.GetComponent<MoveObject> ().PlayerPos = Manager.manager.Player.transform.position.x;
			pro = new PythonProcess ();
			Process p = DLMI_Control.controller.p;
			p.StartInfo.Arguments = DLMI_Control.controller.scriptName +" "+ picture;

			pro.p = p;
			pro.s = gameObject;
			pro.picture = picture;
			pro.Start ();
		}
	}

	void Update(){
	
		if (pro != null) {
		
			if (pro.Update ()) {
			
				pro = null;
			}
		}
	
	}
}
