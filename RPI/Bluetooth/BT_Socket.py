import struct
import array
import serial
from bluetooth import *

server_sock = BluetoothSocket(RFCOMM)
server_sock.bind(("", PORT_ANY))
server_sock.listen(1)

port = server_sock.getsockname()[1]

uuid = "94f39d29-7d6d-437d-973b-fba39e49d4ee"

advertise_service(server_sock, "DronePi", service_classes = [ uuid, SERIAL_PORT_CLASS ], profiles = [SERIAL_PORT_PROFILE ])

serial_socket = serial.Serial('/dev/serial0', 115200, timeout = 3000)
print('Serial socket is open: ' + str(serial_socket.isOpen()))

try:
    while True:
        print('Waiting for connection on RFCOMM channel %d' % port)

        client_sock, client_info = server_sock.accept()
        print('Accepted connection from ', client_info)
        try:
            while True:
                data = array.array('B', client_sock.recv(19))
                if not data:
                    print('Connection closed')
                    break
                serial_socket.write(data)
                serial_socket.flush()
        except:
            client_sock.close()
except:
    server_sock.close()
    serial_socket.close()
#EOF
