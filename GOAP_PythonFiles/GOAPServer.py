import time
import zmq
import json
import collections

class Node(object):
    def __init__(self, parent, runningCost, state, action):
        self.parent = parent
        self.runningCost = runningCost
        self.state = state
        self.action = action


class ActionInformation(object):
    def __init__(self, preconditions, effects, cost):
        self.preconditions = preconditions
        self.effects = effects
        self.cost = cost


class JsonObjectInterp(object):
    def __init__(self, j):
        self.__dict__ = json.loads(j)


class GoapResponseObject(object):
    def __init__(self, solutionList, ID):
        self.solutionList = solutionList
        self.ID = ID


def MatchAllPreconditions(precon, state):
    allMatch = True

    for (key, value) in precon.items():
        match = False
        for key2, value2 in state.items():
            if key == key2 and value == value2:
                match = True

        if not match:
            allMatch = False

    #print(allMatch)
    return allMatch


def PopulateState(currState, stateChange):
    temp = currState.copy()
    temp.update(stateChange)
    return temp


def ActionSubset(actions, toRemove):
    temp = actions.copy()
    temp.remove(toRemove)
    return temp


def BuildGraph(parent, solutions, usableActions, goal, success):
    #success = False

    # iterate over actions available in the current node, to see if they are usable
    for action in usableActions:
        print(success)

        #print(action.preconditions)
        #print(parent.state)
        if MatchAllPreconditions(action.preconditions, parent.state):
            print("The preconditions passed")
            currentState = PopulateState(parent.state, action.effects)

            node = Node(parent, parent.runningCost + action.cost, currentState, action)

            if MatchAllPreconditions(goal, currentState):
                print("goal and state match")
                solutions.append(node)
                success = True

            else:
                subset = ActionSubset(usableActions, action)

                tempBool = False
                subSuccess = BuildGraph(node, solutions, subset, goal, tempBool)

                if subSuccess:
                    success = True

    return success


# jobj = json.dumps(testobj.__dict__)
context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5555")
print("bound socket to 5555")

while True:
    #  Wait for next request from client
    print("Waiting to receive message")
    message = socket.recv()

    time.sleep(0.1)  # Small delay to wait for the texture to be made
    print("Message received:")
    print(message)

    obj = JsonObjectInterp(message)
    ID = obj.ID

    # actions
    usableActions = []
    worldState = dict()
    goal = {}
    cost = 1

    i = 0
    while i < len(obj.actions):
        preconditions = {}
        effects = {}

        preTemp = dict(obj.actions[i].items())["preconditions"]
        j = 0
        while j < len(preTemp):
            temp = dict()
            keyVals = []
            for k, v in preTemp[j].items():
                keyVals.append(v)

            m = 0
            while m < len(keyVals) / 2:
                temp[keyVals[m]] = keyVals[m + 1]
                m += 1

            copy = preconditions.copy()
            if copy is None:
                preconditions.update(temp)
            else:
                preconditions.update(temp)
                preconditions.update(copy)
            j += 1

        preEffects = dict(obj.actions[i].items())["effects"]
        j = 0
        while j < len(preEffects):
            temp = dict()
            keyVals = []
            for k, v in preEffects[j].items():
                keyVals.append(v)

            m = 0
            while m < len(keyVals) / 2:
                temp[keyVals[m]] = keyVals[m + 1]
                m += 1

            copy = effects.copy()
            if copy is None:
                effects.update(temp)
            else:
                effects.update(temp)
                effects.update(copy)
            j += 1

        cost = obj.actions[i]["cost"]
        usableActions.append(ActionInformation(preconditions, effects, cost))
        i += 1

    # Unpack and format world state and Goal correctly - key value pairs in c# will be interpreted as dict with two
    # entries with "Key" as the first key and "Value" as the second.... (yeah i spent 4hrs debugging this...)
    n = 0
    while n < len(obj.worldState):
        temp = dict()
        keyVals = []
        for k, v in obj.worldState[n].items():
            keyVals.append(v)

        m = 0
        while m < len(keyVals)/2:
            temp[keyVals[m]] = keyVals[m + 1]
            m += 1

        copy = worldState.copy()
        if copy is None:
            worldState.update(temp)
        else:
            worldState.update(temp)
            worldState.update(copy)

        #print("World state dict: ", worldState)
        n += 1

    n = 0
    while n < len(obj.goal):
        temp = dict()
        keyVals = []
        for k, v in obj.goal[n].items():
            keyVals.append(v)

        m = 0
        while m < len(keyVals) / 2:
            temp[keyVals[m]] = keyVals[m + 1]
            m += 1

        copy = goal.copy()
        if copy is None:
            goal.update(temp)
        else:
            goal.update(temp)
            goal.update(copy)

        #print("Goal dict: ", worldState)
        n += 1

    # print(type(worldState))
    # now we have our objects

    start = Node(None, 0, worldState, None)
    solutions = []

    for act in usableActions:
        print("precondition: ", act.preconditions)
        print("Effect: ", act.effects)
        print("Cost: ", act.cost)

    print("WorldState: ", worldState)
    print("Goal: ", goal)
    print()

    tempBool = False
    success = BuildGraph(start, solutions, usableActions, goal, tempBool)

    print("success: ", success)

    result = 0
    cheapest = None
    if success:
        result = []
        for sol in solutions:
            if cheapest is None:
                cheapest = sol
            else:
                if sol.runningCost < cheapest.runningCost:
                    cheapest = sol

        node = cheapest
        while node.parent is not None:
            if node.action is not None:
                result.insert(0, node.action)
            node = node.parent
        print("Result:")
        print(result)

    if result is not 0:
        response = GoapResponseObject(result, ID)
        jsonResponse = json.dumps(response, default=lambda o: o.__dict__,
            sort_keys=True, indent=4)

        print(jsonResponse)

        socket.send_string(jsonResponse)
        print("Sent back message")
        print()
    else:
        print("Failed to make response list")
        socket.send_string("Failed to make plan")
