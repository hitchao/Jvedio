using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.SimpleORM
{

    public interface IWrapper<T>
    {

        /**
         * 语法：https://dev.mysql.com/doc/refman/5.7/en/select.html
         * SELECT
            [ALL | DISTINCT | DISTINCTROW ]
            [HIGH_PRIORITY]
            [STRAIGHT_JOIN]
            [SQL_SMALL_RESULT] [SQL_BIG_RESULT] [SQL_BUFFER_RESULT]
            [SQL_CACHE | SQL_NO_CACHE] [SQL_CALC_FOUND_ROWS]
            select_expr [, select_expr] ...
            [into_option]
            [FROM table_references
                [PARTITION partition_list]]
            [WHERE where_condition]
            [GROUP BY {col_name | expr | position}
                [ASC | DESC], ... [WITH ROLLUP]]
            [HAVING where_condition]
            [ORDER BY {col_name | expr | position}
                [ASC | DESC], ...]
            [LIMIT {[offset,] row_count | row_count OFFSET offset}]
            [PROCEDURE procedure_name(argument_list)]
            [into_option]
            [FOR UPDATE | LOCK IN SHARE MODE]

        into_option: {
            INTO OUTFILE 'file_name'
                [CHARACTER SET charset_name]
                export_options
            | INTO DUMPFILE 'file_name'
            | INTO var_name [, var_name] ...
        }
         */
        IWrapper<T> Select(params string[] columns);

        IWrapper<T> Eq(string field, object value);
        IWrapper<T> NotEq(string field, object value);
        IWrapper<T> Gt(string field, object value);
        IWrapper<T> Ge(string field, object value);
        IWrapper<T> Lt(string field, object value);
        IWrapper<T> Le(string field, object value);
        IWrapper<T> Like(string field, object value);
        IWrapper<T> GroupBy(string column);
        IWrapper<T> Desc(string field);
        IWrapper<T> Asc(string field);
        IWrapper<T> Limit(long offset, long row_count);
        IWrapper<T> Limit(long row_count);
        IWrapper<T> In(string field, IEnumerable<string> items);
        IWrapper<T> Between(string field, object value1, object value2);


        string toSelect(bool existField = true);
        string toWhere(bool existField = true);
        string toOrder();

    }
}
