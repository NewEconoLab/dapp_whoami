using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

namespace WhoAmI_Contract
{
    public class Contract1 : SmartContract
    {
        public static bool Main(byte[] pubkey,string text)
        {
            var magicstr = "who am i.";//魔法数据，就算别人也写了一样的合约，我还是可以有我独特的版本
            
            if(Runtime.CheckWitness(pubkey)==false)
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

            //用Scripthash做key，别人用scripthash就可以查了
            Storage.Put(Storage.CurrentContext, scriphash, text);
            Runtime.Log("set succ");//调试用
            return true;
        }
    }
}
