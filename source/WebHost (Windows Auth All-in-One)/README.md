WebHost (Windows Auth All-in-One)
======================================

This solution is composed of a single project that hosts both:
1. The [IdentityServer.WindowsAuthentication](https://github.com/IdentityServer/WindowsAuthentication) component, which converts windows tokens to identity tokens. Note that for IIS Express, Windows Authentication is enabled via the project's proprty pages. For IIS, please refer to the comment in the web.config file.
2. An instance of identity server which is configured to consume the above component via WS-Federation.
Please note that since both components are hosted within the same web app, no other provider can be configured along with the windows provider concurrently as Windows Authentication is setup at the web app host level.


Thanks to Eran Stiller (@estiller) for contributing this sample. Please contact him directly when you have questions or improvements.