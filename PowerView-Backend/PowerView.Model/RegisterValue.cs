
namespace PowerView.Model
{
    public struct RegisterValue
    {
        private readonly ObisCode obisCode;
        private readonly int value;
        private readonly short scale;
        private readonly Unit unit;

        public RegisterValue(ObisCode obisCode, int value, short scale, Unit unit, RegisterValueTag tag = RegisterValueTag.None)
        {
            this.obisCode = obisCode;
            this.value = value;
            this.scale = scale;
            this.unit = unit;
            Tag = tag;
        }

        public ObisCode ObisCode { get { return obisCode; } }
        public int Value { get { return value; } }
        public short Scale { get { return scale; } }
        public Unit Unit { get { return unit; } }
        public RegisterValueTag Tag { get; private set; }
    }
}
