using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using UAB.Controllers;
using UAB.DAL.Models;
using UAB.DAL;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace UAB.UnitTest
{
    public class ProjectTesting
    {
        CommonTestMethod commonTestMethod = new CommonTestMethod();

        [Fact]
        public void ProjectAddedSuccessfully()
        {
            //Arrange
            UABController controller = new UABController();
            var mockTempData = new Mock<ITempDataDictionary>();
            ClinicalcaseOperations clinicalCaseOperations = new ClinicalcaseOperations();
            controller.TempData = mockTempData.Object;
            ApplicationProject project = new ApplicationProject();
            project.Name = "Test" + commonTestMethod.RandomValue();
            project.ProjectTypeId = commonTestMethod.GetFirstProjectTypeId();
            project.ClientId = commonTestMethod.GetFirstClintId();

            //Act
            var result = controller.AddSettingsProject(project);

            //Assert
            var projects = clinicalCaseOperations.GetProjectNames();
            Assert.Contains<string>(project.Name.ToLower(), projects);
        }

        [Fact]
        public void ProjectUpdatedSuccessfully()
        {
            //Arrange
            UABController controller = new UABController();
            var mockTempData = new Mock<ITempDataDictionary>();
            ClinicalcaseOperations clinicalCaseOperations = new ClinicalcaseOperations();
            controller.TempData = mockTempData.Object;
            Project proj = new Project();
            ApplicationProject project = new ApplicationProject();
            project.Name = "Test" + commonTestMethod.RandomValue();
            project.ProjectTypeId = commonTestMethod.GetFirstProjectTypeId();
            project.ClientId = commonTestMethod.GetFirstClintId();
            controller.AddSettingsProject(project);
            var pro = clinicalCaseOperations.GetProjectsList();
            project = pro.Where(p => p.Name == project.Name).FirstOrDefault();
            project.Name = project.Name + "Update";

            //Act
            controller.AddSettingsProject(project);

            //Assert
            var projects = clinicalCaseOperations.GetProjectNames();
            Assert.Contains<string>(project.Name.ToLower(), projects);
        }

        [Fact]
        public void ProjectDeletedSuccessfully()
        {
            //Arrange
            UABController controller = new UABController();
            var mockTempData = new Mock<ITempDataDictionary>();
            ClinicalcaseOperations clinicalCaseOperations = new ClinicalcaseOperations();
            controller.TempData = mockTempData.Object;
            Project proj = new Project();
            ApplicationProject project = new ApplicationProject();
            project.Name = "Test" + commonTestMethod.RandomValue();
            project.ProjectTypeId = commonTestMethod.GetFirstProjectTypeId();
            project.ClientId = commonTestMethod.GetFirstClintId();
            controller.AddSettingsProject(project);
            var pro = clinicalCaseOperations.GetProjectsList();
            project = pro.Where(p => p.Name == project.Name).FirstOrDefault();

            //Act
            controller.DeleteProject(project);

            //Assert
            var projects = clinicalCaseOperations.GetProjectNames();
            Assert.DoesNotContain<string>(project.Name.ToLower(), projects);
        }

        //private string RandomValue()
        //{
        //    var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        //    var stringChars = new char[8];
        //    var random = new Random();

        //    for (int i = 0; i < stringChars.Length; i++)
        //    {
        //        stringChars[i] = chars[random.Next(chars.Length)];
        //    }

        //    var finalString = new String(stringChars);
        //    return finalString;
        //}
    }
}
