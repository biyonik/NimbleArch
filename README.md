# NimbleArch

[English](#english) | [Türkçe](#turkish)

---

## English

### High-Performance .NET Architecture Template

NimbleArch is a cutting-edge, high-performance .NET architecture template that challenges traditional patterns and focuses on maximum performance through innovative approaches.

### Key Features

- **Custom Validation Engine**: Built using Expression Trees and IL emission for compile-time validation, eliminating runtime reflection costs
- **Advanced Data Access**: Hybrid approach combining the best of CQRS and Event Sourcing with optimized query performance
- **Compile-time Dependency Resolution**: Using source generators to create a dependency graph at build time
- **Zero-Copy Operations**: Extensive use of modern .NET features like Span<T> and ArrayPool<T>
- **Modular Architecture**: Clean and modular design with high-performance considerations
- **Performance Metrics**: Built-in benchmarking and performance testing capabilities

### Project Structure

```
NimbleArch/
├── src/
│   ├── NimbleArch.Core/                  # Domain entities, interfaces
│   ├── NimbleArch.Infrastructure/        # Data access, external services
│   ├── NimbleArch.Application/           # Application logic
│   ├── NimbleArch.SharedKernel/          # Cross-cutting concerns
│   ├── NimbleArch.Generators/            # Custom source generators
│   └── NimbleArch.Api/                   # API endpoints
├── tests/
│   ├── NimbleArch.UnitTests/
│   ├── NimbleArch.IntegrationTests/
│   └── NimbleArch.PerformanceTests/      # Benchmark tests
└── tools/                                # Build scripts, tools
```

### Getting Started

1. Clone the repository
```bash
git clone https://github.com/yourusername/NimbleArch.git
```

2. Install dependencies
```bash
dotnet restore
```

3. Run the application
```bash
dotnet run --project src/NimbleArch.Api
```

### Performance Optimizations

- Struct-based value objects for reduced heap allocations
- Object pooling for frequently used objects
- Compile-time dependency resolution
- Zero-copy operations using Span<T>
- Custom validation engine without reflection

### Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

---

## Turkish

### Yüksek Performanslı .NET Mimari Şablonu

NimbleArch, geleneksel kalıpları sorgulayan ve yenilikçi yaklaşımlarla maksimum performansa odaklanan modern bir .NET mimari şablonudur.

### Temel Özellikler

- **Özel Doğrulama Motoru**: Expression Tree'ler ve IL emission kullanılarak derleme zamanında doğrulama, çalışma zamanı reflection maliyetlerini ortadan kaldırır
- **Gelişmiş Veri Erişimi**: CQRS ve Event Sourcing'in en iyi yanlarını optimize edilmiş sorgu performansıyla birleştiren hibrit yaklaşım
- **Derleme Zamanında Bağımlılık Çözümleme**: Build zamanında bağımlılık grafiği oluşturmak için source generator'lar
- **Sıfır Kopya Operasyonları**: Span<T> ve ArrayPool<T> gibi modern .NET özelliklerinin yaygın kullanımı
- **Modüler Mimari**: Yüksek performans odaklı temiz ve modüler tasarım
- **Performans Metrikleri**: Yerleşik benchmark ve performans test yetenekleri

### Proje Yapısı

```
NimbleArch/
├── src/
│   ├── NimbleArch.Core/                  # Domain varlıkları, arayüzler
│   ├── NimbleArch.Infrastructure/        # Veri erişimi, dış servisler
│   ├── NimbleArch.Application/           # Uygulama mantığı
│   ├── NimbleArch.SharedKernel/          # Çapraz kesişen konular
│   ├── NimbleArch.Generators/            # Özel source generator'lar
│   └── NimbleArch.Api/                   # API endpointleri
├── tests/
│   ├── NimbleArch.UnitTests/
│   ├── NimbleArch.IntegrationTests/
│   └── NimbleArch.PerformanceTests/      # Benchmark testleri
└── tools/                                # Build scriptleri, araçlar
```

### Başlangıç

1. Repository'yi klonlayın
```bash
git clone https://github.com/yourusername/NimbleArch.git
```

2. Bağımlılıkları yükleyin
```bash
dotnet restore
```

3. Uygulamayı çalıştırın
```bash
dotnet run --project src/NimbleArch.Api
```

### Performans Optimizasyonları

- Heap tahsislerini azaltmak için struct tabanlı value object'ler
- Sık kullanılan nesneler için object pooling
- Derleme zamanında bağımlılık çözümleme
- Span<T> kullanarak sıfır kopya operasyonları
- Reflection kullanmayan özel doğrulama motoru

### Katkıda Bulunma

Katkılarınızı bekliyoruz! Pull Request göndermekten çekinmeyin.