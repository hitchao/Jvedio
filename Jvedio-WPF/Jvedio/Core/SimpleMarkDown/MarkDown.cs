using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Jvedio.Core.SimpleMarkDown
{
    public class MarkDown
    {


        // 最多支持到 5 级 标题
        public static Dictionary<int, int> FontSize = new Dictionary<int, int>()
        {
            {-1,12 },
            {0,28 },
            {1,24 },
            {2,20 },
            {3,16 },
            {4,14 },
        };


        public enum LineType
        {
            Title,
            Quote,
            Code,
            PlainText
        }




        public static FlowDocument parse(string str)
        {
            FlowDocument document = new FlowDocument();
            List<string> list = str.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (string line in list)
            {
                char[] chars = line.ToCharArray();
                int titleLevel = -1;
                int quoteLevel = -1;
                for (int i = 0; i < chars.Length; i++)
                {
                    char c = chars[i];
                    if (c == '#')
                        titleLevel++;
                    else if (c == '>')
                        quoteLevel++;
                    else if (c == ' ')
                        break;
                    else break;
                }
                Paragraph myParagraph = new Paragraph();
                if (titleLevel >= 0)
                {
                    // 标题
                    myParagraph.Inlines.Add(new Bold(new Run(line.Replace(new string('#', titleLevel + 1) + " ", ""))));
                    myParagraph.FontSize = FontSize[titleLevel];
                    if (titleLevel <= 1)
                    {
                        myParagraph.BorderThickness = new System.Windows.Thickness(0, 0, 0, 1);
                        myParagraph.BorderBrush = Brushes.Gray;
                    }
                    myParagraph.Margin = new System.Windows.Thickness(0, 5, 0, 5);
                }
                else if (quoteLevel >= 0)
                {
                    // 引用
                    myParagraph.Inlines.Add(new Run(line.Replace(new string('>', quoteLevel + 1) + " ", "")));
                    myParagraph.FontSize = FontSize[-1];
                    myParagraph.BorderThickness = new System.Windows.Thickness(5, 0, 0, 0);
                    myParagraph.BorderBrush = Brushes.Gray;
                    myParagraph.Padding = new System.Windows.Thickness(5, 0, 0, 0);
                    myParagraph.Margin = new System.Windows.Thickness(0, 3, 0, 3);

                }
                else
                {
                    bool rendered = false;
                    myParagraph.FontSize = FontSize[-1];

                    // 处理超链接
                    int idx = line.IndexOf("[");

                    // 处理图片
                    if (idx >= 0)
                    {
                        int idx2 = line.IndexOf("](");
                        int idx3 = line.IndexOf(")");
                        if (idx < idx2 && idx2 < idx3)
                        {
                            if (idx > 0)
                            {
                                string value1 = line.Substring(0, idx);
                                myParagraph.Inlines.Add(new Run(value1));
                            }

                            string value2 = line.Substring(idx + 1, idx2 - idx - 1);
                            string url = line.Substring(idx2 + 2, idx3 - idx2 - 2);
                            Underline underline = new Underline(new Run(value2));
                            underline.Foreground = Brushes.LightBlue;
                            underline.Cursor = Cursors.Hand;
                            underline.PreviewMouseLeftButtonDown += (s, e) =>
                            {
                                System.Diagnostics.Process.Start(url);
                            };

                            myParagraph.Inlines.Add(underline);
                            if (idx3 < line.Length)
                            {
                                string value3 = line.Substring(idx3 + 1, line.Length - idx3 - 1);
                                myParagraph.Inlines.Add(new Run(value3));
                            }
                            rendered = true;
                        }
                    }

                    if (!rendered)
                    {
                        string src = Regex.Match(line, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Groups[1].Value;

                        if (!string.IsNullOrEmpty(src))
                        {
                            string width_str = Regex.Match(line, "<img.+?width=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Groups[1].Value;
                            string height_str = Regex.Match(line, "<img.+?height=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Groups[1].Value;

                            float.TryParse(width_str, out float width);
                            float.TryParse(height_str, out float height);
                            BitmapImage bitmap = new BitmapImage(new Uri(src));
                            Image image = new Image();
                            image.Source = bitmap;
                            image.Stretch = Stretch.Uniform;
                            image.Margin = new Thickness(0);
                            if (width > 0) image.Width = width;
                            if (height > 0) image.Height = height;
                            myParagraph.Inlines.Add(image);
                            rendered = true;
                        }
                    }


                    if (!rendered)
                        myParagraph.Inlines.Add(new Run(line));




                }
                document.Blocks.Add(myParagraph);
            }
            return document;
        }
    }
}
