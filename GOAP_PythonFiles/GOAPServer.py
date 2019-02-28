import time
import zmq
import cv2

context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5500")

while True:
    #  Wait for next request from client
    message = socket.recv()

    time.sleep(0.1)  # Small delay to wait for the texture to be made

    print("server ready to recieve data")

    print("server wrote to image")
    socket.send(b"pong")


    #  In the real world usage, you just need to replace time.sleep() with
    #  whatever work you want python to do.
    time.sleep(1)

    #  Send reply back to client
    #  In the real world usage, after you finish your work, send your output here
    socket.send(b"World")