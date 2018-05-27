﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SequenceManager : MonoBehaviour {
    [Header("Sequence Information")]
	public List<Interactable> masterSequence;				//main sequence (contains all interactables)
    public Queue<Interactable> interactedObjects;			//interacted objects queue
    public int[] sequenceSizes;								//sequence sizes array
    [HideInInspector]
    public int currentSequence = 0;							//current sequence location
    [HideInInspector]
    public int currentSequenceSize;							//size of current sequence
   
    public Console[] consoles;								//console objects array

    [Header("Display Information")]
    public SpriteRenderer OfficeDisplay;					//sprite display
    public Text sequenceText;								//text display
	public Text authText;
    public float stepTransitionSpeed = 3.0f;				//time till display change

    [Header("Authentication Button Info")]
	public GameObject lerpObject;							//authenticate button lerp
    [HideInInspector]
    public bool isAnimating = false;						//is the object animated currently



    // Use this for initialization
    void Start() {

        consoles = FindObjectsOfType<Console>();							//add all consoles to console array
        CreateSequence();													//run sequence creation function
        currentSequenceSize = sequenceSizes[currentSequence];				//set current sequence size to start of sequence
        interactedObjects = new Queue<Interactable>(currentSequenceSize);	//create new queue for interacted objects
        StartCoroutine("DisplaySequenceToRenderer");						//start display coroutine
    }

    // Update is called once per frame
    void Update() {
    }

    public void CreateSequence()											//create sequence function
    {
        foreach (Console console in consoles)								//for each console objects in console array
        {
            foreach (Interactable interactable in console.interactables)	//for each interactable on the console
            {
                masterSequence.Add(interactable);							//add to master sequence
                interactable.ObjectUsed += AddUsedObjectToList;				//
            }

        }

        ShuffleSequence();													//run shuffle function
    }

    void ShuffleSequence()													//function to shuffle sequence
    {
        for (int count = 0; count < masterSequence.Count; count++)						//for the entirety of the master sequence
        {
            int randomValue = UnityEngine.Random.Range(count, masterSequence.Count);	//create a random value
            Interactable holder = masterSequence[randomValue];							//temp var to store the interactable from master sequence at randomVal location
            masterSequence[randomValue] = masterSequence[count];						//place the object at count location into the randomVal location
            masterSequence[count] = holder;												//replace the temp object into the master sequence at count location
        }
    }

    // NOTE(barret): This needs to be tested. I don't know how reliable this is. 
    /* I'm not exactly sure how Photon does sycronization, but there might be a 
     * problem with desyced packets. If two players use an interactable at around 
     * the same time and send their new list to the master, which message does the 
     * master use to update first. 
    */
    void AddUsedObjectToList(Interactable _interactable)				//add used object function
    {

		if (interactedObjects.Count + 1 > currentSequenceSize)			//if the count of interactable objects (plus one) is greater than the current sequence size
        {

            interactedObjects.Dequeue();								//remove the object at the beginning of the queue
            interactedObjects.Enqueue(_interactable);					//add the passed in object to the queue
        }
        else             												//else
        {
            interactedObjects.Enqueue(_interactable);					//add the passed in object to the queue
        }

        if (!PhotonNetwork.isMasterClient)													//if not master client
        {
            PhotonView photonView = PhotonView.Get(this);									//set photon view to this
            photonView.RPC("UpdateQueue", PhotonTargets.MasterClient, interactedObjects);	//rpc pass to master client
        }
		else 																				//else (is master client)
        {
            PhotonView photonView = PhotonView.Get(this);									//set photon view to this
            photonView.RPC("UpdateQueue", PhotonTargets.Others, interactedObjects);			//rpc pass to others
        }
    }

    [PunRPC]
    void UpdateQueue(Queue<Interactable>  Sequence)										//update queue function for photon
    {
        interactedObjects = Sequence;													//set interacted objects queue to passed in queue

        if (PhotonNetwork.isMasterClient)												//if master client
        {
            PhotonView photonView = PhotonView.Get(this);								//set photon view to this
            photonView.RPC("UpdateQueue", PhotonTargets.Others, interactedObjects);		//rpc pass to others
        }
    }

	public void Authenticate()																//authenticate function
	{
		bool authenticated = false;															//set authenticated to false initially

		if (interactedObjects.Count == currentSequenceSize) {								//if the count of interacted objects is equal to current sequence size

			//if (Input.GetKeyDown (KeyCode.Space)) {											//~~~~if space is pressed~~~~ not sure how to change this to physical button if needed
				Interactable[] listToCheck = interactedObjects.ToArray ();					//convert interacted objects queue to an array and store in list to check
				int offset = 0;
				for (int count = 0; count < currentSequence; count++) {
					offset += sequenceSizes [count];

				}

				
				for (int count = offset; count < currentSequenceSize + offset; count++) {
					if (listToCheck [count - offset].itemIndex == masterSequence [count].itemIndex) {
						authenticated = true;
					} else {
						authenticated = false;
						break;
					}
				}

				if (authenticated) {
					interactedObjects.Clear ();
					if (currentSequence < sequenceSizes.Length - 1) {
						currentSequence++;
						currentSequenceSize = sequenceSizes [currentSequence];
					}
					Debug.Log ("Authenticated");
					authText.text = "Authenticated";

				} else {
					interactedObjects.Clear ();
					Debug.Log ("Rejected");
					authText.text = "Rejected";
				}
			//}
		} else {
			interactedObjects.Clear ();
			Debug.Log ("Rejected");
			authText.text = "Rejected";
		}
	}

	IEnumerator DisplaySequenceToRenderer()
	{
		while (true)
		{
			int offset = 0;
			for (int count = 0; count < currentSequence; count++)
			{
				offset += sequenceSizes[count];

			}

			for (int count = offset; count < currentSequenceSize + offset; count++)
			{
				sequenceText.text = (count - offset + 1).ToString();
				OfficeDisplay.sprite = masterSequence[count].itemImage;

				Debug.Log("Item that Should be showing: " + masterSequence[count].itemIndex);
				yield return new WaitForSeconds(stepTransitionSpeed);

			}

		}
	}

	public IEnumerator lerpDown(float time)
	{
		if (isAnimating == false)
		{
			isAnimating = true;
			Vector3 start = this.gameObject.transform.position;
			Vector3 end = new Vector3(start.x, start.y - 0.01f, start.z);
			float duration = 0.0f;
			while (duration < time)
			{
				duration += Time.deltaTime;
				lerpObject.transform.position = Vector3.Lerp(start, end, duration / time);
				yield return new WaitForSeconds(Time.deltaTime);
			}
			StartCoroutine(lerpUp(1.0f));
			yield return null;
		}
		yield return null;


	}

	public IEnumerator lerpUp(float time)
	{


		Vector3 start = lerpObject.transform.position;
		Vector3 end = new Vector3 (start.x, start.y + 0.01f, start.z);
		float duration = 0.0f;
		while (duration < time) {
			duration += Time.deltaTime;
			lerpObject.transform.position = Vector3.Lerp (start, end, duration / time);
			yield return new WaitForSeconds (Time.deltaTime);
		}


		isAnimating = false;
		yield return null;

	}
}
