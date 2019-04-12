﻿using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CANAnalyzerApp.Services
{
    class BLEDevice : IAnalyzerDevice
    {
        private const string DeviceName = "CANAnalyzer";
        private const string AnalyzerCharacteristic = "ffe1";
        private List<ICharacteristic> characteristics = new List<ICharacteristic>();

        private const string frameHeader = "DSCA";
        private const int initFrameLength = 12;

        private bool isConnected = false;
        private bool isConnecting = false;

        public BLEDevice()
        {

        }

        public bool IsConnected()
        {
            return isConnected;
        }

        public bool IsConnecting()
        {
            return isConnecting;
        }

        public async Task<bool> ConnectToDeviceAsync()
        {
            var ble = CrossBluetoothLE.Current;
            var adapter = CrossBluetoothLE.Current.Adapter;
            List<IDevice> deviceList = new List<IDevice>();

            isConnecting = true;

            adapter.DeviceDiscovered += (s, a) => deviceList.Add(a.Device);
            await adapter.StartScanningForDevicesAsync();

            IDevice device = deviceList.Find(x => x.Name == DeviceName);

            try
            {
                await adapter.ConnectToDeviceAsync(device);
                List<IService> services = (List<IService>)await device.GetServicesAsync();
                characteristics = (List<ICharacteristic>)await services[1].GetCharacteristicsAsync();
                isConnected = true;
            }
            catch (Exception e)
            {
                // ... could not connect to device
                isConnected = false;
                isConnecting = false;
                return await Task.FromResult(false);
            }

            isConnecting = false;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetCANParametersAsync(CANParam param)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> SetKParametersAsync(KParam param)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> StartSpyAsync(SpyType type)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> StopSpyAsync(SpyType type)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> TestCommandAsync()
        {
            const UInt32 testCommand = 0x3F3F3F3F;

            await SendInitCommand(4);

            await SendCommand(testCommand, 4);

            return await Task.FromResult(true);
        }

        // Invia il frame di start della comunicazione
        // indicando la lunghezza del frame successivo
        // compresi la dimensione dell'header e del CRC finale
        public async Task<bool> SendInitCommand(UInt32 nextLength)
        {
            if (!isConnected)
                return await Task.FromResult(false);
            else
            {
                ICharacteristic analyzerChar = characteristics.Find(x => x.Uuid == AnalyzerCharacteristic);

                if (analyzerChar != null)
                {
                    byte[] frame = new byte[initFrameLength];

                    // Imposto l'header
                    Encoding.ASCII.GetBytes(frameHeader).CopyTo(frame, 0);

                    // Copio la lunghezza del messaggio da inviare successivamente
                    frame[4] = (byte)((nextLength & 0xFF000000) >> 24);
                    frame[5] = (byte)((nextLength & 0x00FF0000) >> 16);
                    frame[6] = (byte)((nextLength & 0x0000FF00) >> 8);
                    frame[7] = (byte)(nextLength & 0x000000FF);

                    UInt32 crc = Crc32_STM.CalculateBuffer(frame, initFrameLength - 4);

                    // Copio la lunghezza del messaggio da inviare successivamente
                    frame[8] = (byte)((crc & 0xFF000000) >> 24);
                    frame[9] = (byte)((crc & 0x00FF0000) >> 16);
                    frame[10] = (byte)((crc & 0x0000FF00) >> 8);
                    frame[11] = (byte)(crc & 0x000000FF);

                    await analyzerChar.WriteAsync(frame);

                    var response1 = await analyzerChar.ReadAsync();
                    var response2 = await analyzerChar.ReadAsync();
                    var response3 = await analyzerChar.ReadAsync();
                    var response4 = await analyzerChar.ReadAsync();
                    var response5 = await analyzerChar.ReadAsync();
                    var response6 = await analyzerChar.ReadAsync();
                    var response7 = await analyzerChar.ReadAsync();
                    var response8 = await analyzerChar.ReadAsync();
                    var response9 = await analyzerChar.ReadAsync();

                    int pinco = 0;
                    pinco = 1;
                }
                else
                    return await Task.FromResult(false);
            }

            return await Task.FromResult(true);
        }

        public async Task<bool> SendCommand(UInt32 command, UInt32 length)
        {
            if (!isConnected)
                return await Task.FromResult(false);
            else
            {
                ICharacteristic analyzerChar = characteristics.Find(x => x.Uuid == AnalyzerCharacteristic);

                if (analyzerChar != null)
                {
                    byte[] frame = new byte[8 + length];

                    // Imposto l'header
                    Encoding.ASCII.GetBytes(frameHeader).CopyTo(frame, 0);

                    // Copio la lunghezza del messaggio da inviare successivamente
                    frame[4] = (byte)((command & 0xFF000000) >> 24);
                    frame[5] = (byte)((command & 0x00FF0000) >> 16);
                    frame[6] = (byte)((command & 0x0000FF00) >> 8);
                    frame[7] = (byte)(command & 0x000000FF);

                    UInt32 crc = Crc32_STM.CalculateBuffer(frame, initFrameLength - 4);

                    // Copio la lunghezza del messaggio da inviare successivamente
                    frame[8] = (byte)((crc & 0xFF000000) >> 24);
                    frame[9] = (byte)((crc & 0x00FF0000) >> 16);
                    frame[10] = (byte)((crc & 0x0000FF00) >> 8);
                    frame[11] = (byte)(crc & 0x000000FF);

                    await analyzerChar.WriteAsync(frame);
                }
                else
                    return await Task.FromResult(false);
            }

            return await Task.FromResult(true);
        }
    }
}
