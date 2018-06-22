using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushToWin
{
  public  class Program
    {
        static int width = 18;
        static int height = 18;

        static string[,] buffer;
        static ConsoleColor[,] color_buffer;

        static List<GameObject> all_objs = new List<GameObject>();



        static void Main(string[] args)
        {




            //读取文件
            //生成示例化物体
            //生成逻辑
            //导入地图
            StageLogic();


            
            Console.ReadKey();







        }

        static void StageLogic()
        {
            List<string> file = FileManager.ReadMapFile("../../TEST.txt");
            all_objs.AddRange(GameLogic.GameObjectInit(file));
            Map map = new Map(width, height);
            buffer = map.GetBuffer();
            color_buffer = map.GetColorBuffer();
            GameLogic.SetLogic();
            map.ClearBuffer_DoubleBuffer();
            DrawALL();
            map.RefreshDoubleBuffer();

            int step = 0;
            Dictionary<int, List<GameObject>> history = new Dictionary<int, List<GameObject>>();

            while (true)
            {
                int run = GameLogic.Input();
                if (run == 1)
                {
                    history[step] = new List<GameObject>();
                    foreach (var obj in all_objs)
                    {
                        history[step].Add(obj.Clone() as GameObject);
                    }
                    step++;
                }
                else if (run == -1)
                {
                    if (step >= 1)
                    {
                        step--;
                        all_objs.Clear();
                        all_objs.AddRange(history[step]);
                        
                    }
                }

                if (run != 0)
                {
                    GameLogic.SetLogic();
                    map.ClearBuffer_DoubleBuffer();
                    DrawALL();
                    map.RefreshDoubleBuffer();

                }
            }
        }

       public static void DrawALL()
        {
          
            foreach (var obj in all_objs)
            {
                if (obj.Icon != "")
                {
                    buffer[obj.x, obj.y] = obj.Icon;
                }
                
                color_buffer[obj.x, obj.y] = obj.color;
            }
        }    
    }

    public class GameLogic
    {
        public const int map_width = 18;
        public const int map_height = 18;
        public static List<GameObject> AllGameObjects = new List<GameObject>();

        public static List<GameObject> GameObjectInit(List<string> file)
        {

            for (int i = 0; i < file.Count; i++)
            {
                char[] line = file[i].ToCharArray();
                for (int j = 0; j < line.Length; j++)
                {
                    if(line[j]==' ')
                    {
                        continue;
                    }
                    GameObject obj = new GameObject(j, i);
                    GameObjectAssign(obj, line[j]);
                    AllGameObjects.Add(obj);
                }

            }
            return AllGameObjects;
        }


        static void GameObjectAssign(GameObject obj, char tag)
        {
            // "■", "●", "♀", "★", "**", "∷", "☀", "♂" ,"  "
            // "墙", "球", "女", "星", "草", "水", "火", "男" ,"空"
            //"赢", "你", "停", "推", "死", "AI", "沉" ,"is"
            int n;
            //实体
            if (tag == '■')
            {
                n = 0;
            }
            else if (tag == '●')
            {
                n = 1;
            }
            else if (tag == '♀')
            {
                n = 2;
            }
            else if (tag == '★')
            {
                n = 3;
            }
            else if (tag == '*')
            {
                n = 4;
            }
            else if (tag == '∷')
            {
                n = 5;
            }
            else if (tag == '☀')
            {
                n = 6;
            }
            else if (tag == '♂')
            {
                n = 7;
            }
            else if (tag == ' ')
            {
                n = 8;
            }
            //名词
            else if (tag == '墙')
            {
                n = 10;
            }
            else if (tag == '球')
            {
                n = 11;
            }
            else if (tag == '女')
            {
                n = 12;
            }
            else if (tag == '星')
            {
                n = 13;
            }
            else if (tag == '草')
            {
                n = 14;
            }
            else if (tag == '水')
            {
                n = 15;
            }
            else if (tag == '火')
            {
                n = 16;
            }
            else if (tag == '男')
            {
                n = 17;
            }
            else if (tag == '空')
            {
                n = 18;
            }
            //动词
            else if (tag == '赢')
            {
                n = 20;
                obj.effect_logic = LogicType.Win;
            }
            else if (tag == '你')
            {
                n = 21;
                obj.effect_logic = LogicType.You;
            }
            else if (tag == '停')
            {
                n = 22;
                obj.effect_logic = LogicType.Stop;
            }
            else if (tag == '推')
            {
                n = 23;
                obj.effect_logic = LogicType.Push;
            }
            else if (tag == '死')
            {
                n = 24;
                obj.effect_logic = LogicType.Kill;
            }
            else if (tag == '智')
            {
                n = 25;
                obj.effect_logic = LogicType.AI;
            }
            else if (tag == '沉')
            {
                n = 26;
                obj.effect_logic = LogicType.Sink;
            }
            else if (tag == '是')
            {
                n = 27;
            }
            else
            {
                n = 30;
            }

            if (n < 10)
            {
                obj.Icon = Data.GameObjectIcon[n];
                obj.color = Data.GameObjectColors[n];
                obj.object_type = ObjectType.eneity;
                //if(n!=8)
                //{
                //    obj.
                //}
                
            }
            else if (n < 20)
            {
                obj.Icon = Data.GameObjectChar[n - 10];
                obj.color = Data.GameObjectColors[n - 10];
                obj.contect_Icon = Data.GameObjectIcon[n - 10];
                obj.logic_type = LogicType.Null | LogicType.Push|LogicType.Subject|LogicType.Object;
                obj.object_type = ObjectType.noun;
            }
            else if (n < 30)
            {
                obj.Icon = Data.behaviourNames[n - 20];
                obj.color = Data.behaviourColors[n - 20];
                obj.logic_type = LogicType.Null | LogicType.Push|LogicType.Object;
                obj.object_type = ObjectType.verb;

            }
            else
            {
                obj.Icon = "错";
                obj.color = ConsoleColor.Red;
            }
        }

        public static void LogicInit()
        {
            foreach (var obj in AllGameObjects)
            {
                if (obj.object_type == ObjectType.eneity)
                {
                    obj.logic_type = LogicType.Null;
                }
            }
        }

        static bool CheckByIcon(List<GameObject> list, string icon)
        {
            foreach (var temp in list)
            {
                if (temp.Icon == icon)
                {
                    return true;
                }
            }
            return false;
        }

        static bool CheckByObjectType(List<GameObject> list, ObjectType type, out GameObject obj)
        {
            foreach (var temp in list)
            {
                if (temp.object_type == type)
                {
                    obj = temp;
                    return true;
                }
            }
            obj = null;
            return false;
        }

        static void ChangeObj(string cur, GameObject tar)
        {
            foreach (var temp in AllGameObjects)
            {
                if (temp.Icon == cur)
                {
                    temp.Icon = tar.contect_Icon;
                    temp.color = tar.color;
                }
            }
        }
        static void ChangeLogic(string cur, LogicType logic)
        {
            foreach (var temp in AllGameObjects)
            {
                if (temp.Icon == cur)
                {
                    temp.logic_type = temp.logic_type | logic;
                }
            }
        }
        public static void SetLogic()
        {
            LogicInit();


            List<GameObject> noun_list = new List<GameObject>();
            foreach (var obj in AllGameObjects)
            {
                if (obj.object_type == ObjectType.noun)
                {
                    noun_list.Add(obj);
                }
            }
            foreach (var n in noun_list)
            {

                List<GameObject> temp;
                temp = GetObjectsByPos(n.x + 1, n.y);
                if (!CheckByIcon(temp, "is"))
                {
                    continue;
                }
                temp = GetObjectsByPos(n.x + 2, n.y);
                GameObject tar;
                if (CheckByObjectType(temp, ObjectType.noun, out tar) || CheckByObjectType(temp, ObjectType.verb, out tar))
                {
                    if (tar.object_type == ObjectType.noun)
                    {
                        ChangeObj(n.contect_Icon, tar);
                    }
                    else
                    {
                        ChangeLogic(n.contect_Icon, tar.effect_logic);
                    }
                }

            }
            foreach (var n in noun_list)
            {

                List<GameObject> temp;
                temp = GetObjectsByPos(n.x, n.y + 1);
                if (!CheckByIcon(temp, "is"))
                {
                    continue;
                }
                temp = GetObjectsByPos(n.x, n.y + 2);
                GameObject tar;
                if (CheckByObjectType(temp, ObjectType.noun, out tar) || CheckByObjectType(temp, ObjectType.verb, out tar))
                {
                    if (tar.object_type == ObjectType.noun)
                    {
                        ChangeObj(n.contect_Icon, tar);
                    }
                    else
                    {
                        ChangeLogic(n.contect_Icon, tar.effect_logic);
                    }
                }

            }

            //LogicInit();
            //List<GameObject> eneity_list = new List<GameObject>();
            //List<GameObject> noun_list = new List<GameObject>();
            //List<GameObject> verb_list = new List<GameObject>();
            //Dictionary<Pos, GameObject> dic = new Dictionary<Pos, GameObject>();
            //foreach (var obj in AllGameObjects)
            //{
            //    if (obj.object_type == ObjectType.eneity)
            //    {
            //        eneity_list.Add(obj);
            //    }
            //    else if (obj.object_type == ObjectType.noun)
            //    {
            //        noun_list.Add(obj);
            //    }
            //    else
            //    {
            //        verb_list.Add(obj);
            //    }
            //    Pos p = new Pos(obj.x, obj.y);
            //    dic.Add(p, obj);
            //}
            //先名词
            //foreach (var n in noun_list)
            //{
            //    下方
            //    GameObject temp = dic[new Pos(n.x, n.y + 1)];
            //    string icon_change = n.contect_Icon;
            //    if (temp.Icon == "is")
            //    {
            //        temp = dic[new Pos(n.x, n.y + 2)];
            //        if (temp.object_type == ObjectType.noun)
            //        {
            //            icon_change = temp.contect_Icon;
            //        }
            //    }
            //    右方
            //    temp = dic[new Pos(n.x + 1, n.y)];
            //    if (temp.Icon == "is")
            //    {
            //        temp = dic[new Pos(n.x = 2, n.y)];
            //        if (temp.object_type == ObjectType.noun)
            //        {
            //            icon_change = temp.contect_Icon;
            //        }
            //    }
            //    foreach (var e in eneity_list)
            //    {
            //        if (e.Icon == n.contect_Icon)
            //        {
            //            e.Icon = icon_change;
            //        }
            //    }
            //}
            //动词
            //foreach (var n in noun_list)
            //{
            //    GameObject temp = dic[new Pos(n.x, n.y + 1)];
            //    LogicType type = LogicType.Null;
            //    if (temp.Icon == "is")
            //    {
            //        temp = dic[new Pos(n.x, n.y + 2)];
            //        if (temp.object_type == ObjectType.verb)
            //        {
            //            type = LogicType.Null | temp.logic_type;
            //        }
            //    }
            //    temp = dic[new Pos(n.x + 1, n.y)];
            //    if (temp.Icon == "is")
            //    {
            //        temp = dic[new Pos(n.x + 2, n.y)];
            //        if (temp.object_type == ObjectType.verb)
            //        {
            //            type = LogicType.Null | temp.logic_type;
            //        }
            //    }
            //    foreach (var e in eneity_list)
            //    {
            //        if (e.Icon == n.contect_Icon)
            //        {
            //            e.logic_type = type;
            //        }
            //    }
            //}
        }

        //static bool CanMovePlayer(int x, int y, Direction dir, List<GameObject> allMoves)
        //{
        //    if (!_ChangeXY(ref x, ref y, dir))
        //    {
        //        return false;
        //    }
        //    if (!CanBePushed(x, y, dir, allMoves))
        //    {
        //        return false;
        //    }
        //    return true;
        //}
        //static bool CanBePushed(int x, int y, Direction dir, List<GameObject> allMoves)
        //{
        //    var boxes = GetObjectsByPos(x, y);
        //    if (boxes.Count == 0)
        //    {
        //        return true;
        //    }
        //    bool needRecur = false;
        //    foreach (var box in boxes)
        //    {
        //        if (!box.HasLogic((LogicType.Push) | LogicType.Stop))
        //        {
        //            continue;
        //        }
        //        if (_OverBorder(x, y, dir))
        //        {
        //            return false;
        //        }
        //        if (box.HasLogic(LogicType.Stop))
        //        {
        //            return false;
        //        }
        //        if (box.HasLogic(LogicType.Push))
        //        {
        //            allMoves.Add(box);
        //            needRecur = true;
        //        }
        //    }
        //    if (needRecur)
        //    {
        //        if (!_ChangeXY(ref x, ref y, dir))
        //        {
        //            return false;
        //        }
        //        return CanBePushed(x, y, dir, allMoves);
        //    }
        //    return true;
        //}
        static bool CheckDirectionLogic(GameObject caller, Direction dir)
        {
            var next_objs = GetObjectsByDir(caller.x, caller.y, dir);
            if (next_objs.Count == 0)
            {
                return true;
            }
            foreach (var obj in next_objs)
            {
                if (obj.HasLogic(LogicType.Stop))
                {
                    return false;
                }
                if (obj.HasLogic(LogicType.Push))
                {
                    return CanMove(obj, dir);
                }
                if (obj.HasLogic(LogicType.Sink))
                {
                    if(caller.HasLogic(LogicType.Object)|caller.HasLogic(LogicType.Subject))
                    {
                        return true;
                    }
                    AllGameObjects.Remove(caller);
                    AllGameObjects.Remove(obj);
                    Program.DrawALL();
                    return false;

                }
                if (obj.HasLogic(LogicType.Win))
                {

                }
                if (obj.HasLogic(LogicType.Kill))
                {

                }
            }
            return true;
        }
        

        //返回是否能改变坐标,若能并移动
        static bool CanMove(GameObject obj, Direction dir)
        {
            switch (dir)
            {
                case Direction.Left:
                    if (obj.x == 0|| !CheckDirectionLogic(obj, dir))
                    {
                        return false;
                    }
                    obj.x -= 1;
                    break;
                case Direction.Right:
                    if (obj.x == map_width - 1||!CheckDirectionLogic(obj, dir))
                    {
                        return false;
                    }
                    obj.x += 1;
                    break;
                case Direction.Up:
                    if (obj.y == 0||!CheckDirectionLogic(obj, dir))
                    {
                        return false;
                    }
                    obj.y -= 1;
                    break;
                case Direction.Down:
                    if (obj.y == map_height - 1|| !CheckDirectionLogic(obj, dir))
                    {
                        return false;
                    }
                    obj.y += 1;
                    break;
            }
            return true;
        }
        
        static List<GameObject> FindAllPlayers()
        {
            var playerlist = new List<GameObject>();
            foreach (var obj in AllGameObjects)
            {
                if (obj.HasLogic(LogicType.You))
                {
                    playerlist.Add(obj);
                }
            }
            return playerlist;
        }
        static List<GameObject> GetObjectsByPos(int x, int y)
        {
            var tars = new List<GameObject>();
            foreach (var obj in AllGameObjects)
            {
                if (obj.x == x && obj.y == y)
                {
                    tars.Add(obj);
                }
            }
            return tars;
        }
        static void RemoveObjectsByPos(int x,int y)
        {
            for(int i=0;i<AllGameObjects.Count;i++)
            {
                if(AllGameObjects[i].x==x&&AllGameObjects[i].y==y)
                {
                    AllGameObjects.Remove(AllGameObjects[i]);
                }
            }
        }

        static List<GameObject> GetObjectsByDir(int x,int y,Direction dir)
        {
            switch(dir)
            {
                case Direction.Left:
                    x -= 1;
                    break;
                case Direction.Right:
                    x += 1;
                    break;
                case Direction.Up:
                    y -= 1;
                    break;
                case Direction.Down:
                    y += 1;
                    break;
            }
            return GetObjectsByPos(x, y);
        }

        public static int Input()
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Backspace)
            {
                return -1;
            }
            List<GameObject> AllPlayers = FindAllPlayers();
            if (AllPlayers.Count == 0)
            {
                return 0;
            }
            Direction dir;
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    {
                        dir = Direction.Up;
                    }
                    break;
                case ConsoleKey.DownArrow:
                    {
                        dir = Direction.Down;
                    }
                    break;
                case ConsoleKey.LeftArrow:
                    {
                        dir = Direction.Left;
                    }
                    break;
                case ConsoleKey.RightArrow:
                    {
                        dir = Direction.Right;
                    }
                    break;
                default:
                    return 0;

            }
            int container=0;
            foreach (var player in AllPlayers)
            {       
                if(!CanMove(player,dir))
                {
                    continue;
                }
                container++;        
            }
            if (container == 0)
            {
                return 0;
            }
            return 1;
        }
    }

    public class FileManager
    {
        public static List<string> ReadMapFile(string path)
        {
            List<string> lines = new List<string>();
            StreamReader reader = File.OpenText(path);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                lines.Add(line);
            }
            return lines;
        }


    }
}
