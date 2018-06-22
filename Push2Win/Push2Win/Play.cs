using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Media;

namespace Push2Win
{
    class Play
    {
        public const int width = 22;//宽度
        public const int height = 22;//高度
        public static int currentLevelNum = 0;//当前关卡数
        public static int moveStepNum = 0;//记录移动步数
        public static int endLevelNum = 13;//最终关卡数
        public static bool pass = true;//是否通过当前关卡
        public static bool getKeyZ = false;//是否按下撤销
        public static Dictionary<short, short> logicHorizontal = new Dictionary<short, short>();//保存最近一次横向逻辑关系，前者表示文字主体ASCII码，后者表示文字属性ASCII码
        public static Dictionary<short, short> logicVertical = new Dictionary<short, short>();//保存最近一次纵向逻辑关系，前者表示文字主体ASCII码，后者表示文字属性ASCII码
        public static Dictionary<int, Dictionary<int, GameObject>> moveStepMap = new Dictionary<int, Dictionary<int, GameObject>>();//保存对应移动步数的实时地图
        public static int Key(int x, int y)//获取该位置对应物体的Key值
        {
            int key = x * width + y;
            return key;
        }

        static void Main(string[] args)//游戏主函数
        {
            Initialize();
            Loop();
        }
        public static void Initialize()//游戏初始化
        {
            //PlayMusic();
            Console.Title = "Push2Win";
            Console.SetWindowSize(width * 2, height);//设置控制台窗口大小
            Console.SetBufferSize(width * 2, height);//设置缓存区大小
            Console.CursorVisible = false;//设置光标隐藏
            //SendKeys.SendWait("+");//给予一个SHIFT键切换成英文输入
            StartUI();
            SendKeys.SendWait("+");//给予一个SHIFT键切换成英文输入
        }
        static void PlayMusic()
        {
            string filename = "kenan.wav ";
            SoundPlayer s = new SoundPlayer(filename);
            s.PlayLooping();
            //s.Play();
        }
        public static void StartUI()
        {
            FileStream fs = new FileStream(@"StartUI.txt", FileMode.Open, FileAccess.Read);
            StreamReader read = new StreamReader(fs, Encoding.Default);
            string line;
            while ((line = read.ReadLine()) != null)
            {
                for(int i=0;i<line.Length; ++i)
                {
                    if (line[i] =='1') { Console.ForegroundColor = ConsoleColor.DarkGreen; Console.Write("█"); }
                    else { Console.Write("  "); }
                }
            }
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(16, 8);
            Console.Write("箭头is控制");
            Console.SetCursorPosition(15, 9);
            Console.Write("  R键is重开");
            Console.SetCursorPosition(15, 10);
            Console.Write("  Z键is撤销");
            Console.SetCursorPosition(14, 12);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("空格键is开始");
            Console.SetCursorPosition(4, 19);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(@"右键标题栏/属性/字体大小调节窗口大小");

            List<ConsoleColor> color = new List<ConsoleColor>()
            { ConsoleColor.Blue,ConsoleColor.Red,ConsoleColor.Cyan,ConsoleColor.Green,ConsoleColor.Yellow};
            int j = 0;
            ConsoleKeyInfo keyInfo;
            while (true)
            {
                Console.SetCursorPosition(28, 12);
                Console.Write("\b\b\b\b\b\b");
                Console.ForegroundColor = color[j%color.Count];
                Console.Write("开始");
                System.Threading.Thread.Sleep(400);
                Console.SetCursorPosition(28, 12);
                Console.Write("\b\b\b\b\b\b");
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write("开始");
                System.Threading.Thread.Sleep(200);
                Console.ResetColor();
                if (Console.KeyAvailable)
                {
                    keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key == ConsoleKey.Spacebar)
                    {
                        Console.Beep(600, 80);
                        Console.Beep(400, 80);
                        break;
                    }
                }
                j++;
                //if (Console.ReadKey() != null)
                //    break;
            }
        }
        public static void Loop()//游戏主循环
        {
            while (true)
            {
                if (pass)
                {
                    moveStepNum = 0;
                    pass= false;
                    currentLevelNum++;
                    if (currentLevelNum == endLevelNum+1)
                    {
                        currentLevelNum = 1;
                    }
                    Console.Clear();
                    ResetMap();
                    ReadMap(currentLevelNum);
                    SaveMoveStepMap(moveStepNum, Map.gameObject);
                    SendKeys.SendWait("s");//按键
                }
                RefreshMap(Map.gameObject);
                RefreshLogic();               
                InputModule();//根据逻辑进行移动
                //Console.Clear();
            }
        }
        public static void ResetMap()
        {
            Map.character.Clear();
            Map.pattern.Clear();
            Map.word.Clear();
            Map.gameObject.Clear();
            Map.levelMap.Clear();
            //if(currentLevelNum>0)
            moveStepMap.Clear();
        }//重置地图
        public static void ReadMap(int currentLevelNum)
        {           
            string s = ReadLevel(currentLevelNum);
            FileStream fs = new FileStream(s, FileMode.Open, FileAccess.Read);
            StreamReader read = new StreamReader(fs, Encoding.Default);
            string strReadline;
            int x = 0;
            while ((strReadline = read.ReadLine()) != null)
            {
                for (int y = 0; y < strReadline.Length; ++y)
                {
                    char c = strReadline[y];
                    if ((short)c > 41 && (short)c < 58)//数字部分表示图案
                    {
                        Pattern p = new Pattern
                        {
                            accessable = true,
                            Posx = x,
                            Posy = y,
                            ASCII = c,
                        };
                        if(c == '*')//表示游戏场景外
                        {
                            p.accessable = false;
                            p.Content = "  ";
                        }
                        if (c == '0')//0表示空地
                        {
                            //p.color = ConsoleColor.Black;
                            p.Content="  ";
                        }
                        if (c == '1')//1表示边界围墙
                        {
                            p.accessable = false;
                            p.color = ConsoleColor.DarkGreen;
                            p.Content = "█";
                        }
                        if (c == '2')//2表示地板
                        {
                            p.color = ConsoleColor.DarkGray;
                            p.Content = "□";
                        }
                        if (c == '3')//3表示墙
                        {
                            //p.accessable = false;
                            p.color = ConsoleColor.Gray;
                            p.Content = "▓";
                            //if (currentLevelNum == 6) { p.accessable = true; }
                        }
                        if (c == '4')//4表示星星（终点）
                        {
                            //p.ispass = true;
                            p.color = ConsoleColor.Yellow;
                            p.Content = "★";
                        }
                        if (c == '5')//5表示草
                        {                           
                            p.color = ConsoleColor.Green;
                            p.Content = "**";
                        }
                        if (c == '6')//6表示河
                        {
                            p.color = ConsoleColor.DarkBlue;
                            p.Content = "▇";
                        }
                        if (c == '7')//7表示火
                        {
                            p.isfire = true;
                            p.color = ConsoleColor.DarkRed;
                            p.Content = "▇";
                        }
                        if (c == '8')//8表示球
                        {
                            p.accessable = false;
                            p.pushable = true;
                            p.color = ConsoleColor.DarkYellow;
                            p.Content = "●";
                            if (currentLevelNum == 1)
                            {
                                AddPattern(x, y, p);
                                AddGameObject(x, y, p);
                                AddLevelMap(x, y, new GameObject() { Posx = x, Posy = y, Content = "□", color = ConsoleColor.DarkGray,accessable=true });
                                continue;
                            }
                        }
                        if (c == '9')//9表示冰面
                        {
                            p.color = ConsoleColor.DarkBlue;
                            p.Content = "□";
                        }
                        AddPattern(x,y,p);
                        AddGameObject(x, y, p);
                        AddLevelMap(x, y, p); 
                    }
                    else if((short)c > 64 && (short)c < 91)//大写字母表示人物
                    {
                        Character ch = new Character
                        {
                            //controlable = true,
                            accessable = true,
                            Posx = x,
                            Posy = y,
                            ASCII= c,
                        };
                        if (c == 'A')//表示女
                        {
                            ch.sex = 1;
                            ch.color = ConsoleColor.White;
                            if (currentLevelNum == 14) { ch.color = ConsoleColor.Blue; }
                            ch.Content="♀";
                        }
                        if (c == 'B')//表示男
                        {                           
                            ch.sex = 2;
                            ch.color = ConsoleColor.Yellow;
                            ch.Content = "♂";
                        }
                        AddCharacter(x, y, ch);
                        AddGameObject(x, y, ch);
                        AddLevelMap(x, y, new GameObject() { ASCII='0',Posx = x, Posy = y, Content="  ",accessable=true});
                    }
                    else if ((short)c > 96 && (short)c < 123)//小写字母表示逻辑单词 与大写字母对应 ASCII码相差49
                    {
                        Word wo = new Word
                        {
                            pushable = true,
                            Posx = x,
                            Posy = y,
                            ASCII= c,
                        }; 
                        if (c == 'a')
                        {
                            wo.Num = 1;
                            wo.color = ConsoleColor.White;
                            wo.Content = "空";
                        }
                        if (c == 'd')//d表示墙
                        {
                            wo.Num = 1;
                            wo.color = ConsoleColor.DarkGray;
                            wo.Content = "墙";
                        }
                        if (c == 'e')//e表示星星（终点）
                        {
                            wo.Num = 1;
                            wo.color = ConsoleColor.Yellow;
                            wo.Content = "星";
                        }
                        if (c == 'f')//f表示草
                        {
                            wo.Num = 1;
                            wo.color = ConsoleColor.Green;
                            wo.Content = "草";
                        }
                        if (c == 'g')//g表示河
                        {
                            wo.Num = 1;
                            wo.color = ConsoleColor.DarkBlue;
                            wo.Content = "河";
                        }
                        if (c == 'h')//h表示火
                        {
                            wo.Num = 1;
                            wo.color = ConsoleColor.Red;
                            wo.Content = "火";
                        }
                        if (c == 'i')//i表示球
                        {
                            wo.Num = 1;
                            wo.color = ConsoleColor.DarkYellow;
                            wo.Content = "球";
                        }
                        if (c == 'j')//j表示冰面
                        {
                            wo.Num = 1;
                            wo.color = ConsoleColor.DarkBlue;
                            wo.Content = "冰";
                        }
                        if (c == 'r')//r表示人
                        {
                            wo.Num = 1;
                            wo.color = ConsoleColor.White;
                            wo.Content = "女";
                        }
                        if (c == 's')//s表示女
                        {
                            wo.Num = 1;
                            wo.color = ConsoleColor.Yellow;
                            wo.Content = "男";
                        }
                        if (c == 't')//t表示is
                        {
                            wo.Num = 2;
                            wo.color = ConsoleColor.White;
                            wo.Content = "is";
                        }
                        if (c == 'u')//u表示停，不可进入
                        {
                            wo.Num = 3;
                            wo.color = ConsoleColor.Red;
                            wo.Content = "停";
                        }
                        if (c == 'v')//v表示可以推动
                        {
                            wo.Num = 3;
                            wo.color = ConsoleColor.Yellow;
                            wo.Content = "推";
                        }
                        if (c == 'w')//w表示玩家
                        {
                            wo.Num = 3;
                            wo.color = ConsoleColor.Magenta;
                            wo.Content = "你";
                        }
                        if (c == 'x')//x表示进入就赢
                        {
                            wo.Num = 3;
                            wo.color = ConsoleColor.DarkGreen;
                            wo.Content = "赢";
                        }
                        if (c == 'y')//x表示进入就死
                        {
                            wo.Num = 3;
                            wo.color = ConsoleColor.DarkRed;
                            wo.Content = "死";
                        }
                        if (c == 'z')//x表示可自动移动
                        {
                            wo.Num = 3;
                            wo.color = ConsoleColor.Blue;
                            wo.Content = "AI";
                        }                        
                        AddWord(x, y, wo);
                        AddGameObject(x, y, wo);
                        AddLevelMap(x, y, wo);
                    }
                }
                x += 1;
            }
            fs.Close();
            read.Close();
        }//读取初始关卡地图
        public static string ReadLevel(int currentLevelNum)
        {
            switch (currentLevelNum)
            {
                case 1:
                    return @"Level1.txt";
                case 2:
                    return @"Level2.txt";
                case 3:
                    return @"Level3.txt";
                case 4:
                    return @"Level4.txt";
                case 5:
                    return @"Level5.txt";
                case 6:
                    return @"Level6.txt";
                case 7:
                    return @"Level7.txt";
                case 8:
                    return @"Level8.txt";
                case 9:
                    return @"Level9.txt";
                case 10:
                    return @"Level10.txt";
                case 11:
                    return @"Level11.txt";
                case 12:
                    return @"Level12.txt";
                case 13:
                    return @"Level13.txt";
                //case 14:
                //    return @"Level14.txt";
                default:
                    return @"Level1.txt";                   
            }
        }//选择关卡文件

