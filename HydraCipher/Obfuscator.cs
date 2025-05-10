using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace HydraCipher
{
    public abstract class Obfuscator(ModuleDefinition module, ModuleDefinition runtimeModule)
    {
        protected ModuleDefinition Module { get; } = module;
        protected ModuleDefinition RuntimeModule { get; } = runtimeModule;

        protected List<(Instruction Instruction, MethodDefinition Method)> GetAllInstructions(Func<OpCode, bool> instructionFilter)
        {
            return Module.GetAllTypes()
                .Where(t => !IsGlobalModuleType(t))
                .SelectMany(t => t.Methods
                    .Where(m => m.HasBody)
                    .SelectMany(m => m.Body.Instructions
                        .Where(i => instructionFilter(i.OpCode))
                        .Select(i => (i, m)))).ToList();
        }

        protected void SimplifyBranches(MethodBody body)
        {
            foreach (var instruction in body.Instructions)
            {
                instruction.OpCode = instruction.OpCode.Code switch
                {
                    Code.Beq_S => OpCodes.Beq,
                    Code.Bge_S => OpCodes.Bge,
                    Code.Bgt_S => OpCodes.Bgt,
                    Code.Ble_S => OpCodes.Ble,
                    Code.Blt_S => OpCodes.Blt,
                    Code.Bne_Un_S => OpCodes.Bne_Un,
                    Code.Bge_Un_S => OpCodes.Bge_Un,
                    Code.Bgt_Un_S => OpCodes.Bgt_Un,
                    Code.Ble_Un_S => OpCodes.Ble_Un,
                    Code.Blt_Un_S => OpCodes.Blt_Un,
                    Code.Br_S => OpCodes.Br,
                    Code.Brfalse_S => OpCodes.Brfalse,
                    Code.Brtrue_S => OpCodes.Brtrue,
                    Code.Leave_S => OpCodes.Leave,
                    _ => instruction.OpCode
                };
            }
        }

        public bool IsAsyncStateMachine(MethodDefinition method)
        {
            if (method.DeclaringType.Name.Contains("d__") || 
                method.DeclaringType.Name.Contains("<>") ||
                method.DeclaringType.Name.Contains("__"))
            {
                return true;
            }

            if (method.CustomAttributes.Any(attr => 
                attr.AttributeType.Name == "AsyncStateMachineAttribute" ||
                attr.AttributeType.Name == "CompilerGeneratedAttribute"))
            {
                return true;
            }

            return false;
        }

        public bool IsGlobalModuleType(TypeDefinition type) => type.Name == "<Module>";

        public bool IsLdcI4(OpCode opCode)
        {
            return opCode.Code switch
            {
                Code.Ldc_I4_M1 or
                Code.Ldc_I4_0 or
                Code.Ldc_I4_1 or
                Code.Ldc_I4_2 or
                Code.Ldc_I4_3 or
                Code.Ldc_I4_4 or
                Code.Ldc_I4_5 or
                Code.Ldc_I4_6 or
                Code.Ldc_I4_7 or
                Code.Ldc_I4_8 or
                Code.Ldc_I4_S or
                Code.Ldc_I4 => true,
                _ => false
            };
        }

        public abstract void Obfuscate();
    }
}
