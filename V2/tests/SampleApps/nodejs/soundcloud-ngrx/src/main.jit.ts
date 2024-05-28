import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { AppModule } from './app/app-module';


document.addEventListener('DOMContentLoaded', () => {
  return platformBrowserDynamic().bootstrapModule(AppModule)
    .catch(error => console.error(error));
});
