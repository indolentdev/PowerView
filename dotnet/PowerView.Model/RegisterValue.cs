
namespace PowerView.Model
{
  public class RegisterValue
  {
    private readonly ObisCode obisCode;
    private readonly int value;
    private readonly short scale;
    private readonly Unit unit;

    public RegisterValue(ObisCode obisCode, int value, short scale, Unit unit)
    {
      this.obisCode = obisCode;
      this.value = value;
      this.scale = scale;
      this.unit = unit;
    }

    public ObisCode ObisCode { get { return obisCode; } }
    public int Value { get { return value; } }
    public short Scale { get { return scale; } }
    public Unit Unit { get { return unit; } }
  }
}
