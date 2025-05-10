using System.Numerics;
using System.Text;
using HydraCipher.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace HydraCipher.Obfuscators;

public class StringObfuscator : Obfuscator
{
    private readonly MethodReference _decryptRef;

    public StringObfuscator(ModuleDefinition module,
                          ModuleDefinition runtimeModule) : base(module, runtimeModule)
    {
        var decryptSrc = RuntimeModule.Types.SelectMany(t => t.Methods).First(m => m.Name == "Deobfuscate");
        var hostType = Module.GetType("<Module>");
        ILCopier.CopyMethodToType(decryptSrc, hostType, Module);
        _decryptRef = Module.ImportReference(hostType.Methods.First(m => m.Name == decryptSrc.Name));
    }

    public override void Obfuscate()
    {
        foreach (var (instruction, method) in GetAllInstructions(op => op == OpCodes.Ldstr))
        {
            var ilProcessor = method.Body.GetILProcessor();
            var stringValue = (string)instruction.Operand;
            var obfuscatedString = ObfuscateString(stringValue).ToString();

            var bigIntegerParseMethod = new MethodReference("Parse", 
                Module.ImportReference(typeof(BigInteger)),
                Module.ImportReference(typeof(BigInteger)))
            {
                HasThis = false,
                Parameters = { new(Module.TypeSystem.String) }
            };

            ilProcessor.InsertBefore(instruction, Instruction.Create(OpCodes.Ldstr, obfuscatedString));
            ilProcessor.InsertBefore(instruction, Instruction.Create(OpCodes.Call, Module.ImportReference(bigIntegerParseMethod)));
            ilProcessor.InsertBefore(instruction, Instruction.Create(OpCodes.Call, _decryptRef));
            
            ilProcessor.Remove(instruction);
            method.Body.SimplifyMacros();
            method.Body.OptimizeMacros();
        }
    }

    private static BigInteger ObfuscateString(string s)
    {
        var data = Encoding.UTF8.GetBytes(s);
        var result = new byte[data.Length];
        for (var i = 0; i < data.Length; i++)
        {
            var b = data[i];
            b = (byte)(b & ~(byte)((Enumerable.Range(0, 8).Sum(j => (b >> j) & 1) & 1) == 0 ? 0xAA : 0x55) | (~b & (byte)((Enumerable.Range(0, 8).Sum(j => (b >> j) & 1) & 1) == 0 ? 0xAA : 0x55)));
            b = (byte)(((b * 0x0202020202UL) & 0x010884422010UL) % 1023);
            result[i] = (byte)(((b << (i & 7)) | (b >> (8 - (i & 7)))) & 0xFF);
        }
        return new BigInteger(result, isUnsigned: true, isBigEndian: false);
    }
}