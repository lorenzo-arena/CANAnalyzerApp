﻿using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CANAnalyzerApp.Models;
using System.Linq;
using Xamarin.Forms;

namespace CANAnalyzerApp.Services
{
    
    public class BLEDevice : IAnalyzerDevice
    {
        private struct Response
        {
            public UInt32 responseData;
            public byte[] responseBuff;
        }

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
                await SendReceiveCommand(setParamCAN1Spy, paramData);
            else if (type == SpyType.CANSpyTwo)
                await SendReceiveCommand(setParamCAN2Spy, paramData);

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
                await SendReceiveCommand(startCAN1Spy);
            else if (type == SpyType.CANSpyTwo)
                await SendReceiveCommand(startCAN2Spy);

            return await Task.FromResult(true);
        }

        public async Task<bool> StopSpyAsync(SpyType type)
        {
            const UInt32 stopCAN1Spy = 0x00010002;
            const UInt32 stopCAN2Spy = 0x00010004;

            if (type == SpyType.CANSpyOne)
                await SendReceiveCommand(stopCAN1Spy);
            else if (type == SpyType.CANSpyTwo)
                await SendReceiveCommand(stopCAN2Spy);

            return await Task.FromResult(true);
        }

        public async Task<bool> GetDeviceInfo()
        {
            Response res;

            const UInt32 getSNCommand = 0x00000001;
            res = await SendReceiveCommand(getSNCommand);

            _serialNumber = res.responseData.ToString("D8");

            const UInt32 getFWVerCommand = 0x00000002;
            res = await SendReceiveCommand(getFWVerCommand);

            string majorVer = (res.responseData >> 16).ToString();
            string minorVer = (res.responseData & 0x0000FFFF).ToString("D3");
            _fwVersion = String.Format("{0}.{1}", majorVer, minorVer);

            return await Task.FromResult(true);
        }

        public async Task<int> GetSpyFileNumber(SpyFileType type)
        {
            var fileNames = new List<string>();

            UInt32 getNumCommand;

            if (type == SpyFileType.FileTypeCAN1)
                getNumCommand = 0x00000003;
            else if (type == SpyFileType.FileTypeCAN2)
                getNumCommand = 0x00000004;
            else if (type == SpyFileType.FileTypeK)
                getNumCommand = 0x00000005;
            else
                throw new Exception("FileType not implemented!");

            var res = await SendReceiveCommand(getNumCommand);

            return (int)res.responseData;
        }

        public async Task<List<string>> GetSpyFileNames(SpyFileType type)
        {
            var fileNames = new List<string>();

            UInt32 getNumCommand;
            UInt32 getFileNameCommand;

            if (type == SpyFileType.FileTypeCAN1)
            {
                getNumCommand = 0x00000003;
                getFileNameCommand = 0x00000006;
            }
            else if (type == SpyFileType.FileTypeCAN2)
            {
                getNumCommand = 0x00000004;
                getFileNameCommand = 0x00000007;
            }
            else if (type == SpyFileType.FileTypeK)
            {
                getNumCommand = 0x00000005;
                getFileNameCommand = 0x00000008;
            }
            else
                throw new Exception("FileType not implemented!");

            var getNumResponse = await SendReceiveCommand(getNumCommand);

            var filesNum = getNumResponse.responseData;

            for(UInt32 fileIndex = 0; fileIndex < filesNum; fileIndex++)
            {
                var fileIndexBuf = new byte[4];
                ArrConverter.SetBufferFromUInt32(fileIndex, fileIndexBuf, 0);

                var getFileNameResponse = await SendReceiveCommand(getFileNameCommand, fileIndexBuf);

                var fileName = Encoding.ASCII.GetString(getFileNameResponse.responseBuff);
                fileNames.Add(fileName);
            }

            return fileNames;
        }

