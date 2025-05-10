using HydraCipher;
using HydraCipher.Obfuscators;
using Mono.Cecil;
using System.CommandLine;

var rootCommand = new RootCommand("HydraCipher - Assembly obfuscation tool");

var inputOption = new Option<string>(
    "--input",
    "The path to the input assembly")
{
    IsRequired = true
};

var outputOption = new Option<string>(
    "--output",
    "The path to the output assembly (optional, will append '_patched' to input if not specified)");

var stringObfuscationOption = new Option<bool>(
    "--strings",
    "Enable string obfuscation");

var integerObfuscationOption = new Option<bool>(
    "--integers",
    "Enable integer obfuscation");

var inMemoryOption = new Option<bool>(
    "--in-memory",
    "Enable in-memory obfuscation");

rootCommand.AddOption(inputOption);
rootCommand.AddOption(outputOption);
rootCommand.AddOption(stringObfuscationOption);
rootCommand.AddOption(integerObfuscationOption);
rootCommand.AddOption(inMemoryOption);

rootCommand.SetHandler((string input, string output, bool stringObfuscation, bool integerObfuscation, bool inMemory) =>
{
    if (string.IsNullOrEmpty(output))
    {
        output = input.Insert(input.Length - 4, "_patched");
    }

    var readParameters = new ReaderParameters 
    {
        InMemory = inMemory,
        ReadWrite = true
    };

    using var runtimeAsm = ModuleDefinition.ReadModule(Runtime.RuntimeStream, readParameters);
    using var mod = ModuleDefinition.ReadModule(input, readParameters);


    var pipeline = new ObfuscationPipeline();

    if (stringObfuscation)
        pipeline.AddObfuscator(new StringObfuscator(mod, runtimeAsm));

    if (integerObfuscation)
        pipeline.AddObfuscator(new IntegerObfuscator(mod, runtimeAsm));
        
    if (pipeline.HasObfuscators)
    {
        pipeline.Run();
        mod.Write(output);
        Console.WriteLine($"Obfuscation completed. Output saved to: {output}");
    }
    
}, inputOption, outputOption, stringObfuscationOption, integerObfuscationOption, inMemoryOption);

return await rootCommand.InvokeAsync(args);
