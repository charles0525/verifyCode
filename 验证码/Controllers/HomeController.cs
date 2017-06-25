using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using 验证码;

namespace 验证码.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Check(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return new JsonResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { msg = "请输入验证码" } };
            }
            string msg = "";
            if (VerifyCodeHelper.CheckCode(code))
            {
                msg = "验证成功";
            }
            else
            {
                msg = "验证失败";
            }
            return new JsonResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { msg = msg } };
        }
    }
}