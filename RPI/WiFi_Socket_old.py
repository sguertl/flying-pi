### This version is used for multiclients and python 3.4.x ###

import socket
import sys
from _thread import *

HOST = ''   # Symbolic name meaning all available interfaces
PORT = 5050 # Abritray non-privileged port

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
print('Socket created')

# Bind socket to local host and port
try:
    s.bind((HOST, PORT))
except socket.error as msg:
    print('Bind failed. Error code: ' + str(msg[0]) + ' Message: ' (msg[1]))
    sys.exit()

print('Socket bind complete')

# Start listening on socket
s.listen(10)
print('Socket now listening')

# Function for handling connections, this will be used to create threads
def clientthread(conn):
    # Sending message to connected client
    conn.send('Welcome!\n') # .send only takes strings

    # Infinite loop so that function does not terminate and thread does not end
    while True:
        # Receiving data from client
        data = conn.recv(1024)
        reply = 'OK ...' + data
        conn.sendall(reply)

# Now keep talking with the client
while 1:
    # Wait to accept a connection - blocking call
    conn, addr = s.accept()
    print('Connected with ' + addr[0] + ':' + str(addr[1]))

    # Start a new thread that takes 1st argument as function name to be run, 2nd argument is the tuple of args given to the function
    start_new_thread(clientthread, (conn,))

s.close()