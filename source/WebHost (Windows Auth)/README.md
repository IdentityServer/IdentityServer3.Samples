WebHost (Windows Auth)
======================================

This solution is composed of two projects:
1. WindowsAuthWebHost - hosts the [IdentityServer.WindowsAuthentication](https://github.com/IdentityServer/WindowsAuthentication) component, which converts windows tokens to identity tokens. Note that for IIS Express, Windows Authentication is enabled via the project's proprty pages. For IIS, please refer to the comment in the web.config file.
2. WebHost - hosts an instance of identity server which is configured to consume the above WindowsAuthWebHost via WS-Federation.