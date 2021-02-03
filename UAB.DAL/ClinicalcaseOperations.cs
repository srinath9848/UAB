using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAB.DAL.Models;
using UAB.DTO;

namespace UAB.DAL
{
    public class ClinicalcaseOperations
    {
        public List<DashboardDTO> GetChartCountByStatus()
        {
            DashboardDTO dto = new DashboardDTO();
            List<DashboardDTO> lstDto = new List<DashboardDTO>();

            using (var context = new UABContext())
            {
                //var param = new SqlParameter[] {
                //        new SqlParameter() {
                //            ParameterName = "@StatusID",
                //            SqlDbType =  System.Data.SqlDbType.Int,
                //            Direction = System.Data.ParameterDirection.Input,
                //            Value = 1
                //        } };

                using (var cnn = context.Database.GetDbConnection())
                {
                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspGetChartCount]";
                    //cmm.Parameters.AddRange(param);
                    cmm.Connection = cnn;
                    cnn.Open();
                    var reader = cmm.ExecuteReader();

                    while (reader.Read())
                    {
                        dto = new DashboardDTO();
                        dto.ProjectName = Convert.ToString(reader["Name"]);
                        if (reader["StatusId"] != DBNull.Value)
                            dto.StatusID = Convert.ToInt32(reader["StatusId"]);
                            dto.Cnt = Convert.ToInt32(reader["Cnt"]);
                        lstDto.Add(dto);
                    }
                }
            }
            return lstDto;
        }
    }
}
