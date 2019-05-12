using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CANAnalyzerApp.Models;

namespace CANAnalyzerApp.Services
{
    public class BLEDevice : IAnalyzerDevice
    {
        public static BLEDevice Instance { get; } = new BLEDevice();

        static BLEDevice()
        {

        }

        private BLEDevice()
        {

        }        

        private const string _deviceName = "CANAnalyzer";
        private const string _analyzerCharacteristic = "ffe1";
        ICharacteristic _analyzerChar = null;
        byte[] _responseFrame = null;

        private const string _frameMarker = "DSCA";
        private const int _initFrameLength = 12;

        private bool _isConnected = false;
        private bool _isConnecting = false;

        private string _fwVersion = "";
        private string _serialNumber = "";

        public bool IsConnected()
        {
            return _isConnected;
        }

        public bool IsConnecting()
        {
            return _isConnecting;
        }

        public string GetSerialNumber()
        {
            return _serialNumber;
        }

        public string GetFirmwareVersion()
        {
            return _fwVersion;
        }

        protected virtual void OnCharacteristicValueUpdated(object o, CharacteristicUpdatedEventArgs args)
        {
            _responseFrame = args.Characteristic.Value;
        }

        public async Task<bool> ConnectToDeviceAsync()
        {
            var ble = CrossBluetoothLE.Current;
            var adapter = CrossBluetoothLE.Current.Adapter;
            List<IDevice> deviceList = new List<IDevice>();

            _isConnecting = true;

            adapter.DeviceDiscovered += (s, a) => deviceList.Add(a.Device);
            await adapter.StartScanningForDevicesAsync();

            IDevice device = deviceList.Find(x => x.Name == _deviceName);

            if(device == null)
                return await Task.FromResult(false);

            try
            {
                await adapter.ConnectToDeviceAsync(device);
                
                _isConnected = true;
            }
            catch (Exception e)
            {
                // Non ho potuto connettermi al dispositivo
                _isConnected = false;
                _isConnecting = false;

                throw e;
            }

            try
            {
                List<IService> services = (List<IService>)await device.GetServicesAsync();
                List<ICharacteristic> characteristics = (List<ICharacteristic>)await services[1].GetCharacteristicsAsync();
                _analyzerChar = characteristics.Find(x => x.Uuid == _analyzerCharacteristic);

                _analyzerChar.ValueUpdated += OnCharacteristicValueUpdated;
                await _analyzerChar.StartUpdatesAsync();

                await GetDeviceInfo();
            }
            catch (Exception e)
            {
                // C'è stato un errore successivamente
                _isConnected = false;
                _isConnecting = false;

                await adapter.DisconnectDeviceAsync(device);

                throw e;
            }

            _isConnecting = false;

            //ICharacteristic analyzerChar = characteristics.Find(x => x.Uuid == AnalyzerCharacteristic);
            //analyzerChar.ValueUpdated += CharUpdated;

            return await Task.FromResult(true);
        }

        public async Task<bool> DisconnectFromDeviceAsync()
        {
            _isConnecting = true;

            var connectedDevices = new List<IDevice>(CrossBluetoothLE.Current.Adapter.ConnectedDevices);

            var device = connectedDevices.Find(x => x.Name == _deviceName);

            if (device == null)
                return await Task.FromResult(false);

            try
            {
                await CrossBluetoothLE.Current.Adapter.DisconnectDeviceAsync(device);
                _isConnected = false;
            }
            catch (Exception e)
            {
                _isConnected = false;
                _isConnecting = false;
                throw e;
            }

            _isConnecting = false;

            return await Task.FromResult(true);
        }

