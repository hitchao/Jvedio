<template>
  <el-container>
      <el-header>
          <common-header></common-header>
      </el-header>
      <el-main>
          <router-view />
      </el-main>
  </el-container>
</template>


<script>
  import CommonHeader from "./components/CommonHeader.vue";

  export default {
      name: "Main",
      components: { CommonHeader },
  };

</script>

<style scoped>
  .el-header{
      padding: 0px;
      background-color: #22252A;
  }
</style>

<style>
  html,body{
    margin: 0px;
    padding: 0px;
    background-color: #1B1B1F;
  }
</style>
