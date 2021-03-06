using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Netnr.Fast
{
    /// <summary>
    /// 引用组件：System.Drawing.Common
    /// Windows首选，速度快，Linux需安装 libgdiplus 组件
    /// </summary>
    public class CaptchaForDrawingCommon
    {
        /// <summary>
        /// 生成图片验证码
        /// </summary>
        /// <param name="code">随机码</param>
        public static byte[] CreateImg(string code)
        {
            Random random = new Random();

            //为验证码插入空格
            for (int i = 0; i < 2; i++)
            {
                code = code.Insert(random.Next(code.Length - 1), " ");
            }

            //验证码颜色集合  
            Color[] colors = { Color.LightBlue, Color.LightCoral, Color.LightGreen, Color.LightPink, Color.LightSkyBlue, Color.LightSteelBlue, Color.LightSalmon };

            //定义图像的大小，生成图像的实例  
            using Bitmap Img = new Bitmap(code.Length * 22, 38);
            using Graphics g = Graphics.FromImage(Img);

            //背景设为白色
            g.Clear(Color.White);

            //在随机位置画背景点
            for (int i = 0; i < 200; i++)
            {
                int x = random.Next(Img.Width);
                int y = random.Next(Img.Height);
                g.DrawRectangle(new Pen(colors[random.Next(colors.Length - 1)], 0), x, y, 1, 1);
            }

            //验证码绘制
            for (int i = 0; i < code.Length; i++)
            {
                Font f = new Font(FontFamily.GenericSerif, 24, (FontStyle.Italic | FontStyle.Bold));//字体  
                Brush b = new SolidBrush(colors[random.Next(colors.Length - 1)]);//颜色  

                //控制验证码不在同一高度
                int ii = random.Next(15) * (random.Next(1) % 2 == 0 ? -1 : 1) + 8;
                g.DrawString(code.Substring(i, 1), f, b, (i * 20), ii);//绘制一个验证字符
            }

            using MemoryStream ms = new MemoryStream();
            Img.Save(ms, ImageFormat.Jpeg);

            return ms.ToArray();
        }
    }
}
