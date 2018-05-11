using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class CustomDial : Interactable {

	public float activationValue;
	VRTK_Knob baseDial;
	// Use this for initialization
	void Start () {
		baseDial = GetComponent<VRTK_Knob>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public override void OnObjectUsed()
	{
		float dialPosition = baseDial.GetValue();
		if (dialPosition >= activationValue)
		{
			Debug.Log("Correct Position");
			base.OnObjectUsed();
		}
	}
}
