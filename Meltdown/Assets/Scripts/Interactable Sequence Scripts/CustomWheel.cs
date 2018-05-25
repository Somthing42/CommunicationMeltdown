using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class CustomWheel : Interactable {

	public float activationValue;
	VRTK_Wheel baseWheel;

	// Use this for initialization
	void Start () {
		baseWheel = GetComponent<VRTK_Wheel> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public override void OnObjectUsed() {
		float wheelPos = baseWheel.GetValue ();
		if (wheelPos >= activationValue) {
			Debug.Log ("Correct Wheel Pos");
			base.OnObjectUsed ();
		}
	}

}
