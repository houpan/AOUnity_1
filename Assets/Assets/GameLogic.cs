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
	public GameObject evaluateCamera;
	public GameObject playerCamera;
	public GameObject targetObject;
	public GameObject playerObject;

	public int userId;//0~5
	public int userObject;//0,1
	public int userDevice;//0~4

	public int isTrainingSession;//0: formal session, 1:training session

	public string note;
	public int boundingBoxSpecified;//0: as usual,1: AO full, 2:AO simple, 3: AO simple with auxiliary, 4: MO, 5: null

	private double evaluateTextAppearTargetTime;
	private int isEvaluateTextAppear;
	private int countdownFailTimes;

	static public int index = 0;
	static public int startPoint;

	static public List<Vector3> TargetRotation;
	static public List<int> TargetRotationType;
	private recordManager recordManagerObject;
	private gamestate nowState;


	class recordManager{
		private int _userId;//0~5
		private int _userObject;//0,1
		//0: house
		//1: missle
		private int _userDevice;//0~4
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
		private double cumulativeError;
		public recordManager(int userId, int userDevice, int userObject, string note, int isTrainingSession,int boundingBoxSpecified){

			this._userId = userId;
			this._userObject = userObject;
			this._userDevice = userDevice;


			recordPath = "Records/User_";
			recordPath += userId;
			recordPath += (isTrainingSession == 1)? "_training" : "";
			recordPath += ".csv";

			
			if (!File.Exists(recordPath))
			{
				File.CreateText(recordPath);
			}
			//dataType::: 0:Note
			//0,The things you have to know,,,,,,,,

			//dataType::: 1: single task
			//dataType, userId ,Device,object,angleType,elapsed time,overall task elapsed time, errorType,error distance
			//1,		20,		3,		1,		0,			20.4,			40.3,					1,	,    30 		

			//errorType::: 0: Excellent(under 17), 1: Good (17~30), 2:Not good(30~)

			using (StreamWriter streamWriterObject = File.AppendText(recordPath))
			{
				streamWriterObject.WriteLine("0,[User:"+userId+" - Device:"+userDevice+" - Object:"+userObject+" - BoundingBoxType:"+boundingBoxSpecified+" - time:"+System.DateTime.Now+"] :: "+note+"  ,,,,,,,,");
				streamWriterObject.WriteLine("dataType, userId ,Device,object,angleType,elapsed time,overall task elapsed time, errorType,error distance");
			}
			taskStartTime = Time.time;
			overallDeviceTime = 0;

			cumulativeError = 0;
		}
		public void startTask(GameObject targetObject, GameObject playerObject){
			taskStartTime = Time.time;
			using (StreamWriter streamWriterObject = File.AppendText(recordPath))
			{

//				streamWriterObject.WriteLine("0,"+TargetRotationType[index]+":::"+playerObject.transform.eulerAngles.x+"-"+playerObject.transform.eulerAngles.y+"-"+playerObject.transform.eulerAngles.z+"  ->  "+targetObject.transform.eulerAngles.x+"-"+targetObject.transform.eulerAngles.y+"-"+targetObject.transform.eulerAngles.z);
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

//					streamWriterObject.WriteLine("[Task:"+(index-startPoint)+", Type:"+TargetRotationType[index]+" -- Countdown End, Succeed ] "+elapsedTime + " / " + currentTaskTime);
				}else if(isSuccess == countdownResult.FAILED){
					streamWriterObject.WriteLine(elapsedTime + "," + currentTaskTime);
//					streamWriterObject.WriteLine("[Task:"+(index-startPoint)+", Type:"+TargetRotationType[index]+" -- Countdown End, Failed ] "+elapsedTime + " / " + currentTaskTime);
				}

			}
		}
		public void endTask(int errorType, double errorDistance){
			cumulativeError += errorDistance;
			double endTime = Time.time;
			double timeDifference = endTime - taskStartTime;
			overallDeviceTime += timeDifference;
			using (StreamWriter streamWriterObject = File.AppendText(recordPath))
			{
				//				streamWriterObject.WriteLine("[Task:"+(index-startPoint)+", Type:"+TargetRotationType[index]+" -- End ] "+timeDifference);
				//								streamWriterObject.WriteLine();


				streamWriterObject.WriteLine("1,"+_userId+","+_userDevice+","+_userObject+","+TargetRotationType[index]+","+timeDifference+","+overallDeviceTime+","+errorType+","+errorDistance);
				
				//dataType::: 1: single task
				//dataType, userId ,Device,object,angleType,elapsed time,overall task elapsed time, errorType,error distance
				//1,		20,		3,		1,		0,			20.4,			40.3,					1,	,    30 		
				
				
			}



			
		}
		public void endDeviceTask(){
			double endTime = Time.time;
			double timeDifference = endTime - taskStartTime;
			using (StreamWriter streamWriterObject = File.AppendText(recordPath))
			{
				streamWriterObject.WriteLine("0,[Device Ends At"+System.DateTime.Now+" -- Overall Times ] " + overallDeviceTime+"[Overall error] "+cumulativeError+"  ,,,,,,,,");
//				streamWriterObject.WriteLine();
			}
			
		}
	}




	// Use this for initialization
	void Start () {
		setRandomDestination();
		startPoint = (userId * 120 + userDevice * 24 + userObject * 12  ) % 720;
		index = startPoint;
		evaluateCamera.SetActive(false);
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
//			if(userDevice==0){//!for super pilot user study only!
//				BoundingBoxListPlayer[0].SetActive(true);
//				BoundingBoxListTarget[0].SetActive(true);
//			}else if(userDevice==1){
//				BoundingBoxListPlayer[1].SetActive(true);
//				BoundingBoxListTarget[1].SetActive(true);
//			}else if(userDevice==2||userDevice==3||userDevice==4){
//				BoundingBoxListPlayer[3].SetActive(true);
//				BoundingBoxListTarget[3].SetActive(true);
//			}
//		BoundingBoxListPlayer[3].SetActive(true);
//		BoundingBoxListPlayer[4].SetActive(true);
//		BoundingBoxListTarget[3].SetActive(true);
//		BoundingBoxListTarget[4].SetActive(true);
//			BoundingBoxListPlayer[0].SetActive(true);
//			BoundingBoxListTarget[0].SetActive(true);
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

				textCamera.SetActive(false);
				isEvaluateTextAppear = 0;
				nowState = gamestate.Playing;
				recordManagerObject.startTask(targetObject,playerObject);



			}else if(nowState == gamestate.End)
			{
				Debug.Log("Ends");
				//doing nothing. Closed manually.
			}else if(nowState == gamestate.Playing){


//				A old method with bias....
//				double difference = (eulerAngleXRealigned(targetObject.transform.eulerAngles.x,playerObject.transform.eulerAngles.x))
//									*(eulerAngleXRealigned(targetObject.transform.eulerAngles.x,playerObject.transform.eulerAngles.x))
//									+(eulerAngleXRealigned(targetObject.transform.eulerAngles.y,playerObject.transform.eulerAngles.y))
//						  			*(eulerAngleXRealigned(targetObject.transform.eulerAngles.y,playerObject.transform.eulerAngles.y))
//									+(eulerAngleXRealigned(targetObject.transform.eulerAngles.z,playerObject.transform.eulerAngles.z))
//						  			*(eulerAngleXRealigned(targetObject.transform.eulerAngles.z,playerObject.transform.eulerAngles.z));
//				
//				difference = Math.Pow(difference,0.5);

				float difference = Quaternion.Angle( targetObject.transform.rotation, playerObject.transform.rotation );

				int errorType = -1;


				SetListActive(VisibleTextList,false);

				if(difference < 17){
					errorType = 0;
					VisibleTextList[4].SetActive(true);
				}else if(difference >= 17 && difference < 34){
					errorType = 1;
					VisibleTextList[5].SetActive(true);
				}else if(difference >= 34){
					errorType = 2;
					VisibleTextList[6].SetActive(true);
				}

				recordManagerObject.endTask(errorType,difference);

				int overallTaskIndex = index - startPoint;


				evaluateCamera.SetActive(true);
				evaluateTextAppearTargetTime = Time.time + 1;
				isEvaluateTextAppear = 1;


				if((overallTaskIndex+1) >= 12){
					VisibleTextList[3].SetActive(true);
					textCamera.SetActive(true);
					nowState = gamestate.End;
					recordManagerObject.endDeviceTask();

				}else{//new round
					index ++;
					targetObject.transform.rotation = playerObject.transform.rotation;
					targetObject.transform.Rotate(Vector3.up, (float)TargetRotation[index].x);		
					targetObject.transform.Rotate(Vector3.right, (float)TargetRotation[index].y);
					targetObject.transform.Rotate(Vector3.forward, (float)TargetRotation[index].z); 

					recordManagerObject.startTask(targetObject,playerObject);

				}



			}
		}

	
