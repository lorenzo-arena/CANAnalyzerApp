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
                ICharacteristic testChar = characteristics.Find(x => x.Uuid == AnalyzerCharacteristic);

                if (testChar != null)
                    await testChar.WriteAsync(Encoding.ASCII.GetBytes("testtest"));
                else
                    return await Task.FromResult(false);
            }

            return await Task.FromResult(true);
        }
    }
}
