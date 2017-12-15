using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo.GUIPlugin;
using Neo;
using Neo.SmartContract;
using Neo.Core;
using Neo.VM;
using System.Windows.Forms;

namespace plugin_whoami
{
    public class plugin_whoami : Neo.GUIPlugin.IPlugin
    {
        public string Name => "Who Am I";

        public string[] GetMenus()
        {
            return new string[] { "WhoAmI", "WhoAmI 增强版" };
        }
        public static IAPI api;
        public static System.Collections.Concurrent.ConcurrentQueue<Neo.VM.StackItem> notifys = new System.Collections.Concurrent.ConcurrentQueue<Neo.VM.StackItem>();
        public static System.Collections.Concurrent.ConcurrentQueue<Neo.VM.StackItem> notifyexs = new System.Collections.Concurrent.ConcurrentQueue<Neo.VM.StackItem>();

        public static string whoami_scripthash = "0x42832a25cf11d0ceee5629cb8b4daee9bac207ca";
        public static string microblog_scripthash = "0x608f129ddd415c003efd0b9fbe33b47854a6afee";
        public void Init(IAPI api)
        {
            var hash = UInt160.Parse(whoami_scripthash);
            var hashex = UInt160.Parse(microblog_scripthash);
            plugin_whoami.api = api;
            Neo.Core.Blockchain.Notify += (s, e) =>
              {
                  if (e.Notifications[0].ScriptHash == hash)
                  {
                      notifys.Enqueue(e.Notifications[0].State);
                  }
                  else if (e.Notifications[0].ScriptHash == hashex)
                  {
                      notifyexs.Enqueue(e.Notifications[0].State);
                  }
              };
        }

        public void OnMenu(string menu)
        {
            if (menu == "WhoAmI")
            {
                WhoAmi win = new WhoAmi();
                win.ShowDialog();
            }
            if (menu == "WhoAmI 增强版")
            {
                WhoAmiEx win = new WhoAmiEx();
                win.ShowDialog();
            }
        }

        public static byte[] FromHexString(string str)
        {
            byte[] data = new byte[str.Length / 2];
            for (var i = 0; i < str.Length / 2; i++)
            {
                var hex = str.Substring(i * 2, 2);
                data[i] = byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            }
            return data;
        }
        public static ApplicationEngine CallScript(byte[] script, bool bDebug = false)
        {
            ApplicationEngine engine = null;
            Fixed8 net_fee = Fixed8.FromDecimal(0.001m);
            //生成交易
            InvocationTransaction tx = new InvocationTransaction();
            {
                tx.Version = 1;
                tx.Script = script;
                tx.Attributes = new TransactionAttribute[0];
                tx.Inputs = new CoinReference[0];
                tx.Outputs = new TransactionOutput[0];
                if (bDebug)
                {
                    engine = ApplicationEngine.RunWithDebug(tx.Script, tx);
                }
                else
                {
                    engine = ApplicationEngine.Run(tx.Script, tx);
                }

                if (!engine.State.HasFlag(VMState.FAULT))
                {
                    tx.Gas = engine.GasConsumed - Fixed8.FromDecimal(10);
                    if (tx.Gas < Fixed8.Zero) tx.Gas = Fixed8.Zero;
                    tx.Gas = tx.Gas.Ceiling();
                }
                else
                {
                    MessageBox.Show("脚本错误");
                    return null;
                }
            }
            InvocationTransaction stx = null;
            Fixed8 fee = tx.Gas.Equals(Fixed8.Zero) ? net_fee : Fixed8.Zero;
            {
                stx = plugin_whoami.api.CurrentWallet.MakeTransaction(new InvocationTransaction
                {
                    Version = tx.Version,
                    Script = tx.Script,
                    Gas = tx.Gas,
                    Attributes = tx.Attributes,
                    Inputs = tx.Inputs,
                    Outputs = tx.Outputs
                }, fee: fee);
            }

            //签名发送交易
            plugin_whoami.api.SignAndShowInformation(stx);
            return engine;
        }
        public static byte[] MakeAppCallScript(string script_hash, string _pubkey, params ContractParameter[] _params)
        {
            //拼合调用脚本
            byte[] script = null;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                var pkeybytes = FromHexString(_pubkey);
                var pubkey = Neo.Cryptography.ECC.ECPoint.FromBytes(pkeybytes, Neo.Cryptography.ECC.ECCurve.Secp256r1);
                ContractParameter[] parameters = new Neo.SmartContract.ContractParameter[] {
                  new Neo.SmartContract.ContractParameter(Neo.SmartContract.ContractParameterType.PublicKey)
                    {
                        Value = pubkey
                    },
                };
                var newparams = parameters.Concat(_params).ToArray();

                var hash = Neo.UInt160.Parse(script_hash);
                Neo.VM.Helper.EmitAppCall(sb, hash, newparams);
                script = sb.ToArray();
            }
            return script;

        }

    }
}
