import socket
import sys
import errno
import serial

class rpi_connection:
    # Constants
    HOST = ''
    PORT = 5050

    # Create wifi socket
    def init_wifi_socket(self):
        self.wifi_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        print('Socket created')

    # Bind socket to local host and port
    def bind_wifi_socket(self):
        try:
            self.wifi_socket.bind((self.HOST, self.PORT))
        except socket.error as msg:
            print('Bind failed. Error code: ' + str(msg[0]) + ' - Message: ' + msg[1])
            sys.exit()
        print('Socket bind complete')

    def init_serial_socket(self):
        try:
            self.serial_socket = serial.Serial('/dev/serial0', 115200, timeout = 3000)
            print('Serial socket is open: ' + str(self.serial_socket.isOpen()))
        except serial.SerialExcpetion:
            print('Serial connection establishment failed')
        
    # Start listening on socket
    def listen(self):
        self.wifi_socket.listen(1)
        print('Socket now listening')

        # Keep talking with the client
        while True:

            # Wait to accept a connection
            conn, addr = self.wifi_socket.accept()
            print('Connected with ' + addr[0] + ':' + str(addr[1]))

            # Infinite loop so that function does not terminate
            while True:
                try:
                    # Receive data from client
                    data = conn.recv(19)
                    self.serial_socket.write(data)
                    self.serial_socket.flush()
                    print(data)
                    # Check if connection is alive
                    if not data:
                        print('Connection closed')
                        break
                    # conn.send('Test')
                except socket.error as msg:
                    if msg.errno != errno.ECONNRESET:
                        raise
                    print('Connection reset by peer')
                    pass
                except serial.SerialException:
                    print('Serial connection error')
                    break
        close()

    # Closes the wifi connection
    def close(self):
        self.wifi_socket.close()
        self.serial_socket.close()
#EOF
