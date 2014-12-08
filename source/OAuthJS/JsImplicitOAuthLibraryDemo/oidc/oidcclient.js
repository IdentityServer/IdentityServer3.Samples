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



function copy(obj, target) {
    target = target || {};
    for (var key in obj) {
        if (obj.hasOwnProperty(key)) {
            target[key] = obj[key];
        }
    }
    return target;
}

function rand() {
    return ((Date.now() + Math.random()) * Math.random()).toString().replace(".", "");
}

function error(message) {
    return Promise.reject(Error(message));
}

function parseOidcResult(queryString) {
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

var requestDataKey = "OidcClient.requestDataKey";

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

    Object.defineProperty(this, "isOidc", {
        get: function () {
            if (this._settings.response_type) {
                var result = this._settings.response_type.split(/\s+/g).filter(function (item) {
                    return item === "id_token";
                });
                return !!(result[0]);
            }
            return false;
        }
    });

    Object.defineProperty(this, "isOAuth", {
        get: function () {
            if (this._settings.response_type) {
                var result = this._settings.response_type.split(/\s+/g).filter(function (item) {
                    return item === "token";
                });
                return !!(result[0]);
            }
            return false;
        }
    });
}

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

OidcClient.prototype.loadAuthorizationEndpoint = function () {

    if (this._settings.authorization_endpoint) {
        return Promise.resolve(this._settings.authorization_endpoint);
    }

    if (!this._settings.authority) {
        return error("No authorization_endpoint configured");
    }

    return this.loadMetadataAsync().then(function (metadata) {
        if (!metadata.authorization_endpoint) {
            return error("Metadata does not contain authorization_endpoint");
        }

        return metadata.authorization_endpoint;
    });
};

OidcClient.prototype.createTokenRequestAsync = function () {
    var client = this;
    var settings = client._settings;

    return client.loadAuthorizationEndpoint().then(function (authorization_endpoint) {
        var state = rand();

        var url =
            authorization_endpoint + "?state=" + encodeURIComponent(state);

        if (client.isOidc) {
            var nonce = rand();
            url += "&nonce=" + encodeURIComponent(nonce);
        }

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
            oidc: client.isOidc,
            oauth: client.isOAuth,
            state: state
        };

        if (nonce) {
            data["nonce"] = nonce;
        }

        settings.store.setItem(requestDataKey, JSON.stringify(data));

        return {
            data: data,
            url: url
        };
    });
}

OidcClient.prototype.loadMetadataAsync = function () {
    var settings = this._settings;

    if (settings.metadata) {
        Promise.resolve(settings.metadata);
    }

    if (!settings.authority) {
        error("No authority configured");
    }

    return getJson(settings.authority)
        .then(function (metadata) {
            settings.metadata = metadata;
            return metadata;
        }, function (err) {
            error("Failed to load metadata (" + err.message + ")");
        });
};

OidcClient.prototype.loadX509SigningKeyAsync = function () {
    var settings = this._settings;

    function getKeyAsync(jwks) {
        if (!jwks.keys || !jwks.keys.length) {
            return error("Signing keys empty");
        }

        var key = jwks.keys[0];
        if (key.kty != "RSA") {
            return error("Signing key not RSA");
        }

        if (!key.x5c || !key.x5c.length) {
            return error("RSA keys empty");
        }

        return Promise.resolve(key.x5c[0]);
    }

    if (settings.jwks) {
        return getKeyAsync(settings.jwks);
    }

    return this.loadMetadataAsync().then(function (metadata) {
        if (!metadata.jwks_uri) {
            return error("Metadata does not contain jwks_uri");
        }

        return getJson(metadata.jwks_uri).then(function (jwks) {
            settings.jwks = jwks;
            return getKeyAsync(jwks);
        }, function (err) {
            return error("Failed to load signing keys (" + err.message + ")");
        });
    });
};

