import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';

// services
import { ApiService } from './services/api';
import { MEDIA_QUERY_PROVIDERS } from './services/media-query';


@NgModule({
  imports: [
    HttpModule
  ],
  providers: [
    ApiService,
    MEDIA_QUERY_PROVIDERS
  ]
})
export class CoreModule {}
