using UnityEngine;
using System.Collections;

public class rotateItself : MonoBehaviour {

public float roll ;
public float pitch;
public float yaw;
public float v3Current;
	// Use this for initialization
	void Start () {
		transform.Rotate(Vector3.up, yaw);		
		transform.Rotate(Vector3.right, pitch);
		transform.Rotate(Vector3.forward, roll); 
	}
	
	// Update is called once per frame
	void Update () {

	}
}
