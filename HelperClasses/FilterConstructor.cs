using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Common.Lib.HelperClasses
{
    public class FilterConstructor : ExpressionVisitor
    {
        private static readonly Dictionary<ExpressionType, string> _logicalOperators;
        private static readonly Dictionary<Type, Func<object, string>> _typeConverters;
        static FilterConstructor()
        {
            //mappings for table, shown above
            _logicalOperators = new Dictionary<ExpressionType, string>
            {
                [ExpressionType.Not] = "NOT",
                [ExpressionType.GreaterThan] = ">",
                [ExpressionType.GreaterThanOrEqual] = ">=",
                [ExpressionType.LessThan] = "<",
                [ExpressionType.LessThanOrEqual] = "<=",
                [ExpressionType.Equal] = "=",
                [ExpressionType.Not] = "NOT",
                [ExpressionType.AndAlso] = "AND",
                [ExpressionType.OrElse] = "OR"
            };

            //if type is string we will wrap it into single quotes
            //if it is a DateTime we will format it like datetime'2008-07-10T00:00:00Z'
            //bool.ToString() returns "True" or "False" with first capital letter, so .ToLower() is applied
            //if it is one of the rest "simple" types we will just call .ToString() method on it
            _typeConverters = new Dictionary<Type, Func<object, string>>
            {
                [typeof(string)] = x => $"'{x}'",
                [typeof(DateTime)] =
                  x => $"datetime'{((DateTime)x).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}'",
                [typeof(bool)] = x => x.ToString().ToLower()
            };
        }

        private StringBuilder _queryStringBuilder;
        private Stack<string> _fieldNames;
        public FilterConstructor()
        {
            //here we will collect our query
            _queryStringBuilder = new StringBuilder();
            //will be discussed below
            _fieldNames = new Stack<string>();
        }

        //entry point
        public string GetQuery(LambdaExpression predicate)
        {
            //Visit transfer abstract Expression to concrete method, like VisitUnary
            //it's invocation chain (at case of unary operator) approximetely looks this way:
            //inside visitor: predicate.Body.Accept(ExpressionVisitor this)
            //inside expression(visitor is this from above): visitor.VisitUnary(this) 
            //here this is Expression
            //we not pass whole predicate, just Body, because we not need predicate.Parameters: "x =>" part
            Visit(predicate.Body);
            var query = _queryStringBuilder.ToString();
            _queryStringBuilder.Clear();

            return query;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            //assume we only allow not (!) unary operator:
            if (node.NodeType != ExpressionType.Not)
                throw new NotSupportedException("Only not(\"!\") unary operator is supported!");

            _queryStringBuilder.Append($"{_logicalOperators[node.NodeType]} ");//!

            _queryStringBuilder.Append("(");                                   //(!
                                                                               //go down from a tree
            Visit(node.Operand);                                               //(!expression
            _queryStringBuilder.Append(")");                                   //(!expression)

            //we should return expression, it will allow to create new expression based on existing one,
            //but, at our case, it is not needed, so just return initial node argument
            return node;
        }

        //corresponds to: and, or, greater than, less than, etc.
        protected override Expression VisitBinary(BinaryExpression node)
        {
            _queryStringBuilder.Append("(");                                    //(
                                                                                //left side of binary operator
            Visit(node.Left);                                                   //(leftExpr

            _queryStringBuilder.Append($" {_logicalOperators[node.NodeType]} ");//(leftExpr and

            //right side of binary operator
            Visit(node.Right);                                                  //(leftExpr and RighExpr
            _queryStringBuilder.Append(")");                                    //(leftExpr and RighExpr)

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            //corresponds to: order.Customer, .order, today variables
            //when we pass parameters to expression via closure, CLR internally creates class:

            //class NameSpace+<>c__DisplayClass12_0
            //{
            //    public Order order;
            //    public DateTime today;
            //}

            //which contains values of parameters. When we face order.Customer, it's node.Expression
            //will not have reference to value "Tom", but instead reference to parent (.order), so we
            //will go to it via Visit(node.Expression) and also save node.Member.Name into 
            //Stack(_fieldNames) fo further usage. order.Customer has type ExpressionType.MemberAccess. 
            //.order - ExpressionType.Constant, because it's node.Expression is ExpressionType.Constant
            //(VisitConstant will be called) that is why we can get it's actual value(instance of Order). 
            //Our Stack at this point: "Customer" <- "order". Firstly we will get "order" field value, 
            //when it will be reached, on NameSpace+<>c__DisplayClass12_0 class instance
            //(type.GetField(fieldName)) then value of "Customer" property
            //(type.GetProperty(fieldName).GetValue(input)) on it. We started from 
            //order.Customer Expression then go up via reference to it's parent - "order", get it's value 
            //and then go back - get value of "Customer" property on order. Forward and backward
            //directions, at this case, reason to use Stack structure

            if (node.Expression.NodeType == ExpressionType.Constant
               ||
               node.Expression.NodeType == ExpressionType.MemberAccess)
            {
                _fieldNames.Push(node.Member.Name);
                Visit(node.Expression);
            }
            else
                //corresponds to: x.Customer - just write "Customer"
                _queryStringBuilder.Append(node.Member.Name);
            return node;
        }

        //corresponds to: 1, "Tom", instance of NameSpace+<>c__DisplayClass12_0, instance of Order, i.e.
        //any expression with value
        protected override Expression VisitConstant(ConstantExpression node)
        {
            //just write value
            _queryStringBuilder.Append(GetValue(node.Value));
            return node;
        }

        private string GetValue(object input)
        {
            var type = input.GetType();
            //if it is not simple value
            if (type.IsClass && type != typeof(string))
            {
                //proper order of selected names provided by means of Stack structure
                var fieldName = _fieldNames.Pop();
                var fieldInfo = type.GetField(fieldName);
                object value;
                if (fieldInfo != null)
                    //get instance of order    
                    value = fieldInfo.GetValue(input);
                else
                    //get value of "Customer" property on order
                    value = type.GetProperty(fieldName).GetValue(input);
                return GetValue(value);
            }
            else
            {
                //our predefined _typeConverters
                if (_typeConverters.ContainsKey(type))
                    return _typeConverters[type](input);
                else
                    //rest types
                    return input.ToString();
            }
        }
    }
}
