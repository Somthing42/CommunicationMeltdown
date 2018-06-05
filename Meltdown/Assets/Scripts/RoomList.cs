using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomList : MonoBehaviour
{

    public GameObject[] ListButtons;

    public void DisplayList(RoomInfo[] Rooms)
    {
        foreach (GameObject entry in ListButtons)
        {
            entry.SetActive(false);
        }
      
        for (int Index = 0;
            Index < Rooms.Length;
            ++Index)
        {
            RoomInfo Info = Rooms[Index];
            GameObject entry = ListButtons[Index];
            //entry.transform.position = transform.position;
            //entry.transform.rotation = transform.rotation;
            entry.SetActive(true);
            
            EntryObject entryatt = entry.GetComponent<EntryObject>();

            Text NameText = entryatt.Name;
            Text SizeText = entryatt.Size;
            entryatt.RoomListEntry = true;

            NameText.text = Info.Name;
            SizeText.text = Info.PlayerCount.ToString() + "/" + Info.MaxPlayers.ToString();

            
        }
    }
}
