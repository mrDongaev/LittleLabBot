using System;
using System.Data;
using Azure.Core;
using Npgsql;

namespace littlelab_bot
{
    public class DataBaseConnection : IDisposable
    {
        readonly NpgsqlConnection conn = new("User ID=postgres;Password=root;Host=localhost;Port=5432;Database=littlelabdb;Include Error Detail=True");
        public NpgsqlConnection Conn { get { return conn; } }
        public DataBaseConnection() 
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
        }
        public void Dispose() => conn.Dispose();
    }
}
