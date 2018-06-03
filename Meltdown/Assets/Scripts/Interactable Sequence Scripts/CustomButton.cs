using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CustomButton : Interactable {
	[HideInInspector]
	public bool isAnimating = false;
	public GameObject lerpObject;
	public Transform returnPosition;

	[Header("Lerp Timer")]
	public float lerpTimeUp = 1.0f;

	void OnCollisionEnter(Collision collision)
	{
	}

	public IEnumerator lerpDown(float time)
	{
		if (isAnimating == false)
		{
			Debug.Log ("ButtonLerpDown");
			isAnimating = true;

			Vector3 start = returnPosition.position;
			Vector3 end = new Vector3(start.x, start.y - 0.01f, start.z);
			float duration = 0.0f;
			while (duration < time)
			{
				duration += Time.deltaTime;
				lerpObject.transform.position = Vector3.Lerp(start, end, duration / time);
				yield return new WaitForSeconds(Time.deltaTime);
			}

			StartCoroutine(lerpUp(lerpTimeUp));
			yield return null;
		}
		yield return null;


	}

	public IEnumerator lerpUp(float time)
	{
		Debug.Log ("ButtonLerpUP");

		Vector3 start = lerpObject.transform.position;

		Vector3 end = new Vector3(start.x, start.y + 0.01f, start.z);
		float duration = 0.0f;
		while (duration < time)
		{
			duration += Time.deltaTime;
			lerpObject.transform.position = Vector3.Lerp(start, end, duration / time);
			yield return new WaitForSeconds(Time.deltaTime);
		}


		isAnimating = false;
		yield return null;
	}

	public override void OnObjectUsed()
	{
		base.OnObjectUsed();
	}
}
