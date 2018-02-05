using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineWithData{

	public Coroutine coroutine { get; private set;}
	public object result;
	private IEnumerator target;

	public CoroutineWithData(MonoBehaviour owner, IEnumerator target){
	
		this.target = target;
		this.coroutine = owner.StartCoroutine (Run ());
	}

	private IEnumerator Run(){
		int i = 0;
		while (target.MoveNext ()) {
			i++;
			if (i > 1) {
				result = target.Current;
				yield return result;
			}
		
		}
	}
}
