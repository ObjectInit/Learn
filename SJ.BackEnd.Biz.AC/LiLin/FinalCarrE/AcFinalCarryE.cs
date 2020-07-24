#region  <<版本注释>>
/* ==========================================================
// <copyright file="AcFinalCarryE.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：AcFinalCarryE
* 创 建 者：李琳
* 创建时间：2019/10/15 9:44:05
* =============================================================*/
#endregion

using SJ.BackEnd.Base;
using SJ.BackEnd.Biz.Pub;
using SJ.Global;
using System;
using System.Collections.Generic;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 期末结转
    /// 1 描述：结转下期、返回上期、数据平衡查询业务处理类
    /// 2 约定：使用Getlist方法,传入参数type值说明： z1结转下期 、z2返回上期、z3查询数据平衡
    /// 3 业务逻辑
    ///     查询前：
    ///         结转下期逻辑：检查当前核算是否存在非已审的凭证，非已审的收付款单，已审的凭证是否借贷平衡，满足条件修改核算期到下一期
    ///         返回上期逻辑：检查当前核算期不是启用核算期，满足条件修改核算期到上一期
    ///     查询后：
    ///         查询数据平衡逻辑：查询已审的凭证是否借贷平衡，返回查询是否平衡的结果消息
    /// </summary>
    [ClassData("cname", "期末结转", "vision", 1)]
    public class AcFinalCarryE : BizQuery
    {
        #region 框架方法重写

        /// <summary>
        /// 初始化sql
        /// </summary>
        /// <param name="sql">查询sql</param>
        protected override void OnInitBaseSql(ref DBSql sql)
        {
            sql = new DBSql
            {
                SqlFrom = new SData("m", "account m"),
                SqlWhere = "id = -1",
                SqlOrderBy = "id",
            };
        }

        /// <summary>
        /// 初始定义未做业务,子类必须实现
        /// </summary>
        protected override void OnInitDefine()
        {
        }

        /// <summary>
        /// 期末结转
        /// </summary>
        /// <param name="data">数据值 type值说明： z1结转下期 、z2返回上期、z3查询数据平衡</param>
        protected override void OnGetListBefore(SData data)
        {
            // 得到系统当前核算期
            var currentPeriod = ApiTask.GetParms(AcConst.PeriodCurrent);

            // 得到每年核算期期数
            var periodNum = ApiTask.GetParms(AcConst.PeriodNum);
            int.TryParse(periodNum, out int periodMaxNum);

            // 新的当前核算期变量
            var newCurrentPeriod = string.Empty;

            // 判断为结转下期操作
            if (data["type"].Equals(AcConst.Next))
            {
                // 结转下期逻辑
                ValidDataNext(currentPeriod);

                // 判断当前核算期等于每年最大核算期数
                if (int.Parse(currentPeriod.ToString().Substring(4, 2)) == periodMaxNum)
                {
                    // 设置当前核算期为下一年的开始核算期（年份加1，期数为01）
                    string strPeriodNum = "01";
                    newCurrentPeriod = (int.Parse(currentPeriod.Substring(0, 4)) + 1) + strPeriodNum;
                }
                else
                {
                    // 设置当前核算期加1
                    newCurrentPeriod = (int.Parse(currentPeriod) + 1).ToString();
                }
            }

            // 判断为返回上期操作
            if (data["type"].Equals(AcConst.Up))
            {
                // 转回上期逻辑
                ValidDataUp(currentPeriod);

                // 得到当前核算期的上一个核算期
                newCurrentPeriod = VoucherHelper.GetPeriodUp(this.ApiTask, currentPeriod);
            }

            // 判断是否是查询数据平衡操作，不是就要更新当前核算期
            if (!data["type"].Equals(AcConst.Quey))
            {
                // 更新当前核算期
                ApiTask.SetParms(AcConst.PeriodCurrent, newCurrentPeriod);
            }

            // 设置查询的id为-1,屏蔽基类查询结果影响
            data.Add("id", -1);

            base.OnGetListBefore(data);
        }

        /// <summary>
        /// 查询之后的处理方法
        /// 如果是数据查询操作，返回查询数据是否平衡的结果
        /// </summary>
        /// <param name="qParm">查询条件数据, 数据值type值说明： z1结转下期 、z2返回上期、z3查询数据平衡</param>
        /// <param name="Data">查询的数据集合</param>
        protected override void OnGetListAfter(SData qParm, List<SData> Data)
        {
            // 得到当前核算期
            var currentPeriod = ApiTask.GetParms(AcConst.PeriodCurrent);

            // 判断操作为查询数据平衡
            if (qParm["type"].Equals(AcConst.Quey))
            {
                var msg = GetBalanceMsg(currentPeriod, out bool isVerify);

                // 设置返回的结果数据
                Data.Add(new SData("message", msg, "isVerify", isVerify));
            }

            base.OnGetListAfter(qParm, Data);
        }

        #endregion

        #region 自定义方法

        /// <summary>
        /// 验证结转下期逻辑
        /// 1、检查当前核算期存在非已审的凭证
        /// 2、检查当前核算期存在非已审的收款单
        /// 3、检查当前核算期存在非已审的付款单
        /// 4、检查已审的凭证是否借贷平衡
        /// </summary>
        /// <param name="currentPeriod">当前核算期</param>
        private void ValidDataNext(string currentPeriod)
        {
            // *查询当前核算期是否存在非已审的凭证
            string querParms = new SData("vstate", "!" + AcConst.Trial, "vclass", AcConst.ActData, "period", currentPeriod).toParmStr();
            SData data = ApiTask.Biz<BizTable>(nameof(AcVoucher)).GetItemByParms(querParms, "id");
            if (data != null)
            {
                throw new Exception(this.ApiTask.LEnd("存在未记账的凭证，不允许结转"));
            }

            // *查询当前核算期存在暂存、在审凭证收款单
            data = ApiTask.Biz<BizTable>(nameof(AcReceipt)).GetItemByParms(querParms, "id");
            if (data != null)
            {
                throw new Exception(this.ApiTask.LEnd("存在未记账的收款单，不允许结转"));
            }

            // *查询当前核算期存在暂存、在审凭证付款单
            data = ApiTask.Biz<BizTable>(nameof(AcPay)).GetItemByParms(querParms, "id");
            if (data != null)
            {
                throw new Exception(this.ApiTask.LEnd("存在未记账的付款单，不允许结转"));
            }

            // *判断借贷是否平衡
            CheckBalance(currentPeriod);
        }

        /// <summary>
        /// 验证结转到上期的逻辑
        /// 1、检查当前核算期是否为启用核算期
        /// </summary>
        /// <param name="currentPeriod">当前核算器</param>
        private void ValidDataUp(string currentPeriod)
        {
            // 得到系统初始核算期
            var periodStartx = ApiTask.GetParms(AcConst.PeriodStartx);
            int.TryParse(periodStartx, out int periodStartxNum);

            // *判断是启用核算期
            if (periodStartxNum == int.Parse(currentPeriod))
            {
                throw new System.Exception(this.ApiTask.LEnd("当前核算期已为系统初始核算期"));
            }
        }

        /// <summary>
        /// 判断核算期内数据是否平衡
        /// </summary>
        /// <param name="period">核算期</param>
        private void CheckBalance(string period)
        {
            // ** 校验期初余额是否平衡
            string bamountdParms = GetParms(period, AcConst.Debit);
            var bamountd = ApiTask.GetBalance(EBalanceType.b, "amount", bamountdParms);

            var bamountcParms = GetParms(period, AcConst.Credit);
            var bamountc = ApiTask.GetBalance(EBalanceType.b, "amount", bamountcParms);

            if (bamountd.ToString().ToDec() != bamountc.ToString().ToDec() * -1)
            {
                throw new Exception(this.ApiTask.LEnd("数据不平衡，不允许结转"));
            }

            // 设置聚合条件为核算期为当前核算期，凭证状态为记账，数据类型为实际数据，科目性质为计入平衡科目
            var sData = new SData("period", period, "vstate", AcConst.Trial, "vclass", AcConst.ActData, "account.property.iseq", "true");

            // ** 校验发生额是否平衡
            var amountd = ApiTask.GetBalance(EBalanceType.h, "amountd", sData.toParmStr());
            var amountc = ApiTask.GetBalance(EBalanceType.h, "amountc", sData.toParmStr());
            if (amountd.ToString().ToDec() != amountc.ToString().ToDec())
            {
                throw new Exception(this.ApiTask.LEnd("数据不平衡，不允许结转"));
            }

            // ** 校验期末余额是否平衡
            var lamountdParms = GetParms(period, AcConst.Debit);
            var lamountd = ApiTask.GetBalance(EBalanceType.l, "amount", lamountdParms);

            var lamountcParms = GetParms(period, AcConst.Credit);
            var lamountc = ApiTask.GetBalance(EBalanceType.l, "amount", lamountcParms);

            if (lamountd.ToString().ToDec() != lamountc.ToString().ToDec() * -1)
            {
                throw new Exception(this.ApiTask.LEnd("数据不平衡，不允许结转"));
            }
        }

        /// <summary>
        /// 得到核算期内数据是否平衡的查询结果
        /// </summary>
        /// <param name="period">核算期</param>
        /// <param name="isVerify">是否平衡：</param>
        /// <returns></returns>
        private string GetBalanceMsg(string period, out bool isVerify)
        {
            string msg = string.Empty;

            // ** 校验期初余额是否平衡
            string bamountdParms = GetParms(period, AcConst.Debit);
            var bamountd = ApiTask.GetBalance(EBalanceType.b, "amount", bamountdParms);

            var bamountcParms = GetParms(period, AcConst.Credit);
            var bamountc = ApiTask.GetBalance(EBalanceType.b, "amount", bamountcParms);

            if (bamountd.ToString().ToDec() != bamountc.ToString().ToDec() * -1)
            {
                msg += this.ApiTask.L("期初余额不平衡") + this.ApiTask.L(PubLang.SymbolComma);
            }

            // 设置聚合条件为核算期为当前核算期，凭证状态为记账，数据类型为实际数据，科目性质为计入平衡科目
            var sData = new SData("period", period, "vstate", AcConst.Trial, "vclass", AcConst.ActData, "account.property.iseq", "true");

            // ** 校验发生额是否平衡
            var amountd = ApiTask.GetBalance(EBalanceType.h, "amountd", sData.toParmStr());
            var amountc = ApiTask.GetBalance(EBalanceType.h, "amountc", sData.toParmStr());
            if (amountd.ToString().ToDec() != amountc.ToString().ToDec())
            {
                msg += this.ApiTask.L("发生额不平衡") + this.ApiTask.L(PubLang.SymbolComma);
            }

            // ** 校验期末余额是否平衡
            var lamountdParms = GetParms(period, AcConst.Debit);
            var lamountd = ApiTask.GetBalance(EBalanceType.l, "amount", lamountdParms);

            var lamountcParms = GetParms(period, AcConst.Credit);
            var lamountc = ApiTask.GetBalance(EBalanceType.l, "amount", lamountcParms);

            if (lamountd.ToString().ToDec() != lamountc.ToString().ToDec() * -1)
            {
                msg += this.ApiTask.L("期末余额不平衡") + this.ApiTask.L(PubLang.SymbolComma);
            }

            isVerify = string.IsNullOrEmpty(msg) ? true : false;
            msg += string.IsNullOrEmpty(msg) ? this.ApiTask.LEnd("数据平衡") : this.ApiTask.L("请检查试算平衡表") + this.ApiTask.L(PubLang.SymbolEnd);
            return msg;
        }

        /// <summary>
        ///  获取查询条件
        /// </summary>
        /// <param name="period">核算器</param>
        /// <param name="accountFlow">科目借贷方向</param>
        /// <returns></returns>
        private static string GetParms(string period, int accountFlow)
        {
            return new SData("vclass", AcConst.ActData, "vstate", AcConst.Trial, "account.flow", accountFlow, "period", period, "account.property.iseq", "true").toParmStr();
        }

        #endregion
    }
}
