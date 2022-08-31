namespace GamerBot_Budget.Commands;

internal sealed class UpTime
{
    [Command("uptime","Shows The Uptime Of This Bot")]
    public static async Task UpTimeCommand(SocketSlashCommand command)
    {
        Embed embed = GetUptimeEmbed();
        await command.RespondAsync("wow",embed: embed);

        return;
    }
    
    private static Embed GetUptimeEmbed()
    {
        TimeSpan sinceStart = DateTime.Now - Program.StartTime;

        var embed = new EmbedBuilder();

        embed.WithColor(Color.Purple);
        embed.WithTitle("Uptime");

        embed.AddField("Time:", FormatDate(sinceStart));
        embed.AddField("Total Milliseconds:", Math.Round(sinceStart.TotalMilliseconds));
        embed.AddField("Restart Happened At:", $"{Program.StartTime.ToShortDateString()} - {Program.StartTime.ToLongTimeString()}");

        embed.WithThumbnailUrl(Program._client!.CurrentUser.GetAvatarUrl());

        return embed.Build();
    }

    private static string FormatDate(TimeSpan time) => 
        $"{(time.Hours < 10 ? ($"0{time.Hours}") : time.Hours)}:" +
        $"{(time.Minutes < 10 ? $"0{time.Minutes}" : time.Minutes)}." +
        $"{(time.Seconds < 10 ? $"0{time.Seconds}" : time.Seconds)}";
}
