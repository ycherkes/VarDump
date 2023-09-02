using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VarDump.CodeDom.Common;

namespace VarDump.Visitor
{
    internal partial class ObjectVisitor //: ExpressionVisitor
    {
        private readonly Dictionary<Expression, CodeExpression> _visited = new();

        private CodeExpression VisitExpression(Expression expression)
        {
            if (_visited.TryGetValue(expression, out var vis))
            {
                return vis;
            }

            if (expression is LambdaExpression lex)
                return VisitLambda(lex);
            if(expression is MethodCallExpression mce)
                return VisitMethodCall(mce);
            if (expression is MemberExpression mae)
                return VisitMemberExpression(mae);
            if (expression is UnaryExpression ue)
                return VisitUnaryExpression(ue);
            if (expression is ParameterExpression pe)
                return VisitParameter(pe);
            if (expression is ConstantExpression ce)
                return VisitConstant(ce);
            if (expression is BinaryExpression be)
                return VisitBinary(be);

            return null;
        }

        private CodeExpression VisitBinary(BinaryExpression be)
        {
            var res = new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(
                        new CodeTypeReference(typeof(Expression), _typeReferenceOptions)),
                    be.NodeType.ToString()),
                Visit(be.Left),
                Visit(be.Right));

            _visited[be] = res;

            return res;
        }

        private CodeExpression VisitConstant(ConstantExpression ce)
        {
            var res = new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(
                        new CodeTypeReference(typeof(Expression), _typeReferenceOptions)),
                    nameof(Expression.Constant)),
                Visit(ce.Value));

            _visited[ce] = res;

            return res;
        }

        private CodeExpression VisitUnaryExpression(UnaryExpression ue)
        {
            var res = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(
                        new CodeTypeReference(typeof(Expression), _typeReferenceOptions)),
                    ue.NodeType.ToString()),
                Visit(ue.Operand),
                VisitType(ue.Type));

            _visited[ue] = res;

            return res;
        }

        private CodeExpression VisitMemberExpression(MemberExpression mae)
        {
            //var pi = typeof(DateTime).GetProperty("Now");
            //var pe = Expression.Property(null, pi);

            CodeExpression res;

            if (mae.Expression != null)
            {

                var objExpression = VisitExpression(mae.Expression);

                res = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(
                        new CodeTypeReferenceExpression(
                            new CodeTypeReference(typeof(Expression), _typeReferenceOptions)),
                        nameof(Expression.Property)),
                    objExpression,
                    VisitPrimitive(mae.Member.Name));

            }
            else
            {
                var prop = new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                        VisitType(mae.Member.DeclaringType),
                        nameof(Type.GetProperty)),
                    VisitPrimitive(mae.Member.Name));

                res = new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                        new CodeTypeReferenceExpression(
                            new CodeTypeReference(typeof(Expression), _typeReferenceOptions)),
                        nameof(Expression.Property)),
                    VisitPrimitive(null),
                    prop);
            }

            _visited[mae] = res;

            return res;
        }

        private CodeExpression VisitLambda(LambdaExpression expression)
        {
            CodeExpression body = VisitExpression(expression.Body);

            CodeExpression[] parameters = expression.Parameters.Select(VisitParameter).ToArray();

            var lambdaCreationParameters = new[]
            {
                body,
                new CodePrimitiveExpression(expression.TailCall)
            };

            if (parameters.Length > 0)
            {
                lambdaCreationParameters = lambdaCreationParameters.Concat(parameters).ToArray();
            }

            var res = new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(
                        new CodeTypeReference(typeof(Expression), _typeReferenceOptions)),
                    nameof(Expression.Lambda)),
                lambdaCreationParameters);

            _visited[expression] = res;

            return res;
        }

        private CodeExpression VisitMethodCall(MethodCallExpression node)
        {
            //Out(".Call ");
            //if (node.Object != null)
            //{
            //    ParenthesizedVisit(node, node.Object);
            //}
            //else if (node.Method.DeclaringType != null)
            //{
            //    Out(node.Method.DeclaringType.ToString());
            //}
            //else
            //{
            //    Out("<UnknownType>");
            //}
            //Out(".");
            //Out(node.Method.Name);
            //VisitExpressions('(', node.Arguments);
            //return node;

            //var objExpression = node.Object != null
            //    ? VisitExpression(node.Object)
            //    : new CodeTypeReferenceExpression(
            //        new CodeTypeReference(node.Method.DeclaringType, _typeReferenceOptions));


            var args = new List<CodeExpression>
            {
                node.Object == null ? VisitPrimitive(null) : VisitExpression(node.Object),
                VisitMethodInfo(node.Method)
            };

            CodeExpression[] parameters = node.Arguments.Select(VisitExpression).ToArray();

            args.AddRange(parameters);

            //return new CodeMethodInvokeExpression(objExpression, node.Method.Name, parameters);

            var res = new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(
                        new CodeTypeReference(typeof(Expression), _typeReferenceOptions)),
                    nameof(Expression.Call)),
                args.ToArray()
            );

            _visited[node] = res;

            return res;
        }

        private CodeExpression VisitMethodInfo(MethodInfo mi)
        {
            //typeof(Regex).GetMethod("IsMatch", new[] { typeof(string), typeof(string) });

            var pars = VisitCollection(mi.GetParameters().Select(x => x.ParameterType).ToArray(), typeof(Type[]));

            var res = new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    VisitType(mi.DeclaringType),
                    nameof(Type.GetMethod)),
                VisitPrimitive(mi.Name),
                pars
            );

            return res;
        }

        private CodeExpression VisitParameter(ParameterExpression parameterExpression)
        {
            var parameters = new List<CodeExpression>
            {
                VisitType(parameterExpression.Type)
            };

            if (parameterExpression.Name != null)
            {
                parameters.Add(new CodePrimitiveExpression(parameterExpression.Name));
            }

            var res = new CodeMethodInvokeExpression(
                new CodeMethodReferenceExpression(
                    new CodeTypeReferenceExpression(
                        new CodeTypeReference(typeof(Expression), _typeReferenceOptions)),
                    nameof(Expression.Parameter)),
                parameters.ToArray()
                    );

            _visited[parameterExpression] = res;

            return res;
        }

        private CodeExpression VisitOrGetParameter(ParameterExpression parameterExpression)
        {
            return _visited.TryGetValue(parameterExpression, out var pe) ? pe : VisitParameter(parameterExpression);
        }
    }
}
