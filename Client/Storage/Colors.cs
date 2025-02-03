using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace Client.Enums;

    public class Colors
    {
        private static Dictionary<int, string> _colors = new Dictionary<int, string>
        {
            { 4, "Blue" },
            { 3, "Yellow" },
            { 2, "Green" },
            { 1, "Red" },
        };

        public static string GetColor(int color)
        {
            return _colors[color];
        }

        public static int GetColorId(string color)
        {
            return _colors.FirstOrDefault(x => x.Value == color).Key;
        }
    }