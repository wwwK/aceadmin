using System.Collections.Generic;
using System.Data;

namespace Netnr.DataKit.Application
{
    /// <summary>
    /// Oracle
    /// </summary>
    public class DataKitOracleService : IDataKitService
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string connectionString;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="conn">连接字符串</param>
        public DataKitOracleService(string conn)
        {
            connectionString = conn;
        }

        /// <summary>
        /// 获取所有表信息的SQL脚本
        /// </summary>
        public string GetTableSQL()
        {
            return @"  
                    SELECT
                        A.table_name AS TableName,
                        B.comments AS TableComment
                    FROM
                        user_tables A,
                        user_tab_comments B
                    WHERE
                        A.table_name = B.table_name
                    ORDER BY A.table_name
                    ";
        }

        /// <summary>
        /// 获取所有列信息的SQL脚本
        /// </summary>
        /// <param name="sqlWhere">SQL条件</param>
        /// <returns></returns>
        public string GetColumnSQL(string sqlWhere)
        {
            return $@"
                    SELECT
                        A.TABLE_NAME AS TableName,
                        B.COMMENTS AS TableComment,
                        C.COLUMN_NAME AS FieldName,
                        C.DATA_TYPE || '(' || CASE
                        WHEN C.CHARACTER_SET_NAME = 'NCHAR_CS' THEN C.DATA_LENGTH / 2
                        ELSE C.DATA_LENGTH
                        END || ')' AS DataTypeLength,
                        C.DATA_TYPE AS DataType,
                        CASE
                        WHEN C.CHARACTER_SET_NAME = 'NCHAR_CS' THEN C.DATA_LENGTH / 2
                        WHEN C.DATA_TYPE = 'NUMBER' THEN C.DATA_PRECISION
                        ELSE C.DATA_LENGTH
                        END AS DataLength,
                        C.DATA_SCALE AS DataScale,
                        C.COLUMN_ID AS FieldOrder,
                        DECODE(PK.COLUMN_NAME, C.COLUMN_NAME, 'YES', '') AS PrimaryKey,
                        DECODE(C.NULLABLE, 'N', 'YES', '') AS NotNull,
                        C.DATA_DEFAULT AS DefaultValue,
                        D.COMMENTS AS FieldComment
                    FROM
                        USER_TABLES A
                        LEFT JOIN USER_TAB_COMMENTS B ON A.TABLE_NAME = B.TABLE_NAME
                        LEFT JOIN USER_TAB_COLUMNS C ON A.TABLE_NAME = C.TABLE_NAME
                        LEFT JOIN USER_COL_COMMENTS D ON A.TABLE_NAME = D.TABLE_NAME
                        AND C.COLUMN_NAME = D.COLUMN_NAME
                        LEFT JOIN (
                        SELECT
                            E.TABLE_NAME,
                            F.COLUMN_NAME
                        FROM
                            USER_CONSTRAINTS E
                            LEFT JOIN USER_CONS_COLUMNS F ON E.TABLE_NAME = F.TABLE_NAME
                            AND E.CONSTRAINT_NAME = F.CONSTRAINT_NAME
                        WHERE
                            E.CONSTRAINT_TYPE = 'P'
                        ) PK ON PK.TABLE_NAME = A.TABLE_NAME
                        AND C.COLUMN_NAME = PK.COLUMN_NAME
                    WHERE
                        1 = 1 {sqlWhere} 
                    ORDER BY
                        A.TABLE_NAME,
                        C.COLUMN_ID
                ";
        }

        /// <summary>
        /// 设置表注释的SQL脚本
        /// </summary>
        /// <param name="dataTableName">表名</param>
        /// <param name="comment">注释内容</param>
        /// <returns></returns>
        public string SetTableCommentSQL(string dataTableName, string comment)
        {
            return $"comment on table \"{dataTableName}\" is '{comment}'";
        }

