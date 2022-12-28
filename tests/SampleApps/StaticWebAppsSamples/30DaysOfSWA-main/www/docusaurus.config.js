// @ts-check
// Note: type annotations allow type checking and IDEs autocompletion

const lightCodeTheme = require('prism-react-renderer/themes/github');
const darkCodeTheme = require('prism-react-renderer/themes/dracula');

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'Azure Static Web Apps',
  tagline: 'Bring Your App Ideas to Life with Static Web Apps',
  url: 'https://www.azurestaticwebapps.dev',
  baseUrl: '/', // NOTE: Use '/30DaysOfSWA/' for GH Pages. '/' otherwise
  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',
  favicon: 'img/favicon.ico',
  // -- Customized for Deployment Configuration
  organizationName: 'staticwebdev', // Usually your GitHub org/user name.
  projectName: '30DaysOfSWA', // Usually your repo name.
  trailingSlash: false,
  deploymentBranch: `gh-pages`, // default = gh-pages

  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: false,
        /*
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          // Please change this to your repo.
          editUrl: 'https://github.com/staticwebdev/30DaysOfSWA/tree/main/www',
        },
        */
        blog: {
          showReadingTime: true,
          blogSidebarCount: 'ALL',
          blogSidebarTitle: 'Most Recent Posts',
          postsPerPage: 1,
        },
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
        gtag: {
          trackingID: 'G-XQTX19ZF9V',
          anonymizeIP: true,
        },
        sitemap: {
          changefreq: 'weekly',
          priority: 0.5,
          ignorePatterns: ['/tags/**'],
        },
      }),
    ],
  ],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      tableOfContents: {
        minHeadingLevel: 2,
        maxHeadingLevel: 3,
      },
      navbar: {
        title: 'Static Web Apps!',
        logo: {
          alt: '#30DaysOfSWA Logo',
          src: 'img/svg/logo.svg',
        },
        items: [
          {to: '/roadmap', label: 'Roadmap', position: 'left'},
          {to: '/thismonth', label: 'Roundup', position: 'left'},
          {to: '/showcase', label: 'Showcase', position: 'left'},
          {to: '/blog', label: 'Blog', position: 'left'},
         /* {to: '/contribute', label: 'Contribute', position: 'left'}, */
          {to: '/resources', label: 'Docs', position: 'right'},
          {to: 'https://portal.azure.com/?feature.customportal=false&WT.mc_id=30daysofswa-61155-ninarasi#create/Microsoft.StaticApp', label: 'Deploy', position: 'right'},
         /* {to: '/roadmap', label: 'Roadmap', position: 'left'},*/
          
          /*
          {
            type: 'doc',
            docId: 'intro',
            position: 'left',
            label: 'Exercises',
          },
          */
          {
            href: 'https://github.com/staticwebdev/30DaysOfSWA',
            position: 'right',
            className: 'header-github-link',
            'aria-label': 'GitHub repository',
          },
        ],
      },
      footer: {
        style: 'light',
        links: [
          /*
          {
            title: 'Docs',
            items: [
              {
                label: 'Tutorial',
                to: '/docs/intro',
              },
            ],
          },
          {
            title: 'Community',
            items: [
              {
                label: 'Stack Overflow',
                href: 'https://stackoverflow.com/questions/tagged/docusaurus',
              },
              {
                label: 'Discord',
                href: 'https://discordapp.com/invite/docusaurus',
              },
              {
                label: 'Twitter',
                href: 'https://twitter.com/docusaurus',
              },
            ],
          },
          {
            title: 'More',
            items: [
              {
                label: 'Blog',
                to: '/blog',
              },
              {
                label: 'GitHub',
                href: 'https://github.com/facebook/docusaurus',
              },
            ],
          },
          */
        ],
        copyright: `Copyright © ${new Date().getFullYear()} Microsoft </a> - Built with <a href="https://docusaurus.io"> Docusaurus </a> - Deployed to <a href="https://aka.ms/swa"> Azure </a> - Created by <a href="https://github.com/nitya"> @nitya </a>`,
      },
      prism: {
        theme: lightCodeTheme,
        darkTheme: darkCodeTheme,
      },

      image: 'https://www.azurestaticwebapps.dev/img/png/roundup/sep.png',

      metadata: [
        {
          name: 'twitter:url', 
          content: 'https://aka.ms/30DaysOfSWA'
        },
        {
          name: 'twitter:title', 
          content: 'Learn Azure Static Web Apps in #30DaysOfSWA'
        },
        {
          name: 'twitter:description', 
          content: 'Learn @AzureStaticApps from Core Concepts to Best Practices in #30DaysOfSWA at https://aka.ms/30DaysOfSWA'
        },
        {
          name: 'twitter:image', 
          content: 'https://www.azurestaticwebapps.dev/assets/images/series-people-13a2856edd7022e82a252ed05dffbabc.png'
        },
        {
          name: 'twitter:card', 
          content: 'summary_large_image'
        },
        {
          name: 'twitter:creator', 
          content: '@nitya'
        },
        {
          name: 'twitter:site', 
          content: '@AzureStaticApps'
        },

      ],

      announcementBar: {
        id: 'Learn in 30 Days',
        content:
        /*
          '<b>Find #30DaysOfSWA useful? Give it a star on <a href="https://github.com/staticwebdev/30DaysOfSWA"><b>GitHub</b></a></b>',
        */
          '<b>Found this project helpful? Give us a star on <a href="https://aka.ms/30DaysOfSWA/github"><b>GitHub</b></a></b> 🙏🏽',
        backgroundColor: '#50E6FF',
        textColor: '#552F99',
        isCloseable: false,
      },
    }),
  
  plugins: [
    [
      '@docusaurus/plugin-ideal-image',
      {
        quality: 70,
        max: 1030, // max resized image's size.
        min: 640, // min resized image's size. 
        steps: 2, // #images b/w min and max (inclusive)
        disableInDev: false,
      },
    ],
  ],
};

module.exports = config;
