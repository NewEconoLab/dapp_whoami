using Neo;
using Neo.Core;
using Neo.SmartContract;
using Neo.VM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace plugin_whoami
{
    public partial class WhoAmi : Form
    {
        public WhoAmi()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var key = GetScriptHashFromAddress(textBox1.Text);
            label1.Text = key.ToHexString();

            var skey = new Neo.Core.StorageKey();
            skey.Key = key;
            skey.ScriptHash = UInt160.Parse(plugin_whoami.whoami_scripthash);

            var item = Neo.Core.Blockchain.Default.GetStorageItem(skey);
            this.listBox1.Items.Clear();
            if (item == null)
            {
                this.listBox1.Items.Add("<nothing>");
            }
            else
            {
                this.listBox1.Items.Add("hexstr:" + item.Value.ToHexString());
                var text = "";
                try
                {
                    text = System.Text.Encoding.UTF8.GetString(item.Value);
                }
                catch
                {

                }
                this.listBox1.Items.Add("as str:" + text);
            }
        }
        static byte[] GetScriptHashFromAddress(string add)
        {
            //后四个自己是hash
            //第一个是魔法数字
            var bts = Neo.Cryptography.Base58.Decode(add);
            return bts.Take(bts.Length - 4).Skip(1).ToArray();

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
        int index = 0;
        private void button2_Click(object sender, EventArgs e)
        {
            if (plugin_whoami.api.CurrentWallet == null)
                return;
            var count = plugin_whoami.api.CurrentWallet.GetKeys().Count();
            if (index >= count) index = 0;
            var key = plugin_whoami.api.CurrentWallet.GetKeys().ToArray()[index];
            this.textBoxPubKey.Text = key.PublicKey.EncodePoint(true).ToHexString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //拼合调用脚本
            byte[] script = null;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                var pkeybytes = FromHexString(this.textBoxPubKey.Text);
                var pubkey = Neo.Cryptography.ECC.ECPoint.FromBytes(pkeybytes, Neo.Cryptography.ECC.ECCurve.Secp256r1);
                ContractParameter[] parameters = new Neo.SmartContract.ContractParameter[] {
                  new Neo.SmartContract.ContractParameter(Neo.SmartContract.ContractParameterType.PublicKey)
                    {
                        Value = pubkey
                    },
                    new Neo.SmartContract.ContractParameter(Neo.SmartContract.ContractParameterType.String)
                    {
                        Value = this.textBoxName.Text
                    }
                };
                var hash = Neo.UInt160.Parse(plugin_whoami.whoami_scripthash);
                Neo.VM.Helper.EmitAppCall(sb, hash, parameters);
                script = sb.ToArray();

            }

            var stx = new InvocationTransaction();
            //生成交易
            InvocationTransaction tx = null;
            {
                Fixed8 fee = Fixed8.FromDecimal((decimal)0.001);
                tx = plugin_whoami.api.CurrentWallet.MakeTransaction(new InvocationTransaction
                {
                    Version = 1,
                    Script = script,
                    Gas = Fixed8.Zero,
                    Attributes = new TransactionAttribute[0],
                    Inputs = new CoinReference[0],
                    Outputs = new TransactionOutput[0]
                }, fee: fee);
            }

            //签名发送交易
            plugin_whoami.api.SignAndShowInformation(tx);
        }

        private void textBoxPubKey_TextChanged(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            StackItem item;
            if(plugin_whoami.notifys.TryDequeue(out item))
            {
                this.listBox2.Items.Add(item.GetArray()[0].GetByteArray().ToHexString());
            }
        }
    }
}
