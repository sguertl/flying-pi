__author__ = "Markus Lechner"
__license__ = "GPL"
__version__ = "0.0.2"
__maintainer__ = "Markus Lechner"
__email__ = "markus.lechner@technikum-wien.at"
__status__ = "Production"

import re
import socket
import logging

class TCP_IP_CLIENT:
    IP_ADDRESS = "169.254.0.1"
    PORT = 30000
    REC_BUFFER_SIZE = 64
    
    CLIENT_INIT_MSG = "Raspi Client\n\r"
    CLIENT_CTRL_DATA_REQ = "CtrlReq\n\r"
    
    def init_socket(self):
        print("Connecting to Server...")
        logging.info("Connecting to Server...")
        
        # Create a TCP/IP socket
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        # Connect the socket to the port on the server
        server_address = (self.IP_ADDRESS, self.PORT)
        self.sock.connect(server_address)
        
        print("Connected to %s, Port: %s" % server_address)
        logging.info("TCP_IP_CLIENT: Connected to %s, Port: %s" % server_address)
        #Unfortunately, Matlab does not receive the very first frame
        #Thus, an init frame is sent to trigger the communication
        self.send_init_message()
        return
    
    def send_init_message(self):
            message = self.CLIENT_INIT_MSG
            try:
                self.sock.send(message.encode('utf-8'))
                return 0
            except:
                self.close_connection()
                return -1
        
    def get_controlling_data(self):
        try:
            #Send Request to Server to receive Controlling Data
            message = self.CLIENT_CTRL_DATA_REQ
            self.sock.send(message.encode('utf-8'))
            
            #Receive Controlling Data from Server        
            ctrl_data = self.sock.recv(self.REC_BUFFER_SIZE)
            
            #Format Packet for further data processing
            ctrl_data = float(ctrl_data)
            return ctrl_data
        except:
            self.close_connection()
            return None
    
    def send_sensor_data(self, sensor_data):
        #Transmit Sensor Data to Server for further processing
        message = str(sensor_data) + "\n\r"       
        
        try:
            self.sock.send(message.encode('utf-8'))
            return    
        except:
            self.close_connection()
            return None       
        
    def close_connection(self): 
        self.sock.close()
        print("Raspi-Client Closed")
        logging.info("Raspi-Client Closed")
        return
#EOF