using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CANAnalyzerApp.Services
{
    public sealed class BLEDevice : IAnalyzerDevice
    {
        public static BLEDevice Instance { get; } = new BLEDevice();

        static BLEDevice()
        {

        }

        private BLEDevice()
        {

        }        

        private const string DeviceName = "CANAnalyzer";
        private const string AnalyzerCharacteristic = "ffe1";
        private List<ICharacteristic> characteristics = new List<ICharacteristic>();

        private const string frameMarker = "DSCA";
        private const int initFrameLength = 12;

        private bool isConnected = false;
        private bool isConnecting = false;

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

            //ICharacteristic analyzerChar = characteristics.Find(x => x.Uuid == AnalyzerCharacteristic);
            //analyzerChar.ValueUpdated += CharUpdated;

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
                try
                {
                    ICharacteristic analyzerChar = characteristics.Find(x => x.Uuid == AnalyzerCharacteristic);

                    if (analyzerChar != null)
                    {
                        byte[] frame = new byte[initFrameLength];

                        // Imposto l'header
                        Encoding.ASCII.GetBytes(frameMarker).CopyTo(frame, 0);

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

                        byte[] responseFrame = null;

                        analyzerChar.ValueUpdated += (o, args) =>
                        {
                            responseFrame = args.Characteristic.Value;
                        };

                        await analyzerChar.StartUpdatesAsync();

                        if (responseFrame != null && responseFrame.Length == 8)
                        {
                            UInt32 errorCode = BitConverter.ToUInt32(responseFrame, 4);
                            string marker = Encoding.ASCII.GetString(responseFrame, 0, 4);

                            if (marker != frameMarker)
                                throw new Exception("Errore: marker non valido");

                            if (errorCode != 0)
                                throw new Exception("Errore: " + errorCode);
                        }
                        else
                            throw new Exception("Dimensione della risposta errata!");

                        return await Task.FromResult(true);
                    }
                    else
                        return await Task.FromResult(false);
                }
                catch(Exception e)
                {
                    return await Task.FromResult(false);
                }
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
                    Encoding.ASCII.GetBytes(frameMarker).CopyTo(frame, 0);

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