        public static void AddPattern(int x, int y, Pattern p)
        {
            int key = Key(x, y);
            Map.pattern.Add(key, p);
        }
        public static void AddCharacter(int x, int y, Character ch)
        {
            int key = Key(x, y);
            Map.character.Add(key, ch);
        }
        public static void AddWord(int x, int y, Word wo)
        {
            int key = Key(x, y);
            Map.word.Add(key, wo);
        }
        public static void AddGameObject(int x, int y, GameObject go)
        {
            int key = Key(x, y);
            Map.gameObject.Add(key, go);
        }//用于刷新画面
        public static void AddLevelMap(int x, int y, GameObject go)
        {
            int key = Key(x, y);
            if (go.pushable == true)//设置可推动物体底下的场景
            {
                Map.levelMap.Add(key, new GameObject() {ASCII='0', Posx = x, Posy = y, Content = "  " ,accessable=true});
            }
            else
            {
                Map.levelMap.Add(key, go);
            }

        }//作为底层地图
        public static void SaveMoveStepMap(int m, Dictionary<int,GameObject> d)
        {
            //Dictionary<int, GameObject> temp = new Dictionary<int, GameObject>();//引用传递转为值传递
            //foreach(var v in d)
            //{
            //    temp.Add(v.Key,v.Value);
            //}

            if (moveStepMap.ContainsKey(m))
            {
                //moveStepMap[m] = temp;
                moveStepMap[m] = new Dictionary<int, GameObject>(d);//(复制)值传递
            }
            else
            {
                moveStepMap.Add(m, new Dictionary<int, GameObject>(d));
            }
        }//保存每一步对应地图画面
        public static void SendGameObject(GameObject get, GameObject beSend)
        {
            get.ASCII = beSend.ASCII;
            get.accessable = beSend.accessable;
            get.color = beSend.color;
            get.controlable = beSend.controlable;
            get.pushable = beSend.pushable;
            get.isdie = beSend.isdie;
            get.ispass = beSend.ispass;
            get.Posx = beSend.Posx;
            get.Posy = beSend.Posy;
            get.Content = beSend.Content;

            get.ismove = beSend.ismove;
            get.direction = beSend.direction;
            get.Num = beSend.Num;
            get.sex = beSend.sex;
            get.isfire = beSend.isfire;
        }//复制并传递对象

