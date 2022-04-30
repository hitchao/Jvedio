<template>
  <div id="app">
    <img @click="routeTo('/')"  class="logo" :src="logo" />
    <div class="nav">
      <span @click="routeTo('/')">首页</span>
      <span @click="routeTo('/download')">下载</span>
      <span ><a style="color: #fff;" href="https://github.com/hitchao/Jvedio/wiki">使用说明</a></span>
      <span><a style="color: #fff;" href="https://github.com/hitchao/Jvedio/issues">反馈</a></span>
    </div>
  </div>
</template>

<script>
  export default {
    data() {
      return {
        logo: require("../assets/logo.png"),
        activeIndex: 0
      }
    }, methods: {
      routeTo(path) {
        this.$router.push({ path: path });
      }
    }
  }

</script>

<style scoped>
  .nav a {
    text-decoration: none;
  }

  .nav {
    margin-left: 20px;
    user-select: none;
  }

  .nav span {
    color: #fff;
    font-size: 15px;
    cursor: pointer;
    padding: 10px 20px;
    border-radius: 4px;
  }

  .nav span:hover {
    background-color: #18191B;
  }

  .logo {
    margin-left: 20px;
    height: 30px;
    cursor: pointer;
  }

  #app {
    height: 100%;
    width: 100%;
    display: flex;
    align-items: center;
    justify-content: normal;
    min-width: 200px;
  }

  .header {
    background-color: #22252A;
  }
</style>