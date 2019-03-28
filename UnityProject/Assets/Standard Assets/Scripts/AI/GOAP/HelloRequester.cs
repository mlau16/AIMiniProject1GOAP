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
                            GoapResposeObject obj = JsonConvert.DeserializeObject<GoapResposeObject>(message);
                            Debug.Log(obj.ID);
                            
                            // enqueue the action list
                            Queue<GoapAction> queue = new Queue<GoapAction>();
                            List<GoapAction> temp = new List<GoapAction>();

                            foreach (var a in obj.solutionList)
                            {
                                tempAction = new GoapAction();
                                tempAction.preconditions = a.preconditions;
                                tempAction.effects = a.effects;
                                tempAction.cost = a.cost;
                                queue.Enqueue(tempAction);
                            }


                        }
                       
                        // TODO: Analyze what we recieved here! :) 
                        // Check if the string is of json format!
                        // if it is, tryparse it to the relevant c# class representations thereof
                        // if it is *, update the relevant object with the new information.
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
    public List<ActionInformation> solutionList;
    public int ID;
}


class TestObject 
{
    public int x, y;
    public string z;
    public float p;
    public bool q;

    public TestObject() 
    {

    }
}
