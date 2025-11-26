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

        private void WaitForRealEstateIndexLoaded()
        {
            Wait.Until(d =>
                d.Url.IndexOf("/realestate", StringComparison.OrdinalIgnoreCase) >= 0 &&
                d.FindElements(By.CssSelector("table")).Any()
            );
        }

        private void GoToRealEstateCreate()
        {
            GoToRealEstateIndex();
            WaitForRealEstateIndexLoaded();
            Driver.FindElement(By.LinkText("Create")).Click();
        }

        private void CreateRealEstate(
            string area,
            string location,
            string roomNumber,
            string buildingType)
        {
            GoToRealEstateCreate();

            Driver.FindElement(By.Id("Area")).SendKeys(area);
            Driver.FindElement(By.Id("Location")).SendKeys(location);
            Driver.FindElement(By.Id("RoomNumber")).SendKeys(roomNumber);
            Driver.FindElement(By.Id("BuildingType")).SendKeys(buildingType);

            Driver.FindElement(By.CssSelector("input[type='submit'][value='Create']")).Click();

            WaitForRealEstateIndexLoaded();
        }

        private int GetRealEstateRowCount()
        {
            return Driver.FindElements(By.CssSelector("table tbody tr")).Count;
        }

        private void EnsureRealEstateRowExists()
        {
            GoToRealEstateIndex();
            WaitForRealEstateIndexLoaded();

            var rows = Driver.FindElements(By.CssSelector("table tbody tr"));
            if (rows.Count == 0)
            {
                CreateRealEstate("120", "Tallinn1", "4", "Apartment");
            }
        }

        private string GetFirstRowArea()
        {
            GoToRealEstateIndex();
            WaitForRealEstateIndexLoaded();

            var cell = Driver.FindElement(By.CssSelector("table tbody tr td:nth-child(2)"));
            return cell.Text;
        }

        private void TryAcceptAlertIfPresent()
        {
            try
            {
                var alert = Driver.SwitchTo().Alert();
                alert.Accept();
            }
            catch (NoAlertPresentException)
            {
                
            }
        }

        // NAVIGEERIMINE
        [TestMethod]
        public void Can_Navigate_To_RealEstate_Index()
        {
            GoToRealEstateIndex();
            WaitForRealEstateIndexLoaded();

            Assert.IsTrue(
                Driver.Url.IndexOf("/realestate", StringComparison.OrdinalIgnoreCase) >= 0,
                $"URL ei sisalda /realestate. Tegelik URL: {Driver.Url}");

            var createLink = Driver.FindElement(By.LinkText("Create"));
            Assert.IsNotNull(createLink, "Create linki ei leitud");

            var table = Driver.FindElement(By.CssSelector("table"));
            Assert.IsNotNull(table, "RealEstate tabelit ei leitud Index vaates");
        }


        [TestMethod]
        public void Can_Create_RealEstate_With_Valid_Data()
        {
            GoToRealEstateIndex();
            WaitForRealEstateIndexLoaded();
            var rowsBefore = GetRealEstateRowCount();

            CreateRealEstate("120", "Tallinn1", "4", "Apartment");

            var rowsAfter = GetRealEstateRowCount();

            Assert.IsTrue(
                rowsAfter > rowsBefore,
                $"Ridade arv ei suurenenud pärast kehtiva andmestikuga lisamist. Enne: {rowsBefore}, pärast: {rowsAfter}");

            var texts = Driver.FindElements(By.CssSelector("table tbody tr td"))
                              .Select(x => x.Text);

            Assert.IsTrue(texts.Contains("120"), "Loodud RealEstate (Area=120) ei ole tabelis näha");
        }

        [TestMethod]
        public void Cannot_Create_RealEstate_With_Invalid_Data()
        {
            GoToRealEstateIndex();
            WaitForRealEstateIndexLoaded();
            var rowsBefore = GetRealEstateRowCount();

            GoToRealEstateCreate();

            Driver.FindElement(By.Id("Area")).SendKeys("kolmsada");
            Driver.FindElement(By.Id("Location")).SendKeys("Tartu2");
            Driver.FindElement(By.Id("RoomNumber")).SendKeys("-.-");    // vale tüüp
            Driver.FindElement(By.Id("BuildingType")).SendKeys("House");

            Driver.FindElement(By.CssSelector("input[type='submit'][value='Create']")).Click();

            GoToRealEstateIndex();
            WaitForRealEstateIndexLoaded();
            var rowsAfter = GetRealEstateRowCount();

            Assert.AreEqual(
                rowsBefore,
                rowsAfter,
                $"Vale tüübiga/väärtusega lisamisel ei tohi tekkida uut rida. Enne: {rowsBefore}, pärast: {rowsAfter}");
        }

        // DETAILS – vaatamine
        [TestMethod]
        public void Can_View_RealEstate_Details()
        {
            EnsureRealEstateRowExists();

            GoToRealEstateIndex();
            WaitForRealEstateIndexLoaded();

            Driver.FindElement(By.LinkText("Details")).Click();
            var title = Driver.FindElement(By.TagName("h1")).Text;

            Assert.IsTrue(
                title.Contains("Details", StringComparison.OrdinalIgnoreCase),
                "Details lehe pealkiri ei sisalda 'Details'");
        }

        // EDIT VALID – muutmine kehtivate andmetega
        [TestMethod]
        public void Can_Edit_RealEstate_With_Valid_Data()
        {
            EnsureRealEstateRowExists();

            GoToRealEstateIndex();
            WaitForRealEstateIndexLoaded();

            var firstRow = Driver.FindElements(By.CssSelector("table tbody tr")).First();
            var updateLink = firstRow.FindElement(By.LinkText("Update"));
            updateLink.Click();

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

            WaitForRealEstateIndexLoaded();

            var tableCells = Driver.FindElements(By.CssSelector("table tbody tr td"));
            var texts = tableCells.Select(x => x.Text);

            Assert.IsTrue(texts.Contains("150"), "Uus Area väärtus ei ilmu tabelisse.");
            Assert.IsTrue(texts.Contains("5"), "Uus RoomNumber väärtus ei ilmu tabelisse.");
            Assert.IsTrue(texts.Contains("Apartment"), "Uus BuildingType ei ilmu tabelisse.");
        }

        // EDIT INVALID – muutmine vale tüübiga (tekst arvuväljas)
        [TestMethod]
        public void Cannot_Edit_RealEstate_With_Invalid_Data()
        {
            EnsureRealEstateRowExists();

            var oldArea = GetFirstRowArea();

            GoToRealEstateIndex();
            WaitForRealEstateIndexLoaded();

            Driver.FindElement(By.LinkText("Update")).Click();

            var areaInput = Driver.FindElement(By.Id("RoomNumber"));
            areaInput.Clear();
            areaInput.SendKeys("-.-"); // vale tüüp (tekst arvuväljas)

            Driver.FindElement(By.CssSelector("input[type='submit']")).Click();

            var newArea = GetFirstRowArea();

            Assert.AreEqual(
                oldArea,
                newArea,
                "Vale tüübiga muutmine ei tohi muuta Area väärtust");
        }

        // DELETE – eemaldamine
        [TestMethod]
        public void Can_Delete_RealEstate()
        {
            EnsureRealEstateRowExists();

            GoToRealEstateIndex();
            WaitForRealEstateIndexLoaded();

            var rowsBefore = GetRealEstateRowCount();
            Assert.IsTrue(rowsBefore > 0, "Kustutamise testi eeldus: peab olema vähemalt üks rida.");

            Driver.FindElement(By.LinkText("Delete")).Click();

            Wait.Until(d =>
                d.FindElements(By.CssSelector("input[type='submit'][value='Delete']")).Any()
            );

            Driver.FindElement(By.CssSelector("input[type='submit'][value='Delete']")).Click();

            TryAcceptAlertIfPresent();

            WaitForRealEstateIndexLoaded();

            var rowsAfter = GetRealEstateRowCount();

            Assert.IsTrue(
                rowsAfter < rowsBefore,
                $"Pärast kustutamist peaks ridade arv vähenema. Enne: {rowsBefore}, pärast: {rowsAfter}");
        }
    }
}