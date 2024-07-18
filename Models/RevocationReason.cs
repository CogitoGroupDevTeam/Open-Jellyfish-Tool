
namespace JellyfishTool.Models {

    public enum RevocationReason {
        Unspecified = 0,
        KeyCompromise = 1,
        CaCompromise = 2,
        AffiliationChanged = 3,
        Superseded = 4,
        CessationOfOperation = 5,
        CertificateHold = 6,
        RemoveFromCRL = 8,
        PrivilegeWithdrawn = 9,
        AaCompromise = 10
    }
}