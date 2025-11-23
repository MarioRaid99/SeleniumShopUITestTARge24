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


        // NAVIGEERIMINE
        [TestMethod]
        public void Can_Navigate_To_Kindergarten_Index()
        {
            GoToKindergartenIndex();

            Assert.IsTrue(Driver.Url.Contains("/Kindergarten"));

            var createLink = Driver.FindElement(By.LinkText("Create"));
            Assert.IsNotNull(createLink);
        }

        // CREATE VALID
        [TestMethod]
        public void Can_Create_Kindergarten_With_Valid_Data()
        {
            GoToKindergartenIndex();
            Driver.FindElement(By.LinkText("Create")).Click();

            Driver.FindElement(By.Id("GroupName")).SendKeys("A1");
            Driver.FindElement(By.Id("ChildrenCount")).SendKeys("25");
            Driver.FindElement(By.Id("KindergartenName")).SendKeys("Täheke123");
            Driver.FindElement(By.Id("TeacherName")).SendKeys("KatiMati");

            Driver.FindElement(By.CssSelector("input[type='submit'][value='Create']")).Click();

            Assert.IsTrue(Driver.Url.Contains("/Kindergarten"));

            var texts = Driver.FindElements(By.CssSelector("table tbody tr td")).Select(x => x.Text);
            Assert.IsTrue(texts.Contains("A1"));
        }

        // CREATE INVALID
        [TestMethod]
        public void Cannot_Create_Kindergarten_With_Invalid_Data()
        {
            GoToKindergartenIndex();

            var rowsBefore = Driver.FindElements(By.CssSelector("table tbody tr")).Count;

            Driver.FindElement(By.LinkText("Create")).Click();

            Driver.FindElement(By.Id("GroupName")).SendKeys("B2");
            Driver.FindElement(By.Id("ChildrenCount")).SendKeys("viis"); // vale tüüp!!!
            Driver.FindElement(By.Id("KindergartenName")).SendKeys("LendavMaja");
            Driver.FindElement(By.Id("TeacherName")).SendKeys("Mari123");

            Driver.FindElement(By.CssSelector("input[type='submit'][value='Create']")).Click();

            GoToKindergartenIndex();
            var rowsAfter = Driver.FindElements(By.CssSelector("table tbody tr")).Count;

            Assert.AreEqual(rowsBefore, rowsAfter);
        }

        // DETAILS
        [TestMethod]
        public void Can_View_Kindergarten_Details()
        {
            GoToKindergartenIndex();

            var rows = Driver.FindElements(By.CssSelector("table tbody tr"));
            if (rows.Count == 0)
            {
                Can_Create_Kindergarten_With_Valid_Data();
                GoToKindergartenIndex();
            }

            Driver.FindElement(By.LinkText("Details")).Click();

            var title = Driver.FindElement(By.TagName("h1")).Text;
            Assert.IsTrue(title.Contains("Details"));
        }

        // EDIT VALID
        [TestMethod]
        public void Can_Edit_Kindergarten_With_Valid_Data()
        {
            GoToKindergartenIndex();

            var rows = Driver.FindElements(By.CssSelector("table tbody tr"));
            if (rows.Count == 0)
            {
                Can_Create_Kindergarten_With_Valid_Data();
                GoToKindergartenIndex();
            }

            var updateLink = Driver.FindElement(By.LinkText("Update"));
            var href = updateLink.GetAttribute("href");
            Driver.Navigate().GoToUrl(href);

            var groupInput = Wait.Until(d => d.FindElement(By.Id("GroupName")));
            groupInput.Clear();
            groupInput.SendKeys("UpdatedGroup");

            var countInput = Driver.FindElement(By.Id("ChildrenCount"));
            countInput.Clear();
            countInput.SendKeys("30");

            Driver.FindElement(By.CssSelector("input[type='submit']")).Click();

            GoToKindergartenIndex();

            var texts = Driver.FindElements(By.CssSelector("table tbody tr td"))
                              .Select(x => x.Text);
            Assert.IsTrue(texts.Contains("UpdatedGroup"));
        }

        // EDIT INVALID
        [TestMethod]
        public void Cannot_Edit_Kindergarten_With_Invalid_Data()
        {
            GoToKindergartenIndex();

            var rows = Driver.FindElements(By.CssSelector("table tbody tr"));
            if (rows.Count == 0)
            {
                Can_Create_Kindergarten_With_Valid_Data();
                GoToKindergartenIndex();
            }

            var oldCount = Driver.FindElement(By.CssSelector("table tbody tr td:nth-child(3)")).Text;

            Driver.FindElement(By.LinkText("Update")).Click();

            var countInput = Driver.FindElement(By.Id("ChildrenCount"));
            countInput.Clear();
            countInput.SendKeys("kakskümmend"); // VALE TÜÜP

            Driver.FindElement(By.CssSelector("input[type='submit']")).Click();

            GoToKindergartenIndex();

            var newCount = Driver.FindElement(By.CssSelector("table tbody tr td:nth-child(3)")).Text;

            Assert.AreEqual(oldCount, newCount);
        }

        // DELETE
        [TestMethod]
        public void Can_Delete_Kindergarten()
        {
            GoToKindergartenIndex();

            var rowsBefore = Driver.FindElements(By.CssSelector("table tbody tr")).Count;

            if (rowsBefore == 0)
            {
                Can_Create_Kindergarten_With_Valid_Data();
                GoToKindergartenIndex();
                rowsBefore = Driver.FindElements(By.CssSelector("table tbody tr")).Count;
            }

            Driver.FindElement(By.LinkText("Delete")).Click();
            Driver.FindElement(By.CssSelector("input[type='submit'][value='Delete']")).Click();

            var rowsAfter = Driver.FindElements(By.CssSelector("table tbody tr")).Count;

            Assert.IsTrue(rowsAfter < rowsBefore);
        }
    }
}
