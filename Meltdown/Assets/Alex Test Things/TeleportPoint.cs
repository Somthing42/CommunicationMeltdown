using UnityEngine;

public class TeleportPoint : MonoBehaviour {
	public GameObject teleportLocation;//Where teleporting here actually puts the player
	public Material notHighlighted;
	public Material highlighted;
	public MeshRenderer mr;

	// Use this for initialization
	void OnEnable () {
		mr = this.GetComponent<MeshRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public void HighlightGreen()
	{
		mr.material = highlighted;
	}

	public void Reset()
	{
		mr.material = notHighlighted;
	}
}
