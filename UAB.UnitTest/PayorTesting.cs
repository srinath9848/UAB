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
    public class PayorTesting
    {
        [Fact]
        public void PayorAddedSuccessfully()
        {
            //Arrange
            UABController controller = new UABController();
            var mockTempData = new Mock<ITempDataDictionary>();
            ClinicalcaseOperations clinicalCaseOperations = new ClinicalcaseOperations();
            controller.TempData = mockTempData.Object;
            Payor payor = new Payor();
            payor.Name = "Test" + RandomValue();

            //Act
            var result = controller.AddSettingsPayor(payor);

            //Assert
            var payors = clinicalCaseOperations.GetPayorNames();
            Assert.Contains<string>(payor.Name.ToLower(), payors);
        }

        [Fact]
        public void PayorUpdatedSuccessfully()
        {
            //Arrange
            UABController controller = new UABController();
            var mockTempData = new Mock<ITempDataDictionary>();
            ClinicalcaseOperations clinicalCaseOperations = new ClinicalcaseOperations();
            controller.TempData = mockTempData.Object;
            Payor payor = new Payor();
            payor.Name = "Test" + RandomValue();
            controller.AddSettingsPayor(payor);
            payor = clinicalCaseOperations.GetPayorByName(payor.Name);
            payor.Name = payor.Name + "Update";

            //Act
            controller.AddSettingsPayor(payor);

            //Assert
            var payors = clinicalCaseOperations.GetPayorNames();
            Assert.Contains<string>(payor.Name.ToLower(), payors);
        }

        [Fact]
        public void PayorDeletedSuccessfully()
        {
            //Arrange
            UABController controller = new UABController();
            var mockTempData = new Mock<ITempDataDictionary>();
            ClinicalcaseOperations clinicalCaseOperations = new ClinicalcaseOperations();
            controller.TempData = mockTempData.Object;
            Payor payor = new Payor();
            payor.Name = "Test" + RandomValue();
            controller.AddSettingsPayor(payor);
            payor = clinicalCaseOperations.GetPayorByName(payor.Name);

            //Act
            controller.DeletePayor(payor);

            //Assert
            var payors = clinicalCaseOperations.GetPayorNames();
            Assert.DoesNotContain<string>(payor.Name.ToLower(), payors);
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
