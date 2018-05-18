using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SequenceManager : MonoBehaviour {
    [Header("Sequence Information")]
    public List<Interactable> masterSequence;
    public Queue<Interactable> interactedObjects;
    public int[] sequenceSizes;
    [HideInInspector]
    public int currentSequence = 0;
    [HideInInspector]
    public int currentSequenceSize;
    [HideInInspector]
    public Console[] consoles;

    [Header("Display Information")]
    public SpriteRenderer OfficeDisplay;
    public Text sequenceText;
    public float stepTransitionSpeed = 3.0f;

    [Header("Authentication Button Info")]
    public GameObject lerpObject;
    [HideInInspector]
    public bool isAnimating = false;



    // Use this for initialization
    void Start() {

        consoles = FindObjectsOfType<Console>();
        CreateSequence();
        currentSequenceSize = sequenceSizes[currentSequence];
        interactedObjects = new Queue<Interactable>(currentSequenceSize);
        StartCoroutine("DisplaySequenceToRenderer");
    }

    // Update is called once per frame
    void Update() {
    }

    public void CreateSequence()
    {
        foreach (Console console in consoles)
        {
            foreach (Interactable interactable in console.interactables)
            {
                masterSequence.Add(interactable);
                interactable.ObjectUsed += AddUsedObjectToList;
            }

        }

        ShuffleSequence();
    }

    void ShuffleSequence()
    {
        for (int count = 0; count < masterSequence.Count; count++)
        {
            int randomValue = UnityEngine.Random.Range(count, masterSequence.Count);
            Interactable holder = masterSequence[randomValue];
            masterSequence[randomValue] = masterSequence[count];
            masterSequence[count] = holder;
        }
    }

    // NOTE(barret): This needs to be tested. I don't know how reliable this is. 
    /* I'm not exactly sure how Photon does sycronization, but there might be a 
     * problem with desyced packets. If two players use an interactable at around 
     * the same time and send their new list to the master, which message does the 
     * master use to update first. 
    */
    void AddUsedObjectToList(Interactable _interactable)
    {

        if (interactedObjects.Count + 1 > currentSequenceSize)
        {

            interactedObjects.Dequeue();
            interactedObjects.Enqueue(_interactable);
        }
        else
        {
            interactedObjects.Enqueue(_interactable);
        }

        if (!PhotonNetwork.isMasterClient)
        {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("UpdateQueue", PhotonTargets.MasterClient, interactedObjects);
        }
        else
        {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("UpdateQueue", PhotonTargets.Others, interactedObjects);
        }
    }

    [PunRPC]
    void UpdateQueue(Queue<Interactable>  Sequence)
    {
        interactedObjects = Sequence;

        if (PhotonNetwork.isMasterClient)
        {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("UpdateQueue", PhotonTargets.Others, interactedObjects);
        }
    }

	public void Authenticate()
	{
		bool authenticated = false;

		if (interactedObjects.Count == currentSequenceSize) {

			if (Input.GetKeyDown (KeyCode.Space)) {
				Interactable[] listToCheck = interactedObjects.ToArray ();
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


				} else {
					interactedObjects.Clear ();
					Debug.Log ("Rejected");
				}
			}
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
