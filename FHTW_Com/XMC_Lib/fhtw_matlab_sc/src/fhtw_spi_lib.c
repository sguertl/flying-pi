/**
 * @file     fhtw_spi_lib.c
 * @version  V0.1
 * @date     June 2017
 * @author   Patrick Schmitt, Roman Beneder
 *
 * @brief   FHTW Matlab showcase XMC4500 SPI library
 *
 */
#ifdef FHTW_MATLAB_SC
#include <fhtw_spi_lib.h>

/******************************************************************** GLOBALS */
uint8_t fhtw_spi_rx_temp_buffer[fhtw_spi_BUFFER_SIZE_RX] = {0};
uint8_t fhtw_spi_tx_temp_buffer[fhtw_spi_BUFFER_SIZE_TX] = {0};

float fhtw_control_r = 0.0; //global variable to store r value of AngleRateController()
float fhtw_control_y = 0.0; //global variable to store y value of AngleRateController()
float fhtw_RPi_result = 0;

/**
 * @brief SPI configuration structure
 */
XMC_SPI_CH_CONFIG_t fhtw_spi_config =
{
		.baudrate = 25000,
		.bus_mode = XMC_SPI_CH_BUS_MODE_SLAVE,
		.selo_inversion = XMC_SPI_CH_SLAVE_SEL_INV_TO_MSLS,
		.parity_mode = XMC_USIC_CH_PARITY_MODE_NONE
};

/**
 * @brief GPIO configuration structure
 */
XMC_GPIO_CONFIG_t fhtw_gpio_config;

/**
 * @brief SPI CMD "Sensor Data" structure
 */
FHTW_SPI_SD_PACKET fhtw_spi_tx_frame =
{
		.fhtw_spi_CMD_SD = FHTW_SPI_CMD_SD,
		.fhtw_spi_paysize_SD = 8
};

/**
 * @brief SPI CMD "Controlled Value" structure
 */
FHTW_SPI_CV_PACKET fhtw_spi_rx_frame;

/*!
 *  @brief This function configures the SPI interface to communicate with the Raspberry Pi
 *  @param none
 *  @return none
 */
void fhtw_spi_init(void)
{
	/*Initialize and Start SPI*/
	XMC_SPI_CH_Init(SPI_USIC_MODULE, &fhtw_spi_config);

	/*GPIO configuration*/
	XMC_GPIO_SetMode(SPI_MOSI, XMC_GPIO_MODE_INPUT_TRISTATE);
	XMC_GPIO_SetMode(SPI_SCLK, XMC_GPIO_MODE_INPUT_TRISTATE);
	XMC_GPIO_SetMode(SPI_SS, XMC_GPIO_MODE_INPUT_TRISTATE);
	XMC_GPIO_SetMode(SPI_MISO, XMC_GPIO_MODE_OUTPUT_PUSH_PULL_ALT1);

	/* Enabling events for TX FIFO and RX FIFO */
	XMC_USIC_CH_RXFIFO_EnableEvent(SPI_USIC_MODULE,XMC_USIC_CH_RXFIFO_EVENT_CONF_STANDARD |
			XMC_USIC_CH_RXFIFO_EVENT_CONF_ALTERNATE);

	XMC_SPI_CH_SetInputSource(SPI_USIC_MODULE,XMC_SPI_CH_INPUT_DIN0,SPI_USIC_MOSI);
	XMC_SPI_CH_SetInputSource(SPI_USIC_MODULE,XMC_SPI_CH_INPUT_SLAVE_SCLKIN,USIC2_C1_DX1_P3_6);
	XMC_SPI_CH_SetInputSource(SPI_USIC_MODULE,XMC_SPI_CH_INPUT_SLAVE_SELIN,USIC2_C1_DX2_P4_1);

	XMC_SPI_CH_SetWordLength(SPI_USIC_MODULE,8);
	XMC_SPI_CH_SetFrameLength(SPI_USIC_MODULE,8);
	XMC_SPI_CH_SetTransmitMode(SPI_USIC_MODULE,XMC_SPI_CH_MODE_STANDARD);

	XMC_SPI_CH_EnableEvent(SPI_USIC_MODULE, XMC_SPI_CH_EVENT_STANDARD_RECEIVE);

	/* Connecting the previously enabled events to a Service Request line number */
	XMC_USIC_CH_SetInterruptNodePointer(SPI_USIC_MODULE,
			XMC_USIC_CH_INTERRUPT_NODE_POINTER_RECEIVE, 1);

	XMC_SPI_CH_Start(SPI_USIC_MODULE);

	NVIC_SetPriority(FHTW_SPI_INTERRUPT_SC,4U);
	NVIC_EnableIRQ(FHTW_SPI_INTERRUPT_SC);
}

/*!
 *  @brief ISR for receiving SPI data from the RPI. Performs one complete SPI data transfer.
 *  @param none
 *  @return none
 */
