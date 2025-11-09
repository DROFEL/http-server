using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace http_server.helpers;

public class CertificateHelper
{
    private static X509Certificate2 CreateCa(string ca = "CN=Dev Root CA")
    {
        using var rootKey = RSA.Create(2048);

        var req = new CertificateRequest(
            new X500DistinguishedName(ca),
            rootKey,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        req.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(true, false, 1, true));

        req.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));

        req.CertificateExtensions.Add(
            new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

        var notBefore = DateTimeOffset.UtcNow.AddDays(-1);
        var notAfter = DateTimeOffset.UtcNow.AddYears(10);

        var root = req.CreateSelfSigned(notBefore, notAfter);
        return root.CopyWithPrivateKey(rootKey);
    }

    private static X509Certificate2 CreateServerCert(
        X509Certificate2 issuerRoot,
        string commonName,
        string[] dnsNames)
    {
        using var leafKey = RSA.Create(2048);
        var req = new CertificateRequest(
            new X500DistinguishedName($"CN={commonName}"),
            leafKey,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        req.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(false, false, 0,true));

        req.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));

        req.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new OidCollection { new Oid("1.3.6.1.5.5.7.3.1")}, false));

        var san = new SubjectAlternativeNameBuilder();
        foreach (var dnsName in dnsNames) san.AddDnsName(dnsName);
        req.CertificateExtensions.Add(san.Build());

        req.CertificateExtensions.Add(
            new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

        var notBefore = DateTimeOffset.UtcNow.AddDays(-1);
        var notAfter  = notBefore.AddYears(2);

        var serial = new byte[16];
        RandomNumberGenerator.Fill(serial);
        var issued = req.Create(issuerRoot, notBefore, notAfter, serial);

        return issued.CopyWithPrivateKey(leafKey);
    }
}