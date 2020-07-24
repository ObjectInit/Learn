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
    /// 往来对象数据库优化脚本
    /// 1 描述: 定义科目T29关联科目对账；将勾对、收款、付款三个科目的T29启用
    /// 2 约定：无
    /// 3 业务逻辑：无
    /// </summary>
    public class DbRecoUpdate : BaseBizData
    {
        public override DateTime Ver => new DateTime(2020, 05, 03);

        public override string Sql {

            get {
                var sqlBuidler = new StringBuilder();

                // 职务acjob
                sqlBuidler.AppendLine(@"update others set isenable=1,s0='AcSubRec.科目对账' where dclass='acdeftcode' and dcode='z29';
                                update account set t29=1 where dclass='a' and (dcode='z90000' or dcode='z90001' or dcode='z90002');");

                // 勾对记录查询索引
                sqlBuidler.AppendLine(@"if not exists (select [name] from sys.indexes where name='ireco')
                                    begin
                                    create nonclustered index [ireco] ON [dbo].[voucherd]
                                    (
	                                    [parent] ASC,
	                                    [t29] ASC,
	                                    [account] ASC,
	                                    [vstate] ASC,
	                                    [a9] ASC
                                    )with (pad_index = off, statistics_norecompute = off, sort_in_tempdb = off, drop_existing = off, online = off, allow_row_locks = on, allow_page_locks = on) on [primary]
                                    end
                                    ");

                // vision 下载菜单(2020/3/24 16:11:44 放到了其他第二个菜单)
                sqlBuidler.AppendLine(@"insert into [dbo].[others](dcode,title,dclass,isenable,isinit,isright,ddesc,rver,a1,a2,a3,a6,a7,a8,s0)
                                    values('z80020','Vision版本下载','acmenu',1,1,0,'',0,null,0,1,0,0,0,'down/ACVision.rar')");

                // 菜单文字重命名(2020/4/14+仓库改为部门,自定义代码设置改为客户自定义代码，分类余额表改为科目余额表)
                sqlBuidler.AppendLine(@" update [others] set title='预算方案' where dclass='acmenu' and dcode='z70030' ;
                                     update [others] set title='自动凭证结转' where dclass='acmenu' and dcode='z70070';
                                     update [others] set title='部门',isenable=0 where dclass='acmenu' and dcode='z60030';
                                     update [others] set title='客户自定义代码' where dclass='acmenu' and dcode='z60060';
                                     update [others] set title='客户自定义代码' where dclass='acmenu' and dcode='z70060';
                                     update [others] set title='科目余额表' where dclass='acmenu' and dcode='z40010';
                                     update [others] set title='账表查询' where dclass='acmenu' and dcode='z400'");

                // opera凭证导入菜单
                sqlBuidler.AppendLine(
                    @"insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) 
                  values ( 'z10030', 'Opera凭证接口', 'acmenu', 1, 1, 0, '凭证导入', 0, 0,(select id from others where dclass='acrightpass' and dcode='z0110')  , 0, 400,250, 0, 'ImportOpera?exname=txt,.TXT');");

                // 凭证类型 (2020/5/26+预设银行对账单的凭证类型）
                sqlBuidler.AppendLine(
                    @"insert into [dbo].[others]([dcode], [title], [dclass], [isenable], [isinit], [isright], [rver], [ddesc]) values ('z0', '对账单', 'acvouchertype', 1, 1, 0, 0, '预设凭证类型');");

                // 银行帐菜单2020/5/26+预设银行帐菜单）
                sqlBuidler.AppendLine(
                    @"insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) 
                      values ( 'z105', '银行账', 'acmenu', 1, 1, 0, '预设菜单', 0, 1, 0, 0, 0, 0, 0, '');");
                sqlBuidler.AppendLine(
                    @"insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) 
                      values ( 'z10505', '银行账户', 'acmenu', 1, 1, 0, '预设菜单', 0, 0, (select id from others where dclass='acrightpass' and dcode='z0120'), 0, 0, 0, 0, 'AcBank');");
                sqlBuidler.AppendLine(
                    @"insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) 
                      values ( 'z10510', '银行对账', 'acmenu', 1, 1, 0, '预设菜单', 0, 0, (select id from others where dclass='acrightpass' and dcode='z0120'), 0, 0, 0, 0, 'BankRecord');");
                sqlBuidler.AppendLine(
                    @"insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) 
                      values ( 'z10520', '银行对账单', 'acmenu', 1, 1, 0, '预设菜单', 0, 0, (select id from others where dclass='acrightpass' and dcode='z0120'), 0, 0, 0, 0, 'BankVD');");
                sqlBuidler.AppendLine(
                    @"insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) 
                      values ( 'z10530', '自动勾对', 'acmenu', 1, 1, 0, '预设菜单', 0, 0, (select id from others where dclass='acrightpass' and dcode='z0120'), 0, 0, 0, 0, 'BAAutoTickPair');");
                sqlBuidler.AppendLine(
                    @"insert into [dbo].[others]( [dcode], [title], [dclass], [isenable], [isinit], [isright], [ddesc], [rver], [a1], [a2], [a3], [a6], [a7], [a8], [s0]) 
                      values ( 'z10540', '勾对记录', 'acmenu', 1, 1, 0, '预设菜单', 0, 0, (select id from others where dclass='acrightpass' and dcode='z0120'), 0, 0, 0, 0, 'BATickPairRecord');");

                return sqlBuidler.ToString();
            }
        }
    }
}