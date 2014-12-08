/// <reference path="jasmine-2.1.3/jasmine.js" />
/// <reference path="jasmine-2.1.3/jasmine-html.js" />
/// <reference path="jasmine-2.1.3/boot.js" />
/// <reference path="../oidc/oidcclient.js" />

describe("OidcClient", function () {

    describe("isOidc", function () {
        it('should detect that "id_token" is OIDC', function () {
            var sub = new OidcClient({
                response_type: "id_token"
            });

            expect(sub.isOidc).toBe(true);
        });

        it('should detect that "token" is OAuth', function () {
            var sub = new OidcClient({
                response_type: "token"
            });

            expect(sub.isOAuth).toBe(true);
        });

        it('should detect that "token" is not OIDC', function () {
            var sub = new OidcClient({
                response_type: "token"
            });

            expect(sub.isOidc).toBe(false);
        });

        it('should detect that "id_token" is not OAuth', function () {
            var sub = new OidcClient({
                response_type: "id_token"
            });

            expect(sub.isOAuth).toBe(false);
        });

        it('should detect that "token id_token" is OAuth', function () {
            var sub = new OidcClient({
                response_type: "token id_token"
            });

            expect(sub.isOAuth).toBe(true);
        });

        it('should detect that "id_token token" is OAuth', function () {
            var sub = new OidcClient({
                response_type: "id_token token"
            });

            expect(sub.isOAuth).toBe(true);
        });

        it('should detect that "token id_token" is OIDC', function () {
            var sub = new OidcClient({
                response_type: "token id_token"
            });

            expect(sub.isOidc).toBe(true);
        });

        it('should detect that "id_token token" is OIDC', function () {
            var sub = new OidcClient({
                response_type: "id_token token"
            });

            expect(sub.isOidc).toBe(true);
        });
    });
});

