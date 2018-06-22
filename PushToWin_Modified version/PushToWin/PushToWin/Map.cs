using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushToWin
{
    public class Map
    {
        public int _height;
        public int _width;
        string empty = "  ";

        

        string[,] Buffer;

        string[,] BackGroundBuffer;

        ConsoleColor[,] ColorBuffer;

        public static List<GameObject> all_object = new List<GameObject>();




        public Map(int h, int w, string _empty="  ")
        {
            _height = h;
            _width = w;
            empty = _empty;
            Buffer = new string[_height, _width];
            BackGroundBuffer = new string[_height, _width];
            ColorBuffer = new ConsoleColor[_height, _width];
            Console.CursorVisible = false;
            ClearBuffer();
        }

        public void ClearBuffer()
        {
            for (int i = 0; i < _height; ++i)
            {
                for (int j = 0; j < _width; ++j)
                {
                    Buffer[i, j] = empty;
                    ColorBuffer[i, j] = ConsoleColor.Gray;
                }
            }
        }


        void CopyBuffer(string[,] source, string[,] replica)
        {
            for (int i = 0; i < source.GetLength(0); ++i)
            {
                for (int j = 0; j < source.GetLength(1); ++j)
                {
                    replica[i, j] = source[i, j];
                }
            }

        }
        public void ClearBuffer_DoubleBuffer()
        {
            CopyBuffer(Buffer, BackGroundBuffer);
            for (int i = 0; i < _height; ++i)
            {
                for (int j = 0; j < _width; ++j)
                {
                    Buffer[i, j] = empty;
                    ColorBuffer[i, j] = ConsoleColor.Gray;
                }
            }
        }
        public string[,] GetBuffer()
        {
            return Buffer;
        }
        public ConsoleColor[,] GetColorBuffer()
        {
            return ColorBuffer;
        }

        public  void RefreshDoubleBuffer()
        {
            for(int i=0;i<_height;i++)
            {
                for(int j=0;j<_width;j++)
                {
                    if(Buffer[i,j]!=BackGroundBuffer[i,j])
                    {
                        Console.SetCursorPosition((i + 1) * 2, j + 1);
                        Console.ForegroundColor = ColorBuffer[i, j];
                        Console.Write(Buffer[i, j]);
                    }
                }
            }
        }

        

        public static void TestDraw(List<GameObject> list)
        {
            foreach (var i in list)
            {

                Console.SetCursorPosition((i.x+1) * 2, i.y+1);
                Console.Write(i.Icon);
            }
        }
    }
}