        public async Task<List<int>> GetSpyFileSizes(SpyFileType type)
        {
            var fileSizes = new List<int>();

            UInt32 getNumCommand;
            UInt32 getFileSizeCommand;

            if (type == SpyFileType.FileTypeCAN1)
            {
                getNumCommand = 0x00000003;
                getFileSizeCommand = 0x00000009;
            }
            else if (type == SpyFileType.FileTypeCAN2)
            {
                getNumCommand = 0x00000004;
                getFileSizeCommand = 0x0000000A;
            }
            else if (type == SpyFileType.FileTypeK)
            {
                getNumCommand = 0x00000005;
                getFileSizeCommand = 0x0000000B;
            }
            else
                throw new Exception("FileType not implemented!");

            var getNumResponse = await SendReceiveCommand(getNumCommand);

            var filesNum = getNumResponse.responseData;

            for (UInt32 fileIndex = 0; fileIndex < filesNum; fileIndex++)
            {
                var fileIndexBuf = new byte[4];
                ArrConverter.SetBufferFromUInt32(fileIndex, fileIndexBuf, 0);

                var getFileSizeResponse = await SendReceiveCommand(getFileSizeCommand, fileIndexBuf);

                UInt32 fileSize = getFileSizeResponse.responseData;
                fileSizes.Add((int)fileSize);
            }

            return fileSizes;
        }

        public async Task<byte[]> GetSpyFile(SpyFileType type, string fileName)
        {
            byte[] fileNameBuff = new byte[20];
            UInt32 getFileCommand;

            if (type == SpyFileType.FileTypeCAN1)
                getFileCommand = 0x0000000C;
            else if (type == SpyFileType.FileTypeCAN2)
                getFileCommand = 0x0000000D;
            else if (type == SpyFileType.FileTypeK)
                getFileCommand = 0x0000000E;
            else
                throw new Exception("FileType not implemented!");

            if (fileName.Length <= fileNameBuff.Length)
                Encoding.ASCII.GetBytes(fileName).CopyTo(fileNameBuff, 0);
            else
                throw new Exception("Nome del file troppo lungo!");

            var res = await SendReceiveCommand(getFileCommand, fileNameBuff);

            return res.responseBuff;
        }

        public async Task<byte[]> GetSpyBuffer(SpyType type)
        {
            UInt32 getBufferCommand;

            if (type == SpyType.CANSpyOne)
                getBufferCommand = 0x00010004;
            else if (type == SpyType.CANSpyTwo)
                getBufferCommand = 0x00020004;
            else if (type == SpyType.KLineSpy)
                getBufferCommand = 0x00030004;
            else
                throw new Exception("FileType not implemented!");

            var res = await SendReceiveCommand(getBufferCommand);

            return res.responseBuff;
        }

        public async Task<bool> TestCommandAsync()
        {
            const UInt32 blinkCommand = 0x3F3F0000;
            const UInt32 sleepCommand = 0x3F3F0001;

            await SendReceiveCommand(sleepCommand);

            return await Task.FromResult(true);
        }

        // Invia il frame di start della comunicazione
        // indicando la lunghezza del frame successivo
        // compresi la dimensione dell'header e del CRC finale
        private async Task<bool> SendReceiveInitCommand(UInt32 nextLength)
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

        private async Task<Response> SendReceiveCommand(UInt32 command)
        {
            await SendReceiveInitCommand(4);
            await SendCommand(command);
            var receivedBuff = await ReceiveFrame();

            // La risposta è composta da:
            // 1 - un marker (4 byte)
            // 2 - il codice di errore (4 byte)
            // 3 - un codice di risultato (4 byte)
            // 4 - se necessario un buffer
            // 5 - il crc del messaggio (4 byte)
            var result = new Response();
            result.responseData = ArrConverter.GetUInt32FromBuffer(receivedBuff, 8);

            if (receivedBuff.Length > 16)
                result.responseBuff = receivedBuff.Skip(12).Take(receivedBuff.Length - 16).ToArray();
            else
                result.responseBuff = null;

            return result;
        }

