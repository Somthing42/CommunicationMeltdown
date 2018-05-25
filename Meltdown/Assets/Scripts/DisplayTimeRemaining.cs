using System.Globalization;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Text))]
public class DisplayTimeRemaining : MonoBehaviour
{

	[Header("Timer stuff")]
	public Text timeText;
	private float timerTime;
	//Traking the current sequence time(how longe player's taking.)
	private float sequenceTime=0;
	//Bool to run and stop the timer. 
	private bool goTimer=true;

	//Demo pourpuses
	public GameObject coolent;

	private FailEvents failEvent;

	void Start()
	{
		timerTime = GameManager.Instance.startingRoundTime;
		failEvent = GetComponent<FailEvents> ();
	}

	// Update is called once per frame
	void Update () {
		//For future use(When players get a sequence right turn false and stop timer.)
		if (goTimer) 
		{
			//OtherTimer ();
			Timer();
		}
		if (!goTimer)
		{
			//Reset sequenceTime
			sequenceTime = 0;
		}
	}



	//To find at what rate coolent needs to drain to be consistent: 0.15-(X*Y)=-5.66
	//X=Speed coolent needs to drain, Y=Amount of time in level.
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
			sequenceTime += Time.deltaTime;//Count up

			if(sequenceTime>GameManager.Instance.sequenceActionTime)
			{
				
				DrainCoolent ();
				//Reset sequenceTime
				sequenceTime = 0;
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
			//Stuff to get Photon time synced
			//timerTime -= (float)PhotonNetwork.time;
			//Debug.Log(Environment.TickCount + "-" + Time.deltaTime);

			//Real timer stuff.
			timerTime -= Time.deltaTime;//Count down.
			string minutes = ((int)timerTime / 60).ToString ();//Minutes formatting
			string seconds = (timerTime % 60).ToString ("f2");//seconds formatting.
			timeText.text = "Timer: " + minutes + ":" + seconds;
			sequenceTime += Time.deltaTime;//Count up(How much time has past.)
			DrainCoolent();

			
		}
	}

	private void DrainCoolent()
	{
		Vector3 drainspeed=new Vector3 (0, GameManager.Instance.coolentDrainSpeed,0);
		coolent.transform.position -= drainspeed;
	}




}
