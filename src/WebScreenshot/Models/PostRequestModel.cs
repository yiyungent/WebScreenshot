using Microsoft.AspNetCore.Mvc;

namespace WebScreenshot.Models;

public class PostRequestModel
{
    public string url { get; set; } = "";
    public string jsurl { get; set; } = "";
    public int windowWidth { get; set; } = 0;
    public int windowHeight { get; set; } = 0;
    public int wait { get; set; } = 0;
    public int forceWait { get; set; } = 0;
    public string mode { get; set; } = "screenshot";
    public string cssSelector { get; set; } = null;
    public int jsExecutedForceWait { get; set; } = 0;
    public string jsStr { get; set; } = "";
}
