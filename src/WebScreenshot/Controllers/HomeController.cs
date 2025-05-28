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
    // 注意: 不能使用 [ApiController] + ControllerBase, 因为不支持 Controller.Action 可选参数 ( string jsurl = "" ), 始终会返回 json 格式的错误信息

    /// <summary>
    /// 获取 Web 截图
    /// </summary>
    [Route("")]
    public class HomeController : Controller
    {
        #region Fields
        private IMemoryCache _cache;
        private readonly SettingsModel _settingsModel;

        #region QueryString
        private int _windowWidth;
        private int _windowHeight;
        private int _wait;
        private int _forceWait;
        #endregion 
        #endregion

        #region Ctor
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
            string debugStr = Environment.GetEnvironmentVariable("WebScreenshot_Debug".ToUpper()) ?? "";
            if (!string.IsNullOrEmpty(debugStr) && bool.TryParse(debugStr, out bool debug))
            {
                _settingsModel.Debug = debug;
            }
            #endregion

        }
        #endregion

        #region Actions
        /// <summary>
        /// 获取 Web 截图
        /// </summary>
        /// <param name="url">目标网页 url</param>
        /// <param name="jsurl">注入js url</param>
        /// <param name="windowWidth">浏览器窗口 宽</param>
        /// <param name="windowHeight">浏览器窗口 高</param>
        /// <param name="wait">隐式等待 秒数</param>
        /// <param name="forceWait">强制等待 秒数</param>
        /// <param name="mode">模式: screenshot 截图(默认); html 获取网页源代码</param>
        /// <param name="cssSelector">CSS 选择器, 选中目标区域</param>
        /// <returns>若成功, screenshot 则返回 image/png 截图, html 则返回网页源代码</returns>
        [Route("")]
        [HttpGet]
        //[Produces("image/png")]
        public async Task<ActionResult> Get([FromQuery] string url = "", [FromQuery] string jsurl = "",
            [FromQuery] int windowWidth = 0, [FromQuery] int windowHeight = 0, [FromQuery] int wait = 0, [FromQuery] int forceWait = 0, string mode = "screenshot", string cssSelector = null)
        {
            #region 检查url
            if (string.IsNullOrEmpty(url) || (!url.StartsWith("http://") && !url.StartsWith("https://")))
            {
                return Content("非法 url");
            }
            #endregion

            #region 检查jsurl
            string jsStr = null;
            if (!string.IsNullOrEmpty(jsurl))
            {
                if (!jsurl.StartsWith("http://") && !jsurl.StartsWith("https://"))
                {
                    return Content("非法 jsurl");
                }
                // 合法 jsurl
                try
                {
                    jsStr = Utils.HttpUtil.HttpGet(url: jsurl);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("获取 jsurl 失败");
                    Console.WriteLine(ex.ToString());

                    return Content("获取 jsurl 失败");
                }
            }
            #endregion

            #region 检查 windowWidith,windowHeight
            if (windowWidth < 0)
            {
                return Content("非法 windowWidth");
            }
            if (windowHeight < 0)
            {
                return Content("非法 windowHeight");
            }
            _windowWidth = windowWidth;
            _windowHeight = windowHeight;
            #endregion

            #region 检查 wait
            if (wait < 0)
            {
                return Content("非法 wait");
            }
            _wait = wait;
            #endregion

            #region 检查 forceWait
            if (forceWait < 0)
            {
                return Content("非法 forceWait");
            }
            _forceWait = forceWait;
            #endregion

            string exStr = string.Empty;
            try
            {
                #region cache
                byte[] cacheEntry = null;
                switch (_settingsModel.CacheModel)
                {
                    case "memory":
                        MemoryCache(out cacheEntry, url: url, jsurl: jsurl, jsStr: jsStr, mode: mode, cssSelector: cssSelector);
                        break;
                    case "file":
                        FileCache(out cacheEntry, url: url, jsurl: jsurl, jsStr: jsStr, mode: mode, cssSelector: cssSelector);
                        break;
                    default:
                        MemoryCache(out cacheEntry, url: url, jsurl: jsurl, jsStr: jsStr, mode: mode, cssSelector: cssSelector);
                        break;
                }
                #endregion

                ActionResult actionResult = null;

                #region mode
                switch (mode)
                {
                    case "screenshot":
                        actionResult = File(cacheEntry, "image/png", true);
                        break;
                    case "html":
                        actionResult = Content(System.Text.Encoding.UTF8.GetString(cacheEntry), "text/html", Encoding.UTF8);
                        break;
                    default:
                        actionResult = File(cacheEntry, "image/png", true);
                        break;
                }
                #endregion

                return actionResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                if (_settingsModel.Debug)
                {
                    exStr = ex.ToString();
                }
            }

            return Content(@$"<div>
                                <h2>Error!</h2>
                                <pre>{exStr}</pre>
                              </div", "text/html", Encoding.UTF8);
        }
        #endregion

        #region Helpers

        #region Cache
        [NonAction]
        private void FileCache(out byte[] cacheEntry, string url, string jsurl, string jsStr, string mode, string cssSelector)
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
                cacheEntry = Save(url: url, jsurl: jsurl, jsStr: jsStr, mode: mode, cssSelector: cssSelector);

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
                    cacheEntry = Save(url: url, jsurl: jsurl, jsStr: jsStr, mode: mode, cssSelector: cssSelector);
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
        private void MemoryCache(out byte[] cacheEntry, string url, string jsurl, string jsStr, string mode, string cssSelector)
        {
            string key = Request.QueryString.Value ?? "";

            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} MemoryCache: {key}");

            #region Cache 控制
            string screenshotCacheKey = $"{CacheKeys.Entry}_{key}";
            // Look for cache key.
            if (!_cache.TryGetValue(screenshotCacheKey, out cacheEntry))
            {
                // Key not in cache, so get data.
                cacheEntry = Save(url: url, jsurl: jsurl, jsStr: jsStr, mode: mode, cssSelector: cssSelector);

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
        #endregion

        [NonAction]
        private byte[] Save(string url, string jsurl, string jsStr, string mode, string cssSelector)
        {
            #region 初始化参数选项
            var options = new ChromeOptions();
            // https://stackoverflow.com/questions/59186984/selenium-common-exceptions-sessionnotcreatedexception-message-session-not-crea
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--headless");
            options.AddArgument("--ignore-certificate-errors");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36");

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
            #endregion

            #region Implicit wait
            if (_wait > 0)
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(_wait);
            }
            #endregion

            driver.Navigate().GoToUrl(url);

            #region 强制 wait
            if (_forceWait > 0)
            {
                Thread.Sleep(TimeSpan.FromSeconds(_forceWait));
            }
            #endregion

            #region 设置窗口大小
            int width = _windowWidth;
            int height = _windowHeight;
            if (_windowWidth <= 0)
            {
                // 默认 width
                string widthStr = driver.ExecuteScript("return document.documentElement.scrollWidth").ToString();
                width = Convert.ToInt32(widthStr);
            }
            if (_windowHeight <= 0)
            {
                // 默认 height
                string heightStr = driver.ExecuteScript("return document.documentElement.scrollHeight").ToString();
                height = Convert.ToInt32(heightStr);
            }
            // https://www.selenium.dev/documentation/webdriver/browser/windows/
            driver.Manage().Window.Size = new System.Drawing.Size(width, height);
            #endregion

            #region 注入js
            // 注入 jsStr
            if (!string.IsNullOrEmpty(jsStr))
            {
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {url} : 注入 jsStr:");
                Console.WriteLine("--------------------------------------------------------------------------------------------");
                Console.WriteLine($"jsStr 来自: jsurl: {jsurl}");
                Console.WriteLine("--------------------------------------------------------------------------------------------");

                driver.ExecuteScript(jsStr);
            }
            #endregion

            byte[] rtnBytes = null;

            #region 截图方法
            Action screenshotAction = () =>
            {
                // 保存截图
                // https://www.selenium.dev/documentation/webdriver/browser/windows/#takescreenshot
                Screenshot screenshot = null;
                if (!string.IsNullOrEmpty(cssSelector))
                {
                    var webElement = driver.FindElement(By.CssSelector(cssSelector));
                    // Screenshot for the element
                    screenshot = (webElement as ITakesScreenshot).GetScreenshot();
                }
                else
                {
                    screenshot = (driver as ITakesScreenshot).GetScreenshot();
                }
                // 直接用 图片数据
                rtnBytes = screenshot.AsByteArray;
            };
            #endregion

            #region mode
            switch (mode)
            {
                case "screenshot":
                    screenshotAction();
                    break;
                case "html":
                    string source = string.Empty;
                    if (!string.IsNullOrEmpty(cssSelector))
                    {
                        var webElement = driver.FindElement(By.CssSelector(cssSelector));
                        // https://www.selenium.dev/documentation/webdriver/elements/information/#text-content
                        // 这样获取到的 为 InnerText 没有 HTML标签
                        //source = webElement.Text;
                        // https://www.selenium.dev/documentation/webdriver/elements/information/#fetching-attributes-or-properties
                        source = webElement.GetAttribute("innerHTML");
                    }
                    else
                    {
                        source = driver.PageSource;
                    }
                    rtnBytes = System.Text.Encoding.UTF8.GetBytes(source);
                    break;
                default:
                    screenshotAction();
                    break;
            }
            #endregion

            driver.Quit();

            return rtnBytes;
        }

        #endregion

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
