# abort the script on error
$ErrorActionPreference = "Stop"

function Add-CertificateToStore
{
    param(
        [System.Security.Cryptography.X509Certificates.StoreName] $store,
        [System.Security.Cryptography.X509Certificates.StoreLocation] $location,
        [string] $certificateFile,
        [string] $password = $null
    )

    # load the certificate to a byte array
    $certificateBytes = [System.IO.File]::ReadAllBytes($certificateFile)

    # create the parameters for the x509Certificae 
    # add the password parameter if it is not null
    # add a comma before the byte array because powershell will interpret a single array parameter incorrectly
    $x509CertificateArgumentList = @(, $certificateBytes)
    if($password -ne $null)
    {
        $x509CertificateArgumentList = $x509CertificateArgumentList + $password
    }

    # create the x509 certificate and the certificate store
    $x509Certificate = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2 `
        -ArgumentList $x509CertificateArgumentList

    $certificateStore = New-Object System.Security.Cryptography.X509Certificates.X509Store `
        -ArgumentList "\\$($env:COMPUTERNAME)\$store", $location

    # add the certificate to the store
    $certificateStore.Open("ReadWrite")
    $certificateStore.Add($x509Certificate)
    $certificateStore.Close()
}

$certificateFile = Join-Path $PSScriptRoot "idsrv3test.cer"
$privateKeyFile = Join-Path $PSScriptRoot "idsrv3test.pfx"

Add-CertificateToStore `
    -store "My" `
    -location "LocalMachine" `
    -certificateFile $privateKeyFile `
    -password "idsrv3test"

Add-CertificateToStore `
    -store "TrustedPeople" `
    -location "LocalMachine" `
    -certificateFile $certificateFile 