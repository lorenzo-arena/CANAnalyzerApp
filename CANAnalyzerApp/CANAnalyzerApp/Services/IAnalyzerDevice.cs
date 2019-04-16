using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CANAnalyzerApp.Services
{
    public enum SpyType
    {
        CANSpyOne,
        CANSpyTwo,
        KLineSpy
    }

    public struct CANParam
    {
        UInt32 bitrate;
    }

    public struct KParam
    {
        UInt32 bitrate;
    }

    public interface IAnalyzerDevice
    {
        bool IsConnecting();
        bool IsConnected();

        string GetSerialNumber();
        string GetFirmwareVersion();

        Task<bool> ConnectToDeviceAsync();
        Task<bool> TestCommandAsync();
        Task<bool> SetCANParametersAsync(CANParam param);
        Task<bool> SetKParametersAsync(KParam param);
        Task<bool> StartSpyAsync(SpyType type);
        Task<bool> StopSpyAsync(SpyType type);
    }
}
 