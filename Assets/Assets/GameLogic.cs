using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class GameLogic : MonoBehaviour {
	
	public enum gamestate
	{
		Preface,
		Playing,
		Wait,
		End,
	}
	public enum countdownResult{
		FAILED,
		SUCCEED
	}


	public List<GameObject> HouseList;
	public List<GameObject> StickList;


	public void SetListActive(List<GameObject> targetObj,bool enable)
	{
		foreach(GameObject obj in targetObj)
		{
			obj.SetActive(enable);
		}

	}



	public List<Material> hintList;

	public GameObject hintComponent;

	//visible while playing
	public List<GameObject> VisibleContorllerList;
	public List<GameObject> VisibleTextList;
	public GameObject textCamera;
	public GameObject playerCamera;
	public GameObject targetObject;
	public GameObject playerObject;

	public int userId;//0~4
	public int userObject;//0,1
	public int userDevice;//0~4

	public string note;

	private double countdownTargetTime;
	private int isCountdown;

	static public int index = 0;
	static public int startPoint;

	static public List<Vector3> TargetRotation;
	static public List<int> TargetRotationType;
	private recordManager recordManagerObject;
	private gamestate nowState;

	class recordManager{
		private int userId;//0~4
		private int userObject;//0,1
		//0: house
		//1: missle
		private int userDevice;//0~4
//			1.GlobeFish
//			2.3D ball
//			3.AO(tactile)
//			4.MO
//			5.fully Mapping
//
		private string recordPath;
		private System.IO.StreamWriter sw;
		private double taskStartTime = Time.time;
		private double countdownTime;
		public recordManager(int userId, int userDevice, int userObject, string note){
			
			recordPath = "Records/User_";
			recordPath += userId;
			recordPath += ".txt";
			
			
			if (!File.Exists(recordPath))
			{
				File.CreateText(recordPath);
			}

			using (StreamWriter streamWriterObject = File.AppendText(recordPath))
			{
				streamWriterObject.WriteLine("[User:"+userId+",Device:"+userDevice+",Object:"+userObject+"] :: "+note);
				//				streamWriterObject.WriteLine(taskStartTime.ToString());
			}
		}
		public void startTask(){
			taskStartTime = Time.time;
			using (StreamWriter streamWriterObject = File.AppendText(recordPath))
			{
				streamWriterObject.WriteLine("[Task:"+(index-startPoint)+", Type:"+TargetRotationType[index]+" -- Start ]");
			}
		}
		public void startCountdown(){
			countdownTime = Time.time;
			using (StreamWriter streamWriterObject = File.AppendText(recordPath))
			{
				streamWriterObject.WriteLine("[Task:"+(index-startPoint)+", Type:"+TargetRotationType[index]+" -- Countdown start At] "+ (countdownTime - taskStartTime));
			}
		}
		public void endCountdown(countdownResult isSuccess){
			double currentTime = Time.time; //just overall time....
			double currentTaskTime = currentTime - taskStartTime;
			double elapsedTime = currentTime - countdownTime;
			using (StreamWriter streamWriterObject = File.AppendText(recordPath))
			{
				if(isSuccess == countdownResult.SUCCEED){
					streamWriterObject.WriteLine("[Task:"+(index-startPoint)+", Type:"+TargetRotationType[index]+" -- Countdown End, Succeed ] "+elapsedTime + " / " + (currentTaskTime - taskStartTime));
				}else if(isSuccess == countdownResult.FAILED){
					streamWriterObject.WriteLine("[Task:"+(index-startPoint)+", Type:"+TargetRotationType[index]+" -- Countdown End, Failed ] "+elapsedTime + " / " + (currentTaskTime - taskStartTime));
				}

			}
		}
		public void endTask(){
			double endTime = Time.time;
			double timeDifference = endTime - taskStartTime;
			using (StreamWriter streamWriterObject = File.AppendText(recordPath))
			{
				streamWriterObject.WriteLine("[Task:"+(index-startPoint)+", Type:"+TargetRotationType[index]+" -- End ] "+timeDifference);
								streamWriterObject.WriteLine();
			}
			
		}
	}




	// Use this for initialization
	void Start () {
		setRandomDestination();

		startPoint = (userId * 60 + userDevice * 12 + userObject * 6) % 300;
		index = startPoint;
		textCamera.SetActive(false);
		textCamera.SetActive(true);

		
		recordManagerObject = new recordManager(userId,userDevice,userObject,note);


		if(userObject == 0) //house
		{
			SetListActive(HouseList,true);
			SetListActive(StickList,false);
		}
		else if(userObject == 1)
		{
			SetListActive(HouseList,false);
			SetListActive(StickList,true);
		}

		VisibleTextList[0].SetActive(true);
		VisibleTextList[1].SetActive(true);
		VisibleTextList[2].SetActive(false);

	}

	// Update is called once per frame
	void Update () {

		if(Input.GetKeyDown(KeyCode.Space))
		{
		
			if(nowState == gamestate.Preface)
			{

				Debug.Log("playing");

				targetObject.transform.rotation = playerObject.transform.rotation;
				targetObject.transform.Rotate(Vector3.up, (float)TargetRotation[index].x);		
				targetObject.transform.Rotate(Vector3.right, (float)TargetRotation[index].y);
				targetObject.transform.Rotate(Vector3.forward, (float)TargetRotation[index].z); 

				index ++;

				textCamera.SetActive(false);
				VisibleTextList[0].SetActive(false);
				VisibleTextList[1].SetActive(false);
				VisibleTextList[2].SetActive(true);
				isCountdown = 0;
				nowState = gamestate.Playing;
				recordManagerObject.startTask();
			}
			else if(nowState == gamestate.Wait)
			{
//
//				if(index >= TargetRotation.Count)
//				{
//					nowState = gamestate.End;
//					Debug.Log("End");
//				}
//				else
//				{
//

				targetObject.transform.rotation = playerObject.transform.rotation;
				targetObject.transform.Rotate(Vector3.up, (float)TargetRotation[index].x);		
				targetObject.transform.Rotate(Vector3.right, (float)TargetRotation[index].y);
				targetObject.transform.Rotate(Vector3.forward, (float)TargetRotation[index].z); 

				index ++;

				textCamera.SetActive(false);
				Debug.Log("playing");

				nowState = gamestate.Playing;
				recordManagerObject.startTask();

			}

		}else if(Input.GetKeyDown(KeyCode.Escape)){
			if(nowState == gamestate.Playing)
			{
				textCamera.SetActive(true);
				nowState = gamestate.Wait;
				Debug.Log("wait");
				recordManagerObject.endTask();

			}
		}


		//Diff between the target orientation and the player orientation.
		if(nowState == gamestate.Playing)
		{
			double difference = (targetObject.transform.eulerAngles.x - playerObject.transform.eulerAngles.x)
							*(targetObject.transform.eulerAngles.x - playerObject.transform.eulerAngles.x)
							+(targetObject.transform.eulerAngles.y - playerObject.transform.eulerAngles.y)
							*(targetObject.transform.eulerAngles.y - playerObject.transform.eulerAngles.y)
							+(targetObject.transform.eulerAngles.z - playerObject.transform.eulerAngles.z)
							*(targetObject.transform.eulerAngles.z - playerObject.transform.eulerAngles.z);

			difference = Math.Pow(difference,0.5);
			if ( difference <= 18){
				double timeDifference = 0;
				Debug.Log("wait");
				if(isCountdown == 1){// in the countdown progress
					timeDifference = countdownTargetTime - Time.time ;
					if(timeDifference<1 && timeDifference>0.75){
						hintComponent.renderer.material = hintList[0];
					}else if(timeDifference<0.75 && timeDifference>0.5){
						hintComponent.renderer.material = hintList[1];
					}else if(timeDifference<0.5 && timeDifference>0.25 ){
						hintComponent.renderer.material = hintList[2];
					}else if(timeDifference<0.25 && timeDifference > 0){
						hintComponent.renderer.material = hintList[3];
					}else{// pass!
						recordManagerObject.endCountdown(countdownResult.SUCCEED);
						playerCamera.camera.backgroundColor = new Vector4(0.0f, 0.0f, 0.0f, 0);
						isCountdown = 0;
						hintComponent.renderer.material = hintList[0];
						hintComponent.SetActive(false);

						recordManagerObject.endTask();
						textCamera.SetActive(true);
						nowState = gamestate.Wait;
					}
				}else{// start countdown

					countdownTargetTime = Time.time + 1;

					recordManagerObject.startCountdown();

					playerCamera.camera.backgroundColor = new Vector4(0.15f, 0.15f, 0.15f, 0);
					isCountdown = 1;
					hintComponent.renderer.material = hintList[0];
					hintComponent.SetActive(true);

				}

			}else{
				if(isCountdown == 1){ // failed to end the countdown
					recordManagerObject.endCountdown(countdownResult.FAILED);
					playerCamera.camera.backgroundColor = new Vector4(0.0f, 0.0f, 0.0f, 0);
					isCountdown = 0;
					hintComponent.SetActive(false);
				}
			}
		}

	}

	
	private void setRandomDestination(){
		TargetRotation = new List<Vector3>();
		TargetRotationType = new List<int>();
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(5,-7,37));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(6,-162,-47));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-59,78,-3));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-24,-24,-49));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(42,17,27));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(127,-63,65));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(37,59,166));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-6,11,44));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-67,21,-126));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(49,8,-8));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-23,51,84));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(27,-14,-26));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-11,-6,-40));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(2,2,-30));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(60,-115,0));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(68,66,47));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-19,10,52));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-77,-5,-5));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-88,2,146));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(5,-59,-10));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-89,-73,-98));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-27,30,-36));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-16,-44,-30));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-15,32,12));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-15,151,3));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-25,10,24));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(42,26,7));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(126,-76,30));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(117,-108,19));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-113,-44,15));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-20,7,30));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(35,129,-39));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-18,-4,-160));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(10,-15,94));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-2,36,17));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-127,-40,-57));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(6,0,-52));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-31,-24,34));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(38,0,9));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(73,88,6));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(8,30,-19));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-130,69,-75));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-3,-37,4));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(37,-16,-27));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(54,22,-58));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-27,-35,-32));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(48,150,80));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(49,-141,28));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(108,-62,1));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-17,24,22));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-42,-70,-124));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-26,-42,149));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(37,-27,19));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-10,33,123));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(106,-126,61));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(42,1,101));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(20,-7,29));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-42,-29,-20));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-55,1,-24));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(46,34,1));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(16,43,13));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-15,-29,-46));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-112,-46,-124));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(2,32,-10));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(20,-39,-23));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(56,-57,56));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-20,20,26));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(0,-45,22));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-34,45,-6));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-104,-68,129));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(97,26,-15));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-11,56,9));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(34,-22,-43));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-102,96,89));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(81,-134,-21));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-8,129,-52));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(22,8,17));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-30,13,-26));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(95,-36,-110));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-27,22,-50));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(168,18,-47));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-32,-37,35));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(0,-55,14));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(16,39,-4));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(66,-36,-76));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-28,13,41));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-10,-28,39));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(89,95,58));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-88,-40,137));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-23,-72,160));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-78,-21,21));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(1,23,-34));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-32,22,-35));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-32,22,32));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-52,-23,84));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(97,-20,45));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(22,161,-78));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(17,-6,51));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(19,48,21));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(30,1,2));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-28,-14,-20));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(108,125,-44));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(30,-40,-104));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(9,20,-51));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(33,115,75));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(105,28,31));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(161,-60,12));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-84,10,-10));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-46,-70,-132));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-169,4,-37));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(8,17,38));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-17,-26,37));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(29,40,-23));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(92,-43,-92));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(2,-78,-66));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-2,169,47));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(14,-169,8));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-13,-41,17));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(67,-120,-4));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-16,18,-4));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-49,104,29));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-28,-16,-35));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-8,-23,52));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(42,161,-35));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-2,-1,47));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(41,-10,6));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(24,5,41));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(18,35,30));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-96,-54,-140));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(5,-23,-17));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-88,58,-2));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-17,-48,31));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-83,58,-61));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-5,5,108));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-90,-40,38));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(19,87,-59));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(37,-5,-16));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-59,-44,135));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(154,45,60));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(15,-109,-121));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-16,-26,-19));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-29,-9,-5));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-30,-99,46));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-43,-20,38));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-25,165,3));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(35,0,32));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(28,75,-34));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(51,23,-64));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-78,154,18));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(51,69,49));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-43,147,-37));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-129,116,4));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(23,-106,-133));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(3,121,-115));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-30,-21,-33));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(29,28,-24));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(20,-52,17));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-9,16,16));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-13,26,-25));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(59,93,-86));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(40,55,78));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(32,34,20));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(34,29,2));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(31,8,-11));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(106,-79,-51));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-118,-15,-132));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(31,-49,2));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(25,-34,19));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-31,-37,-13));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-58,-12,-4));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(33,-4,-18));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(129,80,32));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(82,-35,123));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-10,-22,5));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-75,111,-80));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(42,23,-8));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(122,93,15));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-35,18,-21));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-95,-29,73));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-36,25,27));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(10,-35,-78));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-89,-56,18));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-56,20,-4));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-95,-14,71));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-5,13,-23));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-94,141,-21));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-10,10,-29));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(10,-16,55));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-71,-54,131));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(114,41,-96));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-124,64,94));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(36,-113,-32));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(35,36,33));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-10,2,33));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-37,37,18));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(38,20,24));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(24,9,-45));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-37,-15,11));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(26,65,-163));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(41,124,-89));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(16,35,26));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-82,26,129));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-97,54,54));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(38,-60,-60));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(123,23,29));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(117,-85,7));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-165,74,0));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(110,-92,-10));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(138,-6,2));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(8,34,25));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-7,55,77));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(1,-124,105));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-15,-16,-48));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(18,-41,-13));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(55,-16,-4));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(30,-51,-51));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-100,-7,55));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(0,22,33));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(32,6,-9));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(15,-42,28));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-14,-1,-25));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-35,-13,-37));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-45,-27,-11));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-31,10,1));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-30,-126,20));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-25,40,-35));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-152,63,57));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(21,83,-129));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-27,49,15));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-156,-70,56));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-32,48,-16));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-39,-7,-6));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-66,-98,-63));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-4,4,-41));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-27,-28,-20));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(116,74,-15));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(141,78,18));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-16,-1,20));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-33,-21,15));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-105,-66,-37));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-71,7,-52));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(25,92,-98));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-42,31,21));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-72,-69,-63));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(6,36,34));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-162,-24,-30));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-71,85,59));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-97,-1,6));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-8,-24,16));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-21,51,26));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-35,-10,-41));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(18,28,44));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-34,15,38));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(164,-23,-17));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(63,-35,1));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(0,36,-13));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-39,6,-62));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-132,47,-14));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-58,10,14));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-26,-19,3));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(25,38,6));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(24,26,23));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-116,23,106));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-36,42,-25));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(12,46,-30));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-24,-49,4));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(39,-11,10));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(27,-35,4));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-45,32,18));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-51,-116,78));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(123,62,13));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(102,10,14));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(6,10,50));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-8,21,49));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-30,-32,-5));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(36,32,60));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-25,-109,-83));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(121,-36,33));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-25,47,-19));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-30,-7,13));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-7,39,166));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-29,139,76));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-11,17,155));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-74,11,45));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-26,49,9));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-2,157,85));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-103,-79,-11));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(73,46,-21));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(43,34,-21));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-23,14,-80));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-53,34,121));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(7,-39,-16));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-38,-23,-16));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-48,27,-7));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(13,36,19));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(8,-148,77));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(25,41,-127));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(-47,-70,-150));
		TargetRotationType.Add(1);
		TargetRotation.Add(new Vector3(142,-97,-49));
		TargetRotationType.Add(0);
		TargetRotation.Add(new Vector3(-33,-17,-36));
	}



}