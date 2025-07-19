using Luban.CodeTarget;
using Luban.Gdscript.CodeTarget;
using Luban.Gdscript.TemplateExtensions;
using Scriban;

namespace Luban.Extend;

[CodeTarget("gdscript-editor")]
public class GdscriptEditorCodeTarget:GdscriptCodeTargetBase
{
    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        base.OnCreateTemplateContext(ctx);
        ctx.PushGlobal(new GdscriptJsonTemplateExtension());
    }
}