        public async Task<bool> SetCANParametersAsync(SpyType type, CANSpyParameters param)
        {
            const UInt32 setParamCAN1Spy = 0x00010003;
            const UInt32 setParamCAN2Spy = 0x00020003;

            byte[] paramData = new byte[CANSpyParameters.ParamSize];
            ArrConverter.SetBufferFromUInt32((UInt32)param.BitTiming, paramData, 0);
            ArrConverter.SetBufferFromUInt32((UInt32)param.FrameFormat, paramData, 4);

            if(param.ErrorReception)
                ArrConverter.SetBufferFromUInt32((UInt32)1, paramData, 8);
            else
                ArrConverter.SetBufferFromUInt32((UInt32)0, paramData, 8);

            if (param.ApplyMask)
                ArrConverter.SetBufferFromUInt32((UInt32)1, paramData, 12);
            else
                ArrConverter.SetBufferFromUInt32((UInt32)0, paramData, 12);

            ArrConverter.SetBufferFromUInt32((UInt32)param.Mask, paramData, 16);
            ArrConverter.SetBufferFromUInt32((UInt32)param.ID, paramData, 20);

            if (type == SpyType.CANSpyOne)
            {
                await SendReceiveInitCommand((UInt32)(4 + paramData.Length));
                await SendCommandWithBuffer(setParamCAN1Spy, paramData);
                await ReceiveFrame();
            }
            else if (type == SpyType.CANSpyTwo)
            {
                await SendReceiveInitCommand((UInt32)(4 + paramData.Length));
                await SendCommandWithBuffer(setParamCAN2Spy, paramData);
                await ReceiveFrame();
            }

            return await Task.FromResult(true);
        }

        public async Task<bool> SetKParametersAsync(SpyType type, KSpyParameters param)
        {
            return await Task.FromResult(true);
        }

        public async Task<bool> StartSpyAsync(SpyType type)
        {
            const UInt32 startCAN1Spy = 0x00010001;
            const UInt32 startCAN2Spy = 0x00020001;

            if (type == SpyType.CANSpyOne)
            {
                await SendReceiveInitCommand(4);
                await SendCommand(startCAN1Spy);
                await ReceiveFrame();
            }
            else if (type == SpyType.CANSpyTwo)
            {
                await SendReceiveInitCommand(4);
                await SendCommand(startCAN2Spy);
                await ReceiveFrame();
            }

            return await Task.FromResult(true);
        }

        public async Task<bool> StopSpyAsync(SpyType type)
        {
            const UInt32 stopCAN1Spy = 0x00010002;
            const UInt32 stopCAN2Spy = 0x00010004;

            if (type == SpyType.CANSpyOne)
            {
                await SendReceiveInitCommand(4);
                await SendCommand(stopCAN1Spy);
                await ReceiveFrame();
            }
            else if (type == SpyType.CANSpyTwo)
            {
                await SendReceiveInitCommand(4);
                await SendCommand(stopCAN2Spy);
                await ReceiveFrame();
            }

            return await Task.FromResult(true);
        }

        public async Task<bool> GetDeviceInfo()
        {
            byte[] res = null;

            const UInt32 getSNCommand = 0x00000001;

            await SendReceiveInitCommand(4);
            await SendCommand(getSNCommand);
            res = await ReceiveFrame();

            UInt32 serialNum = ArrConverter.GetUInt32FromBuffer(res, 8);
            _serialNumber = serialNum.ToString("D8");

            const UInt32 getFWVerCommand = 0x00000002;
            await SendReceiveInitCommand(4);
            await SendCommand(getFWVerCommand);
            res = await ReceiveFrame();

            string majorVer = ArrConverter.GetUInt16FromBuffer(res, 8).ToString();
            string minorVer = ArrConverter.GetUInt16FromBuffer(res, 10).ToString("D3");
            _fwVersion = String.Format("{0}.{1}", majorVer, minorVer);

            return await Task.FromResult(true);
        }

        public async Task<List<SpyFile>> GetSpyFiles(SpyFileType type)
        {
            var files = new List<SpyFile>();

            UInt32 getListCommand;

            if (type == SpyFileType.FileTypeCAN1)
                getListCommand = 0x00000003;
            else if (type == SpyFileType.FileTypeCAN2)
                getListCommand = 0x00000004;
            else if (type == SpyFileType.FileTypeK)
                getListCommand = 0x00000005;
            else
                throw new Exception("FileType not implemented!");

            await SendReceiveInitCommand(4);
            await SendCommand(getListCommand);
            var response = await ReceiveFrame();

            // La risposta è costituita dalle stringhe (NOMI) dei vari file separate
            // da byte di fine stringa

            throw new Exception("Implementare l'estrazione delle info!");

            return files;
        }

