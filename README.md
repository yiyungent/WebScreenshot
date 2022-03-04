


> ASP.NET Core + Selenium 实现 网页截图

[![repo size](https://img.shields.io/github/repo-size/yiyungent/WebScreenshot.svg?style=flat)]()
[![LICENSE](https://img.shields.io/github/license/yiyungent/WebScreenshot.svg?style=flat)](https://github.com/yiyungent/WebScreenshot/blob/master/LICENSE)
[![QQ Group](https://img.shields.io/badge/QQ%20Group-894031109-deepgreen)](https://jq.qq.com/?_wv=1027&k=q5R82fYN)

## Introduce


ASP.NET Core + Selenium 实现 网页截图


## Quick Start


[![Deploy on Railway](https://railway.app/button.svg)](https://railway.app/new/template?template=https%3A%2F%2Fgithub.com%2Fyiyungent%2FWebScreenshot&envs=WEBSCREENSHOT_CACHEMINUTES&optionalEnvs=WEBSCREENSHOT_CACHEMINUTES&WEBSCREENSHOT_CACHEMINUTESDesc=%E6%88%AA%E5%9B%BE%E7%BC%93%E5%AD%98+%28+%E5%88%86%E9%92%9F+%29&WEBSCREENSHOT_CACHEMINUTESDefault=60&referralCode=8eKBDA)


## Demo

- https://webscreenshot.up.railway.app/?url=https://moeci.com

- https://webscreenshot.up.railway.app/?url=https://moeci.com&windowWidth=1280
- https://webscreenshot.up.railway.app/?url=https://www.antmoe.com/posts/21874bc7/&jsurl=https://gitee.com/yiyunLearnRepos/test/raw/master/www.antmoe.com/posts.js


## API 文档

- https://webscreenshot.up.railway.app/swagger


## 环境变量

| 环境变量名称                 | 必填 | 备注                             |
| ---------------------------- | ---- | -------------------------------- |
| `WEBSCREENSHOT_CACHEMINUTES` |      | 截图 缓存时间 (分钟), 默认 60    |
| `WEBSCREENSHOT_CACHEMODE`    |      | 缓存模式, 默认 memory, 可填 file |



> 注意:       
>
> 强烈建议 使用 `Railway` 部署的用户，设置 `WEBSCREENSHOT_CACHEMODE` 为 `file` , 降低内存占用，     
>
> 大多数 `出错啦!` ，都是由于内存不足



> 注意:   
>
> 当 `Railway` 长时间 显示 `Server Error` 时，但 `Railway` 后台处于 `active`  时，说明 服务已经挂掉，  
>
> 你可以通过 修改 环境变量 (无需真修改，只要点击 √ 保存一次就行) 的方式，重新触发  `Deploy`



## Donate

WebScreenshot is an Apache-2.0 licensed open source project and completely free to use. However, the amount of effort needed to maintain and develop new features for the project is not sustainable without proper financial backing.

We accept donations through these channels:

- <a href="https://afdian.net/@yiyun" target="_blank">爱发电</a> (￥5.00 起)
- <a href="https://dun.mianbaoduo.com/@yiyun" target="_blank">面包多</a> (￥1.00 起)

## Author

**WebScreenshot** © [yiyun](https://github.com/yiyungent), Released under the [MIT](./LICENSE) License.<br>
Authored and maintained by yiyun with help from contributors ([list](https://github.com/yiyungent/WebScreenshot/contributors)).

> GitHub [@yiyungent](https://github.com/yiyungent) Gitee [@yiyungent](https://gitee.com/yiyungent)

