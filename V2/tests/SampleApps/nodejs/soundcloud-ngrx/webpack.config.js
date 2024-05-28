const path = require('path');

const CheckerPlugin = require('awesome-typescript-loader').CheckerPlugin;
const CommonsChunkPlugin = require('webpack/lib/optimize/CommonsChunkPlugin');
const ContextReplacementPlugin = require('webpack/lib/ContextReplacementPlugin');
const DefinePlugin = require('webpack/lib/DefinePlugin');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const NgcWebpackPlugin = require('ngc-webpack').NgcWebpackPlugin;
const UglifyJsPlugin = require('webpack/lib/optimize/UglifyJsPlugin');
const WebpackMd5Hash = require('webpack-md5-hash');


//=========================================================
//  VARS
//---------------------------------------------------------
const NODE_ENV = process.env.NODE_ENV;

const ENV_DEVELOPMENT = NODE_ENV === 'development';
const ENV_PRODUCTION = NODE_ENV === 'production';
const ENV_TEST = NODE_ENV === 'test';

const SERVER_HOST = '0.0.0.0';
const SERVER_PORT = 3000;

const SOUNDCLOUD_CLIENT_ID = JSON.stringify(process.env.SOUNDCLOUD_CLIENT_ID || 'd02c42795f3bcac39f84eee0ae384b00');


//=========================================================
//  LOADERS
//---------------------------------------------------------
const rules = {
  scss: {
    test: /\.scss$/,
    use: [
      'raw-loader',
      'postcss-loader',
      {
        loader: 'sass-loader',
        options: {
          includePaths: ['src'],
          outputStyle: 'compressed',
          precision: 10,
          sourceComments: false
        }
      }
    ]
  },
  typescript: {
    test: /\.ts$/,
    use: [
      {
        loader: 'awesome-typescript-loader',
        options: {
          configFileName: ENV_PRODUCTION ? 'tsconfig.aot.json' : 'tsconfig.json'
        }
      },
      'angular2-template-loader'
    ]
  }
};


//=========================================================
//  CONFIG
//---------------------------------------------------------
const config = module.exports = {};

config.resolve = {
  extensions: ['.ts', '.js'],
  modules: [
    path.resolve('./src'),
    path.resolve('./node_modules')
  ]
};

config.module = {
  rules: [
    rules.typescript,
    rules.scss
  ]
};

config.output = {
  path: path.resolve('./dist'),
  publicPath: '/'
};

config.plugins = [
  new CheckerPlugin(),
  new DefinePlugin({
    SOUNDCLOUD_CLIENT_ID
  }),
  new ContextReplacementPlugin(
    /angular([\\/])core([\\/])@angular/,
    path.resolve('./src')
  )
];


//=====================================
//  DEVELOPMENT
//-------------------------------------
if (ENV_DEVELOPMENT) {
  config.devtool = 'cheap-module-source-map';

  config.entry = {
    main: './src/main.jit.ts',
    polyfills: './src/polyfills.ts'
  };

  config.output.filename = '[name].js';

  config.plugins.push(
    new CommonsChunkPlugin({
      name: ['main', 'polyfills'],
      minChunks: Infinity
    }),
    new HtmlWebpackPlugin({
      filename: 'index.html',
      hash: false,
      inject: true,
      template: './src/index.html'
    })
  );

  config.devServer = {
    contentBase: './src',
    historyApiFallback: true,
    host: SERVER_HOST,
    port: SERVER_PORT,
    stats: {
      cached: true,
      cachedAssets: true,
      chunks: false,
      chunkModules: false,
      colors: true,
      hash: false,
      maxModules: 300,
      modules: false,
      reasons: false,
      timings: true,
      version: false
    },
    watchOptions: {
      ignored: /node_modules/
    }
  };
}


//=====================================
//  PRODUCTION
//-------------------------------------
if (ENV_PRODUCTION) {
  config.devtool = 'hidden-source-map';

  config.entry = {
    main: './src/main.aot.ts',
    polyfills: './src/polyfills.ts'
  };

  config.output.filename = '[name].[chunkhash].js';

  config.plugins.push(
    new CommonsChunkPlugin({
      name: 'polyfills',
      chunks: ['polyfills']
    }),
    new CommonsChunkPlugin({
      name: 'vendor',
      chunks: ['main'],
      minChunks: module => /node_modules/.test(module.resource)
    }),
    new HtmlWebpackPlugin({
      filename: 'index.html',
      hash: false,
      inject: true,
      template: './src/index.html'
    }),
    new NgcWebpackPlugin({
      disabled: false,
      tsConfig: path.resolve('tsconfig.aot.json')
    }),
    new UglifyJsPlugin({
      comments: false,
      compress: {
        comparisons: true,
        conditionals: true,
        dead_code: true, // eslint-disable-line camelcase
        evaluate: true,
        if_return: true, // eslint-disable-line camelcase
        join_vars: true, // eslint-disable-line camelcase
        negate_iife: false, // eslint-disable-line camelcase
        screw_ie8: true, // eslint-disable-line camelcase
        sequences: true,
        unused: true,
        warnings: false
      },
      mangle: {
        screw_ie8: true // eslint-disable-line camelcase
      },
      sourceMaps: false
    }),
    new WebpackMd5Hash()
  );
}


//=====================================
//  TEST
//-------------------------------------
if (ENV_TEST) {
  config.devtool = 'inline-source-map';

  config.module.rules.push({
    test: /\.(js|ts)$/,
    enforce: 'post',
    use: {
      loader: 'istanbul-instrumenter-loader',
      options: {
        esModules: true
      }
    },
    include: path.resolve('./src'),
    exclude: [
      /\.(e2e|spec)\.ts$/,
      /node_modules/
    ]
  });
}