        public async Task<bool> TestCommandAsync()
        {
            const UInt32 blinkCommand = 0x3F3F0000;
            const UInt32 sleepCommand = 0x3F3F0001;

            await SendReceiveInitCommand(4);

            //await SendCommand(blinkCommand);
            await SendCommand(sleepCommand);
            await ReceiveFrame();

            return await Task.FromResult(true);
        }

        // Invia il frame di start della comunicazione
        // indicando la lunghezza del frame successivo
        // compresi la dimensione dell'header e del CRC finale
        public async Task<bool> SendReceiveInitCommand(UInt32 nextLength)
        {
            if (!_isConnected)
                return await Task.FromResult(false);
            else
            {
                try
                {
                    if (_analyzerChar != null)
                    {
                        byte[] frame = new byte[4];

                        // Copio la lunghezza del messaggio da inviare successivamente
                        ArrConverter.SetBufferFromUInt32(nextLength, frame, 0);

                        await SendFrame(frame);
                        await ReceiveFrame();

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

        public async Task<bool> SendCommand(UInt32 command)
        {
            if (!_isConnected)
                return await Task.FromResult(false);
            else
            {
                if (_analyzerChar != null)
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

        public async Task<bool> SendCommandWithBuffer(UInt32 command, byte[] buff)
        {
            if (!_isConnected)
                return await Task.FromResult(false);
            else
            {
                if (_analyzerChar != null)
                {
                    byte[] frame = new byte[4 + buff.Length];

                    // Copio il codice del comando
                    ArrConverter.SetBufferFromUInt32(command, frame, 0);
                    buff.CopyTo(frame, 4);

                    await SendFrame(frame);
                }
                else
                    return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }

        public async Task<bool> SendFrame(byte[] frame)
        {
            if (!_isConnected)
                return await Task.FromResult(false);
            else
            {
                if (_analyzerChar != null)
                {
                    _responseFrame = null;
                    byte[] frameToSend = new byte[8 + frame.Length];

                    // Imposto l'header
                    Encoding.ASCII.GetBytes(_frameMarker).CopyTo(frameToSend, 0);

                    // Imposto il frame da inviare
                    frame.CopyTo(frameToSend, 4);

                    // Calcolo e imposto il crc
                    UInt32 crc = Crc32_STM.CalculateFromBuffer(frameToSend, frameToSend.Length - 4);
                    ArrConverter.SetBufferFromUInt32(crc, frameToSend, frameToSend.Length - 4);

                    await _analyzerChar.WriteAsync(frameToSend);
                }
                else
                    return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }

        public async Task<byte[]> ReceiveFrame()
        {
            const UInt32 waitCommand = 0x00040000;
            try
            {
                if (!_isConnected)
                    return await Task.FromResult(new byte[0]);
                else
                {
                    bool isWaitCommand = false;

                    do
                    {
                        await Task.Delay(100);
                        await _analyzerChar.StopUpdatesAsync();

                        if (_responseFrame == null || _responseFrame.Length == 0)
                            throw new Exception("dimensione della risposta errata!");

                        // Controllo il marker
                        string marker = Encoding.ASCII.GetString(_responseFrame, 0, 4);
                        if (marker != _frameMarker)
                            throw new Exception("invalid marker");

                        // Controllo il crc
                        UInt32 crcSent = ArrConverter.GetUInt32FromBuffer(_responseFrame, _responseFrame.Length - 4);
                        UInt32 crcCalc = Crc32_STM.CalculateFromBuffer(_responseFrame, _responseFrame.Length - 4);

                        if (crcSent != crcCalc)
                            throw new Exception("invalid crc");

                        // Controllo se ho una risposta con codice di errore
                        UInt32 errorCode = ArrConverter.GetUInt32FromBuffer(_responseFrame, 4);
                        if (errorCode != 0)
                            throw new Exception(errorCode.ToString());

                        if(_responseFrame.Length == 16)
                        {
                            if (ArrConverter.GetUInt32FromBuffer(_responseFrame, 8) == waitCommand)
                                isWaitCommand = true;
                        }

                        await _analyzerChar.StartUpdatesAsync();
                    }
                    while (isWaitCommand);
                }

                await _analyzerChar.StartUpdatesAsync();

                return await Task.FromResult(_responseFrame);
            }
            catch (Exception ex)
            {
                await _analyzerChar.StartUpdatesAsync();
                throw ex;
            }
        }
    }
}
