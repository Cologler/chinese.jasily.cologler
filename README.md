# chinese.jasily.cologler

a lib for china.

一个支持中文数字解析及拼音解析的库。

## chinese digit

supported chinese digit parse.

``` cs
var cd = new ChineseDigit();
cd.ParseInt64("壹"); // return 1L
```

## chinese pin yin

supported chinese pin yin parse.

``` cs
var manager = PinYinManager.CreateInstance();
var pinyin = manager['赖']; // return Nullable<PinYin>
pinyin.Value.PinYin; // return "lai"
pinyin.Value.Tone; // return ToneType.Tone4
```