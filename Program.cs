namespace SherLock;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        const string appName = "SherLock_Instance_Mutex";

        using var mutex = new Mutex(true, appName, out var createdNew);

        if (!createdNew)
        {
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new SherLockContext());
    }
}