OidcClient.prototype.validateIdTokenAsync = function (jwt, nonce) {

    return this.loadX509SigningKeyAsync().then(function (cert) {

        var jws = new KJUR.jws.JWS();
        if (jws.verifyJWSByPemX509Cert(jwt, cert)) {
            var id_token = JSON.parse(jws.parsedJWS.payloadS);

            if (nonce !== id_token.nonce) {
                return error("Invalid nonce");
            }

            return id_token;
        }
        else {
            return error("JWT failed to validate");
        }

    });

};

OidcClient.prototype.validateAccessTokenAsync = function (id_token, id_token_jwt, access_token) {

    if (!id_token.at_hash) {
        return error("No at_hash in id_token");
    }

    return this.loadX509SigningKeyAsync().then(function (cert) {
        var jws = new KJUR.jws.JWS();
        if (jws.verifyJWSByPemX509Cert(id_token_jwt, cert)) {
            if (jws.parsedJWS.headP.alg != "RS256") {
                return error("JWT signature alg not supported");
            }

            var hash = KJUR.crypto.Util.sha256(access_token);
            var left = hash.substr(0, hash.length / 2);
            var left_b64u = hextob64u(left);

            if (left_b64u !== id_token.at_hash) {
                return error("at_hash failed to validate");
            }
        }
        else {
            return error("JWT failed to validate");
        }
    });
};

OidcClient.prototype.loadUserProfile = function (access_token, id_token) {

    return this.loadMetadataAsync().then(function (metadata) {

        if (!metadata.userinfo_endpoint) {
            return Promise.reject(Error("Metadata does not contain userinfo_endpoint"));
        }

        return getJson(metadata.userinfo_endpoint, access_token).then(function (response) {

            return copy(response, id_token);

        });
    });
}

OidcClient.prototype.validateIdTokenAndAccessTokenAsync = function (id_token_jwt, nonce, access_token) {
    var client = this;

    return client.validateIdTokenAsync(id_token_jwt, nonce).then(function (id_token) {
        if (!id_token) {
            return error("Invalid identity token");
        }

        return client.loadMetadataAsync().then(function (metadata) {

            if (id_token.iss !== metadata.issuer) {
                return error("Invalid issuer");
            }

            if (id_token.aud !== settings.client_id) {
                return error("Invalid audience");
            }

            var now = parseInt(Date.now() / 1000);

            // accept tokens issued up to 5 mins ago
            var diff = now - id_token.iat;
            if (diff > (5 * 60)) {
                return error("Token issued too long ago");
            }

            if (id_token.exp < now) {
                return error("Token expired");
            }

            return client.validateAccessTokenAsync(id_token, result.id_token, token).then(function () {

                return client.loadUserProfile(token, id_token);

            });

        });

    });
}

OidcClient.prototype.readResponseAsync = function (queryString) {

    var client = this;
    var settings = client._settings;

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

    var result = parseOidcResult(queryString);
    if (!result) {
        return error("No OIDC response");
    }

    if (result.error) {
        return error(result.error);
    }

    if (result.state !== data.state) {
        return error("Invalid state");
    }

    if (data.oidc) {
        if (!result.id_token) {
            return error("No identity token");
        }

        if (!data.nonce) {
            return error("No nonce loaded");
        }
    }

    if (data.oauth) {
        if (!result.access_token) {
            return error("No access token");
        }

        if (result.token_type !== "Bearer") {
            return error("Invalid token type");
        }

        if (!result.expires_in) {
            return error("No token expiration");
        }
    }

    var promise = Promise.resolve();
    if (data.oidc && data.oauth) {
        promise = client.validateIdTokenAndAccessTokenAsync(result.id_token, data.nonce, result.access_token);
    }
    if (data.oidc) {
        promise = client.validateIdTokenAsync(result.id_token, data.nonce);
    }

    return promise.then(function (id_token) {
        return {
            id_token: id_token,
            id_token_jwt: result.id_token,
            access_token: result.token,
            expires_in: result.expires_in
        };
    });
}

