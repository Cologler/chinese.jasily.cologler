using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Jasily.Chinese.PinYin
{
    public class PinYinManager
    {
        private readonly Lazy<Dictionary<uint, string>> innerLazyData;

        private PinYinManager(string uni2Pinyin)
        {
            var factory = new Func<Dictionary<uint, string>>(() => Init(uni2Pinyin));
            this.innerLazyData = new Lazy<Dictionary<uint, string>>(factory, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private static Dictionary<uint, string> Init(string uni2Pinyin)
        {
            var innerData = new Dictionary<uint, string>();

            using (var reader = new StringReader(uni2Pinyin))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null) break;
                    if (!line.StartsWith("#"))
                    {
                        var lines = line.Split('\t');
                        innerData.Add(uint.Parse(lines[0], NumberStyles.HexNumber), line);
                    }
                }
            }

            return innerData;
        }

        public bool IsChineseChar(char ch)
        {
            return this.innerLazyData.Value.ContainsKey(ch);
        }

        public Pinyin? this[char ch]
        {
            get
            {
                Pinyin pinyin;
                return this.TryGetFirstPinYin(ch, out pinyin) ? (Pinyin?)pinyin : null;
            }
        }

        public bool TryGetPinYin(char ch, out Pinyin[] pinyins)
        {
            string r;

            if (this.innerLazyData.Value.TryGetValue(ch, out r))
            {
                pinyins = Selector(r);
                return true;
            }
            else
            {
                pinyins = null;
                return false;
            }
        }

        private static Pinyin[] Selector(string source)
        {
            return source.Split('\t').Skip(1).Select(z => new Pinyin(z)).ToArray();
        }

        public bool TryGetFirstPinYin(char ch, out Pinyin pinyin)
        {
            Pinyin[] pinyins = null;
            if (this.TryGetPinYin(ch, out pinyins) && pinyins.Length > 0)
            {
                pinyin = pinyins[0];
                return true;
            }
            else
            {
                pinyin = default(Pinyin);
                return false;
            }
        }

        public static PinYinManager CreateInstance()
        {
            const string name = "Jasily.Chinese.Assets.Uni2Pinyin";
            using (var stream = typeof(PinYinManager).GetTypeInfo().Assembly.GetManifestResourceStream(name))
            using (var reader = new StreamReader(stream))
            {
                var text = reader.ReadToEnd();
                return new PinYinManager(text);
            }
        }
    }
}
