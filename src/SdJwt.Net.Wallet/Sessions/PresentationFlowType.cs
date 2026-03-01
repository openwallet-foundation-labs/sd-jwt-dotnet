namespace SdJwt.Net.Wallet.Sessions;

/// <summary>
/// Presentation interaction model.
/// </summary>
public enum PresentationFlowType
{
    /// <summary>
    /// Remote verifier flow over network transport.
    /// </summary>
    Remote,

    /// <summary>
    /// Proximity flow (for example BLE/NFC/QR handover).
    /// </summary>
    Proximity
}
