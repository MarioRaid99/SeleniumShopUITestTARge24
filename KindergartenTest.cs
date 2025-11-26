using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Linq;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace SeleniumShopUITestTARge24
{
    [TestClass]
    public class KindergartenTests : TestBase
    {
        private void GoToKindergartenIndex()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Kindergarten");
        }

        private void WaitForKindergartenIndexLoaded()
        {
            Wait.Until(d =>
                d.Url.IndexOf("/kindergarten", StringComparison.OrdinalIgnoreCase) >= 0 &&
                d.FindElements(By.CssSelector("table")).Any()
            );
        }

        private void GoToKindergartenCreate()
        {
            GoToKindergartenIndex();
            WaitForKindergartenIndexLoaded();
            Driver.FindElement(By.LinkText("Create")).Click();
        }

        private void CreateKindergarten(
            string groupName,
            string childrenCount,
            string kindergartenName,
            string teacherName)
        {
            GoToKindergartenCreate();

            Driver.FindElement(By.Id("GroupName")).SendKeys(groupName);
            Driver.FindElement(By.Id("ChildrenCount")).SendKeys(childrenCount);
            Driver.FindElement(By.Id("KindergartenName")).SendKeys(kindergartenName);
            Driver.FindElement(By.Id("TeacherName")).SendKeys(teacherName);

            Driver.FindElement(By.CssSelector("input[type='submit'][value='Create']")).Click();

            WaitForKindergartenIndexLoaded();
        }

        private int GetKindergartenRowCount()
        {
            return Driver.FindElements(By.CssSelector("table tbody tr")).Count;
        }

        private void EnsureKindergartenRowExists()
        {
            GoToKindergartenIndex();
            WaitForKindergartenIndexLoaded();

            var rows = Driver.FindElements(By.CssSelector("table tbody tr"));
            if (rows.Count == 0)
            {
                CreateKindergarten("A1", "25", "Taheke", "Kott Panak");
            }
        }

        private string GetFirstRowChildrenCount()
        {
            GoToKindergartenIndex();
            WaitForKindergartenIndexLoaded();

            var cell = Driver.FindElement(By.CssSelector("table tbody tr td:nth-child(3)"));
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
        public void Can_Navigate_To_Kindergarten_Index()
        {
            GoToKindergartenIndex();
            WaitForKindergartenIndexLoaded();

            Assert.IsTrue(
                Driver.Url.IndexOf("/kindergarten", StringComparison.OrdinalIgnoreCase) >= 0,
                $"URL ei sisalda /kindergarten. Tegelik URL: {Driver.Url}");

            var createLink = Driver.FindElement(By.LinkText("Create"));
            Assert.IsNotNull(createLink);

            var table = Driver.FindElement(By.CssSelector("table"));
            Assert.IsNotNull(table);
        }

        // CREATE VALID – lisamine kehtivate andmetega
        [TestMethod]
        public void Can_Create_Kindergarten_With_Valid_Data()
        {
            GoToKindergartenIndex();
            WaitForKindergartenIndexLoaded();
            var rowsBefore = GetKindergartenRowCount();

            CreateKindergarten("A1", "25", "Taheke", "Kott Panak");

            var rowsAfter = GetKindergartenRowCount();

            Assert.IsTrue(
                rowsAfter > rowsBefore,
                $"Ridade arv ei suurenenud pärast kehtiva andmestikuga lisamist. Enne: {rowsBefore}, pärast: {rowsAfter}");

            var texts = Driver.FindElements(By.CssSelector("table tbody tr td")).Select(x => x.Text);
            Assert.IsTrue(texts.Contains("A1"));
        }

        // CREATE INVALID – lisamine vale väärtusega (numbrid puuduvad)
        [TestMethod]
        public void Cannot_Create_Kindergarten_With_Invalid_Data()
        {
            GoToKindergartenIndex();
            WaitForKindergartenIndexLoaded();
            var rowsBefore = GetKindergartenRowCount();

            GoToKindergartenCreate();

            Driver.FindElement(By.Id("GroupName")).SendKeys("B2");
            Driver.FindElement(By.Id("ChildrenCount")).SendKeys("-.-"); // vale väärtus (numbrid puuduvad)
            Driver.FindElement(By.Id("KindergartenName")).SendKeys("LendavMaja");
            Driver.FindElement(By.Id("TeacherName")).SendKeys("Mari");

            Driver.FindElement(By.CssSelector("input[type='submit'][value='Create']")).Click();

            GoToKindergartenIndex();
            WaitForKindergartenIndexLoaded();
            var rowsAfter = GetKindergartenRowCount();

            Assert.AreEqual(
                rowsBefore,
                rowsAfter,
                $"Vale väärtusega lisamisel ei tohi tekkida uut rida. Enne: {rowsBefore}, pärast: {rowsAfter}");
        }

        // DETAILS – vaatamine
        [TestMethod]
        public void Can_View_Kindergarten_Details()
        {
            EnsureKindergartenRowExists();

            GoToKindergartenIndex();
            WaitForKindergartenIndexLoaded();

            Driver.FindElement(By.LinkText("Details")).Click();

            var title = Driver.FindElement(By.TagName("h1")).Text;
            Assert.IsTrue(title.Contains("Details", StringComparison.OrdinalIgnoreCase));
        }

        // EDIT VALID – muutmine kehtivate andmetega
        [TestMethod]
        public void Can_Edit_Kindergarten_With_Valid_Data()
        {
            EnsureKindergartenRowExists();

            GoToKindergartenIndex();
            WaitForKindergartenIndexLoaded();

            Driver.FindElement(By.LinkText("Update")).Click();

            var groupInput = Wait.Until(d => d.FindElement(By.Id("GroupName")));
            groupInput.Clear();
            groupInput.SendKeys("UpdatedGroup");

            var countInput = Driver.FindElement(By.Id("ChildrenCount"));
            countInput.Clear();
            countInput.SendKeys("30");

            Driver.FindElement(By.CssSelector("input[type='submit']")).Click();

            WaitForKindergartenIndexLoaded();

            var texts = Driver.FindElements(By.CssSelector("table tbody tr td"))
                              .Select(x => x.Text);

            Assert.IsTrue(texts.Contains("UpdatedGroup"));
        }

        // EDIT INVALID – muutmine vale tüübiga (tekst arvuväljas)
        [TestMethod]
        public void Cannot_Edit_Kindergarten_With_Invalid_Data()
        {
            EnsureKindergartenRowExists();

            var oldCount = GetFirstRowChildrenCount();

            GoToKindergartenIndex();
            WaitForKindergartenIndexLoaded();

            Driver.FindElement(By.LinkText("Update")).Click();

            var countInput = Driver.FindElement(By.Id("ChildrenCount"));
            countInput.Clear();
            countInput.SendKeys("kakskümmend"); // vale tüüp (tekst arvuväljas)

            Driver.FindElement(By.CssSelector("input[type='submit']")).Click();

            var newCount = GetFirstRowChildrenCount();

            Assert.AreEqual(
                oldCount,
                newCount);
        }

        // DELETE – eemaldamine
        [TestMethod]
        public void Can_Delete_Kindergarten()
        {
            EnsureKindergartenRowExists();

            GoToKindergartenIndex();
            WaitForKindergartenIndexLoaded();

            var rowsBefore = GetKindergartenRowCount();
            Assert.IsTrue(rowsBefore > 0);

            Driver.FindElement(By.LinkText("Delete")).Click();

            Wait.Until(d =>
                d.FindElements(By.CssSelector("input[type='submit'][value='Delete']")).Any()
            );

            Driver.FindElement(By.CssSelector("input[type='submit'][value='Delete']")).Click();

            TryAcceptAlertIfPresent();

            WaitForKindergartenIndexLoaded();

            var rowsAfter = GetKindergartenRowCount();

            Assert.IsTrue(
                rowsAfter < rowsBefore,
                $"Pärast kustutamist peaks ridade arv vähenema. Enne: {rowsBefore}, pärast: {rowsAfter}");
        }
    }
}
