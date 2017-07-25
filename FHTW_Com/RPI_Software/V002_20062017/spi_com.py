__author__ = "Markus Lechner"
__license__ = "GPL"
__version__ = "0.0.2"
__maintainer__ = "Markus Lechner"
__email__ = "markus.lechner@technikum-wien.at"
__status__ = "Production"

import spidev
import time
import array
import struct
import logging

from packet_manager import packet_assembler
from packet_manager import Checksum_Verification
from controller_processing import Controller_Processing

class SPI_Communication:
    COMMAND_SPI_SD = 0x02
    
    def init_interface(self, spi_clk):
        self.spi_device = spidev.SpiDev()
        self.spi_device.open(0,0)
        self.spi_device.max_speed_hz = spi_clk
        self.spi_device.mode = 0b01        
        return
    
    def close_connection(self):
        self.spi_device.close()
        return
        
    def send_spi_frame(self, ctrl_data):              
        pa_spi_frame = packet_assembler()        
        spi_frame_cv = pa_spi_frame.assemble_spi_frame(ctrl_data)           

        #Send SPI-Frame Controlled Value from Matlab Controller    
        for index in range(0, len(spi_frame_cv)):
            self.spi_device.cshigh = True
            self.spi_device.writebytes([spi_frame_cv[index]])
            resp = self.spi_device.readbytes(1)
            self.spi_device.cshigh = False       
        return        
            
    def receive_spi_frame(self, command):
        spi_frame_sd = []
        
        #Receive SPI-Frame Sensor Data (Command and Payload Size first)     
        for index in range(0, 2):
            self.spi_device.cshigh = True
            self.spi_device.writebytes([command])
            spi_frame_sd.extend(self.spi_device.readbytes(1))
            self.spi_device.cshigh = False

        #Receive SPI-Frame Sensor Data (Payload and Checksum(+1))     
        for index in range(spi_frame_sd[1] + 1):
            self.spi_device.cshigh = True
            self.spi_device.writebytes([command])
            spi_frame_sd.extend(self.spi_device.readbytes(1))
            self.spi_device.cshigh = False
                    
        return self.verify_and_format_frame(spi_frame_sd)

    def verify_and_format_frame(self, spi_frame_sd):        
        payload_split = []
        
        if len(spi_frame_sd) <= 0x02:
            logging.warn("SPI: Invalid data received, data will be discarded")    
            return None
        
        if spi_frame_sd[0] == self.COMMAND_SPI_SD:

            #Checksum verification
            checksum_verify = Checksum_Verification()
            
            ind_pay_end = spi_frame_sd[1] + 2            
            isValid = checksum_verify.verify_checksum(spi_frame_sd[1:ind_pay_end], \
                                                      spi_frame_sd[ind_pay_end])
            
            if isValid == False:
                logging.warn("SPI-Checksum Error, data will be discarded")    
                return None        

            offset = 0
            value_cnt = int(spi_frame_sd[1] / 4)

            for index_out in range(value_cnt):      
                payload_single = []        
                for index_in in range(0, 4):
                    payload_single.insert(index_in, spi_frame_sd[2 + index_in + offset])

                offset += 4                               
                payload_split.insert(index_out, struct.unpack("f", bytes(payload_single)))
            return payload_split
        else:
            logging.warn("SPI: Invalid command received, data will be discarded")   
        return None        
#EOF