using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Push2Win
{
    class Map
    {
        public static Dictionary<int, GameObject>levelMap= new Dictionary<int, GameObject>();
        public static Dictionary<int, GameObject> gameObject = new Dictionary<int, GameObject>();
        public static Dictionary<int, Character> character = new Dictionary<int, Character>();
        public static Dictionary<int, Word> word = new Dictionary<int, Word>();
        public static Dictionary<int, Pattern> pattern = new Dictionary<int, Pattern>();
    }
    class GameObject
    {
        public char ASCII;//与Text文件内ASCII码字符对应
        public bool accessable;//可否穿过
        public bool controlable;//可否作为控制移动的物体
        public bool pushable  ;//可否推动
        public bool isdie ;//穿过是否死亡
        public bool ispass ;//穿过是否过关
        public ConsoleColor color ;//颜色

        public bool ismove;//true代表会自动移动____人物属性
        public int direction;//1表示上 2表示下 3表示左 4表示右____人物属性
        public int sex;//1代表男 2代表女_____人物属性

        public int Num;//1代表主体， 2代表is（即判断词），3代表主体属性____逻辑单词属性

        public bool isfire;//true代表致死且不可销毁____图案属性

        public string Content { get; set; }
        public int Posx { get; set; }
        public int Posy { get; set; }

        public int GetKey() { return Posx * Play.width + Posy; }
    }
    class Word: GameObject
    {

    }
    class Pattern:GameObject
    {

    }
    class Character:GameObject
    {

    }
}
