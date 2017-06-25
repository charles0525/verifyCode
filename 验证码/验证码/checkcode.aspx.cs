using System;
using System.Drawing;

public partial class checkcode : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string code = GenerateCheckCode(4);
        Session["Verification"] = code;  //Session中保存验证码
        CreateImage(code.ToUpper());
    }

    /// <summary>
    /// 生成随机字符串
    /// </summary>
    /// <param name="codelength">字符串长度</param>
    /// <returns></returns>
    public string GenerateCheckCode(int codelength)
    {
        int number;
        string RandomCode = string.Empty;
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

            RandomCode += ((char)number).ToString();
        }
        return RandomCode;
    }

    ///  <summary>
    ///  创建随机码图片
    ///  </summary>
    ///  <param  name="randomcode">随机码</param>
    private void CreateImage(string randomcode)
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
            map.SetPixel(x, y, c[rand.Next(0,c.Length-1)]);
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
        Response.ClearContent();
        Response.ContentType = "image/gif";
        Response.BinaryWrite(ms.ToArray());
        graph.Dispose();
        map.Dispose();
    }
}