//		Debug.Log(playerObject.transform.eulerAngles.x+","+playerObject.transform.eulerAngles.y+","+playerObject.transform.eulerAngles.z);

		if(isEvaluateTextAppear == 1){// in the text showed process
			double timeDifference = evaluateTextAppearTargetTime - Time.time ;
			if(timeDifference <=0){// End text
				isEvaluateTextAppear = 0;
				VisibleTextList[4].SetActive(false);
				VisibleTextList[5].SetActive(false);
				VisibleTextList[6].SetActive(false);
				evaluateCamera.SetActive(false);
			}
		}
		{//test
			float angle = Quaternion.Angle( targetObject.transform.rotation, playerObject.transform.rotation );
			
			
			// get a "forward vector" for each rotation
			Vector3 forwardA = targetObject.transform.rotation * Vector3.forward;
			Vector3 forwardB = playerObject.transform.rotation * Vector3.forward;
			
			// get a numeric angle for each vector, on the X-Z plane (relative to world forward)
			float angleA = Mathf.Atan2(forwardA.x, forwardA.z) * Mathf.Rad2Deg;
			float angleB = Mathf.Atan2(forwardB.x, forwardB.z) * Mathf.Rad2Deg;
			
			// get the signed difference in these angles
			float angleDiff = Mathf.DeltaAngle( angleA, angleB );
			
			Debug.Log(""+angle);


		}

