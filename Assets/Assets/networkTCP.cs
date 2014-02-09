using UnityEngine;
using System.Collections;
using System.Net.Sockets; 
using System.Text;
using SimpleJSON;
using System.Threading;




public class TCPCommunicator
{


	public float realtimeRoll = 0;
	public float realtimePitch = 0;
	public float realtimeYaw = 0;

	private const string serverDestination = "54.250.127.255";
	private const int serverPort = 10000;

	private TcpClient tcpClientToServer;
	private NetworkStream streamToServer;
	private UTF8Encoding encoder;
	private bool waiting = false;
	private bool isAlive = false;

    private volatile bool _shouldStop;

    public TCPCommunicator(){
		tcpClientToServer = new TcpClient(); 
		tcpClientToServer.Connect(serverDestination, serverPort);
		streamToServer = tcpClientToServer.GetStream(); 
		Debug.Log("connectToServer!");
		encoder = new UTF8Encoding();


/*
		byte[] message = new byte[4096];
		int bytesRead;
		bytesRead = 0;
		bytesRead = streamToServer.Read(message, 0, 4096);


		MonoBehaviour.print(encoder.GetString(message, 0, bytesRead)); 
*/



		//JSON encoding test
		/*
        var I = new JSONClass();
        I["version"].AsInt = 5;
        I["author"]["name"] = "Bunny83";
        I["author"]["phone"] = "0123456789";
        I["data"][-1] = "First item\twith tab";
        I["data"][-1] = "Second item";
        I["data"][-1]["value"] = "class item";
        I["data"].Add("Forth item");
        I["data"][1] = I["data"][1] + " 'addition to the second item'";
        I.Add("version", "1.0");
        var stringfied = I.ToString();

        sendMessageToServerUpon(stringfied);


		Debug.Log("sendFirstMessageToServer!");
*/
		//Thread operation
		_shouldStop = false; 

    }


	void sendMessageToServerUpon(string stringInput){
		int bytesRead;
		byte[] outStream = Encoding.UTF8.GetBytes("__"+stringInput);//接收的
		streamToServer.Write(outStream, 0, outStream.Length);
		streamToServer.Flush();		
	}

	public void listeningToServerMessage(){
		byte[] message = new byte[4096];
		int bytesRead;

		bytesRead = 0;

		sendMessageToServerUpon("Unity is waiting for msg!");
		Debug.Log("unity is waiting!");

		while(true){
			if(streamToServer.DataAvailable){
				bytesRead = streamToServer.Read(message, 0, 4096);
				streamToServer.Flush();
				string gotString = encoder.GetString(message, 0, bytesRead);

				sendMessageToServerUpon("Unity:: Unity got your msg!");
				//Debug.Log("unity get!::"+gotString);

				var parsedJSON = JSON.Parse(gotString);
				var rollReceived = parsedJSON["roll"].AsFloat;
				var pitchReceived = parsedJSON["pitch"].AsFloat;
				var yawReceived = parsedJSON["yaw"].AsFloat;
				var commandReceived = parsedJSON["command"].Value;//parsed as a string
				Debug.Log("Command::"+commandReceived+",Roll::"+rollReceived+"Pitch::"+pitchReceived+",Yaw::"+yawReceived);				
				realtimeRoll = rollReceived;
				realtimePitch = pitchReceived;
				realtimeYaw = yawReceived;


			}
			if(_shouldStop){
				Debug.Log("thread: stop!");
				break;
			}
		}
	}

    public void RequestStop()
    {
    	_shouldStop = true;
    }
}

public class networkTCP : MonoBehaviour {


	TCPCommunicator TCPCommunicatorObject;
    Thread TCPCommunicatorlistenerThread;

	// Use this for initialization
	void Start () {
		TCPCommunicatorObject = new TCPCommunicator();

		TCPCommunicatorlistenerThread = new Thread(TCPCommunicatorObject.listeningToServerMessage);
        TCPCommunicatorlistenerThread.Start();

        while(!TCPCommunicatorlistenerThread.IsAlive);//等程式把這個thread開起來，再做事


	}




	// Update is called once per frame
	void Update () {

		GameObject go = GameObject.Find ("CubeZfront");
//				networkTCP speedController = go.GetComponent <networkTCP> ();
		//transform.Rotate(Time.deltaTime*50, 0, 0);
		//transform.localEulerAngles = new Vector3(90,30,0);
		go.transform.eulerAngles = new Vector3(TCPCommunicatorObject.realtimeRoll,TCPCommunicatorObject.realtimeYaw,TCPCommunicatorObject.realtimePitch);
	}

	void OnDestroy () {
		TCPCommunicatorObject.RequestStop();
        TCPCommunicatorlistenerThread.Join();
	}

}
