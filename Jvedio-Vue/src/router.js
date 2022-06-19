import Vue from 'vue'
import Router from 'vue-router'
import Main from './views/Main.vue'
import Download from './views/Download.vue'

Vue.use(Router)
const originalPush = Router.prototype.push
Router.prototype.push = function push(location) {
  return originalPush.call(this, location).catch(err => err)
}

export default new Router({
  mode: 'history',
  base: "/JvedioWebPage",
  routes: [
    {
      path: '/',
      name: 'main',
      component: Main
    },
    {
      path: '/download',
      name: 'download',
      component: Download
    }
  ]
})
