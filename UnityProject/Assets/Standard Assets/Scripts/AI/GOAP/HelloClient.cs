using UnityEngine;
using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

public class HelloClient : MonoBehaviour
{
    private HelloRequester _helloRequester;
    public bool SendPack = true;
    private byte[] current_img;
    public static GoapAgent[] agents;

    public void SendObject(string msg)
    {    
        _helloRequester.bytes = Encoding.ASCII.GetBytes(msg);
        _helloRequester.Continue();
    }


    private void OnPostRender()
    {
        /*
        if (SendPack)
        {
            string hey = "Hello world!";
            _helloRequester.bytes = Encoding.ASCII.GetBytes(hey);
            _helloRequester.Continue();
        }
        else if (!SendPack)
        {
            _helloRequester.Pause();

        }
        */
    }

    private void Start()
    {
        agents = GameObject.FindObjectsOfType<GoapAgent>();    
        _helloRequester = new HelloRequester();
        _helloRequester.Start();
    }

    private void OnDestroy()
    {
        _helloRequester.Stop();
    }


}