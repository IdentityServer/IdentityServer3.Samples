/// <reference path="jasmine-2.1.3/jasmine.js" />
/// <reference path="jasmine-2.1.3/jasmine-html.js" />
/// <reference path="jasmine-2.1.3/boot.js" />
/// <reference path="../oidc/defaultPromiseFactory.js" />

describe('DefaultPromiseFactory', function () {
    'use strict';

    /** @type DefaultPromiseFactory */
    var promiseFactory;

    beforeEach(function () {
        promiseFactory = new DefaultPromiseFactory();
    });

    describe('resolve', function () {
        it('Returns a resolved promise.', function (done) {
            var value = {};

            promiseFactory.resolve(value)
                .then(function (result) {
                    expect(result).toBe(value);
                }, function (err) {
                    jasmine.getEnv().fail('The promise must be resolved. Error: ' + err);
                })
                .then(done, done);
        });
    });

    describe('reject', function () {
        it('Returns a rejected promise.', function (done) {
            var value = {};

            promiseFactory.reject(value)
                .then(function (result) {
                    jasmine.getEnv().fail('The promise must be rejected. Value: ' + result);
                }, function (err) {
                    expect(err).toBe(value);
                })
                .then(done, done);
        });
    });

    describe('create', function () {

        it('Returns a rejected promise when the callback calls "reject".', function (done) {
            var value = {};

            promiseFactory
                .create(function (resolve, reject) {
                    reject(value);
                })
                .then(function (result) {
                    jasmine.getEnv().fail('The promise must be rejected. Value: ' + result);
                }, function (err) {
                    expect(err).toBe(value);
                })
                .then(done, done);
        });

        it('Returns a resolved promise when the callback calls "resolve".', function (done) {
            var value = {};

            promiseFactory
                .create(function (resolve, reject) {
                    resolve(value);
                })
                .then(function (result) {
                    expect(result).toBe(value);
                }, function (err) {
                    jasmine.getEnv().fail('The promise must be resolved. Error: ' + err);
                })
                .then(done, done);
        });

        it('Returns a promise that can be chained', function (done) {
            var value = {};

            promiseFactory
                .create(function (resolve, reject) {
                    resolve();
                })
                .then(function (result) {
                    expect(result).toBeUndefined();
                    return value;
                }, function (err) {
                    jasmine.getEnv().fail('The promise must be resolved. Error: ' + err);
                })
                .then(function (result) {
                    expect(result).toBe(value);
                }, function (err) {
                    jasmine.getEnv().fail('The promise must be resolved. Error: ' + err);
                })
                .then(done, done);
        });

        it('Returns a promise that can change from resolved to rejected.', function (done) {
            var value = {};

            promiseFactory
                .create(function (resolve, reject) {
                    resolve();
                })
                .then(function (result) {
                    expect(result).toBeUndefined();
                    return promiseFactory.reject(value);
                }, function (err) {
                    jasmine.getEnv().fail('The promise must be resolved. Error: ' + err);
                })
                .then(function (result) {
                    jasmine.getEnv().fail('The promise must be rejected. Value: ' + result);
                }, function (err) {
                    expect(err).toBe(value);
                })
                .then(done, done);
        });
    });


});

