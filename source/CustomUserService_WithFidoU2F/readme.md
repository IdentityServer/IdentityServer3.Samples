Sample showing how to use a custom user service that utilises the FIDO U2F specification and security keys such as Yubikeys to provide Two Factor Authentication.

The implementation detail for FIDO U2F uses the OSS library https://github.com/brucedog/u2flib, including the view/controller functionality contained within the demo site for that repository.

The authentication workflow in this sample is that once a user has provided their username/password, a check is made to determine whether they currently have a U2F device registered. 
If not, then they will prompted to set one up - this is then stored against their user account after which login completes.
If they already have a device registered, then they will be prompted to respond with it in order to complete login. In this case, the "amr" claim is updated from "password" to "2FA".

As per the current FIDO U2F implementation, only Google Chrome is supported. The sample has been tested using the Yubikey Neo.