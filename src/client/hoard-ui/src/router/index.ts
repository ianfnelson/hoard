import { createRouter, createWebHistory } from "vue-router";
import PortfolioOverviewPage from "@/pages/PortfolioOverviewPage.vue";
import PortfolioExposurePage from "@/pages/PortfolioExposurePage.vue";

const router = createRouter({
  history: createWebHistory(), // ← this is the important bit
  routes: [
    {
      path: "/portfolios/:id",
      name: "portfolio-overview",
      component: PortfolioOverviewPage,
      props: true
    },
    {
      path: "/portfolios/:id/exposure",
      name: "portfolio-exposure",
      component: PortfolioExposurePage,
      props: true
    }
  ]
})

export default router
