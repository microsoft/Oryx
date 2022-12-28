# Under The Hood

This is a [Docusaurus-powered](https://docusaurus.io) site to host tutorials and blog posts that can support #30Days learning journeys. Here are the steps used to setup this site and configure it for automated build-deploy workflows.



---

## 1. Setup & Validate Site

| Command | Description |
|:--- |:--- |
| `npx create-docusaurus@latest www classic` | Scaffold **a new** classic docusaurus site in www/ folder |
| `cd www; npm install` | Work with an **existing** docusaurus site scaffolded in www/ folder |
| `cd www; npx docusaurus start` | Validate setup with local preview (and hot reload) |
| `cd www; npm run build` | Build production-ready site (in _build/_ folder by default) |
| `cd www; npm run serve` | Preview production-ready site on local device |
| | |

<br/>

## 2. Customize Site Contents

| Action | Outcome |
|:--- |:--- |
| Add `hello.md` under `www/src/pages/` | Creates a standalone web page accessible at path `/hello`|
| Add `hello.md` under `www/docs/` | Creates a tutorial page (with prev/next navigation and sidebar) accessible at path `/docs/hello`|
| Add `hello.md` under `www/docs/basics/` | Creates a tutorial collection accessible at path `/docs/basics` with `/docs/basics/hello` as first step.|
| Add `2022-01-17-hello.md` under `www/blog` | Creates a blog post timestamped `January 17, 2021` accessible under `/blog/hello`, with blog index at `blog/` |
| Edit site  settings in `www/docusaurus.config.js` | Customize [various parameters](https://docusaurus.io/docs/configuration) for site metadata, deployment, themes, plugins and more. |
| Edit docs sidebar settings in `www/sidebars.js` | Customize [sidebar configuration](https://docusaurus.io/docs/sidebar/items) or learn to use auto-generated versions more effectively. |
| Update [styling and layout](https://docusaurus.io/docs/next/styling-layout) and validate for light & dark theme | Use [colorbox.io](https://colorbox.io/) and [guide](https://justtheskills.com/colorbox/) to define color palette. Create [style variables](https://docusaurus.io/docs/next/styling-layout#styling-your-site-with-infima) to update `custom.css` (based on  the [infima](https://infima.dev/) styling framework) |
| Add [supported plugins](https://docusaurus.io/docs/api/plugins) for enhanced content or behaviors | Default plugins used are [plugin-content-docs](https://docusaurus.io/docs/api/plugins/@docusaurus/plugin-content-docs) (for `docs/` pages) , [plugin-content-blog](https://docusaurus.io/docs/api/plugins/@docusaurus/plugin-content-blog) (for `blog/`) and [plugin-content-pages](https://docusaurus.io/docs/api/plugins/@docusaurus/plugin-content-pages) for default pages. Check out [plugin-sitemap](https://docusaurus.io/docs/api/plugins/@docusaurus/plugin-sitemap), [plugin-ideal-image](https://docusaurus.io/docs/api/plugins/@docusaurus/plugin-ideal-image) and [plugin-pwa](https://docusaurus.io/docs/api/plugins/@docusaurus/plugin-pwa) for other useful features. |
| | |

<br/>

## 3. Deploy to GitHub Pages

| Action | Description |
|:--- |:--- |
| [Modify docusaurus.config.js](https://docusaurus.io/docs/deployment#docusaurusconfigjs-settings)  | Add `organizationName`=user, `projectName`=repo, `deploymentBranch`=gh-pages properties. <br/>Updated `url` property to relevant github.io version for now |
| [Configure publishing source for GitHub Pages](https://docs.github.com/en/pages/getting-started-with-github-pages/configuring-a-publishing-source-for-your-github-pages-site)| Do an initial manual deploy using `GIT_USER=<GITHUB_USERNAME> GIT_PASS=<GITHUB_PERSONAL_ACCESS_TOKEN> npm run deploy` to setup the GitHub Pages branch. Generate [Personal Tokens](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token) if needed. Validate deploy by visiting [https://staticwebdev.github.io/30DaysOfSWA/](https://staticwebdev.github.io/30DaysOfSWA) - then automate deploy using GitHub Actions.|
| | |
| | |

<br/>

## 4. Automate with GitHub Actions

| Action | Description |
|:--- |:--- |
| [Setup GitHub Actions for auto-deploy](https://docusaurus.io/docs/deployment#triggering-deployment-with-github-actions)  | We want this to auto-deploy build to gh-pages when new commit is made to `main/`. Follow the directions for "Same" repo - add `deploy.yml` and `test-deploy.yml` to `.github/workflows` -- commit changes! I used `www/**` for paths) and `npm` for build) |
|  [Visit Actions Dashboard](https://github.com/nitya/docusaurus-demo/actions) | Commits should trigger action - verify that build/deploy works. |
|  | |


<br/>