        public static GameObject temp = new GameObject();//123句型的中间变量
        public static GameObject temp_ = new GameObject();//121句型的中间变量，主体
        public static GameObject temp__ = new GameObject();//121句型的中间变量，主体底层
        public static GameObject temp1 = new GameObject();//121句型的中间变量，客体
        public static GameObject temp2 = new GameObject();//121句型的中间变量，客体底层
        public static Dictionary<short,bool> horizontal = new Dictionary<short, bool>();//某物体是否被设定过逻辑(横向)，用于恢复到上次
        public static Dictionary<short, bool> vertical = new Dictionary<short, bool>();//某物体是否被设定过逻辑(纵向)，用于恢复到上次
        public static void RefreshLogic()//刷新逻辑
        {

            foreach(var d in Map.gameObject)
            {
                if (d.Value.Num == 1)//查找主体词
                {
                    short s1 = (short)Map.gameObject[d.Key].ASCII;//主体ASCII码

                    if (Map.gameObject[d.Key + 1].Num == 2 && Map.gameObject[d.Key + 2].Num == 3)//如果横向构成逻辑语句，则设置新逻辑
                    {
                        //Console.Write(0);
                        if (!horizontal.ContainsKey(s1)) { horizontal.Add(s1, true); }
                        if (horizontal.ContainsKey(s1)) { horizontal[s1] = true; }
                        short s2 = (short)Map.gameObject[d.Key + 2].ASCII;
                        SetLogic(s1, s2);
                        SaveLogicHorizontal(s1, s2);
                    }
                    if (Map.gameObject[d.Key + width].Num == 2 && Map.gameObject[d.Key + 2 * width].Num == 3)
                    {
                        //Console.Write(200);
                        if (!vertical.ContainsKey(s1)) { vertical.Add(s1, true); }
                        if (vertical.ContainsKey(s1)) { vertical[s1] = true; }
                        short s2 = (short)Map.gameObject[d.Key + 2 * width].ASCII;
                        SetLogic(s1, s2);
                        SaveLogicVertical(s1, s2);
                    }
                    if ((Map.gameObject[d.Key + 1].Num != 2 || Map.gameObject[d.Key + 2].Num != 3) && (horizontal.ContainsKey(s1)&&(horizontal[s1] == true)))//如果不成逻辑，则设置为最近的上一次逻辑取反
                    {
                        horizontal[s1] = false;
                        short s2 = logicHorizontal[s1];
                        //Console.Write(100);
                        SetOppositeLogic(s1, s2);
                    }
                    if ((Map.gameObject[d.Key + width].Num != 2 || Map.gameObject[d.Key + 2 * width].Num != 3) && (vertical.ContainsKey(s1) && (vertical[s1] == true)))//如果不成逻辑，则设置为最近的上一次逻辑取反
                    {
                        vertical[s1] = false;
                        short s2 = logicVertical[s1];
                        SetOppositeLogic(s1, s2);
                    }
                }
            }
        }
        public static void SetLogic(short s1,short s2)
        {
            switch (s2)
            {
                case 117://停
                    foreach(var v in Map.gameObject)
                    {
                        if ((short)v.Value.ASCII == s1-49)
                        {
                            v.Value.accessable = false;
                            v.Value.pushable = false;
                        }
                    }                   
                    break;
                case 118://推
                    foreach (var v in Map.gameObject)
                    {
                        if ((short)v.Value.ASCII == s1 - 49)
                        {
                            if (v.Value.sex != 0) { break; }
                            if (v.Value.pushable == false)
                            {
                                SendGameObject(temp, Map.levelMap[v.Key]);
                                Map.levelMap[v.Key] = new GameObject() { Posx = v.Value.Posx, Posy = v.Value.Posy, Content = "  ", accessable = true };
                            }
                            v.Value.accessable = false;
                            v.Value.pushable = true;
                        }
                    }
                    break;
                case 119://你
                    foreach (var v in Map.gameObject)
                    {
                        if ((short)v.Value.ASCII == s1 - 49)
                        {
                            if (v.Value.controlable == false)
                            {
                                Map.levelMap[v.Key] = new GameObject() { Posx = v.Value.Posx, Posy = v.Value.Posy, Content = "  ", accessable = true };
                            }                           
                            v.Value.controlable = true;
                        }
                    }
                    break;
                case 120://赢
                    foreach (var v in Map.gameObject)
                    {
                        if ((short)v.Value.ASCII == s1 - 49)
                        {
                            v.Value.ispass = true;
                        }
                    }
                    break;
                case 121://死
                    foreach (var v in Map.gameObject)
                    {
                        if ((short)v.Value.ASCII == s1 - 49)
                        {
                            v.Value.isdie = true;
                        }
                    }
                    break;
                case 122:
                    foreach (var v in Map.gameObject)
                    {
                        if ((short)v.Value.ASCII == s1 - 49)
                        {
                            v.Value.ismove = true;
                            v.Value.direction = 1;
                        }
                    }
                    break;
            }
        }//设立逻辑
        public static void SetOppositeLogic(short s1, short s2)
        {
            switch (s2)
            {
                case 117://不能阻挡
                    foreach (var v in Map.gameObject)
                    {
                        if ((short)v.Value.ASCII == s1 - 49)
                        {
                            v.Value.accessable = true;
                        }
                    }
                    break;
                case 118://不能推
                    foreach (var v in Map.gameObject)
                    {
                        if ((short)v.Value.ASCII == s1 - 49)
                        {
                            //v.Value.accessable = true;
                            v.Value.pushable = false;
                            SendGameObject(Map.levelMap[v.Key],temp);
                        }
                    }
                    break;
                case 119:
                    foreach (var v in Map.gameObject)
                    {
                        if ((short)v.Value.ASCII == s1 - 49)
                        {
                            v.Value.controlable = false;
                        }
                    }
                    break;
                case 120:
                    foreach (var v in Map.gameObject)
                    {
                        if ((short)v.Value.ASCII == s1 - 49)
                        {
                            v.Value.ispass = false;
                        }
                    }
                    break;
                case 121:
                    foreach (var v in Map.gameObject)
                    {
                        if ((short)v.Value.ASCII == s1 - 49)
                        {
                            v.Value.isdie = false;
                        }
                    }
                    break;
                case 122:
                    foreach (var v in Map.gameObject)
                    {
                        if ((short)v.Value.ASCII == s1 - 49)
                        {
                            v.Value.ismove = false;
                        }
                    }
                    break;
            }
        }//设立反逻辑
        public static void SaveLogicHorizontal(short s1, short s2)
        {
            //short temp = s2;
            if (logicHorizontal.ContainsKey(s1))
            {
                logicHorizontal[s1] = s2;
            }
            else
            {
                logicHorizontal.Add(s1, s2);
            }
        }
        public static void SaveLogicVertical(short s1, short s2)
        {
            //short temp = s2;
            if (logicVertical.ContainsKey(s1))
            {
                logicVertical[s1] = s2;
            }
            else
            {
                logicVertical.Add(s1, s2);
            }
        }
        public static void JudegeLogic()//121句型会直接改变实时地图，放入输入模块，输入时立即判断并改变地图\\有问题
        {
            bool b = false;
            List<int> keyH = new List<int>();
            List<int> keyV = new List<int>();
            foreach (var k in Map.gameObject.Keys)
            {
                if(Map.gameObject[k].Num==1&&Map.gameObject[k + 1].Num == 2 && Map.gameObject[k + 2].Num == 1) { keyH.Add(k); }
                if (Map.gameObject[k].Num == 1 && Map.gameObject[k + width].Num == 2 && Map.gameObject[k + 2*width].Num == 1) { keyV.Add(k); }
            }
            foreach(var k in keyH)
            {
                short s1 = (short)Map.gameObject[k].ASCII;
                short s2 = (short)Map.gameObject[k + 2].ASCII;
                foreach (var v in Map.gameObject)//保存主体原有信息
                {
                    if ((short)v.Value.ASCII == s1 - 49)
                    {
                        SendGameObject(temp_, v.Value);
                        SendGameObject(temp__, Map.levelMap[v.Key]);
                        break;
                    }
                }
                foreach (var v in Map.gameObject)//保存被复制对象的信息
                {
                    if ((short)v.Value.ASCII == s2 - 49)
                    {
                        b = true;
                        SendGameObject(temp1, v.Value);
                        SendGameObject(temp2, Map.levelMap[v.Key]);
                        break;
                    }
                }
                if (b == false)//若当前地图没有则在关卡初始图找?
                {
                    foreach (var v in Map.levelMap)
                    {
                        if ((short)v.Value.ASCII == s2 - 49)
                        {
                            b = true;
                            SendGameObject(temp1, v.Value);
                            SendGameObject(temp2, Map.levelMap[v.Key]);
                        }
                    }
                }
                //if (b == false)
                //{
                //    b = true;
                //    SendGameObject(temp1, temp_);
                //    SendGameObject(temp2, temp__);
                //}
                if (b == false)
                {
                    SendGameObject(temp1, new GameObject()
                    {
                        accessable = true,
                        ASCII = 'B',
                        sex = 2,
                        color = ConsoleColor.Yellow,
                        Content = "♂",
                    });
                    SendGameObject(temp2, new GameObject() { Content = "  ", accessable = true });
                }

                List<int> keys = new List<int>();//不能边遍历边修改枚举器，所以保存KEY列表，遍历并赋值修改
                foreach (var v in Map.gameObject.Keys)
                {
                    if ((short)Map.gameObject[v].ASCII == s1 - 49)
                    {
                        keys.Add(v);
                    }
                }
                foreach(var v in keys)//遍历并修改
                {
                    GameObject go = new GameObject();
                    int x = Map.gameObject[v].Posx;
                    int y = Map.gameObject[v].Posy;
                    SendGameObject(go, temp1);
                    //Map.gameObject[k].Content = temp1.Content;
                    go.Posx = x;
                    go.Posy = y;
                    Map.gameObject[v] = go;
                    if(Map.gameObject[v].pushable==true|| Map.gameObject[v].controlable== true )
                    {
                        GameObject og = new GameObject();
                        SendGameObject(og, temp2);
                        og.Posx = x;
                        og.Posy = y;
                        Map.levelMap[v] = og;
                    }
                }

            }//横
            foreach (var k in keyV)
            {
                short s1 = (short)Map.gameObject[k].ASCII;
                short s2 = (short)Map.gameObject[k + 2*width].ASCII;
                foreach (var v in Map.gameObject)//保存被复制对象的信息
                {
                    if ((short)v.Value.ASCII == s2 - 49)
                    {
                        b = true;
                        SendGameObject(temp1, v.Value);
                        SendGameObject(temp2, Map.levelMap[v.Key]);
                        break;
                    }
                }
                if (b == false)//若当前地图没有则在关卡初始图找
                {
                    foreach (var v in Map.levelMap)
                    {
                        if ((short)v.Value.ASCII == s2 - 49)
                        {
                            b = true;
                            SendGameObject(temp1, v.Value);
                            SendGameObject(temp2, Map.levelMap[v.Key]);
                        }
                    }
                }
                if (b == false)
                {
                    SendGameObject(temp1, new GameObject()
                    {
                        accessable = true,
                        ASCII = 'B',
                        sex = 2,
                        color = ConsoleColor.Yellow,
                        Content = "♂",
                    });
                    SendGameObject(temp2, new GameObject() { Content = "  ", accessable = true });
                }
                foreach (var v in Map.gameObject)//保存主体原有信息
                {
                    if ((short)v.Value.ASCII == s1 - 49)
                    {                     
                        SendGameObject(temp_, v.Value);
                        SendGameObject(temp__, Map.levelMap[v.Key]);
                        break;
                    }
                }
                List<int> keys = new List<int>();//不能边遍历边修改枚举器，所以保存KEY列表，遍历列表并赋值修改
                foreach (var v in Map.gameObject.Keys)
                {
                    if ((short)Map.gameObject[v].ASCII == s1 - 49)
                    {
                        keys.Add(v);
                    }
                }
                foreach (var v in keys)//遍历并修改
                {
                    GameObject go = new GameObject();
                    int x = Map.gameObject[v].Posx;
                    int y = Map.gameObject[v].Posy;
                    SendGameObject(go, temp1);
                    //Map.gameObject[k].Content = temp1.Content;
                    go.Posx = x;
                    go.Posy = y;
                    Map.gameObject[v] = go;
                    if (Map.gameObject[v].pushable == true || Map.gameObject[v].controlable == true)
                    {
                        GameObject og = new GameObject();
                        SendGameObject(og, temp2);
                        og.Posx = x;
                        og.Posy = y;
                        Map.levelMap[v] = og;
                    }
                }

            }//纵
        }

