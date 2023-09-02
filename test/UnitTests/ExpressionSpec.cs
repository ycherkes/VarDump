using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using UnitTests.TestModel;
using VarDump;
using Xunit;

namespace UnitTests
{
    public class ExpressionSpec
    {
        [Fact]
        public void DumpExpressionCSharp()
        {
            var ent = new entity { sub_entity = new subEntity { property = "123", date = DateTime.Now.Date } };

            var expr = (Expression<Func<entity,bool>>)CreateRegExExpression<entity>("123", "sub_entity.property");

            var f = expr.Compile();

            var res = f(ent);

            //var expr1 = Expression.Lambda<Func<Person, bool>>(Regex.IsMatch(Expression.TypeAs(Expression.Parameter(typeof(Person)), typeof(Person)).FirstName, Expression.Constant("123")), false, Expression.Parameter(typeof(Person)));

            //string pattern = "123";

            Expression<Func<entity, bool>> lambda = e => Get(e).sub_entity.date == DateTime.Now.Date.AddTicks(1);

            var eParam = Expression.Parameter(typeof(entity), "e");

            var lambda1 = Expression.Lambda<Func<entity, bool>>(Expression.Equal(Expression.Property(Expression.Property(eParam, "sub_entity"), "date"), Expression.Property(Expression.Property(null, typeof(DateTime).GetProperty("Now")), "Date")), false, eParam);

            var r1 = lambda1.Compile()(ent);
            var lambda2 = Expression.Lambda<Func<entity, bool>>(Expression.Equal(Expression.Property(Expression.Property(eParam, "sub_entity"), "date"), Expression.Call(Expression.Property(Expression.Property(null, typeof(DateTime).GetProperty("Now")), "Date"), typeof(DateTime).GetMethod("AddTicks", new Type[]
            {
                typeof(long)
            }), Expression.Constant(1L))), false, eParam);

            var r2 = lambda2.Compile()(ent);
            var getMethod = typeof(ExpressionSpec).GetMethod("Get", new Type[]
            {
                typeof(entity)
            });

            var m = GetMethod(() => Get(default(entity)));

            var m1 = GetMethod(() => GetMethod(default));

            var dumper = new CSharpDumper();

            var result = dumper.Dump(lambda);
            Assert.Equal(
                @"var dnsEndPoint = new DnsEndPoint(""google.com"", 12345);
", result);


        }

        public static MethodInfo GetMethod<T>(Expression<Action<T>> method)
        {
            if (Unwrap(method.Body) is not MethodCallExpression expression)
            {
                throw new ArgumentException($"Argument {nameof(method)} needs to contain a method call", nameof(method));
            }

            return expression.Method;
        }

        public static MethodInfo GetMethod(Expression<Action> method)
        {
            if (Unwrap(method.Body) is not MethodCallExpression expression)
            {
                throw new ArgumentException($"Argument {nameof(method)} needs to contain a method call", nameof(method));
            }

            return expression.Method;
        }

        private static Expression Unwrap(Expression expression)
        {
            return expression.NodeType == ExpressionType.Convert ? ((UnaryExpression)expression).Operand : expression;
        }

        private static T Get<T>(T input)
        {
            return input;
        }

        class entity
        {
            public subEntity sub_entity { get; set; }
        }

        class subEntity
        {
            public string property { get; set; }
            public DateTime date { get; set; }
        }


        static LambdaExpression CreateRegExExpression<T>(string pattern, string property)
        {
            var paramObject = Expression.Parameter(typeof(T));
            var paramType = Expression.TypeAs(paramObject, typeof(T));

            var props = property.Split('.').ToArray();

            Expression propertyField = Expression.Property(paramType, props[0]);

            for (var i = 1; i < props.Length; i++)
            {
                propertyField = Expression.Property(propertyField, props[i]);
            }

            var patternEx = Expression.Constant(pattern, typeof(string));
            var paramsEx = new[] { propertyField, patternEx };

            var methodInfo = typeof(Regex).GetMethod("IsMatch", new[] { typeof(string), typeof(string) });
            if (methodInfo == null || !methodInfo.IsStatic)
                throw new NotSupportedException();

            var lambdaBody = Expression.Call(null, methodInfo, paramsEx);
            var expr0 = Expression.Lambda<Func<T, bool>>(lambdaBody, paramObject);

            var par = Expression.Parameter(typeof(entity));

            var expr1 = Expression.Lambda(Expression.Call(null, typeof(Regex).GetMethod("IsMatch", new Type[]
            {
                typeof(string),
                typeof(string)
            }), Expression.Property(Expression.Property(Expression.TypeAs(par, typeof(entity)), "sub_entity"), "property"), Expression.Constant("123")), false, par);

            var eparam = Expression.Parameter(typeof(entity), "e");

            var expr2 = Expression.Lambda(Expression.Call(null, typeof(Regex).GetMethod("IsMatch", new Type[]
            {
                typeof(string),
                typeof(string)
            }), Expression.Property(Expression.Property(eparam, "sub_entity"), "property"), Expression.Constant("123")), false, eparam);

            //var expr3 = Expression.Lambda(Expression.Equal(Expression.Property(Expression.Property("DateTime", "Now"), "Date"), Expression.Property(Expression.Property("DateTime", "Now"), "Date")), false, Expression.Parameter(typeof(entity), "e"));


            return expr2;
        }
    }
}
