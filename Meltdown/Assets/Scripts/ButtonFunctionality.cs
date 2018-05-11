using System.Collections;
using UnityEngine;
public class ButtonFunctionality : MonoBehaviour {
	public SpriteRenderer buttonImage;
	public int buttonNumber;
	public GameObject lerpObject;
	public bool isAnimating = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.GetComponent<SteamVR_TrackedObject> () != null) {
			Debug.Log (buttonNumber);
		}
	}




	public IEnumerator lerpDown(float time)
	{
		if (isAnimating == false) 
		{
			isAnimating = true;
			Vector3 start = this.gameObject.transform.position;
			Vector3 end = new Vector3 (start.x,start.y -0.01f, start.z);
			float duration = 0.0f;
			while (duration < time) 
			{
				duration += Time.deltaTime;
				lerpObject.transform.position = Vector3.Lerp (start, end, duration / time);
				yield return new WaitForSeconds (Time.deltaTime);
			}
			StartCoroutine (lerpUp (1.0f));
			yield return null;
		}
		yield return null;


	}

	public IEnumerator lerpUp(float time)
	{

			
			Vector3 start = lerpObject.transform.position;
			Vector3 end = new Vector3 (start.x,start.y +0.01f, start.z);
			float duration = 0.0f;
			while (duration < time) 
			{
				duration += Time.deltaTime;
				lerpObject.transform.position = Vector3.Lerp (start, end, duration / time);
			yield return new WaitForSeconds (Time.deltaTime);
			}


		isAnimating = false;
		yield return null;
	}


}
