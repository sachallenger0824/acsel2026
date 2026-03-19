using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace DbCheck
{
    class Program
    {
        static void Main()
        {
            Dump(@"..\AcselApp\acselold.db", "old_schema.txt");
            Dump(@"..\AcselApp\acsel.db", "new_schema.txt");
        }
        static void Dump(string dbName, string outName)
        {
            if (!File.Exists(dbName)) { File.WriteAllText(outName, "Missing " + dbName); return; }
            using var conn = new SqliteConnection($"Data Source={dbName}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT sql FROM sqlite_master WHERE type='table'";
            using var reader = cmd.ExecuteReader();
            using var sw = new StreamWriter(outName);
            while (reader.Read())
            {
                sw.WriteLine(reader.GetString(0));
            }
        }
    }
}