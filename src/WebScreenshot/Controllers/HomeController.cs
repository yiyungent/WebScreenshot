using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Microsoft.Extensions.Caching.Memory;

namespace WebScreenshot.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private IMemoryCache _cache;
        private readonly SettingsModel _settingsModel;

        public HomeController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;

            #region SettingsModel 环境变量 赋值
            _settingsModel = new SettingsModel();
            string cacheMinutesStr = Environment.GetEnvironmentVariable("WebScreenshot_CacheMinutes".ToUpper()) ?? "";
            if (!string.IsNullOrEmpty(cacheMinutesStr) && long.TryParse(cacheMinutesStr, out long cacheMinutes))
            {
                _settingsModel.CacheMinutes = cacheMinutes;
            }
            string chromeDriverDirectory = Environment.GetEnvironmentVariable("WebScreenshot_ChromeDriverDirectory".ToUpper()) ?? "";
            if (!string.IsNullOrEmpty(cacheMinutesStr))
            {
                _settingsModel.ChromeDriverDirectory = chromeDriverDirectory;
            }
            #endregion

        }

        public async Task<ActionResult> Get([FromQuery] string url = "")
        {
            #region 检查url
            if (string.IsNullOrEmpty(url) || (!url.StartsWith("http://") && !url.StartsWith("https://")))
            {
                return Content("非法 url");
            }
            #endregion

            try
            {
                byte[] cacheEntry = null;

                #region Cache 控制
                string screenshotCacheKey = $"{CacheKeys.Entry}_{url}";
                // Look for cache key.
                if (!_cache.TryGetValue(screenshotCacheKey, out cacheEntry))
                {
                    // Key not in cache, so get data.
                    cacheEntry = SaveScreenshot(url);

                    // Set cache options.
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        // Keep in cache for this time, reset time if accessed.
                        //.SetSlidingExpiration(TimeSpan.FromSeconds(3));
                        .SetAbsoluteExpiration(DateTimeOffset.Now.AddMinutes(_settingsModel.CacheMinutes));

                    // Save data in cache.
                    _cache.Set(screenshotCacheKey, cacheEntry, cacheEntryOptions);
                }
                #endregion

                return File(cacheEntry, "image/png", true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return Content("出错啦!");
        }

        [NonAction]
        private byte[] SaveScreenshot(string url)
        {
            var options = new ChromeOptions();
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--headless");

            // Chrome 的启动文件路径
            // 只要正确安装的就不需要指定
            //options.BinaryLocation = "";

            //string chromeDriverDir = "./chromedriver";

            //var driver = new ChromeDriver(chromeDriverDirectory: _settingsModel.ChromeDriverDirectory, options);
            // "/app/tools/selenium/"
            // TODO: debug 过, 明明 _settingsModel.ChromeDriverDirectory 不为 null, 但 railway 就是报错, 于是写死
            // System.ArgumentException: Path to locate driver executable cannot be null or empty. (Parameter 'servicePath')
            var driver = new ChromeDriver(chromeDriverDirectory: "/app/tools/selenium/", options, TimeSpan.FromMinutes(5));

            // TODO: OpenQA.Selenium.WebDriverException: The HTTP request to the remote WebDriver server for URL http://localhost:40811/session timed out after 60 seconds.
            // 参考: https://www.itranslater.com/qa/details/2326059564510217216
            // new ChromeDriver(chromeDriverDirectory: "/app/tools/selenium/", options, TimeSpan.FromMinutes(5));

            driver.Navigate().GoToUrl(url);

            // https://www.selenium.dev/documentation/webdriver/browser/windows/
            string widthStr = driver.ExecuteScript("return document.documentElement.scrollWidth").ToString();
            string heightStr = driver.ExecuteScript("return document.documentElement.scrollHeight").ToString();
            int width = Convert.ToInt32(widthStr);
            int height = Convert.ToInt32(heightStr);
            driver.Manage().Window.Size = new System.Drawing.Size(width, height);

            // 保存截图
            // https://www.selenium.dev/documentation/webdriver/browser/windows/#takescreenshot
            Screenshot screenshot = (driver as ITakesScreenshot).GetScreenshot();
            // 直接用 图片数据
            byte[] rtnBytes = screenshot.AsByteArray;

            driver.Quit();

            return rtnBytes;
        }

    }

    public static class CacheKeys
    {
        public static string SignKey = "_WebScreenshot";
        public static string Entry => $"{SignKey}_Entry";
        public static string CallbackEntry => $"{SignKey}_Callback";
        public static string CallbackMessage => $"{SignKey}_CallbackMessage";
        public static string Parent => $"{SignKey}_Parent";
        public static string Child => $"{SignKey}_Child";
        public static string DependentMessage => $"{SignKey}_DependentMessage";
        public static string DependentCTS => $"{SignKey}_DependentCTS";
        public static string Ticks => $"{SignKey}_Ticks";
        public static string CancelMsg => $"{SignKey}_CancelMsg";
        public static string CancelTokenSource => $"{SignKey}_CancelTokenSource";
    }
}
