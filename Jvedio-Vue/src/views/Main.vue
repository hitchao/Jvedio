<template>
    <div id="app">
        <h1 class="title">Jvedio-本地视频管理</h1>
        <div class="desc">
            <span>
                Jvedio 是本地视频管理软件，支持扫描本地视频并导入软件，建立视频库， 提取出视频的<b>唯一识别码</b>，自动分类视频， 添加标签管理视频，使用人工智能识别演员，支持翻译信息， 基于 FFmpeg
                截取视频图片，Window 桌面端流畅美观的应用软件
            </span>
        </div>
        <el-carousel height="500px" :interval="5000" arrow="always"
        trigger="click"
        >
            <el-carousel-item v-for="item in sample_list" :key="item">
                <el-image
                style="height: 500px"
                :src="item"
                fit="scale-down"></el-image>
            </el-carousel-item>
        </el-carousel>
    </div>
</template>

<script>
    export default {
        data() {
            return {
                sample_list: [
                    require("../assets/imgs/sample1.png"),
                    require("../assets/imgs/sample2.png"),
                    require("../assets/imgs/sample3.png"),
                    require("../assets/imgs/sample4.png"),
                ]
            }
        }
    }
</script>

<style scoped>
    .el-carousel {
        margin: 20px 5px;
        box-shadow:2px 2px 10px #000;
        text-align: center;
    }


    .el-carousel__item:nth-child(2n) {
        background-color: #101013;
    }

    .el-carousel__item:nth-child(2n+1) {
        background-color: #101013;
    }

    .desc {
        font-size: 15px;
        background-color: #101013;
        color: #9B9B9B;
        padding: 10px 20px;
        line-height: 30px;
        border-radius: 5px;
        box-shadow:2px 2px 10px #000;
    }

    .title {
        text-align: center;
        color: #fff;
    }
</style>