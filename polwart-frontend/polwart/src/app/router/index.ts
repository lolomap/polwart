import { createRouter, createWebHistory } from 'vue-router'
import { AuthPage } from '@/pages/auth'
import { RegPage } from '@/pages/reg'
import { CreateMapPage } from '@/pages/createMap'
import { TestPage } from '@/pages/test'
import { EditPage } from '@/pages/editMap'

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
    },
    {
      path: '/create',
      name: 'create',
      component: CreateMapPage
    },
    {
      path: '/test',
      name: 'test',
      component: TestPage
    },
    {
      path: '/edit/:mapId',
      name: 'edit',
      component: EditPage
    }
  ]
})

export default router
