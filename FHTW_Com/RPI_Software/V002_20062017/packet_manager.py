__author__ = "Markus Lechner"
__license__ = "GPL"
__version__ = "0.0.2"
__maintainer__ = "Markus Lechner"
__email__ = "markus.lechner@technikum-wien.at"
__status__ = "Production"

import array
import types
import struct
import collections

class packet_assembler:    
    COMMAND_COM_INIT = 0x01
    COMMAND_SPIC = 0x02
    COMMAND_NACK = 126
    COMMAND_ACK = 127   
    COMMAND_SPI_CV = 0x01
    
    PACKET_SIZE_COM_INIT = 3
    PACKET_SIZE_SPIC = 3
    PACKET_SIZE_NACK = 3
    PACKET_SIZE_ACK = 2   

    XMC_IDENDIFIER = 0xAF
    
    def assemblePacketComInit(self):
        checksum_calc = Checksum_Verification()  
        checksum = checksum_calc.make_checksum(self.XMC_IDENDIFIER)  
        packetComInit = bytearray([self.COMMAND_COM_INIT,   \
                                   self.XMC_IDENDIFIER,     \
                                   checksum])       
        return packetComInit
    
    def assemblePacketSPIconfig(self, spi_config):
        checksum_calc = Checksum_Verification()  
        checksum = checksum_calc.make_checksum(spi_config)        
        packetSPIconfig = array.array('B', [self.COMMAND_SPIC])
                      
        for index in range(len(spi_config)):
            packetSPIconfig.extend([spi_config[index]])

        packetSPIconfig.extend([checksum])
        
        return packetSPIconfig
    
    def assemblePacketACK(self):
        checksum_calc = Checksum_Verification()  
        checksum = checksum_calc.make_checksum(0)
        packetACK = bytearray([self.COMMAND_ACK, checksum])
        return packetACK

    def assemblePacketNACK(self, errorCode):
        checksum_calc = Checksum_Verification()  
        checksum = checksum_calc.make_checksum(errorCode)
        packetNACK = bytearray([self.COMMAND_NACK, errorCode, checksum])
        return packetNACK   
    
    def assemble_spi_frame(self, payload):
        spi_payload = bytearray(struct.pack("f", payload))  
                   
        payload_size = len(spi_payload)
        
        spi_frame = []
        spi_frame.append(payload_size)    
        
        for index in range(payload_size):
            spi_frame.append(spi_payload[index]) 
   
        checksum_calc = Checksum_Verification()   
        spi_frame.append(checksum_calc.make_checksum(spi_frame))
        spi_frame.insert(0, self.COMMAND_SPI_CV) 
        
        return spi_frame    
    
class Checksum_Verification:
    CHECKSUM_VALUE = 0xFF
    
    def make_checksum(self, data):
        checksum = 0
        
        if isinstance(data, collections.Iterable):    
            for index in range(len(data)):
                checksum = checksum + data[index]
        else:
            checksum = data
        
        checksum = self.CHECKSUM_VALUE - (checksum & 0xFF)
        return checksum
    
    def verify_checksum(self, data, recChecksum):
        calcChecksum = self.make_checksum(data)
        
        if calcChecksum == recChecksum:
            return True
        else:
            return False
        return
#EOF