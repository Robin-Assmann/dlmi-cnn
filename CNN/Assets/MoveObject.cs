using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour {

	public Rigidbody rigid;
	public byte[] CriticalPicture;
	public int LastMove = 0;
	public int lane = 0;

	public float PlayerPos;

	void Start () {
		rigid = GetComponent<Rigidbody> ();
		rigid.velocity = new Vector3 (0, DLMI_Control.controller.speed, 0);
		this.lane = (int)transform.position.x;
	}

	void FixedUpdate(){
	
		rigid.velocity = new Vector3 (0, DLMI_Control.controller.speed*100*Time.deltaTime, 0);
	}


	void OnCollisionEnter(Collision col){

		int v = GetComponent<BlockValue> ().value;
		int result = 0;

		if (v < 0) {
			if (col.collider.gameObject.tag.Equals ("Player")) {
				result = -2;
				Decider (result);
			} else {
				result = 1;
				Decider (result);
			}
		
		} else {
			if (col.collider.gameObject.tag.Equals ("Player")) {
				result = 2;
				Decider (result);
			} else {
				result = -1;
				Decider (result);
			}
		}

		//DLMI_Control.controller.WriteBytes (CriticalPicture, LastMove, result);
		Destroy (gameObject);

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
					//print ((result == 2) +""+ (transform.position.x == Manager.manager.edge)+"" + PlayerPos);
					//print("right"+(PlayerPos == transform.position.x)+ PlayerPos);
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
					//print ((result == 2) +""+ (transform.position.x == -Manager.manager.edge)+"" + PlayerPos);
					//print("left"+(PlayerPos == transform.position.x)+ PlayerPos);
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
