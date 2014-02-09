using UnityEngine;
using System.Collections;

public class rotateItself : MonoBehaviour {

public float roll ;
public float pitch;
public float yaw;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		transform.eulerAngles = new Vector3(roll*5,yaw*5,pitch*5);

/*
		GameObject go = GameObject.Find ("Main Camera");
		networkTCP speedController = go.GetComponent <networkTCP> ();
		float realtimeRoll = speedController.realtimeRoll;
		float realtimePitch = speedController.realtimePitch;
		//transform.Rotate(Time.deltaTime*50, 0, 0);
		//transform.localEulerAngles = new Vector3(90,30,0);
		transform.eulerAngles = new Vector3(realtimeRoll,0,realtimePitch);
//		transform.Rotate(Vector3.right * Time.deltaTime*50);
*/
	}
}
