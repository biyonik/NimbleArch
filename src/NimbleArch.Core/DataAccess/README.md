# NimbleArch Data Access Layer

[English](#english) | [Türkçe](#turkish)

---

## English

### Overview

NimbleArch's Data Access Layer provides a high-performance, unconventional approach to data access in .NET applications. Instead of traditional repository patterns, it leverages Expression Trees, Source Generators, and modern .NET features to achieve optimal performance while maintaining code clarity.

### Key Features

#### 1. Query Specification Pattern
- Expression Tree based query building
- Compiled query caching
- Optimized include handling
- Advanced filtering capabilities
- Dynamic ordering support
- Efficient pagination

#### 2. Command Pattern
- High-performance command handling
- Bulk operation support
- Memory-pooled operations
- Compiled command execution
- Structured error handling

#### 3. Performance Optimizations
- Zero-allocation result handling
- Memory pooling for large operations
- Compiled expressions caching
- Minimal query generation overhead
- Optimized change tracking

### Usage Examples

#### Query Specification
```csharp
public class ActiveUsersSpecification : QuerySpecification<User>
{
    public ActiveUsersSpecification(int minAge, int maxAge)
    {
        AddCriteria(u => u.IsActive);
        AddCriteria(u => u.Age >= minAge && u.Age <= maxAge);
        AddInclude(u => u.Orders);
        AddOrderBy(u => u.LastLoginDate, ascending: false);
        SetPagination(0, 10);
    }
}

// Usage
var spec = new ActiveUsersSpecification(18, 65);
var users = await _queryExecutor.ToListAsync(spec);
```

#### Command Handling
```csharp
public class CreateUserCommand : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public string Username { get; set; }
    public string Email { get; set; }
}

// Handler
public class CreateUserCommandHandler : EFCommandHandler<CreateUserCommand>
{
    protected override async Task<CommandResult> ExecuteCommandAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = new User { /* ... */ };
        await Context.Set<User>().AddAsync(user, cancellationToken);
        return CommandResult.Success(user.Id);
    }
}
```

### Advanced Features

#### 1. Bulk Operations
```csharp
public class BulkCreateUserCommandHandler : EFBulkCommandHandler<CreateUserCommand>
{
    protected override async Task<IEnumerable<(Guid CommandId, CommandResult Result)>> 
        ExecuteBatchAsync(CreateUserCommand[] batch, CancellationToken cancellationToken)
    {
        // Efficient bulk insert implementation
    }
}
```

#### 2. Custom Query Handling
```csharp
public class ComplexReportSpecification : QuerySpecification<Order>
{
    public ComplexReportSpecification()
    {
        AddInclude(o => o.Items);
        AddInclude(o => o.Customer);
        SetGroupBy(o => o.Customer.Region);
        // Advanced query configuration
    }
}
```

### Performance Best Practices

1. Query Optimization
    - Use compiled queries for frequent operations
    - Implement proper indexing strategy
    - Utilize selective loading with includes

2. Command Handling
    - Use bulk operations for multiple items
    - Implement proper batching strategies
    - Utilize change tracking optimization

3. Memory Management
    - Use struct-based results where possible
    - Implement proper pooling strategies
    - Minimize object allocation

### Implementation Guidelines

1. Query Specifications
    - Keep specifications focused and reusable
    - Use meaningful names for specifications
    - Document complex query logic
    - Implement proper validation

2. Command Handlers
    - Implement proper error handling
    - Use structured results
    - Maintain handler independence
    - Document side effects

3. Performance Monitoring
    - Implement proper logging
    - Monitor query performance
    - Track memory usage
    - Set up alerts for bottlenecks

---

## Turkish

### Genel Bakış

NimbleArch'ın Veri Erişim Katmanı, .NET uygulamalarında veri erişimi için yüksek performanslı, alışılmadık bir yaklaşım sunar. Geleneksel repository pattern'ler yerine, Expression Tree'ler, Source Generator'lar ve modern .NET özelliklerini kullanarak kod netliğini korurken optimal performans sağlar.

### Temel Özellikler

#### 1. Sorgu Spesifikasyon Pattern'i
- Expression Tree tabanlı sorgu oluşturma
- Derlenmiş sorgu önbellekleme
- Optimize edilmiş include yönetimi
- Gelişmiş filtreleme yetenekleri
- Dinamik sıralama desteği
- Verimli sayfalama

#### 2. Command Pattern
- Yüksek performanslı komut işleme
- Toplu işlem desteği
- Bellek havuzlu operasyonlar
- Derlenmiş komut yürütme
- Yapılandırılmış hata yönetimi

#### 3. Performans Optimizasyonları
- Sıfır bellek tahsisli sonuç işleme
- Büyük işlemler için bellek havuzlama
- Derlenmiş ifadelerin önbelleklenmesi
- Minimum sorgu oluşturma yükü
- Optimize edilmiş değişiklik takibi

### Kullanım Örnekleri

#### Sorgu Spesifikasyonu
```csharp
public class AktifKullanicilarSpecification : QuerySpecification<User>
{
    public AktifKullanicilarSpecification(int minYas, int maxYas)
    {
        AddCriteria(u => u.IsActive);
        AddCriteria(u => u.Age >= minYas && u.Age <= maxYas);
        AddInclude(u => u.Orders);
        AddOrderBy(u => u.LastLoginDate, ascending: false);
        SetPagination(0, 10);
    }
}

// Kullanım
var spec = new AktifKullanicilarSpecification(18, 65);
var kullanicilar = await _queryExecutor.ToListAsync(spec);
```

#### Komut İşleme
```csharp
public class KullaniciOlusturmaCommand : ICommand
{
    public Guid CommandId { get; } = Guid.NewGuid();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public string KullaniciAdi { get; set; }
    public string Email { get; set; }
}

// İşleyici
public class KullaniciOlusturmaCommandHandler : EFCommandHandler<KullaniciOlusturmaCommand>
{
    protected override async Task<CommandResult> ExecuteCommandAsync(
        KullaniciOlusturmaCommand command,
        CancellationToken cancellationToken)
    {
        var kullanici = new User { /* ... */ };
        await Context.Set<User>().AddAsync(kullanici, cancellationToken);
        return CommandResult.Success(kullanici.Id);
    }
}
```

### Gelişmiş Özellikler

#### 1. Toplu İşlemler
```csharp
public class TopluKullaniciOlusturmaHandler : EFBulkCommandHandler<KullaniciOlusturmaCommand>
{
    protected override async Task<IEnumerable<(Guid CommandId, CommandResult Result)>> 
        ExecuteBatchAsync(KullaniciOlusturmaCommand[] batch, CancellationToken cancellationToken)
    {
        // Verimli toplu ekleme implementasyonu
    }
}
```

#### 2. Özel Sorgu İşleme
```csharp
public class KarmasikRaporSpecification : QuerySpecification<Order>
{
    public KarmasikRaporSpecification()
    {
        AddInclude(o => o.Items);
        AddInclude(o => o.Customer);
        SetGroupBy(o => o.Customer.Region);
        // Gelişmiş sorgu yapılandırması
    }
}
```

### Performans İyi Uygulama Örnekleri

1. Sorgu Optimizasyonu
    - Sık kullanılan operasyonlar için derlenmiş sorgular kullanın
    - Uygun indeksleme stratejisi uygulayın
    - Include'larla seçici yükleme kullanın

2. Komut İşleme
    - Çoklu öğeler için toplu işlemler kullanın
    - Uygun batch stratejileri uygulayın
    - Değişiklik takibi optimizasyonundan yararlanın

3. Bellek Yönetimi
    - Mümkün olduğunda struct tabanlı sonuçlar kullanın
    - Uygun havuzlama stratejileri uygulayın
    - Nesne tahsisini minimize edin

### Uygulama Yönergeleri

1. Sorgu Spesifikasyonları
    - Spesifikasyonları odaklı ve yeniden kullanılabilir tutun
    - Spesifikasyonlar için anlamlı isimler kullanın
    - Karmaşık sorgu mantığını dokümante edin
    - Uygun doğrulama uygulayın

2. Komut İşleyicileri
    - Uygun hata yönetimi uygulayın
    - Yapılandırılmış sonuçlar kullanın
    - İşleyici bağımsızlığını koruyun
    - Yan etkileri dokümante edin

3. Performans İzleme
    - Uygun loglama uygulayın
    - Sorgu performansını izleyin
    - Bellek kullanımını takip edin
    - Darboğazlar için uyarılar oluşturun