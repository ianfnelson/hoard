import { createRouter, createWebHistory } from "vue-router";
import PortfolioOverviewPage from "@/pages/PortfolioOverviewPage.vue";

const router = createRouter({
  history: createWebHistory(), // ← this is the important bit
  routes: [
    {
      path: "/portfolios/:id",
      name: "portfolio-overview",
      component: PortfolioOverviewPage,
      props: true
    }
  ]
})

export default router
