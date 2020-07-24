#region  <<版本注释>>
/* ==========================================================
// <copyright file="VoucherHelper.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：收/付款的帮助类
* 创 建 者：曾倩倩
* 创建时间：2019/10/15 20:54:13
* =============================================================*/
#endregion

using Newtonsoft.Json;
using SJ.BackEnd.Base;
using SJ.BackEnd.Biz.Pub;
using SJ.Global;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 收/付款的帮助类
    /// 1 描述：无
    /// 2 约定：无
    /// 3 业务逻辑：
    ///     1.计算设置汇率、原币金额、单价、数量、金额、借贷
    ///         如果是自动计算金额，单价、汇率优先级高
    ///         如果是用户输入对方科目金额，数量、原币优先级高
    ///     2.添加时，初始化单据头的数据
    ///     3.添加时，初始化单据体的数据
    ///         1、存在父凭证的单据体，数量、摘要、汇率等属性与父凭证的属性一致
    ///         2、对方科目单据体，设置业务七日、到期日期的缺省值
    ///         3、计量单位始终不允许修改，界面上动态得到科目的计量单位
    ///         4、状态、核算期、数据类型等属性与单据头的属性一致
    ///         5、计算设置汇率、原币金额、单价、数量、金额、借贷
    ///     4.修改时，初始化单据体的数据，作为校验数据的依据
    ///     5.收款/付款时，校验是否允许收款/付款
    ///         判断实收/实付金额是否合法
    ///         判断凭证分录是否合法
    ///     6.校验选择收款/付款的凭证分录列表
    ///     7.修改时，全面校验对方科目单据体
    ///     8.修改存在父凭证的单据体时，简单校验是否允许收款/付款
    ///         判断实收/实付金额是否合法
    ///         判断凭证分录是否合法
    ///     9.校验凭证分录的合法性
    ///     10.将字符串转换成数值格式
    ///     11.得到指定核算期的上一个核算期
    ///     12.得到指定字段最大的值
    ///     13.系统自动生成银行对账单科目
    /// </summary>
    public class VoucherHelper
    {
        /// <summary>
        /// 计算设置汇率、原币金额、单价、数量、金额
        /// 如果是自动计算金额，单价、汇率优先级高
        /// 如果是用户输入对方科目金额，数量、原币优先级高
        /// </summary>
        /// <param name="data">单据体实体</param>
        /// <param name="apiTask">访问api</param>
        /// <param name="isUserIn">是否用户输入金额</param>
        /// <param name="subjectEntity">科目</param>
        public static void SetExchangeRateAndAmount(SData data, ApiTask apiTask, bool isUserIn, SData subjectEntity)
        {
            decimal.TryParse(data["amountd", "0"]?.ToString(), out decimal debitAmount); // 借方金额
            decimal.TryParse(data["amountc", "0"]?.ToString(), out decimal creditAmount); // 贷方金额
            var amount = debitAmount != 0 ? debitAmount : creditAmount; // 金额
            if (amount != 0)
            {
                // 金额=借-贷
                decimal amountc = debitAmount - creditAmount;
                data["amount"] = amountc;

                if (subjectEntity != null)
                {
                    // 判断科目核定数量
                    if (subjectEntity.Item<bool>("isqty"))
                    {
                        // 数量
                        decimal.TryParse(data["qty", "0"]?.ToString(), out decimal qty);

                        // 单价
                        decimal.TryParse(data["price", "0"]?.ToString(), out decimal price);

                        if (qty != 0 || price != 0)
                        {
                            // *用户输入对方科目金额，数量、原币优先级高
                            if (isUserIn)
                            {
                                // 判断金额（借或贷）、单价有值，数量为空，则数量自动计算
                                if (price != 0 && qty == 0)
                                {
                                    // 按照（数量=金额/单价）公式，设置数量值
                                    data["qty"] = (amount / price).ToString().ToDec();
                                }

                                // 判断金额（借或贷）、数量、单价有值，则单价自动计算
                                if (qty != 0)
                                {
                                    // 按照(单价=金额/数量）公式，设置单价
                                    data["price"] = (amount / qty).ToString().ToDec();
                                }
                            }
                            else // *自动计算金额，单价、汇率优先级高
                            {
                                // 判断金额（借或贷）、数量、单价有值，则数量自动计算
                                if (price != 0)
                                {
                                    // 按照（数量=金额/单价）公式，设置数量值
                                    data["qty"] = (amount / price).ToString().ToDec();
                                }
                            }

                            // 数量
                            decimal.TryParse(data["qty"]?.ToString(), out qty);

                            // 单价
                            decimal.TryParse(data["price"]?.ToString(), out price);

                            // 单价为正值，如果金额（在贷方且为正值）或（在借方且为负值），数量为负值
                            data["price"] = Math.Abs(price);
                            qty = Math.Abs(qty);
                            if (creditAmount > 0 || debitAmount < 0)
                            {
                                qty = -qty;
                            }

                            data["qty"] = qty;
                        }
                    }

                    // 判断科目启用外币
                    if (subjectEntity.Item<bool>("iscurrency"))
                    {
                        // 汇率
                        decimal.TryParse(data["crate", "0"]?.ToString(), out decimal crate);

                        // 原币金额
                        decimal.TryParse(data["camount", "0"]?.ToString(), out decimal camount);

                        if (crate != 0 || camount != 0)
                        {
                            // *用户输入对方科目金额，数量、原币优先级高
                            if (isUserIn)
                            {
                                // 判断金额（借或贷）、汇率有值，原币金额为空，则原币金额自动计算
                                if (crate != 0 && camount == 0)
                                {
                                    // 按照（原币金额=金额/汇率）公式，设置原币金额
                                    data["camount"] = (amount / crate).ToString().ToDec();
                                }

                                // 判断金额（借或贷）、原币金额、汇率有值，则汇率自动计算
                                if (camount != 0)
                                {
                                    // 按照（汇率=金额/原本金额）公式，设置汇率值
                                    data["crate"] = (amount / camount).ToString().ToDec();
                                }
                            }
                            else // *自动计算金额，单价、汇率优先级高
                            {
                                // 判断金额（借或贷）、原币金额、汇率有值，则原币金额自动计算
                                if (crate != 0)
                                {
                                    // 按照（原币金额=金额/汇率）公式，设置原币金额
                                    data["camount"] = (amount / crate).ToString().ToDec();
                                }
                            }

                            // 汇率
                            decimal.TryParse(data["crate"]?.ToString(), out crate);

                            // 原币金额
                            decimal.TryParse(data["camount"]?.ToString(), out camount);

                            // 汇率为正值，如果金额（在贷方且为正值）或（在借方且为负值），原币金额为负值
                            data["crate"] = Math.Abs(crate);
                            camount = Math.Abs(camount);
                            if (creditAmount > 0 || debitAmount < 0)
                            {
                                camount = -camount;
                            }

                            data["camount"] = camount;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 添加时，初始化单据头的数据
        /// </summary>
        /// <param name="data">单据头对象</param>
        /// <param name="apiTask">访问api</param>
        /// <param name="bizName">对象名</param>
        public static void SetInitData(SData data, ApiTask apiTask, string bizName)
        {
            data["sumd"] = 0; // 借方合计：0
            data["sumc"] = 0; // 贷方合计：0
            data["poster"] = null; // 记账人：空
            data["posttime"] = null; // 记账时间：空
            data["title"] = bizName;
            data["vstate"] = AcConst.Draft; // 状态：草稿
            data["vclass"] = AcConst.ActData; // 数据类型：实际数据
            data["mdate"] = DateTime.Now.ToString("yyyy-MM-dd"); // 制单日期：当天日期
            data["bdate"] = DateTime.Now.ToString("yyyy-MM-dd"); // 业务日期：当天日期
            data["maker"] = $"[{apiTask.UserInfo().UserID()}]"; // 制单人：当前登录用户
            data["dcode"] = AcVoucherHelper.GetPayReceiptNumber(apiTask); // 设置编号的缺省值
        }

        /// <summary>
        /// 添加时，初始化单据体的数据
        /// 1、存在父凭证的单据体，数量、摘要、汇率等属性与父凭证的属性一致
        /// 2、对方科目单据体，设置业务七日、到期日期的缺省值
        /// 3、计量单位始终不允许修改，界面上动态得到科目的计量单位
        /// 4、状态、核算期、数据类型等属性与单据头的属性一致
        /// 5、计算设置汇率、原币金额、单价、数量、金额、借贷
        /// </summary>
        /// <param name="dData">单据体对象</param>
        /// <param name="data">单据头对象</param>
        /// <param name="voucher">父凭证对象</param>
        /// <param name="account">科目对象</param>
        /// <param name="apiTask">访问api</param>
        /// <param name="isUserIn">是否用户输入</param>
        public static void SetInitData(SData dData, SData data, SData voucher, SData account, ApiTask apiTask, bool isUserIn)
        {
            // 业务逻辑 **********
            if (data != null)
            {
                // * 存在父凭证的单据体，数量、摘要、汇率等属性与父凭证的属性一致
                if (!string.IsNullOrEmpty(dData.Item<string>("parent")))
                {
                    if (voucher != null)
                    {
                        dData["qty"] = voucher["qty"]; // 数量
                        dData["ddesc"] = voucher["ddesc"]; // 摘要
                        dData["crate"] = voucher["crate"]; // 汇率
                        dData["price"] = voucher["price"]; // 单价
                        dData["bdate"] = voucher["bdate"]; // 业务日期
                        dData["edate"] = voucher["edate"]; // 到期日期
                        dData["isori"] = voucher["isori"]; // 原始标记
                        dData["bsign"] = voucher["bsign"]; // 备用标志
                        dData["camount"] = voucher["camount"]; // 原币
                        dData["reference"] = voucher["reference"]; // 业务参考
                        dData["currency"] = $"[{voucher["currency.id"]}]"; // 币种
                        dData["amountp"] = dData["cd"].ToString() == AcConst.Debit.ToString() ? dData["amountd"] : dData["amountc"]; // 提审金额为借/贷方金额（本次实收/实付金额）

                        // t0-t28,a0-a9,s0-s9,d1-d5
                        for (int i = 0; i < 29; i++)
                        {
                            // 忽略t29科目对账
                            string key = "t" + i;
                            dData[key] = voucher[key];

                            if (i <= 9)
                            {
                                key = "a" + i;
                                dData[key] = voucher[key];

                                key = "s" + i;
                                dData[key] = voucher[key];
                            }

                            if (i <= 5)
                            {
                                // 忽略d0提审金额
                                if (i > 0)
                                {
                                    key = "d" + i;
                                    dData[key] = voucher[key];
                                }
                            }
                        }
                    }
                }
                else // * 对方科目单据体，设置业务日期、到期日期的缺省值
                {
                    dData["bdate"] = string.IsNullOrEmpty(dData.Item<string>("bdate")) ? DateTime.Now.ToString("yyyy-MM-dd") : dData["bdate"];
                    dData["edate"] = string.IsNullOrEmpty(dData.Item<string>("edate")) ? DateTime.Now.ToString("yyyy-MM-dd") : dData["edate"];
                }

                // * 计量单位始终不允许修改，界面上动态得到科目的计量单位
                dData["unitname"] = string.Empty;

                // * 状态、核算期、数据类型等属性与单据头的属性一致
                dData["period"] = data["period"]; // 核算期
                dData["vstate"] = $"[{data["vstate.id"]}]"; // 状态
                dData["vclass"] = $"[{data["vclass.id"]}]"; // 数据类型
                dData[AcConst.RecoTcode] = $"[{data["subrec.id"]}]"; // 科目对账
                dData["SubUnit"] = $"[{data["SubUnit.id"]}]"; // 核算单位
                dData["vsubstate"] = $"[{data["vsubstate.id"]}]"; // 子状态
                dData["vh.id"] = data["id"];

                // * 计算设置汇率、原币金额、单价、数量、金额
                SetExchangeRateAndAmount(dData, apiTask, isUserIn, account);
            }
        }

        /// <summary>
        /// 修改时，初始化单据体的数据，作为校验数据的依据
        /// </summary>
        /// <param name="oldData">实体原始数据</param>
        /// <param name="updateData">实体修改的数据</param>
        public static void SetUpdateData(SData oldData, SData updateData)
        {
            // 数据准备 **********
            // 变量声明
            // 修改的数据中不包含的，原来的数据有值的
            List<string> oldList = new List<string>() { "amountc", "amountd", "parent", "vh.id", "cd", "bdate", "edate", "currency" };

            // 修改的数据中不包含的，原来的数据有值且不等于0的
            List<string> oldDecimalList = new List<string>() { "crate", "camount", "qty", "price" };
            for (int i = 0; i < 30; i++)
            {
                string item = "t" + i;
                oldDecimalList.Add(item);
            }

            // 业务逻辑 **********
            // * 赋值，作为校验数据的依据
            foreach (var item in oldList)
            {
                if (!updateData.ContainsKey(item))
                {
                    updateData[item] = oldData[item];
                }
            }

            foreach (var item in oldDecimalList)
            {
                if (!updateData.ContainsKey(item) && oldData[item] != null)
                {
                    decimal.TryParse(oldData[item].ToString(), out decimal old);
                    if (old != 0)
                    {
                        updateData[item] = updateData.ContainsKey(item) ? updateData[item] : old;
                    }
                }
            }

            // 设置日期
            updateData["bdate"] = string.IsNullOrEmpty(updateData.Item<string>("bdate")) ? DateTime.Now.ToString("yyyy-MM-dd") : updateData["bdate"];
            updateData["edate"] = string.IsNullOrEmpty(updateData.Item<string>("edate")) ? DateTime.Now.ToString("yyyy-MM-dd") : updateData["edate"];
        }

        /// <summary>
        /// 收款/付款时，校验是否允许收款/付款
        /// 1、判断实收/实付金额是否合法
        /// 2、判断凭证分录是否合法
        /// </summary>
        /// <param name="data">单据头实体数据</param>
        /// <param name="bizTable">单据体</param>
        /// <param name="isPay">true：付款，false：收款</param>
        /// <param name="isDeserial">是否需要转换json</param>
        public static void CheckCanPayReceipt(SData data, BizTable bizTable, bool isPay, bool isDeserial)
        {
            // 数据准备 **********
            // 变量声明
            // 默认为验证付款数据合法性
            string amountStr = "amountc";
            string mesg = "付款";
            string amountMesg = "实付";
            string billType = AcConst.PayVoucher;
            string cd = "借方";

            // 验证收款数据合法性
            if (!isPay)
            {
                amountStr = "amountd";
                mesg = "收款";
                amountMesg = "实收";
                billType = AcConst.ReceiptVoucher;
                cd = "贷方";
            }

            // 因为是作为查询参数传入，不能直接传递list<sdata>，需要利用json转换
            List<SData> dList = null;
            if (isDeserial)
            {
                if (data["dList"] != null)
                {
                    try
                    {
                        dList = JsonConvert.DeserializeObject<List<SData>>(data["dList"].ToString());
                    }
                    catch
                    {
                        throw new Exception(bizTable.ApiTask.L(amountMesg + "金额") + bizTable.ApiTask.LEnd("为数值"));
                    }
                }
            }
            else
            {
                dList = data["dList"] as List<SData>;
            }

            // 业务逻辑 **********
            // * 无收款/付款凭证分录数据：无收款/付款记录
            if (dList == null || dList.Count <= 0)
            {
                throw new Exception(bizTable.ApiTask.LEnd("无" + mesg + "记录"));
            }

            CheckDList(dList, amountStr, mesg, amountMesg, bizTable, data["subrec"].ToString(), billType, cd, out string subUnit, out SData ids);

            // * 设置单据头的核算单位为单据体的核算单位
            data["subunit"] = $"[{subUnit}]"; // 核算单位
            data["dids"] = ids;
        }

        /// <summary>
        /// 校验选择收款/付款的凭证分录列表
        /// </summary>
        /// <param name="dList">凭证分录列表</param>
        /// <param name="amountStr">金额字段</param>
        /// <param name="mesg">提示字段</param>
        /// <param name="amountMesg">金额提示字段</param>
        /// <param name="bizTable">对象</param>
        /// <param name="subRec">科目对账</param>
        /// <param name="billType">单据类型</param>
        /// <param name="cd">借贷</param>
        /// <param name="subUnit">核算单位</param>
        /// <param name="ids">凭证分录id</param>
        public static void CheckDList(List<SData> dList, string amountStr, string mesg, string amountMesg, BizTable bizTable, string subRec, string billType, string cd, out string subUnit, out SData ids)
        {
            subUnit = "0";
            ids = new SData();
            int index = 0;
            decimal sum = 0;
            foreach (var item in dList)
            {
                // * 判断实收/实付金额是否合法
                // 实收/实付金额校验（不能为空，不能为0，是数值）
                AcVoucherHelper.CheckDecimal(bizTable.ApiTask, item.Item<string>(amountStr), bizTable.ApiTask.L(amountMesg + "金额"));
                sum += item[amountStr].ToString().ToDec();

                // * 判断选择收款/付款的凭证分录是否合法
                // 如果选择付款的凭证分录不存在，抛出异常
                if (string.IsNullOrEmpty(item.Item<string>("parent")))
                {
                    throw new Exception(bizTable.ApiTask.L(mesg + "凭证分录") + bizTable.ApiTask.LEnd(PubLang.NotExists));
                }

                index++;
                double numKey = index / 500.0;
                ids[Math.Ceiling(numKey).ToString()] += "," + item["parent"];
            }

            // 实收/实付金额校验（不能为空，不能为0，是数值）
            AcVoucherHelper.CheckDecimal(bizTable.ApiTask, sum.ToString(), bizTable.ApiTask.L(amountMesg + "金额"));

            // 数据准备 **********
            // 变量声明
            // 用于校验凭证分录的科目属于对账科目
            var reconRefBiz = bizTable.ApiTask.Biz<BizTable>("AcReconRef"); // 科目对账和科目关系的对象
            var accountIds = reconRefBiz.GetListData(0, 1, new SData("subrec", subRec).toParmStr(), "account.id,billtype.dcode"); // 科目对账中科目id、单据类型列表

            // 用于校验凭证分录的核算单位、分析项是否相同
            var tcodeList = AcVoucherHelper.GetRecTcodes(subRec, bizTable.ApiTask); // 科目对账中启用的tcode(parm);

            // 当前用户的核算单位
            string subunit = bizTable.ApiTask.UserInfo().UserSubUnit();
            if (!string.IsNullOrEmpty(subunit))
            {
                subUnit = bizTable.ApiTask.Biz<BizTableCode>("AcSubunit").GetIDAuto(subunit).ToString();
            }

            SData tcodes = new SData();
            index = 0;

            // 得到凭证分录列表
            var vSubBiz = bizTable.ApiTask.Biz<BizTable>("AcVoucherD"); // 凭证分录对象
            var voucherList = new List<SData>();
            foreach (var item in ids.Keys)
            {
                string id = ids.Item<string>(item);
                if (!string.IsNullOrEmpty(id))
                {
                    id = id.TrimStart(',');
                    if (!string.IsNullOrEmpty(id))
                    {
                        var itemList = vSubBiz.GetListData(0, 1, new SData("id", id).toParmStr(), "id,vstate,account.id,vclass,subunit.id," + amountStr + "," + AcVoucherHelper.TcodeStr);
                        if (itemList != null)
                        {
                            if (itemList.Count > 0)
                            {
                                voucherList.AddRange(itemList);
                            }
                        }
                    }
                }
            }

            // 业务逻辑 **********
            foreach (var item in dList)
            {
                // 校验凭证分录是否合法
                var voucher = voucherList.Find(a => a["id"].ToString() == item["parent"].ToString());
                CheckVoucherD(voucher, mesg, amountStr, cd, accountIds, billType, bizTable);

                string nowSubUnit = voucher.Item<string>("subunit.id");
                if (string.IsNullOrEmpty(nowSubUnit) || (subUnit != nowSubUnit && subUnit != "0"))
                {
                    throw new Exception(bizTable.ApiTask.L("核算单位") + bizTable.ApiTask.LEnd("不相同"));
                }

                foreach (var temp in tcodeList)
                {
                    var tcode = voucher.Item<string>(temp);
                    if (string.IsNullOrEmpty(tcode) || (tcodes.Item<string>(temp) != tcode && index > 0))
                    {
                        throw new Exception(bizTable.ApiTask.L("分析项") + bizTable.ApiTask.LEnd("不相同"));
                    }

                    tcodes[temp] = tcode;
                }

                subUnit = nowSubUnit;
                index++;
            }
        }

        /// <summary>
        /// 修改时，全面校验对方科目单据体
        /// 1、如果科目没启用：科目已禁用，不允许使用
        /// 2、计算设置汇率、原币金额、单价、数量、金额
        /// 3、科目的数量为启用时，判断数量是否合法
        /// 4、科目的外币为启用时，判断币种、汇率是否合法
        /// 5、判断单据体中 科目、类定义启用的tcode属性值是否合法
        /// </summary>
        /// <param name="entity">对方科目单据体</param>
        /// <param name="subject">科目</param>
        /// <param name="listField">字段定义</param>
        /// <param name="bizTable">单据体</param>
        public static void CheckAll(SData entity, SData subject, SData listField, BizTable bizTable)
        {
            // 数据准备 **********
            // 变量声明
            if (subject != null)
            {
                // 业务逻辑 **********
                // * 如果科目没启用，抛出异常
                if (!subject.Item<bool>("isenable"))
                {
                    throw new Exception(bizTable.ApiTask.LEnd("科目已禁用，不允许使用"));
                }

                // * 计算设置汇率、原币金额、单价、数量、金额
                SetExchangeRateAndAmount(entity, bizTable.ApiTask, true, subject);

                // * 科目的数量为启用时，判断数量是否合法
                if (subject.Item<bool>("isqty"))
                {
                    // 数量校验（不能为空，不能为0，是数值）
                    AcVoucherHelper.CheckDecimal(bizTable.ApiTask, entity.Item<string>("qty"), ((FieldDefine)listField["qty"]).DisplayName);
                }

                // * 科目的币种为启用时，判断币种、汇率是否合法
                if (subject.Item<bool>("iscurrency"))
                {
                    // 如果科目指定了币种，数据库保存科目的币种
                    if (!string.IsNullOrEmpty(subject.Item<string>("currency.id")))
                    {
                        entity["currency"] = $"[{ subject.Item<string>("currency.id")}]";
                    }

                    // 币种为空时，抛出异常
                    bizTable.CheckEmptyOrNull(entity, true, "currency");

                    // 汇率校验（不能为空，不能为0，是数值）
                    AcVoucherHelper.CheckDecimal(bizTable.ApiTask, entity.Item<string>("crate"), ((FieldDefine)listField["crate"]).DisplayName);

                    // 币种不存在，抛出异常
                    bizTable.CheckRefExist(entity["currency"].ToString(), (FieldDefine)listField["currency"]);
                }

                // * 判断单据体中 科目、类定义启用的tcode属性值是否合法
                AcVoucherHelper.CheckTcode(bizTable.ApiTask, subject, entity);
            }
        }

        /// <summary>
        /// 修改存在父凭证的单据体时，简单校验是否允许收款/付款
        /// 1、判断实收/实付金额是否合法
        /// 2、判断凭证分录是否合法
        /// </summary>
        /// <param name="oldDic">实体原始数据</param>
        /// <param name="updateDic">实体修改的数据</param>
        /// <param name="bizTable">单据体</param>
        /// <param name="isPay">true：付款，false：收款</param>
        public static void CheckCanPayReceipt(SData oldDic, SData updateDic, BizTable bizTable, bool isPay)
        {
            // 数据准备 **********
            // 变量声明
            string subRec = oldDic[AcConst.RecoTcode, string.Empty].ToString();

            // 默认为验证付款数据合法性
            string amountStr = "amountc";
            string mesg = "付款";
            string amountMesg = "实付";
            string cd = "借方";
            string billType = AcConst.PayVoucher;

            // 验证收款数据合法性
            if (!isPay)
            {
                amountStr = "amountd";
                mesg = "收款";
                amountMesg = "实收";
                billType = AcConst.ReceiptVoucher;
                cd = "贷方";
            }

            // 业务逻辑 **********
            // * 判断实收/实付金额是否合法
            // 实收/实付金额校验（不能为空，不能为0，是数值）
            AcVoucherHelper.CheckDecimal(bizTable.ApiTask, updateDic.Item<string>(amountStr), bizTable.ApiTask.L(amountMesg + "金额"));

            // * 判断凭证分录是否合法
            // 查询凭证体信息
            var vSubBiz = bizTable.ApiTask.Biz<BizTable>("AcVoucherD");
            var voucher = vSubBiz.GetItemByParms(new SData("id", oldDic["parent"]).toParmStr(), "vstate,account.id,vclass,account.isqty,account.iscurrency," + amountStr);
            var reconRefBiz = bizTable.ApiTask.Biz<BizTable>("AcReconRef");
            var accountIds = reconRefBiz.GetListData(0, 1, new SData("subrec", subRec).toParmStr(), "account.id,billtype.dcode"); // 科目对账中科目id、单据类型列表
            CheckVoucherD(voucher, mesg, amountStr, cd, accountIds, billType, bizTable);

            var ledgerBiz = bizTable.ApiTask.Biz<BizTable>("AcLedgersSub");
            var vList = ledgerBiz.GetListData(0, 1, new SData("payreceipt", oldDic["vh.id"]).toParmStr(), "subunit," + AcVoucherHelper.TcodeStr);

            // 核算单位是否相同，抛出异常
            if (!AcVoucherHelper.CheckSubUnitIsSame(vList))
            {
                throw new Exception(bizTable.ApiTask.L("核算单位") + bizTable.ApiTask.LEnd("不相同"));
            }

            // 判断最小分析项是否相同（不包括对方科目），抛出异常
            var tcodeList = AcVoucherHelper.GetRecTcodes(subRec, bizTable.ApiTask); // 科目对账中启用的tcode
            foreach (var tcode in tcodeList)
            {
                if (vList.GroupBy(m => m[tcode]).Count() > 1 ||
                    string.IsNullOrEmpty(vList.FirstOrDefault().Item<string>(tcode)))
                {
                    throw new Exception(bizTable.ApiTask.L("分析项") + bizTable.ApiTask.LEnd("不相同"));
                }
            }

            // * 计算设置汇率、原币金额、单价、数量、金额
            SData subject = null;
            if (!string.IsNullOrEmpty(voucher.Item<string>("account.isqty")))
            {
                if (!string.IsNullOrEmpty(voucher.Item<string>("account.iscurrency")))
                {
                    subject = new SData("isqty", voucher["account.isqty"], "iscurrency", voucher["account.iscurrency"]);
                }
            }

            SetExchangeRateAndAmount(updateDic, bizTable.ApiTask, true, subject);
        }

        /// <summary>
        /// 校验凭证分录的合法性
        /// </summary>
        /// <param name="voucher">凭证分录</param>
        /// <param name="mesg">收款/付款</param>
        /// <param name="amountStr">借方/贷方</param>
        /// <param name="cd">借贷</param>
        /// <param name="accountIds">科目对账关系</param>
        /// <param name="billType">单据类型</param>
        /// <param name="bizTable">单据体</param>
        private static void CheckVoucherD(SData voucher, string mesg, string amountStr, string cd, List<SData> accountIds, string billType, BizTable bizTable)
        {
            if (voucher == null)
            {
                throw new Exception(bizTable.ApiTask.L(mesg + "凭证分录") + bizTable.ApiTask.LEnd(PubLang.NotExists));
            }

            // 如果凭证分录的状态不是已审，抛出异常
            if (voucher["vstate", string.Empty].ToString().Sp_First() != AcConst.Trial)
            {
                throw new Exception(bizTable.ApiTask.L(mesg + "凭证分录") + bizTable.ApiTask.LEnd("未审核"));
            }

            // 如果凭证分录的数据类型不是实际数据，抛出异常
            if (string.IsNullOrEmpty(voucher.Item<string>("vclass"))
                || voucher["vclass"].ToString().Sp_First() != AcConst.ActData)
            {
                throw new Exception(bizTable.ApiTask.L(mesg + "凭证分录") + bizTable.ApiTask.LEnd("不是实际数据"));
            }

            // 如果收款/付款凭证分录的金额不在借方/贷方，抛出异常：收款/付款凭证分录的金额在贷方/借方不能收款/付款
            decimal.TryParse(voucher[amountStr].ToString(), out decimal amount);
            if (amount == 0)
            {
                throw new Exception(bizTable.ApiTask.LEnd(mesg + "凭证分录的金额在" + cd + "不能" + mesg));
            }

            if (string.IsNullOrEmpty(voucher.Item<string>("account.id")))
            {
                throw new Exception(bizTable.ApiTask.L("科目") + bizTable.ApiTask.LEnd(PubLang.NotExists));
            }

            // 如果凭证分录的科目不属于该科目对账中设置的科目，抛出异常
            if (accountIds == null || accountIds.Count == 0)
            {
                throw new Exception(bizTable.ApiTask.LEnd(mesg + "凭证分录的科目不属于对账科目"));
            }

            var account = accountIds.Find(a => a["account.id"]?.ToString() == voucher["account.id"].ToString());
            if (account == null)
            {
                throw new Exception(bizTable.ApiTask.LEnd(mesg + "凭证分录的科目不属于对账科目"));
            }

            // 收款/付款凭证分录的科目不属于收款/付款科目
            if (string.IsNullOrEmpty(account.Item<string>("billtype.dcode"))
                || account["billtype.dcode"].ToString() != billType)
            {
                throw new Exception(bizTable.ApiTask.LEnd(mesg + "凭证分录的科目不属于" + mesg + "科目"));
            }
        }

        /// <summary>
        /// 得到指定核算期的上一个核算期
        /// </summary>
        /// <param name="apiTask">访问api</param>
        /// <param name="period">核算期</param>
        /// <returns></returns>
        public static string GetPeriodUp(ApiTask apiTask, string period)
        {
            string newPeriod = string.Empty;

            // 得到每年核算期期数
            var periodNum = apiTask.GetParms(AcConst.PeriodNum);
            int.TryParse(periodNum, out int periodMaxNum);

            // 判断当前核算期等于第一个期数
            if (int.Parse(period.Substring(4, 2)) == 1)
            {
                // 设置当前核算期为上一年的结束核算期（年份减1，期数为每年最大核算期数）
                var strPeriodNum = periodMaxNum >= 10 ? periodNum : "0" + periodNum;
                newPeriod = (int.Parse(period.Substring(0, 4)) - 1) + strPeriodNum;
            }
            else
            {
                // 设置当前核算期减1
                newPeriod = (int.Parse(period) - 1).ToString();
            }

            return newPeriod;
        }

        private static Dictionary<string, Dictionary<string, string>> codeNumDic = new Dictionary<string, Dictionary<string, string>>();
        private static readonly object Sync = new object();

        /// <summary>
        /// 得到缓存中指定字段最大的值
        /// </summary>
        /// <param name="apiTask">访问api</param>
        /// <param name="biz">表名</param>
        /// <param name="time">日期字段</param>
        /// <param name="field">指定字段</param>
        /// <param name="sign">标识</param>
        /// <returns></returns>
        public static string GetMaxBankCode(ApiTask apiTask, string biz, string time, string field, string sign)
        {
            // 业务逻辑 **********
            // * 缓存存在key，则直接取值
            var numKey = $"{apiTask.Domain}_{sign}";
            var dayKey = DateTime.Today.ToString("yyMMdd");
            lock (Sync)
            {
                if (codeNumDic.ContainsKey(numKey))
                {
                    if (codeNumDic[numKey].ContainsKey(dayKey))
                    {
                        codeNumDic[numKey][dayKey] = long.Parse(codeNumDic[numKey][dayKey].Replace(sign, string.Empty)) + 1 + sign;
                    }
                    else
                    {
                        codeNumDic[numKey].Add(dayKey, "0");
                        codeNumDic[numKey][dayKey] = GetMaxVoucherNumToday(apiTask, biz, time, field, sign);

                        // 删除旧的key记录
                        var loseKey = codeNumDic[numKey].Keys.ToArray();
                        foreach (var t in loseKey)
                        {
                            if (t != dayKey)
                            {
                                codeNumDic[numKey].Remove(t);
                            }
                        }
                    }
                }
                else
                {
                    codeNumDic.Add(numKey, new Dictionary<string, string>());
                    codeNumDic[numKey].Add(dayKey, "0");
                    codeNumDic[numKey][dayKey] = GetMaxVoucherNumToday(apiTask, biz, time, field, sign);
                }

                Env.Log($"产生{sign}>>>{codeNumDic[numKey][dayKey]}");
                return codeNumDic[numKey][dayKey].ToString();
            }
        }

        /// <summary>
        /// 得到数据库中指定字段最大的值
        /// </summary>
        /// <param name="task">访问api</param>
        /// <param name="biz">表名</param>
        /// <param name="time">日期字段</param>
        /// <param name="field">指定字段</param>
        /// <param name="sign">标识</param>
        /// <returns></returns>
        private static string GetMaxVoucherNumToday(ApiTask task, string biz, string time, string field, string sign)
        {
            var maxDecode = task.DB.ExecuteScalar($"select top 1 {field} from {biz} where {time}='{DateTime.Today:yyyy-MM-dd}' and {field} like '%{sign}' order by id desc");
            long maxNum = 0;
            if (maxDecode != null)
            {
                try
                {
                    maxNum = long.Parse(maxDecode.ToString().Replace(sign, string.Empty)) + 1;
                }
                catch (Exception)
                {
                    maxNum = long.Parse(System.DateTime.Today.ToString("yyMMdd") + "1".PadLeft(6, '0'));
                }
            }
            else
            {
                maxNum = long.Parse(System.DateTime.Today.ToString("yyMMdd") + "1".PadLeft(6, '0'));
            }

            return maxNum + sign;
        }

        /// <summary>
        /// 系统自动生成银行对账单科目
        /// </summary>
        /// <param name="task">访问api</param>
        /// <param name="bkAccount">银行科目</param>
        /// <returns>银行对账单科目</returns>
        public static SData AddBankAccount(ApiTask task, object bkAccount)
        {
            if (bkAccount == null)
            {
                throw new Exception(task.L("银行科目") + task.LEnd(AcLang.NotExists));
            }

            var accountBiz = task.Biz<BizTableCode>("AcAccount"); // 科目对象
            int id = accountBiz.GetIDAuto(bkAccount.ToString());
            var account = accountBiz.GetItem(id); // 银行科目

            // 银行对账单科目
            // 超长时截取
            var dcode = AcVoucherHelper.GetStr(account.Item<string>("dcode"), 77); // 代码
            var title = AcVoucherHelper.GetStr(account.Item<string>("title"), 144); // 名称

            account["dcode"] = $"z{dcode}bk";
            account["title"] = $"{title}对账单";

            // 查询银行对账单科目
            var newAccount = accountBiz.GetItemByParms(new SData("dcode", account["dcode"], "title", account["title"]).toParmStr(), "id");
            if (newAccount == null)
            {
                // 如果不存在，就添加
                account["isinit"] = true;
                account["property"] = AcConst.SubjecPropertyCount; // 科目性质（业务）
                int idBk = accountBiz.Insert(account);
                account["id"] = idBk;
            }

            return account;
        }

        /// <summary>
        /// 根据科目对账、单据类型、科目查询得到科目id
        /// </summary>
        /// <param name="task">访问api</param>
        /// <param name="subrec">科目对账</param>
        /// <param name="billtype">单据类型</param>
        /// <param name="account">科目</param>
        /// <returns></returns>
        public static string GetAccountBySubrec(ApiTask task, string subrec, string billtype, string account)
        {
            // 根据科目对账、单据类型、科目查询得到科目列表
            var recrefBiz = task.Biz<BizTable>("AcReconRef");
            SData recRefParm = new SData("subrec", subrec, "billtype", billtype, "account", account);
            var accountList = recrefBiz.GetListData(0, 1, recRefParm.toParmStr(), "account.id").ToList();

            // 将科目列表转换成id字符串
            var accountIds = string.Empty;
            foreach (var item in accountList)
            {
                if (!string.IsNullOrEmpty(item.Item<string>("account.id")))
                {
                    accountIds += "," + item["account.id"].ToString();
                }
            }

            return string.IsNullOrEmpty(accountIds) ? "-1" : accountIds.TrimStart(',');
        }
    }
}
