using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Mono.Data.Sqlite;

public class PersistenceManager
{
    private SqliteConnection m_dbConnection;

    private bool m_DataBaseExists = false;

    public PersistenceManager()
    {
        string[] files = Directory.GetFileSystemEntries(@".", "*.sqlite");

        foreach (string file in files)
        {
            string filename = Path.GetFileName(file);
            if (filename.Contains("NaiveBayesData.sqlite"))
            {
                m_DataBaseExists = true;
            }
        }
    }

    public void Connect()
    {
        if (!m_DataBaseExists)
        {
            SqliteConnection.CreateFile("NaiveBayesData.sqlite");
        }
        m_dbConnection = new SqliteConnection("Data Source=NaiveBayesData.sqlite; Version=3;");
        m_dbConnection.Open();
    }

    public void Close()
    {
        m_dbConnection.Close();
    }

    public void CreateTable(string tableName, string[] tableColumns)
    {
        StringBuilder sb = new StringBuilder();
        string space = " ";
        string commaspace = ", ";
        string openParentheses = "(";
        string closeParentheses = ")";

        try
        {
            sb.Append("create table");
            sb.Append(space);
            sb.Append(tableName);
            sb.Append(space);
            sb.Append(openParentheses);

            int idx = 0;
            bool isColumnName = true;
            int numberOfCommands = tableColumns.Count() - 1;
            foreach (string columnValue in tableColumns)
            {
                if (idx < numberOfCommands)
                {
                    if (isColumnName)
                    {
                        sb.Append(columnValue);
                        sb.Append(space);
                    }
                    else
                    {
                        sb.Append(columnValue);
                        sb.Append(commaspace);
                    }
                    isColumnName = !isColumnName;
                }
                else
                {
                    sb.Append(columnValue);
                }
                idx++;
            }
            sb.Append(closeParentheses);

            string SqlCreateTableCommand = sb.ToString();

            Console.WriteLine(SqlCreateTableCommand);
            ExecuteCommand(SqlCreateTableCommand);
        }
        catch (SqliteException ex)
        {
            Console.WriteLine("Create table error: " + ex.Message);
            return;
        }
    }

    public void InsertIntoTable(string tableName, string[] cellNames, string[] cellData)
    {
        StringBuilder sb = new StringBuilder();
        string space = " ";
        string commaspace = ", ";
        string openParentheses = "(";
        string closeParentheses = ")";

        try
        {
            sb.Append("insert into");
            sb.Append(space);
            sb.Append(tableName);
            sb.Append(space);
            sb.Append(openParentheses);

            int idx = 0;
            int numberOfCommands = cellNames.Count() - 1;
            foreach (string cellName in cellNames)
            {
                if (idx < numberOfCommands)
                {
                    sb.Append(cellName);
                    sb.Append(commaspace);
                }
                else
                {
                    sb.Append(cellName);
                }
                idx++;
            }
            sb.Append(closeParentheses);
            sb.Append(space);
            sb.Append("values");
            sb.Append(space);

            // Reset idx and get cell data array length
            idx = 0;
            numberOfCommands = cellData.Count() - 1;

            // Add cell names in parentheses
            sb.Append(openParentheses);
            foreach (string data in cellData)
            {
                if (idx < numberOfCommands)
                {
                    sb.Append(data);
                    sb.Append(commaspace);
                }
                else
                {
                    sb.Append(data);
                }
                idx++;
            }
            sb.Append(closeParentheses);

            string SqlInsertIntoTableCommand = sb.ToString();

            Console.WriteLine(SqlInsertIntoTableCommand);
            ExecuteCommand(SqlInsertIntoTableCommand);
        }
        catch (SqliteException ex)
        {
            Console.WriteLine("Insert error: " + ex.Message);
            return;
        }
    }

    public float SelectOutput(string tableName, int index)
    {
        StringBuilder sb = new StringBuilder();
        try
        {
            sb.AppendFormat("select {0}", index);
            string selectCommand = sb.ToString();
            SqliteCommand select = new SqliteCommand(selectCommand, m_dbConnection);

            select.ExecuteNonQuery();
            SqliteDataReader reader = select.ExecuteReader();
            return reader.GetFloat(1);
        }
        catch (SqliteException ex)
        {
            Console.WriteLine("Select error: " + ex.Message);
            return 0.0f;
        }
    }

    public void DeleteTable(string tableName)
    {
        try
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("drop table {0}", tableName);
            string sqlDeleteCommmand = sb.ToString();
            ExecuteCommand(sqlDeleteCommmand);
        }
        catch (SqliteException ex)
        {
            Console.WriteLine("Delete Table Error: " + ex.Message);
        }
    }

    public void DeleteAllFrom(string tableName)
    {
        try
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("delete from {0}", tableName);
            string sqlDeleteFromCommand = sb.ToString();
            ExecuteCommand(sqlDeleteFromCommand);
        }
        catch (SqliteException ex)
        {
            Console.WriteLine("Delete From Error: " + ex.Message);
        }
    }

    private void ExecuteCommand(string commandText)
    {
        SqliteCommand command = new SqliteCommand(commandText, m_dbConnection);
        command.ExecuteNonQuery();
    }
}