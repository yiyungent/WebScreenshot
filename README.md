
<h1 align="center">WebScreenshot</h1>

> ASP.NET Core + Selenium 实现 网页截图

[![repo size](https://img.shields.io/github/repo-size/yiyungent/WebScreenshot.svg?style=flat)]()
[![LICENSE](https://img.shields.io/github/license/yiyungent/WebScreenshot.svg?style=flat)](https://github.com/yiyungent/WebScreenshot/blob/main/LICENSE)
[![QQ Group](https://img.shields.io/badge/QQ%20Group-894031109-deepgreen)](https://jq.qq.com/?_wv=1027&k=q5R82fYN)

## Introduce


ASP.NET Core + Selenium 实现 网页截图


## Quick Start

### 方式1: 使用 Railway 免费 一键部署

[![Deploy on Railway](https://railway.app/button.svg)](https://railway.app/new/template?code=0SqcQn&referralCode=8eKBDA)


### 方式2: 使用 Heroku 免费 一键部署

[![Deploy on Heroku](https://www.herokucdn.com/deploy/button.svg)](https://heroku.com/deploy?template=https://github.com/yiyungent/WebScreenshot)

### 方式3: 使用 Docker

```bash
# 获取源代码: 方式1: ssh 
git clone git@github.com:yiyungent/WebScreenshot.git
# 获取源代码: 方式2: https 
git clone https://github.com/yiyungent/WebScreenshot.git

docker build -t yiyungent/webscreenshot -f src/WebScreenshot/Dockerfile .

docker run -d -p 5004:80 -e ASPNETCORE_URLS="http://*:80" --name webscreenshot yiyungent/webscreenshot
```

## Demo

- https://webscreenshot-yiyun.herokuapp.com/?url=https://moeci.com

- https://webscreenshot-yiyun.herokuapp.com/?url=https://moeci.com&windowWidth=1280
  - 浏览器窗口宽 1280, 高度自动延展
- https://webscreenshot-yiyun.herokuapp.com/?url=https://www.antmoe.com/posts/614360dd/&jsurl=https://gitee.com/yiyunLearnRepos/test/raw/master/www.antmoe.com/posts.js
  - 注入 js, 修改页面

- https://webscreenshot-yiyun.herokuapp.com/?url=https://www.google.com
- https://webscreenshot-yiyun.herokuapp.com/?url=https://manga.bilibili.com/mc27189/367947?from=manga_detail&windowWidth=1280&forceWait=20
  - 强制等待 20 秒

## API 文档

- https://webscreenshot.up.railway.app/swagger
- https://webscreenshot-yiyun.herokuapp.com/swagger


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
> `Railway` `Deploy` , 处于 `ACTIVE` 后，此时 `Server Error`，只需要等待一会完全启动完成即可。

> 注意:   
>
> 正常运行较长时间后, 当 `Railway` 长时间 显示 `Server Error` 时，但 `Railway` 后台处于 `active`  时，说明 服务已经挂掉，  
>
> 你可以通过 修改 环境变量 (无需真修改，只要点击 √ 保存一次就行) 的方式，重新触发  `Deploy`


## 相关项目

- [yiyungent/Dragonfly](https://github.com/yiyungent/Dragonfly)

## Donate

WebScreenshot is an MIT licensed open source project and completely free to use. However, the amount of effort needed to maintain and develop new features for the project is not sustainable without proper financial backing.

We accept donations through these channels:

- <a href="https://afdian.net/@yiyun" target="_blank">爱发电</a> (￥5.00 起)
- <a href="https://dun.mianbaoduo.com/@yiyun" target="_blank">面包多</a> (￥1.00 起)

## Author

**WebScreenshot** © [yiyun](https://github.com/yiyungent), Released under the [MIT](./LICENSE) License.<br>
Authored and maintained by yiyun with help from contributors ([list](https://github.com/yiyungent/WebScreenshot/contributors)).

> GitHub [@yiyungent](https://github.com/yiyungent) Gitee [@yiyungent](https://gitee.com/yiyungent)

