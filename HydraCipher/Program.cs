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

rootCommand.AddOption(inputOption);
rootCommand.AddOption(outputOption);
rootCommand.AddOption(stringObfuscationOption);
rootCommand.AddOption(integerObfuscationOption);

rootCommand.SetHandler((input, output, stringObfuscation, integerObfuscation) =>
{
    if (string.IsNullOrEmpty(output))
    {
        output = input.Insert(input.Length - 4, "_patched");
    }

    using var runtimeAsm = ModuleDefinition.ReadModule(Runtime.RuntimeStream);
    using var mod = ModuleDefinition.ReadModule(input, new() { InMemory = true });

    var pipeline = new ObfuscationPipeline();

    if (stringObfuscation)
        pipeline.AddObfuscator(new StringObfuscator(mod, runtimeAsm));

    if (integerObfuscation)
        pipeline.AddObfuscator(new IntegerObfuscator(mod, runtimeAsm));

    pipeline.Run();

    mod.Write(output);
    Console.WriteLine($"Obfuscation completed. Output saved to: {output}");
}, inputOption, outputOption, stringObfuscationOption, integerObfuscationOption);

return await rootCommand.InvokeAsync(args);
