using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Jasily.Chinese.Test
{
    [TestClass]
    public class UnitTestForChineseDigit
    {
        private static readonly ChineseDigit cd1 = new ChineseDigit();
        private static readonly ChineseDigit cd2 = new ChineseDigit(true);

        [TestClass]
        public class UnitTestForParse
        {
            [TestMethod]
            public void Test_0()
            {
                var cd = new ChineseDigit();
                Assert.AreEqual(cd.ParseInt64("零"), 0L);
            }

            [TestMethod]
            public void Test_1()
            {
                var cd = new ChineseDigit();
                Assert.AreEqual(cd.ParseInt64("壹"), 1L);
                Assert.AreEqual(cd.ParseInt64("一"), 1L);
            }

            [TestMethod]
            public void Test_n_1()
            {
                var cd = new ChineseDigit();
                Assert.AreEqual(cd.ParseInt64("负壹"), -1L);
                Assert.AreEqual(cd.ParseInt64("负一"), -1L);
            }

            [TestMethod]
            public void Test_2()
            {
                Assert.AreEqual(new ChineseDigit().ParseInt64("贰"), 2L);
                Assert.AreEqual(new ChineseDigit().ParseInt64("二"), 2L);
            }

            [TestMethod]
            public void Test_3()
            {
                Assert.AreEqual(new ChineseDigit().ParseInt64("叁"), 3L);
                Assert.AreEqual(new ChineseDigit().ParseInt64("三"), 3L);
            }

            [TestMethod]
            public void Test_4()
            {
                Assert.AreEqual(new ChineseDigit().ParseInt64("肆"), 4L);
                Assert.AreEqual(new ChineseDigit().ParseInt64("四"), 4L);
            }

            [TestMethod]
            public void Test_5()
            {
                Assert.AreEqual(new ChineseDigit().ParseInt64("伍"), 5L);
                Assert.AreEqual(new ChineseDigit().ParseInt64("五"), 5L);
            }

            [TestMethod]
            public void Test_6()
            {
                Assert.AreEqual(new ChineseDigit().ParseInt64("陆"), 6L);
                Assert.AreEqual(new ChineseDigit().ParseInt64("六"), 6L);
            }

            [TestMethod]
            public void Test_7()
            {
                Assert.AreEqual(new ChineseDigit().ParseInt64("柒"), 7L);
                Assert.AreEqual(new ChineseDigit().ParseInt64("七"), 7L);
            }

            [TestMethod]
            public void Test_8()
            {
                Assert.AreEqual(new ChineseDigit().ParseInt64("捌"), 8L);
                Assert.AreEqual(new ChineseDigit().ParseInt64("八"), 8L);
            }

            [TestMethod]
            public void Test_9()
            {
                Assert.AreEqual(new ChineseDigit().ParseInt64("玖"), 9L);
                Assert.AreEqual(new ChineseDigit().ParseInt64("九"), 9L);
            }

            [TestMethod]
            public void Test_10()
            {
                Assert.AreEqual(new ChineseDigit().ParseInt64("拾"), 10L);
                Assert.AreEqual(new ChineseDigit().ParseInt64("十"), 10L);
            }

            [TestMethod]
            public void Test_12()
            {
                var cd = new ChineseDigit();
                Assert.AreEqual(12L, cd.ParseInt64("拾贰"));
                Assert.AreEqual(12L, cd.ParseInt64("十二"));
                Assert.AreEqual(12L, cd.ParseInt64("壹拾贰"));
                Assert.AreEqual(12L, cd.ParseInt64("一十二"));
            }

            [TestMethod]
            public void Test_23()
            {
                Assert.AreEqual(new ChineseDigit().ParseInt64("贰拾叁"), 23L);
                Assert.AreEqual(new ChineseDigit().ParseInt64("二十三"), 23L);
            }

            [TestMethod]
            [ExpectedException(typeof(FormatException))]
            public void Test_e_100()
            {
                Assert.AreEqual(new ChineseDigit().ParseInt64("佰"), 1L);
                Assert.AreEqual(new ChineseDigit().ParseInt64("百"), 1L);
            }

            [TestMethod]
            [ExpectedException(typeof(FormatException))]
            public void Test_e_1000()
            {
                Assert.AreEqual(new ChineseDigit().ParseInt64("仟"), 1L);
                Assert.AreEqual(new ChineseDigit().ParseInt64("千"), 1L);
            }

            [TestMethod]
            [ExpectedException(typeof(FormatException))]
            public void Test_e_10000()
            {
                Assert.AreEqual(new ChineseDigit().ParseInt64("萬"), 1L);
                Assert.AreEqual(new ChineseDigit().ParseInt64("万"), 1L);
            }

            [TestMethod]
            [ExpectedException(typeof(FormatException))]
            public void Test_e_100000000()
            {
                Assert.AreEqual(new ChineseDigit().ParseInt64("亿"), 1L);
                Assert.AreEqual(new ChineseDigit().ParseInt64("亿"), 1L);
            }


        }

        [TestMethod]
        public void Test_192()
        {
            var a = Tuple.Create(192, "一百九十二", "壹佰玖拾贰");

            Assert.AreEqual(a.Item2, a.Item1.ToString(cd1));
            Assert.AreEqual(a.Item3, a.Item1.ToString(cd2));
            Assert.AreEqual(a.Item1, cd1.ParseDecimal(a.Item2));
            Assert.AreEqual(a.Item1, cd1.ParseDecimal(a.Item3));
        }

        [TestMethod]
        public void Test_192_684()
        {
            var a = Tuple.Create(192.684m, "一百九十二点六八四", "壹佰玖拾贰点陆捌肆");

            Assert.AreEqual(a.Item2, a.Item1.ToString(cd1));
            Assert.AreEqual(a.Item3, a.Item1.ToString(cd2));
            Assert.AreEqual(a.Item1, cd1.ParseDecimal(a.Item2));
            Assert.AreEqual(a.Item1, cd1.ParseDecimal(a.Item3));
        }

        [TestMethod]
        public void Test_190531002_684771()
        {
            var a = Tuple.Create(190531002.684771m, "一亿九千零五十三万一千零二点六八四七七一", "壹亿玖仟零伍拾叁萬壹仟零贰点陆捌肆柒柒壹");

            Assert.AreEqual(a.Item2, a.Item1.ToString(cd1));
            Assert.AreEqual(a.Item3, a.Item1.ToString(cd2));
            Assert.AreEqual(a.Item1, cd1.ParseDecimal(a.Item2));
            Assert.AreEqual(a.Item1, cd1.ParseDecimal(a.Item3));
        }

        [TestClass]
        public class UnitTestForToString
        {
            [TestMethod]
            public void Test_1()
            {
                var cd = new ChineseDigit();
                Assert.AreEqual(1.ToString(cd), "一");
                cd = new ChineseDigit(true);
                Assert.AreEqual(1.ToString(cd), "壹");
            }

            [TestMethod]
            public void Test_10()
            {
                var cd = new ChineseDigit();
                Assert.AreEqual(10.ToString(cd), "十");
                cd = new ChineseDigit(true);
                Assert.AreEqual(10.ToString(cd), "拾");
            }

            [TestMethod]
            public void Test_16()
            {
                var cd = new ChineseDigit();
                Assert.AreEqual("十六", 16.ToString(cd));
                cd = new ChineseDigit(true);
                Assert.AreEqual("拾陆", 16.ToString(cd));
            }

            [TestMethod]
            public void Test_n_1()
            {
                var cd = new ChineseDigit();
                Assert.AreEqual("负一", (-1).ToString(cd));
                cd = new ChineseDigit(true);
                Assert.AreEqual("负壹", (-1).ToString(cd));
            }

            [TestMethod]
            public void Test_n_162_12()
            {
                var cd = new ChineseDigit();
                Assert.AreEqual("负一百六十二点一二", (-162.12m).ToString(cd));
                cd = new ChineseDigit(true);
                Assert.AreEqual("负壹佰陆拾贰点壹贰", (-162.12m).ToString(cd));
            }

            [TestMethod]
            public void Test_1_1()
            {
                var cd = new ChineseDigit();
                Assert.AreEqual("一点一", (1.1m).ToString(cd));
                cd = new ChineseDigit(true);
                Assert.AreEqual("壹点壹", (1.1m).ToString(cd));
            }

            [TestMethod]
            public void Test_1_122()
            {
                var cd = new ChineseDigit();
                Assert.AreEqual("一点一二二", (1.122m).ToString(cd));
                cd = new ChineseDigit(true);
                Assert.AreEqual("壹点壹贰贰", (1.122m).ToString(cd));
            }
        }
    }
}