void USIC2_1_IRQHandler(void) {

	static int i = 0;

	NVIC_DisableIRQ(FHTW_SPI_INTERRUPT_SC); //disable IRQ to prevent multiple interrupts during processing

	i++;
	if(i == 2){	//workaround for multiple interrupts
		i = 0;
		NVIC_EnableIRQ(FHTW_SPI_INTERRUPT_SC);
		return;
	}
	fhtw_spi_update_txdata();	//pack sensor values + prepare send buffer
	fhtw_spi_get_slave_frame(SPI_USIC_MODULE); //perform SPI transfer
	fhtw_spi_update_rxdata();	//unpack RPI result
	NVIC_EnableIRQ(FHTW_SPI_INTERRUPT_SC);
}

/*!
 *  @brief This function writes data to a specific SPI channel
 *  @param channel ... SPI channel
 *		   spi_data .. byte which should be transmitted
 *  @return none
 */
void fhtw_spi_transmit_word(XMC_USIC_CH_t *const channel, uint8_t spi_data)
{
	XMC_ASSERT("XMC_USIC_CH_Enable: channel not valid", XMC_USIC_IsChannelValid(channel));

	XMC_SPI_CH_Transmit(channel, spi_data, XMC_SPI_CH_MODE_STANDARD);
	while((XMC_SPI_CH_GetStatusFlag(channel) & XMC_SPI_CH_STATUS_FLAG_TRANSMIT_SHIFT_INDICATION) == 0U);
	XMC_SPI_CH_ClearStatusFlag(channel, XMC_SPI_CH_STATUS_FLAG_TRANSMIT_SHIFT_INDICATION);

}

/*!
 *  @brief This function reads data from a specific SPI channel
 *  @param channel ... SPI channel
 *  @return byte which was received and is valid
 */
uint8_t fhtw_spi_receive_word(XMC_USIC_CH_t *const channel)
{
	XMC_ASSERT("XMC_USIC_CH_Enable: channel not valid", XMC_USIC_IsChannelValid(channel));

	return XMC_SPI_CH_GetReceivedData(channel);
}

/*!
 *  @brief This function performs a complete SPI data transfer with the RPI. XMC acts as SPI slave.
 *  @param channel ... SPI channel
 *  @return on success this function returns SPI_TRANSFER_OK (0)
 */
uint8_t fhtw_spi_get_slave_frame(XMC_USIC_CH_t *const channel) {

	static _Bool spi_cmd_ok = false;
	static _Bool frame_recv_complete = false;
	static _Bool frame_send_complete = false;
	static _Bool spi_cmd_payload_recv = false;
	static _Bool spi_cmd_payload_send = false;

	char spi_data = 0;
	char fhtw_spi_slave_temp_buffer[fhtw_spi_BUFFER_SIZE_RX] = {0};

	//receive controlled value from RPI
	while(!frame_recv_complete){

		if(!spi_cmd_ok && (fhtw_spi_rx_temp_buffer[0] != FHTW_SPI_CMD_CV)){
			spi_data = fhtw_spi_receive_word(channel); //receive CMD byte
			fhtw_spi_transmit_word(channel,spi_data);
			fhtw_spi_receive_word(channel); //2 times needed because of 0x00 insertion
			fhtw_spi_transmit_word(channel,spi_data);
			fhtw_spi_rx_temp_buffer[0] = spi_data;

			spi_cmd_ok = true;
		}

		if(spi_cmd_ok && !spi_cmd_payload_recv){
			spi_data = fhtw_spi_receive_word(channel); //receive payload-size byte
			fhtw_spi_transmit_word(channel,spi_data);
			fhtw_spi_receive_word(channel); //2 times needed because of 0x00 insertion
			fhtw_spi_transmit_word(channel,spi_data);
			fhtw_spi_rx_temp_buffer[1] = spi_data;

			spi_cmd_payload_recv = true;

		}

		if(spi_cmd_ok && spi_cmd_payload_recv){

			for(int i = 2; i < fhtw_spi_rx_temp_buffer[1] + 3; i++){
				fhtw_spi_rx_temp_buffer[i] = fhtw_spi_receive_word(channel); //receive payload
				fhtw_spi_transmit_word(channel,fhtw_spi_rx_temp_buffer[i]);
				fhtw_spi_receive_word(channel);	//2 times needed because of 0x00 insertion
				fhtw_spi_transmit_word(channel,fhtw_spi_rx_temp_buffer[i]);
			}
			frame_recv_complete = true;
			spi_cmd_ok = false;
			spi_cmd_payload_recv = false;
		}
	}
	//send sensor values from XMC
	while(!frame_send_complete){

		if(!spi_cmd_ok && (fhtw_spi_slave_temp_buffer[0] != FHTW_SPI_CMD_CV)){
			spi_data = fhtw_spi_receive_word(channel);
			fhtw_spi_transmit_word(channel,spi_data); //send CMD byte
			fhtw_spi_receive_word(channel); //2 times needed because of 0x00 insertion
			fhtw_spi_transmit_word(channel,spi_data);
			fhtw_spi_slave_temp_buffer[0] = spi_data;

			spi_cmd_ok = true;
		}

		if(spi_cmd_ok && !spi_cmd_payload_send){
			spi_data = fhtw_spi_receive_word(channel);
			fhtw_spi_transmit_word(channel,fhtw_spi_tx_temp_buffer[1]); //send payload-size byte
			fhtw_spi_receive_word(channel); //2 times needed because of 0x00 insertion
			fhtw_spi_transmit_word(channel,fhtw_spi_tx_temp_buffer[1]);
			fhtw_spi_slave_temp_buffer[1] = spi_data;

			spi_cmd_payload_send = true;
		}

		if(spi_cmd_ok && spi_cmd_payload_send){

			for(int i = 2; i < fhtw_spi_tx_temp_buffer[1] + 3; i++){
				fhtw_spi_slave_temp_buffer[i] = fhtw_spi_receive_word(channel);
				fhtw_spi_transmit_word(channel,fhtw_spi_tx_temp_buffer[i]); //send payload
				fhtw_spi_receive_word(channel);	//2 times needed because of 0x00 insertion
				fhtw_spi_transmit_word(channel,fhtw_spi_tx_temp_buffer[i]);
			}
			frame_send_complete = false;
			spi_cmd_ok = false;
			frame_recv_complete = false;
			spi_cmd_payload_send = false;
			return SPI_TRANSFER_OK;
		}
	}

	return 1;
}

