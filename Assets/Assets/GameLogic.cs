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
	public List<GameObject> BoundingBoxListPlayer;
	public List<GameObject> BoundingBoxListTarget;

	public GameObject textCamera;
	public GameObject playerCamera;
	public GameObject targetObject;
	public GameObject playerObject;

	public int userId;//0~5
	public int userObject;//0,1
	public int userDevice;//0~4

	public int isTrainingSession;//0: formal session, 1:training session

	public string note;
	public int boundingBoxSpecified;//0: as usual,1: AO full, 2:AO simple, 3: AO simple with auxiliary, 4: MO, 5: null

	private double countdownTargetTime;
	private int isCountdown;
	private int countdownFailTimes;

	static public int index = 0;
	static public int startPoint;

	static public List<Vector3> TargetRotation;
	static public List<int> TargetRotationType;
	private recordManager recordManagerObject;
	private gamestate nowState;

	class recordManager{
		private int userId;//0~5
		private int userObject;//0,1
		//0: house
		//1: missle
		private int userDevice;//0~4
//			0.GlobeFish
//			1.3D ball
//			2.AO(tactile)
//			3.MO
//			4.fully Mapping

	//       @ super pilot
	//			0.GlobeFish
	//			1.3D ball
	//			2.AO(tactile)
	//			3.MO
	//			4.fully Mapping
		private string recordPath;
		private System.IO.StreamWriter sw;
		private double taskStartTime;
		private double countdownTime;
		private double overallDeviceTime;
		public recordManager(int userId, int userDevice, int userObject, string note, int isTrainingSession,int boundingBoxSpecified){



			recordPath = "Records/User_";
			recordPath += userId;
			recordPath += (isTrainingSession == 1)? "_training" : "";
			recordPath += ".txt";
			
			
			if (!File.Exists(recordPath))
			{
				File.CreateText(recordPath);
			}

			using (StreamWriter streamWriterObject = File.AppendText(recordPath))
			{
				streamWriterObject.WriteLine("[User:"+userId+",Device:"+userDevice+",Object:"+userObject+", BoundingBoxType:"+boundingBoxSpecified+", time:"+System.DateTime.Now+"] :: "+note);
				streamWriterObject.WriteLine();
			}
			taskStartTime = Time.time;
			overallDeviceTime = 0;
		}
		public void startTask(GameObject targetObject, GameObject playerObject){
			taskStartTime = Time.time;
			using (StreamWriter streamWriterObject = File.AppendText(recordPath))
			{

				streamWriterObject.WriteLine(TargetRotationType[index]+"");
				streamWriterObject.WriteLine(""+playerObject.transform.eulerAngles.x+","+playerObject.transform.eulerAngles.y+","+playerObject.transform.eulerAngles.z);
				streamWriterObject.WriteLine("->");
				streamWriterObject.WriteLine(""+targetObject.transform.eulerAngles.x+","+targetObject.transform.eulerAngles.y+","+targetObject.transform.eulerAngles.z);


				streamWriterObject.WriteLine("");
//				streamWriterObject.WriteLine("[Task:"+(index-startPoint)+", Type:"+TargetRotationType[index]+" -- Start ]");
			}
		}
		public void startCountdown(){
			countdownTime = Time.time;
			using (StreamWriter streamWriterObject = File.AppendText(recordPath))
			{
//				streamWriterObject.WriteLine("[Task:"+(index-startPoint)+", Type:"+TargetRotationType[index]+" -- Countdown start At] "+ (countdownTime - taskStartTime));
				streamWriterObject.WriteLine(""+(countdownTime - taskStartTime));
			}
		}
		public void endCountdown(countdownResult isSuccess){
			double currentTime = Time.time; //just overall time....
			double currentTaskTime = currentTime - taskStartTime;
			double elapsedTime = currentTime - countdownTime;
			using (StreamWriter streamWriterObject = File.AppendText(recordPath))
			{
				if(isSuccess == countdownResult.SUCCEED){
					streamWriterObject.WriteLine(elapsedTime + "," + currentTaskTime);
//					streamWriterObject.WriteLine("[Task:"+(index-startPoint)+", Type:"+TargetRotationType[index]+" -- Countdown End, Succeed ] "+elapsedTime + " / " + currentTaskTime);
				}else if(isSuccess == countdownResult.FAILED){
					streamWriterObject.WriteLine(elapsedTime + "," + currentTaskTime);
//					streamWriterObject.WriteLine("[Task:"+(index-startPoint)+", Type:"+TargetRotationType[index]+" -- Countdown End, Failed ] "+elapsedTime + " / " + currentTaskTime);
				}

			}
		}
		public void endTask(int isSuccess){
			double endTime = Time.time;
			double timeDifference = endTime - taskStartTime;
			overallDeviceTime += timeDifference;
			if(isSuccess == 1){
				using (StreamWriter streamWriterObject = File.AppendText(recordPath))
				{
					//				streamWriterObject.WriteLine("[Task:"+(index-startPoint)+", Type:"+TargetRotationType[index]+" -- End ] "+timeDifference);
					//								streamWriterObject.WriteLine();
					streamWriterObject.WriteLine();
					streamWriterObject.WriteLine(""+timeDifference);
					streamWriterObject.WriteLine();
					streamWriterObject.WriteLine();
					streamWriterObject.WriteLine();
				}
			}else{
				using (StreamWriter streamWriterObject = File.AppendText(recordPath))
				{
					streamWriterObject.WriteLine();
					streamWriterObject.WriteLine(""+timeDifference);
					streamWriterObject.WriteLine("Terminated by supervisor.");
					streamWriterObject.WriteLine();
					streamWriterObject.WriteLine();
				}
			}

			
		}
		public void endDeviceTask(){
			double endTime = Time.time;
			double timeDifference = endTime - taskStartTime;
			using (StreamWriter streamWriterObject = File.AppendText(recordPath))
			{
				streamWriterObject.WriteLine("[Device Ends At"+System.DateTime.Now+" -- Overall Times ] " + overallDeviceTime);
				streamWriterObject.WriteLine();
			}
			
		}
	}




	// Use this for initialization
	void Start () {
		setRandomDestination();

		startPoint = (userId * 120 + userDevice * 24 + userObject * 12) % 720;
		index = startPoint;
		textCamera.SetActive(false);
		textCamera.SetActive(true);

		
		recordManagerObject = new recordManager(userId,userDevice,userObject,note,isTrainingSession,boundingBoxSpecified);


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

		hintComponent.SetActive(false);

		VisibleTextList[0].SetActive(true);
		VisibleTextList[1].SetActive(true);
		VisibleTextList[2].SetActive(false);
		VisibleTextList[3].SetActive(false);

		SetListActive(BoundingBoxListPlayer,false);
		SetListActive(BoundingBoxListTarget,false);

		if(boundingBoxSpecified == 0){//as usual, AO full is used for everyone. You should modify this part later.
			if(userDevice==0){//!for super pilot user study only!
				BoundingBoxListPlayer[0].SetActive(true);
				BoundingBoxListTarget[0].SetActive(true);
			}else if(userDevice==1){
				BoundingBoxListPlayer[1].SetActive(true);
				BoundingBoxListTarget[1].SetActive(true);
			}else if(userDevice==2||userDevice==3||userDevice==4){
				BoundingBoxListPlayer[3].SetActive(true);
				BoundingBoxListTarget[3].SetActive(true);
			}
		}else{ //reassigned
			switch(boundingBoxSpecified){
			case 1:
			case 2:
			case 3:
			case 4:
				BoundingBoxListPlayer[boundingBoxSpecified+1].SetActive(true);
				BoundingBoxListTarget[boundingBoxSpecified+1].SetActive(true);
				break;
			case 5://null
				break;
			}
		}


	}

	// Update is called once per frame
	void Update () {

		if(Input.GetKeyDown(KeyCode.Space))
		{
		
			if(nowState == gamestate.Preface)
			{

				targetObject.transform.rotation = playerObject.transform.rotation;
				targetObject.transform.Rotate(Vector3.up, (float)TargetRotation[index].x);		
				targetObject.transform.Rotate(Vector3.right, (float)TargetRotation[index].y);
				targetObject.transform.Rotate(Vector3.forward, (float)TargetRotation[index].z); 

				index ++;

				textCamera.SetActive(false);
				isCountdown = 0;
				nowState = gamestate.Playing;
				recordManagerObject.startTask(targetObject,playerObject);
			}
			else if(nowState == gamestate.Wait)
			{

				targetObject.transform.rotation = playerObject.transform.rotation;
				targetObject.transform.Rotate(Vector3.up, (float)TargetRotation[index].x);		
				targetObject.transform.Rotate(Vector3.right, (float)TargetRotation[index].y);
				targetObject.transform.Rotate(Vector3.forward, (float)TargetRotation[index].z); 

				index ++;

				textCamera.SetActive(false);
				Debug.Log("playing");

				nowState = gamestate.Playing;
				recordManagerObject.startTask(targetObject,playerObject);
				countdownFailTimes = 0;

			}else if(nowState == gamestate.End)
			{
				Debug.Log("Ends");
				//doing nothing. Closed manually.
			}

		}else if(Input.GetKeyDown(KeyCode.Escape)){//test!
			if(nowState == gamestate.Playing)
			{

				recordManagerObject.endCountdown(countdownResult.SUCCEED);
				playerCamera.camera.backgroundColor = new Vector4(0.0f, 0.0f, 0.0f, 0);
				isCountdown = 0;
				hintComponent.renderer.material = hintList[0];
				hintComponent.SetActive(false);
				
				recordManagerObject.endTask(0);
				
				
				VisibleTextList[0].SetActive(false);
				VisibleTextList[1].SetActive(false);
				VisibleTextList[2].SetActive(false);
				VisibleTextList[3].SetActive(false);
				int overallTaskIndex = index - startPoint;
				
				if(overallTaskIndex >= 12){
					VisibleTextList[3].SetActive(true);
					textCamera.SetActive(true);
					nowState = gamestate.End;
					recordManagerObject.endDeviceTask();
				}else{
					VisibleTextList[2].SetActive(true);
					textCamera.SetActive(true);
					nowState = gamestate.Wait;
				}


//				hintComponent.renderer.material = hintList[0];
//				hintComponent.SetActive(false);
//				
//				recordManagerObject.endTask();
//				
//				
//				VisibleTextList[0].SetActive(false);
//				VisibleTextList[1].SetActive(false);
//				VisibleTextList[2].SetActive(false);
//				VisibleTextList[3].SetActive(false);
//				int overallTaskIndex = index - startPoint;
//				
//				if(overallTaskIndex >= 6){
//					VisibleTextList[3].SetActive(true);
//					textCamera.SetActive(true);
//					nowState = gamestate.End;
//					recordManagerObject.endDeviceTask();
//				}else{
//					VisibleTextList[2].SetActive(true);
//					textCamera.SetActive(true);
//					nowState = gamestate.Wait;
//				}



			}
		}


		//Diff between the target orientation and the player orientation.
		if(nowState == gamestate.Playing)
		{
			double differenceBoundary = 18;
			double difference = (targetObject.transform.eulerAngles.x - playerObject.transform.eulerAngles.x)
							*(targetObject.transform.eulerAngles.x - playerObject.transform.eulerAngles.x)
							+(targetObject.transform.eulerAngles.y - playerObject.transform.eulerAngles.y)
							*(targetObject.transform.eulerAngles.y - playerObject.transform.eulerAngles.y)
							+(targetObject.transform.eulerAngles.z - playerObject.transform.eulerAngles.z)
							*(targetObject.transform.eulerAngles.z - playerObject.transform.eulerAngles.z);

//			double difference = (targetObject.rotation.eulerAngles.x - playerObject.rotation.eulerAngles.x)
//					*(targetObject.rotation.eulerAngles.x - playerObject.rotation.eulerAngles.x)
//					+(targetObject.rotation.eulerAngles.y - playerObject.rotation.eulerAngles.y)
//					*(targetObject.rotation.eulerAngles.y - playerObject.rotation.eulerAngles.y)
//					+(targetObject.rotation.eulerAngles.z - playerObject.rotation.eulerAngles.z)
//					*(targetObject.rotation.eulerAngles.z - playerObject.rotation.eulerAngles.z);

			Debug.Log(playerObject.transform.eulerAngles.x+","+playerObject.transform.eulerAngles.y+","+playerObject.transform.eulerAngles.z);

			difference = Math.Pow(difference,0.5);
			if(countdownFailTimes >= 3){
				differenceBoundary = 30;
			}

//			if()

//			Quaternion targetRotation = targetObject.transform.rotation;
//			Quaternion playerRotation = playerObject.transform.rotation;
//
//			Quaternion relative = Quaternion.Inverse(targetRotation) * playerRotation;
//			double difference = Math.Pow((relative.eulerAngles.x * relative.eulerAngles.x+
//			                              relative.eulerAngles.y * relative.eulerAngles.y+
//			                              relative.eulerAngles.z * relative.eulerAngles.z)
//			                             ,0.5);

//			Debug.Log(relative);


			if ( difference <= differenceBoundary){
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
					}else{// pass!, next state


						recordManagerObject.endCountdown(countdownResult.SUCCEED);
						playerCamera.camera.backgroundColor = new Vector4(0.0f, 0.0f, 0.0f, 0);
						isCountdown = 0;
						hintComponent.renderer.material = hintList[0];
						hintComponent.SetActive(false);

						recordManagerObject.endTask(1);


						VisibleTextList[0].SetActive(false);
						VisibleTextList[1].SetActive(false);
						VisibleTextList[2].SetActive(false);
						VisibleTextList[3].SetActive(false);
						int overallTaskIndex = index - startPoint;

						if(overallTaskIndex >= 12){
							VisibleTextList[3].SetActive(true);
							textCamera.SetActive(true);
							nowState = gamestate.End;
							recordManagerObject.endDeviceTask();
						}else{
							VisibleTextList[2].SetActive(true);
							textCamera.SetActive(true);
							nowState = gamestate.Wait;
						}

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

					countdownFailTimes += 1;
					hintComponent.SetActive(false);
				}
			}
		}

	}

	
	private void setRandomDestination(){
		TargetRotation = new List<Vector3>();
		TargetRotationType = new List<int>();
		if(isTrainingSession == 2){//just for target data collection
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(120,60,-86));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(57,36,178));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-64,-106,103));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-90,-107,-131));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-80,-176,117));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-149,-125,-83));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-142,-7,92));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-59,-37,-158));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-104,29,-46));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-175,95,96));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-76,1,-2));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-40,-69,62));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-178,-67,24));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(161,-34,117));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(38,-75,147));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-56,-60,-169));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(88,145,-65));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(1,94,144));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-81,-125,121));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-162,132,95));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(149,174,52));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(164,109,148));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-124,40,-82));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-32,118,-28));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(169,-180,84));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-133,-88,119));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(87,-33,7));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-111,-98,43));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(7,96,129));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(84,-49,163));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(12,-91,-64));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-157,-83,42));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-134,49,-45));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-90,-115,84));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-48,92,80));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-179,107,-23));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(86,-74,50));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-168,-102,-22));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-168,-38,66));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-168,-54,-157));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-170,150,-161));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(134,-141,-134));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-25,-53,-121));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(177,124,-149));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(94,57,173));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-24,-113,141));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-2,-82,-101));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-99,59,85));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-98,131,32));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-156,32,44));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(27,-86,-92));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(54,-59,-53));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(117,-78,92));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-170,103,-56));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-140,-100,11));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-20,89,-124));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(108,-68,33));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(89,6,53));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(169,-17,-130));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-21,-49,-140));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(61,-39,75));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-99,-24,-178));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(45,106,-31));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-109,-90,152));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(76,-70,171));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-165,132,85));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(21,-141,-15));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(144,-176,-130));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(91,-62,11));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-176,-124,-117));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-36,47,137));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(49,47,-9));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(28,-180,-167));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(14,-150,-170));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(154,64,25));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-141,136,-32));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(137,143,-6));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-17,130,-42));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-147,112,-120));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-88,30,150));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-114,113,-149));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-95,53,151));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(74,1,57));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-86,128,152));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(39,-104,134));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(28,-5,127));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-35,-116,-19));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(31,125,1));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-39,-115,-96));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(93,48,158));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(138,85,107));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(29,91,-180));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-49,142,73));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-82,35,-89));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-27,-143,47));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-69,-122,-101));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-66,49,42));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-179,-45,103));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(105,-48,-47));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(134,-5,146));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(3,132,30));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-38,44,98));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(4,8,-96));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-2,136,141));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(124,9,45));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-27,0,-89));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-43,82,-42));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-5,-71,-108));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-162,-20,87));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(173,139,149));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-132,-131,29));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-46,161,51));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-38,56,-175));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-65,-32,1));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-158,-49,-89));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(128,-179,119));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-88,131,-68));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(75,-158,164));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-34,27,-75));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(54,63,-82));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-150,-19,28));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-116,96,77));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-161,-5,59));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(60,-59,108));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(77,49,-86));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-159,178,164));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(121,77,-44));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-170,-49,-23));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(172,-40,138));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(13,-63,24));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(171,-13,169));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(159,-51,51));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-44,-123,-41));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(121,-17,121));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(107,168,-43));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(31,-125,-33));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-79,-102,51));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(2,-47,-40));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-93,-142,-60));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(69,48,162));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(72,-91,-119));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(103,-59,96));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-95,123,98));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-84,-44,-145));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-117,-37,78));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-67,153,151));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(143,-9,31));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-48,-143,67));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(-60,-50,-59));
			TargetRotationType.Add(2);
			TargetRotation.Add(new Vector3(53,-62,-66));
		}else if(isTrainingSession ==1){

			for(int i = 0; i<60; i++){
				TargetRotationType.Add(3);//training
				TargetRotation.Add(new Vector3(180,0,0));
				TargetRotationType.Add(3);//training
				TargetRotation.Add(new Vector3(180,0,0));
				TargetRotationType.Add(3);//training
				TargetRotation.Add(new Vector3(0,180,0));
				TargetRotationType.Add(3);//training
				TargetRotation.Add(new Vector3(0,180,0));
				TargetRotationType.Add(3);//training
				TargetRotation.Add(new Vector3(0,0,180));
				TargetRotationType.Add(3);//training
				TargetRotation.Add(new Vector3(0,0,180));
				TargetRotationType.Add(3);//training
				TargetRotation.Add(new Vector3(180,0,0));
				TargetRotationType.Add(3);//training
				TargetRotation.Add(new Vector3(180,0,0));
				TargetRotationType.Add(3);//training
				TargetRotation.Add(new Vector3(0,180,0));
				TargetRotationType.Add(3);//training
				TargetRotation.Add(new Vector3(0,180,0));
				TargetRotationType.Add(3);//training
				TargetRotation.Add(new Vector3(0,0,180));
				TargetRotationType.Add(3);//training
				TargetRotation.Add(new Vector3(0,0,180));
			}
		}else{
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(121,-17,28));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-27,35,11));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(98,144,-9));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(28,31,-3));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(85,142,62));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-33,-45,74));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(127,-5,67));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(31,23,10));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(48,-5,-34));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(39,-39,-16));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-25,5,18));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-42,-100,29));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(21,44,-13));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(92,81,26));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-21,-11,55));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(50,151,-78));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(55,-22,-1));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(24,113,-97));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-4,-28,-2));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(79,19,0));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-17,47,18));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(37,-16,7));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(67,100,5));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(149,-43,36));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-34,-53,-56));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(69,151,-34));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-29,-95,107));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(24,-30,20));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(11,24,43));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-5,-30,24));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-41,-3,-41));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(30,-41,85));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(19,-131,108));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-5,-31,29));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(110,-85,45));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(42,-17,33));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-131,-110,-22));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(20,32,43));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(37,36,-2));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(148,1,52));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(61,9,-146));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(32,85,-148));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(40,13,42));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(21,-43,11));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(37,14,33));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-1,-149,34));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-14,21,38));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-8,34,173));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(23,36,22));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(40,-42,15));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-69,90,13));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(81,123,45));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-85,-34,-34));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(93,-129,11));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-29,-19,49));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(45,-21,8));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-2,12,42));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(55,-98,115));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-101,27,6));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(33,-2,-7));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(27,44,12));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(147,58,5));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-65,91,55));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(27,-1,-3));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(78,-138,-29));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-6,-19,-6));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(75,114,90));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(79,-46,130));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-67,109,57));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(1,44,20));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-1,-1,-35));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(26,8,-40));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(7,-56,-17));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-106,72,-38));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(17,7,26));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-96,-43,-106));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-29,42,17));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-54,0,13));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-47,7,5));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(34,-30,151));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-26,38,-2));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-2,-51,-84));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-84,-62,-48));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-62,124,-54));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-119,-4,72));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(104,111,44));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(114,-84,-49));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(2,-90,-70));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(16,-91,-57));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(4,0,-50));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-58,-5,-3));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-18,-49,-5));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(55,7,102));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(22,-22,-22));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(5,-42,0));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(29,47,-2));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-150,-84,-8));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(37,39,5));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(23,-71,15));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(6,-32,-49));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(8,-142,-37));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(80,-10,53));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(35,-18,37));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(19,16,-37));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(34,16,3));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-34,72,-76));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-132,-16,35));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(30,24,-45));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-27,-35,-33));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-56,27,-74));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-30,12,-51));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(176,-10,-19));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-121,-71,8));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-18,-46,103));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(13,-80,116));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(51,-14,-29));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(55,23,5));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-57,-11,-134));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(26,4,-21));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-37,-35,6));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(130,65,-57));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-21,36,44));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-130,-19,-76));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(8,-29,64));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(28,17,-38));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(44,-141,46));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(61,144,77));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(129,15,102));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-13,-2,-26));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(24,-23,-10));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-32,-48,17));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(41,-25,24));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-130,35,120));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-87,-98,-78));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-47,18,-31));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(25,-22,-17));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-56,-18,1));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(54,-39,-168));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-150,-22,-27));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(20,47,30));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-33,6,27));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(63,26,25));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-62,41,-102));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(43,33,16));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-25,-26,28));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-46,7,16));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-16,109,-19));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-28,104,92));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(8,4,-21));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-66,-57,85));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(31,38,-17));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(142,-33,9));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(21,-7,-18));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-111,123,42));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-12,47,20));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-91,114,58));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(28,-31,40));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-9,-49,11));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(127,29,-70));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-9,47,11));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(4,28,-19));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-133,-55,-59));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(41,5,-29));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(61,168,-15));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(61,33,53));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-44,152,-35));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-32,6,-36));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-58,101,113));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-8,50,11));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-10,-33,-10));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-4,10,-48));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-30,-9,-30));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-28,13,-13));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-29,159,-73));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-41,38,-24));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(110,-75,-107));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-9,-49,-140));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(136,-78,-59));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(69,-70,-42));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-27,-7,113));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-86,16,-18));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-4,18,-14));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(19,20,-7));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(4,36,-27));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-23,-26,-29));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(0,-20,29));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(30,-39,108));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(157,11,64));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-7,-41,-167));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(98,-83,-97));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-3,-42,1));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(107,-16,-4));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(73,34,-137));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(11,35,-7));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-16,24,-14));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-34,-1,-3));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(6,-91,151));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(27,42,17));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(43,-1,1));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(97,75,16));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-27,119,118));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-135,96,-53));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(36,105,75));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(38,0,-12));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(69,-135,-97));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(1,41,-39));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(37,-69,101));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-22,-38,25));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(52,-8,0));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(45,31,-22));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(10,-42,36));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-75,-146,30));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(63,-130,-12));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(30,37,-120));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-45,83,144));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(4,46,24));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-17,117,-3));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-20,-2,-36));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(76,-50,-3));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(9,-13,-32));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(3,165,43));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-29,29,-23));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(64,25,-81));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(39,-27,29));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(100,-86,60));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-15,50,-11));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(11,-22,-31));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(133,21,-119));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-4,26,37));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-14,-9,22));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(52,88,-108));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(39,95,-125));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-40,6,-39));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(7,-133,37));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(70,-65,-152));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-70,-84,115));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(86,62,-80));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(28,38,26));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(36,-23,38));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(12,37,44));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(4,-60,-87));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(40,31,27));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-19,-112,67));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(17,10,-28));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-28,30,-14));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(27,-21,-38));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-27,9,18));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(36,-58,-110));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(82,9,109));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-1,5,-28));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-54,-65,81));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-148,-76,15));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(13,-25,-41));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(28,-116,75));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-36,-37,-52));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-117,-42,-123));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(45,-15,-29));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(105,-14,-45));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-12,-53,-7));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-14,-23,-21));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-65,158,22));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-11,-48,-33));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-119,112,-75));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-53,13,18));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(26,21,8));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-61,-34,-62));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-40,-96,11));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-52,157,58));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-2,8,-53));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-32,29,-15));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(3,52,8));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-56,-70,100));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-7,-8,-55));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(134,35,13));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-46,-32,107));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-34,-23,-32));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(76,56,33));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-32,29,-161));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(94,-97,106));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-89,117,77));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(57,-20,-6));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-27,21,10));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-35,34,-28));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-31,-4,27));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-35,118,72));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(162,-32,-65));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-16,-14,-17));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-37,-41,-21));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(37,7,21));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(2,-34,42));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(23,-141,26));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-18,-4,-55));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-24,-15,-31));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-138,-107,8));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-23,9,-20));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(83,95,-22));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-84,17,58));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(7,-114,-43));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-64,164,22));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(7,-11,-52));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(69,95,31));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(41,-6,-17));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(19,1,21));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(123,-58,26));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-2,159,63));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(109,36,106));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(21,9,-3));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-39,27,12));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-25,6,10));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-72,143,-65));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(48,54,-105));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(27,-1,30));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(105,134,14));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(28,6,-42));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(102,86,59));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(63,-142,66));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(83,71,-118));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-11,7,46));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(47,-30,3));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-91,-14,89));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(3,32,-43));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(8,-158,35));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(15,-32,42));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(6,-3,-40));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-28,44,-3));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(38,79,-71));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(8,41,0));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(31,-27,34));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-22,7,-5));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(39,71,-72));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-14,135,16));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(73,-42,135));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-117,44,54));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-40,31,4));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(120,36,-117));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-13,-36,-36));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-46,-74,114));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(46,62,9));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-4,12,-41));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-79,-123,94));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(72,125,15));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(41,26,-2));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-5,19,9));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(13,-116,-75));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(4,-40,40));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(8,-29,-47));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-47,1,11));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(36,-27,-89));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(158,25,-56));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(7,-6,31));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-50,17,1));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(16,48,87));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-140,49,96));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-50,-8,16));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-52,-5,-9));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-92,128,-32));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-53,-157,-67));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(19,-25,26));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(9,-20,46));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-67,128,-32));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-28,78,-123));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(1,34,-36));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-90,54,-113));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-13,16,-46));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-16,44,-9));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-32,9,31));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-95,-16,7));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-7,90,37));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(108,143,0));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-22,-42,-16));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-87,60,-95));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(29,-26,-38));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-14,-34,44));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-133,38,95));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-3,-27,10));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-21,34,4));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(100,-49,-68));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-72,1,-156));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-17,-8,-52));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-6,-47,24));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-79,-127,54));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(44,25,-98));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(41,-29,18));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(104,-8,67));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-32,5,4));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-52,43,94));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(92,-8,84));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(78,-38,55));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-24,2,56));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(7,-22,-28));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-1,82,-103));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(8,-8,33));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(39,22,30));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-21,91,-30));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-33,-12,45));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-116,29,-2));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-94,-50,14));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(3,26,14));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-102,-12,14));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-33,-14,49));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(21,94,-86));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(7,-46,-30));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-148,6,87));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-43,-8,22));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(24,35,18));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-64,-55,4));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(25,99,9));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(13,-6,23));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-6,27,35));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(137,-68,7));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-102,8,-48));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(47,-96,-144));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(105,-35,-64));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-135,1,25));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-30,-19,-10));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(13,23,46));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(19,-33,13));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(34,-34,-3));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(52,44,155));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(51,-9,-29));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-88,108,-96));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-16,-7,-16));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(15,178,22));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(16,132,84));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-2,-19,-8));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(3,-35,3));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(17,-30,-45));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-53,115,-117));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(17,27,23));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(33,41,-24));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-4,-3,118));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(57,-110,-91));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-23,-32,43));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-68,14,28));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(40,164,14));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-16,-32,33));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(21,-11,21));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-37,23,-18));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(70,-71,-83));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(25,-4,-45));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(69,123,95));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(114,-105,-7));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(24,42,0));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-82,14,-4));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-76,-99,124));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-116,29,-53));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-30,-33,13));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(51,-18,-78));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-32,-103,-80));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(1,23,-8));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(7,0,-47));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(24,-14,-54));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-65,-40,135));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(48,118,-8));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-13,-36,-5));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(39,-18,16));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(11,-118,-12));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-100,-52,-65));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(19,-9,-47));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(23,49,8));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-12,21,-47));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-48,-3,10));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(83,-23,-111));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-73,-66,53));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-30,135,-2));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(1,8,52));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(54,-15,-19));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-135,81,-11));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(18,17,-24));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-51,33,-4));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(15,28,89));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-152,72,-36));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-54,93,-49));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-15,141,-105));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(47,-37,-5));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(40,-22,-9));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-55,-1,-115));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(67,75,-87));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(23,-51,-16));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-28,47,15));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-41,35,15));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(2,48,-30));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-28,-122,-44));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(80,89,16));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-26,33,10));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-46,146,48));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-14,-31,21));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-10,-47,-14));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(74,-110,13));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(21,43,-153));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-107,71,-62));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(19,-31,-7));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(3,-16,29));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(59,-126,-88));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(73,50,131));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-91,91,7));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(29,31,8));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(54,-164,8));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(48,30,21));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(6,-50,-30));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(105,26,-30));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(24,38,41));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-11,-15,11));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-70,-103,14));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(107,10,76));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(32,41,27));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(75,82,29));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-10,18,17));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(131,-24,8));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-32,-8,-4));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(18,-18,21));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-19,11,-33));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-84,-5,-16));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-11,-22,-118));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-2,55,22));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(40,-79,-93));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-17,-38,30));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(71,-42,75));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-88,-66,-31));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(48,57,-91));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-2,-5,-23));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-25,-37,18));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(60,3,5));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-20,133,-69));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-124,-77,67));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(1,-28,52));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(10,-167,-47));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(22,-33,-25));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-28,14,50));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-13,-22,-53));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(102,45,138));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(16,19,10));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(19,-36,-33));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(112,19,-110));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(47,-149,-55));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(15,127,-28));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(129,111,-29));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(40,-32,-5));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(56,-4,-5));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(99,-5,69));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(8,11,-19));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-34,6,-3));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-28,37,7));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-41,22,9));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-8,52,19));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-44,-8,-30));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(40,94,-41));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(130,84,-58));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(114,-42,24));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-36,70,71));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-8,-13,173));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-138,33,86));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(1,-6,148));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-12,-43,-41));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-87,-3,-21));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(11,1,39));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(56,-20,4));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-33,-93,-39));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-43,-9,-14));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-1,-23,-1));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(5,-32,11));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-24,80,49));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-105,60,-35));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-44,-56,121));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(83,-124,102));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-15,36,31));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-27,25,-13));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(13,14,19));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-57,-121,56));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-137,-58,-9));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(20,20,-51));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-24,23,46));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-44,-21,-32));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(58,-150,-40));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-93,113,-12));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-44,51,47));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-84,35,-96));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-52,-8,12));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-126,29,114));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-27,-49,19));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(24,27,-45));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-44,-16,-6));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(10,93,82));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(54,105,-79));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(62,61,65));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(40,13,32));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-14,-14,-99));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(14,41,-2));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-36,-8,8));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(132,77,-7));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(29,101,-121));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-107,38,-22));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-18,-55,-125));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(133,30,-101));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(27,1,4));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-59,-125,-91));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(48,-14,-27));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-26,37,-37));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-15,19,-27));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(3,26,-7));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-54,-105,-56));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-59,15,51));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-32,-32,-2));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(11,50,7));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(29,28,0));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(11,6,-23));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-6,9,-37));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(28,-49,74));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(39,-7,-75));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-22,11,45));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-34,13,103));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-94,49,112));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-76,86,-74));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(50,-22,-9));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(13,-5,-177));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-19,34,-137));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-16,65,-90));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-18,24,13));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(51,-21,-5));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-122,93,-78));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(25,-19,-23));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-11,-140,109));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-29,-18,32));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(43,-10,30));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-79,5,-127));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(28,9,-17));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-41,-90,119));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-22,-22,-33));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(32,-3,16));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(14,98,92));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(115,7,62));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(3,21,-56));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-4,56,23));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-19,-4,-12));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(118,70,25));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(3,118,-85));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-30,-6,-3));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(24,51,5));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(20,-31,-25));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-21,33,-45));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-69,-89,113));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-35,120,-119));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(8,30,17));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(23,-127,53));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-126,65,70));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(21,-16,12));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(14,77,126));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(131,-107,-26));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(12,-30,43));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-35,169,22));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-126,105,52));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-10,40,28));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-19,25,-20));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(50,-89,-14));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(37,25,111));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(147,54,35));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(7,-21,25));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-7,21,-49));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-118,43,-28));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(59,-8,-11));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-21,27,-42));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(122,18,-87));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(163,69,-18));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(37,-25,-20));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-3,57,-18));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-151,94,20));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(78,-16,-141));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(91,-48,-40));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-27,-14,-46));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-22,-16,-49));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-16,-145,3));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(13,-26,50));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-128,33,-14));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-3,-120,96));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(47,-109,-84));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(35,7,-14));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(3,35,1));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-13,2,-46));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-92,-67,45));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-117,40,107));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-113,-12,116));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(39,-9,-32));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(11,9,24));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-16,-16,-49));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(164,67,-6));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-74,-81,-73));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(123,-31,119));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-83,58,3));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-65,-8,-91));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-6,35,-31));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-11,-23,-7));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(150,-52,43));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-5,-24,53));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-4,-36,16));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-6,25,-31));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(5,52,-29));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(13,25,-48));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-66,92,-84));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-57,69,-146));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-2,43,-31));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(153,31,29));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-46,-28,-26));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-24,0,22));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(91,4,9));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-68,-108,12));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(26,34,-173));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(14,14,-4));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(5,33,20));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(0,10,58));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(49,76,-140));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-104,-48,20));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(53,43,-109));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-7,50,24));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(38,-10,16));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(71,-123,103));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(10,-57,3));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(40,15,-6));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-26,-82,-133));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(111,-99,-93));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(16,-42,13));
		}
	}



}