using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AppScene
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainFrm frm= new MainFrm();
            
            Application.Idle+=new EventHandler(frm.AxSceneControl.OnApplicationIdle);
            Application.Run(frm);
        }
    }
}
