
from rpi_socket import rpi_connection

rpi_conn = rpi_connection()

rpi_conn.init_wifi_socket()
rpi_conn.bind_wifi_socket()
rpi_conn.init_serial_socket()

rpi_conn.listen()
