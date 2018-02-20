/*******************************************************************************
**                      Author(s) Identity                                    **
********************************************************************************
**                                                                            **
** Initials     Name                                                          **
** ---------------------------------------------------------------------------**
** AM           Andreas Mark                                                  **
** ES			Elias Steurer												  **
** DW           Dominik Wieland                                               **
**                                                                            **
**                                                                            **
*******************************************************************************/

/*******************************************************************************
**                      Revision Control History                              **
*******************************************************************************/
/*
 * V0.0  : 22-12-2014, AM:  Initial Version
 * V0.0.1: 10-07-2015, ES:  Added Bluetooth Support
 * V0.1  : 21-07-2016, DW:  Port of SW from DAVE3 to DAVE4
 */


/*******************************************************************************
**                      Includes                                              **
*******************************************************************************/
#include "RCReceive.h"	/**<Header file*/

/*******************************************************************************
**                      Private Constant Definitions to be changed            **
*******************************************************************************/

/*******************************************************************************
**                      Private Macro Definitions                             **
*******************************************************************************/

/*******************************************************************************
**                      Global Type Definitions                               **
*******************************************************************************/

/*******************************************************************************
**                      Private Type Definitions                              **
*******************************************************************************/

/*******************************************************************************
**                      Global Function Declarations                          **
*******************************************************************************/

/*******************************************************************************
**                      Private Function Declarations                         **
*******************************************************************************/
void create_package();
uint32_t float_to_int(float x);
/*******************************************************************************
**                      Global Constant Definitions                           **
*******************************************************************************/

/*******************************************************************************
**                      Private Constant Definitions                          **
*******************************************************************************/

/*******************************************************************************
**                      Global Variable Definitions                           **
*******************************************************************************/
extern uint8_t g_sensor_state;
extern float Radar_Value;
extern uint8_t altHold_active;
extern float EstAlt;
extern float VBat;
/*******************************************************************************
**                      Private Variable Definitions                          **
*******************************************************************************/
extern uint8_t* ReadBufBT;			/**< pointer to bluetooth input buffer */
extern uint8_t* ReadBufRPI;         /**< pointer to raspberry input buffer */
extern uint8_t* WriteBufRPI;        /**< pointer to raspberry input buffer */
extern ControlValue rpi_control_value; /**< control value for raspberry */
extern ControlValue control_value;	/**< control value for bluetooth */
extern DataPacket dpacket;			/**< data packet value for bluetooth*/

uint8_t flightmode = 0;		/**<remote control switch; value can be: 0 -> RC or 1 -> Bluetooth*/
uint8_t RCTimeOut = 1; 		/**<Timeout remote control 0:OK 1:TimeOut */
uint8_t RCCount   = 0; 		/**<Counter for RC read (used for TimeOut)*/
uint8_t BTTimeOut = 1;		/**<Timeout Bluetooth; 0:OK 1:TimeOut */
uint8_t BTCount   = 0;		/**<Counting up, when receiving data */
uint8_t RPITimeOut = 1;     /**<Timeout Raspberry; 0:OK 1:TimeOut */
uint8_t RPICount   = 0;     /**<Counting up, when receiving data */

uint8_t isLoggingActive = 0;
uint8_t isLogDroneControlActive = 0;
uint8_t isLogCollisionStatusActive = 0;
uint8_t isLogRadarActive = 0;
uint8_t isLogBatteryActive = 0;
uint8_t isLogDebug1Active = 0;
uint8_t isLogDebug2Active = 0;
uint8_t isLogDebug3Active = 0;
uint8_t isLogDebug4Active = 0;
uint8_t isLogStreamReceived = 0;
uint8_t isEndCommunication = 0;

// Software timer
uint32_t TimerWatchRC; /**<Timer for RC watchdog*/

//Joystick values (range between -1 and 1, 0 in middle position)
QUADROCOPTER_RC_RECEIVE_JOYSTICK_VALUES g_joystick_values =
{
		.throttle=0.0f,
		.rudder=0.0f,
		.elevator=0.0f,
		.aileron=0.0f
};

