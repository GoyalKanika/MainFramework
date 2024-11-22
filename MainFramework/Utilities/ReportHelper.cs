using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using MainFramework.Driver;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainFramework.Utilities
{
    public  class ReportHelper
    {
        private static ExtentReports _extent;
        //For tests to run in parallel.
        private static readonly ThreadLocal<ExtentTest> _test = new ThreadLocal<ExtentTest>();

        public static void InitializeReport()
        {
            _extent = new ExtentReports();
            string binPath = AppContext.BaseDirectory;
            string dateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string reportPath = Path.Combine(binPath, $"ExtentReport_{dateTime}.html");
            var sparkReporter = new ExtentSparkReporter(reportPath);
            _extent.AttachReporter(sparkReporter);
        }
        /// <summary>
        /// Logs an informational message in the ExtentReport with an optional screenshot.
        /// </summary>
        /// <param name="message">The informational message to log.</param>
        /// <param name="captureScreenshot">Indicates whether to capture a screenshot. Default is true.</param>
        /// <remarks>
        /// This method first checks if a screenshot should be captured. If so, it calls the 
        /// CaptureScreenshot method to obtain the screenshot path. The method then constructs
        /// the log entry by calling BuildLogEntry, which formats the message and the 
        /// screenshot into an HTML string. Finally, it logs the entry with the Status.Info level 
        /// to the current test instance in the ExtentReport. 
        /// The screenshot is displayed as a small clickable icon, which opens the full image 
        /// in a new tab when clicked (cursor: pointer; as part of the css for BuildLogEntry method) .
        /// </remarks>
        

        public static void LogInfo(string message, bool captureScreenshot = true)
        {
            LogMessage(Status.Info, message, captureScreenshot);
        }

        public static void LogPass(string message, bool captureScreenshot = true)
        {
            LogMessage(Status.Pass, message, captureScreenshot);
        }

        public static void LogFail(string message, bool captureScreenshot = true)
        {
            LogMessage(Status.Fail, message, captureScreenshot);
        }
        private static void LogMessage(Status status, string message, bool captureScreenshot)
        {
            string screenshotPath = captureScreenshot ? CaptureScreenshot(status.ToString()) : null;
            string logEntry = BuildLogEntry(message, screenshotPath);
            _test.Value.Log(status, logEntry);
        }
        private static string CaptureScreenshot(string screenshotName)
        {
            IWebDriver driver = DriverManager.GetDriver();
            ITakesScreenshot ts = (ITakesScreenshot)driver;
            Screenshot screenshot = ts.GetScreenshot();
            string screenshotPath = Path.Combine(AppContext.BaseDirectory, $"{screenshotName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            screenshot.SaveAsFile(screenshotPath);
            return screenshotPath;
        }
        /// <summary>
        /// BuildLogEntry Method: This method constructs the HTML string for the log entry. 
        /// It checks if a screenshot path is provided and formats the message accordingly
        /// </summary>
        /// <param name="message"></param>
        /// <param name="screenshotPath"></param>
        /// <returns></returns>
        private static string BuildLogEntry(string message, string screenshotPath)
        {

            var logEntry = message;
            if (screenshotPath != null)
            {
                logEntry += $"<br><a href='{screenshotPath}' target='_blank'>" +
                            $"<img src='{screenshotPath}' style='width: 50px; height: auto; cursor: pointer;' /></a>";
            }
            return logEntry;
        }

        public static void FlushReport()
        {
            _extent.Flush();
        }
        // Optional: Can be used for managing specific test metadata like execution time, environment details etc.
        public static void AddTestMetaData(string key, string value)
        {
            _test.Value.Info($"<b>{key}: </b>{value}");
        }



    }
}
