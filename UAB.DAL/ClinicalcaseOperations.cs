using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UAB.DAL.Models;
using UAB.DTO;
using UAB.DAL.LoginDTO;
using System.Linq;

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
                        dto.CoderRebuttalCharts = Convert.ToInt32(reader["CoderRebuttalCharts"]);
                        dto.QARebuttalCharts = Convert.ToInt32(reader["QARebuttalCharts"]);
                        dto.ShadowQARebuttalCharts = Convert.ToInt32(reader["ShadowQARebuttalCharts"]);
                        dto.ReadyForPostingCharts = Convert.ToInt32(reader["ReadyForPostingCharts"]);
                        dto.OnHoldCharts = Convert.ToInt32(reader["OnHoldCharts"]);
                        lstDto.Add(dto);
                    }
                }
            }
            return lstDto;
        }
        public ChartSummaryDTO GetNext(string Role, string ChartType, int projectID)
        {
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            chartSummaryDTO.ProjectID = projectID;

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
                        }
                         ,   new SqlParameter() {
                            ParameterName = "@UserId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = Auth.UserId
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

                        if ((Role == "QA" && ChartType == "Available") ||
                            (Role == "Coder" && ChartType == "ReadyForPosting") ||
                            (Role == "ShadowQA" && ChartType == "Available") ||
                            (Role == "QA" && ChartType == "OnHold"))
                        {
                            if (reader["ProviderId"] != DBNull.Value)
                                chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            if (reader["PayorId"] != DBNull.Value)
                                chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            if (reader["ProviderFeedbackId"] != DBNull.Value)
                                chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackId"]);
                            if (Role == "QA" && ChartType == "OnHold")
                                chartSummaryDTO.CoderQuestion = Convert.ToString(reader["Question"]);
                        }
                        else if ((Role == "Coder" && ChartType == "Incorrect") ||
                            (Role == "ShadowQA" && ChartType == "RebuttalOfQA") || (Role == "QA" && ChartType == "ShadowQARejected"))
                        {
                            chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            if (reader["QAProviderID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderID = Convert.ToInt32(reader["QAProviderID"]);
                            chartSummaryDTO.QAProviderRemarks = Convert.ToString(reader["QAProviderIDRemark"]);
                            chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            if (reader["QAPayorID"] != DBNull.Value)
                                chartSummaryDTO.QAPayorID = Convert.ToInt32(reader["QAPayorID"]);
                            chartSummaryDTO.QAPayorRemarks = Convert.ToString(reader["QAPayorIdRemark"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.QADx = Convert.ToString(reader["QADx"]);
                            chartSummaryDTO.QADxRemarks = Convert.ToString(reader["QADxRemark"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            chartSummaryDTO.QACPTCode = Convert.ToString(reader["QACPTCode"]);
                            chartSummaryDTO.QACPTCodeRemarks = Convert.ToString(reader["QACPTCodeRemark"]);
                            chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            chartSummaryDTO.QAMod = Convert.ToString(reader["QAMod"]);
                            chartSummaryDTO.QAModRemarks = Convert.ToString(reader["QAModRemark"]);
                            chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackID"]);
                            if (reader["QAProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderFeedbackID = Convert.ToInt32(reader["QAProviderFeedbackID"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QAProviderFeedbackIDRemark"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);

                            chartSummaryDTO.RevisedPayorRemarks = Convert.ToString(reader["RebuttedPayorIdRemark"]);
                            chartSummaryDTO.RevisedProviderRemarks = Convert.ToString(reader["RebuttedProviderIDRemark"]);
                            chartSummaryDTO.RevisedCPTRemarks = Convert.ToString(reader["RebuttedCPTCodeRemark"]);
                            chartSummaryDTO.RevisedModRemarks = Convert.ToString(reader["RebuttedModRemark"]);
                            chartSummaryDTO.RevisedDXRemarks = Convert.ToString(reader["RebuttedDxRemark"]);
                            chartSummaryDTO.RevisedProviderFeedbackRemarks = Convert.ToString(reader["RebuttedProviderFeedbackIDRemark"]);

                        }
                        else if (Role == "QA" && ChartType == "RebuttalOfCoder")
                        {
                            chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            if (reader["QAProviderID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderID = Convert.ToInt32(reader["QAProviderID"]);
                            chartSummaryDTO.QAProviderRemarks = Convert.ToString(reader["QAProviderIDRemark"]);
                            chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            if (reader["QAPayorID"] != DBNull.Value)
                                chartSummaryDTO.QAPayorID = Convert.ToInt32(reader["QAPayorID"]);
                            chartSummaryDTO.QAPayorRemarks = Convert.ToString(reader["QAPayorIdRemark"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.QADx = Convert.ToString(reader["QADx"]);
                            chartSummaryDTO.QADxRemarks = Convert.ToString(reader["QADxRemark"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            chartSummaryDTO.QACPTCode = Convert.ToString(reader["QACPTCode"]);
                            chartSummaryDTO.QACPTCodeRemarks = Convert.ToString(reader["QACPTCodeRemark"]);
                            chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            chartSummaryDTO.QAMod = Convert.ToString(reader["QAMod"]);
                            chartSummaryDTO.QAModRemarks = Convert.ToString(reader["QAModRemark"]);
                            chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackID"]);
                            if (reader["QAProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderFeedbackID = Convert.ToInt32(reader["QAProviderFeedbackID"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QAProviderFeedbackIDRemark"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);

                            chartSummaryDTO.RevisedPayorRemarks = Convert.ToString(reader["RebuttedPayorIdRemark"]);
                            chartSummaryDTO.RevisedProviderRemarks = Convert.ToString(reader["RebuttedProviderIDRemark"]);
                            chartSummaryDTO.RevisedCPTRemarks = Convert.ToString(reader["RebuttedCPTCodeRemark"]);
                            chartSummaryDTO.RevisedModRemarks = Convert.ToString(reader["RebuttedModRemark"]);
                            chartSummaryDTO.RevisedDXRemarks = Convert.ToString(reader["RebuttedDxRemark"]);
                            chartSummaryDTO.RevisedProviderFeedbackRemarks = Convert.ToString(reader["RebuttedProviderFeedbackIDRemark"]);
                        }
                        else if (Role == "QA" && ChartType == "ShadowQARejected")
                        {
                            chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            if (reader["QAProviderID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderID = Convert.ToInt32(reader["QAProviderID"]);
                            chartSummaryDTO.QAProviderRemarks = Convert.ToString(reader["QAProviderIDRemark"]);
                            chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            if (reader["QAPayorID"] != DBNull.Value)
                                chartSummaryDTO.QAPayorID = Convert.ToInt32(reader["QAPayorID"]);
                            chartSummaryDTO.QAPayorRemarks = Convert.ToString(reader["QAPayorIdRemark"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.QADx = Convert.ToString(reader["QADx"]);
                            chartSummaryDTO.QADxRemarks = Convert.ToString(reader["QADxRemark"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            chartSummaryDTO.QACPTCode = Convert.ToString(reader["QACPTCode"]);
                            chartSummaryDTO.QACPTCodeRemarks = Convert.ToString(reader["QACPTCodeRemark"]);
                            chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            chartSummaryDTO.QAMod = Convert.ToString(reader["QAMod"]);
                            chartSummaryDTO.QAModRemarks = Convert.ToString(reader["QAModRemark"]);
                            chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackID"]);
                            if (reader["QAProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderFeedbackID = Convert.ToInt32(reader["QAProviderFeedbackID"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QAProviderFeedbackIDRemark"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);

                            //chartSummaryDTO.RevisedPayorRemarks = Convert.ToString(reader["RebuttedPayorIdRemark"]);
                            //chartSummaryDTO.RevisedProviderRemarks = Convert.ToString(reader["RebuttedProviderIDRemark"]);
                            //chartSummaryDTO.RevisedCPTRemarks = Convert.ToString(reader["RebuttedCPTCodeRemark"]);
                            //chartSummaryDTO.RevisedModRemarks = Convert.ToString(reader["RebuttedModRemark"]);
                            //chartSummaryDTO.RevisedDXRemarks = Convert.ToString(reader["RebuttedDxRemark"]);
                            //chartSummaryDTO.RevisedProviderFeedbackRemarks = Convert.ToString(reader["RebuttedProviderFeedbackIDRemark"]);
                        }
                    }
                }
            }
            return chartSummaryDTO;
        }
        public void SubmitCodingAvailableChart(ChartSummaryDTO chartSummaryDTO)
        {
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
                        }
                       ,   new SqlParameter() {
                            ParameterName = "@UserId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = Auth.UserId
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
        }
        public CodingDTO SubmitQAAvailableChart(ChartSummaryDTO chartSummaryDTO)
        {
            CodingDTO dto = new CodingDTO();

            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@PayorID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAPayorID
                        },
                       new SqlParameter() {
                            ParameterName = "@PayorRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAPayorRemarks
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
                            Value = chartSummaryDTO.QAProviderID
                        },
                        new SqlParameter() {
                            ParameterName = "@ProviderRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAProviderRemarks
                        },
                         new SqlParameter() {
                            ParameterName = "@CPTCode",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QACPTCode
                        },
                         new SqlParameter() {
                            ParameterName = "@CPTCodeRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QACPTCodeRemarks
                        },
                        new SqlParameter() {
                            ParameterName = "@Mod",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAMod
                        },
                        new SqlParameter() {
                            ParameterName = "@ModRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAModRemarks
                        },  new SqlParameter() {
                            ParameterName = "@Dx",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QADx
                        },  new SqlParameter() {
                            ParameterName = "@DxRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QADxRemarks
                        } , new SqlParameter() {
                            ParameterName = "@ProviderFeedbackID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAProviderFeedbackID
                        }, new SqlParameter() {
                            ParameterName = "@ProviderFeedbackRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAProviderFeedbackRemarks
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
                            ParameterName = "@UserId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = Auth.UserId
                        },   new SqlParameter() {
                            ParameterName = "@ErrorTypeID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QADTO.ErrorType
                        }
                };

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspSubmitQA]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();

                    int res = cmd.ExecuteNonQuery();
                }
            }
            return dto;
        }
        public CodingDTO SubmitCodingIncorrectChart(ChartSummaryDTO chartSummaryDTO)
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
                            ParameterName = "@PayorRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.RevisedPayorRemarks
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
                            ParameterName = "@ProviderRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.RevisedProviderRemarks
                        },
                         new SqlParameter() {
                            ParameterName = "@CPTCode",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.CPTCode
                        },
                         new SqlParameter() {
                            ParameterName = "@CPTCodeRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.RevisedCPTRemarks
                        },
                        new SqlParameter() {
                            ParameterName = "@Mod",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.Mod
                        },
                        new SqlParameter() {
                            ParameterName = "@ModRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.RevisedModRemarks
                        },  new SqlParameter() {
                            ParameterName = "@Dx",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.Dx
                        },  new SqlParameter() {
                            ParameterName = "@DxRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.RevisedDXRemarks
                        } , new SqlParameter() {
                            ParameterName = "@ProviderFeedbackID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ProviderFeedbackID
                        }, new SqlParameter() {
                            ParameterName = "@ProviderFeedbackRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.RevisedProviderFeedbackRemarks
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
                            ParameterName = "@UserId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = Auth.UserId
                        },   new SqlParameter() {
                            ParameterName = "@ErrorTypeID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QADTO.ErrorType
                        }
                };

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspSubmitCoderIncorrectChart]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();

                    int res = cmd.ExecuteNonQuery();
                }
            }
            return dto;
        }
        public CodingDTO SubmitCodingReadyForPostingChart(ChartSummaryDTO chartSummaryDTO)
        {
            CodingDTO dto = new CodingDTO();

            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                        new SqlParameter() {
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
                    cmd.CommandText = "[dbo].[UspSubmitApprovedChart]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();

                    int res = cmd.ExecuteNonQuery();
                }
            }
            return dto;
        }

        public CodingDTO SubmitQARebuttalChartsOfCoder(ChartSummaryDTO chartSummaryDTO)
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
                            ParameterName = "@PayorRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAPayorRemarks
                        },
                        new SqlParameter() {
                            ParameterName = "@ProviderID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ProviderID
                        },
                        new SqlParameter() {
                            ParameterName = "@ProviderRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAProviderRemarks
                        },
                         new SqlParameter() {
                            ParameterName = "@CPTCode",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.CPTCode
                        },
                         new SqlParameter() {
                            ParameterName = "@CPTCodeRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QACPTCodeRemarks
                        },
                        new SqlParameter() {
                            ParameterName = "@Mod",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.Mod
                        },
                        new SqlParameter() {
                            ParameterName = "@ModRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAModRemarks
                        },  new SqlParameter() {
                            ParameterName = "@Dx",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.Dx
                        },  new SqlParameter() {
                            ParameterName = "@DxRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QADxRemarks
                        } , new SqlParameter() {
                            ParameterName = "@ProviderFeedbackID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ProviderFeedbackID
                        }, new SqlParameter() {
                            ParameterName = "@ProviderFeedbackRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAProviderFeedbackRemarks
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
                            ParameterName = "@UserId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = Auth.UserId
                        },   new SqlParameter() {
                            ParameterName = "@ErrorTypeID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QADTO.ErrorType
                        }
                };

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspSubmitQARebuttalChartsOfCoder]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();

                    int res = cmd.ExecuteNonQuery();
                }
            }
            return dto;
        }

        public CodingDTO SubmitQARejectedChartsOfShadowQA(ChartSummaryDTO chartSummaryDTO, string hdnPayorIDReject, string hdnProviderIDReject, string hdnCptReject, string hdnModReject, string hdnDxReject, string hdnProviderFeedbackIDReject)
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
                            ParameterName = "@PayorRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.RevisedPayorRemarks
                        },
                        new SqlParameter() {
                            ParameterName = "@ProviderID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ProviderID
                        },
                        new SqlParameter() {
                            ParameterName = "@ProviderRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.RevisedProviderRemarks
                        },
                         new SqlParameter() {
                            ParameterName = "@CPTCode",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.CPTCode
                        },
                         new SqlParameter() {
                            ParameterName = "@CPTCodeRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.RevisedCPTRemarks
                        },
                        new SqlParameter() {
                            ParameterName = "@Mod",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.Mod
                        },
                        new SqlParameter() {
                            ParameterName = "@ModRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.RevisedModRemarks
                        },  new SqlParameter() {
                            ParameterName = "@Dx",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.Dx
                        },  new SqlParameter() {
                            ParameterName = "@DxRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.RevisedDXRemarks
                        } , new SqlParameter() {
                            ParameterName = "@ProviderFeedbackID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ProviderFeedbackID
                        }, new SqlParameter() {
                            ParameterName = "@ProviderFeedbackRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.RevisedProviderFeedbackRemarks
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
                            ParameterName = "@UserId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = Auth.UserId
                        },   new SqlParameter() {
                            ParameterName = "@ErrorTypeID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QADTO.ErrorType
                        }


                        , new SqlParameter() {
                            ParameterName = "@PayorIDReject",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = hdnPayorIDReject
                        }
                        , new SqlParameter() {
                            ParameterName = "@ProviderIDReject",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = hdnProviderIDReject
                        }
                        , new SqlParameter() {
                            ParameterName = "@CptReject",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = hdnCptReject
                        }
                        , new SqlParameter() {
                            ParameterName = "@ModReject",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = hdnModReject
                        }
                        , new SqlParameter() {
                            ParameterName = "@DxReject",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = hdnDxReject
                        }
                        , new SqlParameter() {
                            ParameterName = "@ProviderFeedbackIDReject",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = hdnProviderFeedbackIDReject
                        }

                };

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspSubmitQARejectedChartsOfShadowQA]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();

                    int res = cmd.ExecuteNonQuery();
                }
            }
            return dto;
        }

        public void SubmitQAOnHoldChart(ChartSummaryDTO chartSummaryDTO)
        {
            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@ClinicalCaseId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.CodingDTO.ClinicalCaseID
                        },
                       new SqlParameter() {
                            ParameterName = "@Answer",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.Answer
                        },
                         new SqlParameter() {
                            ParameterName = "@UserId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = Auth.UserId
                        }
                };

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspSubmitQAOnHoldChart]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();

                    int res = cmd.ExecuteNonQuery();
                }
            }
        }
        public CodingDTO SubmitShadowQARebuttalChartsOfQA(ChartSummaryDTO chartSummaryDTO, string hdnPayorIDReject, string hdnProviderIDReject, string hdnCptReject, string hdnModReject, string hdnDxReject, string hdnProviderFeedbackIDReject)
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
                            ParameterName = "@PayorRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAPayorRemarks
                        },
                        new SqlParameter() {
                            ParameterName = "@ProviderID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ProviderID
                        },
                        new SqlParameter() {
                            ParameterName = "@ProviderRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAProviderRemarks
                        },
                         new SqlParameter() {
                            ParameterName = "@CPTCode",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.CPTCode
                        },
                         new SqlParameter() {
                            ParameterName = "@CPTCodeRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QACPTCodeRemarks
                        },
                        new SqlParameter() {
                            ParameterName = "@Mod",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.Mod
                        },
                        new SqlParameter() {
                            ParameterName = "@ModRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAModRemarks
                        },  new SqlParameter() {
                            ParameterName = "@Dx",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.Dx
                        },  new SqlParameter() {
                            ParameterName = "@DxRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QADxRemarks
                        } , new SqlParameter() {
                            ParameterName = "@ProviderFeedbackID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ProviderFeedbackID
                        }, new SqlParameter() {
                            ParameterName = "@ProviderFeedbackRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAProviderFeedbackRemarks
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
                            ParameterName = "@UserId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = Auth.UserId
                         },   new SqlParameter() {
                            ParameterName = "@ErrorTypeID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QADTO.ErrorType
                        }


                        , new SqlParameter() {
                            ParameterName = "@PayorIDReject",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = hdnPayorIDReject
                        }
                        , new SqlParameter() {
                            ParameterName = "@ProviderIDReject",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = hdnProviderIDReject
                        }
                        , new SqlParameter() {
                            ParameterName = "@CptReject",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = hdnCptReject
                        }
                        , new SqlParameter() {
                            ParameterName = "@ModReject",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = hdnModReject
                        }
                        , new SqlParameter() {
                            ParameterName = "@DxReject",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = hdnDxReject
                        }
                        , new SqlParameter() {
                            ParameterName = "@ProviderFeedbackIDReject",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = hdnProviderFeedbackIDReject
                        }

                };

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspSubmitShadowQARebuttalChartsOfQA]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();

                    int res = cmd.ExecuteNonQuery();
                }
            }
            return dto;
        }


        public CodingDTO SubmitShadowQAAvailableChart(ChartSummaryDTO chartSummaryDTO)
        {
            CodingDTO dto = new CodingDTO();

            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@PayorID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAPayorID
                        },
                       new SqlParameter() {
                            ParameterName = "@PayorRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAPayorRemarks
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
                            Value = chartSummaryDTO.QAProviderID
                        },
                        new SqlParameter() {
                            ParameterName = "@ProviderRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAProviderRemarks
                        },
                         new SqlParameter() {
                            ParameterName = "@CPTCode",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QACPTCode
                        },
                         new SqlParameter() {
                            ParameterName = "@CPTCodeRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QACPTCodeRemarks
                        },
                        new SqlParameter() {
                            ParameterName = "@Mod",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAMod
                        },
                        new SqlParameter() {
                            ParameterName = "@ModRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAModRemarks
                        },  new SqlParameter() {
                            ParameterName = "@Dx",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QADx
                        },  new SqlParameter() {
                            ParameterName = "@DxRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QADxRemarks
                        } , new SqlParameter() {
                            ParameterName = "@ProviderFeedbackID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAProviderFeedbackID
                        }, new SqlParameter() {
                            ParameterName = "@ProviderFeedbackRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QAProviderFeedbackRemarks
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
                            ParameterName = "@UserId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = Auth.UserId
                         },   new SqlParameter() {
                            ParameterName = "@ErrorTypeID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QADTO.ErrorType
                        } ,   new SqlParameter() {
                            ParameterName = "@NotesfromJen",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ShadowQADTO.NotesfromJen
                        }
                };

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspSubmitShadowQA]";
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

        public List<Project> GetProjectsList()
        {
            using (var context = new UABContext())
            {
                return context.Project.ToList();
            }
        }
        public List<Role> GetRolesList()
        {
            using (var context = new UABContext())
            {
                return context.Role.ToList();
            }
        }
        public List<string> GetIdentityUsersList()
        {
            using (UAB.DAL.LoginDTO.IdentityServerContext context = new IdentityServerContext())
            {
                return context.Users.Select(a => a.Email).ToList();
            }
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
        public List<ApplicationUser> GetUsers()
        {
            ApplicationUser applicationUser = new ApplicationUser();
            List<ApplicationUser> lstApplicationUser = new List<ApplicationUser>();

            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspGetUsers]";
                    cmm.Connection = cnn;
                    cnn.Open();
                    var reader = cmm.ExecuteReader();

                    while (reader.Read())
                    {
                        applicationUser = new ApplicationUser();
                        applicationUser.UserId = Convert.ToInt32(reader["UserId"]);
                        applicationUser.Email = Convert.ToString(reader["Email"]);
                        applicationUser.IsActive = Convert.ToBoolean(reader["IsActive"]);
                        applicationUser.RoleId = Convert.ToInt32(reader["RoleId"]);
                        applicationUser.RoleName = Convert.ToString(reader["RoleName"]);
                        applicationUser.ProjectId = Convert.ToInt32(reader["ProjectId"]);
                        applicationUser.ProjectName = Convert.ToString(reader["ProjectName"]);
                        applicationUser.ProjectUserId = Convert.ToInt32(reader["ProjectUserId"]);
                        lstApplicationUser.Add(applicationUser);
                    }
                }
            }
            return lstApplicationUser;
        }

        public ApplicationUser Getuser(int ProjectUserId)
        {
            using (var context = new UABContext())
            {
                var projectuser = context.ProjectUser.Where(a => a.ProjectUserId == ProjectUserId).FirstOrDefault();
                var user = context.User.Where(a => a.UserId == projectuser.UserId).FirstOrDefault();
                var ProjctName = context.Project.Where(a => a.ProjectId == projectuser.ProjectId).FirstOrDefault();
                var RoleName = context.Role.Where(a => a.RoleId == projectuser.RoleId).FirstOrDefault();
                ApplicationUser mdl = new ApplicationUser()
                {
                    Email = user.Email,
                    ProjectName = ProjctName.Name,
                    RoleName = RoleName.Name
                };
                return mdl;
            }
        }

        public int AddUser(ApplicationUser user)
        {
            using (var context = new UABContext())
            {
                UAB.DAL.Models.User mdl = new User();
                mdl.Email = user.Email;
                mdl.IsActive = user.IsActive;

                context.User.Add(mdl);
                context.SaveChanges();

                return mdl.UserId;
            }
        }
        public void AddProjectUser(ApplicationUser user)
        {
            using (var context = new UABContext())
            {
                UAB.DAL.Models.ProjectUser mdl = new ProjectUser();
                mdl.UserId = user.UserId;
                mdl.ProjectId = context.Project.Where(a => a.Name == user.ProjectName).Select(a => a.ProjectId).FirstOrDefault();
                mdl.RoleId = context.Role.Where(a => a.Name == user.RoleName).Select(a => a.RoleId).FirstOrDefault();

                context.ProjectUser.Add(mdl);
                context.SaveChanges();
            }
        }
        public void UpdateProjectUser(ApplicationUser user)
        {
            using (var context = new UABContext())
            {

                UAB.DAL.Models.ProjectUser mdl = new ProjectUser();
                mdl.UserId = user.UserId;
                mdl.ProjectId = context.Project.Where(a => a.Name == user.ProjectName).Select(a => a.ProjectId).FirstOrDefault();
                mdl.RoleId = context.Role.Where(a => a.Name == user.RoleName).Select(a => a.RoleId).FirstOrDefault();

                var existingprojectuser = context.ProjectUser.First(a => a.ProjectUserId == user.ProjectUserId);
                if (existingprojectuser.ProjectId != mdl.ProjectId || existingprojectuser.RoleId != mdl.RoleId)
                {
                    existingprojectuser.ProjectId = mdl.ProjectId;
                    existingprojectuser.RoleId = mdl.RoleId;


                    context.Entry(existingprojectuser).State = EntityState.Modified;
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable To Update User : User Already there");
                }

            }
        }
        public void DeleteProjectUser(int ProjectUserId)
        {
            using (var context = new UABContext())
            {
                var exsitingProjectuser = context.ProjectUser.First(a => a.ProjectUserId == ProjectUserId);
                if (exsitingProjectuser != null)
                {
                    context.ProjectUser.Remove(exsitingProjectuser);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable To Delete User : User Not there in UAB");
                }

            }
        }

        public List<string> GetProviderNames()
        {
            Provider provider = new Provider();
            List<Provider> lstProvider = new List<Provider>();
            List<string> providers = new List<string>();

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
                        providers.Add(provider.Name.ToLower());
                    }
                }
            }
            return providers;
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

        public void UpdateProvider(Provider provider)
        {
            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    //SqlCommand cmd = new SqlCommand("UspAddProvider");
                    var cmm = cnn.CreateCommand();
                    //SqlCommand cmd = new SqlCommand("[dbo].[UspUpdateProvider]", cnn);
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspUpdateProvider]";
                    cmm.Connection = cnn;

                    SqlParameter param1 = new SqlParameter();
                    param1.ParameterName = "@Name";
                    param1.Value = provider.Name;
                    SqlParameter param2 = new SqlParameter();
                    param2.ParameterName = "@ProviderID";
                    param2.Value = provider.ProviderId;
                    cmm.Parameters.Add(param1);
                    cmm.Parameters.Add(param2);


                    //SqlParameter name = new SqlParameter();
                    //name.ParameterName = "@Name";
                    //name.Value = provider.Name;
                    //cmm.Parameters.Add(name);

                    cnn.Open();
                    cmm.ExecuteNonQuery();
                }
            }
        }

        public void DeleteProvider(Provider provider)
        {
            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    //SqlCommand cmd = new SqlCommand("UspAddProvider");
                    var cmm = cnn.CreateCommand();
                    //SqlCommand cmd = new SqlCommand("[dbo].[UspUpdateProvider]", cnn);
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspDeleteProvider]";
                    cmm.Connection = cnn;

                    SqlParameter param = new SqlParameter();
                    param.ParameterName = "@ProviderID";
                    param.Value = provider.ProviderId;
                    cmm.Parameters.Add(param);


                    //SqlParameter name = new SqlParameter();
                    //name.ParameterName = "@Name";
                    //name.Value = provider.Name;
                    //cmm.Parameters.Add(name);

                    cnn.Open();
                    cmm.ExecuteNonQuery();
                }
            }
        }
        public void DeleteUser(ApplicationUser applicationUser)
        {
            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {

                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspDeleteUser]";
                    cmm.Connection = cnn;

                    SqlParameter param = new SqlParameter();
                    param.ParameterName = "@UserId";
                    param.Value = applicationUser.UserId;
                    cmm.Parameters.Add(param);
                    cnn.Open();
                    cmm.ExecuteNonQuery();
                }
            }
        }

        public List<string> GetProviderFeedbackNames()
        {
            List<string> feedbacks = new List<string>();
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
                        feedbacks.Add(dto.Name.ToLower());
                    }
                }
            }
            return feedbacks;
        }

        public void AddProviderFeedback(BindDTO providerFeedback)
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
                    feedback.Value = providerFeedback.Name;
                    cmm.Parameters.Add(feedback);

                    cnn.Open();
                    cmm.ExecuteNonQuery();
                }
            }
        }

        public void UpdateProviderFeedback(BindDTO providerFeedback)
        {
            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    //SqlCommand cmd = new SqlCommand("UspAddProvider");
                    var cmm = cnn.CreateCommand();
                    //SqlCommand cmd = new SqlCommand("[dbo].[UspUpdateProvider]", cnn);
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspUpdateProviderFeedback]";
                    cmm.Connection = cnn;

                    SqlParameter param1 = new SqlParameter();
                    param1.ParameterName = "@Feedback";
                    param1.Value = providerFeedback.Name;
                    SqlParameter param2 = new SqlParameter();
                    param2.ParameterName = "@ProviderFeedbackID";
                    param2.Value = providerFeedback.ID;
                    cmm.Parameters.Add(param1);
                    cmm.Parameters.Add(param2);


                    //SqlParameter name = new SqlParameter();
                    //name.ParameterName = "@Name";
                    //name.Value = provider.Name;
                    //cmm.Parameters.Add(name);

                    cnn.Open();
                    cmm.ExecuteNonQuery();
                }
            }
        }

        public void DeleteProviderFeedback(BindDTO providerFeedback)
        {
            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    //SqlCommand cmd = new SqlCommand("UspAddProvider");
                    var cmm = cnn.CreateCommand();
                    //SqlCommand cmd = new SqlCommand("[dbo].[UspUpdateProvider]", cnn);
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspDeleteProviderFeedback]";
                    cmm.Connection = cnn;

                    SqlParameter param = new SqlParameter();
                    param.ParameterName = "@ProviderFeedbackId";
                    param.Value = providerFeedback.ID;
                    cmm.Parameters.Add(param);


                    //SqlParameter name = new SqlParameter();
                    //name.ParameterName = "@Name";
                    //name.Value = provider.Name;
                    //cmm.Parameters.Add(name);

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
                        payor.PayorId = Convert.ToInt32(reader["PayorID"]);
                        payor.Name = Convert.ToString(reader["Name"]);
                        lstPayor.Add(payor);
                    }
                }
            }
            return lstPayor;
        }

        public List<string> GetPayorNames()
        {
            Payor payor = null;
            List<Payor> lstPayor = new List<Payor>();
            List<string> payors = new List<string>();

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
                        payor.PayorId = Convert.ToInt32(reader["PayorID"]);
                        payor.Name = Convert.ToString(reader["Name"]);
                        payors.Add(payor.Name.ToLower());
                    }
                }
            }
            return payors;
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

        public void UpdatePayor(Payor payor)
        {
            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    //SqlCommand cmd = new SqlCommand("UspAddProvider");
                    var cmm = cnn.CreateCommand();
                    //SqlCommand cmd = new SqlCommand("[dbo].[UspUpdateProvider]", cnn);
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspUpdatePayor]";
                    cmm.Connection = cnn;

                    SqlParameter param1 = new SqlParameter();
                    param1.ParameterName = "@Name";
                    param1.Value = payor.Name;
                    SqlParameter param2 = new SqlParameter();
                    param2.ParameterName = "@PayorID";
                    param2.Value = payor.PayorId;
                    cmm.Parameters.Add(param1);
                    cmm.Parameters.Add(param2);


                    //SqlParameter name = new SqlParameter();
                    //name.ParameterName = "@Name";
                    //name.Value = provider.Name;
                    //cmm.Parameters.Add(name);

                    cnn.Open();
                    cmm.ExecuteNonQuery();
                }
            }
        }

        public void DeletePayor(Payor payor)
        {
            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    //SqlCommand cmd = new SqlCommand("UspAddProvider");
                    var cmm = cnn.CreateCommand();
                    //SqlCommand cmd = new SqlCommand("[dbo].[UspUpdateProvider]", cnn);
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspDeletePayor]";
                    cmm.Connection = cnn;

                    SqlParameter param = new SqlParameter();
                    param.ParameterName = "@PayorID";
                    param.Value = payor.PayorId;
                    cmm.Parameters.Add(param);


                    //SqlParameter name = new SqlParameter();
                    //name.ParameterName = "@Name";
                    //name.Value = provider.Name;
                    //cmm.Parameters.Add(name);

                    cnn.Open();
                    cmm.ExecuteNonQuery();
                }
            }
        }

        public List<ErrorType> GetErrorTypes()
        {
            ErrorType errorType = new ErrorType();
            List<ErrorType> lstErrorType = new List<ErrorType>();

            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspGetErrorType]";
                    cmm.Connection = cnn;
                    cnn.Open();
                    var reader = cmm.ExecuteReader();

                    while (reader.Read())
                    {
                        errorType = new ErrorType();
                        errorType.ErrorTypeId = Convert.ToInt32(reader["ErrorTypeID"]);
                        errorType.Name = Convert.ToString(reader["Name"]);
                        lstErrorType.Add(errorType);
                    }
                }
            }
            return lstErrorType;
        }

        public List<string> GetErrorTypeNames()
        {
            ErrorType errorType = new ErrorType();
            List<ErrorType> lstErrorType = new List<ErrorType>();
            List<string> errorTypes = new List<string>();

            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspGetErrorType]";
                    //cmm.Parameters.AddRange(param);
                    cmm.Connection = cnn;
                    cnn.Open();
                    var reader = cmm.ExecuteReader();

                    while (reader.Read())
                    {
                        errorType = new ErrorType();
                        errorType.ErrorTypeId = Convert.ToInt32(reader["ErrorTypeID"]);
                        errorType.Name = Convert.ToString(reader["Name"]);
                        lstErrorType.Add(errorType);
                        errorTypes.Add(errorType.Name.ToLower());
                    }
                }
            }
            return errorTypes;
        }

        public void AddErrorType(ErrorType errorType)
        {
            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspAddErrorType]";
                    cmm.Connection = cnn;

                    SqlParameter name = new SqlParameter();
                    name.ParameterName = "@Name";
                    name.Value = errorType.Name;
                    cmm.Parameters.Add(name);

                    cnn.Open();
                    cmm.ExecuteNonQuery();
                }
            }
        }

        public void UpdateErrorType(ErrorType errorType)
        {
            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspUpdateErrorType]";
                    cmm.Connection = cnn;

                    SqlParameter param1 = new SqlParameter();
                    param1.ParameterName = "@Name";
                    param1.Value = errorType.Name;
                    SqlParameter param2 = new SqlParameter();
                    param2.ParameterName = "@ErrorTypeID";
                    param2.Value = errorType.ErrorTypeId;
                    cmm.Parameters.Add(param1);
                    cmm.Parameters.Add(param2);

                    cnn.Open();
                    cmm.ExecuteNonQuery();
                }
            }
        }

        public void DeleteErrorType(ErrorType errorType)
        {
            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspDeleteErrorType]";
                    cmm.Connection = cnn;

                    SqlParameter param = new SqlParameter();
                    param.ParameterName = "@ErrorTypeID";
                    param.Value = errorType.ErrorTypeId;
                    cmm.Parameters.Add(param);

                    cnn.Open();
                    cmm.ExecuteNonQuery();
                }
            }
        }
    }
}
