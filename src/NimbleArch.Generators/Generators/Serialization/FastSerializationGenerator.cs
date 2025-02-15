using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NimbleArch.Generators.Generators.Serialization;

[Generator]
public class FastSerializationGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SerializationSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SerializationSyntaxReceiver receiver)
            return;

        foreach (var typeDeclaration in receiver.CandidateTypes)
        {
            var model = context.Compilation.GetSemanticModel(typeDeclaration.SyntaxTree);
            var typeSymbol = model.GetDeclaredSymbol(typeDeclaration) as ITypeSymbol;
            if (typeSymbol == null) continue;

            var attribute = typeSymbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "FastSerializableAttribute");
            if (attribute == null) continue;

            var includeNulls = attribute.NamedArguments
                .FirstOrDefault(a => a.Key == "IncludeNulls")
                .Value.Value as bool? ?? false;

            var bufferSizeHint = attribute.NamedArguments
                .FirstOrDefault(a => a.Key == "BufferSizeHint")
                .Value.Value as int? ?? 1024;

            var source = GenerateSerializationCode(typeSymbol, includeNulls, bufferSizeHint);
            context.AddSource($"{typeSymbol.Name}Serializer.g.cs", source);
        }
    }

    private string GenerateSerializationCode(
        ITypeSymbol typeSymbol,
        bool includeNulls,
        int bufferSizeHint)
    {
        var properties = typeSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && p.GetMethod != null);

        var builder = new StringBuilder();
        builder.AppendLine($@"
            using System;
            using System.Buffers.Binary;
            using System.Text;
            using {typeSymbol.ContainingNamespace};

            namespace NimbleArch.Generated.Serialization;

                public class {typeSymbol.Name}Serializer : IFastSerializable<{typeSymbol.Name}>
                {{
                    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false, true);

                    public int GetRequiredBufferSize({typeSymbol.Name} value)
                    {{
                        if (value == null) return 1;
                        var size = 1; // Null flag
        ");

        foreach (var property in properties)
        {
            builder.AppendLine(GenerateBufferSizeCalculation(property));
        }

        builder.AppendLine(@"
            return size;
        }

                public int SerializeToBytes({typeSymbol.Name} value, Span<byte> destination)
                {{
                    if (value == null)
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
            builder.AppendLine(GenerateSerializationCode(property, includeNulls));
        }

        builder.AppendLine(@"
            return position;
        }

        public {typeSymbol.Name} DeserializeFromBytes(ReadOnlySpan<byte> source)
        {{
            if (source.Length < 1) throw new ArgumentException(""Invalid data"");
            if (source[0] == 0) return null;

            var position = 1;
            var result = new {typeSymbol.Name}();
        ");

        foreach (var property in properties)
        {
            builder.AppendLine(GenerateDeserializationCode(property));
        }

        builder.AppendLine(@"
                    return result;
                }
            }
        }");

        return builder.ToString();
    }

    private string GenerateBufferSizeCalculation(IPropertySymbol property)
    {
        return property.Type.SpecialType switch
        {
            SpecialType.System_Int32 => "size += 4;",
            SpecialType.System_Int64 => "size += 8;",
            SpecialType.System_String => $@"
                size += 4; // Length prefix
                if (value.{property.Name} != null)
                {{
                    size += Utf8NoBom.GetByteCount(value.{property.Name})"+
                "}}",
            _ => throw new NotSupportedException($"Type {property.Type} not supported")
        };
    }

    private string GenerateSerializationCode(IPropertySymbol property, bool includeNulls)
    {
        return property.Type.SpecialType switch
        {
            SpecialType.System_Int32 => @"
                if (destination.Length < position + 4) throw new ArgumentException(""Buffer too small"");
                BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(position), value." + property.Name + @");
                position += 4;",
            
            SpecialType.System_Int64 => @"
                if (destination.Length < position + 8) throw new ArgumentException(""Buffer too small"");
                BinaryPrimitives.WriteInt64LittleEndian(destination.Slice(position), value." + property.Name + @");
                position += 8;",
            
            SpecialType.System_String => @"
                var str = value." + property.Name + @";
                if (str == null)
                {
                    if (destination.Length < position + 4) throw new ArgumentException(""Buffer too small"");
                    BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(position), -1);
                    position += 4;
                }
                else
                {
                    var byteCount = Utf8NoBom.GetByteCount(str);
                    if (destination.Length < position + 4 + byteCount) throw new ArgumentException(""Buffer too small"");
                    BinaryPrimitives.WriteInt32LittleEndian(destination.Slice(position), byteCount);
                    position += 4;
                    Utf8NoBom.GetBytes(str, destination.Slice(position));
                    position += byteCount;
                }",
            
            _ => throw new NotSupportedException($"Type {property.Type} not supported")
        };
    }

    string GenerateDeserializationCode(IPropertySymbol property)
    {
        return property.Type.SpecialType switch
        {
            SpecialType.System_Int32 => @"
                if (source.Length < position + 4) throw new ArgumentException(""Invalid data"");
                result." + property.Name + @" = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(position));
                position += 4;",
            
            SpecialType.System_Int64 => @"
                if (source.Length < position + 8) throw new ArgumentException(""Invalid data"");
                result." + property.Name + @" = BinaryPrimitives.ReadInt64LittleEndian(source.Slice(position));
                position += 8;",
            
            SpecialType.System_String => @"
                if (source.Length < position + 4) throw new ArgumentException(""Invalid data"");
                var length = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(position));
                position += 4;
                if (length == -1)
                {
                    result." + property.Name + @" = null;
                }
                else
                {
                    if (source.Length < position + length) throw new ArgumentException(""Invalid data"");
                    result." + property.Name + @" = Utf8NoBom.GetString(source.Slice(position, length));
                    position += length;
                }",
            
            _ => throw new NotSupportedException($"Type {property.Type} not supported")
        };
    }

    public class SerializationSyntaxReceiver : ISyntaxReceiver
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