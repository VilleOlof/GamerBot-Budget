namespace GamerBot_Budget.Commands;

internal sealed class Server_Info
{
    private readonly static DiscordSocketClient? _client = Program._client;

    [Command("server_info", "Displays Server Info")]
    public static async Task ServerInfoCommand(SocketSlashCommand command)
    {
        var guild_ID = command.GuildId;
        if (guild_ID == null) { await command.RespondAsync("No Server Found"); return; }

        var guild = _client!.GetGuild((ulong)guild_ID);
        Embed embed = GetServerEmbed(guild);

        await command.RespondAsync(embed: embed);

        return;
    }

    private static Embed GetServerEmbed(SocketGuild guild)
    {
        var embed = new EmbedBuilder();
        embed.WithColor(Color.Blue);
        embed.WithTitle($"Server Info In {guild.Name}");

        embed.WithDescription(
$@"**Total Members:** {guild.MemberCount}

Online Members: {guild.Users.Count(u => u.Status == UserStatus.Online)}
Idle Members: {guild.Users.Count(u => u.Status == UserStatus.Idle)}
Dnd Members: {guild.Users.Count(u => u.Status == UserStatus.DoNotDisturb)}
Offline Members: {guild.Users.Count(u => u.Status == UserStatus.Offline)}

Bot Members: {guild.Users.Count(u => u.IsBot)}
Online Moderators: {guild.Users.Count(u => u.Status == UserStatus.Online && u.GuildPermissions.Administrator)}

Boosting Members: {guild.PremiumSubscriptionCount}
Boost Level: {guild.PremiumTier}
");
        embed.WithThumbnailUrl(guild.IconUrl);
        return embed.Build();
    }
}