using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Matchmaker : Photon.PunBehaviour
{
    string _gameVersion = "1";
    // Use this for initialization


    RoomInfo[] Rooms;

    public int PlayerCountToStartMatch = 2;

    public RoomList UIList;

    public PlayerRoom UIRoom;

    public GameObject CreateRoomButton;

    public GameObject GetRoomsButton;

    public GameObject LeaveRoomButton;

    public Text InfoPanelText;

    public static Matchmaker instance;

    public Text CountDownText;

    bool CountdownStarted = false;

    public GameObject RoomNameInputWindow;

    public string GameScene = "Game";

    public RectTransform MatchmakerTrans; 



    void Start()
    {
        instance = this;

        if (!PhotonNetwork.connected)
        {
            // #Critical
            // we don't join the lobby. There is no need to join a lobby to get the list of rooms.
            // NOTE(barret): Whats the point of the lobby then
            PhotonNetwork.autoJoinLobby = false;


            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.automaticallySyncScene = true;

            PhotonNetwork.ConnectUsingSettings(_gameVersion);
        }
        else
        {
            if (PhotonNetwork.inRoom)
            {
                InRoom();
            }

            if (!PhotonNetwork.insideLobby)
            {
                ConnectedToMaster();
            }
            else
            {
                InLobby();
            }
        }
    }

    void OnEnable()
    {
        PhotonNetwork.OnEventCall += this.CountDownEvent;

    }
    void OnDisable()
    {
        PhotonNetwork.OnEventCall -= this.CountDownEvent;
    }

    public void GetAvailableRooms()
    {
        Rooms = PhotonNetwork.GetRoomList();

        if (Rooms.Length > 0)
        {
            print("Rooms have stuff in them");
            AddLine("There are " + Rooms.Length.ToString() + " rooms online");
            UIList.DisplayList(Rooms);

        }
        else
        {
            print("No Rooms");
            AddLine("There are 0 rooms online");
        }
    }

    public void CreateRoomEntry(string RoomName)
    {
        
        // NOTE(barret): CreateRoom will place you in the room as soon as you create it
        if (PhotonNetwork.CreateRoom(RoomName, new RoomOptions() { MaxPlayers = 4 }, null))
        {

            AddLine("Room: " + RoomName + " created");
        }
        else
        {
            AddLine("failed to create room: " + RoomName);
        }
    }

    public void CreateRoomWindow()
    {
        RoomNameInputWindow.SetActive(true);
        
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    private void StartGame()
    {
        if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.LoadLevel(GameScene);
        }
    }

    void InRoom()
    {

        //PhotonNetwork.LoadLevel(GameScene);

        if (PhotonNetwork.isMasterClient)
        {
            PlayerManager PM = GetComponent<PlayerManager>();
            PM.NewPlayer(0);
            GameManager.Instance.infoPanel.AddLine("MasterClient NewPlayer");
        }
 
    }

    void ConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    void InLobby()
    {
        CreateRoomButton.SetActive(true);
        GetRoomsButton.SetActive(true);
        LeaveRoomButton.SetActive(false);
    }

#region Photon.PunBehaviour CallBacks


    public override void OnConnectedToMaster()
    {
 
        Debug.Log("Launcher: OnConnectedToMaster() was called by PUN");
        AddLine("Connected to Master");
        
        ConnectedToMaster();
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created a room and joined it");
        AddLine("Created Room and joined it");
    }

    public override void OnJoinedLobby()
    {
        InLobby();
        Debug.Log("Joined Lobby");
        AddLine("Joined Lobby");

    }

    public override void OnDisconnectedFromPhoton()
    {
        Debug.LogWarning("Launcher: OnDisconnectedFromPhoton() was called by PUN");
        AddLine("Disconnected From Photon");
    }

    public override void OnJoinedRoom()
    {

        AddLine("Joined Room");
        InRoom();
    }

    public override void OnLeftRoom()
    {
        AddLine("You left the room");
        UIList.gameObject.SetActive(true);
        UIRoom.gameObject.SetActive(false);
        CountdownStarted = false;
        StopCoroutine(CountdownCoroutine);
        CountDownText.gameObject.SetActive(false);

    }


    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        if (PhotonNetwork.isMasterClient)
        {
            Debug.Log("New Player :" + (PhotonNetwork.otherPlayers.Length + 1));
            var idx = PhotonNetwork.otherPlayers.Length;
            photonView.RPC("NewPlayer", newPlayer, idx);
        }
        //UIRoom.FillPlayerSlot(PhotonNetwork.playerList);

        AddLine("Player " + newPlayer.NickName + " connected");


    }



    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {

        UIRoom.FillPlayerSlot(PhotonNetwork.playerList);

        AddLine("Player " + otherPlayer.NickName + " disconnected");

        if (PhotonNetwork.room.PlayerCount < PlayerCountToStartMatch && CountdownStarted == true)
        {
            CountdownStarted = false;
            StopCoroutine(CountdownCoroutine);
            CountDownText.gameObject.SetActive(false);
            AddLine("Countdown Ended");
        }
    }



#endregion

    int LineCount = 0;
    public void AddLine(string Line)
    {
        string PanelString = InfoPanelText.text;
        InfoPanelText.text = PanelString + Line + "\n";
        ++LineCount;

        int Full = 32;

        if (LineCount > Full)
        {
            while (PanelString.Remove(0) != "\n")
            {

            }
            LineCount = 0;
        }

    }


    public void EntryObjectButton(int Index)
    {
            PhotonNetwork.JoinRoom(Rooms[Index].Name);
    }

    private IEnumerator CountdownCoroutine;
    void CountDownEvent(byte eventcode, object content, int senderid)
    {
        CountDownText.gameObject.SetActive(true);
        CountdownStarted = true;
        AddLine("Countdown Started");
        CountdownCoroutine = StartCountdown(5);
        StartCoroutine(CountdownCoroutine);
    }

    float currCountdownValue;
    public IEnumerator StartCountdown(float countdownValue = 10)
    {

        currCountdownValue = countdownValue;
        while (currCountdownValue > 0)
        {
            CountDownText.text = "Match Begins in\n" + currCountdownValue.ToString(); 
            yield return new WaitForSeconds(1.0f);
            currCountdownValue--;
        }

        StartGame();
    }

    string RoomNameStringCapture;

    public void RoomNameCaptureInputField(string Value)
    {
        
        RoomNameStringCapture = Value;
    }

    public void RoomNameEnter()
    {
        AddLine("Try Create Room Button");
        CreateRoomEntry(RoomNameStringCapture);

        //Destroy(this.gameObject);

    }
}
