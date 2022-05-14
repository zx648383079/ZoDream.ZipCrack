using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ZoDream.Shared.CSharp
{
    /// <summary>
    /// ?a 表示大小写字母数字符号 包含?l?u?d?s，重复几次表示密码有几位
    /// ?d 表示数字
    /// ?l 小写字母
    /// ?u 大写字母
    /// ?s 特殊字符 «space»!"#$%&'()*+,-./:;<=>?@[]^_`{|}~
    /// ?b 0x00 - 0xff
    /// ?h 十六进制字符 0123456789abcdef
    /// ?H 大写的十六进制字符 0123456789ABCDEF
    /// </summary>
    public class PasswordRule: IEnumerator<byte[]>, IEnumerable<byte[]>
    {
        public PasswordRule(string rule)
        {
            Rule = rule;
        }

        private string _rule = string.Empty;

        public string Rule
        {
            get { return _rule; }
            set {
                _rule = value;
                SetRule(value);
            }
        }

        private List<Tuple<byte, bool>> RuleBockItems = new();

        private long Position =  -1;

        /// <summary>
        /// 总数量
        /// </summary>
        public long Count { get; private set; }

        /// <summary>
        /// 有多少位
        /// </summary>
        public int Length { get; private set; }

        public byte[] Current => Generate(Position);

        object IEnumerator.Current => Generate(Position);

        byte[] this[long index] 
        {
            get
            {
                return Generate(index);
            }
        }

        public IEnumerator<byte[]> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            if (Position > Count - 2)
            {
                return false;
            }
            Position++;
            return true;
        }

        public void Reset()
        {
            Position = -1;
        }


        private byte[] Generate(long index)
        {
            var res = new List<byte>();
            foreach (var item in RuleBockItems)
            {
                if (!item.Item2)
                {
                    res.Add(item.Item1);
                    continue;
                }
                var tag = (char)item.Item1;
                var count = GetCount(tag);
                if (count <= 0)
                {
                    continue;
                }
                res.Add(GetByte(tag, (int)(index % count)));
            }
            return res.ToArray();
        }

        private byte GetByte(char tag, int v)
        {
            switch (tag)
            {
                case 'a':
                    return (byte)(32 + v);
                case 'd':
                    return (byte)(48 + v);
                case 'l':
                    return (byte)(97 + v);
                case 'u':
                    return (byte)(65 + v);
                case 's':
                    if (v < 16)
                    {
                        return (byte)(v + 32);// 16
                    }
                    if (v < 23)
                    {
                        return (byte)(42 + v); // 7
                    }
                    if (v < 29)
                    {
                        return (byte)(68 + v);// 6
                    }
                    if (v < 33)
                    {
                        return (byte)(94 + v);// 4
                    }
                    return 22;
                case 'b':
                    return (byte)v;
                case 'h':
                    return (byte)(v < 10 ? (48 + v) : (87 + v));
                case 'H':
                    return (byte)(v < 10 ? (48 + v) : (55 + v));
                default:
                    return 0;
            }
        }

        private void SetRule(string rule)
        {
            RuleBockItems.Clear();
            Length = 0;
            Count = 0;
            foreach (var item in rule.Split('?'))
            {
                if (string.IsNullOrEmpty(item))
                {
                    if (Length > 0)
                    {
                        AddText("?");
                    }
                    continue;
                }
                var first = item[0];
                if (!IsRule(first))
                {
                    AddText("?" + item);
                    continue;
                }
                AddRule(first);
                if (item.Length > 1)
                {
                    AddText(item.Substring(1));
                }
            }
            Position = -1;
        }

        private void AddText(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return;
            }
            var items = Encoding.UTF8.GetBytes(word);
            foreach (var item in items)
            {
                RuleBockItems.Add(new Tuple<byte, bool>(item, false));
            }
            Length += items.Length;
        }

        private bool IsRule(char tag)
        {
            return tag == 'a' || tag == 'd' || tag == 'l' || tag == 'u' || tag == 's' || 
                tag == 'b' || tag == 'h' || tag == 'H';
        }


        private void AddRule(char tag)
        {
            RuleBockItems.Add(new Tuple<byte, bool>((byte)tag, true));
            Length ++;
            Count *= GetCount(tag);
        }

        private int GetCount(char tag)
        {
            return tag switch
            {
                'a' => 95,
                'd' => 10,
                'l' => 26,
                'u' => 26,
                's' => 33,
                'b' => 256,
                'h' => 16,
                'H' => 16,
                _ => 0,
            };
        }

        public IList<byte> Charset
        {
            get
            {
                var data = new List<byte>();
                foreach (var item in RuleBockItems)
                {
                    if (!item.Item2)
                    {
                        if (!data.Contains(item.Item1))
                        {
                            data.Add(item.Item1);
                        }
                        continue;
                    }
                    var tag = (char)item.Item1;
                    var count = GetCount(tag);
                    if (count < 1)
                    {
                        continue;
                    }
                    for (int i = 0; i < count; i++)
                    {
                        var code = GetByte(tag, i);
                        if (!data.Contains(code))
                        {
                            data.Add(code);
                        }
                    }
                }
                data.Sort();
                return data;
            }
        }
        public void Dispose()
        {
            
        }
    }
}
