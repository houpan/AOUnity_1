
private var UDPHost : String = "127.0.0.1";
private var listenerPort : int = 8000;
private var broadcastPort : int = 57131;
private var oscHandler : Osc;

private var eventName : String = "";
private var eventData : String = "";
private var counter : int = 0;
// public var output_txt : GUIText;
//private var ArrowObj : GameObject;
private var yawInput : float;
private var pitchInput : float;
private var rollInput : float;
public var TargetObject : GameObject;

public function Start ()
{	
	var udp : UDPPacketIO = GetComponent("UDPPacketIO");
	udp.init(UDPHost, broadcastPort, listenerPort);
	oscHandler = GetComponent("Osc");
	oscHandler.init(udp);
			
	oscHandler.SetAddressHandler("/YPRValue", updateYPRValue);
	//oscHandler.SetAddressHandler("/counterTest", counterTest);
	//ArrowObj = GameObject.Find("Arrow");
	
}
Debug.Log("Running");

function Update () {

	TargetObject.transform.eulerAngles = new Vector3(0,0,0);
    TargetObject.transform.Rotate(Vector3.up, yawInput);		
    TargetObject.transform.Rotate(Vector3.right, pitchInput);
    TargetObject.transform.Rotate(Vector3.forward, rollInput); 
}	

public function updateYPRValue(oscMessage : OscMessage) : void
{	
	Debug.Log("Update: " + oscMessage.Values[0] + " , " + oscMessage.Values[1] +","+oscMessage.Values[2]);
	yawInput = oscMessage.Values[0];
	pitchInput = oscMessage.Values[1];
	rollInput = oscMessage.Values[2];

		//transform.eulerAngles = v3Current; 
		//transform.eulerAngles = new Vector3(pitch,roll,yaw);
/*		roll = transform.eulerAngles[0];
		pitch = transform.eulerAngles[1];
		yaw = transform.eulerAngles[2];


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
public function updateText(oscMessage : OscMessage) : void
{	
	eventName = Osc.OscMsgTostr(oscMessage);
	eventData = oscMessage.Values[0];
} 

public function counterTest(oscMessage : OscMessage) : void
{	
	Osc.OscMsgTostr(oscMessage);
	counter = oscMessage.Values[0];
} 
