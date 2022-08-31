namespace GamerBot_Budget.Commands;

internal sealed class SetColor
{
    [Command("setcolor", "Sets The Color Of The Background Of The Frame", _Options: new string[] { "color" })]
    public static async Task SetColorCommand(SocketSlashCommand command)
    {
        var options = CommandHandler.GetCurrentOptions(command);
        var colorText = (string)options["color"];
        var user = Program.Data.GetUser(command.User.Id);

        user.SetBGColor(colorText);
        await command.RespondAsync($"Your Frames Background Color Has Been Set To {colorText}", ephemeral: true);

        return;
    }
}