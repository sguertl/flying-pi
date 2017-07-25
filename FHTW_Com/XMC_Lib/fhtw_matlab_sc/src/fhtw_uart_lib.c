/*
 * fhtw_uart_lib.c
 *
 *  Created on: 22.05.2017
 *      Author: Patrick Schmitt, Markus Lechner
 */
#ifdef FHTW_MATLAB_SC
/******************************************************************** LIBS */
#include <stdio.h>
#include <stdarg.h>
#include <errno.h>

#include <xmc_uart.h>
#include <xmc_gpio.h>

#include <fhtw_uart_lib.h>
/******************************************************************** DEFINES */

/******************************************************************** GLOBALS */
_Bool fhtw_uart_str_available = false;
_Bool fhtw_uart_data_available = false;
char fhtw_uart_rx_buffer[fhtw_uart_BUFFER_SIZE_RX] = {0};
char fhtw_uart_rx_data;
uint8_t rx_ctr = 0;
/******************************************************************** STRUCTURES */
XMC_GPIO_CONFIG_t fhtw_uart_tx =
{
		.mode = fhtw_uart_TX_GPIO_mode,
		.output_strength = XMC_GPIO_OUTPUT_STRENGTH_MEDIUM
};

XMC_GPIO_CONFIG_t fhtw_uart_rx =
{
		.mode = XMC_GPIO_MODE_INPUT_TRISTATE
};

XMC_UART_CH_CONFIG_t fhtw_uart_config =
{
		.data_bits = 8U,
		.stop_bits = 1U,
		.baudrate = 9600U
};

/**
 * @brief UART CMD "Communication Init" structure
 */
FHTW_UART_COMIN_PACKET fhtw_uart_rx_COMIN;

/**
 * @brief UART CMD "SPI Configuration" structure
 */
FHTW_UART_SPIC_PACKET fhtw_uart_rx_SPIC;

/******************************************************************** FUNCTIONS */

/** \name fhtw_uart_init
 * \param none
 * \return none
 *
 * \brief This function initializes the UART interface of the XMC4500
 * Furthermore it initializes the RX & TX FIFO of the UART interface, sets the correct
 * input function for the GPIO pins and enables the RX interrupt service routine for
 * the UART interface.
 **/
void fhtw_uart_init(void) {
	/* USIC channels initialization */
	XMC_UART_CH_Init(fhtw_uart_module, &fhtw_uart_config);

	XMC_UART_CH_SetInputSource(fhtw_uart_module, XMC_UART_CH_INPUT_RXD, fhtw_uart_USIC_RX);

	/* FIFOs initialization for both channels:
	 *  8 entries for TxFIFO from point 0, LIMIT=1
	 *  8 entries for RxFIFO from point 8, LIMIT=7 (SRBI is set if all 8*data has been received)
	 *  */
	XMC_USIC_CH_TXFIFO_Configure(fhtw_uart_module, 0, XMC_USIC_CH_FIFO_DISABLED, fhtw_uart_TX_FIFO_INITIAL_LIMIT);
	XMC_USIC_CH_RXFIFO_Configure(fhtw_uart_module, 0, XMC_USIC_CH_FIFO_SIZE_16WORDS, fhtw_uart_RX_FIFO_INITIAL_LIMIT);

	/* Enabling events for TX FIFO and RX FIFO */
	XMC_USIC_CH_RXFIFO_EnableEvent(fhtw_uart_module,XMC_USIC_CH_RXFIFO_EVENT_CONF_STANDARD |
			XMC_USIC_CH_RXFIFO_EVENT_CONF_ALTERNATE);

	/* Connecting the previously enabled events to a Service Request line number */
	XMC_USIC_CH_RXFIFO_SetInterruptNodePointer(fhtw_uart_module,XMC_USIC_CH_RXFIFO_INTERRUPT_NODE_POINTER_STANDARD,0);
	XMC_USIC_CH_RXFIFO_SetInterruptNodePointer(fhtw_uart_module,XMC_USIC_CH_RXFIFO_INTERRUPT_NODE_POINTER_ALTERNATE,0);

	XMC_UART_CH_EnableEvent(fhtw_uart_module, XMC_UART_CH_EVENT_STANDARD_RECEIVE);

	/* Start USIC operation as UART */
	XMC_UART_CH_Start(fhtw_uart_module);

	/*Initialization of the necessary ports*/
	XMC_GPIO_Init(fhtw_uart_TX,&fhtw_uart_tx);
	XMC_GPIO_Init(fhtw_uart_RX,&fhtw_uart_rx);

	/*Configuring priority and enabling NVIC IRQ for the defined Service Request line number */
	NVIC_SetPriority(fhtw_uart_interrupt_sc,3U);
	NVIC_EnableIRQ(fhtw_uart_interrupt_sc);

	return;
}

