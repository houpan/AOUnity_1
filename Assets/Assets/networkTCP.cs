using UnityEngine;
using System.Collections;
using System.Net.Sockets; 
using System.Text;
using SimpleJSON;
using System.Threading;

public class TCPCommunicator
{

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

	TcpClient tcpClient;
	NetworkStream clientStream;
	UTF8Encoding encoder;
	bool waiting = false;
    Thread listenerThread;



	// Use this for initialization
	void Start () {
	
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

		Debug.Log("sent!");

        listenerThread = new Thread(acceptance);
        listenerThread.Start();


	}
	void sendMessageToServerUpon(string stringInput){
		int bytesRead;
		byte[] outStream = Encoding.UTF8.GetBytes("__"+stringInput);
		clientStream.Write(outStream, 0, outStream.Length);
		clientStream.Flush();		
	}

	void acceptance(){
    

			byte[] message = new byte[4096];
			int bytesRead;
			bytesRead = 0;
			while(true){

				sendMessageToServerUpon("Unity is waiting for msg!");
				Debug.Log("unity is waiting!");
				bytesRead = clientStream.Read(message, 0, 4096);
				clientStream.Flush();
				MonoBehaviour.print(encoder.GetString(message, 0, bytesRead));	

				sendMessageToServerUpon("Unity got your msg!");
				Debug.Log("unity get!");
			}
/*
     do{
          numberOfBytesRead = myNetworkStream.Read(myReadBuffer, 0, myReadBuffer.Length);  
          myCompleteMessage = 
              String.Concat(myCompleteMessage, Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));  
     }
     while(myNetworkStream.DataAvailable);
     */

	}

	// Update is called once per frame
	void Update () {



		 
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

		byte[] outStream = Encoding.UTF8.GetBytes("__"+stringfied+"_"+ where );
		clientStream.Write(outStream, 0, outStream.Length);
		clientStream.Flush();
		Debug.Log("sent!");
*/
	
	}

	void OnDestroy () {
		print("Script was destroyed");
	}

}
