__author__ = "Markus Lechner"
__license__ = "GPL"
__version__ = "0.0.2"
__maintainer__ = "Markus Lechner"
__email__ = "markus.lechner@technikum-wien.at"
__status__ = "Production"

import re
import collections
from eth_client_com import TCP_IP_CLIENT  

class Controller_Processing:
    COM_MODE_EXT = 0  
    COM_MODE_ETH = 1
    COM_MODE = -1
    
    def __init__(self, COM_MODE):
        self.COM_MODE = COM_MODE
        
        if self.COM_MODE == self.COM_MODE_ETH:
            self.tcp_ip_client = TCP_IP_CLIENT()
            self.tcp_ip_client.init_socket()
            
        elif self.COM_MODE == self.COM_MODE_EXT:      
            with open('/mnt/RAMDisk/rpi_in.txt', 'w') as f: 
                f.write("0.0, 0.0")
            with open('/mnt/RAMDisk/rpi_out.txt', 'w') as f: 
                f.write("0.0")
                    
    def get_controlled_param(self):        
        if self.COM_MODE == self.COM_MODE_EXT:        
            with open('/mnt/RAMDisk/rpi_out.txt', 'r') as f:
                ctrld_value = float(f.readline())
                return ctrld_value
        elif self.COM_MODE == self.COM_MODE_ETH:
            return self.tcp_ip_client.get_controlling_data()        
        
    def set_controller_param(self, param):   
        if self.COM_MODE == self.COM_MODE_EXT: 
            with open('/mnt/RAMDisk/rpi_in.txt', 'w') as f:   
                f.write(self.format_controller_param(param)) 
        elif self.COM_MODE == self.COM_MODE_ETH:          
            self.tcp_ip_client.send_sensor_data(self.format_controller_param(param))  
        return
    
    def format_controller_param(self, param):
        param_form = []        
        
        if isinstance(param, collections.Iterable):   
            for index in range(len(param)): 
                param_unform = str(param[index])                    
                param_form.append(re.sub('[(),]', '', param_unform))
            return ', '.join(param_form)   
        else:
            return param   
#EOF