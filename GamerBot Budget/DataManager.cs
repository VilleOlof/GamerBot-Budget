namespace GamerBot_Budget;

internal class DataManager
{
    private readonly static DiscordSocketClient? _client = Program._client;

    public static Dictionary<ulong, User> UserData = new();
    
    public interface IData
    {
        public User GetUser(ulong ID);
        public List<User> GetUsers();

        public bool ContainsKey(ulong key);

        public void AddUser(SocketUser user);
        public void AddUser(SocketGuildUser user);

        public void AddXP(SocketUser user, double xp);
        public void RemoveXP(SocketUser user, double xp);
    }

    public class JSONData : IData
    {
        public User GetUser(ulong ID)
        {
            return UserData[ID];
        }

        public List<User> GetUsers()
        {
            List<User> result = new();
            foreach (var user in UserData.Values)
            {
                result.Add(user);
            }
            return result;
        }

        public bool ContainsKey(ulong key)
        {
            return UserData.ContainsKey(key);
        }

        public void AddUser(SocketUser user)
        {
            UserData[user.Id] = new User(user);
        }
        public void AddUser(SocketGuildUser user)
        {
            UserData[user.Id] = new User(user);
        }

        public void AddXP(SocketUser user, double xp)
        {
            UserData[user.Id].ModifyXP(xp, Operation.Add);
        }
        public void RemoveXP(SocketUser user, double xp)
        {
            UserData[user.Id].ModifyXP(xp, Operation.Sub);
        }
    }

    //Used for JSONData.
    public static async Task LoadData()
    {
        UserData = JsonConvert.DeserializeObject<Dictionary<ulong, User>>(await File.ReadAllTextAsync($"{Program.FilePath}Data/userData.json"))
            ?? throw new NullReferenceException("userData.json Doesn't Exist.");
        Console.WriteLine("Loaded userData.json into UserData");
    }
    public static async Task SaveData()
    {
        await File.WriteAllTextAsync($"{Program.FilePath}Data/userData.json", JsonConvert.SerializeObject(UserData, Formatting.Indented));
    }

    public enum Operation
    {
        Sub,
        Add,
        Set,
        Reset
    }

    public class User
    {
        public User()
        {
            DiscordID = 0U;
            Name = string.Empty;
            levelXP = 0;
            Level = 0;
        }
        public User(SocketGuildUser user)
        {
            DiscordID = user.Id;
            Name = user.Username;
            levelXP = 0;
            Level = 0;
            Console.WriteLine($"created user: {user.Username}");
        }
        public User(SocketUser user)
        {
            DiscordID = user.Id;
            Name = user.Username;
            levelXP = 0;
            Level = 0;
            Console.WriteLine($"created user: {user.Username}");
        }

        public void ModifyXP(double xp, Operation OP)
        {
            switch (OP)
            {
                case Operation.Sub:
                    levelXP -= xp;
                    break;
                case Operation.Add:
                    levelXP += xp;
                    break;
                case Operation.Set:
                    levelXP = xp;
                    break;
                case Operation.Reset:
                    levelXP = default;
                    break;
            }
            Console.WriteLine($"Modified XP On {Name}:{Enum.GetName(OP)} - {xp}");
        }

        public void SetFrame(string Frame)
        {
            CurrentFrame = Frame;
        }
        public void SetBGColor(string hexColor)
        {
            HexBackground = hexColor;
        }

        public ulong DiscordID { get; set; }
        public string Name { get; set; }
        public double levelXP { get; set; }
        public int Level { get; set; }
        public DateTime SinceLastXPGain { get; set; } = DateTime.MinValue;

        public string[] AvailableFrames = { "BackrundsFrame0" };
        public string CurrentFrame = "BackrundsFrame0";
        public string HexBackground = "#000000";
    }
    

    //Level Requirement, Role_ID
    public static Dictionary<double, ulong> XP_Roles = new();

    public static async Task LoadXP_Roles()
    {
        XP_Roles = JsonConvert.DeserializeObject<Dictionary<double, ulong>>(await File.ReadAllTextAsync($"{Program.FilePath}Data/xp_roles.json"))
            ?? throw new NullReferenceException("xp_roles.json Doesn't Exist.");
        Console.WriteLine("Loaded xp_roles.json into XP_Roles");
    }
    
    public static async Task SaveXP_RolesAsync()
    {
        await File.WriteAllTextAsync($"{Program.FilePath}xp_roles.json", JsonConvert.SerializeObject(XP_Roles, Formatting.Indented));
        Console.WriteLine("Saved XP_Roles To xp_roles.json");
    }
    
    public static void SaveXP_Roles()
    {
        File.WriteAllText($"{Program.FilePath}xp_roles.json", JsonConvert.SerializeObject(XP_Roles, Formatting.Indented));
        Console.WriteLine("Saved XP_Roles To xp_roles.json");
    }
    
    public static Task DataMain()
    {
        _client!.UserJoined += UserJoined;

        _client!.MessageReceived += MessageHandler.MessageReceived;

        return Task.CompletedTask;
    }

    private static Task UserJoined(SocketGuildUser user)
    {
        Program.Data.AddUser(user);
        Console.WriteLine($"{Program.GetUsername(user)} Joined {user.Guild.Name}({user.Guild.Id})");

        return Task.CompletedTask;
    }

    internal static readonly double XPTimeOut = 10D;
    private static readonly double levelExponent = 2;
    private static readonly double levelBaseOffset = 0;

    internal static double LevelReq(User user) => Math.Pow(user.Level + levelBaseOffset, levelExponent);
}
