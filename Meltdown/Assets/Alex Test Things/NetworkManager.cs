using UnityEngine;

public class NetworkManager : Photon.PunBehaviour
{
	public static string gameVersion = "2018.03.13";

	[Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
	public byte MaxPlayersPerRoom = 4;
	public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;
	float roundDuration;

	void Awake()
	{
        if (!PhotonNetwork.inRoom)
        {
            PhotonNetwork.autoJoinLobby = false;    // we join randomly. always. no need to join a lobby to get the list of rooms.
            PhotonNetwork.automaticallySyncScene = true;
            PhotonNetwork.logLevel = Loglevel;
            PhotonNetwork.ConnectUsingSettings(gameVersion);
        }
        roundDuration = GameManager.Instance.startingRoundTime;
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("Connected to master");

		Debug.Log("Joining random room...");
		PhotonNetwork.JoinRandomRoom();
	}

	public override void OnJoinedLobby()
	{
		Debug.Log("Joined lobby");

		Debug.Log("Joining random room...");
		PhotonNetwork.JoinRandomRoom();
	}

	public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
	{
		Debug.Log("Can't join random room!");

		Debug.Log("Creating room...");

		var newRoomOptions = new RoomOptions()
		{
			EmptyRoomTtl = 0,
			MaxPlayers = MaxPlayersPerRoom,
			CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() {
				{ "RoundDuration", roundDuration }
			}
		};

		PhotonNetwork.CreateRoom(null, newRoomOptions, null);
	}

	public override void OnCreatedRoom()
	{
		Debug.Log("Created room");
	}

	public override void OnJoinedRoom()
	{
		Debug.Log("Joined room");
		if (PhotonNetwork.isMasterClient)
		{
			GameManager.Instance.InitializeButtonDataArray();
			GameManager.Instance.MasterServerCreateSecondSpriteArray();
			GameManager.Instance.ShuffleSprites();
			GameManager.Instance.GiveButtonsImages();
			GameManager.Instance.CreateJSONOfButtonDataArray(GameManager.Instance.buttonDataArray);

			Debug.Log("Got Em");
			GameManager.Instance.NumberButtons();
			GameManager.Instance.GenerateSequence();

			GameManager.Instance.PopulateSequenceArray();
			GameManager.Instance.CreateJSONOfSequenceData(GameManager.Instance.synchronizedSequenceArray);
			//GameManager.Instance.StartCoroutine(GameManager.Instance.ShowSequenceIcons());

		}

		object duration;

		if (PhotonNetwork.room.CustomProperties.TryGetValue("RoundDuration", out duration))
		{
			var timer = (float)PhotonNetwork.time + (float)duration;

			GameManager.Instance.RoundEndTime = timer;
		}
	}

	public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		Debug.Log("Player connected");

		if (PhotonNetwork.isMasterClient)
		{
				photonView.RPC("UpdateGameManager", newPlayer, GameManager.Instance.CreateJSONOfButtonDataArray(GameManager.Instance.buttonDataArray));
				photonView.RPC("UpdateSequence", newPlayer, GameManager.Instance.CreateJSONOfSequenceData(GameManager.Instance.synchronizedSequenceArray));
				GameManager.Instance.photonView.RPC("updateCurrentSequenceInformation", newPlayer, GameManager.Instance.currentSequence);
				GameManager.Instance.photonView.RPC("updateCurrentStepInformation", newPlayer, GameManager.Instance.currentStep);
				Debug.Log("RCP Call to synch GameManager");
		}
	}

	public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
	{
		Debug.Log("Player disconnected");
	}

	public override void OnFailedToConnectToPhoton(DisconnectCause cause)
	{
		Debug.Log("Couldn't connect to Photon network");
	}

	public override void OnConnectionFail(DisconnectCause cause)
	{
		Debug.Log("Connection failed to the Photon network");
	}

	public override void OnDisconnectedFromPhoton()
	{
		Debug.Log("We got disconnected form the Photon network");
	}

	//Attempts to byte serialize
	// Convert an object to a byte array

	[PunRPC]
	void UpdateGameManager(string json)
	{
		// Create a new player at the appropriate spawn spot
		GameManager.Instance.InitializeButtonDataArray();
		GameManager.Instance.buttonDataArray = JsonUtility.FromJson<ButtonDataArray>(json);
		GameManager.Instance.ImplementButtonDataArray();
		GameManager.Instance.GiveButtonsImages();
	}

	[PunRPC]
	void UpdateSequence(string json)
	{
		// Create a new player at the appropriate spawn spot


		//GameManager.instance.ImplementButtonDataArray ();
		//GameManager.instance.GiveButtonsImages ();
		GameManager.Instance.CreateJSONOfButtonDataArray(GameManager.Instance.buttonDataArray);

		GameManager.Instance.NumberButtons();
		GameManager.Instance.GenerateSequence();

		GameManager.Instance.PopulateSequenceArray();
		GameManager.Instance.CreateJSONOfSequenceData(GameManager.Instance.synchronizedSequenceArray);

		GameManager.Instance.synchronizedSequenceArray = JsonUtility.FromJson<SynchronizedSequenceArray>(json);
		GameManager.Instance.ImplementSequenceData();
		GameManager.Instance.StartCoroutine(GameManager.Instance.ShowSequenceIcons());
	}
}
