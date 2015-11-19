using System;

namespace RaspberryRemote
{
    class CodeWordGenerator
    {
        /**
         * Returns a char[13], representing the Code Word to be send.
         * A Code Word consists of 9 address bits, 3 data bits and one sync bit but in our case only the first 8 address bits and the last 2 data bits were used.
         * A Code Bit can have 4 different states: "F" (floating), "0" (low), "1" (high), "S" (synchronous bit)
         *
         * +-------------------------------+--------------------------------+-----------------------------------------+-----------------------------------------+----------------------+------------+
         * | 4 bits address (switch group) | 4 bits address (switch number) | 1 bit address (not used, so never mind) | 1 bit address (not used, so never mind) | 2 data bits (on|off) | 1 sync bit |
         * | 1=0FFF 2=F0FF 3=FF0F 4=FFF0   | 1=0FFF 2=F0FF 3=FF0F 4=FFF0    | F                                       | F                                       | on=FF off=F0         | S          |
         * +-------------------------------+--------------------------------+-----------------------------------------+-----------------------------------------+----------------------+------------+
         *
         * @param nAddressCode  Number of the switch group (1..4)
         * @param nChannelCode  Number of the switch itself (1..4)
         * @param bStatus       Wether to switch on (true) or off (false)
         *
         * @return char[13]
         */
        public static string GetCodeWordB(int addressCode, int channelCode, bool status)
        {
            string[] code = new string[] { "FFFF", "0FFF", "F0FF", "FF0F", "FFF0" };
            if (addressCode >= 1 && addressCode <= 4 && channelCode >= 1 && channelCode <= 4)
            {
                string result = code[addressCode] + code[channelCode] + "FFF";
                result += status ? "F" : "0";
                return result;
            }
            return string.Empty;
        }

        /**
         * Like getCodeWord  (Type A)
         */
        public static string GetCodeWordA(string group, int channelCode, bool status)
        {
            string result = "";
            /*
             * The codeword, that needs to be sent, consists of three main parts:
             * char 0 to 4: Group-Number (already binary)
             * char 5 to 9: Socket Number (converted to binary, where the socket number 0-5 sets the only active bit in the return string)
             *              e.g: socket 1 means: bit 1 is on, others off: 10000
             *                   socket 5 means: bit 4 is on, others off: 00010
             * char 10 to 11: Power state, where on means '01' and off means '10'
            */
            string[] code = new string[] { "FFFFF", "0FFFF", "F0FFF", "FF0FF", "FFF0F", "FFFF0" };

            if (channelCode < 1 || channelCode > 5)
            {
                return string.Empty;
            }

            group = group.Replace('0', 'F').Replace('1', '0');
            result += group;
            for (int i = 0; i < 5; i++)
            {
                if (group[i] != 'F' && group[i] != '0')
                {
                    return string.Empty;
                }
            }

            result += code[channelCode];
            result += status ? "0F" : "F0";

            return result;
        }

        /**
         * Like getCodeWord  (Type A)
         * Like getCodeWordA, but with real binary socket numbers instead of numbering by position of active bit.
         */

        /**
         * To use this function, the sockets need to be numbered in real binary after the following scheme:
         *
         * |no. | old representation | new binary |
         * |--------------------------------------|
         * |   1|              10000 |      00001 |
         * |   2|              01000 |      00010 |
         * |   3|              00100 |      00011 |
         * |   4|              00010 |      00100 |
         * |   5|              00001 |      00101 |
         * |   6|              ----- |      00110 |
         * |   8|              ----- |      01000 |
         * |  16|              ----- |      10000 |
         * |  31|              ----- |      11111 |
         * |--------------------------------------|
         *
         * This means, that now more than 5 sockets can be used per system.
         * It is, indeed, necessary, to set all the sockets used to the new binary numbering system.
         * Therefore, most systems won't work with their dedicated remotes anymor, which
         * only provide buttons for socket A to E, sending only the old representation as noted
         * above.
         */

        public static string GetCodeWordD(string group, int channelCode, bool status)
        {
            string result = "";

            /**
             * The codeword, that needs to be sent, consists of three main parts:
             * char 0 to 4: Group-Number (already binary)
             * char 5 to 9: Socket Number (converted to binary, former: the socket number 0-5 sets the only active bit in the return string)
             *              e.g: socket 1 means: bit 1 is on, others off: 10000
             *                   socket 5 means: bit 4 is on, others off: 00010
             *              now: real binary representation of decimal socket number
             * char 10 to 11: Power state, where on means '01' and off means '10'
            */

            //const char* code[6] = { "FFFFF", "0FFFF", "F0FFF", "FF0FF", "FFF0F", "FFFF0" }; //former conversion of socket number to binary

            if (channelCode < 1 || channelCode > 31)
            {
                return string.Empty;
            }

            group = group.Replace('0', 'F').Replace('1', '0');
            result += group;

            for (int i = 0; i < 5; i++)
            {
                if (group[i] != 'F' && group[i] != '0')
                {
                    return string.Empty;
                }
            }

            string str = Convert.ToString(channelCode, 2);
            string temp = str;

            while (str.Length < 5)
            {
                str = "0" + str;
            }

            str = str.Replace('0', 'F').Replace('1', '0');
            result += str;
            result += status ? "0F" : "F0";

            return result;
        }

        /**
         * Like getCodeWord (Type C = Intertechno)
         */
        public static string GetCodeWordC(char family, int group, int device, bool status)
        {
            string result = "";

            if ((byte)family < 97 || (byte)family > 112 || group < 1 || group > 4 || device < 1 || device > 4)
            {
                return string.Empty;
            }

            string deviceGroupCode = DecimalToBinary((device - 1) + (group - 1) * 4, 4);
            string[] familycode = new string[] { "0000", "F000", "0F00", "FF00", "00F0", "F0F0", "0FF0", "FFF0", "000F", "F00F", "0F0F", "FF0F", "00FF", "F0FF", "0FFF", "FFFF" };
            result += familycode[(int)family - 97];

            for (int i = 0; i < 4; i++)
            {
                result += deviceGroupCode[3 - i] == '1' ? 'F' : '0';
            }

            result += "0FF";
            result += status ? "F" : "0";

            return result;
        }

        /**
          * Turns a decimal value to its binary representation
          */
        public static string DecimalToBinary(long Dec, int bitLength)
        {
            string result = Convert.ToString(Dec, 2);
            while (result.Length < bitLength)
            {
                result = "0" + result;
            }
            return result.Substring(result.Length - bitLength, bitLength);
        }
    }
}
