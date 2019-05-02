using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

///     You can copy this class and modify Run() to suits your needs.
///     To use this class, you just instantiate, call Start() when you want to start and Stop() when you want to stop.

public class HelloRequester : RunAbleThread
{
    public byte[] bytes;
    public delegate void MsgEvent(string msg);
    public MsgEvent msgEvent;
    ///     Stop requesting when Running=false.
    protected override void Run()
    {
        ForceDotNet.Force(); 

        using (RequestSocket client = new RequestSocket())
        {
            client.Connect("tcp://localhost:5555");

            while(Running)
            {
                if (Send)
                {
                    //string message = client.ReceiveFrameString();
                    client.SendFrame(bytes);
                    string message = null;
                    bool gotMessage = false;

                    while (Running)
                    {
                        gotMessage = client.TryReceiveFrameString(out message); // this returns true if it's successful
                        if (gotMessage) break;
                    }
                    if (gotMessage) 
                    {
                     //   Debug.Log("Received " + message);

                        if (message == "Failed to make plan")
                        { }
                        else
                        {

                            if (msgEvent != null)
                            {
                                msgEvent.Invoke(message);
                            }                   
                        }
                    }
                    Send = false;
                }
                
            }
        }

        NetMQConfig.Cleanup();
    }
}

[System.Serializable]
public class GoapResposeObject
{
    public int ID;
    public List<ResponseInformation> solutionList;
}


[System.Serializable]
public class ResponseInformation
{
    public string type;
    public float cost;
    public List<List<string>> effects;
    public List<List<string>> preconditions;
    public int targetID;

    public ResponseInformation(List<List<string>> _preconditions, List<List<string>> _effects, float _cost, int _targetID)
    {
        preconditions = _preconditions;
        effects = _effects;
        cost = _cost;
        targetID = _targetID;
    }

    public ResponseInformation()
    {

    }
}

