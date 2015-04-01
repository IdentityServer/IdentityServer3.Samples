/**
 * @constructor
 */
function DefaultHttpRequest() {

    /**
     * @name _promiseFactory
     * @type DefaultPromiseFactory
     */

    /**
     * @param {XMLHttpRequest} xhr
     * @param {object.<string, string>} headers
     */
    function setHeaders(xhr, headers) {
        var keys = Object.keys(headers);

        for (var i = 0; i < keys.length; i++) {
            var key = keys[i];
            var value = headers[key];

            xhr.setRequestHeader(key, value);
        }
    }

    /**
     * @param {string} url
     * @param {{ headers: object.<string, string> }} [config]
     * @returns {Promise}
     */
    this.getJSON = function (url, config) {
        return _promiseFactory.create(function (resolve, reject) {

            try {
                var xhr = new XMLHttpRequest();

                if ("withCredentials" in xhr) {
                    // XHR for Chrome/Firefox/Opera/Safari.
                    xhr.responseType = "json";
                    if (config) {
                        if (config.headers) {
                            setHeaders(xhr, config.headers);
                        }
                    }
                }
                else if (typeof XDomainRequest != "undefined") {
                    // XDomainRequest for IE.
                    xhr = new XDomainRequest();
                }
                
                xhr.open("GET", url);

                xhr.onload = function () {
                    try {
                        var response = null;
                        if (xhr.status === 200) {
                            response = xhr.response;
                        }
                        else if (typeof xhr.responseText != "undefined") {
                            response = xhr.responseText;
                        }
                        else {
                            reject(Error(xhr.statusText + "(" + xhr.status + ")"));
                        }

                        if (typeof response === "string") {
                            response = JSON.parse(response);
                        }
                        resolve(response);
                    }
                    catch (err) {
                        reject(err);
                    }
                };

                xhr.onerror = function () {
                    reject(Error("Network error"));
                };

                xhr.send();
            }
            catch (err) {
                return reject(err);
            }
        });
    };
}

_httpRequest = new DefaultHttpRequest();
