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
    public partial class WhoAmiEx : Form
    {
        public WhoAmiEx()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var key = GetScriptHashFromAddress(textBox1.Text);
            label1.Text = key.ToHexString();
            var keylast = key.Concat(new byte[] { 0x02 }).ToArray();
            var keynow = key.Concat(new byte[] { 0x01 }).ToArray();
            var keyname = key.Concat(new byte[] { 0x20, 0x01 }).ToArray();
            var keypic = key.Concat(new byte[] { 0x20, 0x02 }).ToArray();
            var hash = UInt160.Parse(plugin_whoami.microblog_scripthash);

            var skeyLast = new Neo.Core.StorageKey();
            skeyLast.Key = keylast;
            skeyLast.ScriptHash = hash;
            var itemLast = Neo.Core.Blockchain.Default.GetStorageItem(skeyLast);

            var skeyNow = new Neo.Core.StorageKey();
            skeyNow.Key = keynow;
            skeyNow.ScriptHash = hash;
            var itemNow = Neo.Core.Blockchain.Default.GetStorageItem(skeyNow);

            var skeyName = new Neo.Core.StorageKey();
            skeyName.Key = keyname;
            skeyName.ScriptHash = hash;
            var itemName = Neo.Core.Blockchain.Default.GetStorageItem(skeyName);

            var skeyPic = new Neo.Core.StorageKey();
            skeyPic.Key = keypic;
            skeyPic.ScriptHash = hash;
            var itemPic = Neo.Core.Blockchain.Default.GetStorageItem(skeyPic);

            this.listBox1.Items.Clear();
            if (itemLast != null)
            {
                this.listBox1.Items.Add("lastblock:" + itemLast.Value.ToHexString());
            }
            else
            {
                this.listBox1.Items.Add("lastblock:<nothing>");

            }
            if (itemNow != null)
            {
                this.listBox1.Items.Add("nowblock:" + itemNow.Value.ToHexString());
            }
            else
            {
                this.listBox1.Items.Add("nowblock:<nothing>");

            }

            if (itemName == null)
            {
                this.listBox1.Items.Add("name:<nothing>");
            }
            else
            {
                var text = "";
                try
                {
                    text = System.Text.Encoding.UTF8.GetString(itemName.Value);
                }
                catch
                {

                }
                this.listBox1.Items.Add("name:" + text);
            }

            if (itemPic == null)
            {
                this.pictureBox1.Image = null;
            }
            else
            {
                using (var ms = new System.IO.MemoryStream(itemPic.Value))
                {
                    this.pictureBox1.Image = Bitmap.FromStream(ms);
                }
            }
        }
        static byte[] GetScriptHashFromAddress(string add)
        {
            //后四个自己是hash
            //第一个是魔法数字
            var bts = Neo.Cryptography.Base58.Decode(add);
            return bts.Take(bts.Length - 4).Skip(1).ToArray();

        }

        int index = 0;
        private void button2_Click(object sender, EventArgs e)
        {
            if (plugin_whoami.api.CurrentWallet == null)
                return;
            var count = plugin_whoami.api.CurrentWallet.GetAccounts().Count();
            if (index >= count) index = 0;
            var key = plugin_whoami.api.CurrentWallet.GetAccounts().ToArray()[index].GetKey();
            this.textBoxPubKey.Text = key.PublicKey.EncodePoint(true).ToHexString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var p1 = new Neo.SmartContract.ContractParameter(Neo.SmartContract.ContractParameterType.ByteArray)
            {
                Value = new byte[] { 0x01 }
            };
            var p2 = new Neo.SmartContract.ContractParameter(Neo.SmartContract.ContractParameterType.ByteArray)
            {
                Value = System.Text.Encoding.UTF8.GetBytes(textBoxName.Text)
            };
            var script = plugin_whoami.MakeAppCallScript(plugin_whoami.microblog_scripthash, this.textBoxPubKey.Text, p1, p2);
            plugin_whoami.CallScript(script);
        }

        private void textBoxPubKey_TextChanged(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            StackItem item;
            if (plugin_whoami.notifyexs.TryDequeue(out item))
            {
                this.listBox2.Items.Add(item.GetArray()[0].GetByteArray().ToHexString());
            }
        }

        private void WhoAmiEx_Load(object sender, EventArgs e)
        {

        }
        byte[] files = null;

        private void button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog opwn = new OpenFileDialog();
            if (opwn.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileInfo f = new System.IO.FileInfo(opwn.FileName);
                if (f.Length > 4096)
                    MessageBox.Show("too big file.");
                files = System.IO.File.ReadAllBytes(f.FullName);
                label5.Text = "file len=" + files.Length;
            }
            else
            {
                files = null;
                label5.Text = "no file";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var p1 = new Neo.SmartContract.ContractParameter(Neo.SmartContract.ContractParameterType.ByteArray)
            {
                Value = new byte[] { 0x02 }
            };
            var p2 = new Neo.SmartContract.ContractParameter(Neo.SmartContract.ContractParameterType.ByteArray)
            {
                Value = files
            };
            var script = plugin_whoami.MakeAppCallScript(plugin_whoami.microblog_scripthash, this.textBoxPubKey.Text, p1, p2);
            plugin_whoami.CallScript(script);
        }
    }
}
