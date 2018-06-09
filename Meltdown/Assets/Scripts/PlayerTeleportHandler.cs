﻿using UnityEngine;
using System.Collections;

public class PlayerTeleportHandler : MonoBehaviour
{

    public int PlayerIndex;
    PhotonView photonView;


    // Use this for initialization
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }


    void OnEnable()
    {
        print("OnEnable PlayerTeleportHandler");
        PhotonNetwork.OnEventCall += this.PlayerTeleport;

    }
    void OnDisable()
    {
        PhotonNetwork.OnEventCall -= this.PlayerTeleport;
    }



    void PlayerTeleport(byte eventcode, object content, int senderid)
    {
        // print("event registered");
        if (eventcode == 4)
        {
            //print("eventcode == 4");

            //print("(photonView.isMine)");
            //GameManager.Instance.infoPanel.AddLine("Teleporting to " + PlayerManager.instance.spawns[PlayerIndex].transform.position.ToString() + " with Index " + PlayerIndex);
            //print("Teleport Fired");
            VRTK.VRTK_HeightAdjustTeleport TS = GetComponent<VRTK.VRTK_HeightAdjustTeleport>();

            TS.Teleport(PlayerManager.instance.spawns[PlayerIndex].transform, PlayerManager.instance.spawns[PlayerIndex].transform.position, transform.rotation);
            //GameManager.Instance.infoPanel.AddLine("New Position is " + transform.position.ToString());

        }

    }

    public void TeleportToLobby()
    {
        //Vector3 Location = new Vector3(1.339f, 3.224f, 10.747f);
        VRTK.VRTK_HeightAdjustTeleport TS = GetComponent<VRTK.VRTK_HeightAdjustTeleport>();
        TS.Teleport(PlayerManager.instance.LobbySpawn.transform, PlayerManager.instance.LobbySpawn.transform.position, transform.rotation);
    }

    public bool CanTeleport = true;
    public IEnumerator TeleportDelay(float SecondsOfDelay)
    {
        CanTeleport = false; 
        float Seconds = 0;
        while(Seconds < SecondsOfDelay)
        { 
            yield return new WaitForSeconds(1.0f);
            Seconds++;
        }

        CanTeleport = true; 
    }
}
