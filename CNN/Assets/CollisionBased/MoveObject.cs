using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour {

	public Rigidbody rigid;
	public byte[] CriticalPicture;
	public int LastMove = 0;
	public int lane = 0;

	public float PlayerPos;

	private int steps = 0;
	private int stepcount = 0;
	private bool done= false;

	public int position = 0;


	void Awake(){
		rigid = GetComponent<Rigidbody> ();
		this.steps = DLMI_Control.controller.data.Parameters.NumHorizontalLanes;

	}

	public void MakeStep(){
		rigid.position = new Vector3(rigid.position.x,rigid.position.y-((DLMI_Control.controller.yOff+DLMI_Control.controller.ySpace)/10),rigid.position.z);
		this.stepcount++;
		if (stepcount == steps) {
			this.done = true;
			DestroyObject ();

		}
	}

	void MakePicture(){
		DLMI_Control.controller.Decider (DLMI_Control.controller.GetPictures (),Manager.manager.PlayerPosition);
	}

	void DestroyObject(){

		DLMI_Control.controller.forms.RemoveAt (0);
		DestroyImmediate (this.gameObject);
	}

	private void Decider(int result){

		float pos = Mathf.Round (transform.position.x);

		switch (LastMove) {

		case 0:
			if (result > 0) {
					DLMI_Control.controller.SavePicture (CriticalPicture, "stand");
			} else {
				if (result == -2) {
					if (pos <= 0) {
						DLMI_Control.controller.SavePicture (CriticalPicture, "right");
					} else {
						DLMI_Control.controller.SavePicture (CriticalPicture, "left");
					}
				}else{			
					if (pos > PlayerPos) {
						DLMI_Control.controller.SavePicture (CriticalPicture, "right");
					} else {
						DLMI_Control.controller.SavePicture (CriticalPicture, "left");
					}
				}
			}
			break;
		case 1:
			if (result > 0) {
				if ((result == 2) && (pos == Manager.manager.edge) && (PlayerPos ==pos))
					DLMI_Control.controller.SavePicture (CriticalPicture, "stand");
				else {
					DLMI_Control.controller.SavePicture (CriticalPicture, "right");
				}
			} else {
				if (result == -2) {
					if (pos <= Manager.manager.edge - 1) {
						DLMI_Control.controller.SavePicture (CriticalPicture, "stand");
					} else {
						DLMI_Control.controller.SavePicture (CriticalPicture, "left");
					}
				} else {
					print (pos + " - " + PlayerPos + " = " +(pos == PlayerPos));
					if (pos == PlayerPos) {
						DLMI_Control.controller.SavePicture (CriticalPicture, "stand");
						break;
					} 
					if (pos >0){
						DLMI_Control.controller.SavePicture (CriticalPicture, "right");
					}
					if (pos <0){
						DLMI_Control.controller.SavePicture (CriticalPicture, "left");
					}
				}
			}
			break;
		case -1:
			if (result > 0) {
				if ((result == 2) && (pos == -Manager.manager.edge) && (PlayerPos ==pos))
					DLMI_Control.controller.SavePicture (CriticalPicture, "stand");
				else {

					DLMI_Control.controller.SavePicture (CriticalPicture, "left");
				}
			} else {
				if (result == -2) {
					if (pos >= (-Manager.manager.edge) + 1) {
						DLMI_Control.controller.SavePicture (CriticalPicture, "stand");
					} else {
						DLMI_Control.controller.SavePicture (CriticalPicture, "right");
					}
				} else {
					if (pos == PlayerPos){
						print ("left stand" + PlayerPos);
						DLMI_Control.controller.SavePicture (CriticalPicture, "stand");
						break;
					}
					if (pos >=0) {
						DLMI_Control.controller.SavePicture (CriticalPicture, "right");
					} 

					if (pos <0){
						DLMI_Control.controller.SavePicture (CriticalPicture, "left");
					}
				}
			}
			break;
		}
	}
}
