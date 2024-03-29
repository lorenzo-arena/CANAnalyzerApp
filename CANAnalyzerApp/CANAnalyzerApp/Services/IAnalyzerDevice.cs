﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CANAnalyzerApp.Models;

namespace CANAnalyzerApp.Services
{
    public enum SpyType
    {
        CANSpyOne,
        CANSpyTwo,
        KLineSpy
    }

    public interface IAnalyzerDevice
    {
        bool IsConnecting();
        bool IsConnected();

        string GetSerialNumber();
        string GetFirmwareVersion();

        Task<bool> ConnectToDeviceAsync();
        Task<bool> DisconnectFromDeviceAsync();
        Task<bool> TestCommandAsync();
        Task<bool> SetCANParametersAsync(SpyType type, CANSpyParameters param);
        Task<bool> SetKParametersAsync(SpyType type, KSpyParameters param);
        Task<bool> StartSpyAsync(SpyType type);
        Task<bool> StopSpyAsync(SpyType type);
        Task<int> GetSpyFileNumber(SpyFileType type);
        Task<List<string>> GetSpyFileNames(SpyFileType type);
        Task<List<int>> GetSpyFileSizes(SpyFileType type);
        Task<byte[]> GetSpyFile(SpyFileType type, string fileName);
        Task<byte[]> GetSpyBuffer(SpyType type);
    }
}
 