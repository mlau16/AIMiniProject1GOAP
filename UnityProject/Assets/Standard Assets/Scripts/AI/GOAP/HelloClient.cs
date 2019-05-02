using UnityEngine;
using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Text;

public class HelloClient : MonoBehaviour
{
    private HelloRequester _helloRequester;
    public bool SendPack = true;
    private byte[] current_img;
    public GoapAgent[] agents;
    public List<GameObject> targets;
    public List<int> targetsID;
    //Dictionary<GameObject, string> map;

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
        //map = new Dictionary<GameObject, string>();
        targets = new List<GameObject>();
        targetsID = new List<int>();
        agents = GameObject.FindObjectsOfType<GoapAgent>();
        var supplyPiles = FindObjectsOfType<SupplyPileComponent>();
        var rocks = FindObjectsOfType<IronRockComponent>();
        var forges = FindObjectsOfType<ForgeComponent>();
        var trees = FindObjectsOfType<TreeComponent>();
        var chopBlocks = FindObjectsOfType<ChoppingBlockComponent>();

        foreach (var item in supplyPiles)
        {
            targets.Add(item.gameObject);
        }
        foreach (var item in rocks)
        {
            targets.Add(item.gameObject);
        }
        foreach (var item in trees)
        {
            targets.Add(item.gameObject);
        }
        foreach (var item in forges)
        {
            targets.Add(item.gameObject);
        }
        foreach (var item in chopBlocks)
        {
            targets.Add(item.gameObject);
        }

        foreach (var item in targets)
        {
            targetsID.Add(item.GetInstanceID());
        }

        _helloRequester = new HelloRequester();
        _helloRequester.Start();
        _helloRequester.msgEvent += ProcessMessage;
    }

    public void ProcessMessage(string message)
    {
        Debug.Log("Json Message: " + message);
        GoapResposeObject obj = JsonConvert.DeserializeObject<GoapResposeObject>(message);
        Debug.Log(obj.ID);

        // enqueue the action list
        Queue<GoapAction> queue = new Queue<GoapAction>();
        List<GoapAction> temp = new List<GoapAction>();

        Debug.Log(obj.solutionList.Count);

        foreach (var a in obj.solutionList)
        {

            GoapAction tempAction;

            switch (a.type)
            { 
            
                case "ChopFirewoodAction":
                    tempAction = new ChopFirewoodAction();
                    break;
                case "ChopTreeAction":
                    tempAction = new ChopTreeAction();
                    break;
                case "DropOffFirewoodAction":
                    tempAction = new DropOffFirewoodAction();
                    break;
                case "DropOffLogsAction":
                    tempAction = new DropOffLogsAction();
                    break;
                case "DropOffOreAction":
                    tempAction = new DropOffOreAction();
                    break;
                case "DropOffToolsAction":
                    tempAction = new DropOffToolsAction();
                    break;
                case "ForgeToolAction":
                    tempAction = new ForgeToolAction();
                    break;
                case "MineOreAction":
                    tempAction = new MineOreAction();
                    break;
                case "PickUpLogsAction":
                    tempAction = new PickUpLogsAction();
                    break;
                case "PickUpOreAction":
                    tempAction = new PickUpOreAction();
                    break;
                case "PickUpToolAction":
                    tempAction = new PickUpToolAction();
                    break;
                default:
                    tempAction = new GoapAction();
                    break;
            }

            foreach (var subList in a.preconditions)
            {
                if (subList[1] == "true")
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
                if (subList[1] == "true")
                {
                    tempAction.addEffect(subList[0], true);
                    Debug.Log("Effect key" + subList[0]);
                }
                else
                {
                    tempAction.addEffect(subList[0], false);
                }
            }

            tempAction.cost = a.cost;

            //Debug.Log(a.targetID);
            for (int i = 0; i < targets.Count; i++)
            {

                if (targetsID[i] == a.targetID)
                {
                    tempAction.targetID = a.targetID;
                    //Debug.Log("Match");
                }
            }

            //tempAction.checkProceduralPrecondition()
            temp.Add(tempAction);
            queue.Enqueue(tempAction);
        }

        foreach (var item in agents)
        {
            if (item.ID == obj.ID)
            {
                item.hasPlan = true;
                item.plan = queue;
                Debug.Log("Set plan");
            }
        }
    }

    private void OnDestroy()
    {
        _helloRequester.Stop();
    }
}