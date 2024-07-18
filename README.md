
# Cogito Group - Open Jellyfish Tool

The Open Jellyfish Tool provides a simple command line interface for PKCS10 and CMC certificate signing request (CSR) and certificate revocation request submission.
Use of this tool requires access to a Jellyfish deployment and an existing API key with certificate management permissions.

The Open Jellyfish Tool may generated Certificate Management over Cryptographic Message Syntax (CMC) formatted requests signed by a provided digital signature certificate, and provides examples of how to consume the returned CMC responses.

This project aims to provide a limited example for API integration developers interacting with the Jellyfish system through the API.
The project uses the [Bouncy Castle C#](https://github.com/bcgit/bc-csharp) for many of the cryptographic operations, and the [Konscious Security Cryptography](https://github.com/kmaragon/Konscious.Security.Cryptography) Argon2 implementation.

The project is written in C# [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) and is intended for use on both Windows and Linux operating systems.


## Dependencies

- Languages
    - C# [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
        - [Microsoft Extensions Configuration](https://www.nuget.org/packages/Microsoft.Extensions.Configuration/9.0.0-preview.5.24306.7)
        - [Microsoft Extensions Configuration Binder](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Binder/9.0.0-preview.5.24306.7)
        - [Microsoft Extensions Configuration JSON](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Json/9.0.0-preview.5.24306.7)
        - [Microsoft Extensions Dependency Inject](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/9.0.0-preview.5.24306.7)

- Nuget Packages
    - [Bouncy Castle Cryptography (2.3.1)](https://www.nuget.org/packages/BouncyCastle.Cryptography)
    - [Konscious Security Cryptography Argon2 (1.3.1)](https://www.nuget.org/packages/Konscious.Security.Cryptography.Argon2)


## Installation

Where this project is provided as a pre-compiled binary, the software may be run as a stand-alone executable with no installation required.


## First-Time Setup

When running the Open Jellyfish Tool for the first time, the operator will be prompted to provide the following resources:

- An accessible Jellyfish webserver URL
- A Jellyfish API key belonging to a user with the following permissions:
    - Add User Certificate (addUserCert)
    - Add Device Certificate (addDeviceCert)
    - Certificate Revocations (revokeCertificate)
    - View Certificate Authorities (getCAs)
- A PEM format x509 certificate with the Digital Signature key usage (when using CMC operations)
- A PEM format PKCS8 private key belonging to the Digital Signature x509 certificate (when using CMC operations)

Completion of the first time setup will result in the generation of an **appsettings.json** file, caching the setup options.
If Jellyfish becomes unavailable, a credential expires, or a file can no longer be found: first time setup steps will be required.


## Module Instructions

### 1 - Certificate Request Module

Takes a path to a PEM format PKCS10 Certificate Signing Request (CSR) on the file system, and submits the CSR to Jellyfish using the elected Certificate Authority and Certificate Template. Returns a PEM format x509 Certificate and prompts for a path for it to be saved on the file system.

### 2 - Certificate Revocation Module

Takes a Certificate Serial Number (format as a HEX string, the same format the serial is displayed in the Jellyfish portal), and submits a revocation against the elected Certificate Authority using the elected revocation reason. Confirmation is returned on successful revocation.

### 3 - CMC Certificate Request Module

Takes a path to a PEM format PKCS10 Certificate Signing Request (CSR) on the file system. The PKCS10 is enveloped in a CMC format PKCS7 container in compliance with [RFC 5272 - Certificate Management over CMS (CMC)](https://datatracker.ietf.org/doc/html/rfc5272). The operator may choose to save the PEM format PKCS7 CMC request to a path on the file system. The CMC request is submitted to Jellyfish using the elected Certificate Authority and Certificate Template. Returns a PEM format PKCS7 CMC certificate response, this may be simple or full message depending on the disposition of the issued certificate. The operator may choose to save the PEM format PKCS7 CMC response to a path on the file system. In the case of a successful certificate issuance, the operator is prompted to save the resulting PEM format x509 certificate extracted from the PKCS7 CMC response.

### 4 - CMC Certificate Revocation Module

Takes a Certificate Serial Number (format as a HEX string, the same format the serial is displayed in the Jellyfish portal), and submits a revocation request against the elected Certificate Authority using the elected revocation reason. The request is enveloped in a CMC format PKCS7 container in compliance with [RFC 5272 - Certificate Management over CMS (CMC)](https://datatracker.ietf.org/doc/html/rfc5272). Confirmation is returned on successful revocation, otherwise the CMC message contains the error. The operator may choose to save the PEM format PKCS7 cmc response to a path on the file system. Returns a PEM format PKCS7 CMC revocation response, this is always a full message. The operator may choose to save the PEM format PKCS7 CMC response to a path on the file system.

### 5 - Exit

Closes the application.


## Configuration

Completion of the first-time setup will define all required configuration items.
There are no additional optional configuration items.
If a configured item is required to be changed after completion of first-time setup, the **appsettings.json** file can be edited.

### appsettings.json

#### Sample

```
{
    "OpenJellyfishTool": {
        "Jellyfish": {
            "Address": "https://jellyfishhq.com",
            "Auth": {
                "ApiKey": "Jellyfish API Key"
            },
            "Proxy": {
                "Address": "127.0.0.1"
                "Port": 3128
            }
        },
        "Crypto": {
            "CmcSignerCertificatePath": "/path/to/signer/x509certificate.crt",
            "CmcSignerKeyPath": "/path/to/signer/pkcs8privatekey.key"
        }
    }
}
```

#### Details

| Configuration Item | URI | Type | Description |
| --- | --- | --- | --- |
| Jellyfish Address | OpenJellyfishTool.Jellyfish.Address | String | The URL of your Jellyfish webserver, must include the protocol (https) |
| Jellyfish API Key | OpenJellyfishTool.Jellyfish.Auth.ApiKey | String | A Jellyfish API key generated within the Jellyfish portal |
| Proxy Address | OpenJellyfishTool.Jellyfish.Proxy.Address | String | An **Optional** address of a HTTP proxy when connecting to Jellyfish indirectly |
| Proxy Port | OpenJellyfishTool.Jellyfish.Proxy.Port | Integer | The port used to connect to the HTTP proxy defined in the Proxy Address, required **only** when a proxy is defined |
| CMC Signer Certificate Path | OpenJellyfishTool.Crypto.CmcSignerCertificatePath | String | An **Optional** location of a PEM format x509 certificate on the file system. CMC operations cannot be performed without a singer certificate |
| CMC Signer Key Path | OpenJellyfishTool.Crypto.CmcSingerKeyPath | String | Location of a PEM format PCKS8 private key on the file system belonging to the CMC Signer Certificate. Required **only** when a CMC Signer Certificate is defined |