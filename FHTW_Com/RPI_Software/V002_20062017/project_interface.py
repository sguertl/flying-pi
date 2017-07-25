__author__ = "Markus Lechner"
__license__ = "GPL"
__version__ = "0.0.2"
__maintainer__ = "Markus Lechner"
__email__ = "markus.lechner@technikum-wien.at"
__status__ = "Production"

import sys
import time
import logging
from app_status import Thread_Application_Status 
from spi_com import SPI_Communication 
from uart_com import uart_communication
from config_parser import SPI_Config
from packet_manager import packet_assembler
from controller_processing import Controller_Processing                           

class Project_Interface:
    SPI_DATA_IND = 0x02
    SPI_TRANSMISSION_DELAY = .001
    
    def __init__(self, COM_MODE):
        self.controller_proc = Controller_Processing(COM_MODE)
    
    def configure_SPI_interface(self):
        print("Starting SPI RaspberryPI Configuration...")
        logging.info("Starting SPI RaspberryPI Configuration...")
        
        spi_config = SPI_Config()
        spi_config.read_spi_config_file()
        self.spi_config_data = spi_config.assemble_spi_config_array()        

        self.int_spi = SPI_Communication()
        self.int_spi.init_interface(spi_config.spi_clk)
        
        print("SPI RaspberryPI Configuration Done")
        logging.info("SPI RaspberryPI Configuration Done")        
        return 0
    
    def configure_UART_interface(self):
        print("Starting UART Configuration...")
        logging.info("Starting UART Configuration...")
        
        self.int_uart = uart_communication()
        self.int_uart.init_serial()
        
        print("UART Configuration Done")
        logging.info("UART Configuration Done")        
        return 0
    
    def start_device_detection(self):
        print("Starting Device Detection...")
        logging.info("Starting Device Detection...")
        
        if self.int_uart.device_detection() != 0:
            self.close_all_interfaces()
            print("Device detection failed!")    
            logging.info("Device detection failed!")            
            return -1
            
        print("Device detection was successful")
        logging.info("Device detection was successful")        
        return 0
    
    def start_SPI_data_transmission(self):        
        #if self.int_uart.send_SPI_config(self.spi_config_data) != 0:            
            #self.close_all_interfaces()            
            #return -1
        
        print("Starting SPI Data transmission...")
        logging.info("Starting SPI Data transmission...")
        self.update_app_status_thread()
        
        spi_frame = []
        
        #It is necessary to send 0.0 as controller value for the very
        #first time, due to sensor data are not available at the beginning
        #of the data transmission
        self.int_spi.send_spi_frame(0.0)
        spi_frame = self.int_spi.receive_spi_frame(self.SPI_DATA_IND)
        self.controller_proc.set_controller_param(spi_frame)
        
        while True:          
            #Get controlled values  
            ctrl_data = self.controller_proc.get_controlled_param()   
            #Send these data to the microcontroller over SPI               
            self.int_spi.send_spi_frame(ctrl_data)
                        
            #Receive sensor data from the microcontroller over SPI    
            spi_frame = self.int_spi.receive_spi_frame(self.SPI_DATA_IND)
            #These sensor data are transmitted to the controller
            self.controller_proc.set_controller_param(spi_frame)
            
            #Delay according to the max. sampling rate of the sensors
            time.sleep(self.SPI_TRANSMISSION_DELAY)   
         
        self.close_all_interfaces()         
        return -1
    
    def close_all_interfaces(self):
        self.int_uart.close_connection()
        self.int_spi.close_connection()      
        return 0
    
    def init_app_status_thread(self):        
        try:
            self.app_status = Thread_Application_Status("Thread-AppStatus")
        except:
            print("Application-Status-Thread could not be created! Ignore green LED!")
            logging.info("Application-Status-Thread could not be created")            
        return   
    
    def update_app_status_thread(self):
        self.app_status.set_app_status(Thread_Application_Status.SYS_INIT_DONE, True)        
        return

    def terminate_app_status_thread(self):
        self.app_status.stop_app_status_thread(True)        
        return
    
    def close_application(self, status): 
        self.close_all_interfaces()
        self.app_status.stop_app_status_thread(True)  
        print("Application terminated", status)
        logging.info("Application terminated - " + status)
        sys.exit()     
#EOF