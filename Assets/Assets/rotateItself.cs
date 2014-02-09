using UnityEngine;
using System.Collections;

public class rotateItself : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

		//transform.Rotate(Time.deltaTime*50, 0, 0);
		//transform.localEulerAngles = new Vector3(90,30,0);
		transform.eulerAngles = new Vector3(65,0,-82);
//		transform.Rotate(Vector3.right * Time.deltaTime*50);
	}
}
