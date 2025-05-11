using System.Security.Cryptography;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace HydraCipher.Obfuscators;

public class RenameObfuscation(ModuleDefinition module,
                              ModuleDefinition runtimeModule) : Obfuscator(module, runtimeModule)
{
    public override void Obfuscate()
    {
        foreach (var type in Module.GetAllTypes().Where(t =>
            !IsGlobalModuleType(t) &&
            !t.IsRuntimeSpecialName &&
            !t.IsSpecialName &&
            !t.IsWindowsRuntime &&
            !t.IsInterface))
        {
            if (type.IsPublic)
            {
                RenameNonPublicMethods(type);
                RenameNonPublicFields(type);
                RenameNonPublicProperties(type);
                RenameNonPublicEvents(type);
            }
            else
            {
                RenameAllMethods(type);
                RenameAllFields(type);
                RenameAllProperties(type);
                RenameAllEvents(type);
            }
        }
    }

    private void RenameNonPublicMethods(TypeDefinition type)
    {
        foreach (var method in type.Methods.Where(m => !m.IsPublic))
        {
            if (!method.IsConstructor && !method.IsStatic && !method.IsRuntime && !method.IsRuntimeSpecialName)
                method.Name = GenerateUniqueName(method.Name);
        }
    }

    private void RenameAllMethods(TypeDefinition type)
    {
        foreach (var method in type.Methods)
        {
            if (!method.IsConstructor && !method.IsStatic && !method.IsRuntime && !method.IsRuntimeSpecialName)
                method.Name = GenerateUniqueName(method.Name);
        }
    }

    private void RenameNonPublicFields(TypeDefinition type)
    {
        foreach (var field in type.Fields.Where(f => !f.IsPublic))
        {
            field.Name = GenerateUniqueName(field.Name);
        }
    }

    private void RenameAllFields(TypeDefinition type)
    {
        foreach (var field in type.Fields)
        {
            field.Name = GenerateUniqueName(field.Name);
        }
    }

    private void RenameNonPublicProperties(TypeDefinition type)
    {
        foreach (var property in type.Properties.Where(p => !p.GetMethod.IsPublic || !p.SetMethod.IsPublic))
        {
            RenameProperty(property);
        }
    }

    private void RenameAllProperties(TypeDefinition type)
    {
        foreach (var property in type.Properties)
        {
            RenameProperty(property);
        }
    }

    private void RenameNonPublicEvents(TypeDefinition type)
    {
        foreach (var eventDef in type.Events.Where(e => !e.AddMethod.IsPublic || !e.RemoveMethod.IsPublic))
        {
            eventDef.Name = GenerateUniqueName(eventDef.Name);
        }
    }

    private void RenameAllEvents(TypeDefinition type)
    {
        foreach (var eventDef in type.Events)
        {
            eventDef.Name = GenerateUniqueName(eventDef.Name);
        }
    }

    private void RenameProperty(PropertyDefinition property)
    {
        if (!property.IsSpecialName)
        {
            string newName = GenerateUniqueName(property.Name);
            property.Name = newName;
            
            if (property.GetMethod != null)
                property.GetMethod.Name = "get_" + newName;
            
            if (property.SetMethod != null)
                property.SetMethod.Name = "set_" + newName;
        }
    }

    private string GenerateUniqueName(string originalName)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(originalName));
        return BitConverter.ToString(bytes).Replace("-", string.Empty);
    }
} 