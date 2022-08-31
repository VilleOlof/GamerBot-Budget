using System.Text.RegularExpressions;

namespace GamerBot_Budget;

internal class MessageHandler
{
    private static List<ulong> AllowedLinkRoles = new();
    private static List<string> AllowedLinks = new();
    private static List<ulong> AllowedChannels = new();

    internal static async Task MessageReceived(SocketMessage message)
    {
        if (message.Author.IsBot) { return; }

        await CheckForLinks(message);

        //React If Message Containts 'Poll'.
        if (message.Content.ToLower().Contains("poll"))
        {
            await message.AddReactionAsync(new Emoji("👍"));
            await message.AddReactionAsync(new Emoji("👎"));
        }

        if (!Program.Data.ContainsKey(message.Author.Id))
        {
            Program.Data.AddUser(message.Author);
        }

        var user = Program.Data.GetUser(message.Author.Id);

        //XP Gain Checks.
        if (DateTime.Now > user.SinceLastXPGain.AddSeconds(DataManager.XPTimeOut))
        {
            user.SinceLastXPGain = DateTime.Now;

            Random rnd = new();
            //var XPGain = (ulong)rnd.Next(1, 10);
            double XPGain = Math.Floor(rnd.NextSingle() * 3) + 1;

            Program.Data.AddXP(message.Author, XPGain);

            //Checks for level-ups
            if (user.levelXP >= DataManager.LevelReq(user))
            {
                //Reset and increase level.
                int prevLevel = user.Level;
                user.Level++;
                user.levelXP = 0;

                Console.WriteLine($"User leveled up! {user.Name}|{prevLevel}=>{user.Level} ({user.DiscordID})");

                //User Level up DM message
                try
                {
                    await message.Author.SendMessageAsync($"Congratulations, You Leveled Up! ({prevLevel}=>{user.Level})");
                }
                catch (Exception ex) { Console.WriteLine($"Couldn't Send Level Up Message, Most Likely They Have Turned It Off: {ex}"); }

                var chnl = message.Channel as SocketGuildChannel;
                var guild = chnl.Guild;

                //Handle Role Levels
                if (DataManager.XP_Roles.ContainsKey(user.Level))
                {
                    var role = guild.GetRole(DataManager.XP_Roles[user.Level]);
                    if (role != null)
                    {
                        var guildUser = guild.GetUser(user.DiscordID);
                        await guildUser.AddRoleAsync(role);
                    }
                }
                //Delete any other XP_Role other than the one they just got.
                foreach (var item in DataManager.XP_Roles)
                {
                    if (item.Key != user.Level)
                    {
                        var role = guild.GetRole(item.Value);
                        if (role != null)
                        {
                            var guildUser = guild.GetUser(user.DiscordID);
                            await guildUser.RemoveRoleAsync(role);
                        }
                    }
                }
            }
        }
        //Shouldnt do this on every message event but since its not connected to a DB this is temporarily.
        await DataManager.SaveData();

        return;
    }

    private static async Task CheckForLinks(SocketMessage message)
    {
        if (IsUrlValid(message.Content))
        {
            if (!AllowedChannels.Contains(message.Channel.Id))
            {
                //AllowedLinkRoles doesnt work yet.

                if (AllowedLinks.Contains(message.Content))
                {
                    return;
                }
                else
                {
                    await message.DeleteAsync();
                    await message.Channel.SendMessageAsync($"{message.Author.Mention} You are not allowed to post that link.");
                    return;
                }
            }
        }
        return;
    }

    //https://stackoverflow.com/questions/5717312/regular-expression-for-url
    private static bool IsUrlValid(string url)
    {
        string pattern = @"^(http|https|ftp|)\://|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
        Regex reg = new(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        return reg.IsMatch(url);
    }

    public static async Task LoadLinkData()
    {
        string baseURL = $"{Program.FilePath}Data/";
        AllowedLinkRoles = JsonConvert.DeserializeObject<List<ulong>>(await File.ReadAllTextAsync($@"{baseURL}AllowedLinkRoles.json"))
            ?? throw new NullReferenceException();
        AllowedLinks = JsonConvert.DeserializeObject<List<string>>(await File.ReadAllTextAsync($"{baseURL}AllowedLinks.json"))
            ?? throw new NullReferenceException();
        AllowedChannels = JsonConvert.DeserializeObject<List<ulong>>(await File.ReadAllTextAsync($"{baseURL}AllowedChannels.json"))
            ?? throw new NullReferenceException();
        Console.WriteLine("Loaded Link Data");
    }
}
