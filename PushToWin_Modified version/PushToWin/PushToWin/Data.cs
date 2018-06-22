using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushToWin
{
    public static class Data
    {
        public static string[] GameObjectIcon = { "■", "●", "♀", "★", "**", "∷", "☀", "♂" , "" };

        public static string[] GameObjectChar = { "墙", "球", "女", "星", "草", "水", "火", "男" ,"空"};

        public static ConsoleColor[] GameObjectColors = { ConsoleColor.Gray, ConsoleColor.DarkYellow, ConsoleColor.Magenta, ConsoleColor.Yellow, ConsoleColor.Green, ConsoleColor.Blue, ConsoleColor.DarkRed, ConsoleColor.DarkCyan,ConsoleColor.White };

        public static string[] behaviourNames = { "赢", "你", "停", "推", "死", "AI", "沉" ,"is"};

        public static ConsoleColor[] behaviourColors = { ConsoleColor.Yellow, ConsoleColor.White, ConsoleColor.Red, ConsoleColor.DarkYellow, ConsoleColor.DarkRed, ConsoleColor.Cyan, ConsoleColor.Blue,ConsoleColor.White};
    }
}
