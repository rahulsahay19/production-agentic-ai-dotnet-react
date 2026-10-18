using Microsoft.EntityFrameworkCore;
using NexaStore.Domain.Catalog;
using NexaStore.Domain.Customers;
using NexaStore.Domain.Inventory;

namespace NexaStore.Infrastructure.Persistence.Seeding;

public sealed class DevelopmentDataSeeder(NexaStoreDbContext dbContext)
{
    public static readonly Guid DemoCustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTimeOffset SeedDate = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!await dbContext.Products.AnyAsync(cancellationToken))
        {
            var categories = CreateCategories();
            await dbContext.Categories.AddRangeAsync(categories.Values, cancellationToken);

            var products = CreateProducts(categories);
            await dbContext.Products.AddRangeAsync(products.Select(item => item.Product), cancellationToken);
            await dbContext.Inventory.AddRangeAsync(
                products.Select(item => new InventoryItem(item.Product.Id, item.StockQuantity, SeedDate)),
                cancellationToken);
        }

        if (!await dbContext.Customers.AnyAsync(customer => customer.Id == DemoCustomerId, cancellationToken))
        {
            var customer = CreateCustomer();
            await dbContext.Customers.AddAsync(customer, cancellationToken);
            dbContext.Entry(customer).Property(item => item.Id).CurrentValue = DemoCustomerId;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Dictionary<string, Category> CreateCategories()
    {
        string[] names =
        [
            "AI Laptops", "Developer Laptops", "Gaming Laptops", "AI Workstations",
            "Mini PCs", "GPUs", "Monitors", "Storage", "Memory", "Docking Stations", "Accessories"
        ];

        return names.ToDictionary(
            name => name,
            name => new Category(name, Slugify(name), $"NexaStore {name.ToLowerInvariant()} collection."));
    }

    private static List<SeededProduct> CreateProducts(IReadOnlyDictionary<string, Category> categories)
    {
        ProductSeed[] seeds =
        [
            new("NB-PRO-X16", "NexaBook Pro X16", "AI Laptops", 249999m, "Intel Core Ultra 9-class", 64, 64, "RTX 5070 Laptop-class", 12, null, null, 2048, "16-inch 3.2K OLED", true, true, true, true, true),
            new(
                "NB-AIS16-2026",
                "NexaBook AI Studio 16",
                "AI Laptops",
                239990m,
                "AMD Ryzen AI 9-class",
                64,
                128,
                "RTX 5080 Laptop-class",
                16,
                "Ryzen AI NPU",
                50,
                2048,
                "16-inch 3.2K OLED",
                true,
                true,
                true,
                true,
                true,
                StockQuantity: 12,
                Brand: "Nexa",
                ShortDescription: "Mobile AI studio with 16 GB VRAM and memory expansion up to 128 GB.",
                Description: "A flagship mobile workstation for generative AI, computer vision, local LLM experimentation and professional creator workflows.",
                CpuCores: 12,
                BatteryWh: 99m,
                WeightKg: 2.35m,
                IsFeatured: true,
                Tags: ["GenAI", "Computer Vision", "Local LLM", "Creator", "Deep Learning"],
                Images:
                [
                    new("/images/products/nb-ais16-2026-1.webp", "NexaBook AI Studio 16", true),
                    new("/images/products/nb-ais16-2026-2.webp", "NexaBook AI Studio 16 alternate view", false)
                ]),
            new("NB-DEV-14", "NexaBook Developer 14", "Developer Laptops", 159999m, "Intel Core Ultra 7-class", 32, 64, "Integrated Intel Arc-class", null, "Intel AI Boost", 47, 1024, "14-inch 2.8K OLED", true, false, true, true, false),
            new("NAI-MINI", "NexaAI Mini", "Mini PCs", 119999m, "Intel Core Ultra 7-class", 32, 64, "RTX 4060 Laptop-class", 8, null, null, 1024, null, true, true, true, false, true),
            new("NWS-AI-1000", "NexaStation AI WS1000", "AI Workstations", 549999m, "Intel Core i9-class 24-core", 64, 256, "RTX 5090-class", 32, null, null, 2048, null, true, true, true, true, true),
            new("NGPU-A2000", "NexaGPU A2000", "GPUs", 39999m, null, null, null, "NexaGPU A2000", 8, null, null, null, null, true, false, true, true, true),
            new("NGPU-A4000", "NexaGPU A4000", "GPUs", 69999m, null, null, null, "NexaGPU A4000", 12, null, null, null, null, true, true, true, true, true),
            new("NGPU-A6000", "NexaGPU A6000", "GPUs", 129999m, null, null, null, "NexaGPU A6000", 24, null, null, null, null, true, true, true, true, true),
            new("NB-DEV-16", "NexaBook Developer 16", "Developer Laptops", 189999m, "AMD Ryzen 9-class", 32, 64, "RTX 4060 Laptop-class", 8, null, null, 1024, "16-inch 2.5K IPS", true, true, true, true, true),
            new("NB-AIR-13", "NexaBook Dev Air 13", "Developer Laptops", 109999m, "Intel Core Ultra 5-class", 16, 32, "Integrated Arc-class", null, "Intel AI Boost", 40, 512, "13.5-inch 2.2K IPS", true, false, true, false, false),
            new("NB-GAME-15", "NexaBook Gaming G15", "Gaming Laptops", 174999m, "AMD Ryzen 7-class", 32, 64, "RTX 4070 Laptop-class", 8, null, null, 1024, "15.6-inch QHD 240Hz", false, false, true, true, true),
            new("NB-GAME-17", "NexaBook Gaming G17", "Gaming Laptops", 229999m, "Intel Core i9-class", 32, 64, "RTX 4080 Laptop-class", 12, null, null, 2048, "17.3-inch QHD 240Hz", true, true, true, true, true),
            new("NWS-CREATOR", "NexaStation Creator C800", "AI Workstations", 379999m, "AMD Ryzen 9 16-core-class", 64, 128, "RTX 5080-class", 16, null, null, 2048, null, true, true, true, true, true),
            new("NMINI-DEV", "NexaMini Developer D7", "Mini PCs", 79999m, "AMD Ryzen 7-class", 32, 64, "Integrated Radeon-class", null, null, null, 1024, null, false, false, true, false, false),
            new("NMON-27-4K", "NexaView Pro 27 4K", "Monitors", 42999m, null, null, null, null, null, null, null, null, "27-inch 4K IPS", false, false, true, true, false),
            new("NMON-32-4K", "NexaView Creator 32 4K", "Monitors", 69999m, null, null, null, null, null, null, null, null, "32-inch 4K Mini-LED", false, false, true, true, false),
            new("NMON-34-UW", "NexaView UltraWide 34", "Monitors", 57999m, null, null, null, null, null, null, null, null, "34-inch WQHD OLED", false, false, true, true, true),
            new("NSSD-1TB", "NexaDrive NVMe 1 TB", "Storage", 8999m, null, null, null, null, null, null, null, 1024, null, false, false, true, true, true),
            new("NSSD-2TB", "NexaDrive NVMe 2 TB", "Storage", 15999m, null, null, null, null, null, null, null, 2048, null, false, false, true, true, true),
            new("NSSD-4TB", "NexaDrive Pro NVMe 4 TB", "Storage", 32999m, null, null, null, null, null, null, null, 4096, null, false, false, true, true, true),
            new("NRAM-32", "NexaMemory DDR5 32 GB Kit", "Memory", 9999m, null, 32, 32, null, null, null, null, null, null, false, false, true, true, true),
            new("NRAM-64", "NexaMemory DDR5 64 GB Kit", "Memory", 17999m, null, 64, 64, null, null, null, null, null, null, true, true, true, true, true),
            new("NRAM-128", "NexaMemory DDR5 128 GB Kit", "Memory", 36999m, null, 128, 128, null, null, null, null, null, null, true, true, true, true, true),
            new("NDOCK-TB4", "NexaDock Thunderbolt 4 Pro", "Docking Stations", 21999m, null, null, null, null, null, null, null, null, null, false, false, true, true, false),
            new("NDOCK-USB4", "NexaDock USB4 Compact", "Docking Stations", 12999m, null, null, null, null, null, null, null, null, null, false, false, true, true, false),
            new("NACC-KEY", "NexaKeys Developer Mechanical Keyboard", "Accessories", 11999m, null, null, null, null, null, null, null, null, null, false, false, true, true, true),
            new("NACC-MOUSE", "NexaPoint Precision Mouse", "Accessories", 6999m, null, null, null, null, null, null, null, null, null, false, false, true, true, true),
            new("NACC-CAM", "NexaCam 4K Creator", "Accessories", 14999m, null, null, null, null, null, null, null, null, null, false, false, true, true, false)
        ];

        return seeds.Select(seed => new SeededProduct(
            CreateProduct(seed, categories[seed.Category]),
            seed.StockQuantity)).ToList();
    }

    private static Product CreateProduct(ProductSeed seed, Category category)
    {
        var product = new Product(
            seed.Sku,
            seed.Name,
            Slugify(seed.Name),
            seed.Brand,
            category.Id,
            seed.ShortDescription ?? $"{seed.Name} for modern developer and creator workflows.",
            seed.Description ?? $"The {seed.Name} is part of the NexaStore computing range for developers, creators, and AI builders.",
            seed.Price,
            "INR",
            SeedDate);

        product.SetTechnicalSpecifications(new ProductTechnicalDetails(
            cpu: seed.Cpu,
            cpuCores: seed.CpuCores,
            ramGb: seed.RamGb,
            maxRamGb: seed.MaxRamGb,
            gpu: seed.Gpu,
            vramGb: seed.VramGb,
            npu: seed.Npu,
            npuTops: seed.NpuTops,
            storageGb: seed.StorageGb,
            storageType: seed.StorageGb is null ? null : "NVMe SSD",
            display: seed.Display,
            batteryWh: seed.BatteryWh,
            weightKg: seed.WeightKg), SeedDate);
        product.SetUsageClassification(new ProductUsageClassification(
            seed.IsAiReady,
            seed.IsLocalLlmReady,
            seed.IsDeveloperReady,
            seed.IsCreatorReady,
            seed.IsGamingReady,
            seed.IsDeveloperReady), SeedDate);
        product.SetFeatured(seed.IsFeatured, SeedDate);

        foreach (var tag in seed.Tags ?? [seed.Category])
        {
            product.AddTag(tag);
        }

        if (seed.Images is not null)
        {
            for (var index = 0; index < seed.Images.Length; index++)
            {
                var image = seed.Images[index];
                product.AddImage(image.Url, image.AltText, image.IsPrimary, index);
            }
        }

        return product;
    }

    private static string Slugify(string value) => value.ToLowerInvariant().Replace(' ', '-');

    private static Customer CreateCustomer()
    {
        var customer = new Customer("Aarav", "Sharma", "aarav.sharma@example.test", "+91-9000000000", SeedDate);
        var address = new Address(
            "Aarav Sharma",
            "101 Developer Avenue",
            "Tech Park",
            "Bengaluru",
            "Karnataka",
            "560001",
            "IN",
            "+91-9000000000");
        customer.SetShippingAddress(address);
        customer.SetBillingAddress(address with { });
        return customer;
    }

    private sealed record ProductSeed(
        string Sku,
        string Name,
        string Category,
        decimal Price,
        string? Cpu,
        int? RamGb,
        int? MaxRamGb,
        string? Gpu,
        int? VramGb,
        string? Npu,
        int? NpuTops,
        int? StorageGb,
        string? Display,
        bool IsAiReady,
        bool IsLocalLlmReady,
        bool IsDeveloperReady,
        bool IsCreatorReady,
        bool IsGamingReady,
        int StockQuantity = 12,
        string Brand = "Nexa",
        string? ShortDescription = null,
        string? Description = null,
        int? CpuCores = null,
        decimal? BatteryWh = null,
        decimal? WeightKg = null,
        bool IsFeatured = false,
        string[]? Tags = null,
        ProductImageSeed[]? Images = null);

    private sealed record ProductImageSeed(string Url, string AltText, bool IsPrimary);

    private sealed record SeededProduct(Product Product, int StockQuantity);
}
