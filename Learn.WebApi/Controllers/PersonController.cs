
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;

// ReSharper disable once InvalidXmlDocComment
/// <summary>
/// 参照 https://blog.csdn.net/lwpoor123/article/details/78285148
/// </summary>
namespace Learn.WebApi.Controllers
{
    public class PersonController : ApiController
    {
        private ApiTools tool = new ApiTools();
        

        [HttpPost]
        public HttpResponseMessage CheckUserName([FromBody]string userName)
        {
            //string content = Request.Content.ReadAsAsync<string>().Result;
                return tool.MsgFormat(ResponseCode.成功, "可注册", "0 " + userName);

        }

        public class ApiTools
        {
            private string msgModel = "{{\"code\":{0},\"message\":\"{1}\",\"result\":{2}}}";
            public ApiTools()
            {
            }
            public HttpResponseMessage MsgFormat(ResponseCode code, string explanation, string result)
            {
                string r = @"^(\-|\+)?\d+(\.\d+)?$";
                string json = string.Empty;
                if (Regex.IsMatch(result, r) || result.ToLower() == "true" || result.ToLower() == "false" || result == "[]" || result.Contains('{'))
                {
                    json = string.Format(msgModel, (int)code, explanation, result);
                }
                else
                {
                    if (result.Contains('"'))
                    {
                        json = string.Format(msgModel, (int)code, explanation, result);
                    }
                    else
                    {
                        json = string.Format(msgModel, (int)code, explanation, "\"" + result + "\"");
                    }
                }
                return new HttpResponseMessage { Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json") };
            }
        }
        public enum ResponseCode
        {
            操作失败 = 00000,
            成功 = 10200,
        }
    }
}