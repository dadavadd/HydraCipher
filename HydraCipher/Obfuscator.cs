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

        public bool IsAsyncStateMachineMethod(MethodDefinition method)
        {
            if (method.CustomAttributes.Any(attr => 
                attr.AttributeType.Name == "AsyncStateMachineAttribute"))
            {
                return true;
            }

            return IsAsyncStateMachineType(method.DeclaringType);
        }

        public bool IsAsyncStateMachineType(TypeDefinition type)
        {
            if (type.CustomAttributes.Any(attr =>
                attr.AttributeType.Name == "AsyncStateMachineAttribute"))
            {
                return true;
            }

            if (type.Name.Contains("d__") || type.Name.Contains("<>"))
            {
                return true;
            }

            if (type.DeclaringType != null)
            {
                return IsAsyncStateMachineType(type.DeclaringType);
            }

            return false;
        }

        public bool IsGlobalModuleType(TypeDefinition type) => type.Name == "<Module>";

        public abstract void Obfuscate();
    }
}
