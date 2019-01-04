'use strict';

var _ = require('lodash');
var path = require('path');
var gulp = require('gulp');
var del = require('del');
var webpack = require('webpack');
var eslint = require('gulp-eslint');
var eslintConfig = require('./.eslintrc.json');

gulp.task('populate-mongodb', function populateMongoDb() {
    const mongo = require('./server/db/sources/mongodb');
    return mongo.initializeDatabase();
});

gulp.task('build', ['build-client-img', 'build-client-font', 'build-client-dev']);

gulp.task('build-client-img', ['clean-client'], function buildClientImg() {
    return gulp.src('./client/img/**/*')
        .pipe(gulp.dest('./client/dist/img'));
});

gulp.task('build-client-font', ['clean-client'], function buildClientFont() {
    return gulp.src('./client/fonts/**/*')
        .pipe(gulp.dest('./client/dist/fonts'));
});

gulp.task('build-client-html', ['clean-client'], function buildClientHtml() {
    return gulp.src('./client/html/*')
        .pipe(gulp.dest('./client/dist/'));
});

gulp.task('build-client-dev', ['clean-client'], function buildClientDev(cb) {
    webpack(require('./webpack.config.js'), function webpackCallback(err, stats) {
        if (err) {
            return cb(err);
        }

        if (stats.compilation.errors && stats.compilation.errors.length) {
            console.error(stats.compilation.errors);
            return cb(new Error('Webpack failed to compile the application.'));
        }

        cb();
    });
});

gulp.task('lint-client', function lintClient() {
    var config = _.assign({
        parserOptions: {
            ecmaVersion: 6,
            sourceType: 'module',
            ecmaFeatures: {
                modules: true,
                jsx: true
            }
        },

        env: {
            browser: true,
            es6: true
        },
        plugins: ['react']
    }, eslintConfig);

    // note that when we return the stream here, builds from the root (e.g., `gulp lint build`) will
    // write some output to the wrong directory.  I don't understand why, but not returning anything
    // here seems to work around the issue and the build still fails if there is a linter error.
    gulp.src('./client/src/**/*.js')
        // eslint() attaches the lint output to the "eslint" property
        // of the file object so it can be used by other modules.
        .pipe(eslint(config))
        // eslint.format() outputs the lint results to the console.
        // Alternatively use eslint.formatEach() (see Docs).
        .pipe(eslint.format())
        // To have the process exit with an error code (1) on
        // lint error, return the stream and pipe to failAfterError last.
        .pipe(eslint.failAfterError());
});

gulp.task('lint-server', function lintServer() {
    var config = _.assign({
        parserOptions: {
            ecmaVersion: 6
        },
        env: {
            node: true,
            es6: true
        }
    }, eslintConfig);

    // note that when we return the stream here, builds from the root (e.g., `gulp lint build`) will
    // write some output to the wrong directory.  I don't understand why, but not returning anything
    // here seems to work around the issue and the build still fails if there is a linter error.
    gulp.src('./server/**/*')
        // eslint() attaches the lint output to the "eslint" property
        // of the file object so it can be used by other modules.
        .pipe(eslint(config))
        // eslint.format() outputs the lint results to the console.
        // Alternatively use eslint.formatEach() (see Docs).
        .pipe(eslint.format())
        // To have the process exit with an error code (1) on
        // lint error, return the stream and pipe to failAfterError last.
        .pipe(eslint.failAfterError());
});

gulp.task('lint-gateway', function lintGateway() {
    var config = _.assign({
        parserOptions: {
            ecmaVersion: 6,
            sourceType: 'module',
            ecmaFeatures: {
                modules: true,
                jsx: true
            }
        },
        env: {
            node: true,
            es6: true
        }
    }, eslintConfig);

    // note that when we return the stream here, builds from the root (e.g., `gulp lint build`) will
    // write some output to the wrong directory.  I don't understand why, but not returning anything
    // here seems to work around the issue and the build still fails if there is a linter error.
    gulp.src(['./apigateway/**/*.js', '!./apigateway/node_modules/**/*'])
        // eslint() attaches the lint output to the "eslint" property
        // of the file object so it can be used by other modules.
        .pipe(eslint(config))
        // eslint.format() outputs the lint results to the console.
        // Alternatively use eslint.formatEach() (see Docs).
        .pipe(eslint.format())
        // To have the process exit with an error code (1) on
        // lint error, return the stream and pipe to failAfterError last.
        .pipe(eslint.failAfterError());
});

gulp.task('clean-client', function cleanClient() {
    del.sync(['./client/dist/**']);
});

gulp.task('clean', ['clean-client']);

gulp.task('lint', ['lint-client', 'lint-server', 'lint-gateway']);

gulp.task('default', ['build-client-dev', 'build-client-img', 'build-client-font', 'build-client-html']);

gulp.task('ci', ['build']);