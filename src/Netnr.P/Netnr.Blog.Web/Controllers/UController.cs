using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Netnr.Blog.Web.Controllers
{
    /// <summary>
    /// 用户主页
    /// </summary>
    public class UController : Controller
    {
        /// <summary>
        /// 我的主页
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var un = RouteData.Values["id"]?.ToString();

            using (var db = new Data.ContextBase())
            {
                var usermo = db.UserInfo.FirstOrDefault(x => x.UserName == un);
                if (usermo != null)
                {
                    return View("_PartialU", usermo);
                }
            }

            return Content("Invalid");
        }
    }
}