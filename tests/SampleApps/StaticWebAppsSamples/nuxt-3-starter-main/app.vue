<template>
  <div>
    <div class="home-page">
      <h1>Hi! This is a simple Nuxt 3 app.</h1>
      <h2>Click on the buttons below to check out a server route or an API route :) </h2>
      <NuxtLink to="/hello" target="_blank">
        <button> 
            What Time Is It?
        </button>
      </NuxtLink>
      <NuxtLink to="/api/hello" target="_blank">
        <button>
            Link to API Route
        </button>
      </NuxtLink>
      <h2> Deploy Nuxt 3 app with universal rendering on Azure Static Web Apps using <NuxtLink to="https://docs.microsoft.com/azure/static-web-apps/deploy-nuxtjs" class="styling">the Microsoft documentation</NuxtLink></h2>
    </div>
    <NuxtWelcome />
  </div>
</template>
<style>
  .home-page{
    margin: 5px;
    margin-bottom: 0;
    padding: 20px;
    padding-top: 0;
    padding-bottom: 0;
  }

  .styling{
    color: rgb(29, 199, 128);
    font-weight: 500;
    text-decoration: none;
  }

  .styling:hover{
    cursor: pointer;
    color: rgb(18, 168, 106);
    font-weight: 500;
    text-decoration: underline;
    transition: 0.2s ease-in-out;

  }

  .home-page h1{
    font-family: ui-sans-serif,system-ui,-apple-system,BlinkMacSystemFont,Segoe UI,Roboto,Helvetica Neue,Arial,Noto Sans,sans-serif,"Apple Color Emoji","Segoe UI Emoji",Segoe UI Symbol,"Noto Color Emoji";
    font-weight: 500;
    font-size: 1.5rem;
    line-height: 2rem;
  }

  .home-page h2{
    font-family: ui-sans-serif,system-ui,-apple-system,BlinkMacSystemFont,Segoe UI,Roboto,Helvetica Neue,Arial,Noto Sans,sans-serif,"Apple Color Emoji","Segoe UI Emoji",Segoe UI Symbol,"Noto Color Emoji";
    font-weight: 400;
    font-size: 1rem;
    line-height: 2rem;
  }

  button {
    font-family: ui-sans-serif,system-ui,-apple-system,BlinkMacSystemFont,Segoe UI,Roboto,Helvetica Neue,Arial,Noto Sans,sans-serif,"Apple Color Emoji","Segoe UI Emoji",Segoe UI Symbol,"Noto Color Emoji";
    font-weight: 500;
    font-size: 1.5rem;
    line-height: 2rem;
    color: white;
    background-color: rgb(42, 219, 145);
    padding: 10px 30px 10px 30px;
    margin: 0px;
    margin-top: 10px;
    margin-bottom: 10px;
    margin-left: 5px;
    border: none;
    border-radius: 5px;
    font-size: 1rem;
  }

  button:hover{
    cursor: pointer;
    background-color: rgb(29, 199, 128);
    transition: 0.2s ease-in;
  }
</style>
