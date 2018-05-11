using UnityEngine;
using UnityEngine.UI;


public class EntryObject : MonoBehaviour
{
    public Text Name;
    public Text Size;

    public Button EntryButton;

    public int Index;

    public bool RoomListEntry;

    public void ButtonPressed()
    {
        if (RoomListEntry)
        {
            Matchmaker.instance.EntryObjectButton(Index);
        }
    }

}
