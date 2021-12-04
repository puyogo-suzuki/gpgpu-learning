struct RGB{
    public byte R;
    public byte G;
    public byte B;

    public RGB(byte r, byte g, byte b) {
        R = r; G = g; B = b;
    }

    public static implicit operator RGB(ValueTuple<byte, byte, byte> t) => new RGB(t.Item1, t.Item2, t.Item3);
}