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

namespace UAB.DAL
{
    public class ClinicalcaseOperations
    {
        int mUserId;
        string mUserRole;
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

        public List<ChartSummaryDTO> GetBlockNext(string Role, string ChartType, int projectID)
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
                        chartSummaryDTO.CodingDTO.DateOfService = Convert.ToString(reader["DateOfService"]);
                        chartSummaryDTO.ProjectID = projectID;
                        if (Role == "Coder" && ChartType == "Block")
                        {
                            chartSummaryDTO.BlockCategory = Convert.ToString(reader["BlockCategory"]);
                            chartSummaryDTO.BlockRemarks = Convert.ToString(reader["BlockRemarks"]);
                            chartSummaryDTO.BlockedDate = Convert.ToDateTime(reader["BlockedDate"]).ToLocalDate();
                        }
                        else if (Role == "ShadowQA" && ChartType == "Block")
                        {
                            chartSummaryDTO.BlockCategory = Convert.ToString(reader["BlockCategory"]);
                            chartSummaryDTO.BlockRemarks = Convert.ToString(reader["BlockRemarks"]);
                            chartSummaryDTO.BlockedDate = Convert.ToDateTime(reader["BlockedDate"]).ToLocalDate();
                            chartSummaryDTO.ProjectName = Convert.ToString(reader["ProjectName"]);


                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            chartSummaryDTO.QABy = Convert.ToString(reader["QABy"]);

                            chartSummaryDTO.PayorText = Convert.ToString(reader["PayorText"]);
                            chartSummaryDTO.QAPayorText = Convert.ToString(reader["QAPayorText"]);
                            chartSummaryDTO.QAPayorRemarks = Convert.ToString(reader["QAPayorIdRemark"]);

                            chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            chartSummaryDTO.QAProviderText = Convert.ToString(reader["QAProviderText"]);
                            chartSummaryDTO.QAProviderRemarks = Convert.ToString(reader["QAProviderIDRemark"]);

                            chartSummaryDTO.QACPTCode = Convert.ToString(reader["QACPTCode"]);
                            chartSummaryDTO.QACPTCodeRemarks = Convert.ToString(reader["QACPTCodeRemark"]);

                            chartSummaryDTO.QAMod = Convert.ToString(reader["QAMod"]);
                            chartSummaryDTO.QAModRemarks = Convert.ToString(reader["QAModRemark"]);

                            chartSummaryDTO.QADx = Convert.ToString(reader["QADx"]);
                            chartSummaryDTO.QADxRemarks = Convert.ToString(reader["QADxRemark"]);

                            chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["ProviderFeedbackText"]);
                            chartSummaryDTO.QAProviderFeedbackText = Convert.ToString(reader["QAProviderFeedbackText"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QAProviderFeedbackIDRemark"]);

                            //if (reader["ProviderId"] != DBNull.Value)
                            //    chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            //if (reader["PayorId"] != DBNull.Value)
                            //    chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            // chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            //if (reader["ProviderFeedbackId"] != DBNull.Value)
                            //    chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackId"]);
                        }
                        else if (Role == "QA" && ChartType == "Block")
                        {
                            chartSummaryDTO.BlockCategory = Convert.ToString(reader["BlockCategory"]);
                            chartSummaryDTO.BlockRemarks = Convert.ToString(reader["BlockRemarks"]);
                            chartSummaryDTO.BlockedDate = Convert.ToDateTime(reader["BlockedDate"]).ToLocalDate();

                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);

                            if (reader["ProviderId"] != DBNull.Value)
                                chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            if (reader["PayorId"] != DBNull.Value)
                                chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            // chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            if (reader["ProviderFeedbackId"] != DBNull.Value)
                                chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackId"]);
                        }
                        lst.Add(chartSummaryDTO);
                    }
                }
            }

            return lst;
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
                            Value = mUserId
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
                        chartSummaryDTO.CodingDTO.ListName = Convert.ToString(reader["ListName"]);
                        //chartSummaryDTO.CodingDTO.ListName = "PK-Card APP Consult";
                        chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        chartSummaryDTO.CodingDTO.DateOfService = Convert.ToString(reader["DateOfService"]);
                        if (Role == "Coder" && ChartType == "Available")
                        {
                            if (reader["ProviderId"] != DBNull.Value)
                                chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                        }

                        if (Role == "Coder" && ChartType == "Block")
                        {
                            chartSummaryDTO.BlockCategory = Convert.ToString(reader["BlockCategory"]);
                            chartSummaryDTO.BlockRemarks = Convert.ToString(reader["BlockRemarks"]);
                            chartSummaryDTO.BlockedDate = Convert.ToDateTime(reader["BlockedDate"]).ToLocalDate();
                        }
                        else if (Role == "QA" && ChartType == "Block")
                        {
                            chartSummaryDTO.BlockCategory = Convert.ToString(reader["BlockCategory"]);
                            chartSummaryDTO.BlockRemarks = Convert.ToString(reader["BlockRemarks"]);
                            chartSummaryDTO.BlockedDate = Convert.ToDateTime(reader["BlockedDate"]).ToLocalDate();

                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);

                            if (reader["ProviderId"] != DBNull.Value)
                                chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            if (reader["PayorId"] != DBNull.Value)
                                chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            // chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            if (reader["ProviderFeedbackId"] != DBNull.Value)
                                chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackId"]);
                        }
                        else if ((Role == "QA" && ChartType == "Available") || (Role == "Coder" && ChartType == "ReadyForPosting") ||
                            (Role == "QA" && ChartType == "OnHold"))
                        {
                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);

                            if (reader["ProviderId"] != DBNull.Value)
                                chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            if (reader["PayorId"] != DBNull.Value)
                                chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            // chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            if (reader["ProviderFeedbackId"] != DBNull.Value)
                                chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackId"]);
                            if (Role == "QA" && ChartType == "OnHold")
                                chartSummaryDTO.CoderQuestion = Convert.ToString(reader["Question"]);
                        }
                        else if (Role == "ShadowQA" && ChartType == "Available")
                        {
                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            chartSummaryDTO.QABy = Convert.ToString(reader["QABy"]);

                            chartSummaryDTO.PayorText = Convert.ToString(reader["PayorText"]);
                            chartSummaryDTO.QAPayorText = Convert.ToString(reader["QAPayorText"]);
                            chartSummaryDTO.QAPayorRemarks = Convert.ToString(reader["QAPayorIdRemark"]);

                            chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            chartSummaryDTO.QAProviderText = Convert.ToString(reader["QAProviderText"]);
                            chartSummaryDTO.QAProviderRemarks = Convert.ToString(reader["QAProviderIDRemark"]);

                            chartSummaryDTO.QACPTCode = Convert.ToString(reader["QACPTCode"]);
                            chartSummaryDTO.QACPTCodeRemarks = Convert.ToString(reader["QACPTCodeRemark"]);

                            chartSummaryDTO.QAMod = Convert.ToString(reader["QAMod"]);
                            chartSummaryDTO.QAModRemarks = Convert.ToString(reader["QAModRemark"]);

                            chartSummaryDTO.QADx = Convert.ToString(reader["QADx"]);
                            chartSummaryDTO.QADxRemarks = Convert.ToString(reader["QADxRemark"]);

                            chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["ProviderFeedbackText"]);
                            chartSummaryDTO.QAProviderFeedbackText = Convert.ToString(reader["QAProviderFeedbackText"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QAProviderFeedbackIDRemark"]);

                            //if (reader["ProviderId"] != DBNull.Value)
                            //    chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            //if (reader["PayorId"] != DBNull.Value)
                            //    chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            // chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            //if (reader["ProviderFeedbackId"] != DBNull.Value)
                            //    chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackId"]);
                        }
                        else if (Role == "ShadowQA" && ChartType == "Block")
                        {
                            chartSummaryDTO.BlockCategory = Convert.ToString(reader["BlockCategory"]);
                            chartSummaryDTO.BlockRemarks = Convert.ToString(reader["BlockRemarks"]);
                            chartSummaryDTO.BlockedDate = Convert.ToDateTime(reader["BlockedDate"]).ToLocalDate();

                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            chartSummaryDTO.QABy = Convert.ToString(reader["QABy"]);

                            chartSummaryDTO.PayorText = Convert.ToString(reader["PayorText"]);
                            chartSummaryDTO.QAPayorText = Convert.ToString(reader["QAPayorText"]);
                            chartSummaryDTO.QAPayorRemarks = Convert.ToString(reader["QAPayorIdRemark"]);

                            chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            chartSummaryDTO.QAProviderText = Convert.ToString(reader["QAProviderText"]);
                            chartSummaryDTO.QAProviderRemarks = Convert.ToString(reader["QAProviderIDRemark"]);

                            chartSummaryDTO.QACPTCode = Convert.ToString(reader["QACPTCode"]);
                            chartSummaryDTO.QACPTCodeRemarks = Convert.ToString(reader["QACPTCodeRemark"]);

                            chartSummaryDTO.QAMod = Convert.ToString(reader["QAMod"]);
                            chartSummaryDTO.QAModRemarks = Convert.ToString(reader["QAModRemark"]);

                            chartSummaryDTO.QADx = Convert.ToString(reader["QADx"]);
                            chartSummaryDTO.QADxRemarks = Convert.ToString(reader["QADxRemark"]);

                            chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["ProviderFeedbackText"]);
                            chartSummaryDTO.QAProviderFeedbackText = Convert.ToString(reader["QAProviderFeedbackText"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QAProviderFeedbackIDRemark"]);

                            //if (reader["ProviderId"] != DBNull.Value)
                            //    chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            //if (reader["PayorId"] != DBNull.Value)
                            //    chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            // chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            //if (reader["ProviderFeedbackId"] != DBNull.Value)
                            //    chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackId"]);
                        }
                        else if (Role == "ShadowQA" && ChartType == "RebuttalOfQA")
                        {
                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            chartSummaryDTO.QABy = Convert.ToString(reader["QABy"]);
                            chartSummaryDTO.ShadowQABy = Convert.ToString(reader["ShadowQABy"]);

                            //chartSummaryDTO.PayorText = Convert.ToString(reader["PayorText"]);
                            //chartSummaryDTO.QAPayorText = Convert.ToString(reader["QAPayorText"]);

                            //chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            //chartSummaryDTO.QAProviderText = Convert.ToString(reader["QAProviderText"]);

                            //chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["FeedbackText"]);
                            //chartSummaryDTO.QAProviderFeedbackText = Convert.ToString(reader["QAFeedbackText"]);

                            chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            chartSummaryDTO.QAProviderText = Convert.ToString(reader["QAProviderText"]);
                            chartSummaryDTO.QAProviderRemarks = Convert.ToString(reader["QARebuttedProviderIDRemark"]);
                            if (reader["ShadowQAProviderID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAProviderID = Convert.ToInt32(reader["ShadowQAProviderID"]);
                            chartSummaryDTO.ShadowQAProviderRemarks = Convert.ToString(reader["ShadowQAProviderIDRemark"]);

                            chartSummaryDTO.PayorText = Convert.ToString(reader["PayorText"]);
                            chartSummaryDTO.QAPayorText = Convert.ToString(reader["QAPayorText"]);
                            if (reader["ShadowQAPayorID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAPayorID = Convert.ToInt32(reader["ShadowQAPayorID"]);
                            chartSummaryDTO.QAPayorRemarks = Convert.ToString(reader["QARebuttedPayorIdRemark"]);
                            chartSummaryDTO.ShadowQAPayorRemarks = Convert.ToString(reader["ShadowQAPayorIdRemark"]);

                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.QADx = Convert.ToString(reader["QADx"]);
                            chartSummaryDTO.QADxRemarks = Convert.ToString(reader["QARebuttedDxRemark"]);
                            chartSummaryDTO.ShadowQADx = Convert.ToString(reader["ShadowQADx"]);
                            chartSummaryDTO.ShadowQADxRemarks = Convert.ToString(reader["ShadowQADxRemark"]);

                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            chartSummaryDTO.QACPTCode = Convert.ToString(reader["QACPTCode"]);
                            chartSummaryDTO.QACPTCodeRemarks = Convert.ToString(reader["QARebuttedCPTCodeRemark"]);
                            chartSummaryDTO.ShadowQACPTCode = Convert.ToString(reader["ShadowQACPTCode"]);
                            chartSummaryDTO.ShadowQACPTCodeRemarks = Convert.ToString(reader["ShadowQACPTCodeRemark"]);

                            chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["ProviderFeedbackText"]);
                            //chartSummaryDTO.QAProviderFeedbackText = Convert.ToString(reader["QAProviderFeedbackText"]);
                            if (reader["ShadowQAProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAProviderFeedbackID = Convert.ToInt32(reader["ShadowQAProviderFeedbackID"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QARebuttedProviderFeedbackIDRemark"]);
                            chartSummaryDTO.ShadowQAProviderFeedbackRemarks = Convert.ToString(reader["ShadowQAProviderFeedbackIDRemark"]);


                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);

                            //if (reader["ShadowQAPayorID"] != DBNull.Value)
                            //    chartSummaryDTO.ShadowQAPayorID = Convert.ToInt32(reader["ShadowQAPayorID"]);
                            //chartSummaryDTO.ShadowQAPayorRemarks = Convert.ToString(reader["ShadowQAPayorIdRemark"]);

                            //if (reader["ShadowQAProviderID"] != DBNull.Value)
                            //    chartSummaryDTO.ShadowQAProviderID = Convert.ToInt32(reader["ShadowQAProviderID"]);
                            //chartSummaryDTO.ShadowQAProviderRemarks = Convert.ToString(reader["ShadowQAProviderIDRemark"]);

                            //chartSummaryDTO.ShadowQADx = Convert.ToString(reader["ShadowQADx"]);
                            //chartSummaryDTO.ShadowQADxRemarks = Convert.ToString(reader["ShadowQADxRemark"]);

                            //chartSummaryDTO.ShadowQAMod = Convert.ToString(reader["ShadowQAMod"]);
                            //chartSummaryDTO.ShadowQAModRemarks = Convert.ToString(reader["ShadowQAModRemark"]);

                            //chartSummaryDTO.ShadowQACPTCode = Convert.ToString(reader["ShadowQACPTCode"]);
                            //chartSummaryDTO.ShadowQACPTCodeRemarks = Convert.ToString(reader["ShadowQACPTCodeRemark"]);

                            //if (reader["ShadowQAProviderFeedbackID"] != DBNull.Value)
                            //    chartSummaryDTO.ShadowQAProviderFeedbackID = Convert.ToInt32(reader["ShadowQAProviderFeedbackID"]);
                            //chartSummaryDTO.ShadowQAProviderFeedbackRemarks = Convert.ToString(reader["ShadowQAProviderFeedbackIDRemark"]);

                            //chartSummaryDTO.RevisedPayorRemarks = Convert.ToString(reader["RebuttedPayorIdRemark"]);
                            //chartSummaryDTO.RevisedProviderRemarks = Convert.ToString(reader["RebuttedProviderIDRemark"]);
                            //chartSummaryDTO.RevisedCPTRemarks = Convert.ToString(reader["RebuttedCPTCodeRemark"]);
                            //chartSummaryDTO.RevisedModRemarks = Convert.ToString(reader["RebuttedModRemark"]);
                            //chartSummaryDTO.RevisedDXRemarks = Convert.ToString(reader["RebuttedDxRemark"]);
                            //chartSummaryDTO.RevisedProviderFeedbackRemarks = Convert.ToString(reader["RebuttedProviderFeedbackIDRemark"]);

                        }
                        else if ((Role == "Coder" && ChartType == "Incorrect") ||
                             (Role == "QA" && ChartType == "ShadowQARejected"))
                        {
                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            chartSummaryDTO.QABy = Convert.ToString(reader["QABy"]);
                            chartSummaryDTO.ShadowQABy = Convert.ToString(reader["ShadowQABy"]);

                            chartSummaryDTO.PayorText = Convert.ToString(reader["PayorText"]);
                            chartSummaryDTO.QAPayorText = Convert.ToString(reader["QAPayorText"]);

                            chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            chartSummaryDTO.QAProviderText = Convert.ToString(reader["QAProviderText"]);

                            chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["FeedbackText"]);
                            chartSummaryDTO.QAProviderFeedbackText = Convert.ToString(reader["QAFeedbackText"]);

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
                            //chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            chartSummaryDTO.QAMod = Convert.ToString(reader["QAMod"]);
                            chartSummaryDTO.QAModRemarks = Convert.ToString(reader["QAModRemark"]);
                            if (reader["ProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackID"]);
                            if (reader["QAProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderFeedbackID = Convert.ToInt32(reader["QAProviderFeedbackID"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QAProviderFeedbackIDRemark"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);

                            if (reader["ShadowQAPayorID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAPayorID = Convert.ToInt32(reader["ShadowQAPayorID"]);
                            chartSummaryDTO.ShadowQAPayorRemarks = Convert.ToString(reader["ShadowQAPayorIdRemark"]);

                            if (reader["ShadowQAProviderID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAProviderID = Convert.ToInt32(reader["ShadowQAProviderID"]);
                            chartSummaryDTO.ShadowQAProviderRemarks = Convert.ToString(reader["ShadowQAProviderIDRemark"]);

                            chartSummaryDTO.ShadowQADx = Convert.ToString(reader["ShadowQADx"]);
                            chartSummaryDTO.ShadowQADxRemarks = Convert.ToString(reader["ShadowQADxRemark"]);

                            chartSummaryDTO.ShadowQAMod = Convert.ToString(reader["ShadowQAMod"]);
                            chartSummaryDTO.ShadowQAModRemarks = Convert.ToString(reader["ShadowQAModRemark"]);

                            chartSummaryDTO.ShadowQACPTCode = Convert.ToString(reader["ShadowQACPTCode"]);
                            chartSummaryDTO.ShadowQACPTCodeRemarks = Convert.ToString(reader["ShadowQACPTCodeRemark"]);

                            if (reader["ShadowQAProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAProviderFeedbackID = Convert.ToInt32(reader["ShadowQAProviderFeedbackID"]);
                            chartSummaryDTO.ShadowQAProviderFeedbackRemarks = Convert.ToString(reader["ShadowQAProviderFeedbackIDRemark"]);

                            chartSummaryDTO.RevisedPayorRemarks = Convert.ToString(reader["RebuttedPayorIdRemark"]);
                            chartSummaryDTO.RevisedProviderRemarks = Convert.ToString(reader["RebuttedProviderIDRemark"]);
                            chartSummaryDTO.RevisedCPTRemarks = Convert.ToString(reader["RebuttedCPTCodeRemark"]);
                            chartSummaryDTO.RevisedModRemarks = Convert.ToString(reader["RebuttedModRemark"]);
                            chartSummaryDTO.RevisedDXRemarks = Convert.ToString(reader["RebuttedDxRemark"]);
                            chartSummaryDTO.RevisedProviderFeedbackRemarks = Convert.ToString(reader["RebuttedProviderFeedbackIDRemark"]);

                        }
                        else if (Role == "QA" && ChartType == "RebuttalOfCoder")
                        {
                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            chartSummaryDTO.QABy = Convert.ToString(reader["QABy"]);
                            chartSummaryDTO.ShadowQABy = Convert.ToString(reader["ShadowQABy"]);

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
                            //chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            chartSummaryDTO.QAMod = Convert.ToString(reader["QAMod"]);
                            chartSummaryDTO.QAModRemarks = Convert.ToString(reader["QAModRemark"]);
                            if (reader["ProviderFeedbackID"] != DBNull.Value)
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
                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            chartSummaryDTO.QABy = Convert.ToString(reader["QABy"]);
                            chartSummaryDTO.ShadowQABy = Convert.ToString(reader["ShadowQABy"]);

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

        public List<ChartSummaryDTO> GetNext1(string Role, string ChartType, int projectID)
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
                        //chartSummaryDTO.CodingDTO.ListName = Convert.ToString(reader["ListName"]);
                        chartSummaryDTO.CodingDTO.ListName = "PK-Card APP Consult";
                        chartSummaryDTO.CodingDTO.PatientMRN = Convert.ToString(reader["PatientMRN"]);
                        chartSummaryDTO.CodingDTO.Name = Convert.ToString(reader["Name"]);
                        chartSummaryDTO.CodingDTO.DateOfService = Convert.ToString(reader["DateOfService"]);
                        if (Role == "Coder" && ChartType == "Available")
                        {
                            if (reader["ProviderId"] != DBNull.Value)
                                chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                        }

                        if (Role == "Coder" && ChartType == "Block")
                        {
                            chartSummaryDTO.BlockCategory = Convert.ToString(reader["BlockCategory"]);
                            chartSummaryDTO.BlockRemarks = Convert.ToString(reader["BlockRemarks"]);
                            chartSummaryDTO.BlockedDate = Convert.ToDateTime(reader["BlockedDate"]).ToLocalDate();
                        }
                        else if (Role == "QA" && ChartType == "Block")
                        {
                            chartSummaryDTO.BlockCategory = Convert.ToString(reader["BlockCategory"]);
                            chartSummaryDTO.BlockRemarks = Convert.ToString(reader["BlockRemarks"]);
                            chartSummaryDTO.BlockedDate = Convert.ToDateTime(reader["BlockedDate"]).ToLocalDate();

                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);

                            if (reader["ProviderId"] != DBNull.Value)
                                chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            if (reader["PayorId"] != DBNull.Value)
                                chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            // chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            if (reader["ProviderFeedbackId"] != DBNull.Value)
                                chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackId"]);
                        }
                        else if ((Role == "Coder" && ChartType == "ReadyForPosting") ||
                            (Role == "QA" && ChartType == "OnHold"))
                        {
                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);

                            if (reader["ProviderId"] != DBNull.Value)
                                chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            if (reader["PayorId"] != DBNull.Value)
                                chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            // chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            if (reader["ProviderFeedbackId"] != DBNull.Value)
                                chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackId"]);
                            if (Role == "QA" && ChartType == "OnHold")
                                chartSummaryDTO.CoderQuestion = Convert.ToString(reader["Question"]);
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
                            if (reader["PayorId"] != DBNull.Value)
                                chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            // chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            if (reader["ProviderFeedbackId"] != DBNull.Value)
                                chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackId"]);
                            chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
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

                            chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            chartSummaryDTO.QAProviderText = Convert.ToString(reader["QAProviderText"]);

                            chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["FeedbackText"]);
                            chartSummaryDTO.QAProviderFeedbackText = Convert.ToString(reader["QAFeedbackText"]);

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
                            chartSummaryDTO.QACPTCodeRemarks = Convert.ToString(reader["QACPTCodeRemark"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            chartSummaryDTO.QACPTCode = Convert.ToString(reader["QACPTCode"]);
                            //chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            chartSummaryDTO.QAMod = Convert.ToString(reader["QAMod"]);
                            chartSummaryDTO.QAModRemarks = Convert.ToString(reader["QAModRemark"]);
                            if (reader["ProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackID"]);
                            if (reader["QAProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderFeedbackID = Convert.ToInt32(reader["QAProviderFeedbackID"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QAProviderFeedbackIDRemark"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);

                            if (reader["ShadowQAPayorID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAPayorID = Convert.ToInt32(reader["ShadowQAPayorID"]);
                            chartSummaryDTO.ShadowQAPayorRemarks = Convert.ToString(reader["ShadowQAPayorIdRemark"]);

                            if (reader["ShadowQAProviderID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAProviderID = Convert.ToInt32(reader["ShadowQAProviderID"]);
                            chartSummaryDTO.ShadowQAProviderRemarks = Convert.ToString(reader["ShadowQAProviderIDRemark"]);

                            chartSummaryDTO.ShadowQADx = Convert.ToString(reader["ShadowQADx"]);
                            chartSummaryDTO.ShadowQADxRemarks = Convert.ToString(reader["ShadowQADxRemark"]);

                            chartSummaryDTO.ShadowQAMod = Convert.ToString(reader["ShadowQAMod"]);
                            chartSummaryDTO.ShadowQAModRemarks = Convert.ToString(reader["ShadowQAModRemark"]);

                            chartSummaryDTO.ShadowQACPTCode = Convert.ToString(reader["ShadowQACPTCode"]);
                            chartSummaryDTO.ShadowQACPTCodeRemarks = Convert.ToString(reader["ShadowQACPTCodeRemark"]);

                            if (reader["ShadowQAProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAProviderFeedbackID = Convert.ToInt32(reader["ShadowQAProviderFeedbackID"]);
                            chartSummaryDTO.ShadowQAProviderFeedbackRemarks = Convert.ToString(reader["ShadowQAProviderFeedbackIDRemark"]);

                            chartSummaryDTO.RevisedPayorRemarks = Convert.ToString(reader["RebuttedPayorIdRemark"]);
                            chartSummaryDTO.RevisedProviderRemarks = Convert.ToString(reader["RebuttedProviderIDRemark"]);
                            chartSummaryDTO.RevisedCPTRemarks = Convert.ToString(reader["RebuttedCPTCodeRemark"]);
                            chartSummaryDTO.RevisedModRemarks = Convert.ToString(reader["RebuttedModRemark"]);
                            chartSummaryDTO.RevisedDXRemarks = Convert.ToString(reader["RebuttedDxRemark"]);
                            chartSummaryDTO.RevisedProviderFeedbackRemarks = Convert.ToString(reader["RebuttedProviderFeedbackIDRemark"]);

                        }
                        else if (Role == "ShadowQA" && ChartType == "Available")
                        {
                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            chartSummaryDTO.QABy = Convert.ToString(reader["QABy"]);

                            chartSummaryDTO.PayorText = Convert.ToString(reader["PayorText"]);
                            chartSummaryDTO.QAPayorText = Convert.ToString(reader["QAPayorText"]);
                            chartSummaryDTO.QAPayorRemarks = Convert.ToString(reader["QAPayorIdRemark"]);

                            chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            chartSummaryDTO.QAProviderText = Convert.ToString(reader["QAProviderText"]);
                            chartSummaryDTO.QAProviderRemarks = Convert.ToString(reader["QAProviderIDRemark"]);

                            chartSummaryDTO.QACPTCode = Convert.ToString(reader["QACPTCode"]);
                            chartSummaryDTO.QACPTCodeRemarks = Convert.ToString(reader["QACPTCodeRemark"]);

                            chartSummaryDTO.QAMod = Convert.ToString(reader["QAMod"]);
                            chartSummaryDTO.QAModRemarks = Convert.ToString(reader["QAModRemark"]);

                            chartSummaryDTO.QADx = Convert.ToString(reader["QADx"]);
                            chartSummaryDTO.QADxRemarks = Convert.ToString(reader["QADxRemark"]);

                            chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["ProviderFeedbackText"]);
                            chartSummaryDTO.QAProviderFeedbackText = Convert.ToString(reader["QAProviderFeedbackText"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QAProviderFeedbackIDRemark"]);

                            //if (reader["ProviderId"] != DBNull.Value)
                            //    chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            //if (reader["PayorId"] != DBNull.Value)
                            //    chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            // chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            //if (reader["ProviderFeedbackId"] != DBNull.Value)
                            //    chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackId"]);
                        }
                        else if (Role == "ShadowQA" && ChartType == "Block")
                        {
                            chartSummaryDTO.BlockCategory = Convert.ToString(reader["BlockCategory"]);
                            chartSummaryDTO.BlockRemarks = Convert.ToString(reader["BlockRemarks"]);
                            chartSummaryDTO.BlockedDate = Convert.ToDateTime(reader["BlockedDate"]).ToLocalDate();

                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            chartSummaryDTO.QABy = Convert.ToString(reader["QABy"]);

                            chartSummaryDTO.PayorText = Convert.ToString(reader["PayorText"]);
                            chartSummaryDTO.QAPayorText = Convert.ToString(reader["QAPayorText"]);
                            chartSummaryDTO.QAPayorRemarks = Convert.ToString(reader["QAPayorIdRemark"]);

                            chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            chartSummaryDTO.QAProviderText = Convert.ToString(reader["QAProviderText"]);
                            chartSummaryDTO.QAProviderRemarks = Convert.ToString(reader["QAProviderIDRemark"]);

                            chartSummaryDTO.QACPTCode = Convert.ToString(reader["QACPTCode"]);
                            chartSummaryDTO.QACPTCodeRemarks = Convert.ToString(reader["QACPTCodeRemark"]);

                            chartSummaryDTO.QAMod = Convert.ToString(reader["QAMod"]);
                            chartSummaryDTO.QAModRemarks = Convert.ToString(reader["QAModRemark"]);

                            chartSummaryDTO.QADx = Convert.ToString(reader["QADx"]);
                            chartSummaryDTO.QADxRemarks = Convert.ToString(reader["QADxRemark"]);

                            chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["ProviderFeedbackText"]);
                            chartSummaryDTO.QAProviderFeedbackText = Convert.ToString(reader["QAProviderFeedbackText"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QAProviderFeedbackIDRemark"]);

                            //if (reader["ProviderId"] != DBNull.Value)
                            //    chartSummaryDTO.ProviderID = Convert.ToInt32(reader["ProviderId"]);
                            //if (reader["PayorId"] != DBNull.Value)
                            //    chartSummaryDTO.PayorID = Convert.ToInt32(reader["PayorId"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            // chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            //if (reader["ProviderFeedbackId"] != DBNull.Value)
                            //    chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackId"]);
                        }
                        else if (Role == "ShadowQA" && ChartType == "RebuttalOfQA")
                        {
                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            chartSummaryDTO.QABy = Convert.ToString(reader["QABy"]);
                            chartSummaryDTO.ShadowQABy = Convert.ToString(reader["ShadowQABy"]);

                            //chartSummaryDTO.PayorText = Convert.ToString(reader["PayorText"]);
                            //chartSummaryDTO.QAPayorText = Convert.ToString(reader["QAPayorText"]);

                            //chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            //chartSummaryDTO.QAProviderText = Convert.ToString(reader["QAProviderText"]);

                            //chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["FeedbackText"]);
                            //chartSummaryDTO.QAProviderFeedbackText = Convert.ToString(reader["QAFeedbackText"]);

                            chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            chartSummaryDTO.QAProviderText = Convert.ToString(reader["QAProviderText"]);
                            chartSummaryDTO.QAProviderRemarks = Convert.ToString(reader["QARebuttedProviderIDRemark"]);
                            if (reader["ShadowQAProviderID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAProviderID = Convert.ToInt32(reader["ShadowQAProviderID"]);
                            chartSummaryDTO.ShadowQAProviderRemarks = Convert.ToString(reader["ShadowQAProviderIDRemark"]);

                            chartSummaryDTO.PayorText = Convert.ToString(reader["PayorText"]);
                            chartSummaryDTO.QAPayorText = Convert.ToString(reader["QAPayorText"]);
                            if (reader["ShadowQAPayorID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAPayorID = Convert.ToInt32(reader["ShadowQAPayorID"]);
                            chartSummaryDTO.QAPayorRemarks = Convert.ToString(reader["QARebuttedPayorIdRemark"]);
                            chartSummaryDTO.ShadowQAPayorRemarks = Convert.ToString(reader["ShadowQAPayorIdRemark"]);

                            chartSummaryDTO.Dx = Convert.ToString(reader["DxCode"]);
                            chartSummaryDTO.QADx = Convert.ToString(reader["QADx"]);
                            chartSummaryDTO.QADxRemarks = Convert.ToString(reader["QARebuttedDxRemark"]);
                            chartSummaryDTO.ShadowQADx = Convert.ToString(reader["ShadowQADx"]);
                            chartSummaryDTO.ShadowQADxRemarks = Convert.ToString(reader["ShadowQADxRemark"]);

                            chartSummaryDTO.CPTCode = Convert.ToString(reader["CPTCode"]);
                            chartSummaryDTO.QACPTCode = Convert.ToString(reader["QACPTCode"]);
                            chartSummaryDTO.QACPTCodeRemarks = Convert.ToString(reader["QARebuttedCPTCodeRemark"]);
                            chartSummaryDTO.ShadowQACPTCode = Convert.ToString(reader["ShadowQACPTCode"]);
                            chartSummaryDTO.ShadowQACPTCodeRemarks = Convert.ToString(reader["ShadowQACPTCodeRemark"]);

                            chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["ProviderFeedbackText"]);
                            //chartSummaryDTO.QAProviderFeedbackText = Convert.ToString(reader["QAProviderFeedbackText"]);
                            if (reader["ShadowQAProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAProviderFeedbackID = Convert.ToInt32(reader["ShadowQAProviderFeedbackID"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QARebuttedProviderFeedbackIDRemark"]);
                            chartSummaryDTO.ShadowQAProviderFeedbackRemarks = Convert.ToString(reader["ShadowQAProviderFeedbackIDRemark"]);


                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                        }
                        else if (Role == "QA" && ChartType == "ShadowQARejected")
                        {
                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            chartSummaryDTO.QABy = Convert.ToString(reader["QABy"]);
                            chartSummaryDTO.ShadowQABy = Convert.ToString(reader["ShadowQABy"]);

                            chartSummaryDTO.PayorText = Convert.ToString(reader["PayorText"]);
                            chartSummaryDTO.QAPayorText = Convert.ToString(reader["QAPayorText"]);

                            chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            chartSummaryDTO.QAProviderText = Convert.ToString(reader["QAProviderText"]);

                            chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["FeedbackText"]);
                            chartSummaryDTO.QAProviderFeedbackText = Convert.ToString(reader["QAFeedbackText"]);

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
                            //chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            chartSummaryDTO.QAMod = Convert.ToString(reader["QAMod"]);
                            chartSummaryDTO.QAModRemarks = Convert.ToString(reader["QAModRemark"]);
                            if (reader["ProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackID"]);
                            if (reader["QAProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderFeedbackID = Convert.ToInt32(reader["QAProviderFeedbackID"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QAProviderFeedbackIDRemark"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);

                            if (reader["ShadowQAPayorID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAPayorID = Convert.ToInt32(reader["ShadowQAPayorID"]);
                            chartSummaryDTO.ShadowQAPayorRemarks = Convert.ToString(reader["ShadowQAPayorIdRemark"]);

                            if (reader["ShadowQAProviderID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAProviderID = Convert.ToInt32(reader["ShadowQAProviderID"]);
                            chartSummaryDTO.ShadowQAProviderRemarks = Convert.ToString(reader["ShadowQAProviderIDRemark"]);

                            chartSummaryDTO.ShadowQADx = Convert.ToString(reader["ShadowQADx"]);
                            chartSummaryDTO.ShadowQADxRemarks = Convert.ToString(reader["ShadowQADxRemark"]);

                            chartSummaryDTO.ShadowQAMod = Convert.ToString(reader["ShadowQAMod"]);
                            chartSummaryDTO.ShadowQAModRemarks = Convert.ToString(reader["ShadowQAModRemark"]);

                            chartSummaryDTO.ShadowQACPTCode = Convert.ToString(reader["ShadowQACPTCode"]);
                            chartSummaryDTO.ShadowQACPTCodeRemarks = Convert.ToString(reader["ShadowQACPTCodeRemark"]);

                            if (reader["ShadowQAProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.ShadowQAProviderFeedbackID = Convert.ToInt32(reader["ShadowQAProviderFeedbackID"]);
                            chartSummaryDTO.ShadowQAProviderFeedbackRemarks = Convert.ToString(reader["ShadowQAProviderFeedbackIDRemark"]);

                            chartSummaryDTO.RevisedPayorRemarks = Convert.ToString(reader["RebuttedPayorIdRemark"]);
                            chartSummaryDTO.RevisedProviderRemarks = Convert.ToString(reader["RebuttedProviderIDRemark"]);
                            chartSummaryDTO.RevisedCPTRemarks = Convert.ToString(reader["RebuttedCPTCodeRemark"]);
                            chartSummaryDTO.RevisedModRemarks = Convert.ToString(reader["RebuttedModRemark"]);
                            chartSummaryDTO.RevisedDXRemarks = Convert.ToString(reader["RebuttedDxRemark"]);
                            chartSummaryDTO.RevisedProviderFeedbackRemarks = Convert.ToString(reader["RebuttedProviderFeedbackIDRemark"]);

                        }
                        else if (Role == "QA" && ChartType == "RebuttalOfCoder")
                        {
                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            chartSummaryDTO.QABy = Convert.ToString(reader["QABy"]);
                            chartSummaryDTO.ShadowQABy = Convert.ToString(reader["ShadowQABy"]);

                            if (reader["ClaimId"] != DBNull.Value)
                                chartSummaryDTO.ClaimId = Convert.ToInt32(reader["ClaimId"]);
                            else
                                chartSummaryDTO.ClaimId = null;
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
                            //chartSummaryDTO.Mod = Convert.ToString(reader["Modifier"]);
                            //chartSummaryDTO.QAMod = Convert.ToString(reader["QAMod"]);
                            //chartSummaryDTO.QAModRemarks = Convert.ToString(reader["QAModRemark"]);
                            if (reader["ProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.ProviderFeedbackID = Convert.ToInt32(reader["ProviderFeedbackID"]);
                            if (reader["QAProviderFeedbackID"] != DBNull.Value)
                                chartSummaryDTO.QAProviderFeedbackID = Convert.ToInt32(reader["QAProviderFeedbackID"]);
                            chartSummaryDTO.QAProviderFeedbackRemarks = Convert.ToString(reader["QAProviderFeedbackIDRemark"]);
                            chartSummaryDTO.NoteTitle = Convert.ToString(reader["NoteTitle"]);
                            chartSummaryDTO.ProviderText = Convert.ToString(reader["ProviderText"]);
                            chartSummaryDTO.PayorText = Convert.ToString(reader["PayorText"]);
                            chartSummaryDTO.ProviderFeedbackText = Convert.ToString(reader["ProviderFeedbackText"]);
                            chartSummaryDTO.QAProviderText = Convert.ToString(reader["QAProviderText"]);
                            chartSummaryDTO.QAPayorText = Convert.ToString(reader["QAPayorText"]);
                            chartSummaryDTO.QAProviderFeedbackText = Convert.ToString(reader["QAProviderFeedbackText"]);
                            chartSummaryDTO.RevisedPayorRemarks = Convert.ToString(reader["RebuttedPayorIdRemark"]);
                            chartSummaryDTO.RevisedProviderRemarks = Convert.ToString(reader["RebuttedProviderIDRemark"]);
                            chartSummaryDTO.RevisedCPTRemarks = Convert.ToString(reader["RebuttedCPTCodeRemark"]);
                            chartSummaryDTO.RevisedModRemarks = Convert.ToString(reader["RebuttedModRemark"]);
                            chartSummaryDTO.RevisedDXRemarks = Convert.ToString(reader["RebuttedDxRemark"]);
                            chartSummaryDTO.RevisedProviderFeedbackRemarks = Convert.ToString(reader["RebuttedProviderFeedbackIDRemark"]);
                        }
                        else if (Role == "QA" && ChartType == "ShadowQARejected")
                        {
                            chartSummaryDTO.CodedBy = Convert.ToString(reader["CodedBy"]);
                            chartSummaryDTO.QABy = Convert.ToString(reader["QABy"]);
                            chartSummaryDTO.ShadowQABy = Convert.ToString(reader["ShadowQABy"]);

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
                        }
                        lstchartSummaryDTO.Add(chartSummaryDTO);
                    }
                }
                return lstchartSummaryDTO;
            }
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
                     SqlDbType = System.Data.SqlDbType.Int,
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
                            Value = mUserId
                        },   new SqlParameter() {
                            ParameterName = "@ErrorTypeID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QADTO.ErrorType
                        },   new SqlParameter() {
                            ParameterName = "@IsAuditRequired",
                            SqlDbType =  System.Data.SqlDbType.Bit,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.IsAuditRequired
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
        public CodingDTO SubmitCodingIncorrectChart(ChartSummaryDTO chartSummaryDTO, int statusId, DataTable dtAudit)
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
                            ParameterName = "@RejectedCpt",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.RejectedCpt
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
                            ParameterName = "@RejectedDx",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.RejectedDx
                        },  new SqlParameter() {
                            ParameterName = "@DxRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.RevisedDXRemarks
                        }

                        , new SqlParameter() {
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
                            Value = mUserId
                        },   new SqlParameter() {
                            ParameterName = "@ErrorTypeID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.QADTO.ErrorType
                        }
                        ,   new SqlParameter() {
                            ParameterName = "@StatusId",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = statusId
                        },
                          new SqlParameter() {
                            ParameterName = "@utAudit",
                            SqlDbType =  System.Data.SqlDbType.Structured,
                            Direction = System.Data.ParameterDirection.Input,
                            TypeName = "utAudit",
                            Value = dtAudit
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
                            Value = mUserId
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

        public List<SearchResultDTO> GetSearchData(SearchParametersDTO searchParametersDTO)
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

                    con.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        SearchResultDTO dto = new SearchResultDTO()
                        {
                            ClinicalCaseId = Convert.ToString(reader["ClinicalCaseID"]),
                            FirstName = Convert.ToString(reader["FirstName"]),
                            LastName = Convert.ToString(reader["LastName"]),
                            MRN = Convert.ToString(reader["PatientMRN"]),
                            ProviderName = Convert.ToString(reader["Provider"]),
                            DoS = Convert.ToDateTime(reader["DateOfService"]),
                            ProjectName = Convert.ToString(reader["ProjectName"]),
                            Status = Convert.ToString(reader["Status"]),
                            IncludeBlocked = Convert.ToString(reader["IsBlocked"]),
                            Assigneduser = Convert.ToString(reader["AssignedTo"])
                        };
                        lstDto.Add(dto);
                    }
                }
            }

            if (!mUserRole.Contains("Manager"))
                lstDto = lstDto.Where(a => a.Assigneduser.Equals(Convert.ToString(mUserId))).ToList();

            if (!string.IsNullOrWhiteSpace(searchParametersDTO.ClinicalCaseId))
            {
                lstDto = lstDto.Where(s => s.ClinicalCaseId == searchParametersDTO.ClinicalCaseId).ToList();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(searchParametersDTO.FirstName))
                    lstDto = lstDto.Where(s => s.FirstName.Contains(searchParametersDTO.FirstName.ToUpper())).ToList();
                if (!string.IsNullOrWhiteSpace(searchParametersDTO.LastName))
                    lstDto = lstDto.Where(s => s.LastName.Contains(searchParametersDTO.LastName.ToUpper())).ToList();
                if (!string.IsNullOrWhiteSpace(searchParametersDTO.MRN))
                    lstDto = lstDto.Where(a => a.MRN == searchParametersDTO.MRN).ToList();
                if (searchParametersDTO.DoSFrom != default(DateTime) && searchParametersDTO.DoSTo != default(DateTime))
                {
                    var DoSFrom = searchParametersDTO.DoSFrom.Value;
                    var DoSTo = searchParametersDTO.DoSTo.Value;
                    lstDto = lstDto.Where(s => s.DoS >= DoSFrom && s.DoS <= DoSTo).ToList();
                }
                if (!string.IsNullOrWhiteSpace(searchParametersDTO.ProviderName))
                    lstDto = lstDto.Where(a => a.ProviderName == searchParametersDTO.ProviderName).ToList();

                if (!string.IsNullOrWhiteSpace(searchParametersDTO.StatusName) && searchParametersDTO.StatusName != "--Select a Status--")
                {
                    if (searchParametersDTO.IncludeBlocked)
                    {
                        lstDto = lstDto.Where(a => a.Status == searchParametersDTO.StatusName || a.IncludeBlocked == "1").ToList();
                    }
                    else
                    {
                        lstDto = lstDto.Where(a => a.Status == searchParametersDTO.StatusName).ToList();
                    }
                }
                if (!string.IsNullOrWhiteSpace(searchParametersDTO.ProjectName) && searchParametersDTO.ProjectName != "--Select a Project--")
                    lstDto = lstDto.Where(a => a.ProjectName == searchParametersDTO.ProjectName).ToList();
                if (searchParametersDTO.IncludeBlocked && (searchParametersDTO.StatusName == null || searchParametersDTO.StatusName == "--Select a Status--"))
                {
                    lstDto = lstDto.Where(a => a.IncludeBlocked == "1").ToList();
                }

            }
            return lstDto;
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
                        },   new SqlParameter() {
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
                            Value = mUserId
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


        public CodingDTO SubmitShadowQAAvailableChart(ChartSummaryDTO chartSummaryDTO, bool isQAAgreed)
        {
            CodingDTO dto = new CodingDTO();

            using (var context = new UABContext())
            {
                var param = new SqlParameter[] {
                     new SqlParameter() {
                            ParameterName = "@ShadowQAPayorID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ShadowQAPayorID
                        },
                       new SqlParameter() {
                            ParameterName = "@ShadowQAPayorRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ShadowQAPayorRemarks
                        },
                        new SqlParameter() {
                            ParameterName = "@ShadowQAProviderID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ShadowQAProviderID
                        },
                        new SqlParameter() {
                            ParameterName = "@ShadowQAProviderRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ShadowQAProviderRemarks
                        },
                         new SqlParameter() {
                            ParameterName = "@ShadowQACPTCode",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ShadowQACPTCode
                        },
                         new SqlParameter() {
                            ParameterName = "@ShadowQACPTCodeRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ShadowQACPTCodeRemarks
                        },
                        new SqlParameter() {
                            ParameterName = "@ShadowQAMod",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ShadowQAMod
                        },
                        new SqlParameter() {
                            ParameterName = "@ShadowQAModRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ShadowQAModRemarks
                        },  new SqlParameter() {
                            ParameterName = "@ShadowQADx",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ShadowQADx
                        },  new SqlParameter() {
                            ParameterName = "@ShadowQADxRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ShadowQADxRemarks
                        } , new SqlParameter() {
                            ParameterName = "@ShadowQAProviderFeedbackID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ShadowQAProviderFeedbackID
                        }, new SqlParameter() {
                            ParameterName = "@ShadowQAProviderFeedbackRemarks",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ShadowQAProviderFeedbackRemarks
                        },   new SqlParameter() {
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
                            ParameterName = "@ErrorTypeID",
                            SqlDbType =  System.Data.SqlDbType.Int,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ShadowQADTO.ErrorType
                        } ,   new SqlParameter() {
                            ParameterName = "@NotesfromJen",
                            SqlDbType =  System.Data.SqlDbType.VarChar,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = chartSummaryDTO.ShadowQADTO.NotesfromJen
                        },
                         new SqlParameter() {
                            ParameterName = "@isQAAgreed",
                            SqlDbType =  System.Data.SqlDbType.Bit,
                            Direction = System.Data.ParameterDirection.Input,
                            Value = isQAAgreed
                        }
                        //,
                        // new SqlParameter() {
                        //    ParameterName = "@QAPayorID",
                        //    SqlDbType =  System.Data.SqlDbType.Int,
                        //    Direction = System.Data.ParameterDirection.Input,
                        //    Value = chartSummaryDTO.QAPayorID
                        //}
                        //   ,
                        // new SqlParameter() {
                        //    ParameterName = "@QAProviderID",
                        //    SqlDbType =  System.Data.SqlDbType.Int,
                        //    Direction = System.Data.ParameterDirection.Input,
                        //    Value = chartSummaryDTO.QAProviderID
                        //}
                        // ,   new SqlParameter() {
                        //    ParameterName = "@QACPTCode",
                        //    SqlDbType =  System.Data.SqlDbType.VarChar,
                        //    Direction = System.Data.ParameterDirection.Input,
                        //    Value = chartSummaryDTO.QACPTCode
                        //}
                        //,   new SqlParameter() {
                        //    ParameterName = "@QAMod",
                        //    SqlDbType =  System.Data.SqlDbType.VarChar,
                        //    Direction = System.Data.ParameterDirection.Input,
                        //    Value = chartSummaryDTO.@QAMod
                        //}
                        //,   new SqlParameter() {
                        //    ParameterName = "@QADx",
                        //    SqlDbType =  System.Data.SqlDbType.VarChar,
                        //    Direction = System.Data.ParameterDirection.Input,
                        //    Value = chartSummaryDTO.QADx
                        //}
                        //  ,
                        // new SqlParameter() {
                        //    ParameterName = "@QAProviderFeedbackID",
                        //    SqlDbType =  System.Data.SqlDbType.Int,
                        //    Direction = System.Data.ParameterDirection.Input,
                        //    Value = chartSummaryDTO.QAProviderFeedbackID
                        //}
 
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



        public List<WorkflowHistoryDTO> GetWorkflowHistories(string ccid)
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
                        wf.DateandTime = Convert.ToDateTime(reader["Date"]).ToLocalDate();
                        wf.ByUser = Convert.ToString(reader["UserName"]);
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
                BlockHistory mdl = new BlockHistory()
                {
                    BlockCategoryId = Convert.ToInt32(bid),
                    BlockedByUserId = mUserId,
                    Remarks = remarks,
                    CreateDate = DateTime.Now,
                    ClinicalCaseId = Convert.ToInt32(ccid)
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
                    existingcc.AssignedTo = AssignedTouserid;
                    existingcc.AssignedBy = mUserId;
                    existingcc.AssignedDate = DateTime.Now;
                    existingcc.IsPriority = searchResultDTO.IsPriority ? 1 : 0;
                    context.Entry(existingcc).State = EntityState.Modified;
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
                return context.Status.ToList();
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
        public List<User> GetManageUsers()
        {
            using (var context = new UABContext())
            {
                return context.User.ToList();
            }
        }

        public List<User> GetAssignedToUsers(string ccid)
        {
            using (var context = new UABContext())
            {
                string fromemial = null;
                var workitem = context.WorkItem.Where(a => a.ClinicalCaseId == Convert.ToInt32(ccid)).FirstOrDefault();
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
                    return context.User.Where(a => !a.Email.Contains(fromemial)).ToList();
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

        public int AddUser(ApplicationUser user)
        {
            using (var context = new UABContext())
            {
                UAB.DAL.LoginDTO.IdentityServerContext Icontext = new IdentityServerContext();

                var iuser = Icontext.Users.Where(a => a.Email == user.Email).FirstOrDefault();
                var existing = context.User.Where(a => a.Email == user.Email).FirstOrDefault();

                if (existing == null)
                {
                    UAB.DAL.Models.User mdl = new User();
                    mdl.Email = user.Email;
                    mdl.FirstName = iuser.FirstName;
                    mdl.LastName = iuser.LastName;
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
                    throw new ArgumentException("Unable to Add project User:trying to add existing project to this user");
                }
            }
        }

        public int UpdateProjectUser(ApplicationUser projectuser)
        {
            using (var context = new UABContext())
            {
                var existingprojectuser = context.ProjectUser.First(a => a.ProjectUserId == projectuser.ProjectUserId);
                //there is no updation
                if (existingprojectuser.RoleId == projectuser.RoleId && existingprojectuser.SamplePercentage == Convert.ToInt32(projectuser.SamplePercentage))
                {
                    return 0;
                }
                //updating role 
                else if (existingprojectuser.RoleId != projectuser.RoleId && existingprojectuser.SamplePercentage == Convert.ToInt32(projectuser.SamplePercentage))
                {
                    existingprojectuser.RoleId = projectuser.RoleId;
                    context.Entry(existingprojectuser).State = EntityState.Modified;
                    context.SaveChanges();
                }
                //updating sample %
                else if (existingprojectuser.RoleId == projectuser.RoleId && existingprojectuser.SamplePercentage != Convert.ToInt32(projectuser.SamplePercentage))
                {
                    existingprojectuser.SamplePercentage = Convert.ToInt32(projectuser.SamplePercentage);
                    context.Entry(existingprojectuser).State = EntityState.Modified;
                    context.SaveChanges();
                }
                //updating role and sample %
                else if (existingprojectuser.RoleId != projectuser.RoleId && existingprojectuser.SamplePercentage != Convert.ToInt32(projectuser.SamplePercentage))
                {
                    existingprojectuser.RoleId = projectuser.RoleId;
                    existingprojectuser.SamplePercentage = Convert.ToInt32(projectuser.SamplePercentage);
                    context.Entry(existingprojectuser).State = EntityState.Modified;
                    context.SaveChanges();
                }
                return 1;
            }
        }

        public void DeletetProjectUser(int ProjectUserId)
        {
            using (var context = new UABContext())
            {
                var exsitingProjectuser = context.ProjectUser.Where(a => a.ProjectUserId == ProjectUserId).FirstOrDefault();

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
        public void DeletetUser(int UserId)
        {
            using (var context = new UABContext())
            {
                var exsitinguser = context.User.Where(a => a.UserId == UserId).FirstOrDefault();
                var exsitingProjectuser = context.ProjectUser.Where(a => a.UserId == UserId).FirstOrDefault();

                if (exsitingProjectuser != null)
                {
                    context.ProjectUser.Remove(exsitingProjectuser);
                    context.SaveChanges();
                }
                if (exsitinguser != null)
                {
                    context.User.Remove(exsitinguser);
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

                if (existingcategory != null)
                {
                    context.BlockCategory.Remove(existingcategory);
                    context.SaveChanges();
                }
                else
                {
                    throw new Exception("Unable To Delete Block Category");
                }

            }
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
                        project.CreatedDate = Convert.ToString(reader["CreatedDate"]);
                        project.InputFileLocation = Convert.ToString(reader["InputFileLocation"]);
                        project.InputFileFormat = Convert.ToString(reader["InputFileFormat"]);
                        project.ClientId = Convert.ToInt32(reader["ClientId"]);
                        project.ClientName = Convert.ToString(reader["ClientName"]);
                        project.ProjectTypeId = Convert.ToInt32(reader["ProjectTypeId"]);
                        project.ProjectTypeName = Convert.ToString(reader["ProjectTypeName"]);

                        lstProject.Add(project);
                    }
                }
            }
            return lstProject;
        }

        public void AddProject(ApplicationProject project)
        {
            using (var context = new UABContext())
            {
                UAB.DAL.Models.Project mdl = new Project();
                mdl.ClientId = project.ClientId;
                mdl.Name = project.Name;
                mdl.IsActive = project.IsActive;
                mdl.CreatedDate = DateTime.Now.ToString();
                mdl.InputFileLocation = project.InputFileLocation;
                mdl.InputFileFormat = project.InputFileFormat;
                mdl.ProjectTypeId = project.ProjectTypeId;

                context.Project.Add(mdl);
                context.SaveChanges();
            }
        }

        public void UpdateProject(ApplicationProject project)
        {
            using (var context = new UABContext())
            {

                UAB.DAL.Models.Project mdl = new Project();

                mdl.ProjectId = project.ProjectId;
                mdl.ClientId = project.ClientId;
                mdl.Name = project.Name;
                mdl.IsActive = project.IsActive;
                mdl.InputFileLocation = project.InputFileLocation;
                mdl.InputFileFormat = project.InputFileFormat;
                mdl.ProjectTypeId = project.ProjectTypeId;
                mdl.CreatedDate = project.CreatedDate;

                context.Entry(mdl).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void DeleteProject(ApplicationProject project)
        {
            using (var context = new UABContext())
            {
                using (var cnn = context.Database.GetDbConnection())
                {
                    var cmm = cnn.CreateCommand();
                    cmm.CommandType = System.Data.CommandType.StoredProcedure;
                    cmm.CommandText = "[dbo].[UspDeleteProject]";
                    cmm.Connection = cnn;

                    SqlParameter param = new SqlParameter();
                    param.ParameterName = "@ProjectId";
                    param.Value = project.ProjectId;
                    cmm.Parameters.Add(param);

                    cnn.Open();
                    cmm.ExecuteNonQuery();
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

        public void UploadAndSave(FileStream stream, int projectId, string fileName)
        {
            DataTable dtClinicalCase = new DataTable();
            dtClinicalCase.Columns.Add("ID", typeof(int));
            dtClinicalCase.Columns.Add("ProjectID", typeof(int));
            dtClinicalCase.Columns.Add("FileName", typeof(string));
            dtClinicalCase.Columns.Add("PatientMRN", typeof(string));
            dtClinicalCase.Columns.Add("PatientLastName", typeof(string));
            dtClinicalCase.Columns.Add("PatientFirstName", typeof(string));
            dtClinicalCase.Columns.Add("DateOfService", typeof(string));
            dtClinicalCase.Columns.Add("EncounterNumber", typeof(string));
            dtClinicalCase.Columns.Add("Provider", typeof(string));

            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                int i = 1;
                while (reader.Read())
                {
                    dtClinicalCase.Rows.Add(i, projectId, fileName, reader.GetValue(0).ToString(), reader.GetValue(5).ToString(), reader.GetValue(7).ToString(), reader.GetValue(10).ToString(), reader.GetValue(1).ToString(), reader.GetValue(31).ToString());
                    i += 1;
                }
            }

            using (UABContext dbContext = new UABContext())
            {
                using (var connection = dbContext.Database.GetDbConnection())
                {
                    connection.Open();

                    var cmd = connection.CreateCommand();
                    cmd.CommandText = "[dbo].[USPLoadData]";
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter param1 = new SqlParameter("@utInputData", SqlDbType.Structured);
                    param1.Value = dtClinicalCase;
                    param1.TypeName = "dbo.utInputData";
                    cmd.Parameters.Add(param1);

                    //SqlParameter param2 = new SqlParameter("@ProjectId", SqlDbType.Int);
                    //param2.Value = projectId;
                    //cmd.Parameters.Add(param2);

                    cmd.ExecuteReader();
                }
            }
        }
    }
}