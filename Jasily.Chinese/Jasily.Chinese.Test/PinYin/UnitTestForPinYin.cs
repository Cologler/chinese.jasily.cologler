using Jasily.Chinese.PinYin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jasily.Chinese.Test.PinYin
{
    [TestClass]
    public class UnitTestForPinYin
    {
        [TestMethod]
        public void Test_0()
        {
            var manager = PinYinManager.CreateInstance();
            var pinyin = manager['赖'];
            Assert.IsNotNull(pinyin);
            Assert.AreEqual("lai", pinyin.Value.PinYin);
            Assert.AreEqual(ToneType.Tone4, pinyin.Value.Tone);
        }
    }
}