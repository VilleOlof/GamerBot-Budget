namespace GamerBot_Budget;

internal sealed class BackgroundChecks
{
    public static void Run()
    {
        Console.WriteLine("Started Background Thread.");
        while (true)
        {
            CheckReminders();
            
            Thread.Sleep(5*1000); //5 seconds
        }
        
    }

    private static void CheckReminders()
    {
        //Reminders Get Lost during restart, should add in a fix/store it for loading during startup.

        if (Reminders.Count > 0)
        {
            List<Reminder> toRemove = new();
            foreach (var reminder in Reminders)
            {
                if (reminder.RemindTime <= DateTime.Now)
                {
                    toRemove.Add(reminder);
                    reminder.User.SendMessageAsync($"**GamerBot Reminder:**\n\n{reminder.Message}");
                    Console.WriteLine($"Sent A Reminder To {reminder.User.Username}");
                }
            }
            foreach (var reminder in toRemove)
            {
                Reminders.Remove(reminder);
            }
        }
    }

    public static List<Reminder> Reminders = new();

    public struct Reminder
    {
        public string Message { get; set; }
        public DateTime RemindTime { get; set; }
        public SocketUser User { get; set; }

        public Reminder(
            string _Message,
            DateTime _RemindTime,
            SocketUser _User)
        {
            Message = _Message;
            RemindTime = _RemindTime;
            User = _User;
        }
    }

}
