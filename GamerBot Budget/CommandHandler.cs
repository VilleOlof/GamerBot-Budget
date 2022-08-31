using Discord.Net;

using System.Reflection;

namespace GamerBot_Budget;

internal class CommandHandler
{
    private readonly static DiscordSocketClient? _client = Program._client;

    public static Dictionary<string, object> GetCurrentOptions(SocketSlashCommand command)
    {
        Dictionary<string, object> result = new();
        foreach (var omg in command.Data.Options)
        {
            result[omg.Name] = omg.Value;
        }
        return result;
    }

    private static SlashCommandBuilder GetCommandBuild(Command? commandInfo)
    {
        var newCommand = new SlashCommandBuilder();

        newCommand.WithName(commandInfo!.Name);
        newCommand.WithDescription(commandInfo.Description);

        newCommand.WithDMPermission(commandInfo.DMPerm);
        newCommand.DefaultMemberPermissions = commandInfo.DefaultMemberPerm;
        newCommand.WithDefaultPermission(commandInfo.WithDefaultPerm);
        newCommand.IsDMEnabled = commandInfo.DMEnabled;

        //Adds All The Options.
        foreach (var option in commandInfo.Options)
        {
            var commandOption = JsonConvert.DeserializeObject<SlashCommandOptionBuilder>(option);
            newCommand.AddOption(commandOption);
        }
        return newCommand;
    }

    public static async Task SlashCommandInit()
    {
        CommandList = GetCommands();

        foreach (var cCommand in CommandList)
        {
            var attributeData = cCommand.Value.Item2;

            //Checks If Command Is Guild Or Global.
            if (attributeData.Scope == CommandScope.Global)
            {
                //Global Command Creation.
                var newCommand = GetCommandBuild(attributeData);

                try
                {
                    await _client!.CreateGlobalApplicationCommandAsync(newCommand.Build());
                    Console.WriteLine($"Created/Updated 'Global' Application Command: {attributeData.Name}");
                }
                catch (HttpException exception)
                {
                    Console.WriteLine(exception);
                }
            }
            else if (attributeData.Scope == CommandScope.Guild)
            {
                //Guild Command Creation.
                foreach (var guildID in Program.server_Ids)
                {

                    var guild = _client!.GetGuild(guildID);

                    List<SocketApplicationCommand> guildCommands = (await guild.GetApplicationCommandsAsync()).ToList();

                    var newCommand = GetCommandBuild(attributeData);

                    //Attempt To Build It.
                    try
                    {
                        await guild.CreateApplicationCommandAsync(newCommand.Build());
                        Console.WriteLine($"Created/Updated 'Guild' Application Command: {attributeData.Name}");
                    }
                    catch (HttpException exception)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }
            else
            {
                throw new Exception("Command Scope Is Not Valid.");
            }
        }

        //Deletes Unused Commands.
        foreach (var guildID in Program.server_Ids)
        {
            var guild = _client!.GetGuild(guildID);
            var guildCommands = (await guild.GetApplicationCommandsAsync()).ToList();

            var invalidCommands = guildCommands.Where(x => !CommandList.ContainsKey(x.Name));

            foreach (var command in invalidCommands)
            {
                await command.DeleteAsync();
                Console.WriteLine($"Deleted Unused 'Guild' Command: {command.Name}.");
            }
        }

        var commands = (await _client!.GetGlobalApplicationCommandsAsync()).ToList();
        commands
            .Where(x => !CommandList.ContainsKey(x.Name))
            .ToList()
            .ForEach(async x =>
            {
                await x.DeleteAsync();
                Console.WriteLine($"Deleted Unused 'Global' Command: {x.Name}.");
            });

        return;
    }

    public static Task SlashCommandHandler(SocketSlashCommand command)
    {
        Console.WriteLine($"({command.User.Username}#{command.User.Discriminator}) Executed Command: '{command.Data.Name}'");

        if (CommandList.ContainsKey(command.Data.Name))
        {
            var methodCommand = CommandList[command.Data.Name];

            Type methodHolder = methodCommand.Item1.DeclaringType
                ?? throw new NullReferenceException();

            ConstructorInfo methodHolderConstructor = methodHolder.GetConstructor(Type.EmptyTypes)
                ?? throw new NullReferenceException();

            object methodHolderClassObject = methodHolderConstructor.Invoke(Array.Empty<object>());

            //Attempt To Invoke The Command Method.
            try
            {
                methodCommand.Item1.Invoke(methodHolderClassObject, new object[] { command });
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException != null) { Console.WriteLine(e.InnerException); }
            }
        }

        return Task.CompletedTask;
    }

    private static Dictionary<string, Tuple<MethodInfo, Command>> CommandList = null!;

    private static Dictionary<string, Tuple<MethodInfo, Command>> GetCommands()
    {
        Dictionary<string, Tuple<MethodInfo, Command>> result = new();

        MethodInfo[] methodInfos =
            Assembly.GetExecutingAssembly()
            .GetTypes()
            .SelectMany(t => t.GetMethods())
            .Where(m => m.GetCustomAttributes(typeof(Command), false).Length > 0)
            .ToArray();

        for (int i = 0; i < methodInfos.Length; i++)
        {
            var commandAttribute = (Command)methodInfos[i].GetCustomAttribute(typeof(Command))!
                ?? throw new NullReferenceException();

            result[commandAttribute.Name] = new Tuple<MethodInfo, Command>(methodInfos[i], commandAttribute);
        }
        return result;
    }
}

public enum CommandScope
{
    Global,
    Guild
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class Command : Attribute
{
    public string Name;
    public string Description;

    public bool DMPerm;
    public bool DMEnabled;

    public GuildPermission? DefaultMemberPerm;
    public bool WithDefaultPerm;

    public string[] Options;

    public CommandScope Scope;

    public Command(
        string _Name,
        string _Description,
        bool _DMPerm = true,
        GuildPermission _DefaultMemberPerm = GuildPermission.UseApplicationCommands,
        bool _WithDefaultPerm = true,
        bool _DMEnabled = true,
        string[]? _Options = null,
        CommandScope _Scope = CommandScope.Guild
        )
    {
        Name = _Name;
        Description = _Description;
        DMPerm = _DMPerm;
        DefaultMemberPerm = _DefaultMemberPerm;
        WithDefaultPerm = _WithDefaultPerm;
        DMEnabled = _DMEnabled;
        Options = _Options == null ? Array.Empty<string>() : new string[_Options.Length];
        if (_Options != null)
        {
            for (int i = 0; i < _Options.Length; i++)
            {
                Options[i] = File.ReadAllText($@"{Program.FilePath}Options\{_Options[i]}.json");
            }
        }
        Scope = _Scope;
    }
}