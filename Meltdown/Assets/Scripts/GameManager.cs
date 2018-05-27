using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class GameManager : Photon.PunBehaviour, IPunObservable
{
	

    public static GameManager Instance { get; private set; }

    public GameObject[] buttonObjectArray;
    public Sprite[] buttonSpritesArray;
    public Sprite[] buttonSpritesArrayRandomized;
    public ButtonDataArray buttonDataArray;
    public List<Sequence> SequenceList = new List<Sequence>();

	public SequenceManager SQM;
	public SpriteRenderer OfficeDisplay;
    public int currentSequence;
    //Sequnce we are currently on
    public int currentStep;
	//current step of the current sequence;
	public Text sequenceText;
    public GameObject greenFirework;
    public GameObject redFirework;
    //public TeleportPoint[] teleportPoints;
    //array of teleport points

    [SerializeField]
    private int MinimumPlayers = 2;

    public float RoundEndTime { get; set; }

	//Can't put a header above enum.
	enum Difficulty {Easy,Medium,Hard};

	[Header("Difficalty")]
	[SerializeField]
	private Difficulty difficulty;
	[Tooltip("0=Easy,1=Medium,2=Hard")]
	public float[] PossibleRoundTimes= new float[3];
	[Tooltip("0=Easy,1=Medium,2=Hard")]
	public float[] PossibleSequenceCompleteRewardTimes= new float[3];
	[Tooltip("0=Easy,1=Medium,2=Hard")]
	public float[] PossibleSequenceActionTimes= new float[3];


    [Header("Timer Info")]
    [Tooltip("Total amount of time Players have this level.")]
    public float startingRoundTime = 300.0f;
	[Tooltip("How long the timer will pause after getting a sequence right.")]
    public float sequenceCompleteReward = 15.0f;
	[Tooltip("How much time players have to compleate a sequence.")]
    public float sequenceActionTime = 10.0f;


	public float RoundTimeExtension { get { return sequenceCompleteReward - (SQM.currentSequence * sequenceActionTime); } }

    [Header("Controller Objects")]
    public GameObject leftController;
    public GameObject rightController;
    [Header("VRTK Controller Objects")]
    public GameObject VRTKLeftController;
    public GameObject VRTKRightController;
    public SequenceValueArray temp;
    //[HideInInspector]
    public SynchronizedSequenceArray synchronizedSequenceArray;


    public RotationZone[] RotationZones; 

	[HideInInspector]
	public InfoPanel infoPanel;

	private bool CountdownStarted { get; set; }
	private bool GameStarted { get; set; }
	private bool ReadyedUp { get; set; }
	private int ReadyUpCount { get; set; }


    public Animation BlastDoorAnimation; 

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(this.gameObject);
		}

		DontDestroyOnLoad(gameObject);

		this.infoPanel = this.GetComponent<InfoPanel>();
		this.infoPanel.AddLine("Log Active!");
	}


	void OnEnable()
	{
		PhotonNetwork.OnEventCall += this.CountDownEvent;
		PhotonNetwork.OnEventCall += this.ReadyUpEvent;
		PhotonNetwork.OnEventCall += this.ReadyDownEvent;

	}
	void OnDisable()
	{
		PhotonNetwork.OnEventCall -= this.CountDownEvent;
		PhotonNetwork.OnEventCall -= this.ReadyUpEvent;
		PhotonNetwork.OnEventCall -= this.ReadyDownEvent;
	}

	void Update()
	{
		// NOTE(barret): Input for readying up. for testing purposes
		if (Input.GetKeyDown(KeyCode.F1) && ReadyedUp == false)
		{
			//print("F1 pressed");
			ReadyUpRaise();
			ReadyedUp = true;
		}

		if (Input.GetKeyDown(KeyCode.F2) && ReadyedUp == true)
		{
			ReadyDownRaise();
			ReadyedUp = false;
		}
	}

	void StartCountdownRaise()
	{
		if (PhotonNetwork.room.PlayerCount >= this.MinimumPlayers && CountdownStarted == false)
		{
			print("Raising Countdown event");

			RaiseEventOptions Options = new RaiseEventOptions()
			{
				Receivers = ReceiverGroup.All
			};

			PhotonNetwork.RaiseEvent(0, new byte[] { 1, 2, 5, 10 }, true, Options);
		}
	}

	void ReadyUpRaise()
	{
		RaiseEventOptions Options = new RaiseEventOptions()
		{
			Receivers = ReceiverGroup.All
		};

		PhotonNetwork.RaiseEvent(1, new byte[] { 1, 2, 5, 10 }, true, Options);
	}

	void ReadyDownRaise()
	{
		RaiseEventOptions Options = new RaiseEventOptions()
		{
			Receivers = ReceiverGroup.All
		};

		PhotonNetwork.RaiseEvent(2, new byte[] { 1, 2, 5, 10 }, true, Options);
	}

	void ReadyUpEvent(byte eventcode, object content, int senderid)
	{
		if (eventcode == 1)
		{
			print("ReadyUpEvent");

			if (PhotonNetwork.isMasterClient)
			{
				ReadyUpCount += 1;
				infoPanel.AddLine("ReadyUpCount: " + ReadyUpCount);
				print("ReadyUpCount: " + ReadyUpCount);

				if (ReadyUpCount == this.MinimumPlayers)
				{
					print("Starting CountdownRaise");
					infoPanel.AddLine("Starting CountdownRaise");
					StartCountdownRaise();
				}
			}
		}
	}

	void ReadyDownEvent(byte eventcode, object content, int senderid)
	{
		if (eventcode == 2)
		{
			print("ReadyDownEvent");
			if (PhotonNetwork.isMasterClient)
			{
				ReadyUpCount -= 1;
				infoPanel.AddLine("ReadyDownCount: " + ReadyUpCount);
				print("ReadyDownCount: " + ReadyUpCount);
				if (ReadyUpCount < 0)
					ReadyUpCount = 0;
			}
		}
	}

	void CountDownEvent(byte eventcode, object content, int senderid)
	{
		if (eventcode == 0)
		{
			StartCoroutine("StartCountdown");
		}
	}

	public IEnumerator StartCountdown()
	{
		float currCountdownValue = 10;

		while (currCountdownValue > 0)
		{
			print("Match Begins in\n" + currCountdownValue.ToString());
			yield return new WaitForSeconds(1.0f);
			currCountdownValue--;
		}

		this.GameStarted = true;

        // TODO(barret): blast door down animation

        print("Get past loop");
        if (PhotonNetwork.isMasterClient)
		{
            print("Fire event");
			TeleportIntoScene();
		}
	}

	void TeleportIntoScene()
	{
		// TODO(barret): Need to teleport the players into the scene
		byte evCode = 4;
		byte[] content = new byte[] { 1, 2, 5, 10 };
		bool reliable = true;

		RaiseEventOptions Options = new RaiseEventOptions()
		{
			Receivers = ReceiverGroup.All
		};
		PhotonNetwork.RaiseEvent(evCode, content, reliable, Options);
        infoPanel.AddLine("Raised Teleport event");
        Debug.Log("Teleport");
	}

	public void NumberButtons()
	{
		for (int i = 0; i < buttonObjectArray.Length; i++)
		{
			buttonObjectArray[i].GetComponent<ButtonFunctionality>().buttonNumber = i;

		}
	}

	public void InitializeButtonDataArray()
	{
		for (int i = 0; i < buttonDataArray.buttonData.Length; i++)
		{
			buttonDataArray.buttonData[i].buttonNumber = i;
			buttonDataArray.buttonData[i].spriteNumber = i;
		}
	}

	public SynchronizedSequenceArray PopulateSequenceArray()
	{
		synchronizedSequenceArray.numberOfSequences = SequenceList.Count;

		for (int i = 0; i < SequenceList.Count; i++)
		{
			synchronizedSequenceArray.sequenceSteps.Add(SequenceList[i].List.Count);
		}

		for (int i = 0; i < SequenceList.Count; i++)
		{
			int[] placeHolder = new int[synchronizedSequenceArray.sequenceSteps[i]];

			temp.intArray = placeHolder;
			synchronizedSequenceArray.arrayList.Add(new SequenceValueArray());
			synchronizedSequenceArray.arrayList[i].intArray = placeHolder;

			for (int j = 0; j < SequenceList[i].List.Count; j++)
			{
				synchronizedSequenceArray.arrayList[i].intArray[j] = SequenceList[i].List[j].SequenceButtonValue;
			}
		}

		return synchronizedSequenceArray;
	}

	public void ImplementButtonDataArray()
	{
		for (int i = 0; i < buttonSpritesArray.Length; i++)
		{
			buttonSpritesArrayRandomized[i] = buttonSpritesArray[buttonDataArray.buttonData[i].spriteNumber];
		}
	}

	public void ImplementSequenceData()
	{
		for (int i = 0; i < synchronizedSequenceArray.numberOfSequences; i++)
		{
			SequenceList.Add(new Sequence());

			for (int j = 0; j < synchronizedSequenceArray.sequenceSteps[i]; j++)
			{
				SequenceList[i].List.Add(new SequenceButton());
				SequenceList[i].List[j].SequenceButtonValue = synchronizedSequenceArray.arrayList[i].intArray[j];
				SequenceList[i].List[j].SequenceButtonObject = buttonObjectArray[SequenceList[i].List[j].SequenceButtonValue];
				SequenceList[i].List[j].SequenceButtonSprite = buttonSpritesArrayRandomized[SequenceList[i].List[j].SequenceButtonValue];
			}
		}
	}

	public void GiveButtonsImages()
	{
		for (int i = 0; i < buttonObjectArray.Length; i++)
		{
			buttonObjectArray[i].GetComponent<ButtonFunctionality>().buttonNumber = i;
			buttonObjectArray[i].GetComponent<ButtonFunctionality>().buttonImage.sprite = buttonSpritesArrayRandomized[i];
		}
	}
	// Fisher-Yates shuffle sprites -
	public void ShuffleSprites()
	{
		int n = buttonSpritesArrayRandomized.Length;
		for (int i = 0; i < n; i++)
		{
			int r = i + (int)(UnityEngine.Random.Range(0.0f, 1.0f) * (n - i));
			Sprite t = buttonSpritesArrayRandomized[r];
			int intT = buttonDataArray.buttonData[r].spriteNumber;
			buttonDataArray.buttonData[r].spriteNumber = buttonDataArray.buttonData[i].spriteNumber;
			buttonSpritesArrayRandomized[r] = buttonSpritesArrayRandomized[i];
			buttonDataArray.buttonData[i].spriteNumber = intT;
			buttonSpritesArrayRandomized[i] = t;
		}
	}

	public void MasterServerCreateSecondSpriteArray()
	{
		for (int i = 0; i < buttonSpritesArrayRandomized.Length; i++)
		{
			buttonSpritesArrayRandomized[i] = buttonSpritesArray[i];
		}
	}

	public void ClientServerCreateSecondSpriteArray()
	{
		for (int i = 0; i < buttonSpritesArrayRandomized.Length; i++)
		{
			//Create from Serialized Data
		}
	}

	public string CreateJSONOfButtonDataArray(ButtonDataArray array)
	{
		string JSON = JsonUtility.ToJson(array);
		return JSON;
	}

	public string CreateJSONOfSequenceData(SynchronizedSequenceArray array)
	{
		string JSON = JsonUtility.ToJson(array);
		return JSON;
	}

	public void GenerateSequence()
	{
		for (int i = 0; i < SequenceList.Count; i++)
		{
			for (int j = 0; j < SequenceList[i].List.Count; j++)
			{
				int random = UnityEngine.Random.Range(0, buttonObjectArray.Length);
				SequenceList[i].List[j].SequenceButtonObject = buttonObjectArray[random];
				SequenceList[i].List[j].SequenceButtonValue = buttonObjectArray[random].GetComponent<ButtonFunctionality>().buttonNumber;
				SequenceList[i].List[j].SequenceButtonSprite = buttonSpritesArrayRandomized[random];
			}
		}
	}

	//Function to show the icons to the plant manager
	public IEnumerator ShowSequenceIcons()
	{
		while (true)
		{
			for (int i = 0; i < SequenceList[currentSequence].List.Count; i++)
			{
				sequenceText.text = (i + 1).ToString();
				//OfficeDisplay.sprite = SequenceList[currentSequence].List[i].SequenceButtonSprite;
				yield return new WaitForSeconds(3.0f);

			}
		}
	}

	//function to check if intputed buttons are correct sequence.
	public void CheckButtonInput(int button)
	{
		if (PhotonNetwork.isMasterClient)
		{
			if (button == SequenceList[currentSequence].List[currentStep].SequenceButtonValue)
			{
				//GameObject temp = Instantiate (greenFirework, position, Quaternion.identity) as GameObject;
				Debug.Log("Correct Button");
				if (currentStep < SequenceList[currentSequence].List.Count - 1)
				{ //if we arent on the last step of a sequence
					currentStep++;//next step
				}
				else if (currentStep == SequenceList[currentSequence].List.Count - 1)
				{
					currentStep = 0;
					currentSequence++;
					//Timer Test
					this.RoundEndTime += RoundTimeExtension;
				}

				photonView.RPC("updateCurrentSequenceInformation", PhotonTargets.All, currentSequence);
				photonView.RPC("updateCurrentStepInformation", PhotonTargets.All, currentStep);
			}
			else
			{
				//GameObject temp = Instantiate (redFirework, position, Quaternion.identity) as GameObject;
				currentStep = 0;
			}
		}
		else
		{
			photonView.RPC("SendButtonPress", PhotonNetwork.masterClient, button);
		}
	}
	public void SetDifficalty()
	{
		if (difficulty == Difficulty.Easy) 
		{
			startingRoundTime = PossibleRoundTimes [0];
			sequenceCompleteReward = PossibleSequenceCompleteRewardTimes [0];
			sequenceActionTime = PossibleSequenceActionTimes [0];
		}
		if (difficulty == Difficulty.Medium) 
		{
			startingRoundTime = PossibleRoundTimes [1];
			sequenceCompleteReward = PossibleSequenceCompleteRewardTimes [1];
			sequenceActionTime = PossibleSequenceActionTimes [1];
		}
		if (difficulty == Difficulty.Hard) 
		{
			startingRoundTime = PossibleRoundTimes [2];
			sequenceCompleteReward = PossibleSequenceCompleteRewardTimes [2];
			sequenceActionTime = PossibleSequenceActionTimes [2];
		}

	}

