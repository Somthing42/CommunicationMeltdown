using UnityEngine;

public class TestReturnScript : MonoBehaviour
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

}
