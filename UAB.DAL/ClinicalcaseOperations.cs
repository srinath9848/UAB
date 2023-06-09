﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UAB.DAL.Models;
using UAB.DTO;
using UAB.DAL.LoginDTO;
using System.Linq;
using System.IO;
using ExcelDataReader;
using System.Data.Entity.Infrastructure;
using Version = UAB.DAL.Models.Version;
using System.Reflection;
using Microsoft.Extensions.Configuration;
namespace UAB.DAL
{
    public class ClinicalcaseOperations
    {
        int mUserId;
        string mUserRole;
        string _conStr = null;
        public ClinicalcaseOperations(int UserId)
        {
            mUserId = UserId;
        }
        public ClinicalcaseOperations(int UserId, string UserRole)
        {
            mUserId = UserId;
            mUserRole = UserRole;
        }
        public ClinicalcaseOperations()
        {

        }
        public List<DashboardDTO> GetChartCountByRole(string Role)
        {
            DashboardDTO dto = new DashboardDTO();
            List<DashboardDTO> lstDto = new List<DashboardDTO>();

            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@UserId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = mUserId
                         },
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
                        dto.QARebuttalCharts = Convert.ToInt32(reader["IncorrectCharts"]);
                        dto.ShadowQARebuttalCharts = Convert.ToInt32(reader["ShadowQARebuttalCharts"]);
                        dto.ReadyForPostingCharts = Convert.ToInt32(reader["ReadyForPostingCharts"]);
                        //dto.OnHoldCharts = Convert.ToInt32(reader["OnHoldCharts"]);
                        dto.BlockedCharts = Convert.ToInt32(reader["BlockedCharts"]);
                        lstDto.Add(dto);
                    }
                }
            }
            return lstDto;
        }
        public AuditDTO GetAuditInfoForCPTAndProvider(int ProjectID)
        {
            AuditDTO dto = new AuditDTO();

            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@ProjectID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = ProjectID
                         }
                };

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[USPGetAuditInfo]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();

                    List<string> lstValues = new List<string>();
                    while (reader.Read())
                    {
                        lstValues.Add(Convert.ToString(reader["ProviderID"]));
                    }
                    dto.ProviderIDs = string.Join(",", lstValues);

                    reader.NextResult();
                    lstValues = new List<string>();

                    while (reader.Read())
                    {
                        lstValues.Add(Convert.ToString(reader["CptCode"]));
                    }
                    dto.CPTCodes = string.Join(",", lstValues);
                }
            }
            return dto;
        }
        public List<ChartSummaryDTO> GetBlockedChartsList(string Role, int projectID, string timeZoneCookie)
        {
            List<ChartSummaryDTO> lst = new List<ChartSummaryDTO>();
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
                            ParameterName = "@UserId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = mUserId
                         }};
                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[USPGetBlockCharts]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        chartSummaryDTO = new ChartSummaryDTO();
                        chartSummaryDTO.CodingDTO.ClinicalCaseID = Convert.ToInt32(reader["ClinicalCaseID"]);
                        chartSummaryDTO.CodingDTO.ListName = Convert.ToString(reader["ListName"]);
                        chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        var dos = Convert.ToDateTime(reader["DateOfService"]);
                        chartSummaryDTO.CodingDTO.DateOfService = dos.ToString("MM/dd/yyyy");
                        chartSummaryDTO.ProjectID = projectID;
                        chartSummaryDTO.ProjectName = Convert.ToString(reader["ProjectName"]);
                        chartSummaryDTO.BlockCategory = Convert.ToString(reader["BlockCategory"]);
                        chartSummaryDTO.BlockRemarks = Convert.ToString(reader["BlockRemarks"]);
                        chartSummaryDTO.BlockedDate = Convert.ToDateTime(reader["BlockedDate"]).ToLocalDate(timeZoneCookie);
                        if (reader["ProviderId"] != DBNull.Value)
                            chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);

                        lst.Add(chartSummaryDTO);
                    }
                    reader.NextResult();

                    while (reader.Read())
                    {
                        lst.ForEach(x => x.CCIDs = Convert.ToString(reader["CCIDs"]));
                    }
                }
            }
            return lst;
        }

        public BlockResponseDTO GetBlockResponseBycid(int cid)
        {
            using (var context = new UABContext())
            {
                var res1 = context.BlockHistory.Where(x => x.ClinicalCaseId == cid).OrderByDescending(x => x.BlockHistoryId).FirstOrDefault();

                var res = context.BlockResponse.Where(x => x.ClinicalCaseId == cid).OrderByDescending(x => x.BlockResponseId).FirstOrDefault();
                if (res1 != null && res != null)
                {
                    var un = context.User.Where(x => x.UserId == res.ResponseByUserId).FirstOrDefault();
                    var bn = context.User.Where(x => x.UserId == res1.BlockedByUserId).FirstOrDefault();
                    string bc = context.BlockCategory.Where(x => x.BlockCategoryId == res1.BlockCategoryId).Select(x => x.Name).FirstOrDefault();

                    BlockResponseDTO brdt = new BlockResponseDTO
                    {
                        ResponseByUserName = un.FirstName + " " + un.LastName,
                        ResponseRemarks = res.ResponseRemarks,
                        ResponseDate = res.ResponseDate,
                        BlockCategory = bc,
                        BlockRemarks = res1.Remarks,
                        Blockedbyuser = bn.FirstName + " " + bn.LastName,
                        BlockedDate = res1.CreateDate,
                        BlockedInQueueKind = res1.BlockedInQueueKind
                    };
                    return brdt;
                }
                return null;
            }
        }
        public ChartSummaryDTO GetNext(string Role, string ChartType, int projectID, string timeZoneCookie, int CurrCCId = 0)
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
                            Value = mUserId
                         }
                        , new SqlParameter() {
                            ParameterName = "@PrevOrNextCCID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = CurrCCId
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

                    chartSummaryDTO.blockHistories = new List<BlockDTO>();
                    while (reader.Read())
                    {
                        chartSummaryDTO.CodingDTO.ClinicalCaseID = Convert.ToInt32(reader["ClinicalCaseID"]);
                        chartSummaryDTO.CodingDTO.ListName = Convert.ToString(reader["ListName"]);
                        //chartSummaryDTO.CodingDTO.ListName = "PK-Card APP Consult";
                        chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        chartSummaryDTO.CodingDTO.DateOfService = Convert.ToString(reader["DateOfService"]);
                        chartSummaryDTO.ProjectTypename = Convert.ToString(reader["ProjectTypename"]);

                        if (Role == "Coder" && ChartType == "Available")
                        {
                            if (reader["ProviderId"] != DBNull.Value)
                                chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                        }

                        if (Role == "Coder" && ChartType == "Block")
                        {
                            chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            chartSummaryDTO.blockHistories.Add(new BlockDTO
                            {
                                Name = Convert.ToString(reader["BlockCategory"]),
                                Remarks = Convert.ToString(reader["BlockRemarks"]),
                                Queue = Convert.ToString(reader["Queue"]),
                                BlockedBy = Convert.ToString(reader["BlockedBy"]),
                                CreateDate = Convert.ToDateTime(reader["BlockedDate"]).ToLocalDate(timeZoneCookie)
                            });
                        }
                    }
                    reader.NextResult();

                    while (reader.Read())
                    {
                        chartSummaryDTO.CCIDs = Convert.ToString(reader["CCIDs"]);
                    }
                }
            }
            return chartSummaryDTO;
        }

        public List<ChartSummaryDTO> GetNext1(string Role, string ChartType, int projectID, string timeZoneCookie = "", int CurrCCId = 0)
        {
            List<ChartSummaryDTO> lstchartSummaryDTO = new List<ChartSummaryDTO>();
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
                            Value = mUserId
                         }
                         , new SqlParameter() {
                            ParameterName = "@PrevOrNextCCID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = CurrCCId
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
                        chartSummaryDTO = new ChartSummaryDTO();
                        chartSummaryDTO.CodingDTO.ClinicalCaseID = Convert.ToInt32(reader["ClinicalCaseID"]);
                        if (reader["ListName"] != DBNull.Value)
                            chartSummaryDTO.CodingDTO.ListName = Convert.ToString(reader["ListName"]);
                        chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        chartSummaryDTO.CodingDTO.DateOfService = Convert.ToString(reader["DateOfService"]);
                        //if (Role == "Coder" && ChartType == "Available")
                        //{
                        //    if (reader["ProviderId"] != DBNull.Value)
                        //        chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                        //}

                        //if (Role == "Coder" && ChartType == "Block")
                        //{
                        //    chartSummaryDTO.BlockCategory = Convert.ToString(reader["BlockCategory"]);
                        //    chartSummaryDTO.BlockRemarks = Convert.ToString(reader["BlockRemarks"]);
                        //    chartSummaryDTO.BlockedDate = Convert.ToDateTime(reader["BlockedDate"]).ToLocalDate(timeZoneCookie);
                        //}
                        //else
                        if (Role == "QA" && ChartType == "Block")
                        {
                            //if (reader["BillingProviderId"] != DBNull.Value)
                            //    chartSummaryDTO.BillingProviderID = Convert.ToInt32(reader["BillingProviderId"]);
                            chartSummaryDTO.ProjectTypename = Convert.ToString(reader["ProjectTypename"]);

                            //chartSummaryDTO.BlockCategory = Convert.ToString(reader["BlockCategory"]);
                            //chartSummaryDTO.BlockRemarks = Convert.ToString(reader["BlockRemarks"]);
                            //chartSummaryDTO.BlockedDate = Convert.ToDateTime(reader["BlockedDate"]).ToLocalDate(timeZoneCookie);

                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            if (reader["ClaimId"] != DBNull.Value)
                                chartSummaryDTO.ClaimId = Convert.ToInt32(reader["ClaimId"]);
                            else
                                chartSummaryDTO.ClaimId = null;
                            if (reader["ProviderId"] != DBNull.Value)
                                chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            if (reader["BillingProviderId"] != DBNull.Value)
                                chartSummaryDTO.BillingProviderID = Convert.ToInt32(reader["BillingProviderId"]);
                            if (reader["PayorId"] != DBNull.Value)
                                chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            if (reader["ProviderFeedbackId"] != DBNull.Value)
                                chartSummaryDTO.ProviderFeedbackID = Convert.ToString(reader["ProviderFeedbackId"]);  //Convert.ToInt32(reader["ProviderFeedbackId"]);
                            chartSummaryDTO.ProjectID = Convert.ToInt32(reader["ProjectId"]);
                        }
                        else if ((Role == "Coder" && ChartType == "ReadyForPosting") ||
                            (Role == "QA" && ChartType == "OnHold"))
                        {
                            if (reader["ClaimId"] != DBNull.Value)
                                chartSummaryDTO.ClaimId = Convert.ToInt32(reader["ClaimId"]);
                            else
                                chartSummaryDTO.ClaimId = null;
                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);

                            chartSummaryDTO.ProjectTypename = Convert.ToString(reader["projecttypename"]);

                            if (reader["ProviderId"] != DBNull.Value)
                                chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            if (reader["BillingProviderId"] != DBNull.Value)
                                chartSummaryDTO.BillingProviderID = Convert.ToInt32(reader["BillingProviderId"]);
                            if (reader["PayorId"] != DBNull.Value)
                                chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                            chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            chartSummaryDTO.BillingProviderText = Convert.ToString(reader["BillingProviderText"]);
                            chartSummaryDTO.PayorText = Convert.ToString(reader["PayorText"]);
                            chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["ProviderFeedbackText"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            // chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            if (reader["ProviderFeedbackId"] != DBNull.Value)
                                chartSummaryDTO.ProviderFeedbackID = Convert.ToString(reader["ProviderFeedbackId"]);
                            if (Role == "QA" && ChartType == "OnHold")
                                chartSummaryDTO.CoderQuestion = Convert.ToString(reader["Question"]);

                            //lstchartSummaryDTO.Add(chartSummaryDTO);

                        }
                        else if (Role == "QA" && ChartType == "Available")
                        {
                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            if (reader["ClaimId"] != DBNull.Value)
                                chartSummaryDTO.ClaimId = Convert.ToInt32(reader["ClaimId"]);
                            else
                                chartSummaryDTO.ClaimId = null;
                            if (reader["ProviderId"] != DBNull.Value)
                                chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            if (reader["BillingProviderId"] != DBNull.Value)
                                chartSummaryDTO.BillingProviderID = Convert.ToInt32(reader["BillingProviderId"]);
                            if (reader["PayorId"] != DBNull.Value)
                                chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            chartSummaryDTO.ProjectTypename = Convert.ToString(reader["ProjectTypename"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            // chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            if (reader["ProviderFeedbackId"] != DBNull.Value)
                                chartSummaryDTO.ProviderFeedbackID = Convert.ToString(reader["ProviderFeedbackId"]);
                            chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            chartSummaryDTO.BillingProviderText = Convert.ToString(reader["BillingProviderText"]);
                            chartSummaryDTO.PayorText = Convert.ToString(reader["PayorText"]);
                            chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["ProviderFeedbackText"]);
                        }
                        else if (Role == "Coder" && ChartType == "Incorrect")
                        {
                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            chartSummaryDTO.QABy = Convert.ToString(reader["QABy"]);
                            chartSummaryDTO.ShadowQABy = Convert.ToString(reader["ShadowQABy"]);

                            if (reader["ClaimId"] != DBNull.Value)
                                chartSummaryDTO.ClaimId = Convert.ToInt32(reader["ClaimId"]);
                            else
                                chartSummaryDTO.ClaimId = null;

                            chartSummaryDTO.PayorText = Convert.ToString(reader["PayorText"]);
                            chartSummaryDTO.QAPayorText = Convert.ToString(reader["QAPayorText"]);

                            chartSummaryDTO.ProjectTypename = Convert.ToString(reader["ProjectTypeName"]);

                            chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            chartSummaryDTO.QAProviderText = Convert.ToString(reader["QAProviderText"]);

                            chartSummaryDTO.BillingProviderText = Convert.ToString(reader["BillingProviderText"]);
                            chartSummaryDTO.QABillingProviderText = Convert.ToString(reader["QABillingProviderText"]);

                            chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["FeedbackText"]);
                            chartSummaryDTO.QAProviderFeedbackText = Convert.ToString(reader["QAFeedbackText"]);

                            chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            if (reader["QAProviderID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderID = Convert.ToInt32(reader["QAProviderID"]);
                            chartSummaryDTO.QAProviderRemarks = Convert.ToString(reader["QAProviderIdRemark"]);

                            if (reader["BillingProviderId"] != DBNull.Value)
                                chartSummaryDTO.BillingProviderID = Convert.ToInt32(reader["BillingProviderId"]);
                            if (reader["QABillingProviderID"] != DBNull.Value)
                                chartSummaryDTO.QABillingProviderID = Convert.ToInt32(reader["QABillingProviderID"]);
                            chartSummaryDTO.QABillingProviderRemarks = Convert.ToString(reader["QABillingProviderIdRemark"]);

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

                            if (reader["ProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.ProviderFeedbackID = Convert.ToString(reader["ProviderFeedbackID"]);
                            if (reader["QAProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderFeedbackID = Convert.ToString(reader["QAProviderFeedbackID"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QAProviderFeedbackIDRemark"]);
                            if (reader["QAErrorTypeId"] != DBNull.Value)
                            {
                                chartSummaryDTO.QADTO.ErrorType = Convert.ToString(reader["QAErrorTypeId"]);
                                chartSummaryDTO.QAErrorTypeText = Convert.ToString(reader["QAErrorTypeText"]);
                            }

                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);

                            chartSummaryDTO.RevisedPayorRemarks = Convert.ToString(reader["RebuttedPayorIdRemark"]);
                            chartSummaryDTO.RevisedProviderRemarks = Convert.ToString(reader["RebuttedProviderIDRemark"]);
                            chartSummaryDTO.RevisedBillingProviderRemarks = Convert.ToString(reader["RebuttedBillingProviderIDRemark"]);
                            chartSummaryDTO.RevisedCPTRemarks = Convert.ToString(reader["RebuttedCPTCodeRemark"]);
                            chartSummaryDTO.RevisedDXRemarks = Convert.ToString(reader["RebuttedDxRemark"]);
                            chartSummaryDTO.RevisedProviderFeedbackRemarks = Convert.ToString(reader["RebuttedProviderFeedbackIDRemark"]);
                        }
                        else if (Role == "ShadowQA" && (ChartType == "Available" || ChartType == "Block"))
                        {
                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            chartSummaryDTO.QABy = Convert.ToString(reader["QABy"]);

                            if (reader["ClaimId"] != DBNull.Value)
                                chartSummaryDTO.ClaimId = Convert.ToInt32(reader["ClaimId"]);
                            else
                                chartSummaryDTO.ClaimId = null;

                            chartSummaryDTO.PayorText = Convert.ToString(reader["PayorText"]);
                            chartSummaryDTO.QAPayorText = Convert.ToString(reader["QAPayorText"]);
                            chartSummaryDTO.QAPayorRemarks = Convert.ToString(reader["QAPayorIdRemark"]);

                            chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            chartSummaryDTO.QAProviderText = Convert.ToString(reader["QAProviderText"]);
                            chartSummaryDTO.QAProviderRemarks = Convert.ToString(reader["QAProviderIDRemark"]);

                            chartSummaryDTO.BillingProviderText = Convert.ToString(reader["BillingProviderText"]);
                            chartSummaryDTO.QABillingProviderText = Convert.ToString(reader["QABillingProviderText"]);
                            chartSummaryDTO.QABillingProviderRemarks = Convert.ToString(reader["QABillingProviderIDRemark"]);

                            chartSummaryDTO.QACPTCode = Convert.ToString(reader["QACPTCode"]);
                            chartSummaryDTO.QACPTCodeRemarks = Convert.ToString(reader["QACPTCodeRemark"]);

                            chartSummaryDTO.QAMod = Convert.ToString(reader["QAMod"]);
                            chartSummaryDTO.QAModRemarks = Convert.ToString(reader["QAModRemark"]);

                            chartSummaryDTO.QADx = Convert.ToString(reader["QADx"]);
                            chartSummaryDTO.QADxRemarks = Convert.ToString(reader["QADxRemark"]);

                            if (reader["QAProviderID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderID = Convert.ToInt32(reader["QAProviderID"]);
                            chartSummaryDTO.QAProviderRemarks = Convert.ToString(reader["QAProviderIDRemark"]);

                            if (reader["QABillingProviderID"] != DBNull.Value)
                                chartSummaryDTO.QABillingProviderID = Convert.ToInt32(reader["QABillingProviderID"]);
                            chartSummaryDTO.QABillingProviderRemarks = Convert.ToString(reader["QABillingProviderIDRemark"]);

                            if (reader["QAPayorID"] != DBNull.Value)
                                chartSummaryDTO.QAPayorID = Convert.ToInt32(reader["QAPayorID"]);
                            chartSummaryDTO.QAPayorRemarks = Convert.ToString(reader["QAPayorIdRemark"]);

                            if (reader["QAProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderFeedbackID = Convert.ToString(reader["QAProviderFeedbackID"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QAProviderFeedbackIDRemark"]);

                            chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["ProviderFeedbackText"]);
                            chartSummaryDTO.QAProviderFeedbackText = Convert.ToString(reader["QAProviderFeedbackText"]);

                            if (reader["QAErrorTypeId"] != DBNull.Value)
                            {
                                chartSummaryDTO.QADTO.ErrorType = Convert.ToString(reader["QAErrorTypeId"]);
                                chartSummaryDTO.QAErrorTypeText = Convert.ToString(reader["QAErrorTypeText"]);
                            }

                            //if (reader["ProviderId"] != DBNull.Value)
                            //    chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            //if (reader["PayorId"] != DBNull.Value)
                            //    chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            chartSummaryDTO.ProjectTypename = Convert.ToString(reader["ProjectTypename"]);

                            //if (reader["ProviderFeedbackId"] != DBNull.Value)
                            //    chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackId"]);
                        }
                        else if (Role == "ShadowQA" && ChartType == "RebuttalOfQA")
                        {
                            if (reader["ClaimId"] != DBNull.Value)
                                chartSummaryDTO.ClaimId = Convert.ToInt32(reader["ClaimId"]);
                            else
                                chartSummaryDTO.ClaimId = null;
                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            chartSummaryDTO.QABy = Convert.ToString(reader["QABy"]);
                            chartSummaryDTO.ShadowQABy = Convert.ToString(reader["ShadowQABy"]);

                            chartSummaryDTO.ProjectTypename = Convert.ToString(reader["projecttypename"]);

                            chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            if (reader["QAProviderID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderID = Convert.ToInt32(reader["QAProviderID"]);
                            chartSummaryDTO.QAProviderRemarks = Convert.ToString(reader["QAProviderIDRemark"]);

                            if (reader["BillingProviderId"] != DBNull.Value)
                                chartSummaryDTO.BillingProviderID = Convert.ToInt32(reader["BillingProviderId"]);
                            if (reader["QABillingProviderID"] != DBNull.Value)
                                chartSummaryDTO.QABillingProviderID = Convert.ToInt32(reader["QABillingProviderID"]);
                            chartSummaryDTO.QABillingProviderRemarks = Convert.ToString(reader["QABillingProviderIDRemark"]);

                            chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            if (reader["QAPayorID"] != DBNull.Value)
                                chartSummaryDTO.QAPayorID = Convert.ToInt32(reader["QAPayorID"]);
                            chartSummaryDTO.QAPayorRemarks = Convert.ToString(reader["QAPayorIdRemark"]);

                            if (reader["ProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.ProviderFeedbackID = Convert.ToString(reader["ProviderFeedbackID"]);
                            if (reader["QAProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderFeedbackID = Convert.ToString(reader["QAProviderFeedbackID"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QAProviderFeedbackIDRemark"]);

                            chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            chartSummaryDTO.QAProviderText = Convert.ToString(reader["QAProviderText"]);
                            chartSummaryDTO.ShadowQAProviderText = Convert.ToString(reader["ShadowQAProviderText"]);

                            chartSummaryDTO.BillingProviderText = Convert.ToString(reader["BillingProviderText"]);
                            chartSummaryDTO.QABillingProviderText = Convert.ToString(reader["QABillingProviderText"]);
                            chartSummaryDTO.ShadowQABillingProviderText = Convert.ToString(reader["ShadowQABillingProviderText"]);

                            chartSummaryDTO.QAProviderFeedbackText = Convert.ToString(reader["QAProviderFeedbackText"]);
                            chartSummaryDTO.ShadowQAProviderFeedbackText = Convert.ToString(reader["ShadowQAProviderFeedbackText"]);

                            chartSummaryDTO.QAProviderRemarks = Convert.ToString(reader["QARebuttedProviderIDRemark"]);
                            if (reader["ShadowQAProviderID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAProviderID = Convert.ToInt32(reader["ShadowQAProviderID"]);
                            chartSummaryDTO.ShadowQAProviderRemarks = Convert.ToString(reader["ShadowQAProviderIDRemark"]);

                            chartSummaryDTO.QABillingProviderRemarks = Convert.ToString(reader["QARebuttedBillingProviderIDRemark"]);
                            if (reader["ShadowQABillingProviderID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQABillingProviderID = Convert.ToInt32(reader["ShadowQABillingProviderID"]);
                            chartSummaryDTO.ShadowQABillingProviderRemarks = Convert.ToString(reader["ShadowQABillingProviderIDRemark"]);


                            chartSummaryDTO.PayorText = Convert.ToString(reader["PayorText"]);
                            chartSummaryDTO.QAPayorText = Convert.ToString(reader["QAPayorText"]);
                            chartSummaryDTO.ShadowQAPayorText = Convert.ToString(reader["ShadowQAPayorText"]);

                            if (reader["ShadowQAPayorID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAPayorID = Convert.ToInt32(reader["ShadowQAPayorID"]);
                            chartSummaryDTO.QAPayorRemarks = Convert.ToString(reader["QARebuttedPayorIdRemark"]);
                            chartSummaryDTO.ShadowQAPayorRemarks = Convert.ToString(reader["ShadowQAPayorIdRemark"]);

                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.QADx = Convert.ToString(reader["QADx"]);
                            chartSummaryDTO.QADxRemarks = Convert.ToString(reader["QADxRemark"]);
                            chartSummaryDTO.ShadowQADx = Convert.ToString(reader["ShadowQADx"]);
                            chartSummaryDTO.ShadowQADxRemarks = Convert.ToString(reader["ShadowQADxRemark"]);

                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            chartSummaryDTO.QACPTCode = Convert.ToString(reader["QACPTCode"]);
                            chartSummaryDTO.QACPTCodeRemarks = Convert.ToString(reader["QACPTCodeRemark"]);
                            chartSummaryDTO.ShadowQACPTCode = Convert.ToString(reader["ShadowQACPTCode"]);
                            chartSummaryDTO.ShadowQACPTCodeRemarks = Convert.ToString(reader["ShadowQACPTCodeRemark"]);

                            chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["ProviderFeedbackText"]);
                            //chartSummaryDTO.QAProviderFeedbackText = Convert.ToString(reader["QAProviderFeedbackText"]);
                            if (reader["ShadowQAProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAProviderFeedbackID = Convert.ToString(reader["ShadowQAProviderFeedbackID"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QARebuttedProviderFeedbackIDRemark"]);
                            chartSummaryDTO.ShadowQAProviderFeedbackRemarks = Convert.ToString(reader["ShadowQAProviderFeedbackIDRemark"]);

                            if (reader["QAErrorTypeId"] != DBNull.Value)
                            {
                                chartSummaryDTO.QADTO.ErrorType = Convert.ToString(reader["QAErrorTypeId"]);
                                chartSummaryDTO.QAErrorTypeText = Convert.ToString(reader["QAErrorTypeText"]);
                            }

                            if (reader["ShadowQAErrorTypeId"] != DBNull.Value)
                            {
                                chartSummaryDTO.ShadowQADTO.ErrorType = Convert.ToString(reader["ShadowQAErrorTypeId"]);
                                chartSummaryDTO.ShadowQAErrorTypeText = Convert.ToString(reader["ShadowQAErrorTypeText"]);
                            }

                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                            chartSummaryDTO.IsCoderRebutted = Convert.ToBoolean(reader["IsCoderRebutted"]);

                        }
                        else if (Role == "QA" && ChartType == "ShadowQARejected")
                        {
                            if (reader["ClaimId"] != DBNull.Value)
                                chartSummaryDTO.ClaimId = Convert.ToInt32(reader["ClaimId"]);
                            else
                                chartSummaryDTO.ClaimId = null;
                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            chartSummaryDTO.QABy = Convert.ToString(reader["QABy"]);
                            chartSummaryDTO.ShadowQABy = Convert.ToString(reader["ShadowQABy"]);

                            chartSummaryDTO.ProjectTypename = Convert.ToString(reader["projecttypename"]);

                            chartSummaryDTO.PayorText = Convert.ToString(reader["PayorText"]);
                            chartSummaryDTO.QAPayorText = Convert.ToString(reader["QAPayorText"]);
                            chartSummaryDTO.ShadowQAPayorText = Convert.ToString(reader["ShadowQAPayorText"]);

                            chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            chartSummaryDTO.QAProviderText = Convert.ToString(reader["QAProviderText"]);
                            chartSummaryDTO.ShadowQAProviderText = Convert.ToString(reader["ShadowQAProviderText"]);

                            chartSummaryDTO.BillingProviderText = Convert.ToString(reader["BillingProviderText"]);
                            chartSummaryDTO.QABillingProviderText = Convert.ToString(reader["QABillingProviderText"]);
                            chartSummaryDTO.ShadowQABillingProviderText = Convert.ToString(reader["ShadowQABillingProviderText"]);

                            chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["FeedbackText"]);
                            chartSummaryDTO.QAProviderFeedbackText = Convert.ToString(reader["QAFeedbackText"]);
                            chartSummaryDTO.ShadowQAProviderFeedbackText = Convert.ToString(reader["ShadowQAFeedbackText"]);

                            chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            if (reader["QAProviderID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderID = Convert.ToInt32(reader["QAProviderID"]);
                            chartSummaryDTO.QAProviderRemarks = Convert.ToString(reader["QAProviderIDRemark"]);

                            if (reader["BillingProviderId"] != DBNull.Value)
                                chartSummaryDTO.BillingProviderID = Convert.ToInt32(reader["BillingProviderId"]);
                            if (reader["QABillingProviderID"] != DBNull.Value)
                                chartSummaryDTO.QABillingProviderID = Convert.ToInt32(reader["QABillingProviderID"]);
                            chartSummaryDTO.QABillingProviderRemarks = Convert.ToString(reader["QABillingProviderIDRemark"]);

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
                            //chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            chartSummaryDTO.QAMod = Convert.ToString(reader["QAMod"]);
                            chartSummaryDTO.QAModRemarks = Convert.ToString(reader["QAModRemark"]);
                            if (reader["ProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.ProviderFeedbackID = Convert.ToString(reader["ProviderFeedbackID"]);
                            if (reader["QAProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderFeedbackID = Convert.ToString(reader["QAProviderFeedbackID"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QAProviderFeedbackIDRemark"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);

                            if (reader["ShadowQAPayorID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAPayorID = Convert.ToInt32(reader["ShadowQAPayorID"]);
                            chartSummaryDTO.ShadowQAPayorRemarks = Convert.ToString(reader["ShadowQAPayorIdRemark"]);

                            if (reader["ShadowQAProviderID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAProviderID = Convert.ToInt32(reader["ShadowQAProviderID"]);
                            chartSummaryDTO.ShadowQAProviderRemarks = Convert.ToString(reader["ShadowQAProviderIDRemark"]);

                            if (reader["ShadowQABillingProviderID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQABillingProviderID = Convert.ToInt32(reader["ShadowQABillingProviderID"]);
                            chartSummaryDTO.ShadowQABillingProviderRemarks = Convert.ToString(reader["ShadowQABillingProviderIDRemark"]);


                            chartSummaryDTO.ShadowQADx = Convert.ToString(reader["ShadowQADx"]);
                            chartSummaryDTO.ShadowQADxRemarks = Convert.ToString(reader["ShadowQADxRemark"]);

                            chartSummaryDTO.ShadowQAMod = Convert.ToString(reader["ShadowQAMod"]);
                            chartSummaryDTO.ShadowQAModRemarks = Convert.ToString(reader["ShadowQAModRemark"]);

                            chartSummaryDTO.ShadowQACPTCode = Convert.ToString(reader["ShadowQACPTCode"]);
                            chartSummaryDTO.ShadowQACPTCodeRemarks = Convert.ToString(reader["ShadowQACPTCodeRemark"]);

                            if (reader["QAErrorTypeId"] != DBNull.Value)
                            {
                                chartSummaryDTO.QADTO.ErrorType = Convert.ToString(reader["QAErrorTypeId"]);
                                chartSummaryDTO.QAErrorTypeText = Convert.ToString(reader["QAErrorTypeText"]);
                            }

                            if (reader["ShadowQAErrorTypeId"] != DBNull.Value)
                            {
                                chartSummaryDTO.ShadowQADTO.ErrorType = Convert.ToString(reader["ShadowQAErrorTypeId"]);
                                chartSummaryDTO.ShadowQAErrorTypeText = Convert.ToString(reader["ShadowQAErrorTypeText"]);
                            }

                            if (reader["ShadowQAProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAProviderFeedbackID = Convert.ToString(reader["ShadowQAProviderFeedbackID"]);
                            chartSummaryDTO.ShadowQAProviderFeedbackRemarks = Convert.ToString(reader["ShadowQAProviderFeedbackIDRemark"]);

                            //chartSummaryDTO.RevisedPayorRemarks = Convert.ToString(reader["RebuttedPayorIdRemark"]);
                            //chartSummaryDTO.RevisedProviderRemarks = Convert.ToString(reader["RebuttedProviderIDRemark"]);

                            //chartSummaryDTO.RevisedDX = Convert.ToString(reader["RebuttedDx"]);
                            //chartSummaryDTO.RevisedDXRemarks = Convert.ToString(reader["RebuttedDxRemark"]);

                            //chartSummaryDTO.RevisedCPTRemarks = Convert.ToString(reader["RebuttedCPTCodeRemark"]);
                            //chartSummaryDTO.RevisedCPTCode = Convert.ToString(reader["RebuttedCPTCode"]);

                            //chartSummaryDTO.RevisedProviderFeedbackRemarks = Convert.ToString(reader["RebuttedProviderFeedbackIDRemark"]);

                        }
                        else if (Role == "QA" && ChartType == "RebuttalOfCoder")
                        {
                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            chartSummaryDTO.QABy = Convert.ToString(reader["QABy"]);
                            chartSummaryDTO.ShadowQABy = Convert.ToString(reader["ShadowQABy"]);

                            chartSummaryDTO.ProjectTypename = Convert.ToString(reader["ProjectTypeName"]);

                            if (reader["ClaimId"] != DBNull.Value)
                                chartSummaryDTO.ClaimId = Convert.ToInt32(reader["ClaimId"]);
                            else
                                chartSummaryDTO.ClaimId = null;
                            chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            if (reader["QAProviderID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderID = Convert.ToInt32(reader["QAProviderID"]);
                            chartSummaryDTO.QAProviderRemarks = Convert.ToString(reader["QAProviderIDRemark"]);

                            if (reader["BillingProviderId"] != DBNull.Value)
                                chartSummaryDTO.BillingProviderID = Convert.ToInt32(reader["BillingProviderId"]);
                            if (reader["QABillingProviderID"] != DBNull.Value)
                                chartSummaryDTO.QABillingProviderID = Convert.ToInt32(reader["QABillingProviderID"]);
                            chartSummaryDTO.QABillingProviderRemarks = Convert.ToString(reader["QABillingProviderIDRemark"]);

                            chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            if (reader["QAPayorID"] != DBNull.Value)
                                chartSummaryDTO.QAPayorID = Convert.ToInt32(reader["QAPayorID"]);
                            chartSummaryDTO.QAPayorRemarks = Convert.ToString(reader["QAPayorIdRemark"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.QADx = Convert.ToString(reader["QADx"]);//Convert.ToString(reader["RebuttedDx"]);
                            chartSummaryDTO.QADxRemarks = Convert.ToString(reader["QADxRemark"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            chartSummaryDTO.QACPTCode = Convert.ToString(reader["QACPTCode"]);//Convert.ToString(reader["RebuttedCPTCode"]); 
                            chartSummaryDTO.QACPTCodeRemarks = Convert.ToString(reader["QACPTCodeRemark"]);

                            if (reader["ProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.ProviderFeedbackID = Convert.ToString(reader["ProviderFeedbackID"]);
                            if (reader["QAProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderFeedbackID = Convert.ToString(reader["QAProviderFeedbackID"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QAProviderFeedbackIDRemark"]);
                            if (reader["QAErrorTypeId"] != DBNull.Value)
                            {
                                chartSummaryDTO.QADTO.ErrorType = Convert.ToString(reader["QAErrorTypeId"]);
                                chartSummaryDTO.QAErrorTypeText = Convert.ToString(reader["QAErrorTypeText"]);
                            }
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                            chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            chartSummaryDTO.BillingProviderText = Convert.ToString(reader["BillingProviderText"]);
                            chartSummaryDTO.PayorText = Convert.ToString(reader["PayorText"]);
                            chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["ProviderFeedbackText"]);
                            chartSummaryDTO.QAProviderText = Convert.ToString(reader["QAProviderText"]);
                            chartSummaryDTO.QABillingProviderText = Convert.ToString(reader["QABillingProviderText"]);
                            chartSummaryDTO.QAPayorText = Convert.ToString(reader["QAPayorText"]);
                            chartSummaryDTO.QAProviderFeedbackText = Convert.ToString(reader["QAProviderFeedbackText"]);
                            chartSummaryDTO.RevisedPayorRemarks = Convert.ToString(reader["RebuttedPayorIdRemark"]);
                            chartSummaryDTO.RevisedProviderRemarks = Convert.ToString(reader["RebuttedProviderIDRemark"]);
                            chartSummaryDTO.RevisedBillingProviderRemarks = Convert.ToString(reader["RebuttedBillingProviderIDRemark"]);
                            chartSummaryDTO.RevisedCPTRemarks = Convert.ToString(reader["RebuttedCPTCodeRemark"]);
                            chartSummaryDTO.RevisedModRemarks = Convert.ToString(reader["RebuttedModRemark"]);
                            chartSummaryDTO.RevisedDXRemarks = Convert.ToString(reader["RebuttedDxRemark"]);
                            chartSummaryDTO.RevisedProviderFeedbackRemarks = Convert.ToString(reader["RebuttedProviderFeedbackIDRemark"]);
                        }
                        //else if (Role == "QA" && ChartType == "ShadowQARejected")
                        //{
                        //    chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                        //    chartSummaryDTO.QABy = Convert.ToString(reader["QABy"]);
                        //    chartSummaryDTO.ShadowQABy = Convert.ToString(reader["ShadowQABy"]);

                        //    chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                        //    if (reader["QAProviderID"] != DBNull.Value)
                        //        chartSummaryDTO.QAProviderID = Convert.ToInt32(reader["QAProviderID"]);
                        //    chartSummaryDTO.QAProviderRemarks = Convert.ToString(reader["QAProviderIDRemark"]);
                        //    chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                        //    if (reader["QAPayorID"] != DBNull.Value)
                        //        chartSummaryDTO.QAPayorID = Convert.ToInt32(reader["QAPayorID"]);
                        //    chartSummaryDTO.QAPayorRemarks = Convert.ToString(reader["QAPayorIdRemark"]);
                        //    chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                        //    chartSummaryDTO.QADx = Convert.ToString(reader["QADx"]);
                        //    chartSummaryDTO.QADxRemarks = Convert.ToString(reader["QADxRemark"]);
                        //    chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                        //    chartSummaryDTO.QACPTCode = Convert.ToString(reader["QACPTCode"]);
                        //    chartSummaryDTO.QACPTCodeRemarks = Convert.ToString(reader["QACPTCodeRemark"]);
                        //    chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                        //    chartSummaryDTO.QAMod = Convert.ToString(reader["QAMod"]);
                        //    chartSummaryDTO.QAModRemarks = Convert.ToString(reader["QAModRemark"]);
                        //    chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackID"]);
                        //    if (reader["QAProviderFeedbackID"] != DBNull.Value)
                        //        chartSummaryDTO.QAProviderFeedbackID = Convert.ToInt32(reader["QAProviderFeedbackID"]);
                        //    if (reader["QAErrorTypeId"] != DBNull.Value)
                        //        chartSummaryDTO.QADTO.ErrorType = Convert.ToInt32(reader["QAErrorTypeId"]);
                        //    chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QAProviderFeedbackIDRemark"]);
                        //    chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                        //}
                        lstchartSummaryDTO.Add(chartSummaryDTO);
                    }
                    reader.NextResult();

                    while (reader.Read())
                    {
                        lstchartSummaryDTO.ForEach(x => x.CCIDs = Convert.ToString(reader["CCIDs"]));
                        //chartSummaryDTO.CCIDs = Convert.ToString(reader["CCIDs"]);
                    }

                    reader.NextResult();
                    //Below code is for Getting Block history in QA screen
                    while (reader.Read())
                    {
                        lstchartSummaryDTO.FirstOrDefault().blockHistories.Add(new BlockDTO
                        {
                            Name = Convert.ToString(reader["BlockCategory"]),
                            Remarks = Convert.ToString(reader["BlockRemarks"]),
                            Queue = Convert.ToString(reader["Queue"]),
                            BlockedBy = Convert.ToString(reader["BlockedBy"]),
                            CreateDate = Convert.ToDateTime(reader["BlockedDate"]).ToLocalDate(timeZoneCookie)
                        });
                    }
                }
                return lstchartSummaryDTO;

            }
        }

        public DataSet GetAgingReport(string projectType = null)
        {
            DataSet ds = new DataSet();
            using (var context = new UABContext())
            {
                using var con = context.Database.GetDbConnection();
                using SqlConnection conn = new SqlConnection(con.ConnectionString);
                conn.Open();
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    da.SelectCommand = new SqlCommand("[dbo].[UspAgingDashboard]", conn);
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddWithValue("@ProjectType", projectType != null && projectType.ToUpper() == "ALL" ? null : projectType);
                    da.Fill(ds);
                    ds.Tables[0].TableName = "AgingBreakdownByProject";
                    ds.Tables[1].TableName = "AgingBreakdownByStatus";
                }
            }
            return ds;
        }

        //public DataSet GetAgingReportOnSelection(string projectType)
        //{
        //    DataSet ds = new DataSet();
        //    using (var context = new UABContext())
        //    {
        //        using var con = context.Database.GetDbConnection();
        //        using SqlConnection conn = new SqlConnection(con.ConnectionString);
        //        conn.Open();
        //        using (SqlDataAdapter da = new SqlDataAdapter())
        //        {
        //            da.SelectCommand = new SqlCommand("[dbo].[UspAgingDashboardOnSelection]", conn);
        //            da.SelectCommand.CommandType = CommandType.StoredProcedure;
        //            da.SelectCommand.Parameters.AddWithValue("@ProjectType", projectType);
        //            da.Fill(ds);
        //            ds.Tables[0].TableName = "AgingBreakdownByProject";
        //            ds.Tables[1].TableName = "AgingBreakdownByStatus";
        //        }
        //    }
        //    return ds;
        //}

        public List<ChartSummaryDTO> GetAgingReportDetails(string ColumnName, string projectTypeName, int projectID)
        {
            List<ChartSummaryDTO> lst = new List<ChartSummaryDTO>();
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            chartSummaryDTO.ProjectID = projectID;
            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                    new SqlParameter() {
                            ParameterName = "@ProjectId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = projectID
                        },
                    new SqlParameter() {
                            ParameterName = "@ProjectTypeName",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = projectTypeName
                        },
                     new SqlParameter() {
                            ParameterName = "@ColumnName",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = ColumnName
                        }
                    };
                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[USPAgingBreakDownDetailedReportByDay]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        chartSummaryDTO = new ChartSummaryDTO();
                        chartSummaryDTO.CodingDTO.ListName = Convert.ToString(reader["ListName"]);
                        chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        var dos = Convert.ToDateTime(reader["DateOfService"]);
                        chartSummaryDTO.CodingDTO.DateOfService = dos.ToString("MM/dd/yyyy");
                        chartSummaryDTO.CodingDTO.Provider = Convert.ToString(reader["Provider"]);
                        lst.Add(chartSummaryDTO);
                    }
                }
            }
            return lst;
        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public List<ChartSummaryDTO> GetAgingReportDetailsByStatus(string ColumnName, string projectTypeName, int projectID)
        {
            List<ChartSummaryDTO> lst = new List<ChartSummaryDTO>();
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            chartSummaryDTO.ProjectID = projectID;
            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                    new SqlParameter() {
                            ParameterName = "@ProjectId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = projectID
                        },
                    new SqlParameter() {
                            ParameterName = "@ProjectTypeName",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = projectTypeName
                        },
                     new SqlParameter() {
                            ParameterName = "@ColumnName",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = ColumnName
                        }
                    };
                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[USPAgingBreakDownDetailedReportByStatus]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        chartSummaryDTO = new ChartSummaryDTO();
                        chartSummaryDTO.CodingDTO.ListName = Convert.ToString(reader["ListName"]);
                        chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        var dos = Convert.ToDateTime(reader["DateOfService"]);
                        chartSummaryDTO.CodingDTO.DateOfService = dos.ToString("MM/dd/yyyy");
                        chartSummaryDTO.CodingDTO.Provider = Convert.ToString(reader["Provider"]);
                        lst.Add(chartSummaryDTO);
                    }
                }
            }
            return lst;
        }

        public List<ChartSummaryDTO> GetAgingReportDetailsForBlockedCharts(string ColumnName, int projectID, string timeZoneCookie)
        {
            List<ChartSummaryDTO> lst = new List<ChartSummaryDTO>();
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
                            ParameterName = "@ColumnName",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = ColumnName
                        }
                    };
                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspAgingDetails]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        chartSummaryDTO = new ChartSummaryDTO();
                        chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        chartSummaryDTO.ProviderText = Convert.ToString(reader["Provider"]);
                        chartSummaryDTO.Status = Convert.ToString(reader["Status"]);
                        chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        chartSummaryDTO.CodingDTO.ClinicalCaseID = Convert.ToInt32(reader["ClinicalCaseId"]);
                        var dos = Convert.ToDateTime(reader["DateOfService"]);
                        chartSummaryDTO.CodingDTO.DateOfService = dos.ToString("MM/dd/yyyy");
                        chartSummaryDTO.BlockCategory = Convert.ToString(reader["BlockCategory"]);
                        chartSummaryDTO.BlockRemarks = Convert.ToString(reader["BlockRemarks"]);
                        chartSummaryDTO.BlockedDate = Convert.ToDateTime(reader["BlockedDate"]).ToLocalDate(timeZoneCookie);
                        chartSummaryDTO.Blockedbyuser = Convert.ToString(reader["BlockedByUser"]);
                        lst.Add(chartSummaryDTO);
                    }

                    reader.NextResult();

                    while (reader.Read())
                    {
                        int CCId = Convert.ToInt32(reader["ClinicalCaseId"]);
                        var item = lst.Where(x => x.CodingDTO.ClinicalCaseID == CCId).FirstOrDefault();
                        item.blockHistories.Add(new BlockDTO()
                        {
                            ClinicalCaseId = Convert.ToInt32(reader["ClinicalCaseId"]),
                            Name = Convert.ToString(reader["BlockCategory"]),
                            Queue = Convert.ToString(reader["Queue"]),
                            BlockedBy = Convert.ToString(reader["BlockedBy"]),
                            Remarks = Convert.ToString(reader["BlockRemarks"]),
                            CreateDate = Convert.ToDateTime(reader["BlockedDate"]).ToLocalDate(timeZoneCookie)
                        });
                    }
                }
            }
            return lst;
        }
        public void SaveOrUnblockchart(int cid, string responseremarks, string flag)
        {
            using (var context = new UABContext())
            {
                BlockResponse mdl = new BlockResponse()
                {
                    ClinicalCaseId = cid,
                    ResponseRemarks = responseremarks,
                    ResponseByUserId = mUserId,
                    ResponseDate = DateTime.UtcNow
                };
                context.BlockResponse.Add(mdl);

                if (flag == "Unblock")
                {
                    var existingblockchart = context.WorkItem.Where(a => a.ClinicalCaseId == cid).FirstOrDefault();

                    if (existingblockchart != null)
                    {
                        existingblockchart.IsBlocked = 0;
                        context.Entry(existingblockchart).State = EntityState.Modified;
                    }
                }

                context.SaveChanges();
            }
        }
        public DataSet GetLevellingReport(int projectID, DateTime startDate, DateTime endDate, string dateType, int? listId, int? providerId)
        {
            DataSet ds = new DataSet();
            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@ProjectId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = projectID
                        },
                        new SqlParameter() {
                            ParameterName = "@DoSStart",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = startDate
                        },
                        new SqlParameter() {
                            ParameterName = "@DoSEnd",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = endDate
                        },
                        new SqlParameter() {
                            ParameterName = "@DateType",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = dateType
                        },
                        new SqlParameter() {
                            ParameterName = "@ListId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = listId
                        },
                        new SqlParameter() {
                            ParameterName = "@ProviderId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = providerId
                        }
                };

                using var con = context.Database.GetDbConnection();
                using SqlConnection conn = new SqlConnection(con.ConnectionString);
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    da.SelectCommand = new SqlCommand("[dbo].[Rpt_GenerateEMLevellingReport]", conn);
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddRange(param);
                    da.Fill(ds);
                }
            }
            return ds;
        }

        public DataSet GetReceivedChartsReport(int projectID, string rangeType, DateTime startDate, DateTime endDate, double timeZoneOffSet)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@ProjectId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = projectID
                        },
                        new SqlParameter() {
                            ParameterName = "@RangeType",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = rangeType
                        },
                        new SqlParameter() {
                            ParameterName = "@StartDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = startDate
                        },
                        new SqlParameter() {
                            ParameterName = "@EndDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = endDate
                        },
                        new SqlParameter() {
                            ParameterName = "@TimeZoneOffset",
                            SqlDbType =  System.Data.SqlDbType.Decimal,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = timeZoneOffSet
                        }
                };

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspReceivedChartReport]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();
                    dt.Load(reader);
                    ds.Tables.Add(dt);
                }
            }
            return ds;
        }
        public List<ChartSummaryDTO> GetReceivedChartReportDetails(DateTime date, int week, string month, string year, int projectID, string range, double timeZoneOffSet, DateTime StartDate, DateTime EndDate, DateTime weekStartDate, DateTime weekEndDate)
        {
            DateValueMustNotBeLessThanCertainDate(ref weekStartDate, ref weekEndDate);
            List<ChartSummaryDTO> lst = new List<ChartSummaryDTO>();
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            chartSummaryDTO.ProjectID = projectID;
            if (date == new DateTime())
                date = DateTime.UtcNow;
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
                            ParameterName = "@RangeType",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = range
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Date",
                            SqlDbType =  System.Data.SqlDbType.Date,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = date
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Week",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = week
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Month",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = month
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Year",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = year
                        },
                         new SqlParameter() {
                            ParameterName = "@TimeZoneOffset",
                            SqlDbType =  System.Data.SqlDbType.Decimal,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = timeZoneOffSet
                        } ,   new SqlParameter() {
                            ParameterName = "@StartDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = StartDate
                        } ,   new SqlParameter() {
                            ParameterName = "@EndDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = EndDate
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@WeekStartDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = weekStartDate
                        } ,   new SqlParameter() {
                            ParameterName = "@WeekEndDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = weekEndDate
                        }
                    };
                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspReceivedChartDetailedReport]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        chartSummaryDTO = new ChartSummaryDTO();
                        chartSummaryDTO.CodingDTO.ListName = Convert.ToString(reader["ListName"]);
                        chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        var dos = Convert.ToDateTime(reader["DateOfService"]);
                        chartSummaryDTO.CodingDTO.DateOfService = dos.ToString("MM/dd/yyyy");
                        chartSummaryDTO.CodingDTO.Provider = Convert.ToString(reader["Provider"]);
                        lst.Add(chartSummaryDTO);
                    }
                }
            }
            return lst;
        }

        public List<ChartSummaryDTO> GetCodedChartReportDetails(DateTime date, int week, string month, string year, int projectID, string range, double timeZoneOffSet, DateTime StartDate, DateTime EndDate, DateTime weekStartDate, DateTime weekEndDate)
        {
            DateValueMustNotBeLessThanCertainDate(ref weekStartDate, ref weekEndDate);
            List<ChartSummaryDTO> lst = new List<ChartSummaryDTO>();
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
                            ParameterName = "@RangeType",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = range
                        },   new SqlParameter() {
                            ParameterName = "@Date",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = date
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Week",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = week
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Month",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = month
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Year",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = year
                        } ,
                        new SqlParameter() {
                            ParameterName = "@TimeZoneOffset",
                            SqlDbType =  System.Data.SqlDbType.Decimal,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = timeZoneOffSet
                        } ,   new SqlParameter() {
                            ParameterName = "@StartDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = StartDate
                        } ,   new SqlParameter() {
                            ParameterName = "@EndDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = EndDate
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@WeekStartDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = weekStartDate
                        } ,   new SqlParameter() {
                            ParameterName = "@WeekEndDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = weekEndDate
                        }
                    };
                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspCodedChartDetailedReport]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        chartSummaryDTO = new ChartSummaryDTO();
                        chartSummaryDTO.CodingDTO.ListName = Convert.ToString(reader["ListName"]);
                        chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        var dos = Convert.ToDateTime(reader["DateOfService"]);
                        chartSummaryDTO.CodingDTO.DateOfService = dos.ToString("MM/dd/yyyy");
                        chartSummaryDTO.CodingDTO.Provider = Convert.ToString(reader["Provider"]);
                        //chartSummaryDTO.CodingDTO.ListName = Convert.ToString(reader["ListName"]);
                        //chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        //chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        //var dos = Convert.ToDateTime(reader["DateOfService"]);
                        //chartSummaryDTO.CodingDTO.DateOfService = dos.ToString("MM/dd/yyyy");
                        //chartSummaryDTO.ProviderName = Convert.ToString(reader["Provider"]);
                        lst.Add(chartSummaryDTO);
                    }
                }
            }
            return lst;
        }

        public List<ChartSummaryDTO> GetQAChartReportDetails(DateTime date, int week, string month, string year, int projectID, string range, double timeZoneOffSet, DateTime StartDate, DateTime EndDate, DateTime weekStartDate, DateTime weekEndDate)
        {
            DateValueMustNotBeLessThanCertainDate(ref weekStartDate, ref weekEndDate);
            List<ChartSummaryDTO> lst = new List<ChartSummaryDTO>();
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            chartSummaryDTO.ProjectID = projectID;
            if (date == new DateTime())
                date = DateTime.UtcNow;
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
                            ParameterName = "@RangeType",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = range
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Date",
                            SqlDbType =  System.Data.SqlDbType.Date,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = date
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Week",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = week
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Month",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = month
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Year",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = year
                        },
                         new SqlParameter() {
                            ParameterName = "@TimeZoneOffset",
                            SqlDbType =  System.Data.SqlDbType.Decimal,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = timeZoneOffSet
                        } ,   new SqlParameter() {
                            ParameterName = "@StartDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = StartDate
                        } ,   new SqlParameter() {
                            ParameterName = "@EndDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = EndDate
                        },   new SqlParameter() {
                            ParameterName = "@WeekStartDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = weekStartDate
                        } ,   new SqlParameter() {
                            ParameterName = "@WeekEndDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = weekEndDate
                        }
                    };
                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspQAChartDetailedReport]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        chartSummaryDTO = new ChartSummaryDTO();
                        chartSummaryDTO.CodingDTO.ListName = Convert.ToString(reader["ListName"]);
                        chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        var dos = Convert.ToDateTime(reader["DateOfService"]);
                        chartSummaryDTO.CodingDTO.DateOfService = dos.ToString("MM/dd/yyyy");
                        chartSummaryDTO.CodingDTO.Provider = Convert.ToString(reader["Provider"]);
                        //chartSummaryDTO.CodingDTO.ListName = Convert.ToString(reader["ListName"]);
                        //chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        //chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        //var dos = Convert.ToDateTime(reader["DateOfService"]);
                        //chartSummaryDTO.CodingDTO.DateOfService = dos.ToString("MM/dd/yyyy");
                        //chartSummaryDTO.ProviderName = Convert.ToString(reader["Provider"]);
                        lst.Add(chartSummaryDTO);
                    }
                }
            }
            return lst;
        }

        public List<ChartSummaryDTO> GetPostedChartReportDetails(DateTime date, int week, string month, string year, int projectID, string range, double timeZoneOffSet, DateTime StartDate, DateTime EndDate, DateTime weekStartDate, DateTime weekEndDate)
        {
            DateValueMustNotBeLessThanCertainDate(ref weekStartDate, ref weekEndDate);
            List<ChartSummaryDTO> lst = new List<ChartSummaryDTO>();
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            chartSummaryDTO.ProjectID = projectID;
            if (date == new DateTime())
                date = DateTime.UtcNow;
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
                            ParameterName = "@RangeType",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = range
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Date",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = date
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Week",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = week
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Month",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = month
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Year",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = year
                        },   new SqlParameter() {
                            ParameterName = "@StartDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = StartDate
                        } ,   new SqlParameter() {
                            ParameterName = "@EndDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = EndDate
                        },
                        new SqlParameter() {
                            ParameterName = "@TimeZoneOffset",
                            SqlDbType =  System.Data.SqlDbType.Decimal,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = timeZoneOffSet
                        },   new SqlParameter() {
                            ParameterName = "@WeekStartDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = weekStartDate
                        } ,   new SqlParameter() {
                            ParameterName = "@WeekEndDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = weekEndDate
                        }
                    };
                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspPostedChartDetailedReport]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        chartSummaryDTO = new ChartSummaryDTO();
                        chartSummaryDTO.CodingDTO.ListName = Convert.ToString(reader["ListName"]);
                        chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        var dos = Convert.ToDateTime(reader["DateOfService"]);
                        chartSummaryDTO.CodingDTO.DateOfService = dos.ToString("MM/dd/yyyy");
                        chartSummaryDTO.CodingDTO.Provider = Convert.ToString(reader["Provider"]);
                        //chartSummaryDTO.CodingDTO.ListName = Convert.ToString(reader["ListName"]);
                        //chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        //chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        //var dos = Convert.ToDateTime(reader["DateOfService"]);
                        //chartSummaryDTO.CodingDTO.DateOfService = dos.ToString("MM/dd/yyyy");
                        //chartSummaryDTO.ProviderName = Convert.ToString(reader["Provider"]);
                        lst.Add(chartSummaryDTO);
                    }
                }
            }
            return lst;
        }

        public List<ChartSummaryDTO> GetProviderPostedChartReportDetails(DateTime date, int week, string month, string year, int projectID, string range, DateTime weekStartDate, DateTime weekEndDate)
        {
            DateValueMustNotBeLessThanCertainDate(ref weekStartDate, ref weekEndDate);
            List<ChartSummaryDTO> lst = new List<ChartSummaryDTO>();
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            chartSummaryDTO.ProjectID = projectID;
            if (date == new DateTime())
                date = DateTime.UtcNow;
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
                            ParameterName = "@RangeType",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = range
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Date",
                            SqlDbType =  System.Data.SqlDbType.Date,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = date
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Week",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = week
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Month",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = month
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Year",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = year
                        },   new SqlParameter() {
                            ParameterName = "@WeekStartDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = weekStartDate
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@WeekEndDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = weekEndDate
                        }
                    };
                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspProviderPostedChartDetailedReport]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        chartSummaryDTO = new ChartSummaryDTO();
                        chartSummaryDTO.CodingDTO.ListName = Convert.ToString(reader["ListName"]);
                        chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        var dos = Convert.ToDateTime(reader["DateOfService"]);
                        chartSummaryDTO.CodingDTO.DateOfService = dos.ToString("MM/dd/yyyy");
                        chartSummaryDTO.CodingDTO.Provider = Convert.ToString(reader["Provider"]);
                        lst.Add(chartSummaryDTO);
                    }
                }
            }
            return lst;
        }

        public List<ChartSummaryDTO> GetChartSummaryReportDetails(int projectID, DateTime CurrentDos, string ColumnName, string DateType)
        {
            List<ChartSummaryDTO> lst = new List<ChartSummaryDTO>();
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
                            ParameterName = "@CurrentDos",
                            SqlDbType =  System.Data.SqlDbType.Date,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = CurrentDos
                        },   new SqlParameter() {
                            ParameterName = "@ColumnName",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = ColumnName
                        },   new SqlParameter() {
                            ParameterName = "@DateType",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = DateType
                        }
                    };
                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[Rpt_GenerateChartSummaryReportDetails]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        chartSummaryDTO = new ChartSummaryDTO();
                        chartSummaryDTO.CodingDTO.ListName = Convert.ToString(reader["ListName"]);
                        chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        var dos = Convert.ToDateTime(reader["Date"]);
                        chartSummaryDTO.CodingDTO.DateOfService = dos.ToString("MM/dd/yyyy");
                        chartSummaryDTO.ProviderName = Convert.ToString(reader["Provider"]);
                        lst.Add(chartSummaryDTO);
                    }
                }
            }
            return lst;
        }
        public DataSet GetChartSummaryReport(int projectID, DateTime StartDate, DateTime EndDate, string dateType)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@ProjectId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = projectID
                        },
                        new SqlParameter() {
                            ParameterName = "@DoSStart",
                            SqlDbType =  System.Data.SqlDbType.Date,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = StartDate
                        },
                        new SqlParameter() {
                            ParameterName = "@DoSEnd",
                            SqlDbType =  System.Data.SqlDbType.Date,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = EndDate
                        },
                        new SqlParameter() {
                            ParameterName = "@DateType",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = dateType
                        }
                };

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[Rpt_GenerateChartSummaryReport]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();
                    dt.Load(reader);
                    ds.Tables.Add(dt);
                }
            }
            return ds;
        }

        public DataTable GetDetailedChartSummaryReport(int projectID, DateTime StartDate, DateTime EndDate, string dateType)
        {
            DataTable finalResult = new DataTable();
            DataSet ds = new DataSet();

            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@ProjectId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = projectID
                        },
                        new SqlParameter() {
                            ParameterName = "@StartDate",
                            SqlDbType =  System.Data.SqlDbType.Date,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = StartDate
                        },
                        new SqlParameter() {
                            ParameterName = "@EndDate",
                            SqlDbType =  System.Data.SqlDbType.Date,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = EndDate
                        },
                        new SqlParameter() {
                            ParameterName = "@DateType",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = dateType
                        }
                };

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[USPGetChartSummaryReportAllData]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        do
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);
                            ds.Tables.Add(dt);
                        } while (!reader.IsClosed);
                    }

                    ds.Tables[0].Columns["DX"].MaxLength = 500;
                    ds.Tables[0].Columns["CPT"].MaxLength = 500;
                    ds.Tables[0].Columns["Mod"].MaxLength = 100;
                    ds.Tables[0].Columns["Qty"].MaxLength = 100;
                    ds.Tables[0].Columns["Links"].MaxLength = 100;

                    finalResult = ds.Tables[0].Clone();

                    if (ds.Tables[1].Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            DataRow[] dataDxInfo = ds.Tables[1].Select("ClinicalCaseID='" + row["ClinicalCaseID"].ToString() + "'");

                            if (dataDxInfo.Count() > 0)
                            {
                                for (int i = 0; i < dataDxInfo.Count(); i++)
                                {
                                    row["DX"] = "Claim" + dataDxInfo[i]["ClaimOrder"].ToString() + ": " + dataDxInfo[i]["DxCode"].ToString();

                                    foreach (var cpt in dataDxInfo[i]["CPTCode"].ToString().Split("|"))
                                    {
                                        if (cpt != "NA-NA-NA-NA")
                                        {
                                            string[] lstCpt = cpt.Split("-");

                                            row["DX"] = "Claim" + dataDxInfo[i]["ClaimOrder"].ToString() + ": " + GetDxInfoByLink(dataDxInfo[i]["DxCode"].ToString(), (lstCpt.Count() > 3 ? lstCpt[3] : ""));

                                            row["CPT"] = "Claim" + dataDxInfo[i]["ClaimOrder"].ToString() + ": " + (lstCpt.Count() > 0 ? lstCpt[0] : "");
                                            row["Mod"] = lstCpt.Count() > 1 ? lstCpt[1] : "";
                                            row["Qty"] = lstCpt.Count() > 2 ? lstCpt[2] : "";
                                            row["Links"] = lstCpt.Count() > 3 ? ((lstCpt[3] == "" || lstCpt[3] == "null") ? "All" : lstCpt[3]) : "";

                                            finalResult.Rows.Add(row.ItemArray);
                                        }
                                    }
                                }
                            }
                            else
                                finalResult.Rows.Add(row.ItemArray);
                        }
                    }
                }
            }
            return finalResult;
        }
        string GetDxInfoByLink(string dx, string linkAll)
        {
            List<string> lstResult = new List<string>();

            if (linkAll == "" || linkAll == "null")
                return dx;
            else
            {
                foreach (var item in linkAll.Split(","))
                {
                    var isNumeric = !string.IsNullOrEmpty(item) && item.All(Char.IsDigit);

                    if (isNumeric)
                    {
                        int index = Convert.ToInt32(item);

                        if (dx.Split(",").Count() > (index - 1) && index > 0)
                            lstResult.Add(dx.Split(",")[index - 1]);
                    }
                }
            }

            return string.Join(",", lstResult);
        }
        public DataSet GetPostedChartsReport(int projectID, string rangeType, DateTime startDate, DateTime endDate, double timeZoneOffSet)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@ProjectId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = projectID
                        },
                        new SqlParameter() {
                            ParameterName = "@RangeType",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = rangeType
                        },
                        new SqlParameter() {
                            ParameterName = "@StartDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = startDate
                        },
                        new SqlParameter() {
                            ParameterName = "@EndDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = endDate
                        },
                        new SqlParameter() {
                            ParameterName = "@TimeZoneOffset",
                            SqlDbType =  System.Data.SqlDbType.Decimal,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = timeZoneOffSet
                        }
                };

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspPostedChartReport]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();
                    dt.Load(reader);
                    ds.Tables.Add(dt);
                }
            }
            return ds;
        }
        public DataSet GetBacklogChartsReport(int projectID, string rangeType, DateTime startDate, DateTime endDate)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@ProjectId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = projectID
                        },
                        new SqlParameter() {
                            ParameterName = "@RangeType",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = rangeType
                        },
                        new SqlParameter() {
                            ParameterName = "@StartDate",
                            SqlDbType =  System.Data.SqlDbType.Date,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = startDate
                        },
                        new SqlParameter() {
                            ParameterName = "@EndDate",
                            SqlDbType =  System.Data.SqlDbType.Date,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = endDate
                        }
                };

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspBacklogChartReport]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();
                    dt.Load(reader);
                    ds.Tables.Add(dt);
                }
            }
            return ds;
        }
        public List<ChartSummaryDTO> GetBacklogChartsReportDetails(int delaydays, int statusid, int projectid)
        {
            List<ChartSummaryDTO> lst = new List<ChartSummaryDTO>();
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            chartSummaryDTO.ProjectID = projectid;
            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@ProjectID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = projectid
                        },
                        new SqlParameter() {
                            ParameterName = "@statusId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = statusid
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@DelayDays",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = delaydays
                        }
                    };
                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspBacklogChartReportDetails]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        chartSummaryDTO = new ChartSummaryDTO();
                        chartSummaryDTO.CodingDTO.ListName = Convert.ToString(reader["ListName"]);
                        chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        var dos = Convert.ToDateTime(reader["DateOfService"]);
                        chartSummaryDTO.CodingDTO.DateOfService = dos.ToString("MM/dd/yyyy");
                        chartSummaryDTO.CodingDTO.Provider = Convert.ToString(reader["Provider"]);
                        lst.Add(chartSummaryDTO);
                    }
                }
            }
            return lst;
        }

        public DataSet GetCodedChartsReport(int projectID, string rangeType, DateTime startDate, DateTime endDate, double timeZoneOffSet)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@ProjectId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = projectID
                        },
                        new SqlParameter() {
                            ParameterName = "@RangeType",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = rangeType
                        },
                        new SqlParameter() {
                            ParameterName = "@StartDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = startDate
                        },
                        new SqlParameter() {
                            ParameterName = "@EndDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = endDate
                        },
                        new SqlParameter() {
                            ParameterName = "@TimeZoneOffset",
                            SqlDbType =  System.Data.SqlDbType.Decimal,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = timeZoneOffSet
                        }
                };

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspCodedChartReport]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();
                    dt.Load(reader);
                    ds.Tables.Add(dt);
                }
            }
            return ds;
        }

        public void RemoveStartAndEndDate(DataSet dataSet, string range)
        {
            if (range == "PerWeek")
            {
                dataSet.Tables[0].Columns.Remove("Week Start Date");
                dataSet.Tables[0].Columns.Remove("Week End Date");
            }
        }

        public DataSet GetQAChartsReport(int projectID, string rangeType, DateTime startDate, DateTime endDate, double timeZoneOffSet)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@ProjectId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = projectID
                        },
                        new SqlParameter() {
                            ParameterName = "@RangeType",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = rangeType
                        },
                        new SqlParameter() {
                            ParameterName = "@StartDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = startDate
                        },
                        new SqlParameter() {
                            ParameterName = "@EndDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = endDate
                        },
                        new SqlParameter() {
                            ParameterName = "@TimeZoneOffset",
                            SqlDbType =  System.Data.SqlDbType.Decimal,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = timeZoneOffSet
                        }
                };

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspQAChartReport]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();
                    dt.Load(reader);
                    ds.Tables.Add(dt);
                }
            }
            return ds;
        }
        public DataSet GetProvidedpostedchartsChartsReport(int projectID, string rangeType, DateTime startDate, DateTime endDate)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@ProjectId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = projectID
                        },
                        new SqlParameter() {
                            ParameterName = "@RangeType",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = rangeType
                        },
                        new SqlParameter() {
                            ParameterName = "@StartDate",
                            SqlDbType =  System.Data.SqlDbType.Date,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = startDate
                        },
                        new SqlParameter() {
                            ParameterName = "@EndDate",
                            SqlDbType =  System.Data.SqlDbType.Date,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = endDate
                        }
                };

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspProvidedpostedChartReport]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();
                    dt.Load(reader);
                    ds.Tables.Add(dt);
                }
            }
            return ds;
        }

        public DataSet GetPendingChartsReport(int projectID, string rangeType, DateTime startDate, DateTime endDate, double timeZoneOffSet)
        {
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@ProjectId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = projectID
                        },
                        new SqlParameter() {
                            ParameterName = "@RangeType",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = rangeType
                        },
                        new SqlParameter() {
                            ParameterName = "@StartDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = startDate
                        },
                        new SqlParameter() {
                            ParameterName = "@EndDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = endDate
                        },
                        new SqlParameter() {
                            ParameterName = "@TimeZoneOffset",
                            SqlDbType =  System.Data.SqlDbType.Decimal,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = timeZoneOffSet
                        }
                };

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspPendingChartReport]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();
                    dt.Load(reader);
                    ds.Tables.Add(dt);
                }
            }
            return ds;
        }
        public List<ChartSummaryDTO> GetPendingReportDetails(DateTime date, int week, string month, string year, int projectID, string range, double timeZoneOffSet, DateTime StartDate, DateTime EndDate, DateTime weekStartDate, DateTime weekEndDate)
        {
            DateValueMustNotBeLessThanCertainDate(ref weekStartDate, ref weekEndDate);
            List<ChartSummaryDTO> lst = new List<ChartSummaryDTO>();
            ChartSummaryDTO chartSummaryDTO = new ChartSummaryDTO();
            chartSummaryDTO.ProjectID = projectID;
            if (date == new DateTime())
                date = DateTime.UtcNow;
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
                            ParameterName = "@RangeType",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = range
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Date",
                            SqlDbType =  System.Data.SqlDbType.Date,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = date
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Week",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = week
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Month",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = month
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@Year",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = year
                        }, new SqlParameter() {
                            ParameterName = "@TimeZoneOffset",
                            SqlDbType =  System.Data.SqlDbType.Decimal,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = timeZoneOffSet
                        } ,   new SqlParameter() {
                            ParameterName = "@StartDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = StartDate
                        } ,   new SqlParameter() {
                            ParameterName = "@EndDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = EndDate
                        },   new SqlParameter() {
                            ParameterName = "@WeekStartDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = weekStartDate
                        } ,   new SqlParameter() {
                            ParameterName = "@WeekEndDate",
                            SqlDbType =  System.Data.SqlDbType.DateTime,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = weekEndDate
                        }
                    };
                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspPendingChartDetailReport]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        chartSummaryDTO = new ChartSummaryDTO();
                        chartSummaryDTO.CodingDTO.ListName = Convert.ToString(reader["ListName"]);
                        chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        var dos = Convert.ToDateTime(reader["DateOfService"]);
                        chartSummaryDTO.CodingDTO.DateOfService = dos.ToString("MM/dd/yyyy");
                        chartSummaryDTO.CodingDTO.Provider = Convert.ToString(reader["Provider"]);
                        lst.Add(chartSummaryDTO);
                    }
                }
            }
            return lst;
        }


        public int? ClaimId { get; set; }

        private DataTable GetCpt(string cpt)
        {
            DataTable dtCPT = new DataTable();
            dtCPT.Columns.Add("RNO", typeof(int));
            dtCPT.Columns.Add("CPTCode", typeof(string));
            dtCPT.Columns.Add("Mod", typeof(string));
            dtCPT.Columns.Add("Qty", typeof(string));
            dtCPT.Columns.Add("Links", typeof(string));
            string[] lstcpts = cpt.Split("|");
            int i = 0;
            foreach (var item in lstcpts)
            {
                i = i + 1;
                string[] lstcptrow = item.Split("^");
                dtCPT.Rows.Add(i, lstcptrow[0], lstcptrow[1], lstcptrow[2], lstcptrow[3]);
            }
            return dtCPT;
        }
        public void SubmitProviderPostedChart(ChartSummaryDTO chartSummaryDto, int providerPostedId, int payorPostedId, DataTable dtClaim, DataTable dtCpt1, DataTable dtProDx, DataTable dtProCpt)
        {
            var dtCpt = GetCpt(chartSummaryDto.CPTCode);

            using var context = new UABContext();
            var param = new SqlParameter[] {
                new SqlParameter() {
                    SqlDbType =  System.Data.SqlDbType.Int,
                    ParameterName = "@PayorID",
                    Direction = System.Data.ParameterDirection.Input,
                    Value = chartSummaryDto.PayorID
                },
                new SqlParameter() {
                    ParameterName = "@NoteTitle",
                    SqlDbType =  System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = chartSummaryDto.NoteTitle
                },
                new SqlParameter() {
                    ParameterName = "@ProviderID",
                    SqlDbType =  System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = chartSummaryDto.ProviderID
                } ,
                new SqlParameter() {
                    ParameterName = "@BillingProviderID",
                    SqlDbType =  System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = chartSummaryDto.BillingProviderID
                }  ,
                new SqlParameter() {
                    ParameterName = "@utClaim1",
                    SqlDbType =  System.Data.SqlDbType.Structured,
                    Direction = System.Data.ParameterDirection.Input,
                    TypeName = "utClaim",
                    Value = dtClaim
                },

                new SqlParameter() {
                    ParameterName = "@utCpt1",
                    SqlDbType =  System.Data.SqlDbType.Structured,
                    Direction = System.Data.ParameterDirection.Input,
                    TypeName = "utCpt",
                    Value = dtCpt1
                },
                new SqlParameter()
                {
                    ParameterName = "@Dx",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = chartSummaryDto.Dx
                },
                new SqlParameter() {
                    ParameterName = "@utCpt",
                    SqlDbType =  System.Data.SqlDbType.Structured,
                    Direction = System.Data.ParameterDirection.Input,
                    TypeName = "utCpt",
                    Value = dtCpt
                },
                new SqlParameter() {
                    ParameterName = "@utDxCode",
                    SqlDbType =  System.Data.SqlDbType.Structured,
                    Direction = System.Data.ParameterDirection.Input,
                    TypeName = "utDxCode",
                    Value = dtProDx
                },
                new SqlParameter() {
                    ParameterName = "@utCptCode",
                    SqlDbType =  System.Data.SqlDbType.Structured,
                    Direction = System.Data.ParameterDirection.Input,
                    TypeName = "utCptCode",
                    Value = dtProCpt
                },
                new SqlParameter()
                {
                    ParameterName = "@ClinicalcaseID",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = chartSummaryDto.CodingDTO.ClinicalCaseID
                }
                ,   new SqlParameter()
                {
                    ParameterName = "@UserId",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = mUserId
                }
                ,
                new SqlParameter()
                {
                    ParameterName = "@ProviderFeedbackID",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = chartSummaryDto.ProviderFeedbackID
                } ,
                new SqlParameter() {
                    ParameterName = "@ProviderPostedId",
                    SqlDbType =  System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = providerPostedId
                }
                ,
                new SqlParameter() {
                    ParameterName = "@PayorPostedId",
                    SqlDbType =  System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = payorPostedId
                }  ,
                new SqlParameter() {
                    ParameterName = "@PostedDate",
                    SqlDbType = System.Data.SqlDbType.DateTime2,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = chartSummaryDto.PostingDate
                },
                new SqlParameter() {
                    ParameterName = "@CoderComment",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = chartSummaryDto.CoderComment
                },
                new SqlParameter() {
                    ParameterName = "@IsWrongProvider",
                    SqlDbType = System.Data.SqlDbType.Bit,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = chartSummaryDto.isWrongProvider
                }
            };

            using var con = context.Database.GetDbConnection();
            var cmd = con.CreateCommand();
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = "[dbo].[UspSubmitProviderPostedChart]";
            cmd.Parameters.AddRange(param);
            cmd.Connection = con;
            con.Open();

            int res = cmd.ExecuteNonQuery();
        }
        public void SubmitCodingAvailableChart(ChartSummaryDTO chartSummaryDTO, DataTable dtClaim, DataTable dtCpt1)
        {
            using (var context = new UABContext())
            {
                DataTable dtCPT = GetCpt(chartSummaryDTO.CPTCode);

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
                            ParameterName = "@BillingProviderID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.BillingProviderID
                        },
                         new SqlParameter() {
                            ParameterName = "@utCpt",
                            SqlDbType =  System.Data.SqlDbType.Structured,
                            Direction = System.Data.ParameterDirection.Input,
                            TypeName = "utCpt",
                            Value = dtCPT
                        },

                new SqlParameter()
                {
                    ParameterName = "@Mod",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = chartSummaryDTO.Mod
                }
                ,  new SqlParameter()
                 {
                     ParameterName = "@Dx",
                     SqlDbType = System.Data.SqlDbType.VarChar,
                     Direction = System.Data.ParameterDirection.Input,
                     Value = chartSummaryDTO.Dx
                 } , new SqlParameter()
                 {
                     ParameterName = "@ProviderFeedbackID",
                     SqlDbType = System.Data.SqlDbType.VarChar,
                     Direction = System.Data.ParameterDirection.Input,
                     Value = chartSummaryDTO.ProviderFeedbackID
                 }, new SqlParameter()
                 {
                     ParameterName = "@CoderQuestion",
                     SqlDbType = System.Data.SqlDbType.VarChar,
                     Direction = System.Data.ParameterDirection.Input,
                     Value = chartSummaryDTO.CoderQuestion
                 } ,   new SqlParameter()
                 {
                     ParameterName = "@ClinicalcaseID",
                     SqlDbType = System.Data.SqlDbType.Int,
                     Direction = System.Data.ParameterDirection.Input,
                     Value = chartSummaryDTO.CodingDTO.ClinicalCaseID
                 }
                ,   new SqlParameter()
                 {
                     ParameterName = "@UserId",
                     SqlDbType = System.Data.SqlDbType.Int,
                     Direction = System.Data.ParameterDirection.Input,
                     Value = mUserId
                 }
                ,
                 new SqlParameter() {
                    ParameterName = "@utClaim1",
                    SqlDbType =  System.Data.SqlDbType.Structured,
                    Direction = System.Data.ParameterDirection.Input,
                    TypeName = "utClaim",
                    Value = dtClaim
                 },

                 new SqlParameter() {
                    ParameterName = "@utCpt1",
                    SqlDbType =  System.Data.SqlDbType.Structured,
                    Direction = System.Data.ParameterDirection.Input,
                    TypeName = "utCpt",
                    Value = dtCpt1
                 },
                  new SqlParameter() {
                    ParameterName = "@IsAuditRequired",
                    SqlDbType = System.Data.SqlDbType.Bit,
                     Direction = System.Data.ParameterDirection.Input,
                     Value = chartSummaryDTO.IsAuditRequired
                 },
                  new SqlParameter() {
                    ParameterName = "@SubmitAndPostAlso",
                    SqlDbType = System.Data.SqlDbType.Bit,
                     Direction = System.Data.ParameterDirection.Input,
                     Value = chartSummaryDTO.SubmitAndPostAlso
                 },
                  new SqlParameter() {
                    ParameterName = "@IsWrongProvider",
                    SqlDbType = System.Data.SqlDbType.Bit,
                     Direction = System.Data.ParameterDirection.Input,
                     Value = chartSummaryDTO.isWrongProvider
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
        public void SubmitProviderPosted(int ClinicalcaseId, int UserID)
        {
            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@ClinicalcaseId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = ClinicalcaseId
                        },  new SqlParameter() {
                            ParameterName = "@UserId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = UserID
                        } };

                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[UspSubmitProviderPosted]";
                    cmd.Parameters.AddRange(param);
                    cmd.Connection = con;
                    con.Open();

                    int res = cmd.ExecuteNonQuery();
                }
            }
        }
        public CodingDTO SubmitQAAvailableChart(ChartSummaryDTO chartSummaryDTO, DataTable dtAudit)
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
                            ParameterName = "@UserId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = mUserId
                        },   new SqlParameter() {
                            ParameterName = "@IsAuditRequired",
                            SqlDbType =  System.Data.SqlDbType.Bit,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.IsAuditRequired
                        },
                          new SqlParameter() {
                            ParameterName = "@utWorkItemAudit1",
                            SqlDbType =  System.Data.SqlDbType.Structured,
                            Direction = System.Data.ParameterDirection.Input,
                            TypeName = "utWorkItemAudit1",
                            Value = dtAudit
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
        public CodingDTO SubmitCodingIncorrectChart(ChartSummaryDTO chartSummaryDTO, int statusId, DataTable dtAudit, DataTable dtbasicParams, DataTable dtDx, DataTable dtCpt)
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
                            ParameterName = "@UserId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = mUserId
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@StatusId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = statusId
                        },
                          new SqlParameter() {
                            ParameterName = "@utWorkItemAudit1",
                            SqlDbType =  System.Data.SqlDbType.Structured,
                            Direction = System.Data.ParameterDirection.Input,
                            TypeName = "utWorkItemAudit1",
                            Value = dtAudit
                        },
                          new SqlParameter() {
                            ParameterName = "@utBasicParams1",
                            SqlDbType =  System.Data.SqlDbType.Structured,
                            Direction = System.Data.ParameterDirection.Input,
                            TypeName = "utBasicParams1",
                            Value = dtbasicParams
                        },
                          new SqlParameter() {
                            ParameterName = "@utDxCode",
                            SqlDbType =  System.Data.SqlDbType.Structured,
                            Direction = System.Data.ParameterDirection.Input,
                            TypeName = "utDxCode",
                            Value = dtDx
                        },
                          new SqlParameter() {
                            ParameterName = "@utCptCode",
                            SqlDbType =  System.Data.SqlDbType.Structured,
                            Direction = System.Data.ParameterDirection.Input,
                            TypeName = "utCptCode",
                            Value = dtCpt
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

        public CodingDTO SubmitQARebuttalChartsOfCoder(ChartSummaryDTO chartSummaryDTO, DataTable dtAudit, DataTable dtbasicParams, DataTable dtDx, DataTable dtCpt)
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
                            ParameterName = "@UserId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = mUserId
                        },
                          new SqlParameter() {
                            ParameterName = "@utWorkItemAudit1",
                            SqlDbType =  System.Data.SqlDbType.Structured,
                            Direction = System.Data.ParameterDirection.Input,
                            TypeName = "utWorkItemAudit1",
                            Value = dtAudit
                        },
                          new SqlParameter() {
                            ParameterName = "@utBasicParams1",
                            SqlDbType =  System.Data.SqlDbType.Structured,
                            Direction = System.Data.ParameterDirection.Input,
                            TypeName = "utBasicParams1",
                            Value = dtbasicParams
                        },
                          new SqlParameter() {
                            ParameterName = "@utDxCode",
                            SqlDbType =  System.Data.SqlDbType.Structured,
                            Direction = System.Data.ParameterDirection.Input,
                            TypeName = "utDxCode",
                            Value = dtDx
                        },
                          new SqlParameter() {
                            ParameterName = "@utCptCode",
                            SqlDbType =  System.Data.SqlDbType.Structured,
                            Direction = System.Data.ParameterDirection.Input,
                            TypeName = "utCptCode",
                            Value = dtCpt
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

        public List<SearchResultDTO> GetSearchData(SearchParametersDTO searchParametersDTO, string timeZoneCookie)
        {
            List<SearchResultDTO> lstDto = new List<SearchResultDTO>();
            StringBuilder parameterBuilder = new StringBuilder();
            using (var context = new UABContext())
            {
                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "[dbo].[USPGetSearchData]";
                    cmd.Connection = con;

                    searchParametersDTO.ProviderName = searchParametersDTO.ProviderName == "--Select a Provider--" ? null : searchParametersDTO.ProviderName;
                    searchParametersDTO.ProjectName = searchParametersDTO.ProjectName == "--Select a Project--" ? null : searchParametersDTO.ProjectName;
                    searchParametersDTO.StatusName = searchParametersDTO.StatusName == "--Select a Status--" ? null : searchParametersDTO.StatusName;
                    int Isblocked = searchParametersDTO.IncludeBlocked == true ? 1 : 0;

                    searchParametersDTO.DoSFrom = searchParametersDTO.DoSFrom == DateTime.Parse("1/1/0001 12:00:00 AM") ? null : searchParametersDTO.DoSFrom;
                    searchParametersDTO.DoSTo = searchParametersDTO.DoSTo == DateTime.Parse("1/1/0001 12:00:00 AM") ? null : searchParametersDTO.DoSTo;
                    var param = new SqlParameter[] {
                        new SqlParameter() {
                            ParameterName = "@mrn",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = searchParametersDTO.MRN
                        },   new SqlParameter() {
                            ParameterName = "@fname",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = searchParametersDTO.FirstName
                        },
                         new SqlParameter() {
                            ParameterName = "@lname",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = searchParametersDTO.LastName
                        },
                        new SqlParameter() {
                            ParameterName = "@projectname",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = searchParametersDTO.ProjectName
                        },   new SqlParameter() {
                            ParameterName = "@providername",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = searchParametersDTO.ProviderName
                        },
                        new SqlParameter()
                        {
                            ParameterName = "@DoSFrom",
                            SqlDbType = System.Data.SqlDbType.Date,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = searchParametersDTO.DoSFrom
                        },
                        new SqlParameter()
                        {
                            ParameterName = "@DoSTo",
                            SqlDbType = System.Data.SqlDbType.Date,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = searchParametersDTO.DoSTo
                        },
                        new SqlParameter()
                        {
                            ParameterName = "@StatusName",
                            SqlDbType = System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = searchParametersDTO.StatusName
                        },
                        new SqlParameter()
                        {
                            ParameterName = "@IsBlocked",
                            SqlDbType = System.Data.SqlDbType.Bit,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = Isblocked
                        },
                        new SqlParameter()
                        {
                            ParameterName = "@userId",
                            SqlDbType = System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = mUserId
                        }
                    };
                    cmd.Parameters.AddRange(param);
                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        SearchResultDTO dto = new SearchResultDTO()
                        {
                            ClinicalCaseId = Convert.ToString(reader["ClinicalCaseID"]),
                            ProjectId = Convert.ToInt32(reader["ProjectId"]),
                            FirstName = Convert.ToString(reader["FirstName"]),
                            LastName = Convert.ToString(reader["LastName"]),
                            MRN = Convert.ToString(reader["PatientMRN"]),
                            ProviderName = Convert.ToString(reader["Provider"]),
                            DoS = Convert.ToDateTime(reader["DateOfService"]),
                            ProjectName = Convert.ToString(reader["ProjectName"]),
                            Status = Convert.ToString(reader["Status"]),
                            IncludeBlocked = Convert.ToString(reader["IsBlocked"]),
                            Assigneduser = Convert.ToString(reader["AssignedTo"]),
                            CodedBy = Convert.ToString(reader["CodedByName"]),
                            QABy = Convert.ToString(reader["QAByName"]),
                            ShadowQABy = Convert.ToString(reader["ShadowQAByName"])
                        };
                        lstDto.Add(dto);
                    }
                    reader.NextResult();

                    while (reader.Read())
                    {
                        string CCID = Convert.ToString(reader["ClinicalCaseID"]);
                        var item = lstDto.Where(x => x.ClinicalCaseId == CCID).FirstOrDefault();
                        if (reader["PostedBy"] != DBNull.Value)
                            item.PostedBy = Convert.ToString(reader["PostedBy"]);
                        if (reader["PostingDate"] != DBNull.Value)
                            item.PostedDate = Convert.ToString(Convert.ToDateTime(reader["PostingDate"]).ToLocalDate(timeZoneCookie));

                        item.CPTDxInfo.Add(new CPTAndDxInfo
                        {
                            CPTCodes = Convert.ToString(reader["CPTCode"]),
                            DxCodes = Convert.ToString(reader["DxCode"]),
                            ClaimOrder = Convert.ToInt32(reader["ClaimOrder"]),
                        });
                    }
                    reader.NextResult();

                    List<ProjUser> lstProjectuser = new List<ProjUser>();

                    while (reader.Read())
                    {
                        lstProjectuser.Add(new ProjUser
                        {
                            ProjectId = Convert.ToInt32(reader["ProjectId"]),
                            RoleId = Convert.ToInt32(reader["RoleId"])
                        });
                    }
                    lstDto.ForEach(x => x.ProjectUser = lstProjectuser);
                }
            }
            return lstDto;
        }

        public string GetDxCodes(List<CPTAndDxInfo> cptAndDxInfo)
        {
            string DxCodes = null;
            foreach (var Dx in cptAndDxInfo)
            {
                if (cptAndDxInfo.Count > 1)
                {
                    DxCodes += "\n Claim " + Dx.ClaimOrder + ": ";
                }
                DxCodes += Dx.DxCodes + "\n ";
            }
            return DxCodes;
        }

        public string GetCptCodes(List<CPTAndDxInfo> cptAndDxInfo)
        {
            string CptCodes = null;
            foreach (var Dx in cptAndDxInfo)
            {
                if (cptAndDxInfo.Count > 1)
                {
                    CptCodes += "\n Claim " + Dx.ClaimOrder + ": ";
                }
                CptCodes += Dx.CPTCodes + "\n ";
            }
            return CptCodes;
        }

        public CodingDTO SubmitQARejectedChartsOfShadowQA(ChartSummaryDTO chartSummaryDTO, DataTable dtAudit, int statusId)
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
                            ParameterName = "@UserId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = mUserId
                        },
                       new SqlParameter() {
                            ParameterName = "@StatusID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = statusId
                        },
                          new SqlParameter() {
                            ParameterName = "@utWorkItemAudit1",
                            SqlDbType =  System.Data.SqlDbType.Structured,
                            Direction = System.Data.ParameterDirection.Input,
                            TypeName = "utWorkItemAudit1",
                            Value = dtAudit
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
                            Value = mUserId
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
        public CodingDTO SubmitShadowQARebuttalChartsOfQA(ChartSummaryDTO chartSummaryDTO, DataTable dtAudit, int statusId)
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
                            ParameterName = "@UserId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = mUserId
                         },
                       new SqlParameter() {
                            ParameterName = "@StatusID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = statusId
                        }
                        ,  new SqlParameter() {
                            ParameterName = "@utWorkItemAudit1",
                            SqlDbType =  System.Data.SqlDbType.Structured,
                            Direction = System.Data.ParameterDirection.Input,
                            TypeName = "utWorkItemAudit1",
                            Value = dtAudit
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


        public CodingDTO SubmitShadowQAAvailableChart(ChartSummaryDTO chartSummaryDTO, DataTable dtAudit)
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
                            ParameterName = "@UserId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = mUserId
                         },
                          new SqlParameter() {
                            ParameterName = "@utWorkItemAudit1",
                            SqlDbType =  System.Data.SqlDbType.Structured,
                            Direction = System.Data.ParameterDirection.Input,
                            TypeName = "utWorkItemAudit1",
                            Value = dtAudit
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



        public List<WorkflowHistoryDTO> GetWorkflowHistories(string ccid, string timeZoneCookie)
        {
            List<WorkflowHistoryDTO> lst = new List<WorkflowHistoryDTO>();
            using (var context = new UABContext())
            {
                using (var con = context.Database.GetDbConnection())
                {
                    var cmd = con.CreateCommand();
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "GetWorkFlowHistory";
                    cmd.Connection = con;

                    SqlParameter ccparam = new SqlParameter();
                    ccparam.ParameterName = "@ClinicalCaseID";
                    ccparam.Value = Convert.ToInt32(ccid);
                    cmd.Parameters.Add(ccparam);

                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        WorkflowHistoryDTO wf = new WorkflowHistoryDTO();
                        wf.Event = Convert.ToString(reader["Event"]);
                        wf.DateandTime = Convert.ToDateTime(reader["Date"]).ToLocalDate(timeZoneCookie);
                        wf.ByUser = Convert.ToString(reader["UserName"]);
                        wf.Remarks = Convert.ToString(reader["Remarks"]);
                        lst.Add(wf);
                    }
                }
                return lst;
            }
        }
        public List<BlockHistory> GetBlockHistories(string ccid)
        {
            using (var context = new UABContext())
            {
                return context.BlockHistory.Where(a => a.ClinicalCaseId == (Convert.ToInt32(ccid))).ToList();
            }
        }
        public int GetStatusId(string ccid)
        {
            using (var context = new UABContext())
            {
                return context.WorkItem.Where(a => a.ClinicalCaseId == Convert.ToInt32(ccid)).Select(a => a.StatusId).FirstOrDefault();
            }
        }
        public List<BlockCategory> GetBlockCategories()
        {
            using (var context = new UABContext())
            {
                return context.BlockCategory.ToList();
            }
        }
        public void BlockClinicalcase(string ccid, string bid, string remarks)
        {
            using (var context = new UABContext())
            {
                int statusid = context.WorkItem.Where(a => a.ClinicalCaseId == Convert.ToInt32(ccid)).Select(a => a.StatusId).FirstOrDefault();
                string BlockedInQueueKind = null;
                if (statusid == 2)
                {
                    BlockedInQueueKind = "Coding";
                }
                else if (statusid == 5)
                {
                    BlockedInQueueKind = "QA";
                }
                else if (statusid == 9)
                {
                    BlockedInQueueKind = "ShadowQA";
                }

                BlockHistory mdl = new BlockHistory()
                {
                    BlockCategoryId = Convert.ToInt32(bid),
                    BlockedByUserId = mUserId,
                    Remarks = remarks,
                    CreateDate = DateTime.UtcNow,
                    ClinicalCaseId = Convert.ToInt32(ccid),
                    BlockedInQueueKind = BlockedInQueueKind
                };
                context.BlockHistory.Add(mdl);//adding to blockhistory table

                var existingworkitem = context.WorkItem.Where(a => a.ClinicalCaseId == Convert.ToInt32(ccid)).FirstOrDefault();

                if (existingworkitem != null)
                {
                    existingworkitem.IsBlocked = 1;   //making Isblocked to 1 in workitem table
                    context.Entry(existingworkitem).State = EntityState.Modified;
                }
                context.SaveChanges();
            }
        }
        public void AssignClinicalcase(SearchResultDTO searchResultDTO)
        {
            int ccid = Convert.ToInt32(searchResultDTO.ClinicalCaseId);
            int AssignedTouserid = Convert.ToInt32(searchResultDTO.AssignToUserEmail);


            using (var context = new UABContext())
            {
                var existingcc = context.WorkItem.Where(c => c.ClinicalCaseId == ccid).FirstOrDefault();
                if (existingcc != null)
                {
                    Version vr1 = new Version();
                    int assignfromuser = 0;
                    string unamefrom = null;
                    string unameto = null;

                    if (existingcc.StatusId == 1 || existingcc.StatusId == 2 || existingcc.StatusId == 3
                        || existingcc.StatusId == 14 || existingcc.StatusId == 15)
                    {

                        //coding

                        assignfromuser = existingcc.AssignedTo == null ? 0 : Convert.ToInt32(existingcc.AssignedTo);
                        existingcc.AssignedTo = AssignedTouserid;

                        if (assignfromuser != 0)
                        {
                            var user = context.User.Where(x => x.UserId == assignfromuser).FirstOrDefault();
                            unamefrom = user.FirstName + " " + user.LastName;
                        }
                        else
                        {
                            unamefrom = "Un Assign";
                        }
                        if (AssignedTouserid != 0)
                        {
                            var user2 = context.User.Where(x => x.UserId == AssignedTouserid).FirstOrDefault();
                            unameto = user2.FirstName + " " + user2.LastName;
                        }

                        //version table assign  event
                        vr1.ClinicalCaseId = ccid;
                        vr1.StatusId = 18;
                        vr1.UserId = mUserId;
                        vr1.VersionDate = DateTime.UtcNow;
                        vr1.Remarks = "Re-Assigned the Coder from" + " " + unamefrom + " " + "to" + " " + unameto;
                    }
                    if (existingcc.StatusId == 4 || existingcc.StatusId == 5 || existingcc.StatusId == 6
                        || existingcc.StatusId == 11 || existingcc.StatusId == 12)
                    {
                        // QA
                        assignfromuser = existingcc.AssignedTo == null ? 0 : Convert.ToInt32(existingcc.QABy);
                        existingcc.QABy = AssignedTouserid;


                        if (assignfromuser != 0)
                        {
                            var user = context.User.Where(x => x.UserId == assignfromuser).FirstOrDefault();
                            unamefrom = user.FirstName + " " + user.LastName;
                        }
                        else
                        {
                            unamefrom = "Un Assign";
                        }
                        if (AssignedTouserid != 0)
                        {
                            var user2 = context.User.Where(x => x.UserId == AssignedTouserid).FirstOrDefault();
                            unameto = user2.FirstName + " " + user2.LastName;
                        }


                        //version table assign  event
                        vr1.ClinicalCaseId = ccid;
                        vr1.StatusId = 18;
                        vr1.UserId = mUserId;
                        vr1.VersionDate = DateTime.UtcNow;
                        vr1.Remarks = "Re-Assigned the QA from" + unamefrom + "to" + unameto;
                    }
                    if (existingcc.StatusId == 7 || existingcc.StatusId == 8 || existingcc.StatusId == 9
                         || existingcc.StatusId == 10 || existingcc.StatusId == 13)
                    {
                        // Shadow QA
                        assignfromuser = existingcc.AssignedTo == null ? 0 : Convert.ToInt32(existingcc.ShadowQABy);
                        existingcc.ShadowQABy = AssignedTouserid;


                        if (assignfromuser != 0)
                        {
                            var user = context.User.Where(x => x.UserId == assignfromuser).FirstOrDefault();
                            unamefrom = user.FirstName + " " + user.LastName;
                        }
                        else
                        {
                            unamefrom = "Un Assign";
                        }
                        if (AssignedTouserid != 0)
                        {
                            var user2 = context.User.Where(x => x.UserId == AssignedTouserid).FirstOrDefault();
                            unameto = user2.FirstName + " " + user2.LastName;
                        }


                        //version table assign  event
                        vr1.ClinicalCaseId = ccid;
                        vr1.StatusId = 18;
                        vr1.UserId = mUserId;
                        vr1.VersionDate = DateTime.UtcNow;
                        vr1.Remarks = "Re-Assigned the Shadow QA from" + unamefrom + "to" + unameto;

                    }
                    existingcc.AssignedBy = mUserId;
                    existingcc.AssignedDate = DateTime.UtcNow;
                    existingcc.IsPriority = searchResultDTO.IsPriority ? 1 : 0;
                    context.Entry(existingcc).State = EntityState.Modified;

                    context.Version.Add(vr1);
                    context.SaveChanges();
                }
            }
        }

        public List<ApplicationProject> GetProjectsList()
        {
            using (var context = new UABContext())
            {
                return GetProjects();
            }
        }
        public List<Status> GetStatusList()
        {
            using (var context = new UABContext())
            {
                return context.Status.Where(x => x.StatusId != 18 && x.StatusId != 19).ToList();
            }
        }
        public List<Role> GetRolesList()
        {
            using (var context = new UABContext())
            {
                return context.Role.ToList();
            }
        }
        public List<string> GetUabuserEmails()
        {
            using (var context = new UABContext())
            {
                return context.User.Select(a => a.Email).ToList();
            }
        }
        public List<string> GetIdentityUsersList()
        {
            using (UAB.DAL.LoginDTO.IdentityServerContext context = new IdentityServerContext())
            {
                List<string> iduseremail = context.Users.Select(a => a.Email).ToList();

                List<string> uabuseremail = GetUabuserEmails();

                return iduseremail.Except(uabuseremail).ToList();
            }
        }
        public List<ProviderFeedback> GetProviderFeedbacks()
        {
            List<ProviderFeedback> lstDto = new List<ProviderFeedback>();
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
                        ProviderFeedback dto = new ProviderFeedback()
                        {
                            ProviderFeedbackId = Convert.ToInt32(reader["ProviderFeedbackId"]),
                            Feedback = Convert.ToString(reader["Feedback"])
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

        public List<Location> GetLocations()
        {
            using (var context = new UABContext())
            {
                return context.Location.ToList();
            }
        }
        public List<Provider> GetProviders()
        {
            Provider provider = new Provider();
            List<Provider> lstProvider = new List<Provider>();

            using (var context = new UABContext())
            {

                return context.Provider.ToList();
            }
        }
        public List<User> GetManageUsers(int Projectid = 0)
        {
            using (var context = new UABContext())
            {
                List<int> userIds = context.ProjectUser.Where(x => (Projectid == 0 || x.ProjectId == Projectid)).Select(x => x.UserId).ToList();
                return context.User.Where(x => userIds.Contains(x.UserId)).ToList();
            }
        }
        public List<int> GetManageEMCodeLevels()
        {
            using (var context = new UABContext())
            {
                return context.EMCodeLevel.Select(x => x.EMLevel).Distinct().ToList();
            }
        }
        public List<EMLevelDTO> GetManageEMCLevelsByProjectId()
        {
            EMLevelDTO eMLevelDTO = new EMLevelDTO();
            List<EMLevelDTO> lsteMLevelDTO = new List<EMLevelDTO>();

            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspGetEMLevelsByProjectId]";
                    //var param = new SqlParameter()
                    //{
                    //    ParameterName = "@ProjectId",
                    //    SqlDbType = System.Data.SqlDbType.Int,
                    //    Direction = System.Data.ParameterDirection.Input,
                    //    Value = projectid
                    //};
                    //cmm.Parameters.Add(param);
                    cmm.Connection = cnn;
                    cnn.Open();
                    var reader = cmm.ExecuteReader();

                    while (reader.Read())
                    {
                        eMLevelDTO = new EMLevelDTO();
                        eMLevelDTO.EMLevelId = Convert.ToInt32(reader["Id"]);
                        eMLevelDTO.EMLevel = Convert.ToInt32(reader["Level"]);
                        eMLevelDTO.ProjectName = Convert.ToString(reader["Name"]);
                        lsteMLevelDTO.Add(eMLevelDTO);
                    }
                }
            }
            return lsteMLevelDTO;
        }

        public List<EMLevelDTO> GetEMCodeLevelsbyId(int ProjectId)
        {
            EMLevelDTO eMLevelDTO = new EMLevelDTO();
            List<EMLevelDTO> lsteMLevelDTO = new List<EMLevelDTO>();

            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspGetEMCodeLevelsbyId]";
                    var param = new SqlParameter()
                    {
                        ParameterName = "@ProjectId",
                        SqlDbType = System.Data.SqlDbType.Int,
                        Direction = System.Data.ParameterDirection.Input,
                        Value = ProjectId
                    };
                    cmm.Parameters.Add(param);
                    cmm.Connection = cnn;
                    cnn.Open();
                    var reader = cmm.ExecuteReader();

                    while (reader.Read())
                    {
                        eMLevelDTO = new EMLevelDTO();
                        eMLevelDTO.EMLevelId = Convert.ToInt32(reader["Id"]);
                        eMLevelDTO.EMLevel = Convert.ToInt32(reader["Level"]);
                        eMLevelDTO.ProjectName = Convert.ToString(reader["Name"]);
                        lsteMLevelDTO.Add(eMLevelDTO);
                    }
                }
            }
            return lsteMLevelDTO;
        }

        public List<EMCodeLevel> GetEMCodeLevelDetails(int eMLevelId)
        {
            using (var context = new UABContext())
            {
                return context.EMCodeLevel.Where(x => x.EMLevel == eMLevelId).ToList();
            }
        }

        //public List<EMLevel> GetManageEMCLevelsByProjectId (int projectid)
        //{
        //    using (var context = new UABContext())
        //    {
        //        return context.EMLevel.Where(x=>x.ProjectId==projectid).ToList();
        //    }
        //}
        //public List<EMCodeLevel> GetEMCodeLevelDetails(int eMLevel)
        //{
        //    using (var context = new UABContext())
        //    {
        //        return context.EMCodeLevel.Where(x => x.EMLevel == eMLevel).ToList();
        //    }
        //}
        public EMCodeLevel GetEMCodeById(int Id)
        {
            using (var context = new UABContext())
            {
                return context.EMCodeLevel.Where(x => x.Id == Id).FirstOrDefault();
            }
        }
        public void UpdateEMCode(EMCodeLevel eMCodeLevel)
        {
            using (var context = new UABContext())
            {
                var emlevellst = context.EMCodeLevel.Where(a => a.EMLevel == eMCodeLevel.EMLevel).ToList();
                var otherexistingemlevelcode = emlevellst.Where(a => a.EMLevel == eMCodeLevel.EMLevel && a.EMCode == eMCodeLevel.EMCode).FirstOrDefault();
                var existingcode = emlevellst.Where(a => a.Id == eMCodeLevel.Id).FirstOrDefault();
                if (existingcode != null && otherexistingemlevelcode == null)
                {
                    existingcode.EMCode = eMCodeLevel.EMCode;
                    context.Entry(existingcode).State = EntityState.Modified;
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable To update EM Code : This EM Code exist  in EM Level");
                }
            }
        }
        public void AddEMCode(EMCodeLevel eMCodeLevel)
        {
            using (var context = new UABContext())
            {
                var isexistingcode = context.EMCodeLevel.Where(a => a.EMCode == eMCodeLevel.EMCode && a.EMLevel == eMCodeLevel.Id).FirstOrDefault();
                EMCodeLevel emc = new EMCodeLevel()
                {
                    EMCode = eMCodeLevel.EMCode,
                    EMLevel = eMCodeLevel.Id
                };
                if (isexistingcode == null)
                {
                    context.EMCodeLevel.Add(emc);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable To Add EM Code : THis EM Code Alreday There  in EM Level");
                }
            }
        }
        public void DeletetEMCode(EMCodeLevel eMCodeLevel)
        {
            using (var context = new UABContext())
            {
                var exsitingCode = context.EMCodeLevel.Where(a => a.Id == eMCodeLevel.Id).FirstOrDefault();

                if (exsitingCode != null)
                {
                    context.EMCodeLevel.Remove(exsitingCode);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable To Delete EM Code : EM Code Not there in UAB");
                }

            }
        }
        public void DeletetEMCode(int eMLevel)
        {
            using (var context = new UABContext())
            {
                var exsitingEMCodeLevel = context.EMCodeLevel.Where(a => a.EMCode == eMLevel.ToString()).ToList();

                if (exsitingEMCodeLevel.Count != 0)
                {
                    context.EMCodeLevel.RemoveRange(exsitingEMCodeLevel);
                    context.SaveChanges();
                }
                if (exsitingEMCodeLevel.Count == 0)
                {
                    var existingEMLevel = context.EMLevel.Where(el => el.Level == eMLevel.ToString()).FirstOrDefault();
                    if (existingEMLevel != null)
                    {
                        context.EMLevel.RemoveRange(existingEMLevel);
                        context.SaveChanges();
                    }
                    else
                    {
                        throw new Exception("Unable To Delete EM Level : EM Level Not there in UAB");
                    }
                }

            }
        }
        public void AddEMLevel(EMLevelDTO eMCodeLevel)
        {
            using (var context = new UABContext())
            {
                var isexisting = context.EMLevel.Where(x => x.Level == eMCodeLevel.EMLevel.ToString() && x.ProjectId == eMCodeLevel.ProjectId).FirstOrDefault();

                EMLevel emc = new EMLevel()
                {
                    Level = eMCodeLevel.EMLevel.ToString(),
                    ProjectId = eMCodeLevel.ProjectId
                };
                if (isexisting == null)
                {
                    context.EMLevel.Add(emc);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable To Add EM Level or Code : THis EM Level Alreday There  in EM Level");
                }
            }
        }
        public List<User> GetAssignedToUsers(string ccid)
        {
            using (var context = new UABContext())
            {
                string fromemial = null;
                var workitem = context.WorkItem.Where(a => a.ClinicalCaseId == Convert.ToInt32(ccid)).FirstOrDefault();
                int roleid = 1;
                if (workitem.StatusId == 4 || workitem.StatusId == 5 || workitem.StatusId == 6
                       || workitem.StatusId == 11 || workitem.StatusId == 12)
                {
                    roleid = 2;
                }
                if (workitem.StatusId == 7 || workitem.StatusId == 8 || workitem.StatusId == 9
                        || workitem.StatusId == 10 || workitem.StatusId == 13)
                {
                    roleid = 3;
                }
                if (workitem.AssignedTo != null)
                {
                    var user = context.User.Where(a => a.UserId == workitem.AssignedTo).FirstOrDefault();
                    fromemial = user.Email;
                }
                if (fromemial == null)
                {
                    return context.User.ToList();
                }
                else
                {
                    List<User> userlist = context.User.Where(a => !a.Email.Contains(fromemial)).ToList();
                    var projectuserids = context.ProjectUser.Where(x => x.ProjectId == workitem.ProjectId && x.RoleId == roleid && x.UserId != workitem.AssignedTo).Select(x => x.UserId).Distinct().ToList();

                    List<User> userlist3 = new List<User>();

                    foreach (var userid in projectuserids)
                    {
                        var re = userlist.Where(x => x.UserId == userid).FirstOrDefault();
                        if (re != null)
                            userlist3.Add(re);
                    }
                    return userlist3;
                }

            }
        }
        public string GetAssignedusername(string ccid)
        {
            using (var context = new UABContext())
            {
                string username = null;
                var workitem = context.WorkItem.Where(a => a.ClinicalCaseId == Convert.ToInt32(ccid)).FirstOrDefault();
                if (workitem.AssignedTo != null)
                {
                    var user = context.User.Where(a => a.UserId == workitem.AssignedTo).FirstOrDefault();
                    username = user.FirstName + user.LastName;
                }
                else
                {
                    username = "Un assigned";
                }
                return username;
            }
        }
        public ApplicationUser GetProjectUser(int projectuserid)
        {
            using (var context = new UABContext())
            {
                var res = context.ProjectUser.Where(a => a.ProjectUserId == projectuserid).FirstOrDefault();

                var project = context.Project.Where(a => a.ProjectId == res.ProjectId).FirstOrDefault();
                var roles = context.Role.Where(a => a.RoleId == res.RoleId).FirstOrDefault();
                var useremail = context.User.Where(a => a.UserId == res.UserId).FirstOrDefault().Email;
                ApplicationUser applicationUser = new ApplicationUser();
                applicationUser.UserId = res.UserId;
                applicationUser.Email = useremail;
                applicationUser.ProjectId = res.ProjectId;
                applicationUser.ProjectName = project.Name;
                applicationUser.RoleId = res.RoleId;
                applicationUser.RoleName = roles.Name;
                applicationUser.SamplePercentage = res.SamplePercentage.ToString();
                return applicationUser;
            }

        }
        public List<ApplicationUser> GetUserProjects(int userId)
        {
            ApplicationUser applicationUser = new ApplicationUser();
            List<ApplicationUser> lstApplicationUser = new List<ApplicationUser>();

            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspGetUser]";
                    cmm.Connection = cnn;

                    SqlParameter param = new SqlParameter();
                    param.ParameterName = "@userId";
                    param.Value = userId;
                    cmm.Parameters.Add(param);

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
                        applicationUser.SamplePercentage = Convert.ToString(reader["SamplePercentage"]);
                        lstApplicationUser.Add(applicationUser);
                    }

                    if (lstApplicationUser.Count != 0)
                    {
                        string temp = null;
                        foreach (var item in lstApplicationUser)
                        {
                            temp = temp + item.ProjectName + "^" + item.RoleName + ",";
                        }
                        var length = temp.Length;
                        string initial = temp.Substring(0, length - 1);
                        applicationUser = lstApplicationUser.FirstOrDefault();
                        applicationUser.hdnProjectAndRole = initial;
                        lstApplicationUser[0] = applicationUser;
                    }
                }
            }
            if (lstApplicationUser.Count == 0)
            {
                using (var context = new UABContext())
                {
                    var user = context.User.Where(a => a.UserId == userId).FirstOrDefault();
                    applicationUser = new ApplicationUser();
                    applicationUser.Email = user.Email;
                    applicationUser.UserId = user.UserId;
                    applicationUser.RoleId = 0;
                    applicationUser.ProjectId = 0;
                    applicationUser.RoleName = "";
                    applicationUser.ProjectName = "";
                    lstApplicationUser.Add(applicationUser);
                }
            }
            return lstApplicationUser;
        }

        public User Getuser(int UserId)
        {
            using (var context = new UABContext())
            {
                var user = context.User.Where(a => a.UserId == UserId).FirstOrDefault();
                User mdl = new User()
                {
                    Email = user.Email,
                    UserId = UserId,
                    IsActive = user.IsActive
                };
                return mdl;
            }
        }
        public void ChangeStatus(int UserId, bool IsActive)
        {
            using (var context = new UABContext())
            {
                var user = context.User.Where(a => a.UserId == UserId).FirstOrDefault();
                user.IsActive = IsActive;
                context.SaveChanges();
            }
        }

        public int AddUser(ApplicationUser user)
        {
            using (var context = new UABContext())
            {
                //UAB.DAL.LoginDTO.IdentityServerContext Icontext = new IdentityServerContext();

                //var iuser = Icontext.Users.Where(a => a.Email == user.Email).FirstOrDefault();
                var existing = context.User.Where(a => a.Email == user.Email).FirstOrDefault();

                if (existing == null)
                {
                    UAB.DAL.Models.User mdl = new User();
                    mdl.Email = user.Email;
                    mdl.FirstName = user.FirstName;
                    mdl.LastName = user.LastName;
                    mdl.IsActive = user.IsActive;

                    context.User.Add(mdl);
                    context.SaveChanges();
                    return mdl.UserId;
                }
                return existing.UserId;
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
                mdl.SamplePercentage = Convert.ToInt32(user.SamplePercentage);

                var exsitingprojectuser = context.ProjectUser.Where(a => a.UserId == user.UserId && a.RoleId == mdl.RoleId && a.ProjectId == mdl.ProjectId).FirstOrDefault();
                if (exsitingprojectuser == null)
                {
                    context.ProjectUser.Add(mdl);
                    context.SaveChanges();
                }
                else
                {
                    throw new ArgumentException("Unable to Add project User : trying to add existing project to this user");
                }
            }
        }

        public void UpdateProjectUser(ApplicationUser projectuser)
        {
            using (var context = new UABContext())
            {
                var userinfo = context.ProjectUser.Where(x => x.UserId == projectuser.UserId).ToList();
                var existingprojectuser = userinfo.Where(x => x.ProjectUserId == projectuser.ProjectUserId).FirstOrDefault();
                var exstingroles = userinfo.Where(x => x.UserId == projectuser.UserId).Select(x => x.RoleId).ToList();

                if (existingprojectuser.RoleId == projectuser.RoleId && existingprojectuser.SamplePercentage == Convert.ToInt32(projectuser.SamplePercentage))
                    throw new Exception("Unable to Update Project User : No changes were detected");

                if (existingprojectuser.RoleId == 1 && projectuser.RoleId != 1)
                {
                    var exsitingworkitem = context.WorkItem.Where(x => x.ProjectId == existingprojectuser.ProjectId &&
                  (x.AssignedTo == projectuser.UserId)).FirstOrDefault();

                    if (exsitingworkitem != null)
                        throw new Exception("Unable to Update Project User : User have Assigned charts");
                }
                else if (existingprojectuser.RoleId == 2 && projectuser.RoleId != 2)
                {
                    var exsitingworkitem = context.WorkItem.Where(x => x.ProjectId == existingprojectuser.ProjectId &&
                  (x.QABy == projectuser.UserId)).FirstOrDefault();

                    if (exsitingworkitem != null)
                        throw new Exception("Unable to Update Project User : User have Assigned charts");
                }
                else if (existingprojectuser.RoleId == 3 && projectuser.RoleId != 3)
                {
                    var exsitingworkitem = context.WorkItem.Where(x => x.ProjectId == existingprojectuser.ProjectId &&
                  (x.ShadowQABy == projectuser.UserId)).FirstOrDefault();

                    if (exsitingworkitem != null)
                        throw new Exception("Unable to Update Project User : User have Assigned charts");
                }


                //updating role only 
                if (!exstingroles.Contains(projectuser.RoleId) && existingprojectuser.SamplePercentage == Convert.ToInt32(projectuser.SamplePercentage))
                {

                    if (projectuser.RoleId == 1 || projectuser.RoleId == 2)
                    {
                        existingprojectuser.RoleId = projectuser.RoleId;
                        context.Entry(existingprojectuser).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                    else
                    {
                        existingprojectuser.RoleId = projectuser.RoleId;
                        existingprojectuser.SamplePercentage = 0;
                        context.Entry(existingprojectuser).State = EntityState.Modified;
                        context.SaveChanges();
                    }
                }
                //updating sample % only
                else if (existingprojectuser.RoleId == projectuser.RoleId && existingprojectuser.SamplePercentage != Convert.ToInt32(projectuser.SamplePercentage))
                {
                    existingprojectuser.SamplePercentage = Convert.ToInt32(projectuser.SamplePercentage);
                    context.Entry(existingprojectuser).State = EntityState.Modified;
                    context.SaveChanges();
                }
                //updating role and sample % both
                else if (!exstingroles.Contains(projectuser.RoleId) && existingprojectuser.SamplePercentage != Convert.ToInt32(projectuser.SamplePercentage))
                {
                    existingprojectuser.RoleId = projectuser.RoleId;
                    existingprojectuser.SamplePercentage = Convert.ToInt32(projectuser.SamplePercentage);
                    context.Entry(existingprojectuser).State = EntityState.Modified;
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable to Update Project User : This user has already this role");
                }
            }
        }

        public void DeletetProjectUser(int ProjectUserId)
        {
            using (var context = new UABContext())
            {
                var exsitingProjectuser = context.ProjectUser.Where(a => a.ProjectUserId == ProjectUserId).FirstOrDefault();
                if (exsitingProjectuser != null)
                {
                    var exsitingworkitem = context.WorkItem.Where(x => x.ProjectId == exsitingProjectuser.ProjectId &&
                    (x.AssignedTo == exsitingProjectuser.UserId || x.QABy == exsitingProjectuser.UserId || x.ShadowQABy == exsitingProjectuser.UserId)).FirstOrDefault();

                    if (exsitingworkitem == null)
                    {
                        context.ProjectUser.Remove(exsitingProjectuser);
                        context.SaveChanges();
                    }
                    else
                    {
                        throw new Exception("Unable To Delete Project User : User have Assigned charts");
                    }
                }
                else
                {
                    throw new Exception("Unable To Delete Project User : Project User not there in UAB");
                }
            }
        }
        public void DeletetUser(int UserId)
        {
            using (var context = new UABContext())
            {
                var exsitinguser = context.User.Where(a => a.UserId == UserId).FirstOrDefault();
                var exsitingProjectuser = context.ProjectUser.Where(a => a.UserId == UserId).FirstOrDefault();

                if (exsitinguser != null && exsitingProjectuser == null)
                {
                    context.User.Remove(exsitinguser);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable to delete User : It is already used in UAB,the associated information should delete first");
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

        public Provider GetProviderByName(string providerName)
        {
            using (var context = new UABContext())
            {
                return context.Provider.Where(p => p.Name == providerName).Select(p => p).FirstOrDefault();
            };
        }

        public Payor GetPayorByName(string payorName)
        {
            using (var context = new UABContext())
            {
                return context.Payor.Where(p => p.Name == payorName).Select(p => p).FirstOrDefault();
            };
        }

        public ProviderFeedback GetProviderFeedbackByName(string providerFeedbackName)
        {
            using (var context = new UABContext())
            {
                return context.ProviderFeedback.Where(p => p.Feedback == providerFeedbackName).Select(p => p).FirstOrDefault();
            };
        }

        public ErrorType GetErrorTypeByName(string errorTypeName)
        {
            using (var context = new UABContext())
            {
                return context.ErrorType.Where(p => p.Name == errorTypeName).Select(p => p).FirstOrDefault();
            };
        }

        public Project GetProjectByName(string projectName)
        {
            using (var context = new UABContext())
            {
                return context.Project.Where(p => p.Name == projectName).Select(p => p).FirstOrDefault();
            };
        }

        public int GetFirstClintId()
        {
            using (var context = new UABContext())
            {
                return context.Project.Select(p => p.ClientId).FirstOrDefault();
            };
        }

        public int GetFirstProjectTypeId()
        {
            using (var context = new UABContext())
            {
                return context.Project.Select(p => p.ProjectTypeId).FirstOrDefault();
            };
        }


        public void AddBlockCategory(BlockCategory blockCategory)
        {
            using (var context = new UABContext())
            {
                var isexistingcategory = context.BlockCategory.Where(a => a.Name == blockCategory.Name).FirstOrDefault();

                if (isexistingcategory == null)
                {
                    context.BlockCategory.Add(blockCategory);
                    context.SaveChanges();
                }
            }
        }

        public void UpdateBlockCategory(BlockCategory blockCategory)
        {
            using (var context = new UABContext())
            {
                var existingcategory = context.BlockCategory.Where(a => a.BlockCategoryId == blockCategory.BlockCategoryId).FirstOrDefault();

                if (existingcategory != null)
                {
                    existingcategory.Name = blockCategory.Name;
                    existingcategory.BlockType = blockCategory.BlockType;
                    context.Entry(existingcategory).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }
        }
        public void DeletetBlockCategory(int id)
        {
            using (var context = new UABContext())
            {
                var existingcategory = context.BlockCategory.Where(a => a.BlockCategoryId == id).FirstOrDefault();
                var existingcategoryhistory = context.BlockHistory.Where(x => x.BlockCategoryId == id).FirstOrDefault();
                if (existingcategory != null && existingcategoryhistory == null)
                {
                    context.BlockCategory.Remove(existingcategory);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable to delete Block Category : It is already used in UAB,the associated information should delete first");
                }

            }
        }

        public void AddLocation(Location location)
        {
            using (var context = new UABContext())
            {
                var isexistingLocation = context.Location.Where(a => a.Name == location.Name).FirstOrDefault();

                if (isexistingLocation == null)
                {
                    context.Location.Add(location);
                    context.SaveChanges();
                }
            }
        }
        public void UpdateLocation(Location location)
        {
            using (var context = new UABContext())
            {
                var existingLocation = context.Location.Where(a => a.LocationId == location.LocationId).FirstOrDefault();
                if (existingLocation != null)
                {
                    existingLocation.Name = location.Name;
                    context.Entry(existingLocation).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }
        }
        public void DeletetLocation(int id)
        {
            using (var context = new UABContext())
            {
                var existingLocation = context.Location.Where(a => a.LocationId == id).FirstOrDefault();
                if (existingLocation != null)
                {
                    context.Location.Remove(existingLocation);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable to delete location : It is not there in UAB");
                }
            }
        }
        public List<List> GetLists()
        {
            using (var context = new UABContext())
            {
                return context.List.Where(x => x.ListId != 0).ToList();
            }
        }
        public void AddListname(List list)
        {
            using (var context = new UABContext())
            {
                var isexistingList = context.List.Where(a => a.Name == list.Name && a.ListId == list.ListId).FirstOrDefault();

                if (isexistingList == null)
                {
                    context.List.Add(list);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable to add List Name : It is already exist in UAB");
                }
            }
        }
        public void UpdateListname(List list)
        {
            using (var context = new UABContext())
            {
                var existingList = context.List.Where(a => a.ListId == list.ListId).FirstOrDefault();
                if (existingList != null)
                {
                    existingList.Name = list.Name;
                    context.Entry(existingList).State = EntityState.Modified;
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable to Update List Name : It is already exist in UAB");
                }
            }
        }
        public void DeletetListname(long id)
        {
            using (var context = new UABContext())
            {
                var existingList = context.List.Where(a => a.ListId == id).FirstOrDefault();
                if (existingList != null)
                {
                    context.List.Remove(existingList);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable to delete list name : It is not there in UAB");
                }
            }
        }
        public void AddProvider(Provider provider)
        {
            using (var context = new UABContext())
            {

                context.Provider.Add(provider);
                context.SaveChanges();
            }
        }

        public void UpdateProvider(Provider provider)
        {
            using (var context = new UABContext())
            {
                var isexisit = context.Provider.Where(x => x.ProviderId == provider.ProviderId).FirstOrDefault();
                if (isexisit != null)
                {
                    isexisit.Name = provider.Name;
                    isexisit.IsAuditNeeded = provider.IsAuditNeeded;
                    context.Entry(isexisit).State = EntityState.Modified;
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable to Update Provider : Provider Id is Invalid");
                }
            }
        }

        public void DeleteProvider(Provider provider)
        {
            using (var context = new UABContext())
            {
                var existingprovider = context.Provider.Where(a => a.ProviderId == provider.ProviderId).FirstOrDefault();
                var existingproviderfromCC = context.ClinicalCase.Where(x => x.ProviderId == provider.ProviderId).FirstOrDefault();
                var existingproviderfromWP = context.WorkItemProvider.Where(x => x.ProviderId == provider.ProviderId).FirstOrDefault();
                if (existingprovider != null && existingproviderfromCC == null && existingproviderfromWP == null)
                {
                    context.Provider.Remove(existingprovider);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable to delete Provider : It is already used in UAB,the associated information should delete first");
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
                    feedback.Value = providerFeedback.Feedback.Trim();
                    cmm.Parameters.Add(feedback);

                    cnn.Open();
                    cmm.ExecuteNonQuery();
                }
            }
        }

        public void UpdateProviderFeedback(ProviderFeedback providerFeedback)
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
                    param1.Value = providerFeedback.Feedback.Trim();
                    SqlParameter param2 = new SqlParameter();
                    param2.ParameterName = "@ProviderFeedbackID";
                    param2.Value = providerFeedback.ProviderFeedbackId;
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
                var existingProviderFeedback = context.ProviderFeedback.Where(a => a.ProviderFeedbackId == providerFeedback.ID).FirstOrDefault();

                var existingProviderFeedbackfromWP = context.WorkItemProvider
                    .Where(s => s.ProviderFeedbackId.StartsWith(providerFeedback.ID.ToString() + ",") || s.ProviderFeedbackId.Contains("," + providerFeedback.ID.ToString() + ",") || s.ProviderFeedbackId.EndsWith("," + providerFeedback.ID.ToString()) || s.ProviderFeedbackId == providerFeedback.ID.ToString()).FirstOrDefault();

                if (existingProviderFeedback != null && existingProviderFeedbackfromWP == null)
                {
                    context.ProviderFeedback.Remove(existingProviderFeedback);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable to delete Providerfeedback : It is already used in UAB,the associated information should delete first");
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
                var existingpayor = context.Payor.Where(a => a.PayorId == payor.PayorId).FirstOrDefault();
                var existingpayorfromWP = context.WorkItemProvider.Where(x => x.PayorId == payor.PayorId).FirstOrDefault();

                if (existingpayor != null && existingpayorfromWP == null)
                {
                    context.Payor.Remove(existingpayor);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable to delete Payor : It is already used in UAB,the associated information should delete first");
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
                    name.Value = errorType.Name.Trim();
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
                var existingErrorType = context.ErrorType.Where(a => a.ErrorTypeId == errorType.ErrorTypeId).FirstOrDefault();
                var existingErrorTypefromWA = context.WorkItemAudit
                   .Where(s => s.ErrorTypeId.StartsWith(errorType.ErrorTypeId.ToString() + ",") || s.ErrorTypeId.Contains("," + errorType.ErrorTypeId.ToString() + ",") || s.ErrorTypeId.EndsWith("," + errorType.ErrorTypeId.ToString()) || s.ErrorTypeId == errorType.ErrorTypeId.ToString()).FirstOrDefault();


                if (existingErrorType != null && existingErrorTypefromWA == null)
                {
                    context.ErrorType.Remove(existingErrorType);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable to delete Error Type : It is already used in UAB,the associated information should delete first");
                }
            }
        }
        public string projectname(int pid)
        {
            using (var context = new UABContext())
            {
                return context.Project.Where(x => x.ProjectId == pid).Select(x => x.Name).FirstOrDefault();
            }
        }
        public int CheckTpicProjectId(int tpicprojectid)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json");

            var configuration = builder.Build();
            _conStr = configuration.GetSection("ConnectionString").GetSection("DataConnectiontoTpic").Value;


            using (var cnn = new SqlConnection(_conStr))
            {
                int id = 0;
                var cmm = cnn.CreateCommand();
                cmm.CommandType = System.Data.CommandType.Text;
                SqlParameter param1 = new SqlParameter();
                param1.ParameterName = "@Id";
                param1.Value = tpicprojectid;
                cmm.CommandText = "select 1 from Project where QuickbooksClientName = 'UAB Health System' AND Id =" + tpicprojectid + "";
                cmm.Connection = cnn;
                cnn.Open();
                var reader = cmm.ExecuteReader();

                while (reader.Read())
                {
                    id = Convert.ToInt32(reader.GetInt32(0));
                }
                return id;
            }
        }
        public List<CptAudit> GetCptAudits()
        {
            using (var context = new UABContext())
            {
                return context.CptAudit.ToList();
            }
        }

        public List<ApplicationProject> GetProjects()
        {
            ApplicationProject project = new ApplicationProject();
            List<ApplicationProject> lstProject = new List<ApplicationProject>();

            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspGetProject]";
                    //cmm.Parameters.AddRange(param);
                    cmm.Connection = cnn;
                    cnn.Open();
                    var reader = cmm.ExecuteReader();

                    while (reader.Read())
                    {
                        project = new ApplicationProject();
                        project.ProjectId = Convert.ToInt32(reader["ProjectId"]);
                        project.Name = Convert.ToString(reader["ProjectName"]);
                        project.IsActive = Convert.ToBoolean(reader["ActiveProject"]);
                        if (reader["CreatedDate"] != DBNull.Value)
                            project.CreatedDate = Convert.ToDateTime(reader["CreatedDate"]);
                        project.InputFileLocation = Convert.ToString(reader["InputFileLocation"]);
                        project.InputFileFormat = Convert.ToString(reader["InputFileFormat"]);
                        project.ClientId = Convert.ToInt32(reader["ClientId"]);
                        project.ClientName = Convert.ToString(reader["ClientName"]);
                        project.ProjectTypeId = Convert.ToInt32(reader["ProjectTypeId"]);
                        project.ProjectTypeName = Convert.ToString(reader["ProjectTypeName"]);
                        project.SLAInDays = Convert.ToInt32(reader["SLAInDays"]);
                        project.TpicProjectId = Convert.ToInt32(reader["TPICProjectId"]);

                        lstProject.Add(project);

                    }
                }

            }
            return lstProject;
        }
        public void AddCptAudit(CptAudit cptAudit)
        {
            using (var context = new UABContext())
            {
                var exisit = context.CptAudit.Where(x => x.CPTCode == cptAudit.CPTCode && x.ProjectId == cptAudit.ProjectId).FirstOrDefault();
                if (exisit == null)
                {
                    context.CptAudit.Add(cptAudit);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable to Add CPT Audit Code : CPT Audit Code Already there");

                }
            }
        }
        public void UpdateCptAudit(CptAudit cptAudit)
        {
            using (var context = new UABContext())
            {
                var existingcptcode = context.CptAudit.Where(a => a.CPTAuditId == cptAudit.CPTAuditId).FirstOrDefault();

                if (existingcptcode != null)
                {
                    existingcptcode.CPTCode = cptAudit.CPTCode;
                    existingcptcode.ProjectId = cptAudit.ProjectId;
                    existingcptcode.IsActive = cptAudit.IsActive;

                    context.Entry(existingcptcode).State = EntityState.Modified;
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable to Update CPT Audit Code : CPT audit code not there");

                }
            }
        }
        public void DeleteCptAudit(CptAudit cptAudit)
        {
            using (var context = new UABContext())
            {
                var existingcptcode = context.CptAudit.Where(a => a.CPTAuditId == cptAudit.CPTAuditId).FirstOrDefault();
                if (existingcptcode != null)
                {
                    context.CptAudit.Remove(existingcptcode);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable to delete CPT Code : this CPT  code not there");
                }
            }
        }
        public void AddProject(ApplicationProject project)
        {
            using (var context = new UABContext())
            {
                int isexisit = CheckTpicProjectId(project.TpicProjectId);
                if (isexisit != 0)
                {
                    UAB.DAL.Models.Project mdl = new Project();
                    mdl.ClientId = project.ClientId;
                    mdl.Name = project.Name;
                    mdl.IsActive = project.IsActive;
                    mdl.CreatedDate = DateTime.UtcNow;
                    mdl.InputFileLocation = project.InputFileLocation;
                    mdl.InputFileFormat = project.InputFileFormat;
                    mdl.ProjectTypeId = project.ProjectTypeId;
                    mdl.SLAInDays = project.SLAInDays;
                    mdl.TPICProjectId = project.TpicProjectId;

                    context.Project.Add(mdl);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable to Add Project : TPIC Project Id is Invalid");
                }
            }
        }

        public void UpdateProject(ApplicationProject project)
        {
            using (var context = new UABContext())
            {
                int isexisit = CheckTpicProjectId(project.TpicProjectId);
                if (isexisit != 0)
                {
                    UAB.DAL.Models.Project mdl = new Project();

                    mdl = context.Project.Where(x => x.ProjectId == project.ProjectId).FirstOrDefault();

                    //mdl.ProjectId = project.ProjectId;
                    mdl.ClientId = project.ClientId;
                    mdl.Name = project.Name;
                    mdl.IsActive = project.IsActive;
                    mdl.InputFileLocation = project.InputFileLocation;
                    mdl.InputFileFormat = project.InputFileFormat;
                    mdl.ProjectTypeId = project.ProjectTypeId;
                    mdl.SLAInDays = project.SLAInDays;
                    mdl.TPICProjectId = project.TpicProjectId;

                    context.Entry(mdl).State = EntityState.Modified;
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable to Update Project : TPIC Project Id is Invalid");
                }
            }
        }

        public void DeleteProject(ApplicationProject project)
        {
            using (var context = new UABContext())
            {
                var existingProject = context.Project.Where(a => a.ProjectId == project.ProjectId).FirstOrDefault();
                var existingProjectCC = context.ClinicalCase.Where(x => x.ProjectId == project.ProjectId).FirstOrDefault();
                var existingProjectPU = context.ProjectUser.Where(x => x.ProjectId == project.ProjectId).FirstOrDefault();


                if (existingProject != null && existingProjectCC == null && existingProjectPU == null)
                {
                    context.Project.Remove(existingProject);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable to delete Project : It is already used in UAB,the associated information should delete first");
                }
            }
        }

        public List<Client> GetClientList()
        {
            using (UAB.DAL.Models.UABContext context = new UABContext())
            {
                return context.Client.ToList();
            }
        }

        public List<ProjectType> GetProjectTypeList()
        {
            using (UAB.DAL.Models.UABContext context = new UABContext())
            {
                return context.ProjectType.ToList();
            }
        }
        public int GetSamplingPercentage(int userId, string role, int projetId)
        {
            int roleId = 1;
            if (role == "Coding")
                roleId = 1;
            else if (role == "QA")
                roleId = 2;

            using (UABContext context = new UABContext())
            {
                return context.ProjectUser
                    .Where(x => x.UserId == userId && x.RoleId == roleId && x.ProjectId == projetId)
                    .Select(x => x.SamplePercentage)
                    .FirstOrDefault();
            }
        }

        public List<string> GetProjectNames()
        {
            Project project = new Project();
            List<Project> lstProject = new List<Project>();
            List<string> providers = new List<string>();

            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspGetProject]";
                    cmm.Connection = cnn;
                    cnn.Open();
                    var reader = cmm.ExecuteReader();

                    while (reader.Read())
                    {
                        project = new Project();
                        project.ProjectId = Convert.ToInt32(reader["ProjectID"]);
                        project.Name = Convert.ToString(reader["ProjectName"]);
                        lstProject.Add(project);
                        providers.Add(project.Name.ToLower());
                    }
                }
            }
            return providers;
        }

        public void DateValueMustNotBeLessThanCertainDate(ref DateTime weekStartDate, ref DateTime weekEndDate)
        {
            if (weekStartDate < new DateTime(1753, 01, 01))
            {
                weekStartDate = DateTime.Now;
            }
            if (weekEndDate < new DateTime(1753, 01, 01))
            {
                weekEndDate = DateTime.Now;
            }
        }
    }
}