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

        private string fwVersion = "";
        private string serialNumber = "";

        public bool IsConnected()
        {
            return isConnected;
        }

        public bool IsConnecting()
        {
            return isConnecting;
        }

        public string GetSerialNumber()
        {
            return serialNumber;
        }

        public string GetFirmwareVersion()
        {
            return fwVersion;
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

                await GetDeviceInfo();
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

        public async Task<bool> GetDeviceInfo()
        {
            byte[] res = null;

            const UInt32 getSNCommand = 0x00000001;

            await SendReceiveInitCommand(4);
            await SendCommand(getSNCommand, 4);
            res = await ReceiveFrame();

            if (res != null && res.Length > 0)
            {
                UInt32 errorCode = ArrConverter.GetUInt32FromBuffer(res, 4);
                string marker = Encoding.ASCII.GetString(res, 0, 4);

                if (marker != frameMarker)
                    throw new Exception("Errore: marker non valido");

                if (errorCode != 0)
                    throw new Exception("Errore: " + errorCode);

                UInt32 serialNum = ArrConverter.GetUInt32FromBuffer(res, 8);
                serialNumber = serialNum.ToString("D8");
            }
            else
                throw new Exception("Dimensione della risposta errata!");

            const UInt32 getFWVerCommand = 0x00000002;
            await SendReceiveInitCommand(4);
            await SendCommand(getFWVerCommand, 4);
            res = await ReceiveFrame();

            if (res != null && res.Length > 0)
            {
                UInt32 errorCode = ArrConverter.GetUInt32FromBuffer(res, 4);
                string marker = Encoding.ASCII.GetString(res, 0, 4);

                if (marker != frameMarker)
                    throw new Exception("Errore: marker non valido");

                if (errorCode != 0)
                    throw new Exception("Errore: " + errorCode);

                string majorVer = ArrConverter.GetUInt16FromBuffer(res, 8).ToString();
                string minorVer = ArrConverter.GetUInt16FromBuffer(res, 10).ToString("D3");
                fwVersion = String.Format("{0}.{1}", majorVer, minorVer);
            }
            else
                throw new Exception("Dimensione della risposta errata!");

            return await Task.FromResult(true);
        }

        public async Task<bool> TestCommandAsync()
        {
            const UInt32 testCommand = 0x3F3F3F3F;

            await SendReceiveInitCommand(4);

            await SendCommand(testCommand, 4);

            return await Task.FromResult(true);
        }

        // Invia il frame di start della comunicazione
        // indicando la lunghezza del frame successivo
        // compresi la dimensione dell'header e del CRC finale
        public async Task<bool> SendReceiveInitCommand(UInt32 nextLength)
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
                        byte[] frame = new byte[4];

                        // Copio la lunghezza del messaggio da inviare successivamente
                        ArrConverter.SetBufferFromUInt32(nextLength, frame, 0);

                        await SendFrame(frame);

                        byte[] responseFrame = await ReceiveFrame();

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
                    byte[] frame = new byte[4];

                    // Copio il codice del comando
                    ArrConverter.SetBufferFromUInt32(command, frame, 0);

                    await SendFrame(frame);
                }
                else
                    return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }

        public async Task<bool> SendFrame(byte[] frame)
        {
            if (!isConnected)
                return await Task.FromResult(false);
            else
            {
                ICharacteristic analyzerChar = characteristics.Find(x => x.Uuid == AnalyzerCharacteristic);

                if (analyzerChar != null)
                {
                    byte[] frameToSend = new byte[8 + frame.Length];

                    // Imposto l'header
                    Encoding.ASCII.GetBytes(frameMarker).CopyTo(frameToSend, 0);

                    // Imposto il frame da inviare
                    frame.CopyTo(frameToSend, 4);

                    // Calcolo e imposto il crc
                    UInt32 crc = Crc32_STM.CalculateBuffer(frameToSend, frameToSend.Length - 4);
                    ArrConverter.SetBufferFromUInt32(crc, frameToSend, frameToSend.Length - 4);

                    await analyzerChar.WriteAsync(frameToSend);
                }
                else
                    return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }

        public async Task<byte[]> ReceiveFrame()
        {
            byte[] responseFrame = null;

            if (!isConnected)
                return await Task.FromResult(new byte[0]);
            else
            {
                ICharacteristic analyzerChar = characteristics.Find(x => x.Uuid == AnalyzerCharacteristic);
                
                analyzerChar.ValueUpdated += (o, args) =>
                {
                    responseFrame = args.Characteristic.Value;
                };

                await analyzerChar.StartUpdatesAsync();

                // Aggiungere controllo del CRC in arrivo
            }

            return await Task.FromResult(responseFrame);
        }
    }
}
