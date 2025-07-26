using Cypherly.Identity.Domain.Entities;
using Cypherly.Identity.Domain.Enums;

namespace Cypherly.Identity.Domain.Services.User;

public interface IDeviceService
{
    public Device RegisterDevice(Identity.Domain.Aggregates.User user, string devicePublicKey,
        string deviceAppVersion, DeviceType deviceType, DevicePlatform devicePlatform);
}

public class DeviceService : IDeviceService
{
    public Device RegisterDevice(Identity.Domain.Aggregates.User user, string devicePublicKey, string deviceAppVersion, DeviceType deviceType, DevicePlatform devicePlatform)
    {
        var device = new Device(Guid.NewGuid(), devicePublicKey, deviceAppVersion, deviceType, devicePlatform, user.Id);

        user.AddDevice(device);

        return device;
    }
}