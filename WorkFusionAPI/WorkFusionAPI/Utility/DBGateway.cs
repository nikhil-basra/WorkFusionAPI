﻿using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.SqlClient;
namespace WorkFusionAPI.Utility
{
    public class DBGateway
    {
        private string _connectionString;

        public DBGateway(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection Connection()
        {
            //IL_0006: Unknown result type (might be due to invalid IL or missing references)
            //IL_000c: Expected O, but got Unknown
            return (IDbConnection)new MySqlConnection(_connectionString);
        }

        public async Task<T> ExeScalarQuery<T>(string QueryText, DynamicParameters paras)
        {
            try
            {
                T result;
                using (IDbConnection conn = Connection())
                {
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }

                    result = await SqlMapper.QueryFirstOrDefaultAsync<T>(conn, QueryText, (object)paras, (IDbTransaction)null, (int?)null, (CommandType?)null);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<int> ExeQuery(string QueryText, DynamicParameters paras)
        {
            try
            {
                int result;
                using (IDbConnection conn = Connection())
                {
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }

                    result = await SqlMapper.ExecuteAsync(conn, QueryText, (object)paras, (IDbTransaction)null, (int?)null, (CommandType?)null);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<T>> ExeQueryList<T>(string QueryText, DynamicParameters paras)
        {
            try
            {
                using IDbConnection conn = Connection();
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                return (await SqlMapper.QueryAsync<T>(conn, QueryText, (object)paras, (IDbTransaction)null, (int?)null, (CommandType?)null)).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<T> ExeScalarQuery<T>(string QueryText)
        {
            try
            {
                using IDbConnection conn = Connection();
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                return await SqlMapper.QueryFirstOrDefaultAsync<T>(conn, QueryText, (object)null, (IDbTransaction)null, (int?)null, (CommandType?)null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<T>> ExeQueryList<T>(string QueryText)
        {
            try
            {
                List<T> result;
                using (IDbConnection conn = Connection())
                {
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }

                    result = (await SqlMapper.QueryAsync<T>(conn, QueryText, (object)null, (IDbTransaction)null, (int?)null, (CommandType?)null)).ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<T> ExeSPScaler<T>(string QueryText, DynamicParameters paras)
        {
            try
            {
                T result;
                using (IDbConnection conn = Connection())
                {
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }

                    CommandType? commandType = CommandType.StoredProcedure;
                    result = await SqlMapper.QueryFirstAsync<T>(conn, QueryText, (object)paras, (IDbTransaction)null, (int?)null, commandType);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<T>> ExeSPList<T>(string QueryText, DynamicParameters paras)
        {
            try
            {
                List<T> result;
                using (IDbConnection conn = Connection())
                {
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }

                    CommandType? commandType = CommandType.StoredProcedure;
                    result = (await SqlMapper.QueryAsync<T>(conn, QueryText, (object)paras, (IDbTransaction)null, (int?)null, commandType)).ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<int> ExeSP(string QueryText, DynamicParameters paras)
        {
            try
            {
                int result;
                using (IDbConnection conn = Connection())
                {
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }

                    CommandType? commandType = CommandType.StoredProcedure;
                    result = await SqlMapper.ExecuteAsync(conn, QueryText, (object)paras, (IDbTransaction)null, (int?)null, commandType);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<dynamic> ExeSPSclarMultiple(string QueryText, DynamicParameters paras)
        {
            using IDbConnection dbConnection = Connection();
            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }

            CommandType? commandType = CommandType.StoredProcedure;
            SqlMapper.GridReader val = SqlMapper.QueryMultiple(dbConnection, QueryText, (object)paras, (IDbTransaction)null, (int?)null, commandType);
            dynamic val2;
            try
            {
                List<object> list = new List<object>();
                while (!val.IsConsumed)
                {
                    list.Add(val.Read(true).ToList());
                }

                val2 = list;
            }
            finally
            {
                ((IDisposable)val)?.Dispose();
            }

            return val2;
        }


        public async Task<T> ExecuteScalarQueryAsync<T>(string query, DynamicParameters parameters)
        {
            using (IDbConnection conn = Connection())
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                return await conn.ExecuteScalarAsync<T>(query, parameters);
            }
        }

        public async Task<int> ExecuteAsync(string query, DynamicParameters parameters)
        {
            using (IDbConnection conn = Connection())
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                return await conn.ExecuteAsync(query, parameters);
            }
        }

        /// <summary>
        /// //Upload profile image connection
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>

        public async Task<int> Executeimage(string query, DynamicParameters parameters)
        {
            using (IDbConnection conn = Connection())
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                return await conn.ExecuteAsync(query, parameters);
            }
        }



        public async Task<int> Executeimagedetails(string query, DynamicParameters parameters)
        {
            try
            {
                using (IDbConnection conn = Connection())
                {
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }
                    return await conn.ExecuteAsync(query, parameters);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        //public async Task<int> ExecuteAsync(string query, DynamicParameters parameters)
        //{
        //    return await _connection.ExecuteAsync(query, parameters);
        //}
        public async Task<IEnumerable<T>> QueryAsync<T>(string query, DynamicParameters parameters)
        {
            using (var connection = Connection())
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                return await connection.QueryAsync<T>(query, parameters);
            }
        }




    }

}
