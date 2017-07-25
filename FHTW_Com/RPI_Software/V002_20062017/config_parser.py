__author__ = "Markus Lechner"
__license__ = "GPL"
__version__ = "0.0.2"
__maintainer__ = "Markus Lechner"
__email__ = "markus.lechner@technikum-wien.at"
__status__ = "Production"

import array
import logging
import configparser

class SPI_Config: 
    DEFAULT_SPI_CLK = 500000   
    def __init__(self):
        self.spi_clk = 0
        return
        
    def read_spi_config_file(self):
        self.spi_config = configparser.ConfigParser()
        self.spi_config.read("spi_config.ini")
        
    def assemble_spi_config_array(self):           
        spi_setting = self.assemble_section_map('SPI_CONFIG')    
        
        self.spi_clk = int(spi_setting.get('spi_clock'))
        spi_clk = spi_setting.get('spi_clock')
        spi_clk_zero = len(spi_clk) - len(spi_clk.rstrip('0'))
        spi_clk_val = int(spi_clk)/(10**spi_clk_zero)
        
        if spi_clk_val > 255:
            logging.error("Invalid SPI-Clock Configuration, \
                            Default Value(500kHz) will be used!")
            self.spi_clk = self.DEFAULT_SPI_CLK
            spi_clk_val = 5
            spi_clk_zero = 5
        
        spi_config_array = []
        spi_config_array.append(int(spi_clk_val))
        spi_config_array.append(spi_clk_zero)
        return spi_config_array
        
    def assemble_section_map(self, section):
        spi_dict = {}        
        options = self.spi_config.options(section)
        
        for option in options:
            try:
                spi_dict[option] = self.spi_config.get(section, option)
                if spi_dict[option] == -1:
                   logging.error("Invalid SPI Configuration, Skip: %s" % option)
            except:
                spi_dict[option] = None
        return spi_dict
#EOF