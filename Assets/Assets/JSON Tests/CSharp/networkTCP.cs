using UnityEngine;
using System.Collections;
using System.Net.Sockets; 
using System.Text;

public class scrff : MonoBehaviour {

	NetworkStream clientStream;
	// Use this for initialization
	void Start () {
	
		TcpClient tcpClient = new TcpClient(); 
		tcpClient.Connect("127.0.0.1", 10000);

		clientStream = tcpClient.GetStream(); 
		byte[] message = new byte[4096];
		int bytesRead;
		bytesRead = 0;
		bytesRead = clientStream.Read(message, 0, 4096);

		UTF8Encoding encoder = new UTF8Encoding();
		MonoBehaviour.print(encoder.GetString(message, 0, bytesRead)); 



	}
	int where = 0;
	// Update is called once per frame
	void Update () {
		where ++;
		byte[] outStream = Encoding.UTF8.GetBytes("___"+ where );
		clientStream.Write(outStream, 0, outStream.Length);
		clientStream.Flush();
		Debug.Log("sent!");
	
	}
}
