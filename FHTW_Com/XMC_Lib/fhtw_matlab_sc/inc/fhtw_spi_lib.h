/*
 * fhtw_spi_lib.h
 *
 *  Created on: 29 Mar 2017
 *  Author: pschmitt, rbeneder
 *  Last modified: 12.06.2017
 */
#ifdef FHTW_MATLAB_SC
#ifndef INC_FHTW_SPI_LIB_H_
#define INC_FHTW_SPI_LIB_H_

#include "xmc_spi.h"
#include "xmc_gpio.h"
#include "errno.h"

#define SPI_CONF_U0C0		false
#define SPI_CONF_U0C1		false
#define SPI_CONF_U1C0		false
#define SPI_CONF_U1C1		false
#define SPI_CONF_U2C0		false
#define SPI_CONF_U2C1		false

#define SPI_SLAVE_U1C0		false
#define SPI_SLAVE_U0C1		false
#define SPI_SLAVE_U2C1		true

#if SPI_CONF_U0C0
#define SPI_USIC_MODULE 	XMC_SPI0_CH0
#define SPI_USIC_MISO		USIC0_C0_DX0_P1_4
#define SPI_MISO 			P1_4
#define SPI_MOSI 			P1_5
#define SPI_SCLK 			P1_10
#define SPI_SS 				P1_11
#endif

#if SPI_CONF_U0C1				//"old" HW config
#define SPI_USIC_MODULE 	XMC_SPI0_CH1
#define SPI_USIC_MISO		USIC0_C1_DX0_P2_2
#define SPI_MISO 			P2_2
#define SPI_MOSI 			P2_5
#define SPI_SCLK 			P2_4
#define SPI_SS 				P2_3
#endif

#if SPI_CONF_U1C0
#define SPI_USIC_MODULE 	XMC_SPI1_CH0
#define SPI_USIC_MISO		USIC1_C0_DX0_P0_4
#define SPI_MISO 			P0_4
#define SPI_MOSI 			P0_5
#define SPI_SCLK 			P0_11
#define SPI_SS 				P0_6
#endif

#if SPI_CONF_U1C1
#define SPI_USIC_MODULE 	XMC_SPI1_CH1
#define SPI_USIC_MISO		USIC1_C1_DX0_P0_0
#define SPI_MISO 			P0_0
#define SPI_MOSI 			P0_1
#define SPI_SCLK 			P0_10
#define SPI_SS 				P0_9
#endif

#if SPI_CONF_U2C0
#define SPI_USIC_MODULE 	XMC_SPI2_CH0
#define SPI_USIC_MISO		USIC2_C0_DX0_P5_1
#define SPI_MISO 			P5_1
#define SPI_MOSI 			P5_0
#define SPI_SCLK 			P5_2
#define SPI_SS 				P2_6
#endif

#if SPI_CONF_U2C1				//new HW config
#define SPI_USIC_MODULE 	XMC_SPI2_CH1
#define SPI_USIC_MISO		USIC2_C1_DX0_P3_4
#define SPI_MISO 			P3_4
#define SPI_MOSI 			P3_5
#define SPI_SCLK 			P3_6
#define SPI_SS 				P4_1
#endif


#if SPI_SLAVE_U1C0				//SLAVE config 1
#define SPI_USIC_MODULE 	XMC_SPI1_CH0
#define SPI_USIC_MISO		USIC1_C0_DX0_P0_5
#define SPI_USIC_MOSI		USIC1_C0_DX0_P0_4
#define SPI_MISO 			P0_5
#define SPI_MOSI 			P0_4
#define SPI_SCLK 			P0_11
#define SPI_SS 				P0_6
#define FHTW_SPI_INTERRUPT_SC USIC1_0_IRQn
#endif

#if SPI_SLAVE_U0C1				//"old" HW config
#define SPI_USIC_MODULE 	XMC_SPI0_CH1
#define SPI_USIC_MISO		USIC0_C1_DX0_P2_5
#define SPI_USIC_MOSI		USIC0_C1_DX0_P2_2
#define SPI_MISO 			P2_5
#define SPI_MOSI 			P2_2
#define SPI_SCLK 			P2_4
#define SPI_SS 				P2_3
#define FHTW_SPI_INTERRUPT_SC USIC0_1_IRQn
#endif

#if SPI_SLAVE_U2C1				//new HW config
#define SPI_USIC_MODULE 	XMC_SPI2_CH1
#define SPI_USIC_MISO		USIC2_C1_DX0_P3_5
#define SPI_USIC_MOSI		USIC2_C1_DX0_P3_4
#define SPI_MISO 			P3_5
#define SPI_MOSI 			P3_4
#define SPI_SCLK 			P3_6
#define SPI_SS 				P4_1
#define FHTW_SPI_INTERRUPT_SC USIC2_1_IRQn
#endif


#define SPI_WR		0x01
#define SPI_RD		0x00
#define CHECKSUM_VALUE	0xFF
#define SPI_TRANSFER_OK 0

#define fhtw_spi_BUFFER_SIZE_RX 16
#define fhtw_spi_BUFFER_SIZE_TX 16
#define fhtw_spi_TX_FIFO_INITIAL_LIMIT 0
#define fhtw_spi_RX_FIFO_INITIAL_LIMIT 0

#define FHTW_SPI_CMD_CV 0x01 //SPI CMD receive P-controller result from RPI
#define FHTW_SPI_CMD_SD 0x02 //SPI CMD send IMU sensor values to RPI

#define SPI_RX_BUFFER_SIZE 16
#define SPI_TX_BUFFER_SIZE 16

typedef struct FHTW_SPI_SD_PACKET
{
	uint8_t fhtw_spi_CMD_SD;
	uint8_t fhtw_spi_paysize_SD;
	float fhtw_spi_payload_SD[2];
	uint8_t fhtw_spi_checksum;

} __attribute__((packed, aligned(1))) FHTW_SPI_SD_PACKET;

typedef struct FHTW_SPI_CV_PACKET
{
	uint8_t fhtw_spi_CMD_CV;
	uint8_t fhtw_spi_paysize_CV;
	float fhtw_spi_payload_CV;
	uint8_t fhtw_spi_checksum;

} __attribute__((packed, aligned(1))) FHTW_SPI_CV_PACKET;

void fhtw_spi_init(void);
void fhtw_spi_transmit_word(XMC_USIC_CH_t *const channel, uint8_t spi_data);
uint8_t fhtw_spi_receive_word(XMC_USIC_CH_t *const channel);
uint8_t fhtw_spi_get_slave_frame(XMC_USIC_CH_t *const channel);
uint8_t fhtw_spi_update_rxdata(void);
uint8_t fhtw_spi_update_txdata(void);
void fhtw_spi_push_ctr_param(float r, float y);
float fhtw_spi_pop_ctr_result(void);
uint16_t fhtw_SPI_make_checksum(uint8_t *frame, uint8_t length);

#endif /* INC_FHTW_SPI_LIB_H_ */
#endif
