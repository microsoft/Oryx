import { enableProdMode } from '@angular/core';
import { platformBrowser } from '@angular/platform-browser';
import { AppModuleNgFactory } from '../build/src/app/app-module.ngfactory';


enableProdMode();


document.addEventListener('DOMContentLoaded', () => {
  return platformBrowser().bootstrapModuleFactory(AppModuleNgFactory)
    .catch(error => console.error(error));
});
