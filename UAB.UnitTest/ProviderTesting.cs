using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using UAB.Controllers;
using UAB.DAL.Models;
using UAB.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace UAB.UnitTest
{
    public class ProviderTesting
    {
        [Fact]
        public void ProviderAddedSuccessfully()
        {
            //Arrange
            UABController controller = new UABController(null);
            var mockTempData = new Mock<ITempDataDictionary>();
            ClinicalcaseOperations clinicalCaseOperations = new ClinicalcaseOperations();
            controller.TempData = mockTempData.Object;
            Provider provider = new Provider();
            provider.Name = "Test"+ RandomValue();

            //Act
            var result = controller.AddSettingsProvider(provider);

            //Assert
            var providers = clinicalCaseOperations.GetProviderNames();
            Assert.Contains<string>(provider.Name.ToLower(),providers);
        }

        [Fact]
        public void ProviderAdditionFailed()
        {
            //Arrange
            UABController controller = new UABController(null);
            var mockTempData = new Mock<ITempDataDictionary>();
            ClinicalcaseOperations clinicalCaseOperations = new ClinicalcaseOperations();
            controller.TempData = mockTempData.Object;
            Provider provider = new Provider();
            provider.Name = "$%^$&";

            //Act
            var result = controller.AddSettingsProvider(provider);

            //Assert
            var providers = clinicalCaseOperations.GetProviderNames();
            Assert.Contains<string>(provider.Name.ToLower(), providers);
        }

        [Fact]
        public void ProviderUpdatedSuccessfully()
        {
            //Arrange
            UABController controller = new UABController(null);
            var mockTempData = new Mock<ITempDataDictionary>();
            ClinicalcaseOperations clinicalCaseOperations = new ClinicalcaseOperations();
            controller.TempData = mockTempData.Object;
            Provider provider = new Provider();
            provider.Name = "Test" + RandomValue();
            controller.AddSettingsProvider(provider);
            provider = clinicalCaseOperations.GetProviderByName(provider.Name);
            provider.Name = provider.Name + "Update";

            //Act
            controller.AddSettingsProvider(provider);

            //Assert
            var providers = clinicalCaseOperations.GetProviderNames();
            Assert.Contains<string>(provider.Name.ToLower(), providers);
        }

        [Fact]
        public void ProviderDeletedSuccessfully()
        {
            //Arrange
            UABController controller = new UABController(null);
            var mockTempData = new Mock<ITempDataDictionary>();
            ClinicalcaseOperations clinicalCaseOperations = new ClinicalcaseOperations();
            controller.TempData = mockTempData.Object;
            Provider provider = new Provider();
            provider.Name = "Test" + RandomValue();
            controller.AddSettingsProvider(provider);
            provider = clinicalCaseOperations.GetProviderByName(provider.Name);

            //Act
            controller.DeleteProvider(provider);

            //Assert
            var providers = clinicalCaseOperations.GetProviderNames();
            Assert.DoesNotContain<string>(provider.Name.ToLower(), providers);
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
