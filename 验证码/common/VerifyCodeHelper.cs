using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;

namespace 验证码
{
    public class VerifyCodeHelper
    {
        public static readonly string defCodeKey = "session_verify_code";

        public static string GenerateImgCode(int codeLenth = 4, string sessionKey = "", EnumVerifyCodeType type = EnumVerifyCodeType.formula)
        {
            if (string.IsNullOrEmpty(sessionKey))
            {
                sessionKey = defCodeKey;
            }

            var imgContent = GenerateRdChars(codeLenth);
            var imgVal = string.Empty;
            switch (type)
            {
                case EnumVerifyCodeType.code:
                    imgContent = imgVal = GenerateRdChars(codeLenth);
                    CreateImage(imgContent);
                    break;
                case EnumVerifyCodeType.formula:
                    var result = GenerateFormula();
                    imgContent = result.Item1;
                    imgVal = result.Item2.ToString();
                    CreateImageWithBg(imgContent);
                    break;
            }

            HttpContext.Current.Session[sessionKey] = imgVal;
            return imgVal;
        }

        public static bool CheckCode(string code, string sessionKey = "")
        {
            if (string.IsNullOrEmpty(sessionKey))
            {
                sessionKey = defCodeKey;
            }
            var sessionVal = "";
            if (HttpContext.Current.Session[sessionKey] != null)
            {
                sessionVal = HttpContext.Current.Session[sessionKey].ToString();
            }
            return string.Equals(sessionVal, code, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// 生成随机字符串
        /// </summary>
        /// <param name="codelength">字符串长度</param>
        /// <returns></returns>
        static string GenerateRdChars(int codelength)
        {
            int number;
            string randomCode = string.Empty;
            Random r = new Random();
            for (int i = 0; i < codelength; i++)
            {
                number = r.Next();
                number = number % 36;  //字符从0-9,A-Z中随机产生，对应的ASCLL码分别为48-57,65-90
                if (number < 10)
                    number += 48;
                else
                    number += 55;

                if (number == 48 || number == 79 || number == 49 || number == 73)//过滤数字0和字母"O",过滤数字1和字母"i"
                    number = number + 2;

                randomCode += ((char)number).ToString();
            }

            return randomCode;
        }

        /// <summary>
        /// 生成简单加减乘除公式
        /// </summary>
        /// <returns></returns>
        static Tuple<string, int> GenerateFormula()
        {
            var operatorRdIndex = new Random().Next(0, arrOperators.Length) ;
            var operatorsStr = arrOperators[operatorRdIndex];
            var num1 = GenerateRdNum(2);
            var num2 = GenerateRdNum(1);
            if (num1 < num2)
            {
                num1 = num2 + GenerateRdNum(1);
            }

            var formula = string.Format("{0}{1}{2}=？", num1, operatorsStr, num2);
            var result = 0;
            switch (operatorsStr)
            {
                case "+":
                    result = num1 + num2;
                    break;
                case "-":
                    result = num1 - num2;
                    break;
            }

            return new Tuple<string, int>(formula, result);
        }

        ///  <summary>
        ///  创建随机码图片
        ///  </summary>
        ///  <param  name="randomcode">随机码</param>
        static void CreateImage(string randomcode)
        {
            int randAngle = 40; //随机转动角度
            int mapwidth = (int)(randomcode.Length * 23);
            Bitmap map = new Bitmap(mapwidth, 35);//创建图片背景
            Graphics graph = Graphics.FromImage(map);
            graph.Clear(Color.White);//清除画面，填充背景
                                     //graph.DrawRectangle(new Pen(Color.Silver, 0), 0, 0, map.Width - 1, map.Height - 1);//画一个边框

            Random rand = new Random();

            //验证码旋转，防止机器识别
            char[] chars = randomcode.ToCharArray();//拆散字符串成单字符数组
                                                    //文字距中
            StringFormat format = new StringFormat(StringFormatFlags.NoClip);
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            //定义颜色
            Color[] c = { Color.Black, Color.Red, Color.DarkBlue, Color.Green, Color.Orange, Color.Brown, Color.DarkCyan, Color.Purple };
            //定义字体 
            string[] font = { "Verdana", "Microsoft Sans Serif", "Comic Sans MS", "Arial", "宋体", "幼圆" };

            //画图片的背景噪音线
            for (int i = 0; i < 6; i++)
            {
                int x1 = rand.Next(10);
                int x2 = rand.Next(map.Width - 10, map.Width);
                int y1 = rand.Next(map.Height);
                int y2 = rand.Next(map.Height);

                graph.DrawLine(new Pen(c[rand.Next(7)]), x1, y1, x2, y2);
            }

            // 制造杂点 杂点面积占图片面积的 10%
            int num = map.Width * map.Height * 20 / 100;
            for (int iCount = 0; iCount < num; iCount++)
            {
                // 在随机的位置使用随机的颜色设置图片的像素
                int x = rand.Next(map.Width);
                int y = rand.Next(map.Height);
                map.SetPixel(x, y, c[rand.Next(0, c.Length - 1)]);
            }

            for (int i = 0; i < chars.Length; i++)
            {
                int cindex = rand.Next(7);
                int findex = rand.Next(5);
                Font f = new System.Drawing.Font(font[rand.Next(0, font.Length - 1)], rand.Next(18, 24), System.Drawing.FontStyle.Bold);//字体样式(参数2为字体大小)
                Brush b = new System.Drawing.SolidBrush(c[cindex]);
                Point dot = new Point(16, 20);
                float angle = rand.Next(-randAngle, randAngle);//转动的度数
                graph.TranslateTransform(dot.X, dot.Y);//移动光标到指定位置
                graph.RotateTransform(angle);
                graph.DrawString(chars[i].ToString(), f, b, 1, 1, format);
                graph.RotateTransform(-angle);//转回去
                graph.TranslateTransform(2, -dot.Y);//移动光标到指定位置
            }
            //生成图片
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.ContentType = "image/gif";
            HttpContext.Current.Response.BinaryWrite(ms.ToArray());
            graph.Dispose();
            map.Dispose();
        }

        static void CreateImageWithBg(string randomcode)
        {
            int randAngle = 40; //随机转动角度
            int mapWidth = (int)(randomcode.Length * 20);
            int mapHeight = 35;

            var bgImgName = arrBgImgs[new Random().Next(1, arrBgImgs.Length) - 1];
            var img = Image.FromFile(HttpContext.Current.Server.MapPath(string.Format("~/验证码/img/{0}", bgImgName)));
            Bitmap map = new Bitmap(img, new Size(mapWidth, mapHeight));
            Graphics graph = Graphics.FromImage(map);
            Random rand = new Random();

            //验证码旋转，防止机器识别
            char[] chars = randomcode.ToCharArray();
            //文字距中
            StringFormat format = new StringFormat(StringFormatFlags.NoClip);
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            //定义颜色
            Color[] c = { Color.Black, Color.DarkBlue, Color.Green, Color.Orange, Color.Brown, Color.DarkCyan, Color.Purple };
            //定义字体 
            string[] font = { "Verdana", "Microsoft Sans Serif", "Comic Sans MS", "Arial", "宋体", "幼圆" };

            //画图片的背景噪音线
            for (int i = 0; i < 2; i++)
            {
                int x1 = rand.Next(10);
                int x2 = rand.Next(map.Width - 10, map.Width);
                int y1 = rand.Next(map.Height);
                int y2 = rand.Next(map.Height);

                graph.DrawLine(new Pen(c[rand.Next(7)]), x1, y1, x2, y2);
            }

            // 制造杂点 杂点面积占图片面积的 1%
            int num = map.Width * map.Height * 1 / 100;
            for (int iCount = 0; iCount < num; iCount++)
            {
                // 在随机的位置使用随机的颜色设置图片的像素
                int x = rand.Next(map.Width);
                int y = rand.Next(map.Height);
                map.SetPixel(x, y, c[rand.Next(0, c.Length - 1)]);
            }

            for (int i = 0; i < chars.Length; i++)
            {
                int cindex = rand.Next(7);
                int findex = rand.Next(5);
                Font f = new System.Drawing.Font(font[rand.Next(0, font.Length - 1)], rand.Next(18, 24), System.Drawing.FontStyle.Bold);//字体样式(参数2为字体大小)
                Brush b = new System.Drawing.SolidBrush(c[cindex]);
                Point dot = new Point(16, 20);
                if (arrOperators.Contains(chars[i].ToString()))
                {
                    float angle = rand.Next(-randAngle, randAngle);//转动的度数
                    graph.TranslateTransform(dot.X, dot.Y);//移动光标到指定位置
                    graph.DrawString(chars[i].ToString(), f, b, 1, 1, format);
                    graph.TranslateTransform(2, -dot.Y);//移动光标到指定位置     
                }
                else
                {
                    float angle = rand.Next(-randAngle, randAngle);//转动的度数
                    graph.TranslateTransform(dot.X, dot.Y);//移动光标到指定位置
                    graph.RotateTransform(angle);
                    graph.DrawString(chars[i].ToString(), f, b, 1, 1, format);
                    graph.RotateTransform(-angle);//转回去
                    graph.TranslateTransform(2, -dot.Y);//移动光标到指定位置
                }
            }
            //生成图片
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            map.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.ContentType = "image/gif";
            HttpContext.Current.Response.BinaryWrite(ms.ToArray());
            graph.Dispose();
            map.Dispose();
        }

        static int GenerateRdNum(int lenth = 2)
        {
            if (lenth < 1)
            {
                lenth = 1;
            }
            var max = (int)(Math.Pow(10, lenth) - 1);
            return new Random().Next(1, max);
        }

        static readonly string[] arrOperators = new string[] { "+", "-" };
        static readonly string[] arrBgImgs = new string[] {
            "verifyimg.jpg", "verifyimg1.jpg", "verifyimg2.jpg", "verifyimg3.jpg"
        };
    }

    public enum EnumVerifyCodeType
    {
        /// <summary>
        /// 随机验证码
        /// </summary>
        code,
        /// <summary>
        /// 简单表达式
        /// </summary>
        formula
    }
}