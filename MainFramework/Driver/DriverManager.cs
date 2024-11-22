using NUnit.Framework.Internal;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainFramework.Driver
{
    public class DriverManager
    {
        private static readonly ThreadLocal<IWebDriver> _driver = new ThreadLocal<IWebDriver>();
        private static readonly object _lock = new object();
        
        private  DriverManager() { }

        public static IWebDriver GetDriver(string browser = "Chrome", Action<DriverOptions> configure = null)
        {
            if (_driver.Value == null)
            {
                lock (_lock)
                {
                    if (_driver.Value == null)
                    {
                        _driver.Value = browser.ToLower() switch
                        {
                            "firefox" => new FirefoxDriver(configureOptions<FirefoxOptions>(configure)),
                            "edge" => new EdgeDriver(configureOptions<EdgeOptions>(configure)),
                            _ => new ChromeDriver(configureOptions<ChromeOptions>(configure))
                        };

                        //Logger.log("WebDriver instance created.", ConsoleColor.Green);
                    }
                }
            }
            return _driver.Value;
        }
        private static T configureOptions<T>(Action<T> configure) where T : DriverOptions, new()
        {
            var options = new T();
            configure?.Invoke(options);
            return options;
        }
        public static ChromeOptions GetChromeOptions(Action<ChromeOptions> configure = null)
        {
            var options = new ChromeOptions();
            options.AddArgument("--disable-extensions");
            options.AddArgument("--start-maximized");
            options.AddArgument("--incognito");
            options.AddUserProfilePreference("download.default_directory", @"C:\path\to\download\directory");
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--ignore-certificate-errors");

            configure?.Invoke(options);

            return options;
        }
        public static void QuitDriver()
        {
            //Adding checks in QuitDriver to ensure WebDriver is not null and isn't already disposed.
            if (_driver.Value != null && (_driver.Value as IDisposable) != null)
            {
                try
                {
                    _driver.Value.Quit();
                   // Logger.log("WebDriver instance is quitting.", ConsoleColor.Yellow);
                    _driver.Value.Dispose();
                    _driver.Value = null;
                   // Logger.log("WebDriver instance disposed.", ConsoleColor.Green);
                }
                catch (Exception ex)
                {
                   // Logger.log($"Error during WebDriver disposal: {ex.Message}", ConsoleColor.Red);
                }
            }
        }
    }
}
