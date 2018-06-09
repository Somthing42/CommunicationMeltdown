using UnityEngine;

public class PlayerManager : Photon.PunBehaviour
{
    public GameObject LobbySpawn;

	public GameObject[] spawns;

	public GameObject[] breakroomspawns;

	[Tooltip("Reference to the player avatar prefab")]
	public GameObject playerAvatar;

	public delegate void OnCharacterInstantiated(GameObject character);

	public static event OnCharacterInstantiated CharacterInstantiated;

	public static PlayerManager instance;

    public GameObject CameraRig; 

	void Awake()
	{
		if (playerAvatar == null)
		{
			Debug.LogError("MyNetworkManager is missing a reference to the player avatar prefab!");
		}
		// spawns = GameObject.FindGameObjectsWithTag("Respawn");

		instance = this;
	}

#if false
	private void Start()
	{

        if (PhotonNetwork.isMasterClient)
		{
            
            NewPlayer(0);
			GameManager.Instance.infoPanel.AddLine("MasterClient NewPlayer");
		}
	}

#endif
#if false 
    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
	{
		if (PhotonNetwork.isMasterClient)
		{
			Debug.Log("New Player :" + (PhotonNetwork.otherPlayers.Length + 1));
			var idx = PhotonNetwork.otherPlayers.Length;
			photonView.RPC("NewPlayer", newPlayer, idx);
		}
	}
#endif 

	[PunRPC]
	public void NewPlayer(int idx)
	{

		//GameManager.Instance.infopanel.AddLine("NewPlayer gets called. idx: " + idx.ToString());
		print("NewPlayer gets called. idx: " + idx.ToString());
		// Create a new player at the appropriate spawn spot
		var trans = breakroomspawns[idx].transform;
		var player = PhotonNetwork.Instantiate(playerAvatar.name, trans.position, trans.rotation, 0);
		CameraRig.GetComponent<PlayerTeleportHandler>().PlayerIndex = idx;
		//master player is plant manager, disable teleport points
		if (idx == 0)
		{

			GameManager.Instance.leftController.GetComponent<ControllerScript>().enabled = false;
			GameManager.Instance.rightController.GetComponent<ControllerScript>().enabled = false;
		}
		else
		//they are a worker, disable groudn teleport
		{
			GameManager.Instance.VRTKLeftController.GetComponent<VRTK.VRTK_StraightPointerRenderer>().enabled = false;
			GameManager.Instance.VRTKLeftController.GetComponent<VRTK.VRTK_ControllerEvents>().enabled = false;
			GameManager.Instance.VRTKLeftController.GetComponent<VRTK.VRTK_Pointer>().enabled = false;
			GameManager.Instance.VRTKRightController.GetComponent<VRTK.VRTK_StraightPointerRenderer>().enabled = false;
			GameManager.Instance.VRTKRightController.GetComponent<VRTK.VRTK_ControllerEvents>().enabled = false;
			GameManager.Instance.VRTKRightController.GetComponent<VRTK.VRTK_Pointer>().enabled = false;
		}
		player.name = "Player " + (idx + 1);
	}


}
