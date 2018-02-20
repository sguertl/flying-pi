import socket
import sys
import errno
import serial
import array

from threading import Thread

class rpi_connection:
    # Constants
    HOST = ''
    PORT = 5050
    DEBUG = False
	CONTROL_BYTES = 19
	LOG_BYTES = 41

    def init(self):
        self.init_wifi_socket()
        self.bind_wifi_socket()
        self.init_serial_socket()
        self.writeThread = Thread(target = self.write_drone)
        self.readThread = Thread(target = self.read_drone)
        self.writeThread.start()

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
        except serial.serialutil.SerialExcpetion:
            print('Serial connection establishment failed')
        
    # Start listening on socket
    def write_drone(self):
        self.wifi_socket.listen(1)
        print('Socket now listening')

        # Keep talking with the client
        while True:

            # Wait to accept a connection
            self.conn, addr = self.wifi_socket.accept()
            print('Connected with ' + addr[0] + ':' + str(addr[1]))
            if self.readThread.is_alive() == False:
                self.readThread.start()
            # Infinite loop so that function does not terminate
            while True:
                try:
                    # Receive data from client
                    data = array.array('B', self.conn.recv(self.CONTROL_BYTES))
                    # Check if connection is alive
                    if not data:
                        print('Connection closed')
                        break
                    self.serial_socket.write(data)
                    self.serial_socket.flush()
                    
                except socket.error as msg:
                    if msg.errno != errno.ECONNRESET:
                        raise
                    print('Connection reset by peer')
                    pass
                except serial.serialutil.SerialTimeoutException:
                    print('Serial connection error')
                    break
            self.close()

    def read_drone(self):
        while True:
            read_packet = array.array('B', self.serial_socket.read(self.LOG_BYTES))
            try:
                self.conn.send(read_packet)
            except IOError as e:
                if e.errno == errno.EPIPE:
                    print('Client disconnected without receiving all data')
                    self.close()
        self.close()

    # Closes the wifi connection
    def close(self):
        self.conn.close()
        count = 0
        print('end of line')
#EOF
