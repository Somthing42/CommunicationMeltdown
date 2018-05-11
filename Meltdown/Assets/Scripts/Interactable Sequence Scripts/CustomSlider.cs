using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class CustomSlider : Interactable {

	public float activationValue;
	VRTK_Slider baseSlider;

	// Use this for initialization
	void Start () {
		baseSlider = GetComponent<VRTK_Slider> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public override void OnObjectUsed ()
	{
		float sliderPosition = baseSlider.GetValue ();
		if (sliderPosition >= activationValue) {
			Debug.Log ("Boom");
			base.OnObjectUsed ();
		}
	}
}
