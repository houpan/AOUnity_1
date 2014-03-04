using UnityEngine;
using System.Collections; 
using System.Text;
using SimpleJSON;
using System.Threading;

public class OSCListener_houpan : MonoBehaviour {

	private string UDPHost = "127.0.0.1";
	private int listenerPort = 8000;
	private int broadcastPort = 57131;
	public Osc oscHandler ;


	private string eventName = "";
	private string eventData = "";
	private int counter = 0;
	private int boxWidth;
	// public var output_txt : GUIText;



	// Use this for initialization
	public void Start () {
		UDPPacketIO udp = (UDPPacketIO)GetComponent("UDPPacketIO");
		udp.init(UDPHost, broadcastPort, listenerPort);
		oscHandler = (Osc) GetComponent("Osc");
		oscHandler.init(udp);
				
		oscHandler.SetAddressHandler("/eventTest", updateText);
		oscHandler.SetAddressHandler("/counterTest", counterTest);
		Debug.Log("Running");

	}
	
	// Update is called once per frame
	public void Update () {
		Debug.Log("Count: "+counter+" Event: " + eventName + " Event data: " + eventData);
/*	
		GameObject cube = GameObject.Find("Cube");
		boxWidth = counter;
	    cube.transform.localScale = new Vector3(boxWidth,5,5);	

*/	}
	public void updateText(OscMessage oscMessage )
	{	
		//真的是我看過最荒唐的錯誤了…C#function名字不能太長，不然會找不到....
		eventName = Osc.OscMsgTostr(oscMessage);
		eventData = (string)oscMessage.Values[0];
	} 

	public void counterTest(OscMessage oscMessage)
	{	
		Osc.OscMsgTostr(oscMessage);
		counter = (int)oscMessage.Values[0];
	} 




}