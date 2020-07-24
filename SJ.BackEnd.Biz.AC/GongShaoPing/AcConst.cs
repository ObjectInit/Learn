#region  <<版本注释>>
/* ==========================================================
// <copyright file="ConstResource.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：VoucherConstResource
* 创 建 者：曾倩倩
* 创建时间：2019/9/27 14:39:05
* =============================================================*/
#endregion

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// AC业务常量
    /// 1 描述: Ac项目中的常量集合
    /// 2 约定：无
    /// 3 业务逻辑：无
    /// </summary>
    public class AcConst
    {
        #region 单据状态

        /// <summary>
        /// 草稿
        /// </summary>
        public const string Draft = "z1";

        /// <summary>
        /// 待审
        /// </summary>
        public const string UnderTrial = "z3";

        /// <summary>
        /// 已审
        /// </summary>
        public const string Trial = "z5";

        /// <summary>
        /// 子状态-锁定
        /// </summary>
        public const string LockS = "z002";

        /// <summary>
        /// 子状态-正常
        /// </summary>
        public const string NormalS = "z000";

        #endregion

        #region 单据类型

        /// <summary>
        /// 收款单（单据类型）
        /// </summary>
        public const string ReceiptVoucher = "zsk";

        /// <summary>
        /// 付款单（单据类型）
        /// </summary>
        public const string PayVoucher = "zfk";

        /// <summary>
        /// 对账单的凭证类型（z0.对账单）
        /// </summary>
        public const string BankVhType = "z0";

        #endregion

        #region 科目代码

        /// <summary>
        /// 往来勾对科目代码
        /// </summary>
        public const string RecoAccount = "z90000";

        /// <summary>
        /// 收款科目代码
        /// </summary>
        public const string ReceiptAccount = "z90001";

        /// <summary>
        /// 付款科目代码
        /// </summary>
        public const string PayAccount = "z90002";

        #endregion

        #region 科目性质

        /// <summary>
        /// 科目性质-业务（统计）
        /// </summary>
        public const string SubjecPropertyCount = "z8";

        /// <summary>
        /// 科目性质-资产
        /// </summary>
        public const string PropertyAssets = "z1";

        #endregion

        #region 数据类型

        /// <summary>
        /// 实际数据的代码
        /// </summary>
        public const string ActData = "z0";

        #endregion

        #region 勾对

        /// <summary>
        /// 勾对头编号生成规则
        /// </summary>
        public const string RecoNumRule = "year";

        /// <summary>
        /// 勾对查询
        /// </summary>
        public const string RecoQuery = "recoquery";

        /// <summary>
        /// 付款单标识
        /// </summary>
        public const string Pay = "pay";

        /// <summary>
        /// 收款单标识
        /// </summary>
        public const string Receipt = "receipt";

        /// <summary>
        /// 自动勾对每次取的记录数
        /// </summary>
        public const int RecoPageSize = 100;

        /// <summary>
        /// 科目对账设置的T列
        /// </summary>
        public const string RecoTcode = "t29";

        #endregion

        #region 系统参数

        /// <summary>
        /// 系统当前核算期
        /// </summary>
        public const string PeriodCurrent = "period";

        /// <summary>
        /// 每年核算期数
        /// </summary>
        public const string PeriodNum = "periodperyear";

        ///// <summary>
        ///// 凭证录入最大跨区间数
        ///// </summary>
        public const string PeriodDuring = "periodduring";

        /// <summary>
        /// 系统启用核算期
        /// </summary>
        public const string PeriodStartx = "periodstart";

        /// <summary>
        /// 按核算期进行账龄分析
        /// </summary>
        public const string AusePeriod = "auseperiod";

        #endregion

        #region 勾对状态

        /// <summary>
        /// 从未勾对
        /// </summary>
        public const string NeverTick = "z1";

        /// <summary>
        /// 部分勾对
        /// </summary>
        public const string PartTick = "z2";

        /// <summary>
        /// 未勾对（包含从未勾对和部分勾对）
        /// </summary>
        public const string NoAllTick = "z3";

        /// <summary>
        /// 全部勾对
        /// </summary>
        public const string AllTick = "z4";

        /// <summary>
        /// 已勾对（包含部分勾对和全部勾对）
        /// </summary>
        public const string AlreadyTick = "z5";

        #endregion

        #region 勾对类型

        /// <summary>
        /// 手工勾对
        /// </summary>
        public const string RecoManual = "z1";

        /// <summary>
        /// 单据勾对
        /// </summary>
        public const string RecoVoucher = "z2";

        /// <summary>
        /// 自动勾对
        /// </summary>
        public const string RecoAuto = "z3";

        #endregion

        #region 余额方向

        /// <summary>
        /// 余额方向-借
        /// </summary>
        public const int Debit = 0;

        /// <summary>
        /// 余额方向-贷
        /// </summary>
        public const int Credit = 1;

        #endregion

        #region 核算单位

        /// <summary>
        /// 核算单位-总部
        /// </summary>
        public const string UnitHead = "z0";

        #endregion

        #region 期末结转

        /// <summary>
        /// 结转下期
        /// </summary>
        public const string Next = "z1";

        /// <summary>
        /// 返回上期
        /// </summary>
        public const string Up = "z2";

        /// <summary>
        /// 数据平衡查询
        /// </summary>
        public const string Quey = "z3";

        #endregion

        #region 年终结转

        /// <summary>
        /// 年终结转
        /// </summary>
        public const int YearEndType = 1;

        #endregion

        #region 审批动作

        /// <summary>
        /// 记账凭证流程代码
        /// </summary>
        public const string AccounT = "z0010";

        /// <summary>
        /// 反记账凭证流程代码
        /// </summary>
        public const string AntAccounT = "z0020";

        /// <summary>
        /// 复核
        /// </summary>
        public const string Review = "z0040";

        /// <summary>
        /// 反复核
        /// </summary>
        public const string NoReview = "z0050";

        /// <summary>
        /// 提交收/付款单
        /// </summary>
        public const string ActCommit = "z03001";

        /// <summary>
        /// 记账收/付款单
        /// </summary>
        public const string ActJi = "z03002";

        /// <summary>
        /// 反记账收/付款单
        /// </summary>
        public const string ActFanJi = "z03003";

        /// <summary>
        /// 撤回收/付款单
        /// </summary>
        public const string ActFanCommit = "z03004";

        #endregion

        #region 职务

        /// <summary>
        /// 职务-会计
        /// </summary>
        public const string JobAcc = "z1";

        /// <summary>
        /// 职务-出纳
        /// </summary>
        public const string JobTeller = "z2";

        #endregion

        #region 核算期范围

        /// <summary>
        /// 核算期为空时-最小核算期
        /// </summary>
        public const string PeriodBegin = "190001";

        /// <summary>
        /// 核算期为空时-最大核算期
        /// </summary>
        public const string PeriodEnd = "209912";

        #endregion

        #region 银行对账接口参数

        /// <summary>
        /// 银行勾对-撤销勾对
        /// </summary>
        public const string BRecoDel = "brdel";

        /// <summary>
        /// 银行勾对-勾对记录
        /// </summary>
        public const string BRecoData = "brdata";

        #endregion

        /// <summary>
        /// 最大日期
        /// </summary>
        public const string BigDate = "2099-12-31";

        #region 账龄分析类型

        /// <summary>
        /// 将要到期
        /// </summary>
        public const string ToExpire = "z1";

        /// <summary>
        /// 逾期
        /// </summary>
        public const string Overdue = "z2";
        #endregion

        #region 账龄分析日期类型

        /// <summary>
        /// 日
        /// </summary>
        public const string Day = "z1";

        /// <summary>
        /// 月
        /// </summary>
        public const string Month = "z2";
        #endregion
    }
}
