using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using WebSocket4Net;
using DynaJson;
using mkbot.ReActions;
namespace mkbot
{

    public class Program
    {
        static string IuserName = "";
        static WebSocket Socket;
        static HttpClient Hc = new();
        static Config Cfg;
        public static List<User> Users = new();
        static List<Func<NoteInfo, ReAction>> funcs;
        static System.Timers.Timer timer = new();
        static List<ReAction> Queue = new();
        static int WaitTimeMS = 1500;
        public static void Main()
        {
            if (!(File.Exists("config.ini")))
            {
                Console.WriteLine("Config not exist");
                return;
            }
            var conf = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddIniFile("config.ini")
                 .Build()
                 .GetSection("APP")
                 .Get<Config>();

            if (conf is null || conf.host == "" || conf.token == "")
            {
                Console.WriteLine("Config invalid");
                return;
            }
            Cfg = conf;
            LoadMemory();
            InitAndAddFunction();
            if (Check_I())
            {
                InitSoclket();
                while (true)
                {
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Escape)
                    {
                        break;
                    }
                    Console.WriteLine("\rESCで終了");
                }
                Socket.Close();
            }

        }
        static void LoadMemory()
        {
            if (File.Exists("memory.json"))
            {
                Console.WriteLine("Loading memory ...");
                var json = File.ReadAllText("memory.json");
                var dyna = JsonObject.Parse(json);
                foreach (var item in dyna)
                {
                    var user = new User(item.username,item.Host,(int)item.Love);
                    Users.Add(user);
                }
            }
        }
        static bool Check_I()
        {
            var I = new I_Rootobject()
            {
                i = Cfg.token
            }; 
            try
            {
                var Content = new StringContent(JsonSerializer.Serialize(I), Encoding.UTF8, @"application/json");
                var res = Hc.PostAsync($"https://{Cfg.host}/api/i", Content);
                var con = res.Result.Content.ReadAsStringAsync().Result;

                var ret = DynaJson.JsonObject.Parse(con);
                IuserName = ret.username;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            if (IuserName == "")
            {
                return false;
            }
            Console.WriteLine($"username:@{IuserName}@{Cfg.host}");
            return true;
        }
        static void InitSoclket()
        {
            timer.Elapsed += Socket_Reconnect;
            timer.Interval = 5000;


            Socket = new WebSocket($"wss://{Cfg.host}/streaming?i={Cfg.token}")
            {
                NoDelay = true,
                ReceiveBufferSize = 81920,
                AutoSendPingInterval = 10
            };
            Socket.MessageReceived += Socket_MessageReceived;
            Socket.Closed  += (sender, e) =>
            { 
                Console.WriteLine("Socket Closed");
                timer.Enabled  = true;
            };
            Socket.Opened += (sender, e) =>
            {
                var RO = new Connect_Rootobject()
                {
                    type = "connect",
                    body = new Body()
                    {
                        channel = "main",
                        id = new Random().Next().ToString()
                    }
                };
                Socket.Send(JsonSerializer.Serialize(RO));
                 RO.body.channel = "homeTimeline";
                 RO.body.id = new Random().Next().ToString();
                Socket.Send(JsonSerializer.Serialize(RO));
                Console.WriteLine("Socket Opened");
            };
            Socket.Open();
        }
        static void Socket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Console.WriteLine("\rDataReceived=" + DateTime.Now);
#if DEBUG
            Console.WriteLine(e.Message);
#endif
            var dyna = DynaJson.JsonObject.Parse(e.Message);
            Console.WriteLine("type:" + dyna.type);
            Console.WriteLine("bodytype:" + dyna.body.type);
            var Body =dyna.body.body;
            switch (dyna.body.type)
            {
                case "note":
                    //Renoteとかの対応
                    if(Body.renoteId != null|| Body.text is null||Body.text == "") { return;}
                    //dynamic型のためContainsするにはキャストが必要
                    var Text = (string)Body.text;
                    //メンションだったらスルー(メンション時判定があるので二重リアクション回避)
                    if (Text.Contains($"@{IuserName}@{Cfg.host}")) { return; }
                    Text= DeleteMention(Body.text, Body.user.host switch
                    { null => "", _ => Body.user.host});
                    var Arg = new NoteInfo()
                    {
                        uId = Body.userId,
                        nId = Body.id,
                        Text = Text,
                        IsNotMention = true,
                        Visibility = Body.visibility,
                        visibleUserIds = Body.userId,
                        username = Body.user.username,
                        Host = Body.user.host
                    };
                    ReAction Reac = new ();
                    foreach (var Func in funcs)
                    {
                        Reac = Func(Arg);
                        if (Reac != null) { break; }
                    }
                    if (Reac is null)
                    {
                        return;
                    }
                    Task.Run(() => Process(Reac));
                    break;

                case "notification":
                Console.WriteLine("notificationtype:" + Body.type);
                switch (Body.type)
                {
                    case "mention":
                       Text =  DeleteMention(Body.note.text, Body.note.user.host switch
                        { null => "",_ => Body.note.user.host});
                         Arg = new NoteInfo()
                        {
                            uId = Body.note.userId,
                            nId = Body.note.id,
                            Text = Text,
                            Visibility = Body.note.visibility,
                            visibleUserIds = Body.note.userId,
                            username = Body.note.user.username,
                            Host = Body.note.user.host
                        };
                         Reac = new ReAction();
                        foreach(var Func in funcs)
                        {
                            Reac = Func(Arg);
                           if (Reac  != null) { break; }
                        }
                        if (Reac is null)
                        {
                            return;
                        }
                        Task.Run(() => Process(Reac));
                        break;
                    case "reply":
                        break;
                    case "reaction":
                        break;
                }
                    break;

                case "follow":
                var user = new User(Body.username, Body.host);
                Users.Add(user);
                var json = JsonSerializer.Serialize(Users);
                File.WriteAllText("memory.json", json);
                Console.WriteLine($"followed:@{Body.username}@{Body.host}");
                break;
            }
        }
        static void Socket_Reconnect(object sender, EventArgs e)
        {
            if (Socket.State == WebSocketState.Closed)
            {
                Console.WriteLine("Socket ReOpening...");
                Socket.Open();
                if(Socket.State == WebSocketState.Open)
                {
                    timer.Enabled = false;
                }
            }
            else
            { 
                timer.Enabled = false;
            }
        }
        static string DeleteMention(string text, string Remote)
        {
            Console.WriteLine($"text:{{{text}}}");
            if(Remote == "")
            {
              return text.Replace($"@{IuserName}", "");
            }
            else
            {
              return text.Replace($"@{IuserName}@{Cfg.host}", "");
            }
        }
        static bool Process(ReAction Reac)
        {
            if(Socket.State != WebSocketState.Open) 
            {return false;}
            Task.Delay(WaitTimeMS).Wait();
            
            switch (Reac.Type) 
            {
                case ReAction.ReactionType.ReAction:
                   Post("notes/reactions/create", JsonSerializer.Serialize(new Reactions_Create()
                   {
                        i = Cfg.token,
                        noteId = Reac.nId,
                        reaction = Reac.Emoji
                    }));
                    break;
                case ReAction.ReactionType.Reply:
                   Post("notes/create", JsonSerializer.Serialize(new Notes_Create()
                    {
                        i = Cfg.token,
                        text = Reac.Text,
                        replyId = Reac.nId,
                        visibility = Reac.Visibility,
                        visibleUserIds = Reac.visibleUserIds
                    }));
                    break;
                case ReAction.ReactionType.ReplyAndReaction:
                    Post("notes/create", JsonSerializer.Serialize(new Notes_Create()
                    {
                        i = Cfg.token,
                        text = Reac.Text,
                        replyId = Reac.nId,
                        visibility = Reac.Visibility,
                       visibleUserIds = Reac.visibleUserIds
                    }));
                   Post("notes/reactions/create", JsonSerializer.Serialize(new Reactions_Create()
                    {
                        i = Cfg.token,
                        noteId = Reac.nId,
                        reaction = Reac.Emoji
                    }));
                    break;
                case ReAction.ReactionType.Registar:
                    Post(JsonSerializer.Serialize(new Following_Create()
                    {
                        i = Cfg.token,
                        userId = Reac.uId
                    }));
                    break;
                case ReAction.ReactionType.none:
                    return true;

            }
            return true;
        }
        static void Post(string EndPoint, string Json)
        {
            try
            {
                var Content = new StringContent(Json, Encoding.UTF8, @"application/json");
                var res = Hc.PostAsync($"https://{Cfg.host}/api/{EndPoint}", Content).Result;
                var Ret = res.Content.ReadAsStringAsync().Result;
                Console.WriteLine(Ret);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        static void Post(string Json)
        {
            try
            {
                var Content = new StringContent(Json, Encoding.UTF8, @"application/json");
                var res = Hc.PostAsync($"https://{Cfg.host}/api/following/create", Content);
                var json =res.Result.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        static void InitAndAddFunction() 
        {
           funcs =  new List<Func<NoteInfo,ReAction>>();
            var nadenade = new Base(new List<string>() { "なでなで","なでなでしてあげましょうね" },"💞", new List<string>() { "" }, new List<string>() { "ふふん♪" }, new List<string>() { "そ、外ではあまりなでるでない・・・///" },false,"👀",5);
            var koyaaan  = new Base(new List<string>() { "こやーん" }, "🦊", new List<string>() { "" }, new List<string>() { "こやーん" }, new List<string>() { "こやーん" }, true,"");
            var registar = new Registar(new List<string>() { "ふぉろー", "フォロー" }, "", new List<string>() { "" }, new List<string>() { "" }, new List<string>() { "" }, false);
            funcs.Add(nadenade.CheckMessage);//追加したかったらここの前後に追加する(上に行くほど優先度が高い)
            funcs.Add(koyaaan.CheckMessage);
            funcs.Add(registar.CheckMessage);
            funcs.Add(Default.CheckMessage);
        }
    }
    //メッセージが邪魔なので
#pragma warning disable CS8618 
#pragma warning disable IDE1006 
    public class Connect_Rootobject
    {
        public string type { get; set; }
        public Body body { get; set; }
    }

    public class Body
    {
        public string channel { get; set; } 
        public string id { get; set; } 
    }
    public class I_Rootobject
    {
        public string i { get; set; }
    }
    public class Config
    {
        public string host { get; set; }
        public string token { get; set; }
    }
#pragma warning restore CS8618
}