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
            { 3, 22 }, // Лестница: с клетки 3 на 22
            { 5, 8 },  // Лестница: с 5 на 8
            { 11, 26 }, // Лестница: с 11 на 26
            { 20, 29 }, // Лестница: с 20 на 29
            { 17, 4 },  // Змея: с 17 на 4
            { 19, 7 },  // Змея: с 19 на 7
            { 27, 1 },  // Змея: с 27 на 1
            { 21, 9 }   // Змея: с 21 на 9
        };
    }