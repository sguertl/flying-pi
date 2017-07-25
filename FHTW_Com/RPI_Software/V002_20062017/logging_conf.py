from _stat import filemode
__author__ = "Markus Lechner"
__license__ = "GPL"
__version__ = "0.0.2"
__maintainer__ = "Markus Lechner"
__email__ = "markus.lechner@technikum-wien.at"
__status__ = "Production"

import logging

class Logging_Config:
    def config_logger(self):        
        logging.basicConfig(filename='logging.log', \
                            level=logging.DEBUG, \
                            filemode = 'w', \
                            format='%(asctime)s - %(levelname)s - %(message)s')
        
        #Print Info Message
        logging.info('Logger Initialized')
        return
#EOF