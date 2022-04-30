﻿using Jvedio.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.SimpleORM
{
    public class SelectWrapper<T> : IWrapper<T>
    {


        /// <summary>
        /// 为空的时候默认为 select * 
        /// </summary>
        protected HashSet<string> SelectColumns { get; set; }
        public string LimitSql { get; set; }

        private class Where
        {

            public bool Or { get; set; }// 后面跟着的是 and 还是 Or，默认 And

            public bool LeftBracket { get; set; } // 左括号
            public bool RightBracket { get; set; } // 左括号

            public string Field { get; set; }

            public List<object> Values { get; set; }
            public WhereCondition Condition { get; set; }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (!(obj is Where)) return false;
                Where other = (Where)obj;
                return Field.Equals(other.Field) &&
                    Values.Count.Equals(other.Values.Count) &&
                    Values.All(other.Values.Contains) &&
                    Condition.Equals(other.Condition);
            }

            public override int GetHashCode()
            {
                return Field.GetHashCode() +
                    Values.GetHashCode() +
                    Condition.GetHashCode();
            }
        }

        protected enum WhereCondition
        {
            Equal,
            GreaterThen,
            GreaterEqual,
            LessThen,
            LessEqual,
            Like,
            NotLike,
            NotEqual,
            Between
        }


        static Dictionary<WhereCondition, string> whereConditions = new Dictionary<WhereCondition, string>()
        {
            { WhereCondition.GreaterThen,">" } ,
            { WhereCondition.GreaterEqual,">=" } ,
            { WhereCondition.LessEqual,"<=" } ,
            { WhereCondition.LessThen,"<" } ,
            { WhereCondition.Equal,"=" } ,
            { WhereCondition.NotEqual,"!=" } ,
        };

        List<Where> Wheres { get; set; }

        string OrderField { get; set; }
        bool SortDesc { get; set; }

        public SelectWrapper()
        {
            Init();
            SelectColumns = new HashSet<string>();
            Wheres = new List<Where>();
        }

        public List<PropertyInfo> existFields = new List<PropertyInfo>();

        public void Init()
        {
            List<PropertyInfo> propertyInfos = typeof(T).GetProperties().ToList();
            List<PropertyInfo> tableFields = propertyInfos.Where(prop => prop.IsDefined(typeof(TableFieldAttribute), false)).ToList();
            foreach (var item in tableFields)
            {
                if (!item.GetCustomAttributes(false).OfType<TableFieldAttribute>().First().exist)
                {
                    propertyInfos.Remove(item);
                }
            }
            existFields = propertyInfos.Select(item => item).ToList();
        }




        public IWrapper<T> GroupBy(string column)
        {
            throw new NotImplementedException();
        }

        public IWrapper<T> Limit(long offset, long row_count)
        {
            LimitSql = $" Limit {offset},{row_count}";
            return this;
        }

        public IWrapper<T> Limit(long row_count)
        {
            throw new NotImplementedException();
        }

        public IWrapper<T> Select(params string[] columns)
        {
            foreach (var item in columns)
            {
                if (item == "*")
                {
                    SelectColumns.Clear();
                    break;
                }
                else
                {
                    SelectColumns.Add(item);
                }
            }
            return this;
        }


        public IWrapper<T> In(string field, IEnumerable<string> items)
        {
            Where where = new Where();
            where.Field = field;
            where.Values = items.OfType<object>().ToList();
            if (!Wheres.Contains(where)) Wheres.Add(where);
            return this;
        }

        public string toSelect(bool existField = true)
        {
            if (SelectColumns.Count == 0) return "SELECT * ";
            StringBuilder builder = new StringBuilder();
            foreach (string field in SelectColumns)
            {
                string name = field;
                int idx = field.IndexOf(" as ");
                if (idx > 0) name = field.Substring(idx + 4).Trim();
                if (existField)
                {
                    if (existFields.Where(item => item.Name == name).Any())
                        builder.Append(field + ",");
                }
                else
                {
                    builder.Append(field + ",");
                }


            }
            if (builder.Length == 0) return "SELECT * ";
            if (builder[builder.Length - 1] == ',') builder.Remove(builder.Length - 1, 1);
            return $"select {builder}";
        }


        public string toWhere(bool exist = true)
        {
            if (Wheres == null || Wheres.Count == 0) return "";
            StringBuilder builder = new StringBuilder();
            foreach (Where where in Wheres)
            {
                List<object> values = where.Values;
                if (values == null || values.Count == 0) continue;
                PropertyInfo propertyInfo = existFields.Where(item => item.Name == where.Field).FirstOrDefault();
                if (exist && propertyInfo == null) continue;
                if (where.LeftBracket) builder.Append(" ( "); // 左括号
                if (values.Count == 1)
                {
                    // 表驱动设计
                    if (whereConditions.ContainsKey(where.Condition))
                    {
                        builder.Append($" {where.Field} {whereConditions[where.Condition]} '{values[0]}'");
                    }
                    else if (where.Condition == WhereCondition.Like)
                    {
                        builder.Append($" {where.Field} like '%{values[0]}%'");
                    }
                    else if (where.Condition == WhereCondition.NotLike)
                    {
                        builder.Append($" {where.Field} not like '%{values[0]}%'");
                    }
                }
                else
                {
                    if (where.Condition == WhereCondition.Between)
                    {
                        builder.Append($" {where.Field} BETWEEN '{values[0]}' AND '{values[1]}'");
                    }
                    else
                    {
                        builder.Append($" {where.Field} in ('{string.Join("','", values)}')");
                    }

                }
                if (where.RightBracket) builder.Append(" ) "); // 左括号
                builder.Append($" {(where.Or ? "or" : "and")}");

            }
            if (builder.Length == 0) return "";
            string result = " where " + builder.ToString();
            if (result.EndsWith(" and")) result = result.Remove(result.LastIndexOf("and"));
            if (result.EndsWith(" or")) result = result.Remove(result.LastIndexOf("or"));
            // 拼接
            return result;
        }

        private Where getWhere(string field, object value, WhereCondition condition)
        {
            Where where = new Where();
            where.Or = false;
            where.Field = field;
            where.Condition = condition;
            where.Values = new List<object> { value };
            return where;
        }

        public IWrapper<T> Eq(string field, object value)
        {
            Where where = getWhere(field, value, WhereCondition.Equal);
            if (!Wheres.Contains(where)) Wheres.Add(where);
            return this;
        }


        public IWrapper<T> NotEq(string field, object value)
        {
            Where where = getWhere(field, value, WhereCondition.NotEqual);
            if (!Wheres.Contains(where)) Wheres.Add(where);
            return this;
        }

        public IWrapper<T> Gt(string field, object value)
        {
            Where where = getWhere(field, value, WhereCondition.GreaterThen);
            if (!Wheres.Contains(where)) Wheres.Add(where);
            return this;
        }

        public IWrapper<T> Ge(string field, object value)
        {
            Where where = getWhere(field, value, WhereCondition.GreaterEqual);
            if (!Wheres.Contains(where)) Wheres.Add(where);
            return this;
        }

        public IWrapper<T> Lt(string field, object value)
        {
            Where where = getWhere(field, value, WhereCondition.LessThen);
            if (!Wheres.Contains(where)) Wheres.Add(where);
            return this;
        }

        public IWrapper<T> Le(string field, object value)
        {
            Where where = getWhere(field, value, WhereCondition.LessEqual);
            if (!Wheres.Contains(where)) Wheres.Add(where);
            return this;
        }

        public IWrapper<T> Like(string field, object value)
        {
            Where where = getWhere(field, value, WhereCondition.Like);
            if (!Wheres.Contains(where)) Wheres.Add(where);
            return this;
        }

        public IWrapper<T> Or()
        {
            if (Wheres.Count <= 0) throw new Exception("where 条件与 and/or 数据不符合");
            else
            {
                Wheres[Wheres.Count - 1].Or = true;
            }
            return this;
        }


        public IWrapper<T> LeftBacket()
        {
            if (Wheres.Count <= 0) throw new Exception("where 条件与 and/or 数据不符合");
            else
            {
                Wheres[Wheres.Count - 1].LeftBracket = true;
            }
            return this;
        }
        public IWrapper<T> RightBacket()
        {
            if (Wheres.Count <= 0) throw new Exception("where 条件与 and/or 数据不符合");
            else
            {
                Wheres[Wheres.Count - 1].RightBracket = true;
            }
            return this;
        }

        public IWrapper<T> Desc(string field)
        {
            OrderField = field;
            SortDesc = true;
            return this;
        }

        public IWrapper<T> Asc(string field)
        {
            OrderField = field;
            SortDesc = false;
            return this;
        }

        public string toOrder()
        {
            if (!string.IsNullOrEmpty(OrderField))
            {
                return $" ORDER BY {OrderField} {(SortDesc ? "DESC" : "ASC")}";
            }
            return "";
        }

        public string toLimit()
        {
            if (!string.IsNullOrEmpty(LimitSql))
                return LimitSql;

            return "";
        }

        public IWrapper<T> Between(string field, object value1, object value2)
        {
            Where where = new Where();
            where.Field = field;
            where.Condition = WhereCondition.Between;
            where.Values = new List<object> { value1, value2 };
            if (!Wheres.Contains(where)) Wheres.Add(where);
            return this;
        }

        public void Join(SelectWrapper<T> wrapper)
        {
            this.Wheres.AddRange(wrapper.Wheres);
        }

        public IWrapper<T> NotIn(string field, IEnumerable<string> items)
        {
            return In(field + " NOT ", items);
        }
    }
}
