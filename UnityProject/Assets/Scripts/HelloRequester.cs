﻿using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using System;
using UnityEngine;

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
                        Debug.Log("Received " + message);
                        // TODO: Analyze what we recieved here! :) 
                        // Check if the string is of json format!
                        // if it is, tryparse it to the relevant c# class representations thereof
                        // if it is *, update the relevant object with the new information.
                    }
                }
                
            }
        }

        NetMQConfig.Cleanup();
    }
}