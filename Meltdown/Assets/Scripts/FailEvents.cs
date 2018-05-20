using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FailEvents : MonoBehaviour {

	public AudioSource sirens;
	public GameObject lights;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}



	void FailedSequence()
	{
		sirens.Play ();
		lights.SetActive (true);

	}
}
