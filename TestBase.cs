using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;

namespace SeleniumShopUITestTARge24
{
    public abstract class TestBase
    {
        protected IWebDriver Driver;
        protected WebDriverWait Wait;

        protected string BaseUrl = "https://localhost:7282";

        [TestInitialize]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--incognito");
            options.AddArgument("--disable-infobars");

            Driver = new ChromeDriver(options);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));

            GoHome();
        }

        [TestCleanup]
        public void Teardown()
        {
            try
            {
                Driver.Quit();
            }
            catch (Exception)
            {
                // ignore error if unable to quit
            }
        }

        protected void GoHome()
        {
            Driver.Navigate().GoToUrl(BaseUrl);
        }

        protected void SetDateTimeLocal(string elementId, DateTime dt)
        {
            var formatted = dt.ToString("yyyy-MM-ddTHH:mm");
            var element = Wait.Until(d => d.FindElement(By.Id(elementId)));

            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            js.ExecuteScript("arguments[0].value = arguments[1]", element, formatted);
        }

        protected string GetScreenshotPath(string fileName)
        {
            var dir = "Screenshots";
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            return System.IO.Path.Combine(dir, $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
        }

        protected void TakeScreenshot(string fileName)
        {
            var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
            var path = GetScreenshotPath(fileName);
            screenshot.SaveAsFile(path);
        }
    }
}