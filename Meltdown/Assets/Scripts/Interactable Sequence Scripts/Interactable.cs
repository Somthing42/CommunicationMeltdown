using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {
	public Sprite itemImage;
	public int itemIndex;

	public event Action<Interactable> ObjectUsed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public virtual void OnObjectUsed()
	{

		if(ObjectUsed != null)
		{
			ObjectUsed(this);
		}
	}
}
