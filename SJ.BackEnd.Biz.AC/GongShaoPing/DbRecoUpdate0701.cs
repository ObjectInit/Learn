#region  << 版 本 注 释 >>
/* ============================================================================== 
// <copyright file="DbRecoUpdate.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：DbRecoUpdate 
* 创 建 者：龚绍平
* 创建时间：2020/1/3 15:51:32 
* ==============================================================================*/
#endregion
using SJ.BackEnd.Biz.Pub;
using System;
using System.Text;

namespace SJ.BackEnd.Biz.AC
{
    /// <summary>
    /// 根据银行账反馈添加脚本
    /// 1 描述: 根据银行账反馈添加脚本-2020-07-01
    /// 2 约定：无
    /// 3 业务逻辑：无
    /// </summary>
    public class DbRecoUpdate0701 : BaseBizData
    {
        public override DateTime Ver => new DateTime(2020, 07, 01);

        public override string Sql {

            get {
                var sqlBuidler = new StringBuilder();

                // ***** 根据银行账反馈添加脚本-2020-07-01
                // 删除菜单：不显示物品和项目
                sqlBuidler.AppendLine(@" update [others] set isenable=0 where dclass='acmenu' and dcode='z60044';
                                     update [others] set isenable=0 where dclass='acmenu' and dcode='z60042'");

                // 添加菜单：应收对账和应付对账
                sqlBuidler.AppendLine(
                    @"insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) 
                      values ( 'z20010', '应收对账', 'acmenu', 1, 1, 0, '预设菜单', 0, 0, (select id from others where dclass='acrightpass' and dcode='z0120'), 0, 0, 0, 0, 'ReceivablesRec');");
                sqlBuidler.AppendLine(
                    @"insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) 
                      values ( 'z20011', '应付对账', 'acmenu', 1, 1, 0, '预设菜单', 0, 0, (select id from others where dclass='acrightpass' and dcode='z0120'), 0, 0, 0, 0, 'PayRec');");

                // 账龄分析时间类别-添加预设数据
                sqlBuidler.AppendLine(
                    @"insert into [dbo].[others]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values ('z0', '按月分析', 'acanalygenre', 1, 1, 0, 0, '预设账龄分析时间类别');");

                return sqlBuidler.ToString();
            }
        }
    }
}