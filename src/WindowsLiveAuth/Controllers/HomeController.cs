using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using WindowsLiveAuth.Models;

namespace WindowsLiveAuth.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 你申請的 client_id
        /// </summary>
        private const string client_id = "{client_id}";
        /// <summary>
        /// 你申請的 client_secret
        /// </summary>
        private const string client_secret = "{client_id}";
        /// <summary>
        /// 申請時候設定的回傳網址
        /// </summary>
        private const string redirect_uri = "{redirect_uri}";

        public ActionResult Index()
        {
            string Url = "https://oauth.live.com/authorize?scope={0}&redirect_uri={1}&response_type={2}&client_id={3}";
            // wl.photos 是存取相簿的權限。scope 若有多個，可以使用半型逗號(,)做分隔。
            // 所有權限列表可以到這邊查詢：
            // http://msdn.microsoft.com/en-us/library/hh243646.aspx
            string scope = "wl.photos,wl.offline_access";
            string redirect_uri_encode = Utitity.UrlEncode(redirect_uri);
            string response_type = "code";

            Response.Redirect(string.Format(Url, scope, redirect_uri_encode, response_type, client_id));

            return null;
        }

        public ActionResult CallBack(string Code)
        {
            // 沒有接收到參數
            if (string.IsNullOrEmpty(Code))
                return Content("沒有收到 Code");

            string Url = "https://oauth.live.com/token?code={0}&client_id={1}&client_secret={2}&redirect_uri={3}&grant_type={4}";
            string grant_type = "authorization_code";
            string redirect_uri_encode = Utitity.UrlEncode(redirect_uri);

            HttpWebRequest request = HttpWebRequest.Create(string.Format(Url, Code, client_id, client_secret, redirect_uri_encode, grant_type)) as HttpWebRequest;
            string result = null;
            request.Method = "GET";    // 方法
            request.KeepAlive = true; //是否保持連線
            request.ContentType = "application/x-www-form-urlencoded";

            using (WebResponse response = request.GetResponse())
            {
                StreamReader sr = new StreamReader(response.GetResponseStream());
                result = sr.ReadToEnd();
                sr.Close();
            }

            TokenData tokenData = JsonConvert.DeserializeObject<TokenData>(result);
            Session["token"] = tokenData.access_token;

            // 這邊不建議直接把 Token 當做參數傳給 CallAPI 可以避免 Token 洩漏
            return RedirectToAction("CallAPI");
        }

        public ActionResult CallAPI()
        {
            if (Session["token"] == null)
                return Content("請先取得授權！");

            string token = Session["token"] as string;
            // 取得相簿列表的 API 網址
            string Url = "https://apis.live.net/v5.0/me/albums?access_token=" + token;
            HttpWebRequest request = HttpWebRequest.Create(Url) as HttpWebRequest;
            string result = null;
            request.Method = "GET";    // 方法
            request.KeepAlive = true; //是否保持連線

            using (WebResponse response = request.GetResponse())
            {
                StreamReader sr = new StreamReader(response.GetResponseStream());
                result = sr.ReadToEnd();
                sr.Close();
            }

            Response.Write(result);

            return null;
        }
    }
}
