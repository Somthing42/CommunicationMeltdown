using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class CustomThrottle : Interactable {

	[Tooltip("Upper Position")]
	public float activationValue1;
	[Tooltip("Middle Position")]
	public float activationValue2;
	[Tooltip("Lower Position")]
	public float activationValue3;
	VRTK_Lever baseLever;

	// Use this for initialization
	void Start () {
		baseLever = GetComponent<VRTK_Lever>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public override void OnObjectUsed()
	{
		float leverPosition = baseLever.GetValue();
		Debug.Log(leverPosition);

		if(leverPosition >= activationValue1)
		{
			Debug.Log("Position 1");
			base.OnObjectUsed();
		}

		if (leverPosition == activationValue2)
		{
			Debug.Log("Position 2");
			base.OnObjectUsed();
		}

		if (leverPosition <= activationValue3)
		{
			Debug.Log("Position 3");
			base.OnObjectUsed();
		}


	}
}
