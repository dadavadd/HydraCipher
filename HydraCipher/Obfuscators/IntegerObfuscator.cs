using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace HydraCipher.Obfuscators;

public class IntegerObfuscator(ModuleDefinition module,
                               ModuleDefinition runtimeModule) : Obfuscator(module, runtimeModule)
{
    public override void Obfuscate()
    {
        foreach (var (instr, method) in GetAllInstructions(IsLdcI4))
        {
            SimplifyBranches(method.Body);

            var il = method.Body.GetILProcessor();
            var value = GetInt32Value(instr);
            
            var key = Random.Shared.Next();
            var obfuscated = value ^ key;

            il.InsertBefore(instr, Instruction.Create(OpCodes.Ldc_I4, obfuscated));
            il.InsertBefore(instr, Instruction.Create(OpCodes.Ldc_I4, key));
            il.InsertBefore(instr, Instruction.Create(OpCodes.Xor));

            il.Remove(instr);

            method.Body.OptimizeMacros();
        }
    }

    private static int GetInt32Value(Instruction instr) => instr.OpCode.Code switch
    {
        Code.Ldc_I4_M1 => -1,
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
        _ => instr.Operand is int i ? i : 0
    };
}
