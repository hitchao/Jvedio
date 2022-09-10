# 插件

目前插件支持两种：

- 信息同步插件
- 皮肤插件

# 信息同步插件

## 说明

无

## 上传信息同步插件

目前仅支持 C# 版本的同步插件，未来将支持 Python 版本的信息同步插件

# 皮肤插件

## 说明

皮肤插件位于 `plugins/themes` 目录下，例如皮肤【一拳超人】，其目录结构如下

```bash
一拳超人
 |-main.json		# 保存皮肤基本信息
 |-images
 	|-bg.jpg		# 背景图片
 	|-plugin.png	# 在插件中显示的图片
 	|-small.jpg		# 在皮肤中显示的缩略图
 |-汉仪黑荔枝.ttf	 # 自定义字体
 |-readme.md		# readme.md
```

其中 main.json 格式说明如下

```json
{
    "PluginMetaData": {
        "PluginName": "【皮肤】一拳超人",// 皮肤名称
        "PluginType": 1,// 插件类型：0-信息同步，1-皮肤
        "Authors": [
            {
                "Name": "SuperStudio",
                "Github": "https://github.com/SuperStudio"
            }
        ],
        "ReleaseNotes": {
            "Version": "1.0.0",
            "Date": "2022-09-04",
            "Desc": "一拳超人",
            "MarkDown": "# 皮肤 \n Jvedio 一拳超人",// 显示在插件信息栏的 markdown 文档，不填则默认使用 readme.md
            "KeyWords": [
                {
                    "Key": "动漫",
                    "Value": "琦玉"
                }
            ]
        }
    },
    "Data": {
        "Images": {
            "Background": "./images/bg.jpg",// 需要展示的背景照片
            "Big": "./images/big.jpg",// 备用
            "Normal": "./images/default.jpg",// 备用
            "Small": "./images/small.jpg"// 用于在皮肤切换列表里显示的图片
        },
        "BgColorOpacity": 0.7,// 下面 Colors 所有带 Background 的颜色的透明度
        "Colors": {// 皮肤的颜色，如果不设置，默认黑色皮肤
            "Window.Background": "#1E1E1E",
            // ...
        }
    }
}
```

**在本地调试你的皮肤**

你可以复制现有的皮肤，起个新名字，每当你修改了本地的图片、颜色等信息，点击刷新可实时同步到 Jvedio 界面上。

<img src="Image/image-20220904130712482.png" alt="image-20220904130712482" style="zoom:80%;" />

## 上传自定义皮肤

在本地调试好自己的皮肤后，向 [Jvedio-Plugin](https://github.com/hitchao/Jvedio-Plugin) 提 Pull Request 即可，步骤如下：

1. 打开 [Jvedio-Plugin](https://github.com/hitchao/Jvedio-Plugin)，点击右上角 Fork

2. 在本地新建一个空目录，在当前目录下使用初始化仓库

```bash
git init
git config user.name "名字"
git config user.email "邮箱"
```

3. 添加远程地址并拉取代码

```bash
git remote add origin <你的远程仓库地址>
# 拉取代码
git pull origin master
```

4. 本地添加自己的皮肤，在 pluginlist.json 中的 themes 下按照模板添加自己的皮肤信息，注意 PluginID 不可和已有的重复

```json
{
    "PluginID": "在本地的目录名",
    "PluginName": "插件名称",
    "Version": "1.1.0",
    "Date": "2022-06-23",
    "Desc": "描述信息"
}
```

5. 提交

```bash
git commit -m "修改版本号和关系"
git push origin master
```

6. 去到自己的 fork 的网址，点击 Contribute，再点击 Open pull request

<img src="Image/image-20220904124813586.png" alt="image-20220904124813586" style="zoom:80%;" />

7. 点击 Create pull request，然后等待审核即可，审核后在 [Jvedio-Plugin](https://github.com/hitchao/Jvedio-Plugin) 会显示如下内容

<img src="Image/image-20220904125106639.png" alt="image-20220904125106639" style="zoom:80%;" />

8. 大概需要 3-5 分钟等待部署，部署成功后打开 Jvedio 即可显示刚才上传的皮肤





