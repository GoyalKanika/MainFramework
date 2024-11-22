using MainFramework.Driver;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainFramework.Utilities
{
    public static class DriverUtils
    {
        private const int DefaultRetryCount = 3; // Default number of retries
        private static readonly TimeSpan DefaultRetryDelay = TimeSpan.FromSeconds(5); // Delay between retries

        private static IWebDriver GetDriver()
        {
            return DriverManager.GetDriver();
        }
        #region Execute Retry Mechanism with optional fallback
        /// <summary>
        /// Executes the specified action with retry logic.
        /// Optional Fallback: You can choose to use the retry logic alone or include the fallback JavaScript 
        /// logic depending on the test scenario. By default string jsScript is set to Null.
        /// Centralized retry mechanism
        /// </summary>
        /// <param name="action">The action to be executed</param>
        /// <param name="retries">The number of retry attempts. Defaults to 3</param>
        private static void ExecuteWithRetryAndFallback(Action action, string jsScript = null, int retries = DefaultRetryCount)
        {
            for (int attempt = 0; attempt < retries; attempt++)
            {
                try
                {
                    action(); //actions: The action to be executed. This is a delegate of type Action   
                    return; // Exit if successful
                }
                catch (WebDriverException ex)
                {
                    ReportHelper.LogFail($"Attempt {attempt + 1} failed. Error: {ex.Message}");
                    // If retries are exhausted and we have a fallback JS script not null, perform fallback with JS

                    if (attempt == retries - 1 && !string.IsNullOrEmpty(jsScript))
                    {
                        try
                        {
                            ReportHelper.LogInfo("Retries exhausted. Attempting JavaScript executor fallback...");
                            PerformFallbackJSActions(jsScript);

                        }
                        catch (Exception jsex)
                        {
                            ReportHelper.LogFail($"JavaScript fallback failed. Error: {jsex.Message}");
                            throw; // Rethrow the exception after JS fallback fails
                        }
                    }

                    Wait(DefaultRetryDelay);
                }
            }
        }
        /// <summary>
        /// Waits for a specific condition to be met.
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="delay">The maximum time to wait for the condition.</param>
        private static void Wait(Func<bool> condition, TimeSpan delay)
        {
            var endTime = DateTime.Now.Add(delay);
            while (DateTime.Now < endTime)
            {
                if (condition()) return;
                Thread.Sleep(100);
            }
        }
        private static void Wait(TimeSpan delay)
        {
            var endTime = DateTime.Now.Add(delay);
            while (DateTime.Now < endTime)
            {
                Thread.Sleep(100); // Brief pause to prevent tight looping
            }
        }
        #endregion
        #region JavaScript Fallback Logic

        /// <summary>
        /// Executes JavaScript actions as a fallback if the test fails after retries.
        /// This method takes dynamic JavaScript code as a parameter.
        /// </summary>
        /// <param name="jsScript">JavaScript code to be executed.</param>
        /// <param name="args">Arguments to be passed to the JavaScript code (optional).</param>
        private static void PerformFallbackJSActions(string jsScript, params object[] args)
        {
            try
            {
                var driver = GetDriver();
                IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;

                // Execute the provided JavaScript code with any arguments
                jsExecutor.ExecuteScript(jsScript, args);

                ReportHelper.LogInfo($"JavaScript fallback executed successfully with script: {jsScript}");
            }
            catch (Exception ex)
            {
                ReportHelper.LogFail($"JavaScript fallback action failed. Error: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region reusable Methods
        //----------------------------Reusable Functions----------------------------

        /// <summary>
        /// The WaitForElementVisible method waits for an element to be visible, 
        /// and once visible, it executes the assertion (or action) we provide
        /// </summary>
        /// <param name="locator"></param>
        /// <param name="assertion"></param>
        /// <param name="timeout"></param>
        public static void WaitForElementVisible(By locator, Action<IWebElement> assertion, int timeout = 10)
        {
            ExecuteWithRetryAndFallback(() =>
            {
                var driver = GetDriver();
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
                var element = wait.Until(ExpectedConditions.ElementIsVisible(locator)); // Wait until element is visible
                assertion(element); // Execute the assertion action on the element
            });
        }
        /// <summary>
        /// Waits for the page to load completely.
        /// </summary>
        /// <param name="timeout">The timeout period to wait for the page to load.</param>
        public static void WaitForPageToLoad(int timeout = 10)
        {
            ExecuteWithRetryAndFallback(() =>
            {
                var driver = GetDriver();
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
                wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
            });
        }
        /// <summary>
        /// Generic Assertion.  
        /// Waits for an element to meet a condition and returns the result of a given assertion.
        /// </summary>
        /// <typeparam name="T">The return type of the assertion function.</typeparam>
        /// <param name="locator">The locator of the element.</param>
        /// <param name="assertion">The assertion function to be executed on the element.</param>
        /// <param name="timeout">The maximum time to wait for the element to meet the condition.</param>
        /// <returns>The result of the assertion.</returns>
        public static T WaitForElementAndAssert<T>(By locator, Func<IWebElement, T> assertion, int timeout = 10)
        {
            T result = default(T);
            ExecuteWithRetryAndFallback(() =>
            {
                var driver = GetDriver();
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
                var element = wait.Until(ExpectedConditions.ElementIsVisible(locator)); // Wait until element is visible
                result = assertion(element); // Perform the assertion and return the result
            });
            return result;
        }
        /// <summary>
        /// Executes a JavaScript function on the page.
        /// </summary>
        /// <param name="script">The JavaScript code to execute.</param>
        /// <param name="args">Arguments to pass to the JavaScript function.</param>
        /// <returns>The result of the JavaScript execution.</returns>
        
        public static string GetJavaScriptForAction(JavaScriptActionsEnums actions)
        { //Map the enum to actual JavaScript code:
            //This method takes the JavaScriptActionsEnums and returns the corresponding JavaScript string.
            switch (actions)
            {
                case JavaScriptActionsEnums.ScrollIntoView:
                    return "arguments[0].scrollIntoView(true);";
                case JavaScriptActionsEnums.GetElementText:
                    return "return arguments[0].innerText;";
                case JavaScriptActionsEnums.ShowElement:
                    return "arguments[0].style.display = 'block';";
                case JavaScriptActionsEnums.GetPageTitle:
                    return "return document.title;";
                case JavaScriptActionsEnums.HideElement:
                    return "arguments[0].style.display = 'none';";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public static object ExecuteJavaScript(string script, params object[] args)
        {
            // Executes the JavaScript with the provided script and arguments (usually WebElement).
            var driver = GetDriver();
            IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
            return jsExecutor.ExecuteScript(script, args);
        }   
        // Helper to execute JS based on the action
        public static object ExecuteJavaScriptAction(JavaScriptActionsEnums action, params object[] args)
        {
            //Combines the previous two methods, so you can directly pass the enum to execute
            //the JavaScript without needing to call
            string script = GetJavaScriptForAction(action);
            return ExecuteJavaScript(script, args);
        }
        #endregion
    }

}
