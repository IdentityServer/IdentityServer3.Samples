/// <reference path="oidc-client.js" />

///////////////////////////////
// config
///////////////////////////////
Oidc.Log.logger = console;
Oidc.Log.level = Oidc.Log.INFO;

var settings = {
    authority: "https://localhost:44333/core",
    client_id: "js.usermanager",
    redirect_uri: window.location.protocol + "//" + window.location.host + "/index.html",
    post_logout_redirect_uri: window.location.protocol + "//" + window.location.host + "/index.html",

    // these two will be done dynamically from the buttons clicked, but are
    // needed if you want to use the silent_renew
    response_type: "id_token token",
    scope: "openid profile email read write",

    // silent renew will get a new access_token via an iframe 
    // just prior to the old access_token expiring (60 seconds prior)
    silent_redirect_uri: window.location.protocol + "//" + window.location.host + "/silent_renew.html",
    automaticSilentRenew: true,

    // this will allow all the OIDC protocol claims to vbe visible in the window. normally a client app 
    // wouldn't care about them or want them taking up space
    filterProtocolClaims: true,

    // this will use the user info endpoint if it's an OIDC request and there's an access_token
    loadUserInfo: true
};
var mgr = new Oidc.UserManager(settings);

///////////////////////////////
// events
///////////////////////////////
var user;
mgr.events.addUserLoaded(function (u) {
    user = u;
    console.log("user loaded");
    showUser(user);
});

mgr.events.addUserUnloaded(function () {
    user = null;
    console.log("user unloaded");
    showUser();
});

mgr.events.addAccessTokenExpiring(function () {
    console.log("token expiring");
    log("token expiring");
});

mgr.events.addAccessTokenExpired(function () {
    console.log("token expired");
    log("token expired");
});

mgr.events.addSilentRenewError(function (e) {
    console.log("silent renew error", e.message);
    log("silent renew error", e.message);
});

///////////////////////////////
// UI event handlers
///////////////////////////////
[].forEach.call(document.querySelectorAll(".request"), function (button) {
    button.addEventListener("click", function () {
        signIn(this.dataset["scope"], this.dataset["type"]);
    });
});
document.querySelector('.call').addEventListener("click", callApi, false);
document.querySelector(".logout").addEventListener("click", signOut, false);

///////////////////////////////
// functions for UI elements
///////////////////////////////
function signIn(scope, response_type) {
    mgr.signinRedirect({ scope: scope, response_type: response_type }).then(null, function (e) {
        log(e);
    });
}

function signInCallback() {
    mgr.signinRedirectCallback().then(function (user) {
        var hash = window.location.hash.substr(1);
        var result = hash.split('&').reduce(function (result, item) {
            var parts = item.split('=');
            result[parts[0]] = parts[1];
            return result;
        }, {});
        log(result);
    }).catch(function (error) {
        log(error);
    });
}

function signOut() {
    mgr.signoutRedirect();
}

function callApi() {
    var xhr = new XMLHttpRequest();
    xhr.onload = function (e) {
        if (xhr.status >= 400) {
            logAjaxResult({
                status: xhr.status,
                statusText: xhr.statusText,
                wwwAuthenticate: xhr.getResponseHeader("WWW-Authenticate")
            });
        }
        else {
            logAjaxResult(JSON.parse(xhr.responseText));
        }
    };
    xhr.onerror = function () {
        if (xhr.status === 401) {
            mgr.removeUser();
        }

        logAjaxResult({
            status: xhr.status,
            statusText: xhr.statusText,
            wwwAuthenticate: xhr.getResponseHeader("WWW-Authenticate")
        });
    };
    xhr.open("GET", "http://localhost:2727/identity", true);
    if (user) {
        xhr.setRequestHeader("Authorization", "Bearer " + user.access_token);
    }
    xhr.send();
}

function checkSessionState(user) {
    mgr.metadataService.getCheckSessionIframe().then(function (url) {
        if (url && user && user.session_state) {
            console.log("setting up check session iframe for session state", user.session_state);
            document.getElementById("rp").src = "check_session.html#" +
                "session_state=" + user.session_state +
                "&check_session_iframe=" + url +
                "&client_id=" + mgr.settings.client_id
            ;
        }
        else {
            console.log("no check session url, user, or session state: not setting up check session iframe");
            document.getElementById("rp").src = "about:blank";
        }
    });
}

window.onmessage = function (e) {
    if (e.origin === window.location.protocol + "//" + window.location.host && e.data === "changed") {
        console.log("user session has changed");
        mgr.removeUser();
        mgr.signinSilent().then(function () {
            // Session state changed but we managed to silently get a new identity token, everything's fine
            console.log('renewTokenSilentAsync success');
        }).catch(function (err) {
            // Here we couldn't get a new identity token, we have to ask the user to log in again
            console.log('renewTokenSilentAsync failed', err.message);
        });
    }
}

///////////////////////////////
// init
///////////////////////////////

// clears any old stale requests from storage
mgr.clearStaleState().then(function () {
    console.log("Finished clearing old state");
}).catch(function (e) {
    console.error("Error clearing state:", e.message);
});

// checks to see if we already have a logged in user
mgr.getUser().then(function (user) {
    showUser(user);
}).catch(function (e) {
    log(e);
});

// checks to see if the page being loaded looks like a login callback
if (window.location.hash) {
    signInCallback();
}

///////////////////////////////
// debugging helpers
///////////////////////////////
function log(msg) {
    display("#response", msg);
}
function logIdToken(msg) {
    display("#id-token", msg);
}
function logAccessToken(msg) {
    display("#access-token", msg);
}
function logAjaxResult(msg) {
    display("#ajax-result", msg);
}
function display(selector, msg) {
    document.querySelector(selector).innerText = '';

    if (msg) {
        if (msg instanceof Error) {
            msg = "Error: " + msg.message;
        }
        else if (typeof msg !== 'string') {
            msg = JSON.stringify(msg, null, 2);
        }
        document.querySelector(selector).innerHTML += msg + '\r\n';
    }
}
function showUser(user) {
    if (!user) {
        log("user not signed in");
        logIdToken();
        logAccessToken();
        logAjaxResult();
    }
    else {
        if (user.profile) {
            logIdToken({ profile: user.profile, session_state: user.session_state });
        }
        else {
            logIdToken();
        }
        if (user.access_token) {
            logAccessToken({ access_token: user.access_token, expires_in: user.expires_in, scope: user.scope });
        }
        else {
            logAccessToken();
        }
    }
    checkSessionState(user);
}
