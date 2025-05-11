
using Mono.Cecil;

namespace HydraCipher;

public record AssemblyMetadata(List<TypeDefinition> Types,
                               List<MethodDefinition> Methods,
                               List<FieldDefinition> Fields,
                               List<EventDefinition> Events,
                               List<PropertyDefinition> Properties);
