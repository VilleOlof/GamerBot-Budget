using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace GamerBot_Budget;

//This Basically Means That System.Drawing Only Works On Windows
[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
internal class FrameHandler
{
    public static async Task MakeFrame(DataManager.User user)
    {
        string imageFilePath = $@"{Program.FilePath}Frames\{user.CurrentFrame}.png";
        Bitmap bitmap = (Bitmap)System.Drawing.Image.FromFile(imageFilePath)
            ?? throw new NullReferenceException("Could not Find Image");

        using Graphics graphics = Graphics.FromImage(bitmap);

        try
        {
            graphics.Clear(ColorTranslator.FromHtml(user.HexBackground));
        }
        catch
        {
            graphics.Clear(System.Drawing.Color.Black);
        }
        
        graphics.DrawImage(System.Drawing.Image.FromFile(imageFilePath), 0, 0);

        DrawText(graphics,user);

        graphics.DrawImage(await DrawProfilePicture(user), 125, 80);

        DrawProgressBar(user,graphics);
        
        bitmap.Save($@"temp-{user.Name}.png");

        graphics.Dispose();
    }

    private static void DrawProgressBar(DataManager.User user,Graphics graphics)
    {
        Point location = new(125,450);

        SolidBrush color = new(ColorTranslator.FromHtml("#898C87"));
        Rectangle backBar = new(location, new Size(250, 40));
        graphics.FillRectangle(color, backBar);

        //Copilot code
        color = new(ColorTranslator.FromHtml("#ffffff"));
        Rectangle frontBar = new(location, new Size((int)(250 * (user.levelXP / DataManager.LevelReq(user))), 40));
        graphics.FillRectangle(color, frontBar);
    }

    private static void DrawText(Graphics graphics,DataManager.User user)
    {
        StringFormat Format = new()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        Brush color = Brushes.White;

        graphics.DrawString($"{user.Name}", GetFont(50), color, new PointF(250, 375), Format);
        graphics.DrawString($"Level: {user.Level}", GetFont(40), color, new PointF(250, 425), Format);
        graphics.DrawString($"{Math.Round(user.levelXP / DataManager.LevelReq(user) * 100)}%", GetFont(55), color, new PointF(250, 525), Format);
    }

    private static async Task<System.Drawing.Image> DrawProfilePicture(DataManager.User user)
    {
        string avatarURL = Program._client!.GetUser(user.DiscordID).GetAvatarUrl();
        Stream stream = await Program.http_client.GetStreamAsync(avatarURL);
        var bmpprofilePic = new Bitmap(stream);
        return ResizeImage(bmpprofilePic, 350, 350);
    }

    private static Font GetFont(int size)
    {
        PrivateFontCollection collection = new();
        collection.AddFontFile($@"{Program.FilePath}Data/Hard Compound.ttf");
        FontFamily fontFamily = new("Hard Compound", collection);
        return new Font(fontFamily, size, FontStyle.Bold);
    }

    //https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp
    private static Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
    {
        var destRect = new Rectangle(0, 0, width, height);
        var destImage = new Bitmap(width, height);

        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        using (var graphics = Graphics.FromImage(destImage))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using var wrapMode = new ImageAttributes();
            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
        }

        return destImage;
    }
}