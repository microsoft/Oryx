export default defineNuxtConfig({
  app: {
    head: {
      titleTemplate: 'Nuxt HN | %s',
      meta: [
        { property: 'og:image', content: 'https://user-images.githubusercontent.com/904724/58238637-f189ca00-7d47-11e9-8213-ae072d7cd3aa.png' },
        { property: 'twitter:card', content: 'summary_large_image' },
        { property: 'twitter:site', content: '@nuxt_js' }
      ],
      link: [
        { rel: 'icon', type: 'image/x-icon', href: '/favicon.ico' },
        { rel: 'dns-prefetch', href: 'https://api.hackerwebapp.com' },
        { rel: 'preconnect', href: 'https://api.hackerwebapp.com' }
      ]
    }
  },

  plugins: [
    '~/plugins/filters'
  ],

  runtimeConfig: {
    public: {
      apiBase: 'https://api.hackerwebapp.com'
    }
  },

  compatibilityDate: '2025-08-26'
});