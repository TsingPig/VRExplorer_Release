using System.Text;
using UnityEngine;

namespace HenryLab
{
    public class RichText
    {
        private StringBuilder _textBuilder;

        public RichText()
        {
            _textBuilder = new StringBuilder();
        }

        /// <summary>
        /// ��Ӹ��ı���֧�ֶ����ʽ����
        /// </summary>
        /// <param name="text"></param>
        /// <param name="bold"></param>
        /// <param name="italic"></param>
        /// <param name="underline"></param>
        /// <param name="color"></param>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        public RichText Add(string text, bool bold = false, bool italic = false, bool underline = false,
                            Color? color = null, int fontSize = -1)
        {
            if(bold) text = $"<b>{text}</b>";
            if(italic) text = $"<i>{text}</i>";
            if(underline) text = $"<u>{text}</u>";

            if(color.HasValue)
            {
                string colorHex = ColorUtility.ToHtmlStringRGB(color.Value);
                text = $"<color=#{colorHex}>{text}</color>";
            }

            if(fontSize >= 0)
            {
                text = $"<size={fontSize}>{text}</size>";
            }

            _textBuilder.Append(text);
            return this;
        }

        /// <summary>
        /// ��ȡ���յĸ��ı��ַ���
        /// </summary>
        /// <returns></returns>
        public string GetText()
        {
            return _textBuilder.ToString();
        }

        /// <summary>
        /// ���ڵ���������ı�
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetText();
        }
    }
}