using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Jasily.Chinese
{
    public sealed class ChineseDigit
    {
        private const int GroupBy = 10000;
        private const string NumberDecimalSeparator = "点";

        private static readonly char[][] ToLowerTable = new[]
        {
            new [] { '零', '零' },
            new [] { '壹', '一' },
            new [] { '贰', '二' },
            new [] { '叁', '三' },
            new [] { '肆', '四' },
            new [] { '伍', '五' },
            new [] { '陆', '六' },
            new [] { '柒', '七' },
            new [] { '捌', '八' },
            new [] { '玖', '九' },
            new [] { '拾', '十' },

            new [] { '佰', '百' },
            new [] { '仟', '千' },
            new [] { '萬', '万' },
        };

        private NumberFormatInfo numberFormatInfo = new NumberFormatInfo();

        public bool IsUpperCase { get; }

        public ChineseDigit(bool isUpperCase = false)
        {
            this.IsUpperCase = isUpperCase;
            this.numberFormatInfo.PositiveSign = "正";
            this.numberFormatInfo.NegativeSign = "负";
            this.numberFormatInfo.NumberDecimalSeparator = NumberDecimalSeparator;
            typeof(NumberFormatInfo).GetRuntimeFields().First(z => z.Name == "nativeDigits")
                .SetValue(this.numberFormatInfo, ToLowerTable.Take(10).Select(z => "0".ToString()).ToArray());
        }

        private static char ToCase(char input, int index)
            => ToLowerTable.FirstOrDefault(t => input == t[(~index) & 1])?[index] ?? input;

        private static char ToLower(char input) => ToCase(input, 1);

        private static char ToUpper(char input) => ToCase(input, 0);

        private static string ToLower(string input)
        {
            return new string(input.ToCharArray().Select(ToLower).ToArray());
        }

        #region parse

        public long ParseInt64(string number)
        {
            if (number == null) throw new ArgumentNullException(nameof(number));
            if (string.IsNullOrWhiteSpace(number)) throw new ArgumentException(nameof(number));

            number = ToLower(number);
            var ptr = 0;
            var sign = 1L;
            var @group = 0L;
            var value = 0L;

            var first = ChineseDigitChar.Convert(number[ptr]);
            if ((first.Flags & ChineseDigitFlags.Start) != ChineseDigitFlags.Start)
            {
                throw new FormatException();
            }
            else
            {
                if (first.IsSign)
                {
                    sign = first.Value;
                    ptr++;
                }
            }

            if (number.Length == ptr) throw new FormatException();

            for (; ptr < number.Length; ptr++)
            {
                var ns = new List<ChineseDigitChar>();
                var us = new List<ChineseDigitChar>();
                ChineseDigitChar dc = null;

                for (; ptr < number.Length; ptr++)
                {
                    var ch = number[ptr];
                    dc = ChineseDigitChar.Convert(ch);
                    if (ns.Count == 0)
                    {
                        if ((dc.Flags & ChineseDigitFlags.Padding) == ChineseDigitFlags.Padding)
                        {
                        }
                        else if ((dc.Flags & ChineseDigitFlags.Number) == ChineseDigitFlags.Number)
                        {
                            ns.Add(dc);
                        }
                        else
                        {
                            throw new FormatException();
                        }
                    }
                    else
                    {
                        if ((dc.Flags & ChineseDigitFlags.Level) == ChineseDigitFlags.Level)
                        {
                            us.Add(dc);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                var val = ns.Sum(n => n.Value);
                val = us.Aggregate(val, (current, u) => current * u.Value);
                @group += val;
                if ((dc.Flags & ChineseDigitFlags.Group) == ChineseDigitFlags.Group)
                {
                    value += @group * dc.Value;
                    @group = 0;
                }
                else
                {
                    ptr--;
                }
            }
            value += @group;
            return value * sign;
        }

        public int ParseInt32(string number)
            => Convert.ToInt32(this.ParseInt64(number));

        public decimal ParseDecimal(string number)
        {
            if (number == null) throw new ArgumentNullException(nameof(number));

            var dot = number.IndexOf(NumberDecimalSeparator, StringComparison.Ordinal);
            if (dot == -1) return this.ParseInt64(number);
            if (dot == 0 || dot == number.Length - 1) throw new FormatException();

            var val = (decimal)this.ParseInt64(number.Substring(0, dot));
            var rightString = ToLower(number.Substring(dot));
            for (int i = 1; i < rightString.Length; i++)
            {
                var c = ChineseDigitChar.TryConvert(rightString[i]);
                if (c == null || c.Value == 10) throw new FormatException();
                if ((c.Flags & ChineseDigitFlags.Number) != ChineseDigitFlags.Number) throw new FormatException();
                val += (decimal)Math.Pow(0.1, i) * c.Value;
            }
            return val;
        }

        #endregion

        public string Format(decimal value)
        {
            var caseIndex = this.IsUpperCase ? 0 : 1;
            var sb = new StringBuilder();

            // sign
            var sign = Math.Sign(value);
            if (sign == 0) return ToLowerTable[0][caseIndex].ToString();
            if (sign < 0) sb.Append("负");
            value = Math.Abs(value);

            var left = Math.Truncate(value);
            var right = value - left;

            // > 1
            var groups = new List<ChineseDigitGroup>();
            while (left >= GroupBy)
            {
                groups.Insert(0, new ChineseDigitGroup((int)(left % GroupBy), groups.Count));
                left /= GroupBy;
            }
            groups.Insert(0, new ChineseDigitGroup((int)(left % GroupBy), groups.Count, false));
            var c = groups.Count;
            while (right > 0)
            {
                right *= GroupBy;
                groups.Add(new ChineseDigitGroup((int)right, c - groups.Count - 1));
                right -= Math.Truncate(right);
            }
            foreach (var s in groups.Select(z => z.ToString(this.IsUpperCase
                    ? ChineseDigitFormatProvider.Upper
                    : ChineseDigitFormatProvider.Lower)))
            {
                sb.Append(s);
            }
            return sb.ToString();
        }

        public static int? TryBinarySqrt(int value)
        {
            if (value == 0) return null;
            if (value == 1) return 0;

            var n = 0;
            while (value != 0 && (value & 1) == 0)
            {
                if (value == 2) return 1 + n;
                if (value == 4) return 2 + n;
                if (value == 8) return 3 + n;
                if (value == 16) return 4 + n;
                if ((value & 0xf) != 0) return null;
                value >>= 4;
                n += 4;
            }
            return value == 0 ? n : (int?)null;
        }

        public static Tuple<int, long> SplitHighestPlace(long value, int system)
        {
            if (system <= 0) throw new ArgumentOutOfRangeException(nameof(system));

            if (value == 0) return Tuple.Create(0, 0L);

            value = Math.Abs(value);
            var next = 1L;
            var x = TryBinarySqrt(system);
            if (x.HasValue)
            {
                do
                {
                    next <<= x.Value;
                } while (value >= next);
                next >>= x.Value;
            }
            else
            {
                do
                {
                    next *= system;
                } while (value >= next);
                next /= system;
            }
            return Tuple.Create((int)(value / next), value % next);
        }

        private class ChineseDigitGroup
        {
            private readonly int level;
            private readonly bool hasHeader;
            private readonly int n100;
            private readonly int n1000;
            private readonly int n10;
            private readonly int n1;

            public ChineseDigitGroup(int number, int level, bool hasHeader = true)
            {
                this.level = level;
                this.hasHeader = hasHeader;
                this.n1 = number % 10;
                if (number >= 10)
                {
                    number /= 10;
                    this.n10 = number % 10;
                }
                if (number >= 10)
                {
                    number /= 10;
                    this.n100 = number % 10;
                }
                if (number >= 10)
                {
                    number /= 10;
                    this.n1000 = number % 10;
                }
            }

            public string ToString(ChineseDigitFormatProvider provider) => string.Concat(this.EnumerableToString(provider));

            private IEnumerable<string> EnumerableToString(ChineseDigitFormatProvider provider)
            {
                var hasEmpty = false;
                var hasHeader = this.hasHeader;

                foreach (var s in this.EnumerableProperties(provider))
                {
                    if (s == null)
                    {
                        hasEmpty = true;
                    }
                    else
                    {
                        if (hasHeader && hasEmpty)
                        {
                            yield return provider[0];
                        }

                        yield return s;

                        hasEmpty = false;
                        hasHeader = true;
                    }
                }
            }

            private IEnumerable<string> EnumerableProperties(ChineseDigitFormatProvider provider)
            {
                if (this.level == -1) yield return "点";

                if (this.n1000 > 0)
                {
                    yield return provider[this.n1000];
                    if (this.level > -1) yield return provider.Thousand;
                }
                else
                {
                    yield return null;
                }

                if (this.n100 > 0)
                {
                    yield return provider[this.n100];
                    if (this.level > -1) yield return provider.Hundred;
                }
                else
                {
                    yield return null;
                }

                if (this.n10 > 0)
                {
                    if (this.n10 == 1 && !this.hasHeader && this.n1000 == 0 && this.n100 == 0)
                        yield return "";
                    else
                        yield return provider[this.n10];
                    if (this.level > -1) yield return provider.Ten;
                }
                else
                {
                    yield return null;
                }

                if (this.n1 > 0)
                {
                    yield return provider[this.n1];
                }

                if (this.level > 0)
                {
                    yield return provider.GetGroupSeparator(this.level);
                }
            }
        }

        private sealed class ChineseDigitFormatProvider
        {
            private readonly int index;

            private static readonly string[][] UpperLowerNumberNativeDigits = new[]
            {
                new[] { "零", "零" },
                new[] { "壹", "一" },
                new[] { "贰", "二" },
                new[] { "叁", "三" },
                new[] { "肆", "四" },
                new[] { "伍", "五" },
                new[] { "陆", "六" },
                new[] { "柒", "七" },
                new[] { "捌", "八" },
                new[] { "玖", "九" },
            };

            private static readonly string[][] UpperLowerNumberGroupSeparators = new[]
            {
                new[] { "", "" },
                new[] { "萬", "万" },
                new[] { "亿", "亿" },
                new[] { "兆", "兆" },
            };

            private static readonly string[][] UpperLowerNumberLevelSeparators = new[]
            {
                new[] { "拾", "十" },
                new [] { "佰", "百" },
                new [] { "仟", "千" },
            };

            private ChineseDigitFormatProvider(int index)
            {
                this.index = index;
            }

            public string this[int index] => UpperLowerNumberNativeDigits[index][this.index];

            public string GetGroupSeparator(int index) => UpperLowerNumberGroupSeparators[index][this.index];

            public string Ten => UpperLowerNumberLevelSeparators[0][this.index];

            public string Hundred => UpperLowerNumberLevelSeparators[1][this.index];

            public string Thousand => UpperLowerNumberLevelSeparators[2][this.index];

            public static ChineseDigitFormatProvider Upper { get; } = new ChineseDigitFormatProvider(0);

            public static ChineseDigitFormatProvider Lower { get; } = new ChineseDigitFormatProvider(1);
        }

        private class ChineseDigitChar : IEquatable<char>
        {
            public static readonly ChineseDigitChar[] Chars;

            static ChineseDigitChar()
            {
                Chars = new[]
                {
                    new ChineseDigitChar('正', 1,
                        ChineseDigitFlags.Sign | ChineseDigitFlags.Start),
                    new ChineseDigitChar('负', -1,
                        ChineseDigitFlags.Sign | ChineseDigitFlags.Start),
                    new ChineseDigitChar('零', 0,
                        ChineseDigitFlags.Number | ChineseDigitFlags.Start | ChineseDigitFlags.Padding),
                    new ChineseDigitChar('一', 1,
                        ChineseDigitFlags.Number | ChineseDigitFlags.Start),
                    new ChineseDigitChar('二', 2,
                        ChineseDigitFlags.Number | ChineseDigitFlags.Start),
                    new ChineseDigitChar('三', 3,
                        ChineseDigitFlags.Number | ChineseDigitFlags.Start),
                    new ChineseDigitChar('四', 4,
                        ChineseDigitFlags.Number | ChineseDigitFlags.Start),
                    new ChineseDigitChar('五', 5,
                        ChineseDigitFlags.Number | ChineseDigitFlags.Start),
                    new ChineseDigitChar('六', 6,
                        ChineseDigitFlags.Number | ChineseDigitFlags.Start),
                    new ChineseDigitChar('七', 7,
                        ChineseDigitFlags.Number | ChineseDigitFlags.Start),
                    new ChineseDigitChar('八', 8,
                        ChineseDigitFlags.Number | ChineseDigitFlags.Start),
                    new ChineseDigitChar('九', 9,
                        ChineseDigitFlags.Number | ChineseDigitFlags.Start),
                    new ChineseDigitChar('十', 10,
                        ChineseDigitFlags.Number | ChineseDigitFlags.Level | ChineseDigitFlags.Start),
                    new ChineseDigitChar('百', 100,
                        ChineseDigitFlags.Level),
                    new ChineseDigitChar('千', 1000,
                        ChineseDigitFlags.Level),
                    new ChineseDigitChar('万', 10000,
                        ChineseDigitFlags.Group),
                    new ChineseDigitChar('亿', 100000000,
                        ChineseDigitFlags.Group),
                };
            }

            public char Char { get; }

            public long Value { get; }

            public bool IsSign => (this.Flags & ChineseDigitFlags.Sign) == ChineseDigitFlags.Sign;

            public ChineseDigitFlags Flags { get; }

            public ChineseDigitChar(char ch, long value, ChineseDigitFlags flags)
            {
                this.Char = ch;
                this.Value = value;
                this.Flags = flags;
            }

            public static ChineseDigitChar TryConvert(char ch)
                => Chars.FirstOrDefault(z => z.Equals(ch));

            public static ChineseDigitChar Convert(char ch)
            {
                var ret = Chars.FirstOrDefault(z => z.Equals(ch));
                if (ret == null) throw new FormatException();
                return ret;
            }

            public static bool IsChineseDigitChar(char ch)
                => TryConvert(ch) != null;

            public bool Equals(char other) => this.Char == other;
        }

        [Flags]
        private enum ChineseDigitFlags
        {
            None = 0,

            Sign = 1,

            Number = 2,

            Level = 4,

            Start = 8,

            Group = 16,

            Padding = 32,
        }
    }

    public static class ChineseDigitExtensions
    {
        public static string ToString(this decimal value, ChineseDigit provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            return provider.Format(value);
        }
        public static string ToString(this long value, ChineseDigit provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            return provider.Format(value);
        }
        public static string ToString(this int value, ChineseDigit provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            return provider.Format(value);
        }
    }
}