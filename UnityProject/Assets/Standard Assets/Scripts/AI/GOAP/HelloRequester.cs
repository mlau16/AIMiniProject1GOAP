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
                            Debug.Log("Json Message: " + message);
                            GoapResposeObject obj = JsonConvert.DeserializeObject<GoapResposeObject>(message);
                            Debug.Log(obj.ID);
                            
                            // enqueue the action list
                            Queue<GoapAction> queue = new Queue<GoapAction>();
                            List<GoapAction> temp = new List<GoapAction>();


                            foreach (var a in obj.solutionList)
                            {
                                var tempAction = new GoapAction();

                                foreach (var subList in a.preconditions)
                                {
                                    if (subList[1] == "True")
                                    {
                                        tempAction.addPrecondition(subList[0], true);
                                    }
                                    else
                                    {
                                        tempAction.addPrecondition(subList[0], false);
                                    } 
                                }

                                foreach (var subList in a.effects)
                                {
                                    if (subList[1] == "True")
                                    {
                                        tempAction.addEffect(subList[0], true);
                                    }
                                    else
                                    {
                                        tempAction.addEffect(subList[0], false);
                                    }
                                }

                                tempAction.cost = a.cost;

                                tempAction.target = GameObject.Find(a.targetID);

                                queue.Enqueue(tempAction);
                            }

                            foreach (var item in queue)
                            {
                                Debug.Log(item.cost);
                                Debug.Log(item.effects.Count);

                            }
                            
                            GoapAgent[] agents = HelloClient.agents;

                            foreach (var item in agents)
                            {
                                if (item.ID == obj.ID)
                                {
                                    item.plan = queue;
                                }
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
    public int ID;
    public List<ResponseInformation> solutionList;
}


[System.Serializable]
public class ResponseInformation
{
    public float cost;
    public List<List<string>> effects;
    public List<List<string>> preconditions;
    public string targetID;

    public ResponseInformation(List<List<string>> _preconditions, List<List<string>> _effects, float _cost, string _targetID)
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

