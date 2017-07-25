__author__ = "Markus Lechner"
__license__ = "GPL"
__version__ = "0.0.2"
__maintainer__ = "Markus Lechner"
__email__ = "markus.lechner@technikum-wien.at"
__status__ = "Production"

import sys
import time
import array
import serial
import logging
from packet_manager import packet_assembler
from packet_manager import Checksum_Verification
from controller_processing import Controller_Processing

class uart_communication:
    CHECKSUM_ERROR = 1
    MAX_RETRANSMIT = 3   
     
    RES_RETRANSMIT = 100
    ERR_TIMEOUT = 200
    ERR_TARGET_UNREACHABLE = 201
    DEVICE_DETECTED = 300
    
    def init_serial(self):
        self.serialDevice = serial.Serial('/dev/serial0', 9600)
        
    def close_connection(self):   
        self.serialDevice.close()
        
    def device_detection(self):
        packetAssembler = packet_assembler()
        packetComInt = packetAssembler.assemblePacketComInit()        
        return self.packet_transmission(packetComInt, False)

    def send_SPI_config(self, spi_config):
        packetAssembler = packet_assembler()
        packetSPIconfig = packetAssembler.assemblePacketSPIconfig(spi_config)
        return self.packet_transmission(packetSPIconfig, True)
     
    def packet_transmission(self, packet, isTimeout):
        timeout_index = 0
        retransmission_app = -1

        while True:
            self.serialDevice.write(packet)
            read_byte = self.serialDevice.inWaiting()
            
            if read_byte:
                read_packet = array.array('B', self.serialDevice.read(read_byte))
                ret = self.parse_packet(read_packet)
                
                if ret == self.RES_RETRANSMIT:
                    retransmission_app += 1 
                    
                    if retransmission_app >= self.MAX_RETRANSMIT:
                        self.serialDevice.close()
                        print("Unable to connect to target - Max retransmit exceeded!")
                        logging.critical("Unable to connect to target - Max retransmit exceeded!")
                        return self.ERR_TARGET_UNREACHABLE
                    else:
                        continue
                else:
                    break            
            elif isTimeout:
                if timeout_index >= 250:
                    print("Transmission time out - Target did not response")
                    logging.critical("Transmission time out - Target did not response")
                    return self.ERR_TIMEOUT                
                else:
                    timeout_index += 50
                    time.sleep(.05)
                    continue
            else:
                time.sleep(1)
                continue
        return 0
        
    def parse_packet(self, packet):
        command = packet[0]
        
        if command == packet_assembler.COMMAND_ACK:
            if len(packet) >= packet_assembler.PACKET_SIZE_ACK:

                #Calculate checksum and verify
                checksum_verify = Checksum_Verification()                
                isValid = checksum_verify.verify_checksum(0, packet[1]) 
                
                if isValid == False:
                    logging.warn("UART-Checksum Error, data will be discarded")
                    return self.RES_RETRANSMIT
                else: 
                    return 0
            else:
                logging.warn("UART: Invalid data received, data will be discarded")
                return self.RES_RETRANSMIT
            
        elif command == packet_assembler.COMMAND_NACK:
            if len(packet) >= packet_assembler.PACKET_SIZE_NACK:
                
                #Calculate checksum and verify
                checksum_verify = Checksum_Verification()                
                isValid = checksum_verify.verify_checksum(packet[1], packet[2]) 
                
                if isValid == False:
                    logging.warn("UART-Checksum Error, data will be discarded")
                    return self.RES_RETRANSMIT
                                
                ret = packet_error_handling(errorCode)
                return ret
        else:
            logging.warn("UART: Invalid data received, data will be discarded")
            return self.RES_RETRANSMIT
        
    def packet_error_handling(errorCode):
            if errorCode == self.CHECKSUM_ERROR:
                return self.RES_RETRANSMIT
            else: #TODO CHECK OTHER ERROR CODES
               return self.RES_RETRANSMIT 
#EOF