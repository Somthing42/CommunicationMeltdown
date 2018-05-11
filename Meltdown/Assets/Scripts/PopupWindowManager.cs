using UnityEngine;

public class PopupWindowManager : MonoBehaviour
{
    string StringCapture;

    public void CaptureInputField(string Value)
    {
        StringCapture = Value;
    }

    public void ButtonPress()
    {
        Matchmaker.instance.CreateRoomEntry(StringCapture);

        Destroy(this.gameObject);
    }
}
