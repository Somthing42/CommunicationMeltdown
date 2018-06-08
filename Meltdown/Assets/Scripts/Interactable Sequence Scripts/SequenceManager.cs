using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SequenceManager :  Photon.MonoBehaviour {
    [Header("Sequence Information")]
	public List<Interactable> masterSequence;				//main sequence (contains all interactables)
	public Queue<Interactable> interactedObjects;			//interacted objects queue
	public List<Interactable> interactedObj;
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
	public float authDispDelay = 10.0f;
	[HideInInspector]
	public float authDuration = 0.0f;

    [Header("Authentication Button Info")]
	public GameObject lerpObject;							//authenticate button lerp
	public Transform returnPosition;
	public float lerpTimeUp = 1.0f;
    [HideInInspector]
    public bool isAnimating = false;						//is the object animated currently

	[Header("Timer")]
	public float timerStuff;

	[Header("Win Conditions")]
	public float gameCounter;
	public Sprite winSprite;

	public Interactable[] testSeq;

    // Use this for initialization
    void Start() {
        gameCounter = 0;                                                    //set counter to zero at start
        consoles = FindObjectsOfType<Console>();							//add all consoles to console array
        CreateSequence();													//run sequence creation function
		testSeq[0] = masterSequence[0];
		testSeq [1] = masterSequence [1];
		testSeq[2] = masterSequence[2];
		testSeq[3] = masterSequence[3];
		testSeq[4] = masterSequence[4];
        currentSequenceSize = sequenceSizes[currentSequence];				//set current sequence size to start of sequence
        interactedObjects = new Queue<Interactable>(currentSequenceSize);	//create new queue for interacted objects
        StartCoroutine("DisplaySequenceToRenderer");						//start display coroutine
    }

    // Update is called once per frame
    void Update() {
		if (authDuration < authDispDelay + 1) {
			authDuration += Time.deltaTime;
		}
		if (authDuration >= authDispDelay) {
			authText.text = "Plz Enter \nSequence:";
		}
		if (authDuration >= 2) {
			WinCase ();
		}
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

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			//stream.SendNext(this.OfficeDisplay);
			stream.SendNext(this.testSeq);
		}
		else
		{
			//this.OfficeDisplay = (SpriteRenderer)stream.ReceiveNext();
			this.testSeq = (Interactable)stream.ReceiveNext();
			masterSequence [0] = testSeq [0];
			masterSequence [1] = testSeq [1];
			masterSequence [2] = testSeq [2];
			masterSequence [3] = testSeq [3];
			masterSequence [4] = testSeq [4];
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
			interactedObj.Add(_interactable);
        }
        else             												//else
        {
            interactedObjects.Enqueue(_interactable);					//add the passed in object to the queue
			interactedObj.Add(_interactable);
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
				interactedObj.Clear ();
				if (currentSequence < sequenceSizes.Length - 1) {
					currentSequence++;
					currentSequenceSize = sequenceSizes [currentSequence];
				}
				Debug.Log ("Authenticated");
				authText.text = "Authenticated";
                gameCounter++;                                                      //increae counter for every successful sequence
				authDuration = 0.0f;
				return;

			} else {
				interactedObjects.Clear ();
				interactedObj.Clear ();
				Debug.Log ("Rejected");
				authText.text = "Rejected";
				authDuration = 0.0f;
			}
			//}
		} else {
			interactedObjects.Clear ();
			interactedObj.Clear ();
			Debug.Log ("Rejected");
			authText.text = "Rejected";
			authDuration = 0.0f;
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
			Debug.Log ("SeqLerpDown");
			isAnimating = true;
			Vector3 start = returnPosition.position;
			Vector3 end = new Vector3(start.x, start.y - 0.01f, start.z);
			float duration = 0.0f;
			while (duration < time)
			{
				duration += Time.deltaTime;
				lerpObject.transform.position = Vector3.Lerp(start, end, duration / time);
				yield return new WaitForSeconds(Time.deltaTime);
			}


			StartCoroutine(lerpUp(lerpTimeUp));
			yield return null;
		}
		yield return null;


	}

	public IEnumerator lerpUp(float time)
	{

		Debug.Log ("SeqLerpUp");
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

	public void WinCase() {
		if (gameCounter >= sequenceSizes.Length) {
			bool temp = true;
			if (temp) {
				StopCoroutine ("DisplaySequenceToRenderer");
				authText.text = "MELTDOWN \nAVERTED!";
				sequenceText.text = "GAME \nWIN!";
				OfficeDisplay.sprite = winSprite;
				//TODO: win game
				temp = false;
				authDuration = 20.0f;
			}
		}
	}
		
}