#if false
    //function to reset all teleport poitns to yellow after teleporting.
    public void resetTeleportPoint()
	{
		for (int i = 0; i < teleportPoints.Length; i++)
		{
			teleportPoints[i].Reset();
			teleportPoints[i].gameObject.SetActive(false);
		}
	}

	public void showTeleportPoints()
	{
		for (int i = 0; i < teleportPoints.Length; i++)
		{
			teleportPoints[i].gameObject.SetActive(true);
		}
	}
#endif 

	[PunRPC]
	void SendButtonPress(int buttonValue)
	{
		// Create a new player at the appropriate spawn spot
		CheckButtonInput(buttonValue);
	}

	[PunRPC]
	void updateCurrentSequenceInformation(int sentCurrentSequence)
	{
		// Create a new player at the appropriate spawn spot
		Instance.currentSequence = sentCurrentSequence;
	}

	[PunRPC]
	void updateCurrentStepInformation(int sentCurrentStep)
	{
		// Create a new player at the appropriate spawn spot
		Instance.currentStep = sentCurrentStep;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			stream.SendNext(this.RoundEndTime);
		}
		else
		{
			this.RoundEndTime = (float)stream.ReceiveNext();
		}
	}
}

[System.Serializable]
public class ButtonData
{
	public int buttonNumber;
	public int spriteNumber;
}

[System.Serializable]
public class ButtonDataArray
{
	public ButtonData[] buttonData;
}

[System.Serializable]
public class SequenceButton
{
	public GameObject SequenceButtonObject;
	public int SequenceButtonValue;
	public Sprite SequenceButtonSprite;
}

[System.Serializable]
public class Sequence
{
	public List<SequenceButton> List = new List<SequenceButton>();
}

[System.Serializable]
public class SynchronizedSequenceArray
{
	public int numberOfSequences;
	public List<int> sequenceSteps = new List<int>();
	public List<SequenceValueArray> arrayList = new List<SequenceValueArray>();
}

[System.Serializable]
public class SequenceValueArray
{
	public int[] intArray;
}
