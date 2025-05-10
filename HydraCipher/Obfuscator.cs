using Mono.Cecil;

namespace HydraCipher
{
    public abstract class Obfuscator
    {
        protected ModuleDefinition Module { get; }
        protected ModuleDefinition RuntimeModule { get; }

        protected Obfuscator(ModuleDefinition module, ModuleDefinition runtimeModule)
        {
            Module = module;
            RuntimeModule = runtimeModule;
        }

        public abstract void Obfuscate();
    }
}
