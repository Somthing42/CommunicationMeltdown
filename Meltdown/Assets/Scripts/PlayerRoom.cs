using UnityEngine;
using UnityEngine.UI;

public class PlayerRoom : MonoBehaviour
{
    public EntryObject[] PlayerSlots;


    public void FillPlayerSlot(PhotonPlayer[] Players)
    {
        
        for(int Index = 0;
            Index < PlayerSlots.Length;
            ++Index)
        {
            Text NameText = PlayerSlots[Index].Name;
            if (Index < Players.Length)
            {
                NameText.text = Players[Index].NickName;
            }
            else
            {
                
                NameText.text = null;
            }
            
         }
    }


}
