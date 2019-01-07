module.exports = config => {
  config.set({
    frameworks: ['jasmine'],

    files: ['karma.entry.js'],

    preprocessors: {
      'karma.entry.js': config.singleRun ? ['coverage', 'webpack', 'sourcemap'] : ['webpack', 'sourcemap']
    },

    webpack: require('./webpack.config'),

    webpackMiddleware: {
      noInfo: true
    },

    coverageReporter: {
      type: 'in-memory'
    },

    remapCoverageReporter: {
      html: './coverage/html',
      lcovonly: './coverage/coverage.info',
      'text-summary': null
    },

    reporters: config.singleRun ? ['dots', 'coverage', 'remap-coverage'] : ['dots'],

    logLevel: config.LOG_INFO,

    autoWatch: false,

    singleRun: false,

    browsers: ['Chrome']
  });
};
