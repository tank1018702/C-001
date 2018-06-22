using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushToWin
{
    public class GameObject : ICloneable
    {
        //在地图上的坐标
        public int x; 
        public int y;

        //逻辑与类型信息
        public LogicType logic_type;
        public ObjectType object_type;

        public ConsoleColor color;
        //显示默认两个空格,因为汉字占用两个字节。
        public string Icon = "  ";

        //指代的物体图标,仅名词
        public string contect_Icon;
        //附加的逻辑关系,仅动词
        public LogicType effect_logic;

        //除String外所有字段都是值类型,因此可以直接使用浅表拷贝
        public object Clone()
        {
            return MemberwiseClone() as GameObject;
        }

        public GameObject(int x, int y)
        {
            this.x = x;
            this.y = y;
            logic_type = LogicType.Null;
        }

        public bool HasLogic(LogicType logic)
        {
            return (logic_type & logic) != 0;
        }

        public void RemoveLogic(LogicType type)
        {
            logic_type = logic_type & (~type);
        }
        

       
    }


    [Flags]
    public enum LogicType
    {
        //没有附加任何逻辑,只是显示在游戏界面里
        Null = 1,
        //其他逻辑
        Win = 1 << 1,
        You = 1 << 2,
        Stop = 1 << 3,
        Push = 1 << 4,
        Kill = 1 << 5,
        Sink = 1 << 6,
        AI = 1 << 7,
        Subject = 1 << 8,
        Object = 1 << 9

    }


    //类型枚举,因为类型唯一故不需要使用位域特性
    public enum ObjectType
    {
        eneity,
        verb,
        noun
    }


    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public struct Pos
    {
        int x;
        int y;
        public Pos(int _x,int _y)
        {
            x = _x;
            y = _y;
        }
    }

}
