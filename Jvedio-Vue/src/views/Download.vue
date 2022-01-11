<template>
    <div id="app">
        <div class="card" v-for="(item,index) in config.files">
            <p>Jvedio for {{item.device}}</p>
            <el-divider></el-divider>

            <div v-if="!item.enable">
                <p>敬请期待</p>
            </div>
            <div style="text-align: center;" v-if="item.enable">
                <img :src="window_logo" style="width: 200px;" />
                <p style="font-size: 20px;margin: 5px;font-weight: bold;">{{item.version}}</p>
                <p style="font-size: 10px;margin: 5px;">{{item.date}}</p>
                
                <el-button @click="downloadApp(index)" type="primary">下载</el-button>
            </div>

        </div>
    </div>
</template>

<script>
    import axios from "axios";
    export default {
        data() {
            return {
                window_logo: require("../assets/icons/Windows.png"),
                config: {},
            }
        }, methods: {
            downloadApp(idx) {
                location.href="files/" + this.config.files[idx].name;
            },
            loadConfig() {
                axios.request({
                    url: "files/config.json",
                    method: "get"
                }).then((res) => {
                    this.config=res.data;
                });
            }
        }, created() {
            this.loadConfig();
        }
    }
</script>

<style scoped>
    .el-divider {
        background-color: gray;
    }


    .card {
        background-color: #101013;
        width: 300px;
        height: 280px;
        color: #9B9B9B;
        padding: 10px 20px;
        margin: 10px;
        line-height: 30px;
        border-radius: 5px;
        box-shadow: 2px 2px 10px #000;
        float: left;
    }

    #app {
        padding: 0px;
    }
</style>