
using UnityEngine;
using System.Collections.Generic;

public class GoapAction : MonoBehaviour {

    
	public HashSet<KeyValuePair<string,object>> preconditions;
	public HashSet<KeyValuePair<string,object>> effects;

	private bool inRange = false;

	/* The cost of performing the action. 
	 * Figure out a weight that suits the action. 
	 * Changing it will affect what actions are chosen during planning.*/
	public float cost = 1f;

    /**
	 * An action often has to perform on an object. This is that object. Can be null. */
    public int targetID;
	public GameObject target;

	public GoapAction() {
		preconditions = new HashSet<KeyValuePair<string, object>> ();
		effects = new HashSet<KeyValuePair<string, object>> ();
	}

	public void doReset() {
		inRange = false;
		target = null;
		reset ();
	}

    /**
	 * Reset any variables that need to be reset before planning happens again.
	 */
    public virtual void reset() { }

    /**
	 * Is the action done?
	 */
    public virtual bool isDone() { return false; }

    /**
	 * Procedurally check if this action can run. Not all actions
	 * will need this, but some might.
	 */
    public virtual bool checkProceduralPrecondition(GameObject agent) { return false; }

	/**
	 * Run the action.
	 * Returns True if the action performed successfully or false
	 * if something happened and it can no longer perform. In this case
	 * the action queue should clear out and the goal cannot be reached.
	 */
	public virtual bool perform(GameObject agent) { return false; }

	/**
	 * Does this action need to be within range of a target game object?
	 * If not then the moveTo state will not need to run for this action.
	 */
	public virtual bool requiresInRange () { return false; }
	

	/**
	 * Are we in range of the target?
	 * The MoveTo state will set this and it gets reset each time this action is performed.
	 */
	public virtual bool isInRange () {
		return inRange;
	}
	
	public virtual void setInRange(bool inRange) {
		this.inRange = inRange;
	}


	public virtual void addPrecondition(string key, object value) {
		preconditions.Add (new KeyValuePair<string, object>(key, value) );
	}


	public virtual void removePrecondition(string key) {
		KeyValuePair<string, object> remove = default(KeyValuePair<string,object>);
		foreach (KeyValuePair<string, object> kvp in preconditions) {
			if (kvp.Key.Equals (key)) 
				remove = kvp;
		}
		if ( !default(KeyValuePair<string,object>).Equals(remove) )
			preconditions.Remove (remove);
	}


	public virtual void addEffect(string key, object value) {
		effects.Add (new KeyValuePair<string, object>(key, value) );
	}


	public virtual void removeEffect(string key) {
		KeyValuePair<string, object> remove = default(KeyValuePair<string,object>);
		foreach (KeyValuePair<string, object> kvp in effects) {
			if (kvp.Key.Equals (key)) 
				remove = kvp;
		}
		if ( !default(KeyValuePair<string,object>).Equals(remove) )
			effects.Remove (remove);
	}

	
	public HashSet<KeyValuePair<string, object>> Preconditions {
		get {
			return preconditions;
		}
	}

	public HashSet<KeyValuePair<string, object>> Effects {
		get {
			return effects;
		}
	}
}