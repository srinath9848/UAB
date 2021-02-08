using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UAB.DAL.Models;
using UAB.DTO;

namespace UAB.DAL
{
    public class ClinicalcaseOperations
    {
        public List<DashboardDTO> GetChartCountByRole(string Role)
        {
            DashboardDTO dto = new DashboardDTO();
            List<DashboardDTO> lstDto = new List<DashboardDTO>();

            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                        new SqlParameter() {
                            ParameterName = "@Role",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = Role
                        }};

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspGetChartCountByRole]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        dto = new DashboardDTO();
                        dto.ProjectID = Convert.ToInt32(reader["ProjectID"]);
                        dto.ProjectName = Convert.ToString(reader["Name"]);
                        dto.AvailableCharts = Convert.ToInt32(reader["AvailableCharts"]);
                        dto.RebuttalCharts = Convert.ToInt32(reader["RebuttalCharts"]);
                        dto.ReadyForPostingCharts = Convert.ToInt32(reader["ReadyForPostingCharts"]);
                        lstDto.Add(dto);
                    }
                }
            }
            return lstDto;
        }
        public ChartSummaryDTO GetNext(string Role, string ChartType, int projectID)
        {
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();

            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@ProjectID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = projectID
                        },
                        new SqlParameter() {
                            ParameterName = "@Role",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = Role
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@ChartType",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = ChartType
                        }};

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[USPGetNext]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        chartSummaryDTO.CodingDTO.ClinicalCaseID = Convert.ToInt32(reader["ClinicalCaseID"]);
                        chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        chartSummaryDTO.CodingDTO.DateOfService = Convert.ToString(reader["DateOfService"]);
                    }
                }
            }
            return chartSummaryDTO;
        }
        public CodingDTO SubmitCoding(ChartSummaryDTO chartSummaryDTO)
        {
            CodingDTO dto = new CodingDTO();

            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@PayorID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.PayorID
                        },
                      new SqlParameter() {
                            ParameterName = "@NoteTitle",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.NoteTitle
                        },
                        new SqlParameter() {
                            ParameterName = "@ProviderID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ProviderID
                        },
                         new SqlParameter() {
                            ParameterName = "@CPTCode",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.CPTCode
                        },
                        new SqlParameter() {
                            ParameterName = "@Mod",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.Mod
                        }
                        ,  new SqlParameter() {
                            ParameterName = "@Dx",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.Dx
                        } , new SqlParameter() {
                            ParameterName = "@ProviderFeedbackID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ProviderFeedbackID
                        }, new SqlParameter() {
                            ParameterName = "@CoderQuestion",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.CoderQuestion
                        } ,   new SqlParameter() {
                            ParameterName = "@ClinicalcaseID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.CodingDTO.ClinicalCaseID
                        },   new SqlParameter() {
                            ParameterName = "@AssignedTo",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.AssignedTo
                        }
                };

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspSubmitCoding]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();

                    int res = cmd.ExecuteNonQuery();
                }
            }
            return dto;
        }

        #region binding data
        public List<BindDTO> GetPayorsList()
        {
            List<BindDTO> lstDto = new List<BindDTO>();
            using (var context = new UABContext())
            {
                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[uspgetpayor]";
                    cmd.Connection = con;

                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        BindDTO dto = new BindDTO()
                        {
                            ID = Convert.ToInt32(reader["PayorId"]),
                            Name = Convert.ToString(reader["Name"])
                        };
                        lstDto.Add(dto);
                    }
                }
            }
            return lstDto;
        }

        public List<BindDTO> GetProvidersList()
        {
            List<BindDTO> lstDto = new List<BindDTO>();
            using (var context = new UABContext())
            {
                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[uspgetprovider]";
                    cmd.Connection = con;

                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        BindDTO dto = new BindDTO()
                        {
                            ID = Convert.ToInt32(reader["ProviderID"]),
                            Name = Convert.ToString(reader["Name"])
                        };
                        lstDto.Add(dto);
                    }
                }
            }
            return lstDto;
        }

        public List<BindDTO> GetProviderFeedbacksList()
        {
            List<BindDTO> lstDto = new List<BindDTO>();
            using (var context = new UABContext())
            {
                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspGetProviderFeedback]";
                    cmd.Connection = con;

                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        BindDTO dto = new BindDTO()
                        {
                            ID = Convert.ToInt32(reader["ProviderFeedbackId"]),
                            Name = Convert.ToString(reader["Feedback"])
                        };
                        lstDto.Add(dto);
                    }
                }
            }
            return lstDto;
        }
        #endregion

        public List<Provider> GetProviders()
        {
            Provider provider = new Provider();
            List<Provider> lstProvider = new List<Provider>();

            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspGetProvider]";
                    //cmm.Parameters.AddRange(param);
                    cmm.Connection = cnn;
                    cnn.Open();
                    var reader = cmm.ExecuteReader();

                    while (reader.Read())
                    {
                        provider = new Provider();
                        provider.ProviderId = Convert.ToInt32(reader["ProviderID"]);
                        provider.Name = Convert.ToString(reader["Name"]);
                        lstProvider.Add(provider);
                    }
                }
            }
            return lstProvider;
        }

        public void AddProvider(Provider provider)
        {
            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    //SqlCommand cmd = new SqlCommand("UspAddProvider");
                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspAddProvider]";
                    cmm.Connection = cnn;

                    SqlParameter name = new SqlParameter();
                    name.ParameterName = "@Name";
                    name.Value = provider.Name;
                    cmm.Parameters.Add(name);

                    cnn.Open();
                    cmm.ExecuteNonQuery();
                }
            }
        }

        public void AddProviderFeedback(ProviderFeedback providerFeedback)
        {
            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    //SqlCommand cmd = new SqlCommand("UspAddProvider");
                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspAddProviderFeedback]";
                    cmm.Connection = cnn;

                    SqlParameter feedback = new SqlParameter();
                    feedback.ParameterName = "@Feedback";
                    feedback.Value = providerFeedback.Feedback;
                    cmm.Parameters.Add(feedback);

                    cnn.Open();
                    cmm.ExecuteNonQuery();
                }
            }
        }

        public List<Payor> GetPayors()
        {
            Payor payor = new Payor();
            List<Payor> lstPayor = new List<Payor>();

            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspGetPayor]";
                    //cmm.Parameters.AddRange(param);
                    cmm.Connection = cnn;
                    cnn.Open();
                    var reader = cmm.ExecuteReader();

                    while (reader.Read())
                    {
                        payor = new Payor();
                        payor.Name = Convert.ToString(reader["Name"]);
                        lstPayor.Add(payor);
                    }
                }
            }
            return lstPayor;
        }

        public void AddPayor(Payor payor)
        {
            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspAddPayor]";
                    cmm.Connection = cnn;

                    SqlParameter name = new SqlParameter();
                    name.ParameterName = "@Name";
                    name.Value = payor.Name;
                    cmm.Parameters.Add(name);

                    cnn.Open();
                    cmm.ExecuteNonQuery();
                }
            }
        }
    }
}
