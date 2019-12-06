#region  <<版本注释>>
/* ========================================================== 
// <copyright file="User.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：User 
* 创 建 者：Administrator 
* 创建时间：2019/12/6 13:24:30 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Console.RestSharp.RestFulService
{

    [DataContract]
    public class User
    {
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Age { get; set; }

        [DataMember]
        public int Score { get; set; }
    }
}
