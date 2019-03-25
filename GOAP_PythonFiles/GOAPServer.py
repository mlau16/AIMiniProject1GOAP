import time
import zmq
import json


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


def MatchAllPreconditions(precon, state):
    allMatch = True

    #for pre in precon:
        #match = False
        #for sta in state:
            #print(type(pre))
            #print(type(sta))


    #for pk, pv in pitems:
       # match = False
       # for sk, sv in sitems:
          #  if pk == sk:
              #  if pv == sv:
                  #  match = True
                 #   break
       # if match is False:
           # allMatch = False

    # print(allMatch)
    return allMatch


def PopulateState(currState, stateChange):
    print(type(currState))
    print(type(stateChange))
    for s in currState:
        if s not in stateChange:
            stateChange.update(s)
    return stateChange


def ActionSubset(actions, toRemove):
    return actions.remove(toRemove)


def BuildGraph(parent, solutions, usableActions, goal):
    success = False

    # iterate over actions available in the current node, to see if they are usable
    for action in usableActions:

        if MatchAllPreconditions(action.preconditions, parent.state):

            currentState = PopulateState(parent.state, action.effects)

            node = Node(parent, parent.runningCost + action.cost, currentState, action)

            if MatchAllPreconditions(goal, currentState):
                solutions.append(node)
                success = True

            else:
                subset = ActionSubset(usableActions, action)
                subSuccess = BuildGraph(node, solutions, subset, goal)

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
    print("Waiting to recieve message")
    message = socket.recv()

    time.sleep(0.1)  # Small delay to wait for the texture to be made
    print("Message recieved:")
    print(message)

    obj = JsonObjectInterp(message)
    ID = obj.ID

    # actions
    usableActions = []
    preconditions = {}  # obj.actions["preconditions"]
    effects = {}
    worldState = {}
    cost = 1

    # print(type(usableActions))

    for action in obj.actions:
        preconditions.update(action["preconditions"])
        effects.update(action["effects"])
        cost = action["cost"]
        usableActions.append(ActionInformation(preconditions, effects, cost))

    # worldstate and goal
    for s in obj.worldState:
        worldState.update(s)

    goal = obj.goal

    # print(type(worldState))
    # now we have our objects

    start = Node(None, 0, worldState, None)
    solutions = []

    success = BuildGraph(start, solutions, usableActions, goal)

    # print(success)

    cheapest = None
    if success:
        for sol in solutions:
            if cheapest is None:
                cheapest = sol
            else:
                if sol.runningCost < cheapest.runningCost:
                    cheapest = sol

        result = []
        node = cheapest
        while node.parent is not None:
            if node.action is not None:
                [node.action] + result

            node = node.parent
        print("Result:")
        print(result)

    socket.send_string("pong")
    print("Sent back message")
    print()