/*!
 *  @brief This function unpacks data from the RPI and stores the received value in fhtw_RPi_result.
 *  @param none
 *  @return on success this function returns 0
 */
uint8_t fhtw_spi_update_rxdata(void) {

	uint8_t rx_checksum = 0;

	memcpy(&fhtw_spi_rx_frame, &fhtw_spi_rx_temp_buffer, sizeof(FHTW_SPI_CV_PACKET));

	rx_checksum = fhtw_SPI_make_checksum(fhtw_spi_rx_temp_buffer,fhtw_spi_rx_frame.fhtw_spi_paysize_CV);

	memset(&fhtw_spi_rx_temp_buffer, 0x00, fhtw_spi_BUFFER_SIZE_RX);

	if(rx_checksum == fhtw_spi_rx_frame.fhtw_spi_checksum){
		fhtw_RPi_result = fhtw_spi_rx_frame.fhtw_spi_payload_CV;
	}

	return 0;
}

/*!
 *  @brief This function packs the sensor values for the RPI and stores the transmit ready data into a buffer.
 *  @param none
 *  @return on success this function returns 0
 */
uint8_t fhtw_spi_update_txdata(void) {

	fhtw_spi_tx_frame.fhtw_spi_payload_SD[0] = fhtw_control_r;
	fhtw_spi_tx_frame.fhtw_spi_payload_SD[1] = fhtw_control_y;

	memset(&fhtw_spi_tx_temp_buffer, 0x00, fhtw_spi_BUFFER_SIZE_TX);
	memcpy(fhtw_spi_tx_temp_buffer, &fhtw_spi_tx_frame, sizeof(FHTW_SPI_SD_PACKET) - 1);

	fhtw_spi_tx_temp_buffer[fhtw_spi_tx_frame.fhtw_spi_paysize_SD + 2] = fhtw_SPI_make_checksum(fhtw_spi_tx_temp_buffer,fhtw_spi_tx_frame.fhtw_spi_paysize_SD);

	return 0;
}

/*!
 *  @brief This function stores IMU sensor values into global variables.
 *  @param none
 *  @return none
 */
void fhtw_spi_push_ctr_param(float r, float y){

	fhtw_control_r = r;
	fhtw_control_y = y;
}

/*!
 *  @brief This function returns the unpacked RPI controlled value.
 *  @param none
 *  @return float value of the RPI controller result
 */
float fhtw_spi_pop_ctr_result(void){

	float ret = fhtw_RPi_result;

	return ret;
}


/*!
 *  @brief This function generates a checksum for the SPI data transfer
 *  @param structure storing the frame data. length of the payload
 *  @return uint16_t calculated checksum
 */
uint16_t fhtw_SPI_make_checksum(uint8_t *frame, uint8_t length) {
	uint16_t frame_data_sum = 0;
	//Calculate sum used for the checksum
	//Sum all bytes in the frame data part

	for (uint8_t i = 1; i < length + 2; i++) {
		frame_data_sum += frame[i];
	}
	frame_data_sum = CHECKSUM_VALUE - (uint8_t)frame_data_sum;
	//Store the result and keep only the 8 LSB's
	return frame_data_sum;
}

#endif
