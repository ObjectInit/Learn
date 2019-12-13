#region  <<版本注释>>
/* ========================================================== 
// <copyright file="Program_Visit.cs" company="Shiji.BO.CS">
// Copyright (c) Shiji.BO.CS. All rights reserved.
// </copyright>
* 功能描述：Program_Visit 
* 创 建 者：Administrator 
* 创建时间：2019/12/12 17:23:00 
* =============================================================*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Learn.Console.ExpressionTree
{
    public class Program_Visit : IMain
    {
        public void Main(string[] args)
        {
            Expression<Func<int, int, bool>> expre = (x, y) => x - y > 5;
            SnailExpressionVisitor visitor = new SnailExpressionVisitor();
            Expression modifiedExpr = visitor.Visit(expre);

            System.Console.WriteLine($"Lambda的转换最后结果：{modifiedExpr.ToString()}");
            System.Console.ReadLine();
        }
    }

    /// <summary>
    /// 为了更好的理解里面的访问顺序
    /// </summary>
    public class SnailExpressionVisitor : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            System.Console.WriteLine($"访问了 Visit，内容：{node.ToString()}");
            return base.Visit(node);
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {

            System.Console.WriteLine($"访问了 VisitCatchBlock，内容：{node.ToString()}");
            return base.VisitCatchBlock(node);
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            System.Console.WriteLine($"访问了 VisitElementInit，内容：{node.ToString()}");
            return base.VisitElementInit(node);
        }
        protected override LabelTarget VisitLabelTarget(LabelTarget node)
        {

            System.Console.WriteLine($"访问了 VisitLabelTarget，内容：{node.ToString()}");
            return base.VisitLabelTarget(node);
        }
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {

            System.Console.WriteLine($"访问了 VisitMemberAssignment，内容：{node.ToString()}");
            return base.VisitMemberAssignment(node);
        }
        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {

            System.Console.WriteLine($"访问了 VisitMemberBinding，内容：{node.ToString()}");
            return base.VisitMemberBinding(node);
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {

            System.Console.WriteLine($"访问了 VisitMemberListBinding，内容：{node.ToString()}");
            return base.VisitMemberListBinding(node);
        }
        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {

            System.Console.WriteLine($"访问了 VisitMemberMemberBinding，内容：{node.ToString()}");
            return base.VisitMemberMemberBinding(node);
        }
        protected override SwitchCase VisitSwitchCase(SwitchCase node)
        {
            System.Console.WriteLine($"访问了 VisitSwitchCase，内容：{node.ToString()}");
            return base.VisitSwitchCase(node);
        }
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.GreaterThan)
            {
                Expression left = this.Visit(node.Left);
                Expression right = this.Visit(node.Right);

                Expression result = Expression.MakeBinary(ExpressionType.GreaterThanOrEqual,left,right,node.IsLiftedToNull,node.Method);
                System.Console.WriteLine($"访问了 VisitBinary，更改之后的内容：{result.ToString()}");
                return result;
            }
            else if (node.NodeType == ExpressionType.Subtract || node.NodeType == ExpressionType.SubtractChecked)
            {
                Expression left = this.Visit(node.Left);
                Expression right = this.Visit(node.Right);

                var result = Expression.MakeBinary(ExpressionType.Add, left, right, node.IsLiftedToNull, node.Method);
                System.Console.WriteLine($"访问了 VisitBinary，更改之后的内容：{result.ToString()}");
                return result;
            }
            else
            {
                return base.VisitBinary(node);
            }
        }
        protected override Expression VisitBlock(BlockExpression node)
        {
            System.Console.WriteLine($"访问了 VisitBlock，内容：{node.ToString()}");
            return base.VisitBlock(node);
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            System.Console.WriteLine($"访问了 VisitConditional，内容：{node.ToString()}");
            return base.VisitConditional(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            System.Console.WriteLine($"访问了 VisitConstant，内容：{node.ToString()}");
            return base.VisitConstant(node);
        }
        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            System.Console.WriteLine($"访问了 VisitDebugInfo，内容：{node.ToString()}");
            return base.VisitDebugInfo(node);
        }
        protected override Expression VisitDefault(DefaultExpression node)
        {
            System.Console.WriteLine($"访问了 VisitDefault，内容：{node.ToString()}");
            return base.VisitDefault(node);
        }

        protected override Expression VisitDynamic(DynamicExpression node)
        {
            System.Console.WriteLine($"访问了 VisitDynamic，内容：{node.ToString()}");
            return base.VisitDynamic(node);
        }
        protected override Expression VisitExtension(Expression node)
        {
            System.Console.WriteLine($"访问了 VisitExtension，内容：{node.ToString()}");
            return base.VisitExtension(node);
        }
        protected override Expression VisitGoto(GotoExpression node)
        {
            System.Console.WriteLine($"访问了 VisitGoto，内容：{node.ToString()}");
            return base.VisitGoto(node);
        }
        protected override Expression VisitIndex(IndexExpression node)
        {
            System.Console.WriteLine($"访问了 VisitIndex，内容：{node.ToString()}");
            return base.VisitIndex(node);
        }
        protected override Expression VisitInvocation(InvocationExpression node)
        {
            System.Console.WriteLine($"访问了 VisitInvocation，内容：{node.ToString()}");
            return base.VisitInvocation(node);
        }
        protected override Expression VisitLabel(LabelExpression node)
        {
            System.Console.WriteLine($"访问了 VisitLabel，内容：{node.ToString()}");
            return base.VisitLabel(node);
        }
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            System.Console.WriteLine($"访问了 VisitLambda，内容：{node.ToString()}");
            return base.VisitLambda(node);
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            System.Console.WriteLine($"访问了 VisitListInit，内容：{node.ToString()}");
            return base.VisitListInit(node);
        }
        protected override Expression VisitLoop(LoopExpression node)
        {
            System.Console.WriteLine($"访问了 VisitLoop，内容：{node.ToString()}");
            return base.VisitLoop(node);
        }
        protected override Expression VisitMember(MemberExpression node)
        {
            System.Console.WriteLine($"访问了 VisitMember，内容：{node.ToString()}");
            return base.VisitMember(node);
        }
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            System.Console.WriteLine($"访问了 VisitMemberInit，内容：{node.ToString()}");
            return base.VisitMemberInit(node);
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            System.Console.WriteLine($"访问了 VisitMethodCall，内容：{node.ToString()}");
            return base.VisitMethodCall(node);
        }
        protected override Expression VisitNew(NewExpression node)
        {
            System.Console.WriteLine($"访问了 VisitNew，内容：{node.ToString()}");
            return base.VisitNew(node);
        }
        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            System.Console.WriteLine($"访问了 VisitNewArray，内容：{node.ToString()}");
            return base.VisitNewArray(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            System.Console.WriteLine($"访问了 VisitParameter，内容：{node.ToString()}");
            return base.VisitParameter(node);
        }
        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            System.Console.WriteLine($"访问了 VisitRuntimeVariables，内容：{node.ToString()}");
            return base.VisitRuntimeVariables(node);
        }

        protected override Expression VisitSwitch(SwitchExpression node)
        {
            System.Console.WriteLine($"访问了 VisitSwitch，内容：{node.ToString()}");
            return base.VisitSwitch(node);
        }
        protected override Expression VisitTry(TryExpression node)
        {
            System.Console.WriteLine($"访问了 VisitTry，内容：{node.ToString()}");
            return base.VisitTry(node);
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            System.Console.WriteLine($"访问了 VisitTypeBinary，内容：{node.ToString()}");
            return base.VisitTypeBinary(node);
        }
        protected override Expression VisitUnary(UnaryExpression node)
        {
            System.Console.WriteLine($"访问了 VisitUnary，内容：{node.ToString()}");
            return base.VisitUnary(node);
        }

    }
}
