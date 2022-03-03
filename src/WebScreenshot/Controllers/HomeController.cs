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
            string cacheMode = Environment.GetEnvironmentVariable("WebScreenshot_CacheMode".ToUpper()) ?? "memory";
            switch (cacheMode.ToLower())
            {
                case "memory":
                    _settingsModel.CacheModel = "memory";
                    break;
                case "file":
                    _settingsModel.CacheModel = "file";
                    break;
                default:
                    _settingsModel.CacheModel = "memory";
                    break;
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

                switch (_settingsModel.CacheModel)
                {
                    case "memory":
                        MemoryCache(out cacheEntry, url);
                        break;
                    case "file":
                        FileCache(out cacheEntry, url);
                        break;
                    default:
                        MemoryCache(out cacheEntry, url);
                        break;
                }


                return File(cacheEntry, "image/png", true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return Content("出错啦!");
        }

        [NonAction]
        private void FileCache(out byte[] cacheEntry, string url)
        {
            string key = Request.QueryString.Value ?? "";

            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} FileCache: {key}");

            #region Cache 控制
            string screenshotCacheKey = Utils.Md5Util.MD5Encrypt32($"{CacheKeys.Entry}_{key}");
            string screenshotCacheFileDir = Path.Combine(Directory.GetCurrentDirectory(), "FileCache");
            if (!Directory.Exists(screenshotCacheFileDir))
            {
                Directory.CreateDirectory(screenshotCacheFileDir);
            }
            string screenshotCacheFilePath = Path.Combine(screenshotCacheFileDir, screenshotCacheKey);
            // Look for cache key.
            if (!System.IO.File.Exists(screenshotCacheFilePath))
            {
                // Key not in cache, so get data.
                cacheEntry = SaveScreenshot(url);

                // Save data in cache.
                System.IO.File.WriteAllBytes(screenshotCacheFilePath, cacheEntry);
            }
            else
            {
                // 缓存文件存在
                DateTime fileCreateTime = System.IO.File.GetCreationTime(screenshotCacheFilePath);
                if (DateTime.Now > fileCreateTime.AddMinutes(_settingsModel.CacheMinutes))
                {
                    // 过期缓存
                    Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} FileCache: 过期缓存: {key}");

                    // 注意: 一定要先删除, 直接覆盖, 不会更新 文件创建时间
                    System.IO.File.Delete(screenshotCacheFilePath);
                    cacheEntry = SaveScreenshot(url);
                    System.IO.File.WriteAllBytes(screenshotCacheFilePath, cacheEntry);
                }
                else
                {
                    // 未过期缓存
                    cacheEntry = System.IO.File.ReadAllBytes(screenshotCacheFilePath);
                }
            }

            #endregion
        }

        [NonAction]
        private void MemoryCache(out byte[] cacheEntry, string url)
        {
            string key = Request.QueryString.Value ?? "";

            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} MemoryCache: {key}");

            #region Cache 控制
            string screenshotCacheKey = $"{CacheKeys.Entry}_{key}";
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
            var driver = new ChromeDriver(chromeDriverDirectory: "/app/tools/selenium/", options, commandTimeout: TimeSpan.FromMinutes(5));

            // fixed: OpenQA.Selenium.WebDriverException: The HTTP request to the remote WebDriver server for URL http://localhost:40811/session timed out after 60 seconds.
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