#if UART_CONF_U0C0_HW_OLD

/** \function USIC0_0_IRQHandler
 * \param none
 * \return none
 *
 * \brief This interrupt service routine is called, if the UART interface receives
 * a character.
 **/
void USIC0_0_IRQHandler(void) {

	uint8_t uart_rx_cnt = 0;

	while(!XMC_USIC_CH_RXFIFO_IsEmpty(fhtw_uart_module)) {

		fhtw_uart_rx_buffer[uart_rx_cnt] = XMC_UART_CH_GetReceivedData(fhtw_uart_module);

		if(uart_rx_cnt < (fhtw_uart_BUFFER_SIZE_RX - 1)){
			uart_rx_cnt++;
		}
		else
		{
			uart_rx_cnt = 0;
		}
	}
	if((uart_rx_cnt > 1) && ((fhtw_uart_rx_buffer[0] == FHTW_UART_CMD_COMIN) || (fhtw_uart_rx_buffer[0] == FHTW_UART_CMD_SPIC))){
		fhtw_uart_rx_unpack(uart_rx_cnt);
	}
}

#endif

#if UART_CONF_U2C0_HW_NEW

/** \function USIC2_0_IRQHandler
 * \param none
 * \return none
 *
 * \brief This interrupt service routine is called, if the UART interface receives
 * a character.
 **/
void USIC2_0_IRQHandler(void) {

	uint8_t uart_rx_cnt = 0;

	fhtw_uart_rx_buffer[uart_rx_cnt] = XMC_UART_CH_GetReceivedData(fhtw_uart_module);
	uart_rx_cnt++;

	while(!XMC_USIC_CH_RXFIFO_IsEmpty(fhtw_uart_module)) {

		fhtw_uart_rx_buffer[uart_rx_cnt] = XMC_UART_CH_GetReceivedData(fhtw_uart_module);

		if(uart_rx_cnt < (fhtw_uart_BUFFER_SIZE_RX - 1)){
			uart_rx_cnt++;
		}
		else
		{
			uart_rx_cnt = 0;
		}

	}
	//check if first byte is a valid CMD and more than two bytes were received
	if((uart_rx_cnt > 1) && ((fhtw_uart_rx_buffer[0] == FHTW_UART_CMD_COMIN) || (fhtw_uart_rx_buffer[0] == FHTW_UART_CMD_SPIC))){
		fhtw_uart_rx_unpack(uart_rx_cnt);	//unpack UART frame
	}
}

#endif

/** \function fhtw_uart_send_char
 * \param char c
 * \return always 0
 *
 * \brief This function sends a given character via the UART interface to the
 * workstation.
 **/
uint8_t fhtw_uart_send_char(char c) {
	while(XMC_USIC_CH_GetTransmitBufferStatus(fhtw_uart_module) == XMC_USIC_CH_TBUF_STATUS_BUSY);
	XMC_UART_CH_Transmit(fhtw_uart_module, c);

	return 0;
}

/** \function fhtw_uart_printf
 * \param char *fmt
 * \return 0 on success or ENOMEM on failure
 *
 * \brief This function prints a formatted string to the UART interface and transmits
 * it to the workstation.
 **/
uint8_t fhtw_uart_printf(char *fmt, ...){
	va_list arg_ptr;
	char buffer[fhtw_uart_BUFFER_SIZE_PRINTF];

	if(fmt == NULL)  {
		return ENOMEM;
	}

	va_start(arg_ptr, fmt);
	vsprintf(buffer, fmt, arg_ptr);
	va_end(arg_ptr);

	fhtw_uart_send_string(buffer);
	return 0;
}

