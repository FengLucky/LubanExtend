using Luban.Defs;
namespace Luban.Extend;

public interface IConstTarget
{
    void Handle(GenerationContext ctx,string targetName, OutputFileManifest manifest);
}