//		{
//		
//		double difference = (eulerAngleXRealigned(targetObject.transform.eulerAngles.x,playerObject.transform.eulerAngles.x))
//			*(eulerAngleXRealigned(targetObject.transform.eulerAngles.x,playerObject.transform.eulerAngles.x))
//				+(eulerAngleXRealigned(targetObject.transform.eulerAngles.y,playerObject.transform.eulerAngles.y))
//				*(eulerAngleXRealigned(targetObject.transform.eulerAngles.y,playerObject.transform.eulerAngles.y))
//				+(eulerAngleXRealigned(targetObject.transform.eulerAngles.z,playerObject.transform.eulerAngles.z))
//				*(eulerAngleXRealigned(targetObject.transform.eulerAngles.z,playerObject.transform.eulerAngles.z));
//		
//		//			double difference = (targetObject.rotation.eulerAngles.x - playerObject.rotation.eulerAngles.x)
//		//					*(targetObject.rotation.eulerAngles.x - playerObject.rotation.eulerAngles.x)
//		//					+(targetObject.rotation.eulerAngles.y - playerObject.rotation.eulerAngles.y)
//		//					*(targetObject.rotation.eulerAngles.y - playerObject.rotation.eulerAngles.y)
//		//					+(targetObject.rotation.eulerAngles.z - playerObject.rotation.eulerAngles.z)
//		//					*(targetObject.rotation.eulerAngles.z - playerObject.rotation.eulerAngles.z);
//		
//		
//		
//		difference = Math.Pow(difference,0.5);
//
//			Debug.Log(difference);
//		}

	}

	private double eulerAngleXRealigned(double x1, double x2){
		if( Math.Abs(x1-x2) > 180){//cross point 0
			return Math.Abs(360 - Math.Abs(x1-x2));
		}else{
			return Math.Abs(x1-x2);
		}
	}

	private void setRandomDestination(){
		TargetRotation = new List<Vector3>();
		TargetRotationType = new List<int>();
		if(isTrainingSession == 2){//just for target data collection

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
		}else if(isTrainingSession == 0){
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-64,-41,5));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(29,-12,-69));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-106,-100,-35));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-51,-81,-74));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(37,-30,-18));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-13,-48,-7));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(20,-1,-47));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(7,-52,-3));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(12,-5,-45));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(41,-27,4));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-18,104,122));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-32,17,131));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(10,-72,-133));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(51,-14,15));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-11,-41,-34));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(52,-97,-5));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(31,11,26));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-18,6,-179));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-32,-30,-30));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-66,-27,-125));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(77,-113,-83));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(40,-5,45));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-72,-114,110));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-23,32,12));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-49,9,24));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(121,55,-115));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-53,-27,1));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(54,-22,13));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(41,-30,24));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-18,-9,26));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-81,92,-68));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(96,-1,83));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(24,26,157));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(59,-1,5));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-116,-82,-28));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-98,-50,57));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(67,-28,98));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-4,0,-35));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-8,63,109));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(30,10,-24));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(36,21,4));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(11,-55,136));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-55,9,72));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(0,-40,19));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-5,48,-6));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-72,-134,11));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-5,19,-29));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-91,112,-106));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-24,129,32));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(23,40,-10));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(9,-15,-34));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(91,-97,18));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-43,23,8));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(104,-105,29));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-47,9,-28));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(36,-22,3));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-85,-81,76));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-150,-20,-4));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-38,44,0));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(28,-163,26));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(17,-103,124));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(5,10,23));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-24,65,-71));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(37,45,7));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-20,46,10));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-8,35,-11));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(11,-123,74));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-88,-83,-87));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(9,27,29));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-35,-9,40));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-89,104,-115));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(164,52,-49));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-35,-23,7));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-92,-4,145));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-14,-21,-35));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(56,130,55));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(123,18,-80));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-68,1,-162));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-14,-22,-47));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-48,-13,4));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(19,-7,42));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-33,-27,-42));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(45,-81,-77));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(10,34,-96));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(39,112,-71));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(95,37,-22));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-54,-103,-71));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(13,4,30));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(81,74,0));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(51,-13,22));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-20,-4,13));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(19,-75,82));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-145,9,-88));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(18,-25,-8));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-37,-37,-25));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(27,-29,-7));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(12,-2,51));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-13,-36,-45));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(40,-5,-11));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(23,111,-126));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(97,-82,-96));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(48,-13,168));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(27,-99,-16));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-46,38,-1));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-34,-36,21));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-71,-147,78));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-5,-52,97));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-35,-3,21));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-32,-2,19));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(23,-4,-45));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-45,16,-18));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-63,-48,-90));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-39,-46,116));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(83,-109,-81));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(39,-13,13));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(148,55,83));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(89,62,36));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(26,48,-64));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-19,19,47));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-40,-4,-22));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(45,-10,5));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(3,-48,0));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(2,-20,-35));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-148,5,-44));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-14,-137,78));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-57,-4,55));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(30,-144,-53));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-44,-95,-52));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(5,45,-27));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-7,-26,31));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-74,-18,-6));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(47,26,-27));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(46,7,-24));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(14,9,-26));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(7,-26,-22));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(67,92,-83));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(12,-88,-1));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-17,19,13));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(125,128,22));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-67,-46,-135));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(51,67,-155));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-70,73,10));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(24,26,7));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(5,23,-56));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-8,21,14));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(49,35,-2));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(44,-20,-110));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(9,23,49));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(162,61,-24));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-1,35,-140));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(0,-18,-54));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-103,-82,44));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-16,3,-40));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-36,41,-16));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-64,-28,-94));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-50,-48,-54));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-35,5,-29));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-29,161,-32));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(44,13,-40));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(89,-30,85));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-45,5,9));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-165,-26,26));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-62,77,65));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-10,-38,28));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-45,-17,10));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(117,91,-63));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-37,-19,34));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(137,68,83));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-55,-22,-83));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-32,-23,-44));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(2,-13,53));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-20,11,-20));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-32,-23,42));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(0,8,-26));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(4,-35,3));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-165,51,46));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(100,38,-114));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-130,-42,61));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-109,-6,22));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(46,-144,64));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-102,-81,81));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(34,-103,-90));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-16,-27,-15));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-74,-32,61));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-51,-8,13));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-8,-152,73));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(21,21,-37));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(45,11,21));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(32,131,108));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-19,-35,-28));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(23,-17,40));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-119,-113,-19));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(60,51,159));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-122,12,-112));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(4,22,35));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-16,87,-145));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-41,-15,-28));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(120,-16,-71));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-44,-3,-22));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(49,-7,15));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-31,45,-9));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(133,-59,89));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-30,21,17));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-104,-18,96));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(54,12,150));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(99,97,7));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(6,124,-131));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-8,23,10));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-49,-15,17));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-46,92,-56));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(126,9,113));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(22,34,20));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-32,49,14));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(15,-29,-44));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-159,-24,-80));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(2,-28,-24));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-51,93,-5));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-17,19,49));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-140,104,46));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-28,-20,-19));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-16,-46,30));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(29,-121,-17));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-61,-91,76));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(74,-38,-112));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(16,32,-22));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(15,-37,3));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(142,-55,-94));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(32,8,2));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(68,7,21));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-28,23,-109));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(20,-21,33));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(132,42,-88));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(66,-86,-26));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(26,13,26));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-23,-119,-92));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-9,56,22));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(71,4,-25));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(26,-23,-3));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-3,-38,-18));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(48,14,-26));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(15,10,27));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-44,-28,-28));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(49,1,-28));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-6,-3,49));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(165,16,-56));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(61,55,91));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-67,77,-30));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(78,-38,-123));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-33,-31,-40));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-40,2,18));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(66,-36,-24));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(98,60,-43));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-47,-24,-3));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(9,7,-17));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-4,124,-116));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(34,114,-40));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-37,-144,-3));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-173,-31,-18));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-33,-29,-4));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(22,37,-23));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(26,45,-138));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(52,0,17));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-120,61,26));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(29,30,-30));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(21,37,5));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(122,-1,-11));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(116,-9,-74));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(33,9,-27));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(22,-14,-23));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(26,-21,38));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(5,102,-90));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-99,-9,48));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(13,1,-18));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(42,7,-174));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(93,107,-64));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-26,9,-47));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-8,39,-40));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(101,-28,-134));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-82,7,117));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(3,103,-127));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-103,-8,75));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(28,-46,-8));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(78,51,42));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-34,-78,-122));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(28,28,23));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(6,-23,-31));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-51,-27,6));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-48,-24,12));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-16,-32,-41));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-44,124,96));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-38,14,-37));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(68,140,84));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(33,-76,-114));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(54,21,-9));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(36,-39,165));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-24,12,-24));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(30,23,-26));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(154,-74,-54));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-15,-34,-14));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-10,-82,17));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(54,10,19));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-64,-154,9));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(50,159,8));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-28,-8,173));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-30,-38,-94));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-56,16,14));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(70,-85,-126));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(33,115,82));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(12,55,-17));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(32,30,6));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(21,21,-25));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-4,42,5));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(68,40,119));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(23,28,-2));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-75,9,-113));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-30,-24,-30));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-5,5,21));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-4,-159,-33));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(138,68,81));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-28,-3,-13));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(27,6,-27));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-27,82,-6));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(9,-43,-34));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-79,0,-156));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-37,-31,9));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(21,46,-103));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-17,-94,-113));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-79,83,-122));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-20,-48,14));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-80,66,-93));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-8,-21,-13));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(3,3,-51));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-70,-3,137));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-34,16,44));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(113,130,15));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(44,-10,-17));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(26,-44,10));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-109,1,-22));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(18,9,-52));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-34,28,-29));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-32,-34,112));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-106,11,118));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(16,44,23));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(30,140,36));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(19,-37,68));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-49,26,-21));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(18,93,-8));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-39,-28,30));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-11,28,-7));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-52,-33,-77));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(53,13,51));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(7,-34,17));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-12,35,-41));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-48,-104,71));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-10,-114,-39));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(21,23,-10));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-56,40,127));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(7,28,15));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-13,-142,40));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-16,15,-18));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-22,35,-136));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-145,-30,5));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(9,-41,-42));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-12,-30,1));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-38,18,-9));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(7,-12,92));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-119,-105,41));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-36,-2,13));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-20,18,12));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-4,-31,154));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-122,-85,-91));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(8,34,1));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(101,-25,2));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-117,74,64));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-26,113,-118));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-108,28,-126));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(16,-19,-25));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-31,-12,23));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-60,-65,98));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(7,11,20));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(18,-20,-26));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-15,-7,-31));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-8,14,19));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-19,31,-166));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-24,5,13));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-3,-56,-2));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-1,0,84));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(34,7,18));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-29,128,78));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-42,25,-21));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(100,80,21));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-38,-26,-23));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(16,-56,-4));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-77,78,-97));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(105,55,89));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-64,-141,0));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-17,-12,43));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-26,-15,-30));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(21,60,111));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-32,64,14));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-63,133,-54));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-112,101,77));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(30,-160,6));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(2,2,25));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(41,34,-14));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(21,-8,23));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(40,8,39));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(86,61,-1));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(30,-38,2));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(90,13,-8));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-25,147,-42));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-21,2,35));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(74,67,69));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-51,-11,8));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(5,163,-23));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(17,-40,-5));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-16,-29,-22));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(31,-141,-82));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-83,-21,15));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-45,-28,5));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-9,42,41));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-153,46,-54));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(24,-137,103));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(7,-34,18));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(16,-43,14));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-51,56,58));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-66,-48,42));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-41,-98,71));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-51,-14,15));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(16,-35,5));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-112,-73,-80));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(14,-46,12));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-57,-23,-36));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(17,-91,107));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-55,7,-10));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-31,14,24));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(46,11,-107));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(91,120,-95));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-23,32,-10));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-12,-34,18));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-135,34,-70));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(46,21,16));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-100,61,108));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(14,23,13));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(26,-67,-74));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-73,-77,-93));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-51,21,-15));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-64,155,5));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-26,42,26));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(34,-18,9));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(21,15,-45));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(10,13,-29));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(27,-114,-32));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(37,-111,114));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-7,69,-38));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-3,45,35));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(43,89,-14));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(4,87,-29));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-5,-5,39));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(130,-85,86));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-10,-19,43));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-26,-25,-43));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(18,-15,-44));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-51,151,70));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(41,-14,41));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-105,68,-129));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-34,104,71));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(1,-37,16));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(103,104,77));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(47,-83,104));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(115,2,111));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(60,33,44));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(31,-132,-109));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(12,28,29));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-5,27,-34));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(88,-24,77));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(15,11,-20));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(44,2,7));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(32,-11,32));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-21,0,9));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-50,71,136));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(10,-21,3));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-29,-14,-48));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-112,-39,37));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-15,-9,-27));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-27,25,-30));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(21,19,12));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-101,64,-108));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(102,77,-104));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(82,60,6));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-11,-18,-96));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(30,-23,-36));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(12,23,-52));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-144,-46,-53));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(13,36,-46));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(25,-50,75));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-120,-80,60));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(137,-14,30));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-9,-17,29));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-96,-131,-32));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-99,38,72));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-45,-20,8));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-48,12,13));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(6,-35,47));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(48,78,-59));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(54,-38,124));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(12,19,32));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-72,-116,-112));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(8,-36,-10));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(53,-16,-19));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(30,113,-45));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-9,2,-24));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-12,19,-7));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-35,-142,-71));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-100,23,-146));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(17,-17,-11));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-25,-36,172));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(42,96,97));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-149,89,-28));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(80,-144,-19));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-17,20,36));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-5,-162,-16));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(36,127,6));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(11,-33,35));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(54,-9,7));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(9,18,30));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(12,-19,-50));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-22,21,49));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(5,94,-153));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-26,-5,43));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-4,11,39));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-113,-67,-110));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(45,6,-18));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(26,-21,-23));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(23,-74,9));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-83,95,13));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(12,-98,44));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-14,96,126));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(6,56,-3));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(25,-53,-16));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(32,-142,80));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-143,-46,36));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(139,-115,1));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-12,-157,-86));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-25,-15,37));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-33,-6,34));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(18,-39,24));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-27,-14,0));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(69,-56,4));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-12,17,23));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(59,149,29));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-14,3,-41));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-25,-41,3));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(81,-142,74));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(9,50,12));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(5,114,89));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(19,5,-19));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-30,8,-42));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-95,-110,-58));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-71,27,50));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(32,36,-8));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-36,2,-166));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(31,-33,-34));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-9,70,-8));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(126,61,4));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(153,49,30));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(9,11,-31));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-161,7,-60));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(37,15,26));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(12,9,-32));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(7,-23,26));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(164,36,-7));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(13,-18,48));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(111,85,1));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-135,48,72));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-19,9,34));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(16,2,-20));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(9,22,147));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-118,19,57));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(70,88,75));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(33,-4,42));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(25,-137,107));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-48,-21,-15));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(51,11,56));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(14,-147,-91));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(16,42,-37));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-4,54,-2));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(24,-44,7));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(13,-47,23));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-57,-89,97));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(88,76,31));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-34,-13,-33));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-48,-83,110));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-47,-22,3));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-21,25,0));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(34,-25,-159));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-70,66,72));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(54,117,-24));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-31,-41,-13));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-12,30,10));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(2,-23,115));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(20,14,39));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(12,-147,-11));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(79,-109,-30));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(57,-14,59));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(11,-51,-20));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-14,-140,-107));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-23,32,12));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-12,-53,9));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(33,36,4));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(56,-23,-5));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(19,90,58));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-11,116,137));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-79,-43,72));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(8,58,-13));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(17,-57,1));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(107,13,33));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(43,-8,-25));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(82,12,-91));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(35,25,34));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-76,-100,105));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(31,-11,-1));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-92,-120,-90));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-3,8,-24));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(7,28,-36));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(113,21,-136));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(72,-89,-12));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-37,44,6));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(9,-36,41));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(8,173,23));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-53,99,-57));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(27,-11,48));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(27,92,9));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(127,-73,-28));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-26,-32,-18));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(45,8,-22));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-9,-38,24));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-52,83,127));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(102,-40,-68));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-41,-29,-15));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-4,-30,7));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-20,22,-9));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-5,-23,46));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(105,31,-29));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-42,-108,-52));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(22,30,-6));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-64,-56,-30));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-101,87,-26));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(18,-78,79));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(56,58,-2));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(22,-14,17));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-105,143,5));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(13,-59,-5));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(25,118,106));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-22,13,-51));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(33,23,43));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-15,34,-5));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-14,-84,77));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(3,15,-26));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-9,-84,-54));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(2,-34,27));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(85,-29,0));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(57,-110,95));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(43,145,18));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-67,-142,13));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-14,-36,-16));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-40,-18,30));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(6,51,25));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(27,44,93));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-148,-47,78));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(13,-25,-32));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(0,-35,-19));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(132,-83,49));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-23,36,41));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(87,58,-78));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(10,21,35));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-1,-133,-26));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(56,-13,-13));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(128,103,-61));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(1,40,25));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-16,-24,38));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-3,18,43));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-34,19,74));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-119,35,60));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-32,39,9));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-12,42,-39));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(49,25,-115));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-61,-108,-4));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-44,-13,-35));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(34,-5,20));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(153,8,19));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(25,8,-23));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(67,57,65));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-2,64,-105));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-20,0,-20));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-62,51,-11));
			
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-46,-24,-4));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-52,143,12));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-21,17,-47));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-5,-36,-18));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-16,-63,-72));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(10,-58,109));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-37,-16,3));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-3,13,-41));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-56,-5,10));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-8,74,138));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(29,-103,-17));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-32,-62,147));
			
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(80,88,-3));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(41,41,8));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-154,36,-44));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-105,138,-29));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(2,-41,-30));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-36,5,11));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-36,15,-36));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(-9,-37,43));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(142,-35,17));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(-132,38,85));
			TargetRotationType.Add(0);
			TargetRotation.Add(new Vector3(4,26,-54));
			TargetRotationType.Add(1);
			TargetRotation.Add(new Vector3(114,7,22));
			

		}
	}



}