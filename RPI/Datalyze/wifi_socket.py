import socket
import sys
import errno
import array
import datetime

from WifiDataCheck import WifiData

HOST = ''
PORT = 5050

def getSystemTimeMillis():
    return int((datetime.datetime.utcnow()-datetime.datetime(1970,1,1)).total_seconds() * 1000)

# Create wifi socket
wifi_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
print('Socket created')

# Bind socket to local host and port
try:
    wifi_socket.bind((HOST, PORT))
except socket.error as msg:
    print('Bind failed. Error code: ' + str(msg[0]) + ' - Message: ' + msg[1])
    sys.exit()

print('Socket bind complete')

# Start listening on socket
wifi_socket.listen(1)
print('Socket now listening')

# Keep talking with the client
while True:

    # Wait to accept a connection
    conn, addr = wifi_socket.accept()
    print('Connected with ' + addr[0] + ':' + str(addr[1]))
    oldmillis = 0
    length = 2
    data_list = []
    # Infinite loop so that function does not terminate
    while True:
        try:
            # Receive data from client
            data = array.array('B', conn.recv(length))
            # Check if connection is alive
            if not data:
                print('Connection closed')
                break
            if data[0] == 10:
                print(data)
                currentmillis = getSystemTimeMillis()
                difmillis = currentmillis - oldmillis
                oldmillis = currentmillis
                calcchecksum = data[1]
                count = 2
                while(count < length - 4):
                    calcchecksum ^= data[count]
                    count = count + 1
                incchecksum = (data[length-4] << 24 | data[length-3] << 16 | data[length-2] << 8 | data[length-1])
                isRight = calcchecksum == incchecksum
                bandwidth = round(length / (difmillis / 1000.0), 2)
                data_list.append(WifiData(isRight, difmillis, bandwidth))
            elif data[0] == 11:
                i = len(data_list) - 1
                while(i > -1):
                    send_str = data_list[i].getAll()
                    if i == 0:
                        send_str = send_str + "#"
                    conn.send(send_str)
                    data_list.pop()
                    i = i - 1
            elif data[0] == 1:
                length = data[1]
                oldmillis = getSystemTimeMillis()
                
        except socket.error as msg:
            if msg.errno != errno.ECONNRESET:
                raise
            print('Connection reset by peer')
            pass
wifi_socket.close()
