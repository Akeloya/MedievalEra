import { createRouter, createWebHistory } from "vue-router"

const routes = [
  {
    path: "/",
    component: () => import("@/components/Views/HomeView.vue"),
    children: [
      {
        path: "",
        name: "home",
        component: () => import("@/components/Views/HomeView.vue"),
        meta: {
          title: "Главное меню",
          breadcrumb: "Главное меню",
          icon: "🏰",
          description: "Выберите раздел"
        },
        children: [
          {
            path: "game",
            name: "game",
            component: () => import("@/components/Views/HomeView.vue"),
            meta: {
              title: "Играть",
              breadcrumb: "Игра",
              icon: "⚔️",
              description: "Старт новой игры"
            },
            children: [
              {
                path: "online",
                name: "online",
                component: () => import("@/components/Views/HomeView.vue"),
                meta: {
                  title: "Играть онлайн",
                  breadcrumb: "Онлайн",
                  icon: "⚔️",
                  description: "Старт новой игры"
                },
                children: [
                  {
                    path: "lobby",
                    name: "lobby",
                    component: () => import("@/components/Views/LobbyView.vue"),
                    meta: {
                      title: "Лобби",
                      breadcrumb: "Онлайн игра",
                      icon: "👥",
                      description: "Онлайн игра"
                    }
                  },
                  {
                    path: "settings",
                    name: "settings",
                    component: () => import("@/components/Views/SettingsView.vue"),
                    meta: {
                      title: "Настройки",
                      breadcrumb: "Настройки",
                      icon: "⚙️",
                      description: "Настройки игры"
                    }
                  }
                ]
              },
              {
                path: "tableGame",
                name: "tableGame",
                component: () => import("@/components/Views/TableGameView.vue"),
                meta: { title: "Настольная игра", breadcrumb: "Настольная игра", icon: "🎲" }
              }
            ]
          },
          {
            path: "/results",
            name: "results",
            component: () => import("@/components/Views/ScoresView.vue"),
            meta: {
              title: "Результаты",
              breadcrumb: "Результаты",
              icon: "🏆",
              description: "Результаты прошедших игр"
            }
          },
          {
            path: "/profile",
            name: "profile",
            component: () => import("@/components/Views/ProfileView.vue"),
            meta: {
              title: "Профиль",
              breadcrumb: "Профиль игрока",
              icon: "👤",
              description: "Вход и подтверждение результатов"
            }
          },
          {
            path: "/rules",
            name: "rules",
            component: () => import("@/components/Views/RulesView.vue"),
            meta: {
              title: "Правила",
              breadcrumb: "Правила",
              icon: "📜",
              description: "Правила игры"
            }
          },
          {
            path: "/authors",
            name: "authors",
            component: () => import("@/components/Views/AuthorsView.vue"),
            meta: {
              title: "Авторы",
              breadcrumb: "Авторы",
              icon: "✍️",
              description: "Создатели настольной игры"
            }
          }
        ]
      }
    ]
  },
  {
    path: "/:pathMatch(.*)*",
    name: "not-found",
    component: () => import("@/components/Views/NotFoundView.vue"),
    meta: { title: "404", breadcrumb: "Ошибка" }
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

export default router
