global using Discord;
global using Discord.WebSocket;
global using Newtonsoft.Json;

namespace GamerBot_Budget;

internal class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    public static DiscordSocketClient? _client;
    public static ulong[] server_Ids = Array.Empty<ulong>();

    public static DataManager.IData Data = new DataManager.JSONData();
    public static readonly DateTime StartTime = DateTime.Now;

    public static readonly Dictionary<string, Func<SocketMessageComponent, Task>> Buttons = new();
    public static readonly Dictionary<string, Func<SocketMessageComponent, Task>> SelectMenu = new();

    public readonly static string FilePath = @"..\..\..\";

    private readonly static string token =
        Environment.GetEnvironmentVariable("DiscordTestBot",
            EnvironmentVariableTarget.User)
        ?? throw new NullReferenceException("No ENV Token Found.");

    public readonly static HttpClient http_client = new();

    public async Task MainAsync()
    {
        //Makes A New Discord Client And Sets Up The Log Event.
        _client = new DiscordSocketClient();
        _client.Log += Log;

        //Logins And Starts The Bot On Discord.
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        //Runs A  Method To Initialize Everything That Is Dependent On The Client Being Ready.
        _client.Ready += Ready_Client;

        //Triggers The SlashCommandHandlers When A Command Has Been Executed.
        _client.SlashCommandExecuted += CommandHandler.SlashCommandHandler;

        //Triggers Button Components When Clicked.
        _client.ButtonExecuted += async (SocketMessageComponent component) =>
        {
            await Buttons[component.Data.CustomId](component);
            Console.WriteLine($"{GetUsername(component)} Clicked On '{component.Data.CustomId}'");
        };

        //Triggers When A Selection Menu Options Has Been Clicked.
        _client.SelectMenuExecuted += async (SocketMessageComponent component) =>
        {
            await SelectMenu[component.Data.CustomId](component);
            Console.WriteLine($"{GetUsername(component)} Selected On '{component.Data.CustomId}'");
        };
        
        /*-------- Connected To Discord, Rest of The Bot's Code Go Below This Line --------*/

        Thread BGCheck = new(BackgroundChecks.Run) { IsBackground = true };
        BGCheck.Start();

        await Commands.SetFrame.LoadFrameData();

        await MessageHandler.LoadLinkData();

        await DataManager.LoadData();
        await DataManager.LoadXP_Roles();

        await DataManager.DataMain();

        await Task.Delay(-1);
    }

    public static string GetUsername(SocketMessageComponent component) => $"{component.User.Username}#{component.User.Discriminator}";
    public static string GetUsername(SocketSlashCommand command) => $"{command.User.Username}#{command.User.Discriminator}";
    public static string GetUsername(SocketGuildUser user) => $"{user.Username}#{user.Discriminator}";

    private static async Task Ready_Client()
    {
        //Gets All The Guild IDs.
        server_Ids = _client!.Guilds.Select(g => g.Id).ToArray();

        //Initialize The Slash Commands.
        await CommandHandler.SlashCommandInit();
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}