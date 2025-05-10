using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace HydraCipher.Obfuscators;

public class IntegerObfuscator : Obfuscator
{
    public IntegerObfuscator(ModuleDefinition module, ModuleDefinition runtimeModule) : base(module, runtimeModule)
    {
        
    }

    public override void Obfuscate()
    {
        foreach (var (instr, method) in Module.GetAllTypes()
                .SelectMany(t => t.Methods
                    .Where(m => m.HasBody)
                    .SelectMany(m => m.Body.Instructions
                        .Where(i => i.OpCode == OpCodes.Ldc_I4 || 
                                i.OpCode == OpCodes.Ldc_I4_S || 
                                i.OpCode == OpCodes.Ldc_I4_0 || 
                                i.OpCode == OpCodes.Ldc_I4_1 || 
                                i.OpCode == OpCodes.Ldc_I4_2 || 
                                i.OpCode == OpCodes.Ldc_I4_3 || 
                                i.OpCode == OpCodes.Ldc_I4_4 || 
                                i.OpCode == OpCodes.Ldc_I4_5 || 
                                i.OpCode == OpCodes.Ldc_I4_6 || 
                                i.OpCode == OpCodes.Ldc_I4_7 || 
                                i.OpCode == OpCodes.Ldc_I4_8)
                        .Select(i => (i, m)))))
        {
            var il = method.Body.GetILProcessor();
            var value = GetInt32Value(instr);
            
            var key = Random.Shared.Next(1, 1000);
            var obfuscated = value ^ key;
            
            il.InsertBefore(instr, Instruction.Create(OpCodes.Ldc_I4, obfuscated));
            il.InsertBefore(instr, Instruction.Create(OpCodes.Ldc_I4, key));
            il.InsertBefore(instr, Instruction.Create(OpCodes.Xor));
            
            il.Remove(instr);
            method.Body.SimplifyMacros();
            method.Body.OptimizeMacros();
        }
    }

    private static int GetInt32Value(Instruction instr)
    {
        return instr.OpCode.Code switch
        {
            Code.Ldc_I4_0 => 0,
            Code.Ldc_I4_1 => 1,
            Code.Ldc_I4_2 => 2,
            Code.Ldc_I4_3 => 3,
            Code.Ldc_I4_4 => 4,
            Code.Ldc_I4_5 => 5,
            Code.Ldc_I4_6 => 6,
            Code.Ldc_I4_7 => 7,
            Code.Ldc_I4_8 => 8,
            Code.Ldc_I4_S => (sbyte)instr.Operand,
            _ => (int)instr.Operand
        };
    }
}
