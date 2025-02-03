using System.Collections.Generic;
using Avalonia.Controls.Shapes;

namespace Client.Enums ;

    public class Storage
    {
        public static int[] AvailibleColors = {0, 1, 2, 3, 4}; // хехе костыль чтобы за индексами не следить
        public static string? Name { get; set; }
        public static int HandshakeMagic;
        public static bool CorrectInf = true;
        public static Dictionary<Path, int> TokenPositions = new(); // Словарь: фишка → её текущая позиция
        public static Dictionary<string, Path> PlayerTokens = new();  // Словарь: имя игрока → его фишка
        public static readonly Dictionary<int, int> SnakesAndLadders = new()
        {
            { 1, 38 }, { 4, 14 }, { 9, 31 }, { 21, 42 }, { 28, 84 }, // Лестницы
            { 36, 44 }, { 51, 67 }, { 71, 91 }, { 80, 100 },

            { 16, 6 }, { 47, 26 }, { 49, 11 }, { 56, 53 }, { 62, 19 }, // Змеи
            { 63, 60 }, { 87, 24 }, { 93, 73 }, { 95, 75 }, { 98, 78 }
        };
    }