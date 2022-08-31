namespace GamerBot_Budget.Commands;

internal sealed class Ping
{
    [Command("ping", "Ping Pong!")]
    public static async Task PingCommand(SocketSlashCommand command)
    {
        //Responds with three decimals of the ping.
        var time = (command.CreatedAt - DateTimeOffset.Now).TotalSeconds.ToString();
        time = (time.Length < 5 ? time : time[0..5]);

        await command.RespondAsync(embed: GetPingEmbed(time));

        return;
    }

    private static Embed GetPingEmbed(string time)
    {
        var embed = new EmbedBuilder();

        embed.WithTitle(":ping_pong: Pong!");
        embed.WithDescription($"Took {time} Seconds");
        embed.WithColor(Color.Teal);

        return embed.Build();
    }
}