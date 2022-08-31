namespace GamerBot_Budget.Commands;

internal sealed class SetFrame
{
    private static Dictionary<string, string> frameToName = new();
    private static Dictionary<string, string> FrameToURL = new();

    private static readonly Dictionary<string, FrameSelect_Page> PageInstances = new();

    [Command("setframe", "Sets the frame of the user.")]
    public static async Task SetFrameCommand(SocketSlashCommand command)
    {
        var user = Program.Data.GetUser(command.User.Id);

        MessageComponent component = CreateSelectionMenu(user);

        var pageSystem = new FrameSelect_Page(0, user);
        PageInstances[command.Token] = pageSystem;

        await command.RespondAsync(embed: pageSystem.GetPage(0), components: component, ephemeral: true);

        SelectMenuDel(command);

        return;
    }
    
    public static async Task LoadFrameData()
    {
        frameToName = JsonConvert.DeserializeObject<Dictionary<string, string>>(await File.ReadAllTextAsync($@"{Program.FilePath}Data/FrameNames.json")) 
            ?? throw new NullReferenceException();
        FrameToURL = JsonConvert.DeserializeObject<Dictionary<string,string>>(await File.ReadAllTextAsync($@"{Program.FilePath}Data/FrameURLs.json"))
            ?? throw new NullReferenceException();
        Console.WriteLine("Loaded Frame JSON Data.");
    }

    private static MessageComponent CreateSelectionMenu(DataManager.User user,bool disabled = false)
    {
        var menuOptions = new List<SelectMenuOptionBuilder>();
        foreach (var frame in user.AvailableFrames)
        {
            menuOptions.Add(new SelectMenuOptionBuilder(frameToName[frame], frame, "Select This To Change Your Frame!"));
        }

        var compBuilder = new ComponentBuilder();
        compBuilder.WithSelectMenu("select_frame", menuOptions, disabled: disabled);

        return compBuilder.Build();
    }

    private static void SelectMenuDel(SocketSlashCommand command)
    {
        Program.SelectMenu["select_frame"] = delegate (SocketMessageComponent component)
        {
            var user = Program.Data.GetUser(component.User.Id);

            //not the best to de-activate it since it requires a button press but oh well
            if (DateTime.Now - command.CreatedAt > TimeSpan.FromMinutes(10))
            {
                PageInstances.Remove(command.Token);
                component.UpdateAsync(m =>
                {
                    m.Components = CreateSelectionMenu(user, true);
                });
                return Task.CompletedTask;
            }
            
            var pageSystem = PageInstances[command.Token];
            var selectedFrame = component.Data.Values.First();

            user.SetFrame(selectedFrame);

            int pageNum = Array.IndexOf(user.AvailableFrames, selectedFrame);
            if (pageNum < 0)
            {
                pageNum = 0;
            }

            component.UpdateAsync(m =>
            {
                m.Embed = pageSystem.GetPage(pageNum);
            });

            //Temporary For Saving Using JSON.
            DataManager.SaveXP_Roles();

            return Task.CompletedTask;
        };
    }

    private static Embed Get_SetFrameEmbed(DataManager.User user, int frameSelected)
    {
        var embed = new EmbedBuilder();

        embed.WithAuthor(user.Name);
        embed.WithTitle("To Select A Frame You Can See The Menu Below");
        embed.WithColor(Color.DarkGrey);

        embed.WithImageUrl(FrameToURL[user.AvailableFrames[frameSelected]]);
        embed.WithFooter($"{frameSelected+1}/{user.AvailableFrames.Length} - {frameToName[user.AvailableFrames[frameSelected]]}");

        return embed.Build();
    }

    private class FrameSelect_Page
    {
        public int Page { get; set; }
        public int MaxPages { get; set; }
        public int MinPages { get; set; } = 0;
        public DataManager.User User { get; set; }

        public FrameSelect_Page(int page, DataManager.User user)
        {
            Page = page;
            MaxPages = user.AvailableFrames.Length;
            User = user;
        }

        public Embed GetPage(int pageNumber)
        {
            var embed = Get_SetFrameEmbed(User, pageNumber);
            return embed;
        }
    }
}
