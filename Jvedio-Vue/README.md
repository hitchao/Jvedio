# jvedio-web

安装环境

```bash
npm run install
```

启动

```bash
yarn start
```

编译

```bash
yarn build
```

docker 运行

```bash
docker build -f Dockerfile -t jvedio-web .
docker rm -f jvedio-web
docker run --name jvedio-web -p 8099:80 -d jvedio-web

```

