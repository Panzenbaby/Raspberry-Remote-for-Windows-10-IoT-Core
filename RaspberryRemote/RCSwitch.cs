using System;
using Windows.Devices.Gpio;

namespace RaspberryRemote
{
    public class RCSwitch
    {
        private int mTransmitterPin;
        private int mRepeatTransmit;
        private char mProtocol;

        private Delayer mDelayer = new Delayer();
        private GpioPin mGpioPin = null;

        public RCSwitch()
        {
            mTransmitterPin = -1;
            SetPulseLengthInMicroseconds(350);
            SetRepeatTransmit(10);
            SetProtocol(1);
        }

        /// <summary>
        /// Initialize the GPIO-Pin for the chosen port mTransmitterPin in drive mode GpioPinDriveMode.Output.
        /// After this call the port is ready for the transmission.
        /// </summary>
        private void InitGpio()
        {
            GpioController mGpioController = GpioController.GetDefault();
            if (mGpioController != null)
            {
                mGpioPin = mGpioController.OpenPin(mTransmitterPin);
                mGpioPin.SetDriveMode(GpioPinDriveMode.Output);
            }
        }

        /**
         * Sets the protocol to send.
         */
        public void SetProtocol(int protocol)
        {
            mProtocol = Convert.ToChar(protocol);
            if (mProtocol == 1)
            {
                SetPulseLengthInMicroseconds(350);
            }
            else if (mProtocol == 2)
            {
                SetPulseLengthInMicroseconds(650);
            }
        }

        /**
         * Sets pulse length in microseconds
         */
        public void SetPulseLengthInMicroseconds(int pulseLength)
        {
            mDelayer.SetMicroseconds(pulseLength);
        }

        /**
         * Sets Repeat Transmits
         */
        public void SetRepeatTransmit(int repeatTransmit)
        {
            mRepeatTransmit = repeatTransmit;
        }

        /**
         * Enable transmissions
         *
         * @param transmitterPin Raspberry Pin to which the sender is connected to
         */
        public void EnableTransmit(int transmitterPin)
        {
            mTransmitterPin = transmitterPin;
            InitGpio();
        }

        /**
          * Disable transmissions
          */
        public void DisableTransmit()
        {
            mTransmitterPin = -1;
        }

        /**
         * Switch a remote switch on (Type C Intertechno)
         *
         * @param sFamily  Familycode (a..f)
         * @param nGroup   Number of group (1..4)
         * @param nDevice  Number of device (1..4)
          */
        public void SwitchOn(char family, int group, int device)
        {
            SendTriState(CodeWordGenerator.GetCodeWordC(family, group, device, true));
        }

        /**
         * Switch a remote switch off (Type C Intertechno)
         *
         * @param sFamily  Familycode (a..f)
         * @param nGroup   Number of group (1..4)
         * @param nDevice  Number of device (1..4)
         */
        public void SwitchOff(char family, int group, int device)
        {
            SendTriState(CodeWordGenerator.GetCodeWordC(family, group, device, false));
        }

        /**
         * Switch a remote switch on (Type B with two rotary/sliding switches)
         *
         * @param nAddressCode  Number of the switch group (1..4)
         * @param nChannelCode  Number of the switch itself (1..4)
         */
        public void SwitchOn(int addressCode, int channelCode)
        {
            SendTriState(CodeWordGenerator.GetCodeWordB(addressCode, channelCode, true));
        }

        /**
         * Switch a remote switch off (Type B with two rotary/sliding switches)
         *
         * @param nAddressCode  Number of the switch group (1..4)
         * @param nChannelCode  Number of the switch itself (1..4)
         */
        public void SwitchOff(int addressCode, int channelCode)
        {
            SendTriState(CodeWordGenerator.GetCodeWordB(addressCode, channelCode, false));
        }

        /**
         * Switch a remote switch on (Type A with 10 pole DIP switches)
         *
         * @param group        Code of the switch group (refers to DIP switches 1..5 where "1" = on and "0" = off, if all DIP switches are on it's "11111")
         * @param channelCode  Number of the switch itself (1..4)
         */
        public void SwitchOn(string group, int channel)
        {
            SendTriState(CodeWordGenerator.GetCodeWordA(group, channel, true));
        }

        /**
         * Switch a remote switch off (Type A with 10 pole DIP switches)
         *
         * @param sGroup        Code of the switch group (refers to DIP switches 1..5 where "1" = on and "0" = off, if all DIP switches are on it's "11111")
         * @param nChannelCode  Number of the switch itself (1..4)
         */
        public void SwitchOff(string group, int channel)
        {
            SendTriState(CodeWordGenerator.GetCodeWordA(group, channel, false));
        }

