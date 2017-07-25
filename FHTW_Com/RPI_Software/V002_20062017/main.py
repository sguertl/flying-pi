#!/usr/bin/python3
__author__ = "Markus Lechner"
__license__ = "GPL"
__version__ = "0.0.2"
__maintainer__ = "Markus Lechner"
__email__ = "markus.lechner@technikum-wien.at"
__status__ = "Production"

import sys
import logging

from logging_conf import Logging_Config
from project_interface import Project_Interface
from controller_processing import Controller_Processing   

print("***RaspberryPI Mathworks ShowCase***")
print("***Starting Application!***")

logging_config = Logging_Config()
logging_config.config_logger()

#Set COM_MODE_EXT for RPI-Extension Mode
#Set COM_MODE_ETH for Matlab Ethernet Mode
project_interface = Project_Interface(Controller_Processing.COM_MODE_ETH)
project_interface.init_app_status_thread()

try:
    project_interface.configure_SPI_interface()
    project_interface.configure_UART_interface()
    if project_interface.start_device_detection() != 0:
        project_interface.close_application("Error")
        
    if project_interface.start_SPI_data_transmission() !=  0:
        project_interface.close_application("Error")
    
except KeyboardInterrupt:
    project_interface.close_application("KeyboardInterrupt")

#EOF
