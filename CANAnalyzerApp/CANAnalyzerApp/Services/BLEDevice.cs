using Plugin.BLE;
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
        private List<ICharacteristic> characteristics;
        private bool connected = false;

        private const string frameHeader = "DSCA";
        private const int initFrameLength = 12;
  
        public async Task<bool> ConnectToDeviceAsync()
        {
            var ble = CrossBluetoothLE.Current;
            var adapter = CrossBluetoothLE.Current.Adapter;
            List<IDevice> deviceList = new List<IDevice>();

            adapter.DeviceDiscovered += (s, a) => deviceList.Add(a.Device);
            await adapter.StartScanningForDevicesAsync();

            IDevice nos3Device = deviceList.Find(x => x.Name == DeviceName);

            try
            {
                await adapter.ConnectToDeviceAsync(nos3Device);
                List<IService> services = (List<IService>)await nos3Device.GetServicesAsync();
                characteristics = (List<ICharacteristic>)await services[1].GetCharacteristicsAsync();
                connected = true;
            }
            catch (Exception e)
            {
                // ... could not connect to device
                connected = false;
                return await Task.FromResult(false);
            }

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
            if (!connected)
                return await Task.FromResult(false);
            else
            {
                ICharacteristic analyzerChar = characteristics.Find(x => x.Uuid == AnalyzerCharacteristic);

                if (analyzerChar != null)
                {
                    await SendInitCommand(4);

                    byte[] frame = new byte[12];

                    const UInt32 testCommand = 0x3F3F3F3F;

                    // Imposto l'header
                    Encoding.ASCII.GetBytes(frameHeader).CopyTo(frame, 0);

                    // Copio la lunghezza del messaggio da inviare successivamente

                    frame[4] = (byte)((testCommand & 0xFF000000) >> 24);
                    frame[5] = (byte)((testCommand & 0x00FF0000) >> 16);
                    frame[6] = (byte)((testCommand & 0x0000FF00) >> 8);
                    frame[7] = (byte)(testCommand & 0x000000FF);

                    UInt32 crc = Crc32_STM.CalculateBuffer(frame, 12 - 4);

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

        // Invia il frame di start della comunicazione
        // indicando la lunghezza del frame successivo
        // compresi la dimensione dell'header e del CRC finale
        public async Task<bool> SendInitCommand(UInt32 nextLength)
        {
            if (!connected)
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
                }
                else
                    return await Task.FromResult(false);
            }

            return await Task.FromResult(true);
        }
    }
}
