using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class CustomLever : Interactable {

	public enum TriggerDirection { Positive, Negative, Both}
	[Header("Activation Info")]
	public TriggerDirection activationDirection;
	public float positiveActivationValue;
	public float negativeActivationValue;
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
		switch (activationDirection)
		{
			case TriggerDirection.Positive:
				if (leverPosition >= positiveActivationValue)
				{
					Debug.Log("Triggered");
					base.OnObjectUsed();
				}
				break;

			case TriggerDirection.Negative:
				if (leverPosition <= negativeActivationValue)
				{
					Debug.Log("-Triggered");
					base.OnObjectUsed();
				}
				break;

			case TriggerDirection.Both:
				if(leverPosition >= positiveActivationValue || leverPosition <= negativeActivationValue)
				{
					Debug.Log("Trigger");
					base.OnObjectUsed();
				}
				break;

			default:
				break;

		}
	}
}
