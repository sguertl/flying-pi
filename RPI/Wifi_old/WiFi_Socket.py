import socket
import sys
from thread import *

HOST = ''   # Symbolic name meaning all available interfaces
PORT = 5050 # Abritray non-privileged port

s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
print('Socket created')

# Bind socket to local host and port
try:
    s.bind((HOST, PORT))
except socket.error as msg:
    print('Bind failed. Error code: ' + str(msg[0]) + ' Message: ' msg[1])
    sys.exit()

print('Socket bind complete')

# Start listening on socket
s.listen(10)
print('Socket now listening')

# Now keep talking with the client
while 1:
    # Wait to accept a connection - blocking call
    conn, addr = s.accept()
    print('Connected with ' + addr[0] + ':' + str(addr[1]))

    # Sending message to connected client
    conn.send('Welcome!\n')
    
    # Infinite loop so that function does not terminate
    while True:
        # Receiving data from client
        data = conn.recv(19)
        print('Client: ' + data)

s.close()