        /// <summary>
        /// 设置列注释的SQL脚本
        /// </summary>
        /// <param name="dataTableName">表名</param>
        /// <param name="dataColumnName">列名</param>
        /// <param name="comment">注释内容</param>
        /// <returns></returns>
        public string SetColumnCommentSQL(string dataTableName, string dataColumnName, string comment)
        {
            return $"comment on column \"{dataTableName}\".\"{dataColumnName}\" is '{comment}'";
        }

        /// <summary>
        /// 获取所有列
        /// </summary>
        /// <param name="listTableName">表名</param>
        /// <returns></returns>
        public List<Model.DkTableColumn> GetColumn(List<string> listTableName = null)
        {
            var whereSql = string.Empty;

            if (listTableName?.Count > 0)
            {
                listTableName.ForEach(x => x = x.Replace("'", ""));

                whereSql = "AND A.TABLE_NAME IN ('" + string.Join("','", listTableName) + "')";
            }

            var sql = GetColumnSQL(whereSql);
            var ds = new Data.Oracle.OracleHelper(connectionString).Query(sql);

            var list = ds.Tables[0].ToModel<Model.DkTableColumn>();

            return list;
        }

        /// <summary>
        /// 获取所有表
        /// </summary>
        /// <returns></returns>
        public List<Model.DkTableName> GetTable()
        {
            var ds = new Data.Oracle.OracleHelper(connectionString).Query(GetTableSQL());

            var list = ds.Tables[0].ToModel<Model.DkTableName>();

            return list;
        }

        /// <summary>
        /// 设置表注释
        /// </summary>
        /// <param name="TableName">表名</param>
        /// <param name="TableComment">表注释</param>
        /// <returns></returns>
        public bool SetTableComment(string TableName, string TableComment)
        {
            var sql = SetTableCommentSQL(TableName.Replace("\"", ""), TableComment.Replace("'", "''"));
            new Data.Oracle.OracleHelper(connectionString).ExecuteNonQuery(sql);
            return true;
        }

        /// <summary>
        /// 设置列注释
        /// </summary>
        /// <param name="TableName">表名</param>
        /// <param name="FieldName">列名</param>
        /// <param name="FieldComment">列注释</param>
        /// <returns></returns>
        public bool SetColumnComment(string TableName, string FieldName, string FieldComment)
        {
            var sql = SetColumnCommentSQL(TableName.Replace("\"", ""), FieldName.Replace("\"", ""), FieldComment.Replace("'", "''"));
            new Data.Oracle.OracleHelper(connectionString).ExecuteNonQuery(sql);
            return true;
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="TableName">表名</param>
        /// <param name="page">页码</param>
        /// <param name="rows">页量</param>
        /// <param name="sort">排序字段</param>
        /// <param name="order">排序方式</param>
        /// <param name="listFieldName">查询列，默认为 *</param>
        /// <param name="whereSql">条件</param>
        /// <param name="total">返回总条数</param>
        /// <returns></returns>
        public DataTable GetData(string TableName, int page, int rows, string sort, string order, string listFieldName, string whereSql, out int total)
        {
            if (listFieldName == "*")
            {
                listFieldName = "t.*";
            }

            var countWhere = string.Empty;
            if (string.IsNullOrWhiteSpace(whereSql))
            {
                whereSql = "";
            }
            else
            {
                countWhere = "WHERE " + whereSql;
                whereSql = "AND " + whereSql;
            }

            var sql = $@"
                        SELECT
                          *
                        FROM
                          (
                            SELECT
                              ROWNUM AS rowno,{listFieldName}
                            FROM
                              {TableName} t
                            WHERE
                              ROWNUM <= {(page * rows)} {whereSql}
                            ORDER BY {sort} {order}
                          ) table_alias
                        WHERE
                          table_alias.rowno >= {((page - 1) * rows + 1)}";

            var sqlTotal = $"select count(1) as total from {TableName} {countWhere}";

            var dt = new Data.Oracle.OracleHelper(connectionString).Query(sql).Tables[0];
            var dtTotal = new Data.Oracle.OracleHelper(connectionString).Query(sqlTotal).Tables[0].Rows[0][0].ToString();
            int.TryParse(dtTotal, out total);
            return dt;
        }
    }
}
