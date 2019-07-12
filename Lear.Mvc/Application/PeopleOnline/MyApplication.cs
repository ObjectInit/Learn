using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Lear.Mvc.Application.PeopleOnline
{
    public class MyApplication: System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            Application.Lock();

            Application["dateTime"] = DateTime.Now.ToShortDateString();

            Application["ipList"] = new List<string>();

            //默认总访问记录数为0
            Application["count"] = 0;
            //默认当前在线数为0
            Application["online"] = 0;
            //将当前人数写入文件中
            WriteCountPerson(0);

            Application.UnLock();

        }
        protected void Session_Start(object sender, EventArgs e)
        {
            //临时日期和系统记录的日期对比，若不相等表示不是同一天
            string tempDate = DateTime.Now.ToShortDateString();
            string appDate = Application["dateTime"].ToString();
            if (!tempDate.Equals(appDate))
            {
                Application["dateTime"] = tempDate;
                Application["ipList"] = null;
                int countNums = ReadCountPerson();
                WriteCountPerson(countNums + int.Parse(Application["count"].ToString()));
            }

            //发起会话的客户端IP地址
            string tempIp = Context.Request.UserHostAddress;
            //设置一个会话的作用时间为一分钟，即一分钟内不做任何操作的话，该会话就会失效。
            Session.Timeout = 1;
            //用于存储客户端的IP地址集合，若没有则表示是新的一天并且实例化出集合对象
            List<string> ipList = Application["ipList"] as List<string>;
            if (ipList == null)
            {
                ipList = new List<string>();        //如果ipList集合为空那么实例化他
            }

            //读取出文件中保存的总访问人数
            int countNums_2 = ReadCountPerson();
            if (!ipList.Contains(tempIp))
            {
                //在ip集合中添加客户端IP地址
                ipList.Add(tempIp);
                Application["ipList"] = ipList;
                //总访问数在文件中保存的数据累加1
                countNums_2 += 1;
                WriteCountPerson(countNums_2);

            }
            //当前在线人数累加1
            Application["online"] = (int)Application["online"] + 1;

            Application["count"] = countNums_2;

            Application.UnLock();
        }

        protected void Session_End(object sender, EventArgs e)
        {
            Application.Lock();

            Session.Abandon();                                            //当以一个会话结束后，注销该会话

            int online = int.Parse(Application["online"].ToString());
            if (online <= 0)
            {
                Application["online"] = 0;
            }
            else
            {
                Application["online"] = (int)Application["online"] - 1;
            }

            Application.UnLock();
        }

        /// <summary>
        /// 写入网页总访问人数
        /// </summary>
        /// <param name="nums"></param>
        public void WriteCountPerson(int nums)
        {
            string filePath = System.Web.HttpRuntime.AppDomainAppPath + "ConfigFiles";
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            filePath += "\\countPersonNums.txt";
            if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }
            StreamWriter sw = new StreamWriter(filePath, false);
            sw.WriteLine("访问总数为:" + nums);
            sw.Flush();
            sw.Close();
        }

        /// <summary>
        /// 读取网页总访问人数
        /// </summary>
        public int ReadCountPerson()
        {
            try
            {
                int nums = 0;
                string filePath = System.Web.HttpRuntime.AppDomainAppPath + "ConfigFiles\\countPersonNums.txt";
                if (!File.Exists(filePath))
                {
                    return 0;
                }
                FileStream fs = new FileStream(filePath, FileMode.Open);
                StreamReader streamReader = new StreamReader(fs);
                string strLine = streamReader.ReadLine();
                string[] split = strLine.Split(':');
                if (split.Length <= 1)
                {
                    return 0;
                }
                int.TryParse(split[1], out nums);
                fs.Flush();
                fs.Close();
                streamReader.Close();
                streamReader.Dispose();
                return nums;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}