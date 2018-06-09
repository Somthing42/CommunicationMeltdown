using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMenu : MonoBehaviour
{
    public PlayerTeleportHandler PTH;
    
    public void LeaveRoomButton()
    {
        Matchmaker.instance.LeaveRoom();
        PTH.TeleportToLobby();
    }

    public void ReadUpButton()
    {
        GameManager.Instance.ReadyUpToggle();
    }
}
