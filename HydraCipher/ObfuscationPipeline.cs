namespace HydraCipher;

public class ObfuscationPipeline
{
    private readonly List<Obfuscator> _obfuscators = new();

    public void AddObfuscator(Obfuscator obfuscator) 
        => _obfuscators.Add(obfuscator);

    public bool HasObfuscators => _obfuscators.Count > 0;

    public void Run()
    {
        foreach (var obfuscator in _obfuscators)
            obfuscator.Obfuscate(); 
    }
}

