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
    public class ErrorTypeTesting
    {
        [Fact]
        public void ErrorTypeAddedSuccessfully()
        {
            //Arrange
            UABController controller = new UABController();
            var mockTempData = new Mock<ITempDataDictionary>();
            ClinicalcaseOperations clinicalCaseOperations = new ClinicalcaseOperations();
            controller.TempData = mockTempData.Object;
            ErrorType errorType = new ErrorType();
            errorType.Name = "Test" + RandomValue();

            //Act
            var result = controller.AddSettingsErrorType(errorType);

            //Assert
            var errorTypes = clinicalCaseOperations.GetErrorTypeNames();
            Assert.Contains<string>(errorType.Name.ToLower(), errorTypes);
        }

        [Fact]
        public void ErrorTypeUpdatedSuccessfully()
        {
            //Arrange
            UABController controller = new UABController();
            var mockTempData = new Mock<ITempDataDictionary>();
            ClinicalcaseOperations clinicalCaseOperations = new ClinicalcaseOperations();
            controller.TempData = mockTempData.Object;
            ErrorType errorType = new ErrorType();
            errorType.Name = "Test" + RandomValue();
            controller.AddSettingsErrorType(errorType);
            errorType = clinicalCaseOperations.GetErrorTypeByName(errorType.Name);
            errorType.Name = errorType.Name + "Update";

            //Act
            controller.AddSettingsErrorType(errorType);

            //Assert
            var errorTypes = clinicalCaseOperations.GetErrorTypeNames();
            Assert.Contains<string>(errorType.Name.ToLower(), errorTypes);
        }

        [Fact]
        public void ErrorTypeDeletedSuccessfully()
        {
            //Arrange
            UABController controller = new UABController();
            var mockTempData = new Mock<ITempDataDictionary>();
            ClinicalcaseOperations clinicalCaseOperations = new ClinicalcaseOperations();
            controller.TempData = mockTempData.Object;
            ErrorType errorType = new ErrorType();
            errorType.Name = "Test" + RandomValue();
            controller.AddSettingsErrorType(errorType);
            errorType = clinicalCaseOperations.GetErrorTypeByName(errorType.Name);

            //Act
            controller.DeleteErrorType(errorType);

            //Assert
            var errorTypes = clinicalCaseOperations.GetErrorTypeNames();
            Assert.DoesNotContain<string>(errorType.Name.ToLower(), errorTypes);
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