        public static void InputModule()
        {           
            //Console.Write('\r');
            //System.Threading.Thread.Sleep(500);
            int key = 23;
            int x = 1;
            int y = 1;
            List<int> keyList = new List<int>();
            List<int> xList = new List<int>();
            List<int> yList = new List<int>();
            foreach (var v in Map.gameObject.Values)
            {
                if (v.controlable == true)
                {
                    //System.Threading.Thread.Sleep(500);
                    keyList.Add (v.GetKey());
                    xList.Add(v.Posx);
                    yList.Add(v.Posy);
                    //Console.Write("{0},{1}", v.Posx, v.Posy);
                }
            }
            if (keyList.Count == 1)
            {
                key = keyList[0];
                x = xList[0];
                y = yList[0];
            }
            if (keyList.Count > 1) { MultiControl(keyList, xList, yList);return; }
            //Console.Write("{0},{1}", x, y);

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            
            //ConsoleKeyInfo keyInfo;
            //if (Console.KeyAvailable)
            //{
            //    keyInfo = Console.ReadKey(true);
            //}
            if (keyInfo.Key == ConsoleKey.UpArrow)
            {              
                //Console.Write(key);
                if (Map.gameObject[key - width].ispass == true)
                {
                    Console.Beep(600, 80);
                    Console.Beep(400, 80);
                    pass = true;
                    return;
                }
                if (Map.gameObject[key - width].isdie == true)
                {
                    Console.Beep(90, 100);
                    Console.Beep(240, 400);
                    Map.gameObject[key] = Map.levelMap[key];                   
                    moveStepNum++;
                    SaveMoveStepMap(moveStepNum, Map.gameObject);
                    return;
                }
                if (Map.gameObject[key - width].accessable == true)
                {                    
                    Console.Beep(90, 100);
                    //Map.gameObject[key].Posx -=1;
                    GameObject temp = new GameObject();
                    SendGameObject(temp, Map.gameObject[key]);//不能直接指向引用型变量Map.gameObject[key]，因为对它的Pos修改会改变所有的引用
                    temp.Posx -= 1;
                    Map.gameObject[key - width] = temp;
                    //Map.gameObject[key - width]=Map.gameObject[key];
                    //GameObject temp_ = new GameObject();
                    //SendGameObject(temp_, Map.levelMap[key]);
                    //Map.gameObject[key] = temp_;
                    Map.gameObject[key]= Map.levelMap[key];//因为levelMap在初始读取过后不再改变                   
                    AutoMove();
                    JudegeLogic();
                    moveStepNum++;
                    SaveMoveStepMap(moveStepNum, Map.gameObject);
                }
                if (Map.gameObject[key - width].pushable == true)
                {
                    for (int i = 2; key - i*width > 0; ++i)
                    {
                        if (Map.gameObject[key - i * width].ispass == true)
                        {
                            break;
                        }
                        if (Map.gameObject[key - i * width].isdie == true)
                        {
                            Console.Beep(60, 200);
                            if(Map.gameObject[key - i * width].isfire == false)
                            {
                                Map.gameObject[key - i * width] = new GameObject() { Posx = x - i, Posy = y, Content = "  ", accessable = true };
                                Map.levelMap[key - i * width] = new GameObject() { Posx = x - i, Posy = y, Content = "  ", accessable = true };
                            }
                            //Map.gameObject[key].Posx -= 1;
                            for (int j = i; j > 1; --j)
                            {
                                GameObject temp = new GameObject();
                                SendGameObject(temp, Map.gameObject[key - (j - 2) * width]);
                                temp.Posx -= 1;
                                Map.gameObject[key - (j-1)*width]= temp;
                            }
                            Map.gameObject[key] =Map.levelMap[key];
                            AutoMove();
                            JudegeLogic();
                            moveStepNum++;
                            SaveMoveStepMap(moveStepNum, Map.gameObject);
                            return;
                        }
                        if (Map.gameObject[key - i * width].pushable == true)
                        {
                            continue;
                        }
                        //if (Map.gameObject[key - i * width].pushable == false)
                        //{
                        //    break;
                        //}
                        if (Map.gameObject[key - i * width].accessable == true)
                        {
                            Console.Beep(60, 200);
                            //Console.Write("Debug");
                            for (int j = i; j > 0; --j)
                            {
                                GameObject temp = new GameObject();
                                SendGameObject(temp, Map.gameObject[key - (j - 1) * width]);
                                temp.Posx -= 1;
                                //Map.gameObject[key - (j - 1) * width].Posx -= 1;
                                Map.gameObject[key - j * width] = temp;
                            }
                            Map.gameObject[key] = Map.levelMap[key];
                            AutoMove();
                            JudegeLogic();
                            moveStepNum++;
                            SaveMoveStepMap(moveStepNum, Map.gameObject);
                            break;
                        }
                        if (Map.gameObject[key - i * width].pushable == false)
                        {
                            break;
                        }
                    }
                }
            }
            if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                //Console.Write(key);
                if (Map.gameObject[key + width].ispass == true)
                {
                    Console.Beep(600, 80);
                    Console.Beep(400, 80);
                    pass = true;
                }
                if (Map.gameObject[key + width].isdie == true)
                {
                    Console.Beep(90, 100);
                    Console.Beep(240, 400);
                    //Console.Beep(1000, 200);
                    Map.gameObject[key] = Map.levelMap[key];
                    moveStepNum++;
                    SaveMoveStepMap(moveStepNum, Map.gameObject);
                    return;
                }
                if (Map.gameObject[key + width].accessable == true)
                {
                    Console.Beep(90, 100);
                    //System.Threading.Thread.Sleep(500);
                    //Map.gameObject[key].Posx += 1;
                    GameObject temp = new GameObject();
                    SendGameObject(temp , Map.gameObject[key]);
                    temp.Posx += 1;
                    Map.gameObject[key + width] = temp;
                    Map.gameObject[key] =Map.levelMap[key];
                    AutoMove();
                    JudegeLogic();
                    moveStepNum++;
                    SaveMoveStepMap(moveStepNum, Map.gameObject);
                }
                if (Map.gameObject[key + width].pushable == true)
                {
                    for (int i = 2; key + i * width < (height-1)*width; ++i)
                    {
                        if (Map.gameObject[key + i * width].ispass == true)
                        {
                            break;
                        }
                        if (Map.gameObject[key + i * width].isdie == true)
                        {
                            Console.Beep(60, 200);
                            if(Map.gameObject[key + i * width].isfire == false)
                            {
                                Map.gameObject[key + i * width] = new GameObject() { Posx = x + i, Posy = y, Content = "  ", accessable = true };
                                Map.levelMap[key + i * width] = new GameObject() { Posx = x + i, Posy = y, Content = "  ", accessable = true };
                            }
                            //Map.gameObject[key].Posx += 1;
                            for (int j=i; j > 1; --j)
                            {
                                GameObject temp = new GameObject();
                                SendGameObject(temp, Map.gameObject[key + (j - 2) * width]);
                                temp.Posx += 1;
                                Map.gameObject[key + (j - 1)* width] = temp;
                            }
                            Map.gameObject[key] = Map.levelMap[key];
                            AutoMove();
                            JudegeLogic();
                            moveStepNum++;
                            SaveMoveStepMap(moveStepNum, Map.gameObject);
                            return;
                        }
                        if (Map.gameObject[key + i * width].pushable == true)
                        {
                            continue;
                        }
                        if (Map.gameObject[key + i * width].accessable == true)
                        {
                            Console.Beep(60, 200);
                            //Console.Write("Debug");
                            for (int j = i; j > 0; --j)
                            {
                                GameObject temp = new GameObject();
                                SendGameObject(temp, Map.gameObject[key + (j - 1) * width]);
                                temp.Posx += 1;
                                //Map.gameObject[key + (j - 1) * width].Posx += 1;
                                Map.gameObject[key + j * width] = temp;
                            }
                            Map.gameObject[key] = Map.levelMap[key];
                            AutoMove();
                            JudegeLogic();
                            moveStepNum++;
                            SaveMoveStepMap(moveStepNum, Map.gameObject);
                            break;
                        }
                        if (Map.gameObject[key + i * width].pushable == false)
                        {
                            break;
                        }
                    }
                }
            }
            if (keyInfo.Key == ConsoleKey.LeftArrow)
            {
                //Console.Write(key);
                if (Map.gameObject[key - 1].ispass == true)
                {
                    Console.Beep(600, 80);
                    Console.Beep(400, 80);
                    pass = true;
                }
                if (Map.gameObject[key - 1].isdie == true)
                {
                    Console.Beep(90, 100);
                    Console.Beep(240, 400);
                    Map.gameObject[key] = Map.levelMap[key];
                    moveStepNum++;
                    SaveMoveStepMap(moveStepNum, Map.gameObject);
                    return;
                }
                if (Map.gameObject[key - 1].accessable == true)
                {
                    Console.Beep(90, 100);
                    //Map.gameObject[key].Posy -= 1;
                    GameObject temp = new GameObject();
                    SendGameObject(temp, Map.gameObject[key]);
                    temp.Posy -= 1;
                    Map.gameObject[key - 1] = temp;
                    Map.gameObject[key] = Map.levelMap[key];
                    AutoMove();
                    JudegeLogic();
                    moveStepNum++;
                    SaveMoveStepMap(moveStepNum, Map.gameObject);
                }
                if (Map.gameObject[key - 1].pushable == true)
                {
                    for (int i = 2; key - i > width * x ; ++i)
                    {
                        if (Map.gameObject[key - i].ispass == true)
                        {
                            break;
                        }
                        if (Map.gameObject[key - i].isdie == true)
                        {
                            Console.Beep(60, 200);
                            if (Map.gameObject[key - i].isfire == false)
                            {
                                Map.gameObject[key - i] = new GameObject() { Posx = x, Posy = y - i, Content = "  ", accessable = true };
                                Map.levelMap[key - i] = new GameObject() { Posx = x, Posy = y - i, Content = "  ", accessable = true };
                            }                            
                            //Map.gameObject[key].Posy -= 1;
                            for (int j = i; j > 1; --j)
                            {
                                GameObject temp = new GameObject();
                                SendGameObject(temp, Map.gameObject[key - (j - 2)]);
                                temp.Posy -= 1;
                                Map.gameObject[key - (j - 1)] = temp;
                            }
                            Map.gameObject[key] = Map.levelMap[key];
                            AutoMove();
                            JudegeLogic();
                            moveStepNum++;
                            SaveMoveStepMap(moveStepNum, Map.gameObject);
                            return;
                        }
                        if (Map.gameObject[key - i].pushable == true)
                        {
                            continue;
                        }
                        if (Map.gameObject[key -i].accessable == true)
                        {
                            Console.Beep(60, 200);
                            //Console.Write("Debugaaaaaaaaaaaaaaaaa");
                            for (int j = i; j > 0; --j)
                            {
                                //Console.Write("Debug");
                                GameObject temp = new GameObject();
                                SendGameObject(temp, Map.gameObject[key - (j - 1)]);
                                temp.Posy -= 1;
                                //Map.gameObject[key - (j - 1)].Posy -= 1;
                                Map.gameObject[key - j] = temp;
                            }
                            Map.gameObject[key] = Map.levelMap[key];
                            AutoMove();
                            JudegeLogic();
                            moveStepNum++;
                            SaveMoveStepMap(moveStepNum, Map.gameObject);
                            //JudegeLogic();
                            break;
                        }
                        if (Map.gameObject[key - i].accessable == false)
                        {
                            break;
                        }
                    }
                }

            }
            if (keyInfo.Key == ConsoleKey.RightArrow)
            {
                //Console.Write(key);
                if (Map.gameObject[key + 1].ispass == true)
                {
                    Console.Beep(600, 80);
                    Console.Beep(400, 80);
                    pass = true;
                }
                if (Map.gameObject[key + 1].isdie == true)
                {
                    Console.Beep(90, 100);
                    Console.Beep(240, 400);
                    Map.gameObject[key] = Map.levelMap[key];
                    moveStepNum++;
                    SaveMoveStepMap(moveStepNum, Map.gameObject);
                    return;
                }
                if (Map.gameObject[key + 1].accessable == true)
                {
                    Console.Beep(90, 100);
                    //Console.Write(key);
                    //Map.gameObject[key].Posy += 1;
                    GameObject temp = new GameObject();
                    SendGameObject(temp , Map.gameObject[key]);
                    temp.Posy += 1;
                    Map.gameObject[key + 1] = temp;
                    Map.gameObject[key] =Map.levelMap[key];
                    AutoMove();
                    JudegeLogic();
                    moveStepNum++;
                    SaveMoveStepMap(moveStepNum, Map.gameObject);
                }
                if(Map.gameObject[key + 1].pushable == true)
                {
                    for(int i = 2; key + i < width*(x+1); ++i)
                    {
                        if (Map.gameObject[key + i].ispass == true)
                        {
                            break;
                        }
                        if (Map.gameObject[key + i].isdie == true)
                        {
                            Console.Beep(60, 200);
                            if (Map.gameObject[key + i].isfire == false)
                            {
                                Map.gameObject[key + i] = new GameObject() { Posx = x, Posy = y + i, Content = "  ", accessable = true };
                                Map.levelMap[key + i] = new GameObject() { Posx = x, Posy = y + i, Content = "  ", accessable = true };
                            }
                            //Map.gameObject[key].Posy += 1;
                            for (int j = i; j > 1; --j)
                            {
                                GameObject temp = new GameObject();
                                SendGameObject(temp, Map.gameObject[key + (j - 2)]);
                                temp.Posy += 1;
                                Map.gameObject[key + (j - 1)] = temp;
                            }
                            Map.gameObject[key] = Map.levelMap[key];
                            AutoMove();
                            JudegeLogic();
                            moveStepNum++;
                            SaveMoveStepMap(moveStepNum, Map.gameObject);
                            return;
                        }
                        if (Map.gameObject[key + i].pushable == true)
                        {
                            continue;
                        }
                        if (Map.gameObject[key + i].accessable == true)
                        {
                            Console.Beep(60, 200);
                            //Console.Write("Debug");
                            for (int j = i; j > 0; --j)
                            {
                                GameObject temp = new GameObject();
                                //Map.gameObject[key+j-1].Posy += 1;
                                SendGameObject(temp, Map.gameObject[key + j - 1]);
                                temp.Posy += 1;
                                Map.gameObject[key + j] = temp;
                            }
                            Map.gameObject[key] = Map.levelMap[key];
                            AutoMove();
                            JudegeLogic();
                            moveStepNum++;
                            SaveMoveStepMap(moveStepNum, Map.gameObject);
                            break;
                        }
                        if (Map.gameObject[key + i].accessable == false)
                        {
                            break;
                        }
                    }
                }
            }