//Bluetooth control values
QUADROCOPTER_RC_RECEIVE_BLUETOOTH_VALUES g_bluetooth_values =
{
		.throttle=0,
		.heightControl=0,
		.rudder=0.0f,
		.aileron=0.0f,
		.elevator=0.0f
};

//Raspberry control values
QUADROCOPTER_RC_RECEIVE_BLUETOOTH_VALUES g_raspberry_values =
{
		.throttle=0,
		.heightControl=0,
		.rudder=0.0f,
		.aileron=0.0f,
		.elevator=0.0f
};

QUADROCOPTER_RC_TRANSMIT_RASPBERRY_VALUES g_rasperry_transmit_values =
{
		.throttle=0,
		.heightControl=0,
		.rudder=0.0f,
		.aileron=0.0f,
		.elevator=0.0f,
		.collisionStatus=0,
		.radarValue=0.0f,
		.battery=0,
		.debug1=0.0f,
		.debug2=0.0f,
		.debug3=0.0f,
		.debug4=0.0f
};

/*******************************************************************************
**                      Global Function Definitions                           **
*******************************************************************************/
/**
 *  \brief Fetches data from the Remote Control
 *  
 *  \param [in] power Pointer to variable for throttle value
 *  \param [in] altHold_active Pointer to variable for Height control
 *  \param [in] yaw_dot Pointer to variable for value of yaw_dot
 *  \param [in] pitch Pointer to variable for the value of pitch
 *  \param [in] roll Pointer to variable for the value of roll
 *  
 *  \details Starts taking actions after checking if the connection should be per bluetooth or remote control
 */
void GetRCData(float* power,uint8_t* altHold_active, float* yaw_dot, float* pitch, float* roll)
{
#ifdef BT_ONLY		//BT only controlled (no RC needed)
		if (BTTimeOut)
		{
			*power   = 0.0f;
			*yaw_dot = 0.0f;
			*pitch   = 0.0f;
			*roll   = 0.0f;
		}
		else
		{
			*power =(float) g_bluetooth_values.throttle;
			*altHold_active = g_bluetooth_values.heightControl;
			*pitch = g_bluetooth_values.elevator;
			*roll = -g_bluetooth_values.aileron;
			*yaw_dot = g_bluetooth_values.rudder;
			if (*pitch > 30.0f)
			{
				*pitch = 30.0f;
			}
			else if (*pitch < -30.0f)
			{
				*pitch = -30.0f;
			}

			if (*roll > 30.0f)
			{
				*roll = 30.0f;
			}
			else if (*roll < -30.0f)
			{
				*roll = -30.0f;
			}
		}

#elif defined RC_ONLY

	if (RCTimeOut)
	{
		*power   = 0.0f;
		*yaw_dot = 0.0f;
		*pitch   = 0.0f;
		*roll    = 0.0f;
		*altHold_active = 0;
	}
	else
	{
		*power = SCALE_POWER * g_joystick_values.throttle;
		if (g_joystick_values.rudder > 0.01f || g_joystick_values.rudder < -0.01f)
			*yaw_dot = g_joystick_values.rudder * SCALE_YAW;
		else
			*yaw_dot = 0.0f;
		*pitch=g_joystick_values.elevator*SCALE_PITCH;
		*roll=-g_joystick_values.aileron*SCALE_ROLL;
		if (flightmode == 0)
		{
			*altHold_active = 0;
		}
		else
		{
			*altHold_active = 1;
		}

	}
#elif defined RPI_ONLY
	if (RPITimeOut)
		{
			*power   = 0.0f;
			*yaw_dot = 0.0f;
			*pitch   = 0.0f;
			*roll    = 0.0f;
			*altHold_active = 0;
		}
		else
		{
			*power = g_raspberry_values.throttle;
			if (g_raspberry_values.rudder > 0.01f || g_raspberry_values.rudder < -0.01f)
				*yaw_dot = g_raspberry_values.rudder;
			else
				*yaw_dot = 0.0f;
			*pitch=g_raspberry_values.elevator;
			*roll=-g_raspberry_values.aileron;
			if (flightmode == 0)
			{
				*altHold_active = 0;
			}
			else
			{
				*altHold_active = 1;
			}

		}


#else		//for RC Control with switch to BT Control
	if (flightmode == 0)
	{
		if (RCTimeOut)
		{
			*power   = 0.0f;
			*yaw_dot = 0.0f;
			*pitch   = 0.0f;
			*roll    = 0.0f;
		}
		else
		{
			*power = SCALE_POWER * g_joystick_values.throttle;
			if (g_joystick_values.rudder > 0.01f || g_joystick_values.rudder < -0.01f)
				*yaw_dot = g_joystick_values.rudder * SCALE_YAW;
			else
				*yaw_dot = 0.0f;
			*pitch=g_joystick_values.elevator*SCALE_PITCH;
			*roll=-g_joystick_values.aileron*SCALE_ROLL;
		}
	}
	else
	{
		if (BTTimeOut || RCTimeOut)
		{
			*power   = 0.0f;
			*yaw_dot = 0.0f;
			*pitch   = 0.0f;
			*roll    = 0.0f;
		}
		else
		{
			*power =(float) g_bluetooth_values.throttle;
			*altHold_active = g_bluetooth_values.heightControl;
			*pitch = g_bluetooth_values.elevator;
			*roll = -g_bluetooth_values.aileron;
			*yaw_dot = g_bluetooth_values.rudder;
			if (*pitch > 30.0f)
			{
				*pitch = 30.0f;
			}
			else if (*pitch < -30.0f)
			{
				*pitch = -30.0f;
			}

			if (*roll > 30.0f)
			{
				*roll = 30.0f;
			}
			else if (*roll < -30.0f)
			{
				*roll = -30.0f;
			}
		}
	}
#endif
}

