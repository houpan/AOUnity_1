using UnityEngine;
using System.Collections;
using System.Net.Sockets; 
using System.Text;
using SimpleJSON;
using System.Threading;

public class TCPCommunicator
{

	TcpClient tcpClient;
	NetworkStream clientStream;
	UTF8Encoding encoder;
	bool waiting = false;
	bool isAlive = false;


    public TCPCommunicator(){
		tcpClient = new TcpClient(); 
		tcpClient.Connect("54.250.127.255", 10000);


		clientStream = tcpClient.GetStream(); 
		byte[] message = new byte[4096];
		int bytesRead;
		bytesRead = 0;
		bytesRead = clientStream.Read(message, 0, 4096);

		encoder = new UTF8Encoding();
		MonoBehaviour.print(encoder.GetString(message, 0, bytesRead)); 

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
/*		byte[] outStream = Encoding.UTF8.GetBytes("__"+stringfied);
		clientStream.Write(outStream, 0, outStream.Length);
		clientStream.Flush();*/

		Debug.Log("sendFirstMessageToServer!");


    }

	void sendMessageToServerUpon(string stringInput){
		int bytesRead;
		byte[] outStream = Encoding.UTF8.GetBytes("__"+stringInput);
		clientStream.Write(outStream, 0, outStream.Length);
		clientStream.Flush();		
	}

	public void listeningToServerMessage(){
		byte[] message = new byte[4096];
		int bytesRead;
		bytesRead = 0;
		while(true){
			sendMessageToServerUpon("Unity is waiting for msg!");
			Debug.Log("unity is waiting!");
			bytesRead = clientStream.Read(message, 0, 4096);
			clientStream.Flush();

			sendMessageToServerUpon("Unity got your msg!");
			Debug.Log("unity get!::");
			MonoBehaviour.print(encoder.GetString(message, 0, bytesRead));	
		}

	}

    // This method will be called when the thread is started.
    public void DoWork()
    {
    }
    public void RequestStop()
    {
    }
    // Volatile is used as hint to the compiler that this data
    // member will be accessed by multiple threads.
    private volatile bool _shouldStop;
}

public class networkTCP : MonoBehaviour {



	TCPCommunicator TCPCommunicatorObject;
    Thread TCPCommunicatorlistenerThread;

	// Use this for initialization
	void Start () {
		TCPCommunicatorObject = new TCPCommunicator();

		TCPCommunicatorlistenerThread = new Thread(TCPCommunicatorObject.listeningToServerMessage);
        TCPCommunicatorlistenerThread.Start();
        while(!TCPCommunicatorlistenerThread.IsAlive);//讓程式把這個thread開起來候再作事


	}




	// Update is called once per frame
	void Update () {

/*
        // Request that the worker thread stop itself:
        TCPCommunicatorObject.RequestStop();

        // Use the Join method to block the current thread 
        // until the object's thread terminates.
        TCPCommunicatorObject.Join();
		Debug.Log("main thread: Worker thread has terminated.");
		*/
	}

	void OnDestroy () {
	}

}