            if (keyInfo.Key == ConsoleKey.R)
            {
                Console.Clear();
                ResetMap();
                ReadMap(currentLevelNum);
                moveStepNum = 0;
                SaveMoveStepMap(moveStepNum, Map.gameObject);
            }
            if (keyInfo.Key == ConsoleKey.Z)
            {
                getKeyZ = true;
                if (moveStepNum > 0)
                {
                    moveStepNum--;
                    Map.gameObject = new Dictionary<int, GameObject>(moveStepMap[moveStepNum]);//传值
                    //foreach (var k in moveStepMap[moveStepNum].Keys)
                    //{
                    //    Map.gameObject[k] = moveStepMap[moveStepNum][k];
                    //}
                    //Map.gameObject = moveStepMap[moveStepNum];
                }
                if (moveStepNum == 0)
                {
                    Console.Clear();
                    ResetMap();
                    ReadMap(currentLevelNum);
                    moveStepNum = 0;
                    SaveMoveStepMap(moveStepNum, Map.gameObject);
                }
            }
            if (keyInfo.Key == ConsoleKey.P)
            {
                pass = true;
            }
            if (keyInfo.Key == ConsoleKey.Backspace)
            {
                Console.Clear();
                ResetMap();
                ReadMap(--currentLevelNum);
                moveStepNum = 0;
                SaveMoveStepMap(moveStepNum, Map.gameObject);
            }
        }//输入模块
        public static bool changeDirection = false;//判断自动人物是否转变方向
        public static void AutoMove()
        {

            List<int> keys = new List<int>();
            foreach (int key in Map.gameObject.Keys)
            {
                if (Map.gameObject[key].ismove == true) { keys.Add(key); }             
            }
            foreach (var k in keys)
            {
                if (Map.gameObject[k].direction == 1)//左
                {
                    if ((Map.gameObject[k - 1].accessable == true) && (changeDirection == false))
                    {
                        if (Map.gameObject[k - 1].controlable == true) { break; }
                        GameObject temp = new GameObject();
                        SendGameObject(temp, Map.gameObject[k]);
                        temp.Posy -= 1;
                        Map.gameObject[k - 1] = temp;
                        Map.gameObject[k] = Map.levelMap[k];
                        continue;
                    }
                    if ((Map.gameObject[k - 1].accessable == false) && (changeDirection == false))
                    {
                        changeDirection = true;
                        JudgeRight(k);
                        if (changeDirection)
                        {
                            GameObject temp = new GameObject();
                            SendGameObject(temp, Map.gameObject[k]);
                            temp.Posy += 1;
                            Map.gameObject[k + 1] = temp;
                            Map.gameObject[k] = Map.levelMap[k];  
                        }
                        continue;
                    }
                    if ((Map.gameObject[k + 1].accessable == true) && (changeDirection == true))
                    {
                        if (Map.gameObject[k - 1].controlable == true) { break; }
                        GameObject temp = new GameObject();
                        SendGameObject(temp, Map.gameObject[k]);
                        temp.Posy += 1;
                        Map.gameObject[k +1] = temp;
                        Map.gameObject[k] = Map.levelMap[k];
                        continue;
                    }
                    if ((Map.gameObject[k + 1].accessable == false) && (changeDirection == true))
                    {
                        changeDirection = false;
                        JudgeLeft(k);
                        if (!changeDirection)
                        {
                            GameObject temp = new GameObject();
                            SendGameObject(temp, Map.gameObject[k]);
                            temp.Posy -= 1;
                            Map.gameObject[k - 1] = temp;
                            Map.gameObject[k] = Map.levelMap[k];      
                        }
                        continue;
                    }
                }
            }
            void JudgeRight(int k)
            {
                if (Map.gameObject[k - 1].pushable == true)
                {
                    for (int i = 2; k - i  > k/width*width; ++i)
                    {
                        if (Map.gameObject[k - i].isdie == true)
                        {
                            if (Map.gameObject[k - i ].isfire == false)
                            {
                                Map.gameObject[k - i ] = new GameObject() { Posx = k / width , Posy = k % width- i, Content = "  ", accessable = true };
                                Map.levelMap[k - i ] = new GameObject() { Posx = k / width , Posy = k % width- i, Content = "  ", accessable = true };
                            }
                            for (int j = i; j > 1; --j)
                            {
                                GameObject temp = new GameObject();
                                SendGameObject(temp, Map.gameObject[k - (j - 2)]);
                                temp.Posy -= 1;
                                Map.gameObject[k - (j - 1) ] = temp;
                            }
                            Map.gameObject[k] = Map.levelMap[k];
                            changeDirection = false;
                            return;
                        }
                        if (Map.gameObject[k - i ].pushable == true)
                        {
                            continue;
                        }
                        if (Map.gameObject[k - i ].accessable == true)
                        {
                            for (int j = i; j > 0; --j)
                            {
                                GameObject temp = new GameObject();
                                SendGameObject(temp, Map.gameObject[k - (j - 1) ]);
                                temp.Posy -= 1;
                                Map.gameObject[k - j ] = temp;
                            }
                            Map.gameObject[k] = Map.levelMap[k];
                            changeDirection = false;
                            return;

                        }
                        if (Map.gameObject[k - i ].pushable == false|| Map.gameObject[k - i].accessable == false)
                        {
                            changeDirection = true;
                            return;
                        }
                    }
                }
            }
            void JudgeLeft(int k)
            {
                if (Map.gameObject[k + 1].pushable == true)
                {
                    for (int i = 2; k + i  < (k/width+1)*width; ++i)
                    {
                        if (Map.gameObject[k + i].isdie == true)
                        {
                            if (Map.gameObject[k + i ].isfire == false)
                            {
                                Map.gameObject[k + i ] = new GameObject() { Posx = k / width , Posy = k % width+ i, Content = "  ", accessable = true };
                                Map.levelMap[k + i ] = new GameObject() { Posx = k / width , Posy = k % width+ i, Content = "  ", accessable = true };
                            }
                            for (int j = i; j > 1; --j)
                            {
                                GameObject temp = new GameObject();
                                SendGameObject(temp, Map.gameObject[k + (j - 2) ]);
                                temp.Posy += 1;
                                Map.gameObject[k + (j - 1) ] = temp;
                            }
                            Map.gameObject[k] = Map.levelMap[k];
                            changeDirection = true;
                        }
                        if (Map.gameObject[k + i].pushable == true)
                        {
                            continue;
                        }
                        if (Map.gameObject[k + i ].accessable == true)
                        {
                            for (int j = i; j > 0; --j)
                            {
                                GameObject temp = new GameObject();
                                SendGameObject(temp, Map.gameObject[k + (j - 1)]);
                                temp.Posy += 1;
                                Map.gameObject[k + j ] = temp;
                            }
                            Map.gameObject[k] = Map.levelMap[k];
                            changeDirection = true;

                        }
                        if (Map.gameObject[k + i ].pushable == false || Map.gameObject[k + i ].accessable == false)
                        {
                            changeDirection = false;
                        }
                    }
                }
                return;
            }
        }//AI移动
        public static void MultiControl(List<int>keylist, List<int> xlist, List<int> ylist)
        {           
            List<int> allX = new List<int>();
            List<int> allY = new List<int>();
            List<int> rowKey = new List<int>();//行： x相等 可能有间隔
            Dictionary<int, List<int>> rowqueue = new Dictionary<int, List<int>>();//行队列 ：以队首为key(此队列最小key) 没有间隔 同一行可能多个队列
            List<int> colKey = new List<int>();//列： y相等 可能有间隔
            Dictionary<int, List<int>> colqueue = new Dictionary<int, List<int>>();
            foreach (int x in xlist)
            {
                if (allX.Contains(x)) { continue; }
                allX.Add(x);
            }
            foreach (int y in ylist)
            {
                if (allY.Contains(y)) { continue; }
                allY.Add(y);
            }
            for (int i = 0; i < allX.Count; ++i)
            {
                for (int j = 0; j < keylist.Count; ++j)
                {
                    if ((keylist[j] / width) == allX[i]) { rowKey.Add(keylist[j]); }
                }
                rowKey.Sort();//这一行key升序排列
                List<int> temp = new List<int>(rowKey);
                int indexmin = 0;
                int indexmax = 0;
                for (int a = 0; a < temp.Count; ++a)
                {
                    if (temp[temp.Count - 1] - temp[0] == temp.Count - 1)//说明之间没有间隔
                    {
                        rowqueue.Add(temp[0], temp);
                        break;
                    }
                    if (a + 1 <= temp.Count - 1)
                    {
                        if (temp[temp.Count - 1] - temp[indexmin] != temp.Count - 1 - indexmin)
                        {
                            if (temp[a + 1] - temp[a] > 1)
                            {
                                indexmax = a;
                                List<int> temp_ = new List<int>();
                                for (int b = indexmin; b < indexmax + 1; ++b)
                                {
                                    temp_.Add(temp[b]);
                                    temp_.Sort();
                                }
                                rowqueue.Add(temp[indexmin], temp_);
                                indexmin = a + 1;
                            }
                        }

                        if (temp[temp.Count - 1] - temp[indexmin] == temp.Count - 1 - indexmin)
                        {
                            indexmax = temp.Count - 1;
                            List<int> temp_ = new List<int>();
                            for (int b = indexmin; b < indexmax + 1; ++b)
                            {
                                temp_.Add(temp[b]);
                                temp_.Sort();
                            }
                            rowqueue.Add(temp[indexmin], temp_);
                            break;
                        }
                    }
                }
                rowKey.Clear();//清除并查下一列
            }
            for (int i = 0; i < allY.Count; ++i)
            {
                for (int j = 0; j < keylist.Count; ++j)
                {
                    if ((keylist[j] % width) == allY[i]) { colKey.Add(keylist[j]); }
                }
                colKey.Sort();//这一列key升序排列
                List<int> temp = new List<int>(colKey);//colKey每次会被清空 所以不能直接传引用
                int indexmin = 0;
                int indexmax = 0;
                for (int a=0;a<temp.Count; ++a)
                {
                    if(temp[temp.Count - 1]-temp[0]== (temp.Count - 1 ) * width)//说明之间没有间隔
                    {
                        colqueue.Add(temp[0], temp);
                        break;
                    }
                    if (a + 1 <= temp.Count - 1)
                    {
                        if (temp[temp.Count - 1] - temp[indexmin] != (temp.Count - 1 - indexmin) * width)
                        {
                            if (temp[a + 1] - temp[a] > width)//分割列
                            {
                                indexmax = a;
                                List<int> temp_ = new List<int>();
                                for (int b = indexmin; b < indexmax + 1; ++b)
                                {
                                    temp_.Add(temp[b]);
                                    temp_.Sort();
                                }
                                colqueue.Add(temp[indexmin], temp_);
                                indexmin = a + 1;
                            }
                        }
                        if (temp[temp.Count - 1] - temp[indexmin] == (temp.Count - 1 - indexmin) * width)
                        {
                            indexmax = temp.Count - 1;
                            List<int> temp_ = new List<int>();
                            for (int b = indexmin; b < indexmax + 1; ++b)
                            {
                                temp_.Add(temp[b]);
                                temp_.Sort();
                            }
                            colqueue.Add(temp[indexmin], temp_);
                            break;
                        }
                    }
                }
                colKey.Clear();//清除并查下一列
            }

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                foreach(int k in colqueue.Keys)
                {
                    int keyMin = colqueue[k][0];
                    int keyMax = colqueue[k][colqueue[k].Count-1];
                    if (Map.gameObject[keyMin - width].ispass == true)
                    {
                        Console.Beep(600, 80);
                        Console.Beep(400, 80);
                        pass = true;
                        break;
                    }
                    if (Map.gameObject[keyMin- width].isdie == true)
                    {
                        Map.gameObject[keyMax]= Map.levelMap[keyMax];
                        continue;
                    }
                    if (Map.gameObject[keyMin - width].controlable == true)//遇到的是前面队列的尾部，且前队被压缩为1个时才会撞到，否则等距移动。
                    {
                        continue;                                          //控制体默认是accessable,按穿过处理即可
                    }
                    if (Map.gameObject[keyMin - width].accessable == false&& Map.gameObject[keyMin - width].pushable == false)//挡住时压缩直到1个
                    {
                        
                        if (colqueue[k].Count > 1)
                        {
                            Map.gameObject[keyMax] = Map.levelMap[keyMax];
                        }
                        continue;
                    }
                    if (Map.gameObject[keyMin - width].accessable == true)
                    {
                        GameObject temp = new GameObject();
                        SendGameObject(temp, Map.gameObject[keyMin]);
                        temp.Posx -= 1;
                        Map.gameObject[keyMin - width] = temp;
                        Map.gameObject[keyMax] = Map.levelMap[keyMax];
                        //Map.gameObject[keyMax] = new GameObject() { Posx = keyMax/ width , Posy = keyMax % width, Content = "  ", accessable = true };
                        continue;
                    }
                    if(Map.gameObject[keyMin - width].pushable == true)
                    {
                        for (int i = 2; keyMin - i * width > 0; ++i)
                        {
                            if (Map.gameObject[keyMin - i * width].ispass == true)
                            {
                                if (colqueue[k].Count > 1)
                                {
                                    Map.gameObject[keyMax] = Map.levelMap[keyMax];
                                }
                                break;
                            }
                            if(Map.gameObject[keyMin - i * width].isdie == true)
                            {
                                if (Map.gameObject[keyMin - i * width].isfire == false)
                                {
                                    Map.gameObject[keyMin - i * width] = new GameObject() { Posx = keyMin/width - i, Posy = keyMin%width, Content = "  ", accessable = true };
                                    Map.levelMap[keyMin - i * width] = new GameObject() { Posx = keyMin / width - i, Posy = keyMin % width, Content = "  ", accessable = true };
                                }
                                if (i > 2)
                                {
                                    for (int j = i; j > 2; --j)
                                    {
                                        GameObject temp = new GameObject();
                                        SendGameObject(temp, Map.gameObject[keyMin - (j - 2) * width]);
                                        temp.Posx -= 1;
                                        Map.gameObject[keyMin - (j - 1) * width] = temp;
                                    }
                                }
                                GameObject temp_ = new GameObject();
                                SendGameObject(temp_, Map.gameObject[keyMin]);
                                temp_.Posx -= 1;
                                Map.gameObject[keyMin - width] = temp_;
                                Map.gameObject[keyMax] = Map.levelMap[keyMax];
                                break;
                            }
                            if(Map.gameObject[keyMin - i * width].pushable == true) { continue; }
                            if(Map.gameObject[keyMin - i * width].accessable == true)
                            {
                                for (int j = i; j > 1; --j)
                                {
                                    GameObject temp = new GameObject();
                                    SendGameObject(temp, Map.gameObject[keyMin - (j - 1) * width]);
                                    temp.Posx -= 1;                                  
                                    Map.gameObject[keyMin - j * width] = temp;
                                }
                                GameObject temp_ = new GameObject();
                                SendGameObject(temp_, Map.gameObject[keyMin]);
                                temp_.Posx -= 1;            
                                Map.gameObject[keyMin - width] = temp_;
                                Map.gameObject[keyMax] = Map.levelMap[keyMax];
                                break;
                            }
                            if((Map.gameObject[keyMin - i * width].pushable == false)|| (Map.gameObject[keyMin - i * width].accessable == false))
                            {
                                if (colqueue[k].Count > 1)
                                {
                                    Map.gameObject[keyMax] = Map.levelMap[keyMax];
                                }
                                break;
                            }
                        }
                    }
                }
                Console.Beep(90, 100);
                JudegeLogic();
                moveStepNum++;
                SaveMoveStepMap(moveStepNum, Map.gameObject);
            }
            if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                foreach (int k in colqueue.Keys)
                {
                    int keyMin = colqueue[k][0];
                    int keyMax = colqueue[k][colqueue[k].Count - 1];
                    if (Map.gameObject[keyMax + width].ispass == true)
                    {
                        Console.Beep(600, 80);
                        Console.Beep(400, 80);
                        pass = true;
                        break;
                    }
                    if (Map.gameObject[keyMax + width].isdie == true)
                    {
                        Map.gameObject[keyMin] = Map.levelMap[keyMin];
                        continue;
                    }
                    if (Map.gameObject[keyMax + width].controlable == true)//遇到的是前面队列的尾部，且前队被压缩为1个时才会撞到，否则等距移动。
                    {
                        continue;                                          //控制体默认是accessable,按穿过处理即可
                    }
                    if (Map.gameObject[keyMax + width].accessable == false && Map.gameObject[keyMax + width].pushable == false)//挡住时压缩直到1个
                    {

                        if (colqueue[k].Count > 1)
                        {
                            Map.gameObject[keyMin] = Map.levelMap[keyMin];
                        }
                        continue;
                    }
                    if (Map.gameObject[keyMax + width].accessable == true)
                    {
                        GameObject temp = new GameObject();
                        SendGameObject(temp, Map.gameObject[keyMax]);
                        temp.Posx += 1;
                        Map.gameObject[keyMax + width] = temp;
                        Map.gameObject[keyMin] = Map.levelMap[keyMin];
                        //Map.gameObject[keyMax] = new GameObject() { Posx = keyMax/ width , Posy = keyMax % width, Content = "  ", accessable = true };
                        continue;
                    }
                    if (Map.gameObject[keyMax + width].pushable == true)
                    {
                        for (int i = 2; keyMax + i * width <(height-1)*width; ++i)
                        {
                            if (Map.gameObject[keyMax + i * width].ispass == true)
                            {
                                if (colqueue[k].Count > 1)
                                {
                                    Map.gameObject[keyMin] = Map.levelMap[keyMin];
                                }
                                break;
                            }
                            if (Map.gameObject[keyMax + i * width].isdie == true)
                            {
                                if (Map.gameObject[keyMax + i * width].isfire == false)
                                {
                                    Map.gameObject[keyMax + i * width] = new GameObject() { Posx = keyMax / width + i, Posy = keyMax % width, Content = "  ", accessable = true };
                                    Map.levelMap[keyMax + i * width] = new GameObject() { Posx = keyMax / width + i, Posy = keyMax % width, Content = "  ", accessable = true };
                                }
                                if (i > 2)
                                {
                                    for (int j = i; j > 2; --j)
                                    {
                                        GameObject temp = new GameObject();
                                        SendGameObject(temp, Map.gameObject[keyMax + (j - 2) * width]);
                                        temp.Posx += 1;
                                        Map.gameObject[keyMax + (j - 1) * width] = temp;
                                    }
                                }
                                GameObject temp_ = new GameObject();
                                SendGameObject(temp_, Map.gameObject[keyMax]);
                                temp_.Posx += 1;
                                Map.gameObject[keyMax + width] = temp_;
                                Map.gameObject[keyMin] = Map.levelMap[keyMin];
                                break;
                            }
                            if (Map.gameObject[keyMax + i * width].pushable == true) { continue; }
                            if (Map.gameObject[keyMax + i * width].accessable == true)
                            {
                                for (int j = i; j > 1; --j)
                                {
                                    GameObject temp = new GameObject();
                                    SendGameObject(temp, Map.gameObject[keyMax + (j - 1) * width]);
                                    temp.Posx += 1;
                                    Map.gameObject[keyMax + j * width] = temp;
                                }
                                GameObject temp_ = new GameObject();
                                SendGameObject(temp_, Map.gameObject[keyMax]);
                                temp_.Posx += 1;
                                Map.gameObject[keyMax + width] = temp_;
                                Map.gameObject[keyMin] = Map.levelMap[keyMin];
                                break;
                            }
                            if ((Map.gameObject[keyMax + i * width].pushable == false) || (Map.gameObject[keyMax + i * width].accessable == false))
                            {
                                if (colqueue[k].Count > 1)
                                {
                                    Map.gameObject[keyMin] = Map.levelMap[keyMin];
                                }
                                break;
                            }
                        }
                    }
                }
                Console.Beep(90, 100);
                JudegeLogic();
                moveStepNum++;
                SaveMoveStepMap(moveStepNum, Map.gameObject);
            }
            if (keyInfo.Key == ConsoleKey.LeftArrow)
            {
                foreach (int k in rowqueue.Keys)
                {
                    int keyMin = rowqueue[k][0];
                    int keyMax = rowqueue[k][rowqueue[k].Count - 1];
                    if (Map.gameObject[keyMin - 1].ispass == true)
                    {
                        Console.Beep(600, 80);
                        Console.Beep(400, 80);
                        pass = true;
                        break;
                    }
                    if (Map.gameObject[keyMin - 1].isdie == true)
                    {
                        Map.gameObject[keyMax] = Map.levelMap[keyMax];
                        continue;
                    }
                    if (Map.gameObject[keyMin - 1].controlable == true)//遇到的是前面队列的尾部，且前队被压缩为1个时才会撞到，否则等距移动。
                    {
                        continue;                                          //控制体默认是accessable,按穿过处理即可
                    }
                    if (Map.gameObject[keyMin - 1].accessable == false && Map.gameObject[keyMin - 1].pushable == false)//挡住时压缩直到1个
                    {

                        if (rowqueue[k].Count > 1)
                        {
                            Map.gameObject[keyMax] = Map.levelMap[keyMax];
                        }
                        continue;
                    }
                    if (Map.gameObject[keyMin - 1].accessable == true)
                    {
                        GameObject temp = new GameObject();
                        SendGameObject(temp, Map.gameObject[keyMin]);
                        temp.Posy -= 1;
                        Map.gameObject[keyMin - 1] = temp;
                        Map.gameObject[keyMax] = Map.levelMap[keyMax];
                        //Map.gameObject[keyMax] = new GameObject() { Posx = keyMax/ width , Posy = keyMax % width, Content = "  ", accessable = true };
                        continue;
                    }
                    if (Map.gameObject[keyMin - 1].pushable == true)
                    {
                        for (int i = 2; keyMin - i  >width*(keyMin/width) ; ++i)
                        {
                            if (Map.gameObject[keyMin - i ].ispass == true)
                            {
                                if (rowqueue[k].Count > 1)
                                {
                                    Map.gameObject[keyMax] = Map.levelMap[keyMax];
                                }
                                break;
                            }
                            if (Map.gameObject[keyMin - i ].isdie == true)
                            {
                                if (Map.gameObject[keyMin - i ].isfire == false)
                                {
                                    Map.gameObject[keyMin - i ] = new GameObject() { Posx = keyMin / width , Posy = keyMin % width-i, Content = "  ", accessable = true };
                                    Map.levelMap[keyMin - i ] = new GameObject() { Posx = keyMin / width , Posy = keyMin % width-i, Content = "  ", accessable = true };
                                }
                                if (i > 2)
                                {
                                    for (int j = i; j > 2; --j)
                                    {
                                        GameObject temp = new GameObject();
                                        SendGameObject(temp, Map.gameObject[keyMin - (j - 2)]);
                                        temp.Posy -= 1;
                                        Map.gameObject[keyMin - (j - 1)] = temp;
                                    }
                                }
                                GameObject temp_ = new GameObject();
                                SendGameObject(temp_, Map.gameObject[keyMin]);
                                temp_.Posy -= 1;
                                Map.gameObject[keyMin - 1] = temp_;
                                Map.gameObject[keyMax] = Map.levelMap[keyMax];
                                break;
                            }
                            if (Map.gameObject[keyMin - i ].pushable == true) { continue; }
                            if (Map.gameObject[keyMin - i ].accessable == true)
                            {
                                for (int j = i; j > 1; --j)
                                {
                                    GameObject temp = new GameObject();
                                    SendGameObject(temp, Map.gameObject[keyMin - (j - 1)]);
                                    temp.Posy -= 1;
                                    Map.gameObject[keyMin - j ] = temp;
                                }
                                GameObject temp_ = new GameObject();
                                SendGameObject(temp_, Map.gameObject[keyMin]);
                                temp_.Posy -= 1;
                                Map.gameObject[keyMin - 1] = temp_;
                                Map.gameObject[keyMax] = Map.levelMap[keyMax];
                                break;
                            }
                            if ((Map.gameObject[keyMin - i].pushable == false) || (Map.gameObject[keyMin - i].accessable == false))
                            {
                                if (rowqueue[k].Count > 1)
                                {
                                    Map.gameObject[keyMax] = Map.levelMap[keyMax];
                                }
                                break;
                            }
                        }
                    }
                }
                Console.Beep(90, 100);
                JudegeLogic();
                moveStepNum++;
                SaveMoveStepMap(moveStepNum, Map.gameObject);
            }
            if (keyInfo.Key == ConsoleKey.RightArrow)
            {
                foreach (int k in rowqueue.Keys)
                {
                    int keyMin = rowqueue[k][0];
                    int keyMax = rowqueue[k][rowqueue[k].Count - 1];
                    if (Map.gameObject[keyMax + 1].ispass == true)
                    {
                        Console.Beep(600, 80);
                        Console.Beep(400, 80);
                        pass = true;
                        break;
                    }
                    if (Map.gameObject[keyMax + 1].isdie == true)
                    {
                        Map.gameObject[keyMin] = Map.levelMap[keyMin];
                        continue;
                    }
                    if (Map.gameObject[keyMax + 1].controlable == true)//遇到的是前面队列的尾部，且前队被压缩为1个时才会撞到，否则等距移动。
                    {
                        continue;                                          //控制体默认是accessable,按穿过处理即可
                    }
                    if (Map.gameObject[keyMax + 1].accessable == false && Map.gameObject[keyMax + 1].pushable == false)//挡住时压缩直到1个
                    {

                        if (rowqueue[k].Count > 1)
                        {
                            Map.gameObject[keyMin] = Map.levelMap[keyMin];
                        }
                        continue;
                    }
                    if (Map.gameObject[keyMax + 1].accessable == true)
                    {
                        GameObject temp = new GameObject();
                        SendGameObject(temp, Map.gameObject[keyMax]);
                        temp.Posy += 1;
                        Map.gameObject[keyMax + 1] = temp;
                        Map.gameObject[keyMin] = Map.levelMap[keyMin];
                        //Map.gameObject[keyMax] = new GameObject() { Posx = keyMax/ width , Posy = keyMax % width, Content = "  ", accessable = true };
                        continue;
                    }
                    if (Map.gameObject[keyMax + 1].pushable == true)
                    {
                        for (int i = 2; keyMax + i < width * ((keyMax/width)+1); ++i)
                        {
                            if (Map.gameObject[keyMax + i].ispass == true)
                            {
                                if (rowqueue[k].Count > 1)
                                {
                                    Map.gameObject[keyMin] = Map.levelMap[keyMin];
                                }
                                break;
                            }
                            if (Map.gameObject[keyMax + i].isdie == true)
                            {
                                if (Map.gameObject[keyMax + i].isfire == false)
                                {
                                    Map.gameObject[keyMax + i] = new GameObject() { Posx = keyMax / width, Posy = keyMax % width + i, Content = "  ", accessable = true };
                                    Map.levelMap[keyMax + i] = new GameObject() { Posx = keyMax / width, Posy = keyMax % width + i, Content = "  ", accessable = true };
                                }
                                if (i > 2)
                                {
                                    for (int j = i; j > 2; --j)
                                    {
                                        GameObject temp = new GameObject();
                                        SendGameObject(temp, Map.gameObject[keyMax + (j - 2)]);
                                        temp.Posy += 1;
                                        Map.gameObject[keyMax + (j - 1)] = temp;
                                    }
                                }
                                GameObject temp_ = new GameObject();
                                SendGameObject(temp_, Map.gameObject[keyMax]);
                                temp_.Posy += 1;
                                Map.gameObject[keyMax + 1] = temp_;
                                Map.gameObject[keyMin] = Map.levelMap[keyMin];
                                break;
                            }
                            if (Map.gameObject[keyMax + i].pushable == true) { continue; }
                            if (Map.gameObject[keyMax + i].accessable == true)
                            {
                                for (int j = i; j > 1; --j)
                                {
                                    GameObject temp = new GameObject();
                                    SendGameObject(temp, Map.gameObject[keyMax + (j - 1)]);
                                    temp.Posy += 1;
                                    Map.gameObject[keyMax + j] = temp;
                                }
                                GameObject temp_ = new GameObject();
                                SendGameObject(temp_, Map.gameObject[keyMax]);
                                temp_.Posy += 1;
                                Map.gameObject[keyMax + 1] = temp_;
                                Map.gameObject[keyMin] = Map.levelMap[keyMin];
                                break;
                            }
                            if ((Map.gameObject[keyMax + i].pushable == false) || (Map.gameObject[keyMax + i].accessable == false))
                            {
                                if (rowqueue[k].Count > 1)
                                {
                                    Map.gameObject[keyMin] = Map.levelMap[keyMin];
                                }
                                break;
                            }
                        }
                    }
                }
                Console.Beep(90, 100);
                JudegeLogic();
                moveStepNum++;
                SaveMoveStepMap(moveStepNum, Map.gameObject);
            }
            if (keyInfo.Key == ConsoleKey.R)
            {
                Console.Clear();
                ResetMap();
                ReadMap(currentLevelNum);
                moveStepNum = 0;
                SaveMoveStepMap(moveStepNum, Map.gameObject);
            }
            if (keyInfo.Key == ConsoleKey.Z)
            {
                getKeyZ = true;
                if (moveStepNum > 0)
                {
                    moveStepNum--;
                    Map.gameObject = new Dictionary<int, GameObject>(moveStepMap[moveStepNum]);//传值
                    //foreach (var k in moveStepMap[moveStepNum].Keys)
                    //{
                    //    Map.gameObject[k] = moveStepMap[moveStepNum][k];
                    //}
                    //Map.gameObject = moveStepMap[moveStepNum];
                }
                if (moveStepNum == 0)
                {
                    Console.Clear();
                    ResetMap();
                    ReadMap(currentLevelNum);
                    moveStepNum = 0;
                    SaveMoveStepMap(moveStepNum, Map.gameObject);
                }
            }
            if (keyInfo.Key == ConsoleKey.P)
            {
                pass = true;
            }
            if (keyInfo.Key == ConsoleKey.Backspace)
            {
                Console.Clear();
                ResetMap();
                ReadMap(--currentLevelNum);
                moveStepNum = 0;
                SaveMoveStepMap(moveStepNum, Map.gameObject);
            }
        }//多人物控制

        public static void RefreshMap(Dictionary<int,GameObject> d)
        {

            //Console.Write(Map.gameObject[0].Content);
            if (getKeyZ == false)
            {
                for (int i = 0; i < height - 1; ++i)
                {
                    for (int j = 0; j < width; ++j)
                    {
                        int key = Key(i, j);
                        if (moveStepNum > 0)
                        {
                            if (!d[key].Equals(moveStepMap[moveStepNum - 1][key]))//擦除 只打印不同的物体
                            {
                                Console.SetCursorPosition(2 * j + 2, i);
                                Console.Write("\b\b");
                                Console.ForegroundColor = d[key].color;
                                Console.Write(d[key].Content);
                                Console.ResetColor();
                            }
                        }
                        else
                        {
                            //if (d[key].controlable == true)
                            //{
                            //    Console.BackgroundColor = Map.levelMap[key].color;
                            //}
                            Console.ForegroundColor = d[key].color;
                            Console.Write(d[key].Content);
                            //System.Threading.Thread.Sleep(50);
                            Console.ResetColor();
                        }
                    }
                    //Console.WriteLine();
                }
            }
            if (getKeyZ == true)
            {
                Console.Clear();
                for (int i = 0; i < height - 1; ++i)
                {
                    for (int j = 0; j < width; ++j)
                    {
                        int key = Key(i, j);
                        Console.ForegroundColor = d[key].color;
                        Console.Write(d[key].Content);
                        //System.Threading.Thread.Sleep(50);
                        Console.ResetColor();
                    }
                    //Console.WriteLine();
                }
                getKeyZ = false;
            }
                                   
            {

            }
        }//刷新画面
    }

}