/*******************************************************************************
**                      Private Function Definitions                          **
*******************************************************************************/
 /**
 *  \brief Function for merging two Bytes into one Integer value
 *  
 *  \param [in] highBytes Variable which should embrace the high Bytes
 *  \param [in] lowBytes Variable which should embrace the low Bytes
 *  \return uint16_t variable with merged Bytes
 *  
 *  \details This is required for extracting two Bytes of the data frame into one variable
 */
int mergeBytes(uint8_t highBytes, uint8_t lowBytes)
{
	int c = highBytes;
	return (c << 8) | lowBytes;
}


 /**
 *  \brief Timer Interrupt-Service-Routine for Bluetooth Keep-Alive Messages
 *  
 *  
 *  \details This function is for messaging the connected device that the connection is alive
 */
void GeneralPurpose_Timer_Bluetooth_Keep_Alive_ISR()
{
#ifdef ALT_HOLD_RADAR
	FRI_sendRadarData(EstAlt/100.0f);
#else
	FRI_sendRadarData(Radar_Value);
#endif

	if(isLogStreamReceived == 1)
	{
		WriteBufRPI[0] = 1;
		WriteBufRPI[1] = 0;
		UART_WriteDataBuffer(&Raspberry_Handle, WriteBufRPI, 41);
		isLogStreamReceived = 0;
	}

	if(isLoggingActive == 1)
	{
		if(isLogDroneControlActive == 1)
		{
			g_rasperry_transmit_values.throttle = g_raspberry_values.throttle;
			g_rasperry_transmit_values.heightControl = g_raspberry_values.heightControl;
			g_rasperry_transmit_values.rudder = g_raspberry_values.rudder;
			g_rasperry_transmit_values.aileron = g_raspberry_values.aileron;
			g_rasperry_transmit_values.elevator = g_raspberry_values.elevator;
		}

		if(isLogCollisionStatusActive == 1)
		{
			g_rasperry_transmit_values.collisionStatus = g_sensor_state;
		}

		if(isLogRadarActive == 1)
		{
			g_rasperry_transmit_values.radarValue = Radar_Value;
		}

		if(isLogBatteryActive == 1)
		{
			g_rasperry_transmit_values.battery = (uint8_t)floorf(VBat);
		}

		if(isLogDebug1Active == 1)
		{
			g_rasperry_transmit_values.debug1 = 0;
		}

		if(isLogDebug2Active == 1)
		{
			g_rasperry_transmit_values.debug2 = 0;
		}

		if(isLogDebug3Active == 1)
		{
			g_rasperry_transmit_values.debug3 = 0;
		}

		if(isLogDebug4Active == 1)
		{
			g_rasperry_transmit_values.debug4 = 0;
		}

		create_package();

		UART_WriteDataBuffer(&Raspberry_Handle, WriteBufRPI, 41);
	}

	if(isEndCommunication == 1)
	{
		WriteBufRPI[0] = 99;
		UART_WriteDataBuffer(&Raspberry_Handle, WriteBufRPI, 41);
		isEndCommunication = 0;
	}
}

