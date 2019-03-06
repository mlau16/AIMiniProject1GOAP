import time
import zmq
import json


class TestObject(object):
    def __init__(self):
        self.x = 5
        self.y = 6
        self.z = "hello world"
        self.p = 5.0
        self.q = False


print("hello world")

testobj = TestObject()
jobj = json.dumps(testobj.__dict__)
print(jobj)
context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5555")
print("bound socket to 5555")

while True:
    #  Wait for next request from client
    print("Waiting to recieve message")
    message = socket.recv()
    time.sleep(0.1)  # Small delay to wait for the texture to be made
    print("server ready to recieve data")
    socket.send(message)
    socket.send(jobj)
    print("Sent back message")

