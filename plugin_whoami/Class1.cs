using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo.GUIPlugin;
using Neo;

namespace plugin_whoami
{
    public class plugin_whoami : Neo.GUIPlugin.IPlugin
    {
        public string Name => "Who Am I";

        public string[] GetMenus()
        {
            return new string[]{"Show" };
        }
        public static IAPI api;
        public static System.Collections.Concurrent.ConcurrentQueue<Neo.VM.StackItem> notifys = new System.Collections.Concurrent.ConcurrentQueue<Neo.VM.StackItem>();
           

        public static string whoami_scripthash= "0x42832a25cf11d0ceee5629cb8b4daee9bac207ca";
        public void Init(IAPI api)
        {
            var hash =  UInt160.Parse(whoami_scripthash);
            plugin_whoami.api = api;
            Neo.Core.Blockchain.Notify += (s, e) =>
              {
                  if (e.Notifications[0].ScriptHash == hash)
                  {
                      notifys.Enqueue(e.Notifications[0].State);
                  }
              };
        }
        
        public void OnMenu(string menu)
        {
            if (menu == "Show")
            {
                WhoAmi win = new WhoAmi();
                win.ShowDialog();
            }
        }
    }
}
