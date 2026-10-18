using System.Numerics;
using NexaStore.Domain.Common;

namespace NexaStore.Domain.Catalog;

public sealed record ProductTechnicalDetails
{
    public ProductTechnicalDetails(
        string? cpu = null,
        int? cpuCores = null,
        int? ramGb = null,
        int? maxRamGb = null,
        string? gpu = null,
        int? vramGb = null,
        string? npu = null,
        int? npuTops = null,
        int? storageGb = null,
        string? storageType = null,
        string? display = null,
        decimal? batteryWh = null,
        decimal? weightKg = null)
    {
        ValidateNonNegative(cpuCores, nameof(cpuCores));
        ValidateNonNegative(ramGb, nameof(ramGb));
        ValidateNonNegative(maxRamGb, nameof(maxRamGb));
        ValidateNonNegative(vramGb, nameof(vramGb));
        ValidateNonNegative(npuTops, nameof(npuTops));
        ValidateNonNegative(storageGb, nameof(storageGb));
        ValidateNonNegative(batteryWh, nameof(batteryWh));
        ValidateNonNegative(weightKg, nameof(weightKg));

        if (ramGb is not null && maxRamGb is not null && maxRamGb < ramGb)
        {
            throw new DomainException("Maximum RAM cannot be less than installed RAM.");
        }

        Cpu = cpu;
        CpuCores = cpuCores;
        RamGb = ramGb;
        MaxRamGb = maxRamGb;
        Gpu = gpu;
        VramGb = vramGb;
        Npu = npu;
        NpuTops = npuTops;
        StorageGb = storageGb;
        StorageType = storageType;
        Display = display;
        BatteryWh = batteryWh;
        WeightKg = weightKg;
    }

    public string? Cpu { get; }
    public int? CpuCores { get; }
    public int? RamGb { get; }
    public int? MaxRamGb { get; }
    public string? Gpu { get; }
    public int? VramGb { get; }
    public string? Npu { get; }
    public int? NpuTops { get; }
    public int? StorageGb { get; }
    public string? StorageType { get; }
    public string? Display { get; }
    public decimal? BatteryWh { get; }
    public decimal? WeightKg { get; }

    private static void ValidateNonNegative<T>(T? value, string fieldName) where T : struct, INumber<T>
    {
        if (value is not null && value.Value < T.Zero)
        {
            throw new DomainException($"{fieldName} cannot be negative.");
        }
    }
}