void create_package()
{
	uint32_t rudder = float_to_int(g_rasperry_transmit_values.rudder);
	uint32_t elevator = float_to_int(g_rasperry_transmit_values.elevator);
	uint32_t aileron = float_to_int(g_rasperry_transmit_values.aileron);
	uint32_t radarValue = float_to_int(g_rasperry_transmit_values.radarValue);
	uint32_t debug1 = float_to_int(g_rasperry_transmit_values.debug1);
	uint32_t debug2 = float_to_int(g_rasperry_transmit_values.debug2);
	uint32_t debug3 = float_to_int(g_rasperry_transmit_values.debug3);
	uint32_t debug4 = float_to_int(g_rasperry_transmit_values.debug4);

	int checksum = 0xA;
	checksum ^= (g_rasperry_transmit_values.heightControl << 8 | g_rasperry_transmit_values.throttle) & 0xFFFF;
	checksum ^= rudder;
	checksum ^= elevator;
	checksum ^= aileron;
	checksum ^= g_rasperry_transmit_values.collisionStatus;
	checksum ^= radarValue;
	checksum ^= g_rasperry_transmit_values.battery;
	checksum ^= debug1;
	checksum ^= debug2;
	checksum ^= debug3;
	checksum ^= debug4;

	WriteBufRPI[0] = 0xA; // 1

	WriteBufRPI[1] = g_rasperry_transmit_values.heightControl & 0xFF; // 2
	WriteBufRPI[2] = g_rasperry_transmit_values.throttle & 0xFF; // 3

	WriteBufRPI[3] = (rudder >> 24) & 0xFF;
	WriteBufRPI[4] = (rudder >> 16) & 0xFF;
	WriteBufRPI[5] = (rudder >> 8) & 0xFF;
	WriteBufRPI[6] = rudder & 0xFF; // 7

	WriteBufRPI[7] = (aileron >> 24) & 0xFF;
	WriteBufRPI[8] = (aileron >> 16) & 0xFF;
	WriteBufRPI[9] = (aileron >> 8) & 0xFF;
	WriteBufRPI[10] = aileron & 0xFF; // 11

	WriteBufRPI[11] = (elevator >> 24) & 0xFF;
	WriteBufRPI[12] = (elevator >> 16) & 0xFF;
	WriteBufRPI[13] = (elevator >> 8) & 0xFF;
	WriteBufRPI[14] = elevator & 0xFF; // 15

	WriteBufRPI[15] = (radarValue >> 24) & 0xFF;
	WriteBufRPI[16] = (radarValue >> 16) & 0xFF;
	WriteBufRPI[17] = (radarValue >> 8) & 0xFF;
	WriteBufRPI[18] = radarValue & 0xFF; // 19

	WriteBufRPI[19] = (debug1 >> 24) & 0xFF;
	WriteBufRPI[20] = (debug1 >> 16) & 0xFF;
	WriteBufRPI[21] = (debug1 >> 8) & 0xFF;
	WriteBufRPI[22] = debug1 & 0xFF; // 23

	WriteBufRPI[23] = (debug2 >> 24) & 0xFF;
	WriteBufRPI[24] = (debug2 >> 16) & 0xFF;
	WriteBufRPI[25] = (debug2 >> 8) & 0xFF;
	WriteBufRPI[26] = debug2 & 0xFF; // 27

	WriteBufRPI[27] = (debug3 >> 24) & 0xFF;
	WriteBufRPI[28] = (debug3 >> 16) & 0xFF;
	WriteBufRPI[29] = (debug3 >> 8) & 0xFF;
	WriteBufRPI[30] = debug3 & 0xFF; // 31

	WriteBufRPI[31] = (debug4 >> 24) & 0xFF;
	WriteBufRPI[32] = (debug4 >> 16) & 0xFF;
	WriteBufRPI[33] = (debug4 >> 8) & 0xFF;
	WriteBufRPI[34] = debug4 & 0xFF; // 35

	WriteBufRPI[35] = g_rasperry_transmit_values.collisionStatus & 0xFF;

	WriteBufRPI[36] = g_rasperry_transmit_values.battery & 0xFF;

	WriteBufRPI[37] = (checksum >> 24) & 0xFF;
	WriteBufRPI[38] = (checksum >> 16) & 0xFF;
	WriteBufRPI[39] = (checksum >> 8) & 0xFF;
	WriteBufRPI[40] = checksum & 0xFF;
}

