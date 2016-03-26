using System.Diagnostics;

namespace Jasily.Chinese.PinYin
{
    public struct Pinyin
    {
        internal Pinyin(string pinyin)
        {
            Debug.Assert(!string.IsNullOrEmpty(pinyin));

            this.PinYin = pinyin.Substring(0, pinyin.Length - 1);

            switch (pinyin[pinyin.Length - 1])
            {
                case '1':
                    this.Tone = ToneType.Tone1; break;
                case '2':
                    this.Tone = ToneType.Tone2; break;
                case '3':
                    this.Tone = ToneType.Tone3; break;
                case '4':
                    this.Tone = ToneType.Tone4; break;
                default:
                    this.Tone = ToneType.Unknown; break;
            }
        }

        public string PinYin { get; }

        public ToneType Tone { get; }
    }
}