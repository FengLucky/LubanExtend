using Luban.CustomBehaviour;

namespace Luban.Extend;

[AttributeUsage(AttributeTargets.Class)]
public class ConstTargetAttribute:BehaviourBaseAttribute
{
    public ConstTargetAttribute(string name) : base(name)
    {
    }
}