uint32_t float_to_int(float x)
{
	int y = *((int*)(&x));
	return y;
}

 /**
 *  \brief Interrupt-Service-Routine from UART FIFO in buffer
 *  
 *  
 *  \details When buffer is full, new data is ready to read
 */
void RemoteControl_RX_ISR()
{
	//Raw values from RC
	int throttleRaw;
	int rudderRaw;
	int elevatorRaw;
	int aileronRaw;
	int flightmodeRaw;

	uint8_t ReadBufRC[32]; //Readbuffer
	int start = 0; //Index of start byte (contains 0x30)


	//Read data from UART buffer
	UART_ReadDataBytes(&RemoteControl_Handle, ReadBufRC, 32);

	//Search for start byte and check static values
	while (ReadBufRC[start] != 0x30 || ReadBufRC[start + 1] != 0x00
			|| ReadBufRC[start + 5] != 0xA2 || ReadBufRC[start + 8] != 0x2B
			|| ReadBufRC[start + 9] != 0xFE)
	{
		if (start++ > 16)
		{
			//Communication check bytes not in buffer
			return;
		}
	}
	//Get data from stream
	//get raw values
	throttleRaw = mergeBytes(ReadBufRC[start + 2], ReadBufRC[start + 3]);
	aileronRaw = mergeBytes(ReadBufRC[start + 6], ReadBufRC[start + 7]);
	elevatorRaw = mergeBytes(ReadBufRC[start + 10], ReadBufRC[start + 11]);
	rudderRaw = mergeBytes(ReadBufRC[start + 14], ReadBufRC[start + 15]);
	flightmodeRaw = mergeBytes(ReadBufRC[start + 12],ReadBufRC[start + 13]);
	//remap raw values to control values
	g_joystick_values.throttle = map(throttleRaw, THROTTLE_MIN, THROTTLE_MAX, 0, 60000)/ 60000.0f;
	g_joystick_values.aileron = map(aileronRaw, AILERON_MIN, AILERON_MAX, -30000, 30000)/ 30000.0f;
	g_joystick_values.elevator = map(elevatorRaw, ELEVATOR_MIN, ELEVATOR_MAX, -30000, 30000)/ 30000.0f;
	g_joystick_values.rudder = map(rudderRaw, RUDDER_MIN, RUDDER_MAX, -30000, 30000)/ 30000.0f;

	if (flightmodeRaw == FLIGHTMODE0)
		flightmode = 0;
	if (flightmodeRaw == FLIGHTMODE1)
		flightmode = 1;
	//Set values for RC Timeout check
	RCTimeOut = 0;
	RCCount++;

}
/**
 *  \brief Interrupt-Service-Routine for receiving Bluetooth Bytes
 *  
 *  
 *  \details reads data bytes, analyzes the data and checks if error occurs
 *  \details if errors occur -> motors are stopped
 */
