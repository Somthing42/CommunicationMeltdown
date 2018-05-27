using System.Globalization;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Text))]
public class DisplayTimer : MonoBehaviour
{

	[Header("Timer stuff")]
	public Text timeText;
	private float timerTime;
	//Traking the current sequence time(how longe player's taking.)
	private float trackTime=0;

	//Bool to run and stop the timer. Is what the authenticator will trigger when players do good.
	public bool goTimer=true;

	//Demo pourpuses
	public GameObject coolent;

	private FailEvents failEvent;

	void Start()
	{
		//Gamemaneger dosn't have a Start() and I Don't want to mess with it to much.
		GameManager.Instance.SetDifficalty();
		timerTime = GameManager.Instance.startingRoundTime;
		failEvent = GetComponent<FailEvents> ();
		coolent.transform.localPosition = new Vector3 (0, 0, 0);

	}

	// Update is called once per frame
	void Update () {
		//For future use(When players get a sequence right turn false and stop timer.)
		if (goTimer) 
		{
			OtherTimer ();
			//Timer();
		}
		if (!goTimer)
		{
			trackTime += Time.deltaTime;//Count up
			if (trackTime > GameManager.Instance.sequenceCompleteReward) 
			{
				trackTime = 0;
				goTimer = true;
			}
		}
	}



	/// <summary>
	/// Old demo time
	/// </summary>
	private void Timer()
	{
		//Insure the game manager exists.
		if (GameManager.Instance) {
			//Stuff to get Photon time synced
			//timerTime -= (float)PhotonNetwork.time;
			//Debug.Log(Environment.TickCount + "-" + Time.deltaTime);

			//Real timer stuff.
			timerTime-=Time.deltaTime;//Count down.
			string minutes=((int) timerTime/60).ToString();//Minutes formatting
			string seconds= (timerTime%60).ToString("f2");//seconds formatting.
			timeText.text = "Timer: " + minutes + ":" + seconds;
			trackTime += Time.deltaTime;//Count up

			if(trackTime>GameManager.Instance.sequenceActionTime)
			{
				
				DrainCoolent ();
				//Reset trackTime
				trackTime = 0;
			}

			//Testing
//			if (minutes != 9.ToString ()) {
//				Debug.Log (Environment.TickCount);
//			}
		}
	}




	//Code to drain consitntlly.
	void OtherTimer()
	{
		//Insure the game manager exists.
		if (GameManager.Instance) {
			//Real timer stuff.
			timerTime -= Time.deltaTime;//Count down.
			string minutes = ((int)timerTime / 60).ToString ();//Minutes formatting
			string seconds = (timerTime % 60).ToString ("f2");//seconds formatting.
			timeText.text = "Timer: " + minutes + ":" + seconds;
			trackTime += Time.deltaTime;//Count up(How much time has past.)
			DrainCoolent();

			if (timerTime <= 0) 
			{
				failEvent.TestFailedSequence ();
				Time.timeScale = 0;
			}
		}
	}

	private void DrainCoolent()
	{
		Vector3 drainspeed=new Vector3 (0, 4.3f/GameManager.Instance.startingRoundTime,0);
		coolent.transform.position -= drainspeed*Time.deltaTime;
	}




}
