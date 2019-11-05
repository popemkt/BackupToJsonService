using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;

namespace WindowsService1
{
    class DBService
    {
        static public IEnumerable<Person> ReadDb()
        {
            List<Person> personList = null;
            var connectionString = ConfigurationManager.ConnectionStrings["DBRead"].ConnectionString;
            string queryString =
            "SELECT Id, Name, Phone, Email from Person"
                + " where IsWritten = @IsWritten";

            using (SqlConnection connection =
                new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@IsWritten", false);
                personList = new List<Person>();
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        personList.Add(new Person()
                        {
                            id = reader.GetInt32(0),
                            name = reader.GetString(1),
                            phone = reader.GetString(2),
                            email = reader.GetString(3)
                        });
                    };
                }
                catch (Exception ex)
                {
                    LoggingService.WriteToLog(ex.Message);
                }
            }

            return personList;
        }

        static public bool UpdateDb(IEnumerable<Person> personList)
        {
            if (personList.IsNullOrEmpty())
            {
                LoggingService.WriteToLog("List is null or empty");
                return false;
            }
            bool isUpdated = false;
            string inQuery = String.Join(",", personList.Select(a => a.id.ToString()).ToArray());
            string connectionString = ConfigurationManager.ConnectionStrings["DBRead"].ConnectionString;
            string queryString =
            "UPDATE dbo.Person set IsWritten = @IsWritten"
                + " where Id in (" + (String.IsNullOrEmpty(inQuery) ? "0" : inQuery) + ")";

            using (var connection =
                new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@IsWritten", true);
                try
                {
                    connection.Open();
                    isUpdated = command.ExecuteNonQuery() > 0;
                }
                catch (SqlException ex)
                {
                    LoggingService.WriteToLog(ex.Message);
                }
            }

            return isUpdated;
        }
    }
}
