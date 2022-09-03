# 信息同步获取 Headers 步骤

1. 打开浏览器（本教程使用 Chrome 浏览器），输入网址，注册后登入（有些网址不用注册登陆也行）
2. 在空白处右击点击审查元素（或者按 F12），出现如下界面，一次点击刷新，接着点击 Network（在弹出的界面的第 1 行），Doc（在第 2 或第 3 行），然后列表里会出现一个文件（文件名形如：xxxxx.com），点击该文件

[<img src="https://s1.ax1x.com/2022/06/11/XcZwZQ.png" alt="XcZwZQ.png" style="zoom:80%;" />](https://imgtu.com/i/XcZwZQ)

3. 选择 Headers（第一个）滚动到 **Request Headers** 这块，复制所有内容（不包括 Request Headers）

[<img src="https://s1.ax1x.com/2022/06/11/XcZ0aj.png" alt="XcZ0aj.png" style="zoom:80%;" />](https://imgtu.com/i/XcZ0aj)

示例

```properties
accept: xxx
accept-encoding: gzip, deflate, br
accept-language: zh-CN,zh;q=0.9
cache-control: no-cache
cookie: xxx
pragma: no-cache
upgrade-insecure-requests: 1
user-agent: xxx
...
```

4. 粘贴到软件中，并测试通过