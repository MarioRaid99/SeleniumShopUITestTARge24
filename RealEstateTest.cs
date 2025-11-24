using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Linq;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace SeleniumShopUITestTARge24
{
    [TestClass]
    public class RealEstateTests : TestBase
    {
        private void GoToRealEstateIndex()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/RealEstate");
        }

        // NAVIGEERIMINE
        [TestMethod]
        public void Can_Navigate_To_RealEstate_Index()
        {
            GoToRealEstateIndex();
            Assert.IsTrue(Driver.Url.Contains("/RealEstate"));

            var createLink = Driver.FindElement(By.LinkText("Create"));
            Assert.IsNotNull(createLink);
        }

        // CREATE VALID
        [TestMethod]
        public void Can_Create_RealEstate_With_Valid_Data()
        {
            GoToRealEstateIndex();
            Driver.FindElement(By.LinkText("Create")).Click();

            Driver.FindElement(By.Id("Area")).SendKeys("120");
            Driver.FindElement(By.Id("Location")).SendKeys("Tallinn1");
            Driver.FindElement(By.Id("RoomNumber")).SendKeys("4");
            Driver.FindElement(By.Id("BuildingType")).SendKeys("Apartment");

            Driver.FindElement(By.CssSelector("input[type='submit'][value='Create']")).Click();

            Assert.IsTrue(Driver.Url.Contains("/RealEstate"));

            var texts = Driver.FindElements(By.CssSelector("table tbody tr td")).Select(x => x.Text);
            Assert.IsTrue(texts.Contains("120"));
        }

        // CREATE INVALID
        [TestMethod]
        public void Cannot_Create_RealEstate_With_Invalid_Data()
        {
            GoToRealEstateIndex();
            var rowsBefore = Driver.FindElements(By.CssSelector("table tbody tr")).Count;

            Driver.FindElement(By.LinkText("Create")).Click();

            Driver.FindElement(By.Id("Area")).SendKeys("kolmsada"); // vale tüüp!
            Driver.FindElement(By.Id("Location")).SendKeys("Tartu2");
            Driver.FindElement(By.Id("RoomNumber")).SendKeys("-200"); // vale tüüp!
            Driver.FindElement(By.Id("BuildingType")).SendKeys("House");

            Driver.FindElement(By.CssSelector("input[type='submit'][value='Create']")).Click();

            GoToRealEstateIndex();
            var rowsAfter = Driver.FindElements(By.CssSelector("table tbody tr")).Count;

            Assert.AreEqual(rowsBefore, rowsAfter);
        }

        // DETAILS
        [TestMethod]
        public void Can_View_RealEstate_Details()
        {
            GoToRealEstateIndex();

            var rows = Driver.FindElements(By.CssSelector("table tbody tr"));
            if (rows.Count == 0)
            {
                Can_Create_RealEstate_With_Valid_Data();
                GoToRealEstateIndex();
            }

            Driver.FindElement(By.LinkText("Details")).Click();
            var title = Driver.FindElement(By.TagName("h1")).Text;
            Assert.IsTrue(title.Contains("Details"));
        }

        // EDIT VALID
        [TestMethod]
        public void Can_Edit_RealEstate_With_Valid_Data()
        {
            GoToRealEstateIndex();

            var rows = Driver.FindElements(By.CssSelector("table tbody tr"));
            if (rows.Count == 0)
            {
                Can_Create_RealEstate_With_Valid_Data();
                GoToRealEstateIndex();
                rows = Driver.FindElements(By.CssSelector("table tbody tr"));
            }

            var updateLink = rows[0].FindElement(By.LinkText("Update"));
            var href = updateLink.GetAttribute("href");
            Driver.Navigate().GoToUrl(href);

            var areaInput = Wait.Until(d => d.FindElement(By.Id("Area")));
            var roomInput = Driver.FindElement(By.Id("RoomNumber"));
            var buildingTypeInput = Driver.FindElement(By.Id("BuildingType"));
            var locationInput = Driver.FindElement(By.Id("Location"));

            areaInput.Clear();
            areaInput.SendKeys("150");
            roomInput.Clear();
            roomInput.SendKeys("5");
            buildingTypeInput.Clear();
            buildingTypeInput.SendKeys("Apartment");
            locationInput.Clear();
            locationInput.SendKeys("Tallinn");

            Driver.FindElement(By.CssSelector("input[type='submit']")).Click();

            Wait.Until(d => d.Url.Contains("/RealEstate"));

            var tableCells = Wait.Until(d => d.FindElements(By.CssSelector("table tbody tr td")));

            var texts = tableCells.Select(x => x.Text);

            Assert.IsTrue(texts.Contains("150"), "Uus Area väärtus ei ilmu tabelisse.");
            Assert.IsTrue(texts.Contains("5"), "Uus RoomNumber väärtus ei ilmu tabelisse.");
            Assert.IsTrue(texts.Contains("Apartment"), "Uus BuildingType ei ilmu tabelisse.");
            Assert.IsTrue(texts.Contains("Tallinn"), "Uus Location ei ilmu tabelisse.");
        }

        // EDIT INVALID
        [TestMethod]
        public void Cannot_Edit_RealEstate_With_Invalid_Data()
        {
            GoToRealEstateIndex();

            var rows = Driver.FindElements(By.CssSelector("table tbody tr"));
            if (rows.Count == 0)
            {
                Can_Create_RealEstate_With_Valid_Data();
                GoToRealEstateIndex();
            }

            var oldArea = Driver.FindElement(By.CssSelector("table tbody tr td:nth-child(2)")).Text;

            Driver.FindElement(By.LinkText("Update")).Click();
            var areaInput = Driver.FindElement(By.Id("Area"));
            areaInput.Clear();
            areaInput.SendKeys("suurtuba"); // vale tüüp!

            Driver.FindElement(By.CssSelector("input[type='submit']")).Click();

            GoToRealEstateIndex();
            var newArea = Driver.FindElement(By.CssSelector("table tbody tr td:nth-child(2)")).Text;

            Assert.AreEqual(oldArea, newArea);
        }

        // DELETE
        [TestMethod]
        public void Can_Delete_RealEstate()
        {
            GoToRealEstateIndex();

            var rowsBefore = Driver.FindElements(By.CssSelector("table tbody tr")).Count;
            if (rowsBefore == 0)
            {
                Can_Create_RealEstate_With_Valid_Data();
                GoToRealEstateIndex();
                rowsBefore = Driver.FindElements(By.CssSelector("table tbody tr")).Count;
            }

            Driver.FindElement(By.LinkText("Delete")).Click();
            Driver.FindElement(By.CssSelector("input[type='submit'][value='Delete']")).Click();

            var rowsAfter = Driver.FindElements(By.CssSelector("table tbody tr")).Count;
            Assert.IsTrue(rowsAfter < rowsBefore);
        }
    }
}