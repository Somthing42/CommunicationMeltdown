using UnityEngine;

public class DestroyAfterOneSecond : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Destroy (this.gameObject, 1.0f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
