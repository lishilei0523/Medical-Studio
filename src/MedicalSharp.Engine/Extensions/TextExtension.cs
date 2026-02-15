using System.Text;

namespace MedicalSharp.Engine.Extensions
{
    /// <summary>
    /// 文本扩展
    /// </summary>
    public static class TextExtension
    {
        /// <summary>
        /// 过滤注释
        /// </summary>
        /// <param name="code">代码</param>
        /// <returns>过滤注释后代码</returns>
        public static string RemoveComments(this string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return code;
            }

            StringBuilder result = new StringBuilder();
            int index = 0;
            int length = code.Length;
            while (index < length)
            {
                char current = code[index];
                char next = index + 1 < length ? code[index + 1] : '\0';

                //检查单行注释 - //
                if (current == '/' && next == '/')
                {
                    //跳过直到行尾
                    while (index < length && code[index] != '\n')
                    {
                        index++;
                    }
                    //保留换行符以维护行号
                    if (index < length)
                    {
                        result.Append('\n');
                        index++;
                    }
                    continue;
                }

                //检查多行注释 - /* */
                if (current == '/' && next == '*')
                {
                    //跳过 /*
                    index += 2;

                    //查找 */
                    while (index < length - 1)
                    {
                        if (code[index] == '*' && code[index + 1] == '/')
                        {
                            index += 2;
                            break;
                        }
                        index++;
                    }
                    continue;
                }

                //检查字符串常量（避免误删字符串中的注释标记）
                if (current == '"')
                {
                    //保留字符串内容
                    result.Append(current);
                    index++;

                    while (index < length)
                    {
                        result.Append(code[index]);

                        if (code[index] == '"' && code[index - 1] != '\\')
                        {
                            index++;
                            break;
                        }

                        if (code[index] == '\\' && index + 1 < length)
                        {
                            //处理转义字符
                            result.Append(code[index + 1]);
                            index += 2;
                        }
                        else
                        {
                            index++;
                        }
                    }
                    continue;
                }

                //正常字符
                result.Append(current);
                index++;
            }

            return result.ToString();
        }
    }
}
