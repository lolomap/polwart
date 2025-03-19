import { createRouter, createWebHistory } from 'vue-router'
import { AuthPage } from '@/pages/auth'
import { RegPage } from '@/pages/reg'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/auth',
      name: "auth",
      component: AuthPage
    },
    {
      path: '/reg',
      name: 'reg',
      component: RegPage
    }
  ]
})

export default router
