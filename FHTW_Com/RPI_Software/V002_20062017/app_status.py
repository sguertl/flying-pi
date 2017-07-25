__author__ = "Markus Lechner"
__license__ = "GPL"
__version__ = "0.0.2"
__maintainer__ = "Markus Lechner"
__email__ = "markus.lechner@technikum-wien.at"
__status__ = "Production"

import time
import logging
import threading
import RPi.GPIO as GPIO

class Thread_Application_Status(threading.Thread):
    SYS_INIT_DONE = 1    
    LED_BLINKING_DELAY = 0.1
    
    def __init__(self, name):
        threading.Thread.__init__(self)
        self.stop_event = threading.Event()
        self.deamon = True
        self.configure_LED_GPIO()
        self.start()
        
    def run(self):
        logging.info("Thread-App-Status(ID: #%s) started" % self.ident)
        while not self.stop_event.is_set():
            GPIO.output(47, GPIO.LOW)
            time.sleep(self.LED_BLINKING_DELAY)
            GPIO.output(47, GPIO.HIGH)
            time.sleep(self.LED_BLINKING_DELAY)    
        return
    
    def stop_app_status_thread(self, isGPIO_CleanUp):        
        if self.is_alive():
            self.stop_event.set()
            self.join()
            logging.info("Thread-App-Status(ID: #%s) terminated" % self.ident)
        if isGPIO_CleanUp == True:
            GPIO.cleanup()                       
        return
    
    def configure_LED_GPIO(self):
        GPIO.setmode(GPIO.BCM)
        GPIO.setup(47, GPIO.OUT)        
        return
    
    def set_app_status(self, stopThread, status_update):
        if stopThread == True:
            self.stop_app_status_thread(False)
            
        if status_update == self.SYS_INIT_DONE:
            GPIO.output(47, GPIO.HIGH)
        return
#EOF