        private async Task<Response> SendReceiveCommand(UInt32 command, byte[] buffer)
        {
            await SendReceiveInitCommand((UInt32)(4 + buffer.Length));
            await SendCommandWithBuffer(command, buffer);
            var receivedBuff = await ReceiveFrame();

            // La risposta è composta da:
            // 1 - un marker (4 byte)
            // 2 - il codice di errore (4 byte)
            // 3 - un codice di risultato (4 byte)
            // 4 - se necessario un buffer
            // 5 - il crc del messaggio (4 byte)
            var result = new Response();
            result.responseData = ArrConverter.GetUInt32FromBuffer(receivedBuff, 8);

            if (receivedBuff.Length > 16)
                result.responseBuff = receivedBuff.Skip(12).Take(receivedBuff.Length - 16).ToArray();
            else
                result.responseBuff = null;

            return result;
        }

        private async Task<bool> SendCommand(UInt32 command)
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

        private async Task<bool> SendCommandWithBuffer(UInt32 command, byte[] buff)
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

        private async Task<bool> SendFrame(byte[] frame)
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

        private async Task<byte[]> ReceiveFrame()
        {
            const UInt32 waitCommand = 0x00040000;
            const UInt32 packetCommand = 0x3F3F3F3F;
            const UInt32 bigFileCommand = 0x3F3F0001;

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
                        {
                            string tmp = "";
                            foreach (byte data in _responseFrame)
                                tmp += data.ToString("X2");

                            throw new Exception("invalid marker, received: " + tmp);
                        }

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
                        else if (_responseFrame.Length == 20)
                        {
                            if (ArrConverter.GetUInt32FromBuffer(_responseFrame, 8) == packetCommand)
                            {
                                int totalSize = (int)ArrConverter.GetUInt32FromBuffer(_responseFrame, 12);
                                int packetsNum = ((totalSize % 20) == 0) ? (totalSize / 20) : ((totalSize / 20) + 1);

                                var fullResponse = new byte[totalSize];

                                // Il device cerca di trasmettere un pacchetto
                                await _analyzerChar.StartUpdatesAsync();
                                await SendCommand(packetCommand);

                                for(var packetIndex = 0; packetIndex < packetsNum; packetIndex++)
                                {
                                    await Task.Delay(100);
                                    await _analyzerChar.StopUpdatesAsync();

                                    if (_responseFrame == null || _responseFrame.Length == 0)
                                        throw new Exception("dimensione della risposta errata!");

                                    _responseFrame.CopyTo(fullResponse, 20 * packetIndex);

                                    await _analyzerChar.StartUpdatesAsync();
                                    await SendCommand(packetCommand);
                                }

                                return await Task.FromResult(fullResponse);
                            }
                            else if(ArrConverter.GetUInt32FromBuffer(_responseFrame, 8) == bigFileCommand)
                            {
                                int totalSize = (int)ArrConverter.GetUInt32FromBuffer(_responseFrame, 12);
                                int receivedLength = 0;

                                // Alloco un buffer per l'intero file e il preambolo
                                // Per adesso ignoro il preambolo e copio soltanto il buffer contenente il file
                                byte[] fullRespose = new byte[totalSize + 16];

                                // Il device cerca di trasmettere un pacchetto
                                await _analyzerChar.StartUpdatesAsync();
                                await SendCommand(packetCommand);

                                while(receivedLength < totalSize)
                                {
                                    await Task.Delay(100);
                                    await _analyzerChar.StopUpdatesAsync();

                                    if (_responseFrame == null || _responseFrame.Length == 0)
                                        throw new Exception("dimensione della risposta errata!");

                                    // Copio partendo da 12 e poi in base alla dimensione dei pacchetti ricevuti
                                    Buffer.BlockCopy(_responseFrame, 0, fullRespose, receivedLength + 12, _responseFrame.Length);

                                    MessagingCenter.Send<IAnalyzerDevice, double>(this, "DownloadFileProgress", (double)receivedLength / totalSize);

                                    receivedLength += _responseFrame.Length;

                                    await _analyzerChar.StartUpdatesAsync();
                                    await SendCommand(packetCommand);
                                }

                                return await Task.FromResult(fullRespose);
                            }
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
