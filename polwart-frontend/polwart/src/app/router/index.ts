import { createRouter, createWebHistory } from 'vue-router'
import { Auth } from '@/pages/auth_page'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
        path: '/',
        name: 'home',
        component: Auth
    },
    {
        path: '/auth',
        name: "auth",
        component: Auth
    },
  ]
})

export default router
