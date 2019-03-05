import time
import zmq
#import cv2 # Emil kunne ikke køre det mens det her er der; noget med openCV?  Det virker i hvertfald uden!

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
    print("Sent back message")

