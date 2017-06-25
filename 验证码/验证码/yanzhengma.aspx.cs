using System;
using System.Collections;
using System.Configuration;
using System.Data;

using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using System.Drawing;


public partial class Default2 : System.Web.UI.Page
{
    /// <summary>
    /// 验证码产生程序
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void Page_Load(object sender, EventArgs e)
    {
        GetCode();
    }

    //产生code
    protected void GetCode()
    {
        System.Random rand = new Random();
        int len = 5;    //这里设置验证码长度，我的定义为4，最近见到些网上上有随机4，5，6位的。这里自己写个随机函数就可以了。
        char[] chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        System.Text.StringBuilder myStr = new System.Text.StringBuilder();

        for (int iCount = 0; iCount < len; iCount++)
        {
            myStr.Append(chars[rand.Next(chars.Length)]);
        }
        string text = myStr.ToString();
        // 保存验证码到 session 中以便其他模块使用
        Session["code"] = text;   //这里你的登陆页面判断输入验证码是否正确，if (txtCode.text == Session["code"]) 正确读库检索User/Password,不正确弹窗。自己写
        Size ImageSize = Size.Empty;
        Font myFont = new Font("MS Sans Serif", 12, FontStyle.Bold);  //绘制在验证图片上的文字大小MS Sans Serif ,Verdana ,Roman
        // 计算验证码图片大小
        using (Bitmap bmp = new Bitmap(5, 5))
        {
            using (Graphics g = Graphics.FromImage(bmp))
            {
                SizeF size = g.MeasureString(text, myFont, 10000);
                ImageSize.Width = (int)size.Width + 3;
                ImageSize.Height = (int)size.Height + 3;
            }
        }
        // 创建验证码图片
        using (Bitmap bmp = new Bitmap(ImageSize.Width, ImageSize.Height))
        {
            // 绘制验证码文本
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);
                using (StringFormat f = new StringFormat())
                {
                    f.Alignment = StringAlignment.Near;
                    f.LineAlignment = StringAlignment.Center;
                    f.FormatFlags = StringFormatFlags.NoWrap;
                    g.DrawString(text, myFont, Brushes.Black, new RectangleF(0, 0, ImageSize.Width, ImageSize.Height), f);
                }//using
            }//using
            // 制造杂点 杂点面积占图片面积的 20%
            int num = ImageSize.Width * ImageSize.Height * 30 / 100;
            for (int iCount = 0; iCount < num; iCount++)
            {
                // 在随机的位置使用随机的颜色设置图片的像素
                int x = rand.Next(ImageSize.Width);
                int y = rand.Next(ImageSize.Height);
                int r = rand.Next(255);
                int g = rand.Next(255);
                int b = rand.Next(255);
                Color c = Color.FromArgb(r, g, b);
                bmp.SetPixel(x, y, c);
            }//for
            // 输出图片
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            this.Response.ContentType = "image/png";
            ms.WriteTo(this.Response.OutputStream);
            ms.Close();
        }//using
        myFont.Dispose();
    }
}