void Bluetooth_RX_ISR()
{
	UART_ReadDataBytes(&Bluetooth_Handle, ReadBufBT, PACKET_SIZE);
	BTTimeOut = 0;
	BTCount++;
	int32_t rec_mode = maintainBluetoothInputBuffer(ReadBufBT,&control_value, &dpacket);
	switch (rec_mode)
	{
	case RECEIVED_CONTROL_PACKET:
		g_bluetooth_values.throttle = (uint16_t)*control_value.speed;
		g_bluetooth_values.heightControl = *control_value.altHold_active;
		g_bluetooth_values.aileron = (*control_value.x_pitch);
		g_bluetooth_values.elevator = (-*control_value.y_roll);
		g_bluetooth_values.rudder = (*control_value.z_rotate);
		break;
	case RECEIVED_DATA_PACKET:
		break;
	case CHECKSUM_ERROR:
		g_bluetooth_values.throttle = 0;
		g_bluetooth_values.aileron  = 0.0f;
		g_bluetooth_values.elevator = 0.0f;
		g_bluetooth_values.rudder   = 0.0f;
		XMC_USIC_CH_RXFIFO_Flush(Bluetooth_Handle.channel);
		//Todo: Add Error-Handling
		break;
	case UNDEFINED_ERROR:
		g_bluetooth_values.throttle = 0;
		g_bluetooth_values.aileron  = 0.0f;
		g_bluetooth_values.elevator = 0.0f;
		g_bluetooth_values.rudder   = 0.0f;
		XMC_USIC_CH_RXFIFO_Flush(Bluetooth_Handle.channel);
		//Todo: Add Error-Handling
		break;
	}
}

/**
 *  \brief Interrupt-Service-Routine for receiving Raspberry Bytes
 *
 *
 *  \details reads data bytes, analyzes the data and checks if error occurs
 *  \details if errors occur -> motors are stopped
 */
void Raspberry_RX_ISR(void)
{
	//Read data from UART buffer
	UART_ReadDataBytes(&Raspberry_Handle, ReadBufRPI, 19);
	RPITimeOut = 0;
	RPICount++;
	int32_t rec_mode = 0;
	switch(ReadBufRPI[0])
	{
		case 0x00:
			// receive controller data
			rec_mode = maintainBluetoothInputBuffer(ReadBufRPI, &rpi_control_value, &dpacket);
			break;
		case 0xA:
			// receive log init
			isLogStreamReceived = 1;
			if(ReadBufRPI[1] == 1)
			{
				isLoggingActive = 1;
				isLogBatteryActive = ReadBufRPI[2];
				isLogRadarActive = ReadBufRPI[3];
				isLogCollisionStatusActive = ReadBufRPI[4];
				isLogDroneControlActive = ReadBufRPI[5];
				isLogDebug1Active = ReadBufRPI[6];
				isLogDebug2Active = ReadBufRPI[7];
				isLogDebug3Active = ReadBufRPI[8];
				isLogDebug4Active = ReadBufRPI[9];
			}
			break;
		default:
			// receive stop serial communication
			isLoggingActive = 0;
			isLogStreamReceived = 0;
			isEndCommunication = 1;
	}

	switch (rec_mode)
	{
		case RECEIVED_CONTROL_PACKET:
			g_raspberry_values.throttle = (uint16_t)*rpi_control_value.speed;
			g_raspberry_values.heightControl = *rpi_control_value.altHold_active;
			g_raspberry_values.aileron = (*rpi_control_value.x_pitch);
			g_raspberry_values.elevator = (-*rpi_control_value.y_roll);
			g_raspberry_values.rudder = (*rpi_control_value.z_rotate);
			break;
		case RECEIVED_DATA_PACKET:
			break;
		case CHECKSUM_ERROR:
			g_raspberry_values.throttle = 0;
			g_raspberry_values.aileron  = 0.0f;
			g_raspberry_values.elevator = 0.0f;
			g_raspberry_values.rudder   = 0.0f;
			XMC_USIC_CH_RXFIFO_Flush(Raspberry_Handle.channel);
			//Todo: Add Error-Handling
			break;
		case UNDEFINED_ERROR:
			g_raspberry_values.throttle = 0;
			g_raspberry_values.aileron  = 0.0f;
			g_raspberry_values.elevator = 0.0f;
			g_raspberry_values.rudder   = 0.0f;
			XMC_USIC_CH_RXFIFO_Flush(Raspberry_Handle.channel);
			//Todo: Add Error-Handling
			break;
	}
}

