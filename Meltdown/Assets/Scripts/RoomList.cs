using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomList : MonoBehaviour
{
    public GameObject ListEntryPrefab;

    int EntryWidth = 32;

    List<GameObject> RL;

    List<Button> ListButtons;
	// Use this for initialization
	void Awake()
    {
        RL = new List<GameObject>();


        ListButtons = new List<Button>();
    }

    public void DisplayList(RoomInfo[] Rooms)
    {
        foreach (GameObject rl in RL)
        {
            GameObject.Destroy(rl);
        }
        RL.Clear();
        ListButtons.Clear();

        int currentposition = 32;
        int Index = 0;
        foreach (RoomInfo Info in Rooms)
        {
            GameObject entry = Instantiate(ListEntryPrefab, new Vector3(0, currentposition, 0), Quaternion.identity, this.transform);
            entry.transform.position = transform.position;
            entry.transform.rotation = transform.rotation;
            EntryObject entryatt = entry.GetComponent<EntryObject>();

            Text NameText = entryatt.Name;
            Text SizeText = entryatt.Size;
            entryatt.RoomListEntry = true;

            NameText.text = Info.Name;
            SizeText.text = Info.PlayerCount.ToString() + "/" + Info.MaxPlayers.ToString();

            RL.Add(entry);

            currentposition += (EntryWidth * 2);

            ListButtons.Add(entry.GetComponentInChildren<Button>());

            entryatt.Index = Index++;
        }
    }
}
