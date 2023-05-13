
namespace PowerView.Model;

[Flags]
public enum RegisterValueTag : byte
{
    None = 0x0,
    Manual = 0x1,
    Import = 0x2
}