using System.Globalization;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Text))]
public class DisplayTimeRemaining : MonoBehaviour
{
	private Text timeText;

	private void Awake()
	{
		timeText = GetComponent<Text>();
	}

	private void OnEnable()
	{
		StartCoroutine("RunTimer");
	}

	private void OnDisable()
	{
		StopCoroutine("RunTimer");
	}

	private IEnumerator RunTimer()
	{
		while (true)
		{
			if (GameManager.Instance)
			{
				timeText.text = "Timer: " + Math.Round(((GameManager.Instance.RoundEndTime - PhotonNetwork.time)), 2).ToString(CultureInfo.CurrentCulture);
			}

			yield return null;
		}
	}
}