/**
 *  \brief Interrupt-Service-Routine for transmitting Raspberry Bytes
 *
 *
 *  \details transmits data bytes
 */
void Raspberry_TX_ISR(void)
{
}

 /**
 *  \brief This function is for checking if new data has been arrived since last function call
 *  
 *  \param [in] Temp Needed due to SYSTIMER-App - but not in use 
 *  
 *  \details This function is called from software Timer "TimerWatchRC"
 *  \details
 */
void WatchRC(void* Temp)
{
	static uint8_t lastCount;
	static uint8_t lastBTCount;
	static uint8_t lastRPICount;

	if (lastCount == RCCount)
		RCTimeOut = 1;
	lastCount = RCCount;

	if (lastBTCount == BTCount)
	{
		BTTimeOut = 1;
		XMC_USIC_CH_RXFIFO_Flush(Bluetooth_Handle.channel);
	}
	lastBTCount = BTCount;

	if (lastRPICount == RPICount)
	{
		RPITimeOut = 1;
		XMC_USIC_CH_RXFIFO_Flush(Raspberry_Handle.channel);
	}
	lastRPICount = RPICount;
}


 /**
 *  \brief Initialize RC watchdog
 *  
 *  
 *  \details Timer overflows every 0.2 seconds
 *  \details Further it's been checked if the timer is running
 */
void WatchRC_Init()
{
	TimerWatchRC = (uint32_t)SYSTIMER_CreateTimer(200000,SYSTIMER_MODE_PERIODIC,(void*)WatchRC,NULL);
	if (TimerWatchRC != 0U)
	{
		//Timer is created successfully
		// Start/Run Software Timer
		if (SYSTIMER_StartTimer(TimerWatchRC) == SYSTIMER_STATUS_SUCCESS)
		{
			// Timer is running
		}
	}
}
/**
 *  \brief Getter-function for getting the value of RCCount
 *  
 *  \return Return value type is uint8_t
 *  \return Return value is the value of the variable "RCCount"
 *  
 *  \details The variable RCCount is used for TimeOut 
 */
uint8_t GetRCCount()
{
	return RCCount;
}


/**
 *  \brief Function for converting a value within a range to the same value according to the target range
 *  
 *  \param [in] x Value to be scaled
 *  \param [in] in_min Minimum of the first value range
 *  \param [in] in_max Maximum of the first value range
 *  \param [in] out_min Minimum of the second value range 
 *  \param [in] out_max Maximum of the second value range
 *  
 *  \return if x==in_min it returns out_min
 * 	\return if x==in_max it returns out_max
 *  
 *  \details This function is used to scale the value of x from the value range [in_min ... in_max] to the value range [out_min ... out_max]
 */
long map(long x, long in_min, long in_max, long out_min, long out_max)
{
	return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
}
