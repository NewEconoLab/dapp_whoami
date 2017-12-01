using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

namespace WhoAmI_Contract
{
    public class Contract1 : SmartContract
    {
        public static bool Main(byte[] pubkey, byte[] key, byte[] info)
        {
            var magicstr = "NeoSmartBlog.";//魔法数据，就算同样功能的合约，我也可以部署多个

            if (Runtime.CheckWitness(pubkey) == false)
            {
                Runtime.Log("verify fail");//调试用
                return false;
            }
            Runtime.Log("verify succ");//调试用

            //用pubkey 计算出scripthash，因为地址是scripthash处理了一下
            //scripthash 和用户地址可以互相转换
            byte[] head = { 33 };
            byte[] end = { 0xac };
            byte[] script = Helper.Concat(head, pubkey);
            script = Helper.Concat(script, end);
            byte[] scriphash = SmartContract.Hash160(script);

            Runtime.Notify(scriphash);//调试用


            byte[] ktag = { 0x20 };
            var skey = Helper.Concat(scriphash, ktag);
            skey = Helper.Concat(skey, key);
            //用Scripthash做key，别人用scripthash就可以查了
            Storage.Put(Storage.CurrentContext, skey, info);

            byte[] nowtag = { 0x01 };
            var nowkey = Helper.Concat(scriphash, nowtag);
            byte[] lasttag = { 0x02 };
            var lastkey = Helper.Concat(scriphash, lasttag);

            var lastid = Storage.Get(Storage.CurrentContext, nowkey);
            var nowid = Helper.AsByteArray(Blockchain.GetHeight());
            Storage.Put(Storage.CurrentContext, nowkey, nowid);
            Storage.Put(Storage.CurrentContext, lastkey, lastid);
            Runtime.Log("set succ");//调试用
            return true;
        }
    }
}
