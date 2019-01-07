const express = require('express');
const paths = require('./paths');


module.exports = (app, production) => {
  const router = new express.Router();
  const htmlFile = paths.indexHtml;

  if (production) {
    router.all('*', (req, res) => {
      if (req.headers['x-forwarded-proto'] !== 'https') {
        return res.redirect(`https://${req.headers.host}${req.url}`);
      }
      else {
        res.sendFile(htmlFile);
      }
    });
  }
  else {
    router.all('*', (req, res) => {
      res.sendFile(htmlFile);
    });
  }

  app.use(router);
};
