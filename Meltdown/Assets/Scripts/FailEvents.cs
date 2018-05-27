using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FailEvents : MonoBehaviour {

	public AudioSource sirens;
	public GameObject lights;

	// Use this for initialization
	void Start () {
		
	}
	//Hust so desighners can put their lights and sounds in the event.
	public void FailedSequence()
	{
		sirens.Play ();
		lights.SetActive (true);

	}

	// for me so I can test.
	public void TestFailedSequence()
	{
		Debug.Log ("You failed try again.");
	}
}
