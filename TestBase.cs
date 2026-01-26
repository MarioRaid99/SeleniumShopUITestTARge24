using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

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

            Login();
        }

        [TestCleanup]
        public void Teardown()
        {
            try
            {
                Logout();
            }
            catch
            {
            }

            try
            {
            }
            catch (Exception)
            {
            }
        }

        protected void GoHome()
        {
            Driver.Navigate().GoToUrl(BaseUrl);
        }

        protected virtual void Login()
        {
            Driver.Navigate().GoToUrl($"{BaseUrl}/Accounts/Login");

            var emailInput = Wait.Until(d => d.FindElement(By.Id("Email")));
            emailInput.Clear();
            emailInput.SendKeys("MarioRaid@gmail.com");

            var passwordInput = Driver.FindElement(By.Id("Password"));
            passwordInput.Clear();
            passwordInput.SendKeys("MarioRaid99!");

            var loginButton = Driver.FindElement(By.CssSelector("button[type='submit']"));
            loginButton.Click();

            Wait.Until(d =>
                !d.Url.Contains("/Accounts/Login", StringComparison.OrdinalIgnoreCase) ||
                d.FindElements(By.LinkText("Logout")).Any()
            );
        }

        protected virtual void Logout()
        {
            try
            {
                GoHome();

                var logoutLinks = Driver.FindElements(By.LinkText("Logout"));
                if (logoutLinks.Any())
                {
                    logoutLinks.First().Click();
                    return;
                }

                var logoutButtons = Driver.FindElements(
                    By.XPath("//button[contains(., 'Logout') or contains(., 'Log out')]"));
                if (logoutButtons.Any())
                {
                    logoutButtons.First().Click();
                }
            }
            catch (NoSuchElementException)
            {
            }
            catch (WebDriverException)
            {
            }
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
