namespace HydraCipher;

public class ObfuscationPipeline
{
    private readonly List<Obfuscator> _obfuscators = new();

    public void AddObfuscator(Obfuscator obfuscator) 
    => _obfuscators.Add(obfuscator);

    public void Run()
    {
        foreach (var obfuscator in _obfuscators)
            obfuscator.Obfuscate(); 
    }
}

