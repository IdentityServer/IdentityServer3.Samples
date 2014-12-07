/// <reference path="es6-promise-2.0.0.js" />
/// <reference path="base64.js" />
/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */



(function () {
    "use strict";

    function copy(obj, target) {
        target = target || {};
        for (var key in obj) {
            if (obj.hasOwnProperty(key)) {
                target[key] = obj[key];
            }
        }
        return target;
    }

    function OidcClient(settings) {
        this._settings = settings || {};

        if (this._settings.authority && this._settings.authority.indexOf('.well-known/openid-configuration') < 0) {
            if (this._settings.authority[this._settings.authority.length - 1] != '/') {
                this._settings.authority += '/';
            }
            this._settings.authority += '.well-known/openid-configuration';
        }

        if (!this._settings.response_type) {
            this._settings.response_type = "id_token token";
        }

        if (!this._settings.store) {
            this._settings.store = window.localStorage;
        }
    }

    var requestDataKey = "OidcClient.requestDataKey";

    OidcClient.prototype.redirectForToken = function () {
        this.createTokenRequestAsync().then(function (request) {
            window.location = request.url;
        }, function (err) {
            console.error(err);
        });
    }

    OidcClient.prototype.redirectForLogout = function (id_token_hint) {
        var settings = this._settings;
        this.loadMetadataAsync().then(function (metadata) {
            if (!metadata.end_session_endpoint) {
                console.error("No end_session_endpoint in metadata");
            }
            var url = metadata.end_session_endpoint;
            if (id_token_hint && settings.post_logout_redirect_uri) {
                url += "?post_logout_redirect_uri=" + settings.post_logout_redirect_uri;
                url += "&id_token_hint=" + id_token_hint;
            }
            window.location = url;
        }, function (err) {
            console.error(err);
        });
    }

    function rand() {
        return ((Date.now() + Math.random()) * Math.random()).toString().replace(".", "");
    }

    OidcClient.prototype.loadAuthorizationEndpoint = function (settings) {
        settings = settings || this._settings;

        if (settings.authorization_endpoint) {
            return Promise.resolve(settings.authorization_endpoint);
        }

        if (!settings.authority) {
            return Promise.reject(Error("No authorization_endpoint configured"));
        }

        return this.loadMetadataAsync(settings).then(function (metadata) {
            if (!metadata.authorization_endpoint) {
                return Promise.reject("Metadata does not contain authorization_endpoint");
            }
            return metadata.authorization_endpoint;
        });
    };

    OidcClient.prototype.createTokenRequestAsync = function (settings) {
        settings = settings || this._settings;

        return this.loadAuthorizationEndpoint(settings).then(function (authorization_endpoint) {
            var state = rand();
            var nonce = rand();

            var url =
                authorization_endpoint +
                "?state=" + encodeURIComponent(state) +
                "&nonce=" + encodeURIComponent(nonce);

            var required = ["client_id", "redirect_uri", "response_type", "scope"];
            required.forEach(function (key) {
                var value = settings[key];
                if (value) {
                    url += "&" + key + "=" + encodeURIComponent(value);
                }
            });

            var optional = ["prompt", "display", "max_age", "ui_locales", "id_token_hint", "login_hint", "acr_values"];
            optional.forEach(function (key) {
                var value = settings[key];
                if (value) {
                    url += "&" + key + "=" + encodeURIComponent(value);
                }
            });

            var data = {
                state: state,
                nonce: nonce
            }
            settings.store.setItem(requestDataKey, JSON.stringify(data));

            return {
                data: data,
                url: url
            };
        });
    }

    OidcClient.prototype.parseResult = function (queryString) {
        queryString = queryString || location.hash;

        var idx = queryString.lastIndexOf("#");
        if (idx >= 0) {
            queryString = queryString.substr(idx + 1);
        }

        var params = {},
            regex = /([^&=]+)=([^&]*)/g,
            m;

        var counter = 0;
        while (m = regex.exec(queryString)) {
            params[decodeURIComponent(m[1])] = decodeURIComponent(m[2]);
            if (counter++ > 50) {
                return {
                    error: "Response exceeded expected number of parameters"
                };
            }
        }

        for (var prop in params) {
            return params;
        }
    }

    function getJson(url, token) {
        return new Promise(function (resolve, reject) {

            var xhr = new XMLHttpRequest();
            xhr.open("GET", url);
            xhr.responseType = "json";
            if (token) {
                xhr.setRequestHeader("Authorization", "Bearer " + token);
            }

            xhr.onload = function () {
                if (xhr.status === 200) {
                    var response = xhr.response;
                    if (typeof response === "string") {
                        response = JSON.parse(response);
                    }
                    resolve(response);
                }
                else {
                    reject(Error(xhr.statusText + "(" + xhr.status + ")"));
                }
            };

            xhr.onerror = function () {
                reject(Error("Network error"));
            }

            xhr.send();
        });
    }

    OidcClient.prototype.loadMetadataAsync = function (settings) {
        settings = settings || this._settings;

        if (settings.metadata) {
            Promise.resolve(settings.metadata);
        }

        if (!settings.authority) {
            Promise.reject(Error("No authority configured"));
        }

        return getJson(settings.authority)
            .then(function (metadata) {
                settings.metadata = metadata;
                return metadata;
            }, function (err) {
                Promise.reject(Error("Failed to load metadata (" + err.message + ")"));
            });
    };

    OidcClient.prototype.loadX509SigningKeyAsync = function (settings) {
        settings = settings || this._settings;

        function getKeyAsync(jwks) {
            if (!jwks.keys || !jwks.keys.length) {
                return Promise.reject(Error("Signing keys empty"));
            }

            var key = jwks.keys[0];
            if (key.kty != "RSA") {
                return Promise.reject(Error("Signing key not RSA"));
            }

            if (!key.x5c || !key.x5c.length) {
                return Promise.reject(Error("RSA keys empty"));
            }

            return Promise.resolve(key.x5c[0]);
        }
        
        if (settings.jwks) {
            return getKeyAsync(settings.jwks);
        }

        return this.loadMetadataAsync(settings).then(function (metadata) {
            if (!metadata.jwks_uri) {
                return Promise.reject(Error("Metadata does not contain jwks_uri"));
            }

            return getJson(metadata.jwks_uri).then(function (jwks) {
                settings.jwks = jwks;
                return getKeyAsync(jwks);
            }, function (err) {
                return Promise.reject(Error("Failed to load signing keys (" + err.message + ")"));
            });
        });
    };

    OidcClient.prototype.validateJwtAsync = function (jwt, settings) {
        return this.loadX509SigningKeyAsync(settings).then(function (cert) {
            var jws = new KJUR.jws.JWS();
            if (jws.verifyJWSByPemX509Cert(jwt, cert)) {
                return JSON.parse(jws.parsedJWS.payloadS);
            }
            else {
                return Promise.reject(Error("JWT failed to validate"));
            }
        });
    };

    OidcClient.prototype.validateAccessTokenAsync = function (id_token, id_token_jwt, access_token) {

        if (!id_token.at_hash) {
            return Promise.reject("No at_hash in id_token");
        }

        return this.loadX509SigningKeyAsync().then(function (cert) {
            var jws = new KJUR.jws.JWS();
            if (jws.verifyJWSByPemX509Cert(id_token_jwt, cert)) {
                if (jws.parsedJWS.headP.alg != "RS256") {
                    return Promise.reject(Error("JWT signature alg not supported"));
                }

                var hash = KJUR.crypto.Util.sha256(access_token);
                var left = hash.substr(0, hash.length / 2);
                var left_b64u = hextob64u(left);

                if (left_b64u !== id_token.at_hash) {
                    return Promise.reject(Error("at_hash failed to validate"));
                }
            }
            else {
                return Promise.reject(Error("JWT failed to validate"));
            }
        });
    };

    OidcClient.prototype.loadUserProfile = function (access_token, id_token) {

        var settings = this._settings;

        return this.loadMetadataAsync(settings).then(function (metadata) {

            if (!metadata.userinfo_endpoint) {
                return Promise.reject(Error("Metadata does not contain userinfo_endpoint"));
            }

            return getJson(metadata.userinfo_endpoint, access_token).then(function (response) {

                return copy(response, id_token);

            });
        });
    }
    
    OidcClient.prototype.readResponseAsync = function (queryString) {

        var client = this;
        var settings = client._settings;

        function error(message) {
            return Promise.reject(Error(message));
        }

        var data = settings.store.getItem(requestDataKey);
        settings.store.removeItem(requestDataKey);

        if (!data) {
            return error("No request state loaded");
        }

        data = JSON.parse(data);
        if (!data) {
            return error("No request state loaded");
        }

        if (!data.state) {
            return error("No state loaded");
        }

        if (!data.nonce) {
            return error("No nonce loaded");
        }

        var result = OidcClient.prototype.parseResult(queryString);
        if (!result) {
            return error("No OIDC response");
        }

        if (result.error) {
            return error(result.error);
        }

        if (result.state !== data.state) {
            return error("Invalid state");
        }

        var token = result.access_token;
        if (!token) {
            return error("No access token");
        }

        if (result.token_type !== "Bearer") {
            return error("Invalid token type");
        }

        var expires_in = result.expires_in;
        if (!expires_in) {
            return error("No token expiration");
        }

        if (!result.id_token) {
            return error("No identity token");
        }

        return client.validateJwtAsync(result.id_token).then(function (id_token) {
            if (!id_token) {
                return error("Invalid identity token");
            }

            if (data.nonce !== id_token.nonce) {
                return error("Invalid nonce");
            }

            return client.loadMetadataAsync().then(function (metadata) {

                if (id_token.iss !== metadata.issuer) {
                    return error("Invalid issuer");
                }

                if (id_token.aud !== settings.client_id) {
                    return error("Invalid audience");
                }

                var now = parseInt(Date.now() / 1000);

                // accept tokens issues up to 5 mins ago
                var diff = now - id_token.iat;
                if (diff > (5 * 60)) {
                    return error("Token issued too long ago");
                }

                if (id_token.exp < now) {
                    return error("Token expired");
                }

                return client.validateAccessTokenAsync(id_token, result.id_token, token).then(function () {

                    return client.loadUserProfile(token, id_token).then(function (id_token) {

                        return Token.fromResponse({
                            id_token: id_token,
                            id_token_jwt: result.id_token,
                            access_token: token,
                            expires_in: expires_in
                        });

                    });

                });

            });

        });
    }

    function Token(id_token, id_token_jwt, access_token, expires_at) {
        this.id_token = id_token;
        this.id_token_jwt = id_token_jwt;
        this.access_token = access_token;
        this.expires_at = parseInt(expires_at);

        Object.defineProperty(this, "expired", {
            get: function () {
                var now = parseInt(Date.now() / 1000);
                return this.expires_at < now;
            }
        });

        Object.defineProperty(this, "expires_in", {
            get: function () {
                var now = parseInt(Date.now() / 1000);
                return this.expires_at - now;
            }
        });
    }

    Token.fromResponse = function (response) {
        var now = parseInt(Date.now() / 1000);
        var expires_at = now + parseInt(response.expires_in);
        return new Token(response.id_token, response.id_token_jwt, response.access_token, expires_at);
    }

    Token.fromJSON = function (json) {
        if (json) {
            try {
                var obj = JSON.parse(json);
                return new Token(obj.id_token, obj.id_token_jwt, obj.access_token, obj.expires_at);
            }
            catch (e) {
            }
        }
        return new Token(null, 0, null);
    }

    Token.prototype.toJSON = function () {
        return JSON.stringify({
            id_token: this.id_token,
            id_token_jwt: this.id_token_jwt,
            access_token: this.access_token,
            expires_at : this.expires_at
        });
    }

    function FrameLoader(url) {
        this.url = url;
    }

    FrameLoader.prototype.loadAsync = function (url) {
        url = url || this.url;

        if (!url) {
            return Promise.reject("No url provided");
        }

        return new Promise(function (resolve, reject) {
            var frameHtml = '<iframe style="display:none"></iframe>';
            var frame = $(frameHtml).appendTo("body");

            function cleanup() {
                window.removeEventListener("message", message, false);
                if (handle) {
                    window.clearTimeout(handle);
                }
                handle = null;
                frame.remove();
            }

            function cancel(e) {
                cleanup();
                reject();
            }

            function message(e) {
                if (handle && e.origin === location.protocol + "//" + location.host) {
                    cleanup();
                    resolve(e.data);
                }
            }

            var handle = window.setTimeout(cancel, 5000);
            window.addEventListener("message", message, false);
            frame.attr("src", url);
        });
    }

    function TokenManager(settings) {
        this._settings = settings || {};

        this._settings.persist = this._settings.persist || true;
        this._settings.store = this._settings.store || window.localStorage;

        this._callbacks = {
            tokenRemovedCallbacks: [],
            tokenExpiringCallbacks: [],
            tokenExpiredCallbacks: [],
            tokenObtainedCallbacks: []
        };

        Object.defineProperty(this, "id_token", {
            get: function () {
                if (this._token) {
                    return this._token.id_token;
                }
            }
        });
        Object.defineProperty(this, "id_token_jwt", {
            get: function () {
                if (this._token) {
                    return this._token.id_token_jwt;
                }
            }
        });
        Object.defineProperty(this, "access_token", {
            get: function () {
                if (this._token && !this._token.expired) {
                    return this._token.access_token;
                }
            }
        });
        Object.defineProperty(this, "expired", {
            get: function () {
                if (this._token) {
                    return this._token.expired;
                }
                return true;
            }
        });
        Object.defineProperty(this, "expires_in", {
            get: function () {
                if (this._token) {
                    return this._token.expires_in;
                }
                return 0;
            }
        });
        Object.defineProperty(this, "expires_at", {
            get: function () {
                if (this._token) {
                    return this._token.expires_at;
                }
                return 0;
            }
        });

        loadToken(this);
        configureTokenExpired(this);
        configureAutoRenewToken(this);

        // delay this so consuming apps can register for callbacks first
        var mgr = this;
        window.setTimeout(function () {
            configureTokenExpiring(mgr);
        }, 0);
    }

    var storageKey = "TokenManager.token";

    TokenManager.prototype.saveToken = function (token) {
        this._token = token;

        if (this._settings.persist && !this.expired) {
            this._settings.store.setItem(storageKey, token.toJSON());
        }
        else {
            this._settings.store.removeItem(storageKey);
        }

        if (token) {
            callTokenObtained(this);
        }
        else {
            callTokenRemoved(this);
        }
    }

    function loadToken(mgr) {
        if (mgr._settings.persist) {
            var tokenJson = mgr._settings.store.getItem(storageKey);
            if (tokenJson) {
                var token = Token.fromJSON(tokenJson);
                if (!token.expired) {
                    mgr._token = token;
                }
            }
        }
    }

    function callTokenRemoved(mgr) {
        mgr._callbacks.tokenRemovedCallbacks.forEach(function (cb) {
            cb();
        });
    }

    function callTokenExpiring(mgr) {
        mgr._callbacks.tokenExpiringCallbacks.forEach(function (cb) {
            cb();
        });
    }

    function callTokenExpired(mgr) {
        mgr._callbacks.tokenExpiredCallbacks.forEach(function (cb) {
            cb();
        });
    }

    function callTokenObtained(mgr) {
        mgr._callbacks.tokenObtainedCallbacks.forEach(function (cb) {
            cb();
        });
    }

    TokenManager.prototype.addOnTokenRemoved = function (cb) {
        this._callbacks.tokenRemovedCallbacks.push(cb);
    }

    TokenManager.prototype.addOnTokenObtained = function (cb) {
        this._callbacks.tokenObtainedCallbacks.push(cb);
    }

    TokenManager.prototype.addOnTokenExpiring = function (cb) {
        this._callbacks.tokenExpiringCallbacks.push(cb);
    }

    TokenManager.prototype.addOnTokenExpired = function (cb) {
        this._callbacks.tokenExpiredCallbacks.push(cb);
    }

    TokenManager.prototype.removeToken = function () {
        this.saveToken(null);
    }

    TokenManager.prototype.redirectForToken = function () {
        var oidc = new OidcClient(this._settings);
        oidc.redirectForToken();
    }

    TokenManager.prototype.redirectForLogout = function () {
        var oidc = new OidcClient(this._settings);
        var id_token_jwt = this.id_token_jwt;
        this.removeToken();
        oidc.redirectForLogout(id_token_jwt);
    }

    TokenManager.prototype.processTokenCallbackAsync = function (queryString) {
        var mgr = this;
        var oidc = new OidcClient(mgr._settings);
        return oidc.readResponseAsync(queryString).then(function (token) {
            mgr.saveToken(token);
        });
    }

    TokenManager.prototype.renewTokenSilentAsync = function () {
        var mgr = this;

        if (!mgr._settings.silent_redirect_uri) {
            return Promise.reject("silent_redirect_uri not configured");
        }

        var settings = copy(mgr._settings);
        settings.redirect_uri = settings.silent_redirect_uri;
        settings.prompt = "none";

        var oidc = new OidcClient(settings);
        return oidc.createTokenRequestAsync().then(function (request) {
            var frame = new FrameLoader(request.url);
            return frame.loadAsync().then(function(hash) {
                return oidc.readResponseAsync(hash).then(function (token) {
                    mgr.saveToken(token);
                });
            });
        });
    }

    TokenManager.prototype.processTokenCallbackSilent = function () {
        if (window.top && window !== window.top) {
            var hash = window.location.hash;
            if (hash) {
                window.top.postMessage(hash, location.protocol + "//" + location.host);
            }
        };
    }

    function configureTokenExpiring(mgr) {

        function callback() {
            handle = null;
            callTokenExpiring(mgr);
        }

        var handle = null;
        function cancel() {
            if (handle) {
                window.clearTimeout(handle);
                handle = null;
            }
        }

        function setup(duration) {
            handle = window.setTimeout(callback, duration * 1000);
        }

        function configure() {
            cancel();

            if (!mgr.expired) {
                var duration = mgr.expires_in;
                if (duration > 60) {
                    setup(duration - 60);
                }
                else {
                    callback();
                }
            }
        }
        configure();

        mgr.addOnTokenObtained(configure);
        mgr.addOnTokenRemoved(cancel);
    }

    function configureAutoRenewToken(mgr) {

        if (mgr._settings.silent_redirect_uri && mgr._settings.silent_renew) {

            mgr.addOnTokenExpiring(function () {
                mgr.renewTokenSilentAsync().catch(function (e) {
                    console.error(e.message || e);
                });
            });

        }
    }

    function configureTokenExpired(mgr) {

        function callback() {
            handle = null;

            if (mgr._token) {
                mgr.saveToken(null);
            }

            callTokenExpired(mgr);
        }

        var handle = null;
        function cancel() {
            if (handle) {
                window.clearTimeout(handle);
                handle = null;
            }
        }

        function setup(duration) {
            handle = window.setTimeout(callback, duration * 1000);
        }

        function configure() {
            cancel();
            if (mgr.expires_in > 0) {
                // register 1 second beyond expiration so we don't get into edge conditions for expiration
                setup(mgr.expires_in + 1);
            }
        }
        configure();

        mgr.addOnTokenObtained(configure);
        mgr.addOnTokenRemoved(cancel);
    }

    // exports
    window.TokenManager = TokenManager;
})();
