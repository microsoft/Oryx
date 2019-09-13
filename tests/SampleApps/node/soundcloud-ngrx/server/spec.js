import test from 'ava';
import express from 'express';
import request from 'supertest';

import configureApp from './config/app';
import configureRoutes from './config/routes';


function getApp(production) {
  let app = express();
  configureApp(app);
  configureRoutes(app, production);
  return app;
}


test.cb('server should redirect http to https (production)', t => {
  request(getApp(true))
    .get('/')
    .set('host', '127.0.0.1')
    .set('x-forwarded-proto', 'http')
    .expect('Location', 'https://127.0.0.1/')
    .expect(302, t.end);
});

test.cb('server should NOT redirect http to https (development)', t => {
  request(getApp())
    .get('/')
    .set('x-forwarded-proto', 'http')
    .expect(200, t.end);
});

test.cb('server should include security headers in response', t => {
  request(getApp())
    .get('/')
    .expect('Strict-Transport-Security', 'max-age=7776000000; includeSubDomains')
    .expect('X-Content-Type-Options', 'nosniff')
    .expect('X-Download-Options', 'noopen')
    .expect('X-Frame-Options', 'DENY')
    .expect('X-XSS-Protection', '1; mode=block')
    .expect(200, t.end);
});

test.cb('server should send index.html (development)', t => {
  request(getApp())
    .get('/')
    .expect('Content-Type', 'text/html; charset=UTF-8')
    .expect(200, t.end);
});

test.cb('server should send index.html (production)', t => {
  request(getApp(true))
    .get('/')
    .set('x-forwarded-proto', 'https')
    .expect('Content-Type', 'text/html; charset=UTF-8')
    .expect(200, t.end);
});
