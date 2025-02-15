namespace NimbleArch.Generators.Generators.CacheKey;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

[Generator]
public class CacheKeySerializationGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new KeySerializationSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not KeySerializationSyntaxReceiver receiver)
            return;

        foreach (var typeDeclaration in receiver.CandidateTypes)
        {
            var model = context.Compilation.GetSemanticModel(typeDeclaration.SyntaxTree);
            var typeSymbol = model.GetDeclaredSymbol(typeDeclaration) as ITypeSymbol;
            if (typeSymbol == null) continue;

            var attribute = typeSymbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "CacheKeySerializableAttribute");
            if (attribute == null) continue;

            var bufferSizeHint = attribute.NamedArguments
                .FirstOrDefault(a => a.Key == "BufferSizeHint")
                .Value.Value as int? ?? 256;

            var source = GenerateSerializerCode(typeSymbol, bufferSizeHint);
            context.AddSource($"{typeSymbol.Name}CacheKeySerializer.g.cs", source);
        }
    }

    private string GenerateSerializerCode(ITypeSymbol typeSymbol, int bufferSizeHint)
    {
        var properties = typeSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && p.GetMethod != null);

        var builder = new StringBuilder();
        builder.AppendLine($@"
using System;
using System.Buffers.Binary;
using {typeSymbol.ContainingNamespace};

namespace NimbleArch.Generated.Caching
{{
    public class {typeSymbol.Name}CacheKeySerializer : ICacheKeySerializer<{typeSymbol.Name}>
    {{
        public int GetRequiredBufferSize({typeSymbol.Name} key)
        {{
            if (key == null) return 1;
            var size = 1; // Null flag
");

        foreach (var property in properties)
        {
            builder.AppendLine(GenerateBufferSizeCalculation(property));
        }

        builder.AppendLine(@"
            return size;
        }

        public int SerializeToBytes({typeSymbol.Name} key, Span<byte> destination)
        {
            if (key == null)
            {
                if (destination.Length < 1) throw new ArgumentException(""Buffer too small"");
                destination[0] = 0;
                return 1;
            }

            var position = 0;
            destination[position++] = 1; // Not null
");

        foreach (var property in properties)
        {
            builder.AppendLine(GenerateSerializationCode(property));
        }

        builder.AppendLine(@"
            return position;
        }
    }
}");

        return builder.ToString();
    }

    private string GenerateBufferSizeCalculation(IPropertySymbol property)
    {
        return property.Type.SpecialType switch
        {
            SpecialType.System_Boolean => "size += 1;",
            SpecialType.System_Int32 => "size += 4;",
            SpecialType.System_Int64 => "size += 8;",
            SpecialType.System_String => @"
                size += 4; // Length prefix
                if (key." + property.Name + @" != null)
                {{
                    size += System.Text.Encoding.UTF8.GetByteCount(key.{property.Name})"+
                "}}",
            _ => throw new NotSupportedException($"Type {property.Type} not supported for cache key")
        };
    }

    private string GenerateSerializationCode(IPropertySymbol property)
    {
        return property.Type.SpecialType switch
        {
            SpecialType.System_Boolean => @"
                if (destination.Length < position + 1) throw new ArgumentException(""Buffer too small"");
                destination[position++] = (byte)(key." + property.Name + @" ? 1 : 0);",
            
            SpecialType.System_Int32 => @"
                if (destination.Length < position + 4) throw new ArgumentException(""Buffer too small"");
                BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(position), key." + property.Name + @");
                position += 4;",
            
            SpecialType.System_Int64 => @"
                if (destination.Length < position + 8) throw new ArgumentException(""Buffer too small"");
                BinaryPrimitives.WriteInt64LittleEndian(destination.Slice(position), key." + property.Name + @");
                position += 8;",
            
            SpecialType.System_String => @"
                var str = key." + property.Name + @";
                if (str == null)
                {
                    if (destination.Length < position + 4) throw new ArgumentException(""Buffer too small"");
                    BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(position), -1);
                    position += 4;
                }
                else
                {
                    var byteCount = System.Text.Encoding.UTF8.GetByteCount(str);
                    if (destination.Length < position + 4 + byteCount) throw new ArgumentException(""Buffer too small"");
                    BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(position), byteCount);
                    position += 4;
                    System.Text.Encoding.UTF8.GetBytes(str, destination.Slice(position));
                    position += byteCount;
                }",
            
            _ => throw new NotSupportedException($"Type {property.Type} not supported for cache key")
        };
    }

    public class KeySerializationSyntaxReceiver : ISyntaxReceiver
    {
        public List<TypeDeclarationSyntax> CandidateTypes { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is TypeDeclarationSyntax typeDeclaration &&
                typeDeclaration.AttributeLists.Count > 0)
            {
                CandidateTypes.Add(typeDeclaration);
            }
        }
    }
}