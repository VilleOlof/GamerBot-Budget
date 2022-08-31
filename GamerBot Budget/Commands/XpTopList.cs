using System.Text;

namespace GamerBot_Budget.Commands;

internal sealed class XpTopList
{
    private static readonly Dictionary<string, XP_Page> PageInstances = new();

    [Command("xptoplist", "Shows A Leaderboard XP Rankings", _Options: new string[] { "page" })]
    public static async Task XPTopListCommand(SocketSlashCommand command)
    {
        var options = CommandHandler.GetCurrentOptions(command);
        int StartingPage = (options.ContainsKey("page") ? Convert.ToInt32(options["page"]) : 0);

        _ = Emoji.TryParse("➡️", out var emoji);
        _ = Emoji.TryParse("⬅️", out var emoji2);

        var compBuilder = new ComponentBuilder();
        compBuilder.WithButton(null, "xpList_BackButton", ButtonStyle.Secondary, emoji2);
        compBuilder.WithButton(null, "xpList_ForwardButton", ButtonStyle.Secondary, emoji);

        var pageSystem = new XP_Page(StartingPage);
        PageInstances[command.Token] = pageSystem;

        if (StartingPage > pageSystem.MaxPageNumber)
        {
            await command.RespondAsync("Page Number Too High, Falling Back To 0", ephemeral: true);
            StartingPage = 0;
        }

        await command.RespondAsync(embed: pageSystem.GetPage(StartingPage), components: compBuilder.Build());

        ButtonDel(command);

        return;
    }

    private static void ButtonDel(SocketSlashCommand command)
    {
        //We Add A Delegate To The Button Dictionary
        Program.Buttons["xpList_BackButton"] = delegate (SocketMessageComponent component)
        {
            var Page = PageInstances[command.Token];
            component.UpdateAsync(m => m.Embed = Page.GetPage(Page.PageNumber - 1));
            return Task.CompletedTask;
        };

        Program.Buttons["xpList_ForwardButton"] = delegate (SocketMessageComponent component)
        {
            var Page = PageInstances[command.Token];
            component.UpdateAsync(m => m.Embed = Page.GetPage(Page.PageNumber + 1));
            return Task.CompletedTask;
        };
    }

    private class XP_Page
    {
        public int PageNumber { get; set; } = 0;
        public int MaxPageNumber { get; set; } = 0;
        public int MinPageNumber { get; set; } = 0;

        private static readonly int UsersPerPage = 10;

        public XP_Page(int pageNumber)
        {
            PageNumber = pageNumber;

            MaxPageNumber = (Program.Data.GetUsers().Count / UsersPerPage);
            MinPageNumber = 0;

            LimitPage();
        }

        private void LimitPage()
        {
            if (PageNumber > MaxPageNumber)
            {
                PageNumber = MaxPageNumber;
            }
            else if (PageNumber < MinPageNumber)
            {
                PageNumber = MinPageNumber;
            }
        }

        public Embed GetPage(int pageNumber)
        {
            PageNumber = pageNumber;
            LimitPage();

            var users = Program.Data.GetUsers();
            users.Sort((x, y) => x.Level == y.Level ? x.levelXP.CompareTo(y.levelXP) : x.Level.CompareTo(y.Level));
            users.Reverse();

            var page = new StringBuilder();

            var embed = new EmbedBuilder();


            for (int i = 0; i < UsersPerPage; i++)
            {
                string fieldContent = string.Empty;
                string fieldTitle = $"**{(i + (PageNumber * UsersPerPage))}**";

                var user = new DataManager.User();
                //shitty bad solution, just wanna see that it works
                try
                {
                    user = users[i + (PageNumber * UsersPerPage)];
                }
                catch
                {
                    break;
                }

                //fieldContent += ($"{(i + (PageNumber * UsersPerPage)) + 1}: {user.Name} - {user.Level}Lvl ({user.levelXP}/{DataManager.LevelReq(user)})\n");
                fieldContent = $"User: {Program._client!.GetUser(user.DiscordID).Mention}\nLevel: `{user.Level}`";

                embed.AddField(fieldTitle, fieldContent);
            }

            embed.WithTitle("**XP Top List**");
            embed.WithColor(Color.DarkerGrey);

            return embed.Build();
        }
    }



}