/** \function fhtw_uart_send_string
 * \param char *str
 * \return 0 on success or ENOMEM on failure
 *
 * \brief This function takes a given input string and sends it via the
 *UART interface to the workstation.
 **/
uint8_t fhtw_uart_send_string(char *str) {
	if(str == NULL) {
		return ENOMEM;
	}

	for(int i=0;i<strlen(str);i++) {
		while(XMC_USIC_CH_GetTransmitBufferStatus(fhtw_uart_module) == XMC_USIC_CH_TBUF_STATUS_BUSY);
		XMC_UART_CH_Transmit(fhtw_uart_module, str[i]);
	}
	return 0;
}

/** \function fhtw_uart_rx_unpack
 * \param uint8_t uart_rx_size
 * \return 0 on success or 1 on failure
 *
 * \brief This function checks the received UART frame and sends ACK/NACK as reply
 **/
uint8_t fhtw_uart_rx_unpack(uint8_t uart_rx_size){

	uint8_t rx_checksum = 0;

	//check if a "Communication Init – UART" package was received
	if((fhtw_uart_rx_buffer[0] == FHTW_UART_CMD_COMIN) && uart_rx_size >= 2){

		memcpy(&fhtw_uart_rx_COMIN, &fhtw_uart_rx_buffer, sizeof(FHTW_UART_COMIN_PACKET)); //copy content to struct

		rx_checksum = fhtw_UART_make_checksum(fhtw_uart_rx_buffer,1); //calculate checksum

		memset(&fhtw_uart_rx_buffer, 0x00, fhtw_uart_BUFFER_SIZE_RX);

		XMC_USIC_CH_RXFIFO_Flush(fhtw_uart_module);

		if(rx_checksum == fhtw_uart_rx_COMIN.fhtw_uart_checksum){
			fhtw_uart_send_string("\x7F\xFF"); //send ACK message if checksum was correct
		}
		else{
			fhtw_uart_send_string("\x7E\x01\xFE"); //send NACK message
		}

		return 0;
	}
	//check if a "SPI Configuration – UART" package was received
	else if((fhtw_uart_rx_buffer[1] == FHTW_UART_CMD_SPIC) && uart_rx_size >= 3){
		memcpy(&fhtw_uart_rx_SPIC, &fhtw_uart_rx_buffer, sizeof(FHTW_UART_SPIC_PACKET));

		rx_checksum = fhtw_UART_make_checksum(fhtw_uart_rx_buffer,2);

		memset(&fhtw_uart_rx_buffer, 0x00, fhtw_uart_BUFFER_SIZE_RX);

		XMC_USIC_CH_RXFIFO_Flush(fhtw_uart_module);

		if(rx_checksum == fhtw_uart_rx_SPIC.fhtw_uart_checksum){
			fhtw_uart_send_string("\x7F\xFF"); //currently the XMC only acknowledge this message, SPI config not necessary
											   //because RPi configured as SPI Master (due to Python restrictions)
		}
		else{
			fhtw_uart_send_string("\x7E\x01\xFE");
		}

		return 0;
	}
	else if(uart_rx_size >= 3){ //no valid data send NACK and abort
		memset(&fhtw_uart_rx_buffer, 0x00, fhtw_uart_BUFFER_SIZE_RX);

		XMC_USIC_CH_RXFIFO_Flush(fhtw_uart_module);

		fhtw_uart_send_string("\x7E\x01\xFE");
		return 0;
	}
	else
		return 1;
}

/*!
 *  @brief This function generates a checksum for the UART data transfer
 *  @param structure storing the frame data. length of the payload
 *  @return uint16_t calculated checksum
 */
uint16_t fhtw_UART_make_checksum(uint8_t *frame, uint8_t length) {
	uint16_t frame_data_sum = 0;
	//Calculate sum used for the checksum
	//Sum all bytes in the frame data part

	for (uint8_t i = 1; i < length + 1; i++) {
		frame_data_sum += frame[i];
	}
	frame_data_sum = CHECKSUM_VALUE - (uint8_t)frame_data_sum;
	//Store the result and keep only the 8 LSB's
	return frame_data_sum;
}


#endif
/* EOF */
