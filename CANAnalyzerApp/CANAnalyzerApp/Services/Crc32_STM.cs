﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace CANAnalyzerApp.Services
{
    public static class Crc32_STM
    {
        public static UInt32 CalculateWord(UInt32 crc, UInt32 data)
        {
            UInt32 bitIndex;
            crc = crc ^ data;
            for (bitIndex = 0; bitIndex < 32; bitIndex++)
            {
                if ((crc & 0x80000000) != 0)
                {
                    crc = (crc << 1) ^ 0x04C11DB7;
                    // Polynomial used in STM32
                }
                else
                {
                    crc = (crc << 1);
                }
            }
            return crc;
        }

        public static UInt32 CalculateFromBuffer(UInt32 initialValue, UInt32[] buff)
        {
            UInt32 crcValue = initialValue;
            for (int index = 0; index < buff.Length; index++)
            {
                crcValue = CalculateWord(crcValue, buff[index]);
            }

            return crcValue;
        }

        public static UInt32 CalculateFromBuffer(byte[] buffBytes, int length)
        {
            UInt32[] buff = new UInt32[(length % 4) == 0 ? (length / 4) : ((int)(length / 4) + 1)];
            Buffer.BlockCopy(buffBytes, 0, buff, 0, length);

            if((length % 4) != 0)
            {
                buff[buff.Length - 1] = buff[buff.Length - 1] << (8 * (4 - (length % 4)));
            }

            UInt32 crcValue = 0xFFFFFFFFu;
            for (int index = 0; index < buff.Length; index++)
            {
                crcValue = CalculateWord(crcValue, buff[index]);
            }

            return crcValue;
        }
    }
}