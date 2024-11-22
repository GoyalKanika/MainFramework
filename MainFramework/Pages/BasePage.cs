using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MainFramework.Driver;
using SeleniumExtras.WaitHelpers;

namespace MainFramework.Pages
{
    public class BasePage
    {
        // Protected members to access WebDriver and WebDriverWait
        protected IWebDriver _driver;
        protected WebDriverWait _wait;

        /// <summary>
        /// Constructor initializes the WebDriver and WebDriverWait for element handling
        /// </summary>
        public BasePage()
        {
            // Initialize the WebDriver from the DriverManager
            _driver = DriverManager.GetDriver();
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10)); // Adjust wait time if necessary
        }

        /// <summary>
        /// Waits for and returns a single element that matches the specified locator.
        /// </summary>
        /// <param name="by">The locator to find the element.</param>
        /// <returns>The found IWebElement.</returns>
        protected IWebElement GetElement(By by)
        {
            return _wait.Until(ExpectedConditions.ElementIsVisible(by));
        }
        /// <summary>
        /// Waits for and returns a collection of elements that match the specified locator.
        /// </summary>
        /// <param name="by">The locator to find the elements.</param>
        /// <returns>A collection of IWebElement objects.</returns>
        protected IReadOnlyCollection<IWebElement> GetElements(By by)
        {
            return _wait.Until(driver => driver.FindElements(by));
        }

        #region element interaction methods
        /// <summary>
        /// Waits for and clicks an element once it's visible.
        /// </summary>
        /// <param name="by">The locator of the element to click.</param>
        protected void ClickElement(By by)
        {
            IWebElement element = GetElement(by);
            element.Click();
        }

        protected void SendText(By by, string input)
        {
            IWebElement element = GetElement(by);
            element.Clear();
            element.SendKeys(input);
        }

        #endregion

    }
}
