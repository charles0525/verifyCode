using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace 验证码.验证码
{
    public partial class VerifyCode : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            VerifyCodeHelper.GenerateImgCode(4);
        }
    }
}