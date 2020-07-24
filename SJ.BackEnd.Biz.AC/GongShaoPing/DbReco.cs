#region  << 版 本 注 释 >>
/* ============================================================================== 
// <copyright file="dbv1.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：dbv1 
* 创 建 者：龚绍平
* 创建时间：2019/11/13 17:48:25 
* ==============================================================================*/
#endregion
using SJ.BackEnd.Biz.Pub;
using System;
using System.Text;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 往来基础脚本
    /// 1 描述: 
    /// 2 约定：无
    /// 3 业务逻辑：无
    /// </summary>
    public class DbReco : BaseBizData
    {
        public override string Sql
        {
            get
            {
                var sqlBuidler = new StringBuilder();

                // 职务acjob
                sqlBuidler.AppendLine(@"delete from others where dclass = 'acjob';
                                insert into [dbo].[others] ([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values('z1', '会计', 'acjob', 1, 1, 1, 0, '预设职务');
                                insert into [dbo].[others] ([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values('z2', '出纳', 'acjob', 1, 1, 1, 0, '预设职务');");

                // 审批流程
                sqlBuidler.AppendLine(
                        @"insert into [dbo].[others]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values ('z03', '应收应付', 'acapproveflow', 1, 1, 0, 0, '预设审批流程');
                    insert into[dbo].[others]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc], [a0], [a1], [a3], [a8]) values('z03001', '提交', 'acapproveact', 1, 1, 0, 0, '预设审批动作', (select id from others where dclass = 'acapproveflow' and dcode = 'z03'),(select id from others where dclass = 'acvstate' and dcode = 'z1'),(select id from others where dclass = 'acvstate' and dcode = 'z3'),(select id from others where dclass = 'acrightpass' and dcode = 'z0110'));
                    insert into[dbo].[others] ([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc], [a0], [a1], [a3], [a8]) values('z03002', '记账', 'acapproveact', 1, 1, 0, 0, '预设审批动作', (select id from others where dclass = 'acapproveflow' and dcode = 'z03'),(select id from others where dclass = 'acvstate' and dcode = 'z3'),(select id from others where dclass = 'acvstate' and dcode = 'z5'),(select id from others where dclass = 'acrightpass' and dcode = 'z0120'));
                    insert into[dbo].[others] ([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc], [a0], [a1], [a3], [a8]) values('z03003', '反记账', 'acapproveact', 1, 1, 0, 0, '预设审批动作', (select id from others where dclass = 'acapproveflow' and dcode = 'z03'),(select id from others where dclass = 'acvstate' and dcode = 'z5'),(select id from others where dclass = 'acvstate' and dcode = 'z1'),(select id from others where dclass = 'acrightpass' and dcode = 'z0120'));               
                    insert into[dbo].[others] ([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc], [a0], [a1], [a3], [a8]) values('z03004', '撤回', 'acapproveact', 1, 1, 0, 0, '预设审批动作', (select id from others where dclass = 'acapproveflow' and dcode = 'z03'),(select id from others where dclass = 'acvstate' and dcode = 'z3'),(select id from others where dclass = 'acvstate' and dcode = 'z1'),(select id from others where dclass = 'acrightpass' and dcode = 'z0110'));               
                
                ");

                // 菜单
                sqlBuidler.AppendLine(@"
                                    delete [dbo].[others] where dcode = 'z200' and dclass = 'acmenu';
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z200', '应收应付', 'acmenu', 1, 1, 0, '', 2, 1, '', 0, 0, 0, 0, '');
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z20020', '收款单', 'acmenu', 1, 1, 0, '', 1, 0, (select id from others where dclass ='acrightpass' and dcode ='z0120'), 0, 0, 0, 0, 'ReceivableVoucher');
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z20030', '付款单', 'acmenu', 1, 1, 0, '', 1, 0, (select id from others where dclass ='acrightpass' and dcode ='z0120'), 0, 0, 0, 0, 'PaymentVoucher');
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z20040', '预测账龄分析', 'acmenu', 1, 1, 0, '', 0, 0, (select id from others where dclass ='acrightpass' and dcode ='z0120'), 0, 0, 0, 0, 'ForecastAnalysis');
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z20050', '逾期账龄分析', 'acmenu', 1, 1, 0, '', 0, 0, (select id from others where dclass ='acrightpass' and dcode ='z0120'), 0, 0, 0, 0, 'OverdueAnalysis');
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z20060', '应收逾期账龄分析', 'acmenu', 1, 1, 0, '', 0, 0, (select id from others where dclass ='acrightpass' and dcode ='z0120'), 0, 0, 0, 0, 'RecOverPar');
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z20070', '应付逾期账龄分析', 'acmenu', 1, 1, 0, '', 0, 0, (select id from others where dclass ='acrightpass' and dcode ='z0120'), 0, 0, 0, 0, 'PayOverPar');
                                    
                                    delete [dbo].[others] where dcode = 'z201' and dclass = 'acmenu';
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z201', '账龄分析', 'acmenu', 1, 1, 0, '', 2, 1, '', 0, 0, 0, 0, '');
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z20110', '账龄分析时间类别', 'acmenu', 1, 1, 0, '', 1, 0, (select id from others where dclass ='acrightpass' and dcode ='z0120'), 0, 0, 0, 0, 'AgeAnalysisTypeSet');                                    
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z20120', '账龄分析时间设置', 'acmenu', 1, 1, 0, '', 1, 0, (select id from others where dclass ='acrightpass' and dcode ='z0120'), 0, 0, 0, 0, 'AgeAnalysisTimeSet');
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z20130', '借方逾期账龄分析', 'acmenu', 1, 1, 0, '', 1, 0, (select id from others where dclass ='acrightpass' and dcode ='z0120'), 0, 0, 0, 0, 'DebitOverPar');
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z20140', '贷方逾期账龄分析', 'acmenu', 1, 1, 0, '', 1, 0, (select id from others where dclass ='acrightpass' and dcode ='z0120'), 0, 0, 0, 0, 'CreditOverPar');

                                    delete [dbo].[others] where dcode = 'z300' and dclass = 'acmenu';
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z300', '科目对账', 'acmenu', 1, 1, 0, '', 2, 1, '', 0, 0, 0, 0, '');
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z30010', '科目对账', 'acmenu', 1, 1, 0, '', 0, 0, (select id from others where dclass ='acrightpass' and dcode ='z3020'), 0, 0, 0, 0, 'AcSubRec');
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z30020', '自动勾对', 'acmenu', 1, 1, 0, '', 0, 0, (select id from others where dclass ='acrightpass' and dcode ='z0120'), 0, 0, 0, 0, 'AutoTickPai');
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z30030', '勾对记录', 'acmenu', 1, 1, 0, '', 0, 0, (select id from others where dclass ='acrightpass' and dcode ='z0120'), 0, 0, 0, 0, 'BlendingRecord');
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z40030', '科目历史余额表', 'acmenu', 1, 1, 0, '', 0, 0, (select id from others where dclass ='acrightpass' and dcode ='z2010'), 0, 0, 0, 0, 'AcBalance?gp1=period.核算期');
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z40040', '数量余额表', 'acmenu', 1, 1, 0, '', 0, 0, (select id from others where dclass ='acrightpass' and dcode ='z2010'), 0, 0, 0, 0, 'AcBalance?q.isqty=True');
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z40050', '外币余额表', 'acmenu', 1, 1, 0, '', 0, 0, (select id from others where dclass ='acrightpass' and dcode ='z2010'), 0, 0, 0, 0, 'AcBalance?q.iscurrency=True');
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z40060', '科目性质余额表', 'acmenu', 1, 1, 0, '', 0, 0, (select id from others where dclass ='acrightpass' and dcode ='z2010'), 0, 0, 0, 0, 'AcBalance?gp1=account-property.科目性质');
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z40070', '试算平衡表', 'acmenu', 1, 1, 0, '', 1, 0, (select id from others where dclass ='acrightpass' and dcode ='z2010'), 0, 0, 0, 0, 'TrialBalance');
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z70070', '年终结转', 'acmenu', 1, 1, 0, '', 1, 0, (select id from others where dclass ='acrightpass' and dcode ='z4010'), 0, 0, 0, 0, 'YearEndCarryOver');
                                    insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) values ( 'z70080', '期末结转', 'acmenu', 1, 1, 0, '', 7, 0, (select id from others where dclass ='acrightpass' and dcode ='z4010'), 0, 400, 300, 0, 'FinalCarryOver');");

                // 科目
                sqlBuidler.AppendLine(@"insert into[dbo].[account]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc], [property], [flow], [iscurrency], [isqty]) values('z90000', '勾对科目', 'a', 1, 1, 0, 0, '预设勾对科目', (select id from others where dclass = 'acacProperty' and dcode = 'z8'), 1, 0, 0);
                                    insert into[dbo].[account] ([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc], [property], [flow], [iscurrency], [isqty]) values('z90001', '收款单', 'a', 1, 1, 0, 0, '预设专用于收款单的科目', (select id from others where dclass= 'acacProperty' and dcode = 'z8'), 1, 0, 0);
                                    insert into[dbo].[account] ([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc], [property], [flow], [iscurrency], [isqty]) values('z90002', '付款单', 'a', 1, 1, 0, 0, '预设专用于付款单的科目', (select id from others where dclass= 'acacProperty' and dcode = 'z8'), 1, 0, 0);"
                );

                // 自动勾对规则
                sqlBuidler.AppendLine(
                    "insert into [dbo].[others]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values ('z1', '一对一', 'acrecorule', 1, 1, 0, 0, '预设自动勾对规则');");

                // 账龄分析类型acanalytype
                sqlBuidler.AppendLine(
                    @"insert into [dbo].[others]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values ('z1', '将要到期', 'acanalytype', 1, 1, 0, 0, '预设账龄分析类型');
                insert into [dbo].[others]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values ('z2', '逾期', 'acanalytype', 1, 1, 0, 0, '预设账龄分析类型');");

                // 账龄分析日期类型
                sqlBuidler.AppendLine(
                    @"insert into [dbo].[others]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values ('z1', '日', 'acanalydttype', 1, 1, 0, 0, '预设账龄分析日期类型');
                    insert into [dbo].[others]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values ('z2', '月', 'acanalydttype', 1, 1, 0, 0, '预设账龄分析日期类型');");

                // 勾对类型
                sqlBuidler.AppendLine(
                    @"insert into [dbo].[others]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values ('z1', '手动勾对', 'acrecotype', 1, 1, 0, 0, '预设勾对类型');
                    insert into [dbo].[others]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values ('z2', '单据勾对', 'acrecotype', 1, 1, 0, 0, '预设勾对类型');
                    insert into [dbo].[others]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values ('z3', '自动勾对', 'acrecotype', 1, 1, 0, 0, '预设勾对类型');");

                // 单据类型
                sqlBuidler.AppendLine(
                    @"insert into [dbo].[others]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values ('zsk', '收款单', 'acbilltype', 1, 1, 0, 0, '预设单据类型');
                    insert into [dbo].[others]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values ('zfk', '付款单', 'acbilltype', 1, 1, 0, 0, '预设单据类型');");

                // 勾对状态
                sqlBuidler.AppendLine(
                    @"insert into [dbo].[others]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values ('z1', '从未勾对', 'acrecostate', 1, 1, 0, 0, '预设勾对状态');
                    insert into [dbo].[others]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values ('z2', '部分勾对', 'acrecostate', 1, 1, 0, 0, '预设勾对状态');
                    insert into [dbo].[others]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values ('z3', '未勾对', 'acrecostate', 1, 1, 0, 0, '预设勾对状态');
                    insert into [dbo].[others]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values ('z4', '全部勾对', 'acrecostate', 1, 1, 0, 0, '预设勾对状态');
                    insert into [dbo].[others]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values ('z5', '已勾对', 'acrecostate', 1, 1, 0, 0, '预设勾对状态');");

                return sqlBuidler.ToString();
            }
        }

        public override DateTime Ver => new DateTime(2020, 05, 02);
    }
}