        /**
         * Switch a remote switch on (Type A with 10 pole DIP switches), now with real-binary numbering (see comments in getCodeWordA and getCodeWordD)
         *
         * @param sGroup        Code of the switch group (refers to DIP switches 1..5 where "1" = on and "0" = off, if all DIP switches are on it's "11111")
         * @param nChannelCode  Number of the switch itself (1..31)
         */
        public void SwitchOnBinary(string group, int channel)
        {
            SendTriState(CodeWordGenerator.GetCodeWordD(group, channel, true));
        }

        /**
         * Switch a remote switch off (Type A with 10 pole DIP switches), now with real-binary numbering (see comments in getCodeWordA and getCodeWordD)
         *
         * @param sGroup        Code of the switch group (refers to DIP switches 1..5 where "1" = on and "0" = off, if all DIP switches are on it's "11111")
         * @param nChannelCode  Number of the switch itself (1..31)
         */
        public void SwitchOffBinary(string group, int channel)
        {
            SendTriState(CodeWordGenerator.GetCodeWordD(group, channel, false));
        }

        /**
         * Sends a Code Word
         * @param sCodeWord   /^[10FS]*$/  -> see getCodeWord
         */
        private void SendTriState(string codeWord)
        {
            for (int mRepeat = 0; mRepeat < mRepeatTransmit; mRepeat++)
            {
                for (int i = 0; i < codeWord.Length; i++)
                {
                    switch (codeWord[i])
                    {
                        case '0':
                            SendTriState0();
                            break;
                        case 'F':
                            SendTriStateF();
                            break;
                        case '1':
                            SendTriState1();
                            break;
                    }
                }
                SendSync();
            }
        }

        private void Send(ulong code, uint length)
        {
            Send(CodeWordGenerator.DecimalToBinary((int)code, (int)length));
        }

        private void Send(string codeWord)
        {
            for (int mRepeat = 0; mRepeat < mRepeatTransmit; mRepeat++)
            {
                for (int i = 0; i < codeWord.Length; i++)
                {
                    switch (codeWord[i])
                    {
                        case '0':
                            Send0();
                            break;
                        case '1':
                            Send1();
                            break;
                    }
                }
                SendSync();
            }
        }

        private void Transmit(int highPulses, int lowPulses)
        {
            if (mTransmitterPin != -1)
            {
                for (int i = 0; i < highPulses; i++)
                {
                    mGpioPin.Write(GpioPinValue.High);
                    mDelayer.DelayMicroseconds();
                }
                for (int i = 0; i < lowPulses; i++)
                {
                    mGpioPin.Write(GpioPinValue.Low);
                    mDelayer.DelayMicroseconds();
                }
            }
        }

        /**
         * Sends a "0" Bit
         *                       _
         * Waveform Protocol 1: | |___
         *                       _
         * Waveform Protocol 2: | |__
         */
        private void Send0()
        {
            if (mProtocol == 1)
            {
                Transmit(1, 3);
            }
            else if (mProtocol == 2)
            {
                Transmit(1, 2);
            }
        }

        /**
         * Sends a "1" Bit
         *                       ___
         * Waveform Protocol 1: |   |_
         *                       __
         * Waveform Protocol 2: |  |_
         */
        private void Send1()
        {
            if (mProtocol == 1)
            {
                Transmit(3, 1);
            }
            else if (mProtocol == 2)
            {
                Transmit(2, 1);
            }
        }

        /**
         * Sends a Tri-State "0" Bit
         *            _     _
         * Waveform: | |___| |___
         */
        private void SendTriState0()
        {
            Transmit(1, 3);
            Transmit(1, 3);
        }

        /**
         * Sends a Tri-State "1" Bit
         *            ___   ___
         * Waveform: |   |_|   |_
         */
        private void SendTriState1()
        {
            Transmit(3, 1);
            Transmit(3, 1);
        }

        /**
         * Sends a Tri-State "F" Bit
         *            _     ___
         * Waveform: | |___|   |_
         */
        private void SendTriStateF()
        {
            Transmit(1, 3);
            Transmit(3, 1);
        }

        /**
         * Sends a "Sync" Bit
         *                       _
         * Waveform Protocol 1: | |_______________________________
         *                       _
         * Waveform Protocol 2: | |__________
         */
        private void SendSync()
        {

            if (mProtocol == 1)
            {
                Transmit(1, 31);
            }
            else if (mProtocol == 2)
            {
                Transmit(1, 10);
            }
        }
    }
}
