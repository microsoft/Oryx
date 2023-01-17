[![E2E tests](https://github.com/staticwebdev/nextjs-starter/actions/workflows/playwright.js.yml/badge.svg)](https://github.com/staticwebdev/nextjs-starter/actions/workflows/playwright.js.yml)

# Next.js starter

[Azure Static Web Apps](https://docs.microsoft.com/azure/static-web-apps/overview) allows you to easily build [Next.js](https://nextjs.org/) apps in minutes. Use this repo with the [Next.js tutorial](https://docs.microsoft.com/azure/static-web-apps/deploy-nextjs) to build and customize a new static site.

## Running locally

To run locally, open the development server with the following command:

```bash
npm run dev
```

Next, open [http://localhost:3000](http://localhost:3000) in your browser to see the result.

For a more rich local development experience, refer to [Set up local development for Azure Static Web Apps](https://docs.microsoft.com/azure/static-web-apps/local-development).

## How it works

This starter application is configured to build a static site with dynamic routes. 

### Dynamic routes

The *pages/project/[slug].js* file implements code that tells Next.js what pages to generate based on associated data. In Next.js, each page powered by dynamic routes needs to implement `getStaticPaths` and `getStaticProps` to give Next.js the information it needs to build pages that match possible route values.

Inside `getStaticPaths`, each data object is used to create a list of paths all possible pages.

```javascript
export async function getStaticPaths() {
  const paths = projects.map((project) => ({
    params: { path: project.slug },
  }))
  return { paths, fallback: false };
}
```
The `getStaticProps` function is run each time a page is generated. Based off the parameter values, the function matches the full data object to the page being generated. Once the data object is returned, it is used as the context for the generated page.

```javascript
export async function getStaticProps({ params }) {
  const project = projects.find(proj => proj.slug === params.path);
  return { props: { project } };
}
```
### Application configuration

The `next.config.js` file is set up to enforce trailing slashes on all page.

```javascript
module.exports = {
    trailingSlash: true
};
```
### Build scripts

The npm `build` script runs commands to not only build the application, but also generate all the static files to the `out` folder.

```json
"scripts": {
  "dev": "next dev",
  "build": "next build && next export",
},
```

> **Note:** If you use the [Azure Static Web Apps CLI](https://docs.microsoft.com/azure/static-web-apps/local-development), copy the *staticwebapp.config.json* file to the *out* folder, and start the CLI from the *out* folder.
