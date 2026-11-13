using System;
using System.Windows.Forms;

namespace MunicipalServicesApp
{
    /// <summary>
    /// Application entry point. Starts the Windows Forms message loop
    /// with the Main Menu form as the first screen shown to the user.
    /// </summary>
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainMenuForm());
        }
    }
}
