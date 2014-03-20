
private var UDPHost : String = "127.0.0.1";
private var listenerPort : int = 8000;
private var broadcastPort : int = 57131;
private var oscHandler : Osc;

private var eventName : String = "";
private var eventData : String = "";
private var counter : int = 0;
private var lowpassAlpha : float = 0.4f;

private var yawTemp : float ;
private var pitchTemp : float ;
private var rollTemp : float ;

// public var output_txt : GUIText;
//private var ArrowObj : GameObject;
public var yawInput : float;
public var pitchInput : float;
public var rollInput : float;
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

	yawTemp = oscMessage.Values[0];
	pitchTemp = oscMessage.Values[1];
	rollTemp = oscMessage.Values[2];
	
	if((yawInput < 0 && yawTemp > 0 )||(yawInput > 0 && yawTemp < 0 )){
		yawInput = yawTemp;
	}else{
		yawInput = yawTemp * lowpassAlpha + (yawInput * (1.0 - lowpassAlpha));		
	}

	if((pitchInput < 0 && pitchTemp > 0 )||(pitchInput > 0 && pitchTemp < 0 )){
		pitchInput = pitchTemp;
	}else{
		pitchInput = pitchTemp * lowpassAlpha + (pitchInput * (1.0 - lowpassAlpha));		
	}
	
	if((rollInput < 0 && rollTemp > 0 )||(rollInput > 0 && rollTemp < 0 )){
		rollInput = rollTemp;
	}else{
		rollInput = rollTemp * lowpassAlpha + (rollInput * (1.0 - lowpassAlpha));		
	}
/*
	yawInput = oscMessage.Values[0] ;
	pitchInput = oscMessage.Values[1] ;
	rollInput = oscMessage.Values[2] ;
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
