namespace GamerBot_Budget.Commands;

internal sealed class Lvl
{
    [Command("lvl", "Displays User Level", _Options: new string[] { "user" })]
    public static async Task LvlCommand(SocketSlashCommand command)
    {
        var options = CommandHandler.GetCurrentOptions(command);
        var user = (options.ContainsKey("user") ? (SocketUser)options["user"] : null);

        if (user == null)
        {
            var selfUserData = Program.Data.GetUser(command.User.Id);
            await RespondWithLvlImage(selfUserData, command);
            return;
        }

        if (!Program.Data.ContainsKey(user.Id)) { await command.RespondAsync("User Not Found"); return; }

        var userData = Program.Data.GetUser(user.Id);
        await RespondWithLvlImage(userData, command);

        return;
    }
    
    private static async Task RespondWithLvlImage(DataManager.User user,SocketSlashCommand command)
    {
        string tempFilePath = $@"temp-{user.Name}.png";

        await FrameHandler.MakeFrame(user);
        await command.RespondWithFileAsync(tempFilePath);

        File.Delete(tempFilePath);
    }

    private static async Task<Embed> GetLvlEmbed(SocketSlashCommand command, DataManager.User user)
    {
        string tempPath = $@"temp-{user.Name}.png";

        var embed = new EmbedBuilder();
        embed.WithColor(Color.Green);
        embed.WithTitle($"**{command.User.Username}**");
        embed.WithDescription(
$@"**Level:** {user.Level}
**Procent:** {Math.Round(user.levelXP / DataManager.LevelReq(user) * 100)}%
");
        Console.WriteLine("right before MakeFrame");
        await FrameHandler.MakeFrame(user);
        Console.WriteLine("Right after WithImageURL");
        //File.Delete(tempPath);

        return embed.Build();
    }
}
