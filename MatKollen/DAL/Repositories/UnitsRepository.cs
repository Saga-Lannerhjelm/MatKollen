using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatKollen.Models;
using MySql.Data.MySqlClient;

namespace MatKollen.DAL.Repositories
{
    public class UnitsRepository
    {
         private readonly string _connectionString;
        
        public UnitsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public List<MeasurementUnit> GetUnits(out string errorMsg)
        {
            var myConnectionString = _connectionString;
            string query  = "SELECT id, unit, conversion_multiplier, type FROM measurement_units";
            List<MeasurementUnit> unitList = [];

            using (var myConnection = new MySqlConnection(myConnectionString))
            {
                try
                {
                    MySqlCommand myCommand = new MySqlCommand(query, myConnection);
                    myConnection.Open();
                    errorMsg = "";

                    using var reader = myCommand.ExecuteReader();
                    {
                        while (reader.Read())
                        {
                            var unit = new MeasurementUnit()
                            {
                                Id = reader.GetInt32("id"),
                                Unit = reader.GetString("unit"),
                                Multiplier = reader.GetDouble("conversion_multiplier"),
                                Type = reader.GetString("type")
                            };
                            unitList.Add(unit);
                        }
                    }
                    return unitList;
                }
                catch (MySqlException e)
                {
                    errorMsg = e.Message;
                    return null;
                }    
            }
        }
    }
}