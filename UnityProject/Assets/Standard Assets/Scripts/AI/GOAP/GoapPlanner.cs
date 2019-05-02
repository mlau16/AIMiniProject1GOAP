using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/**
 * Plans what actions can be completed in order to fulfill a goal state.
 */
public class GoapPlanner
{
	/**
	 * Plan what sequence of actions can fulfill the goal.
	 * Returns null if a plan could not be found, or a list of the actions
	 * that must be performed, in order, to fulfill the goal.
	 */
	public void plan(GameObject agent,
								  HashSet<GoapAction> availableActions, 
	                              HashSet<KeyValuePair<string,object>> worldState, 
	                              HashSet<KeyValuePair<string,object>> goal) 
	{
		// reset the actions so we can start fresh with them
		foreach (GoapAction a in availableActions) {
			a.doReset ();
		}

		// check what actions can run using their checkProceduralPrecondition
		HashSet<GoapAction> usableActions = new HashSet<GoapAction> ();
		foreach (GoapAction a in availableActions) {
			if ( a.checkProceduralPrecondition(agent) )
				usableActions.Add(a);
		}
		
		// we now have all actions that can run, stored in usableActions

		// build up the tree and record the leaf nodes that provide a solution to the goal.
		List<Node> leaves = new List<Node>();

		// build graph
		Node start = new Node (null, 0, worldState, null);

        List<ActionInformation> actionInfo = new List<ActionInformation>();
        int i = 0;
        foreach (var item in usableActions)
        {
            ActionInformation info = new ActionInformation();

            string o = "Type: " + item;
            Debug.Log(o);
            string output = o.Split('(', ')')[1];
            info.type = output;
            Debug.Log(output);
            info.preconditions = item.Preconditions;
            info.effects = item.Effects;
            info.cost = item.cost;
            info.targetID = item.target.GetInstanceID();
            Debug.Log(item.target.GetInstanceID());
            actionInfo.Add(info);
        }

        // Send a message with usable actions, goal and ID to python via a public method from the goap agent
        GoapRequestObject requestObject = new GoapRequestObject(agent.GetComponent<GoapAgent>().ID, actionInfo, worldState, goal);
        string json = JsonConvert.SerializeObject(requestObject);
        agent.GetComponent<GoapAgent>().SendPlanRequestToServer(json);
	}

	/**
	 * Returns true if at least one solution was found.
	 * The possible paths are stored in the leaves list. Each leaf has a
	 * 'runningCost' value where the lowest cost will be the best action
	 * sequence.
	 */
	private bool buildGraph (Node parent, List<Node> solutionPaths, HashSet<GoapAction> usableActions, HashSet<KeyValuePair<string, object>> goal)
	{
		bool foundOne = false;

		// go through each action available at this node and see if we can use it here
		foreach (GoapAction action in usableActions) {

			// if the parent state has the conditions for this action's preconditions, we can use it here
			if ( MatchAllPreconditions(action.Preconditions, parent.state) ) {

				// apply the action's effects to the parent state
				HashSet<KeyValuePair<string,object>> currentState = populateState (parent.state, action.Effects);
				//Debug.Log(GoapAgent.prettyPrint(currentState));
				Node node = new Node(parent, parent.runningCost+action.cost, currentState, action);

				if (MatchAllPreconditions(goal, currentState)) {
					// we found a solution!
					solutionPaths.Add(node);
					foundOne = true;
				} else {
					// not at a solution yet, so test all the remaining actions and branch out the tree
					HashSet<GoapAction> subset = actionSubset(usableActions, action);
					bool found = buildGraph(node, solutionPaths, subset, goal);
					if (found)
						foundOne = true;
				}
			}
		}

		return foundOne;
	}

	/**
	 * Create a subset of the actions excluding the removeMe one. Creates a new set.
	 */
	private HashSet<GoapAction> actionSubset(HashSet<GoapAction> actions, GoapAction removeMe) {
		HashSet<GoapAction> subset = new HashSet<GoapAction> ();
		foreach (GoapAction a in actions) {
			if (!a.Equals(removeMe))
				subset.Add(a);
		}
		return subset;
	}

	/**
	 * Check that all items in 'test' are in 'state'. If just one does not match or is not there
	 * then this returns false.
	 */
	private bool MatchAllPreconditions(HashSet<KeyValuePair<string,object>> test, HashSet<KeyValuePair<string,object>> state) {
		bool allMatch = true;
		foreach (KeyValuePair<string,object> t in test) {
			bool match = false;
			foreach (KeyValuePair<string,object> s in state) {
				if (s.Equals(t)) {
					match = true;
					break;
				}
			}
			if (!match)
				allMatch = false;
		}
		return allMatch;
	}
	
	/**
	 * Apply the stateChange to the currentState
	 */
	private HashSet<KeyValuePair<string,object>> populateState(HashSet<KeyValuePair<string,object>> currentState, HashSet<KeyValuePair<string,object>> stateChange) {
		HashSet<KeyValuePair<string,object>> state = new HashSet<KeyValuePair<string,object>> ();
		// copy the KVPs over as new objects
		foreach (KeyValuePair<string,object> s in currentState) {
			state.Add(new KeyValuePair<string, object>(s.Key,s.Value));
		}

		foreach (KeyValuePair<string,object> change in stateChange) {
			// if the key exists in the current state, update the Value
			bool exists = false;

			foreach (KeyValuePair<string,object> s in state) {
				if (s.Equals(change)) {
					exists = true;
					break;
				}
			}

			if (exists) {
				state.RemoveWhere( (KeyValuePair<string,object> kvp) => { return kvp.Key.Equals (change.Key); } );
				KeyValuePair<string, object> updated = new KeyValuePair<string, object>(change.Key,change.Value);
				state.Add(updated);
			}
			// if it does not exist in the current state, add it
			else {
				state.Add(new KeyValuePair<string, object>(change.Key,change.Value));
			}
		}
		return state;
	}
}

/**
 * Used for building up the graph and holding the running costs of actions.
 */
 [System.Serializable]
public class Node
{
    public Node parent;
    public float runningCost;
    public HashSet<KeyValuePair<string, object>> state;
    public GoapAction action;

    public Node(Node parent, float runningCost, HashSet<KeyValuePair<string, object>> state, GoapAction action)
    {
        this.parent = parent;
        this.runningCost = runningCost;
        this.state = state;
        this.action = action;
    }
}

[System.Serializable]
public class GoapRequestObject
{
    public int ID;
    public List<ActionInformation> actions;
    public HashSet<KeyValuePair<string, object>> worldState;
    public HashSet<KeyValuePair<string, object>> goal;

    public GoapRequestObject(int _ID, List<ActionInformation> _actions, HashSet<KeyValuePair<string, object>> _worldState, HashSet<KeyValuePair<string, object>> _goal) 
    {
        ID = _ID;
        actions = _actions;
        worldState = _worldState;
        goal = _goal;
    }
}

[System.Serializable]
public class ActionInformation
{
    public string type;
    public HashSet<KeyValuePair<string, object>> preconditions;
    public HashSet<KeyValuePair<string, object>> effects;
    public float cost;
    public int targetID;

    public ActionInformation(HashSet<KeyValuePair<string, object>> _preconditions, HashSet<KeyValuePair<string, object>> _effects, float _cost, int _targetID)
    {
        preconditions = _preconditions;
        effects = _effects;
        cost = _cost;
        targetID = _targetID;
    }

    public ActionInformation()
    {

    }
}







