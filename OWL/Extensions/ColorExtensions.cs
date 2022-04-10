namespace Microsoft.Xna.Framework
{
    public static class ColorExtensions
    {
        public static Color FromHex(this Color color, string hex)
        {
            if (hex[0] == '#')
                hex = hex.Substring(1);

            var r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            var g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            var b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
            return new Color(r, g, b);
        }
    }
}
