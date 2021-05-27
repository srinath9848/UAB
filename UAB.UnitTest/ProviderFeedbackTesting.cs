using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using UAB.Controllers;
using UAB.DAL.Models;
using UAB.DAL;
using UAB.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace UAB.UnitTest
{
    public class ProviderFeedbackTesting
    {
        [Fact]
        public void ProviderFeedbackAddedSuccessfully()
        {
            //Arrange
            UABController controller = new UABController(null, null);
            var mockTempData = new Mock<ITempDataDictionary>();
            ClinicalcaseOperations clinicalCaseOperations = new ClinicalcaseOperations();
            controller.TempData = mockTempData.Object;
            BindDTO providerFeedback = new BindDTO();
            providerFeedback.Name = "Test" + RandomValue();

            //Act
            var result = controller.AddSettingsProviderFeedback(providerFeedback);

            //Assert
            var providerFeedbacks = clinicalCaseOperations.GetProviderFeedbackNames();
            Assert.Contains<string>(providerFeedback.Name.ToLower(), providerFeedbacks);
        }

        [Fact]
        public void ProviderFeedbackUpdatedSuccessfully()
        {
            //Arrange
            UABController controller = new UABController(null, null);
            var mockTempData = new Mock<ITempDataDictionary>();
            ClinicalcaseOperations clinicalCaseOperations = new ClinicalcaseOperations();
            controller.TempData = mockTempData.Object;
            ProviderFeedback providerFeedback = new ProviderFeedback();
            BindDTO feedback = new BindDTO();
            feedback.Name = "Test" + RandomValue();
            controller.AddSettingsProviderFeedback(feedback);
            providerFeedback = clinicalCaseOperations.GetProviderFeedbackByName(feedback.Name);
            feedback.ID = providerFeedback.ProviderFeedbackId;
            feedback.Name = providerFeedback.Feedback + "Update";

            //Act
            controller.AddSettingsProviderFeedback(feedback);

            //Assert
            var providerFeedbacks = clinicalCaseOperations.GetProviderFeedbackNames();
            Assert.Contains<string>(feedback.Name.ToLower(), providerFeedbacks);
        }

        [Fact]
        public void ProviderFeedbackDeletedSuccessfully()
        {
            //Arrange
            UABController controller = new UABController(null, null);
            var mockTempData = new Mock<ITempDataDictionary>();
            ClinicalcaseOperations clinicalCaseOperations = new ClinicalcaseOperations();
            controller.TempData = mockTempData.Object;
            ProviderFeedback providerFeedback = new ProviderFeedback();
            BindDTO feedback = new BindDTO();
            feedback.Name = "Test" + RandomValue();
            controller.AddSettingsProviderFeedback(feedback);
            providerFeedback = clinicalCaseOperations.GetProviderFeedbackByName(feedback.Name);
            feedback.ID = providerFeedback.ProviderFeedbackId;

            //Act
            controller.DeleteProviderFeedback(feedback);

            //Assert
            var providerFeedbacks = clinicalCaseOperations.GetProviderFeedbackNames();
            Assert.DoesNotContain<string>(feedback.Name.ToLower(), providerFeedbacks);
        }

        private string RandomValue()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }
    }
}
