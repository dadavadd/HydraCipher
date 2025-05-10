using Mono.Cecil;
using Mono.Cecil.Cil;

namespace HydraCipher.Utils;

public static class ILCopier
{
    public static void CopyMethodToType(MethodDefinition src, TypeDefinition destType, ModuleDefinition destModule)
    {
        // create target method
        var dst = new MethodDefinition(src.Name,
                                       src.Attributes,
                                       destModule.ImportReference(src.ReturnType))
        {
            ImplAttributes = src.ImplAttributes,
            SemanticsAttributes = src.SemanticsAttributes
        };

        foreach (var gp in src.GenericParameters)
            dst.GenericParameters.Add(new GenericParameter(gp.Name, dst));

        foreach (var p in src.Parameters)
            dst.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, destModule.ImportReference(p.ParameterType)));

        destType.Methods.Add(dst);
        if (!src.HasBody) return;

        var sBody = src.Body;
        var dBody = dst.Body;
        dBody.InitLocals = sBody.InitLocals;
        dBody.MaxStackSize = sBody.MaxStackSize;

        foreach (var v in sBody.Variables)
            dBody.Variables.Add(new VariableDefinition(destModule.ImportReference(v.VariableType)));

        var map = new Dictionary<Instruction, Instruction>();

        // copy instructions with correct Create overloads
        foreach (var ins in sBody.Instructions)
        {
            Instruction ni;
            switch (ins.Operand)
            {
                case TypeReference t:
                    ni = Instruction.Create(ins.OpCode, destModule.ImportReference(t));
                    break;
                case MethodReference m:
                    ni = Instruction.Create(ins.OpCode, destModule.ImportReference(m));
                    break;
                case FieldReference f:
                    ni = Instruction.Create(ins.OpCode, destModule.ImportReference(f));
                    break;
                case ParameterDefinition pp:
                    ni = Instruction.Create(ins.OpCode, dst.Parameters[pp.Index]);
                    break;
                case VariableDefinition vv:
                    ni = Instruction.Create(ins.OpCode, dBody.Variables[vv.Index]);
                    break;
                case CallSite cs:
                    var nc = new CallSite(destModule.ImportReference(cs.ReturnType))
                    {
                        HasThis = cs.HasThis,
                        ExplicitThis = cs.ExplicitThis,
                        CallingConvention = cs.CallingConvention
                    };
                    foreach (var p in cs.Parameters)
                        nc.Parameters.Add(new ParameterDefinition(destModule.ImportReference(p.ParameterType)));
                    ni = Instruction.Create(ins.OpCode, nc);
                    break;
                case Instruction target:
                    ni = Instruction.Create(ins.OpCode, target);
                    break;
                case Instruction[] targets:
                    ni = Instruction.Create(ins.OpCode, targets);
                    break;
                case string s:
                    ni = Instruction.Create(ins.OpCode, s);
                    break;
                case sbyte sb:
                    ni = Instruction.Create(ins.OpCode, sb);
                    break;
                case byte b:
                    ni = Instruction.Create(ins.OpCode, b);
                    break;
                case int i:
                    ni = Instruction.Create(ins.OpCode, i);
                    break;
                case long l:
                    ni = Instruction.Create(ins.OpCode, l);
                    break;
                case float fl:
                    ni = Instruction.Create(ins.OpCode, fl);
                    break;
                case double db:
                    ni = Instruction.Create(ins.OpCode, db);
                    break;
                default:
                    ni = Instruction.Create(ins.OpCode);
                    break;
            }

            dBody.Instructions.Add(ni);
            map[ins] = ni;
        }

        // fix branch targets
        foreach (var ni in dBody.Instructions)
        {
            if (ni.Operand is Instruction oldT && map.TryGetValue(oldT, out var newT))
                ni.Operand = newT;
            else if (ni.Operand is Instruction[] arr)
            {
                for (int i = 0; i < arr.Length; i++)
                    if (map.TryGetValue(arr[i], out var r))
                        arr[i] = r;
            }
        }

        // exception handlers
        foreach (var eh in sBody.ExceptionHandlers)
        {
            var neh = new ExceptionHandler(eh.HandlerType)
            {
                TryStart = map[eh.TryStart],
                TryEnd = map[eh.TryEnd],
                HandlerStart = map[eh.HandlerStart],
                HandlerEnd = map[eh.HandlerEnd],
                CatchType = eh.CatchType != null ? destModule.ImportReference(eh.CatchType) : null,
                FilterStart = eh.FilterStart != null ? map[eh.FilterStart] : null
            };
            dBody.ExceptionHandlers.Add(neh);
        }
    }
}
