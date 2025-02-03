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
        
        public const int GridSize = 10; // Размер поля 10x10
        public const int СellSize = 45; // Размер клетки 
        
        /// <summary>
        /// Внимание координаты тут не совпадают с координатами на картинке, решение не очень, но зато быстро
        /// для тех кто будет что-то править потом
        /// на поле клеточки нумеруются с 0, поэтому надо менять корды змеек и лестниц
        /// </summary>
        public static readonly Dictionary<int, int> SnakesAndLadders = new()
        {
            { 0, 37 }, { 3, 13 }, { 8, 30 }, { 20, 41 }, { 27, 83 }, // Лестницы
            { 35, 43 }, { 50, 66 }, { 70, 90 }, { 79, 99 },

            { 15, 5 }, { 46, 25 }, { 48, 29 }, { 55, 52 }, { 61, 18 }, // Змеи
            { 62, 59 }, { 86, 23 }, { 92, 72 }, { 94, 74 }, { 97, 77 }
        };

    }