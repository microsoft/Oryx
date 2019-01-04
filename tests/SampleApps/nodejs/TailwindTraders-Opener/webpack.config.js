'use strict';

const path = require('path');
const fs = require('fs');
const ExtractTextPlugin = require("extract-text-webpack-plugin");
const autoprefixer = require('autoprefixer');

const ROUTE_ROOT = './client/src/routes/';

const routeSources = fs.readdirSync(ROUTE_ROOT);
const routes = {};
for (const routeSource of routeSources) {
    routes[routeSource.replace('.js', '')] = ROUTE_ROOT + routeSource;
};

module.exports = {

    entry: routes,

    output: {
        path: path.join(__dirname, './client/dist/src'),
        filename: '[name].js'
    },

    module: {
        loaders: [{
            test: /\.jsx?$/,
            exclude: /(node_modules)/,
            loaders: [ 'babel' ],
        },
        { test: /\.css$/, loader: ExtractTextPlugin.extract('style-loader', 'css-loader') },
        { test: /\.woff(2)?(\?v=[0-9]\.[0-9]\.[0-9])?$/, loader: 'url-loader?limit=10000&minetype=application/font-woff' },
        { test: /\.(ttf|eot|svg)(\?v=[0-9]\.[0-9]\.[0-9])?$/, loader: 'file-loader' }
      ]
    },

     plugins: [
        new ExtractTextPlugin('[name].css', {
            allChunks: true
        })
    ],

    devtool: 'source-map'
};
