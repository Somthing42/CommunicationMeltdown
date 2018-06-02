using UnityEngine;

public class TestReturnScript : Photon.PunBehaviour
{
    public void ButtonPressed()
    {
        if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.LoadLevel(0);
        }
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel(0);
    }

}
