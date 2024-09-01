using System;

namespace MetalMachine.Services;

public interface IGeocodingService
{
    Task<(double, double)?> GeocodeAddress (string address);

}
