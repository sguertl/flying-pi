/*
 * fhtw_uart.h
 *
 *  Created on: 22.05.2017
 *      Author: Patrick Schmitt, Markus Lechner
 */
#ifdef FHTW_MATLAB_SC
#ifndef FHTW_UART_H_
#define FHTW_UART_H_

#include <stdint.h>

/******************************************************************** DEFINES */
#define UART_CONF_U2C0_HW_NEW	true
#define UART_CONF_U0C0_HW_OLD	false
#define UART_CONF_U0C1			false
#define UART_CONF_U1C0			false
#define UART_CONF_U1C1			false
#define UART_CONF_U2C0			false
#define UART_CONF_U2C1			false

#if UART_CONF_U2C0_HW_NEW
#define fhtw_uart_TX 			P5_0
#define fhtw_uart_RX 			P5_1
#define fhtw_uart_USIC_RX		USIC2_C0_DX0_P5_1
#define fhtw_uart_TX_GPIO_mode 	XMC_GPIO_MODE_OUTPUT_PUSH_PULL_ALT1
#define fhtw_uart_module		XMC_UART2_CH0
#define fhtw_uart_interrupt_sc	USIC2_0_IRQn
#endif

#if UART_CONF_U0C0_HW_OLD
#define fhtw_uart_TX 			P5_1
#define fhtw_uart_RX 			P1_5
#define fhtw_uart_USIC_RX		USIC0_C0_DX0_P1_5
#define fhtw_uart_TX_GPIO_mode 	XMC_GPIO_MODE_OUTPUT_PUSH_PULL_ALT1
#define fhtw_uart_module		XMC_UART0_CH0
#define fhtw_uart_interrupt_sc	USIC0_0_IRQn
#endif

#if UART_CONF_U1C0
#define fhtw_uart_TX 			P0_5
#define fhtw_uart_RX 			P0_4
#define fhtw_uart_USIC_RX		USIC1_C0_DX0_P0_4
#define fhtw_uart_TX_GPIO_mode 	XMC_GPIO_MODE_OUTPUT_PUSH_PULL_ALT2
#define fhtw_uart_module		XMC_UART1_CH0
#define fhtw_uart_interrupt_sc	USIC1_0_IRQn
#endif

#if UART_CONF_U1C1
#define fhtw_uart_TX 			P0_1
#define fhtw_uart_RX 			P0_0
#define fhtw_uart_USIC_RX		USIC1_C1_DX0_P0_0
#define fhtw_uart_TX_GPIO_mode 	XMC_GPIO_MODE_OUTPUT_PUSH_PULL_ALT2
#define fhtw_uart_module		XMC_UART1_CH1
#define fhtw_uart_interrupt_sc	USIC1_1_IRQn
#endif

#define fhtw_uart_BUFFER_SIZE_PRINTF 255
#define fhtw_uart_BUFFER_SIZE_RX 16
#define fhtw_uart_STRING_LF 10

#define FHTW_UART_CMD_COMIN 0x01
#define FHTW_UART_CMD_SPIC 0x02
#define CHECKSUM_VALUE	0xFF

#define fhtw_uart_RX_FIFO_INITIAL_LIMIT 15
#define fhtw_uart_TX_FIFO_INITIAL_LIMIT 0

typedef struct FHTW_UART_COMIN_PACKET
{
	uint8_t fhtw_uart_CMD_COMIN;
	uint8_t fhtw_uart_ident;
	uint8_t fhtw_uart_checksum;

} __attribute__((packed, aligned(1))) FHTW_UART_COMIN_PACKET;

typedef struct FHTW_UART_SPIC_PACKET
{
	uint8_t fhtw_uart_CMD_SPIC;
	uint8_t fhtw_uart_spi_clock_val;
	uint8_t fhtw_uart_spi_clock_power;
	uint8_t fhtw_uart_checksum;

} __attribute__((packed, aligned(1))) FHTW_UART_SPIC_PACKET;

/******************************************************************** FUNCTION PROTOTYPES */
void fhtw_uart_init(void);
uint8_t fhtw_uart_send_char(char c);
uint8_t fhtw_uart_printf(char *fmt, ...);
uint8_t fhtw_uart_send_string(char *str);
uint8_t fhtw_uart_rx_unpack(uint8_t uart_rx_size);
uint16_t fhtw_UART_make_checksum(uint8_t *frame, uint8_t length);

#endif /* FHTW_UART_H_ */
#endif
/* EOF */
