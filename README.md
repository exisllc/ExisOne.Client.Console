# ExisOne.Client SDK Demo

[![NuGet](https://img.shields.io/nuget/v/ExisOne.Client.svg)](https://www.nuget.org/packages/ExisOne.Client)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

A comprehensive demo console application showcasing all features of the **ExisOne.Client** NuGet package for software licensing and activation.

## 🌟 What is ExisOne?

[ExisOne](https://www.exisone.com) is a cloud-based software licensing platform that helps developers:

- 🔐 **Protect** software with hardware-locked licenses
- 💰 **Monetize** applications with license keys
- 📊 **Track** activations with real-time analytics
- 🌐 **Support** offline customers with RSA-signed activation codes
- 💳 **Integrate** with Stripe & PayPal for automatic license delivery

## 🚀 Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- An [ExisOne account](https://www.exisone.com/register.html) (free tier available)

### Installation

```bash
# Clone this repository
git clone https://github.com/ExisLLC/ExisOne.Client.Console.git

# Navigate to the project
cd ExisOne.Client.Console

# Run the demo
dotnet run --project ExisOne.Client.Console
```

### Configuration

Before running, update the configuration in `Program.cs`:

```csharp
// Your ExisOne API base URL
private const string BaseUrl = "https://www.exisone.com";

// Your access token from the ExisOne dashboard
private const string AccessToken = "exo_at_YOUR_PUBLIC_KEY_YOUR_SECRET_KEY";

// Your product name
private const string ProductName = "MyProduct";
```

## 📋 Demo Features

This demo showcases all ExisOne.Client SDK capabilities:

| Feature | Description |
|---------|-------------|
| **Hardware ID Generation** | Generate unique device fingerprints |
| **License Activation** | Activate licenses with version checking |
| **Simple Validation** | Quick boolean license check |
| **Rich Validation** | Get status, expiration, features, server version |
| **Smart Validation** | Auto-detect online/offline with fallback |
| **Offline Validation** | Validate RSA-signed codes without network |
| **Deactivation** | Release licenses from hardware |
| **Smart Deactivation** | Opportunistic server sync |
| **Support Tickets** | Submit tickets from within your app |
| **Key Generation** | Programmatically create license keys |

## 💻 Using the SDK in Your Application

### 1. Install the NuGet Package

```bash
dotnet add package ExisOne.Client --version 0.6.0
```

### 2. Initialize the Client

```csharp
using ExisOne.Client;

var client = new ExisOneClient(new ExisOneClientOptions
{
    BaseUrl = "https://www.exisone.com",
    AccessToken = "exo_at_<public>_<secret>",
    OfflinePublicKey = null // Set for offline license support
});
```

### 3. Generate Hardware ID

```csharp
// Generate once and persist locally
var hardwareId = client.GenerateHardwareId();
```

### 4. Activate a License

```csharp
// Activate with version check (v0.5.0+)
var result = await client.ActivateAsync(
    activationKey, 
    email, 
    hardwareId, 
    "MyProduct", 
    version: "1.0.0"
);

if (!result.Success)
{
    if (result.ErrorCode == "version_outdated")
        Console.WriteLine($"Please upgrade to {result.MinimumRequiredVersion}");
    else
        Console.WriteLine(result.ErrorMessage);
}
```

### 5. Validate a License

```csharp
// Rich validation with version info (v0.5.0+)
var (isValid, status, expiration, features, serverVersion, minVersion) = 
    await client.ValidateAsync(hardwareId, "MyProduct", activationKey, version: "1.0.0");

if (status == "version_outdated")
{
    Console.WriteLine($"Upgrade required: minimum version is {minVersion}");
}
else if (isValid)
{
    Console.WriteLine($"Licensed until: {expiration}");
    
    // Check for updates
    if (serverVersion != "1.0.0")
        Console.WriteLine($"Update available: v{serverVersion}");
}
```

### 6. Smart Validation (Online/Offline)

```csharp
// Auto-detects online vs offline keys
var result = await client.ValidateSmartAsync(keyOrOfflineCode, hardwareId, "MyProduct");

Console.WriteLine($"Valid: {result.IsValid}");
Console.WriteLine($"Mode: {(result.WasOffline ? "Offline" : "Online")}");
Console.WriteLine($"Server Version: {result.ServerVersion}");
```

### 7. Deactivate a License

```csharp
// Standard deactivation
await client.DeactivateAsync(activationKey, hardwareId, "MyProduct");

// Smart deactivation (works offline)
var result = await client.DeactivateSmartAsync(keyOrCode, hardwareId, "MyProduct");
Console.WriteLine($"Server notified: {result.ServerNotified}");
```

## 🔒 Offline Licensing

For air-gapped environments, ExisOne supports RSA-signed offline activation codes:

### Setup

```csharp
var client = new ExisOneClient(new ExisOneClientOptions
{
    BaseUrl = "https://www.exisone.com",
    OfflinePublicKey = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A...
-----END PUBLIC KEY-----"
});
```

### Workflow

1. Customer runs your app and sees their **Hardware ID**
2. Customer contacts you with their Hardware ID
3. You generate an **Offline Activation Code** in the ExisOne dashboard
4. Customer enters the code into your app
5. App validates locally using `ValidateOffline()` or `ValidateSmartAsync()`

```csharp
// Direct offline validation (no network)
var result = client.ValidateOffline(offlineCode, hardwareId);

if (result.IsValid)
    Console.WriteLine($"Valid until: {result.ExpirationDate}");
else if (result.IsExpired)
    Console.WriteLine("License expired");
else if (result.HardwareMismatch)
    Console.WriteLine("Wrong machine");
```

## 📊 Version Enforcement (v0.5.0+)

Force users to upgrade their software before using new features:

1. Set **Minimum Required Version** in ExisOne dashboard → Product Management
2. Enable **Enforce Version Check**
3. Pass version during activation/validation
4. Handle `version_outdated` status

```csharp
var (isValid, status, _, _, serverVer, minVer) = 
    await client.ValidateAsync(hwid, "MyProduct", key, version: "1.0.0");

if (status == "version_outdated")
{
    // Block usage until upgrade
    MessageBox.Show($"Please upgrade to version {minVer}");
    return;
}

// Optionally notify about updates
if (serverVer != null && serverVer != "1.0.0")
{
    ShowNotification($"Update available: v{serverVer}");
}
```

## ⏱️ Trial / Initial Mode Expiration (v0.6.0+)

Starting in v0.6.0, the API **always returns an expiration date** during validation, even for trial users or invalid licenses. This enables consistent UI messaging about when trials expire.

### How Expiration is Calculated

| Scenario | Expiration Date |
|----------|-----------------|
| **First visit** (new hardware ID + valid product) | `NOW + TrialDays` |
| **Returning visit** (existing device record) | Stored expiration from device record |
| **Invalid/blank license key** | Device's creation date + TrialDays |
| **Activated license** | License expiration date |
| **Product not found** | Current date |

### Example: Trial Mode

```csharp
// First time user with no activation key (trial mode)
var (isValid, status, expirationDate, features, _, _) = 
    await client.ValidateAsync(hardwareId, "MyProduct", activationKey: null);

// status = "trial" (if within trial period) or "expired" (if trial ended)
// expirationDate = CreatedAt + TrialDays (always set, never null)

if (status == "trial")
{
    var daysLeft = (expirationDate.Value - DateTime.UtcNow).Days;
    Console.WriteLine($"Trial mode: {daysLeft} days remaining");
}
else if (status == "expired")
{
    Console.WriteLine($"Trial expired on {expirationDate:d}. Please purchase a license.");
}
```

### Key Points

- **First contact creates a record**: When a hardware ID first contacts the API with a valid product, a device record is created with `CreatedAt = NOW` and `ExpirationDate = NOW + TrialDays`
- **Consistent dates**: The same hardware ID will always get the same expiration date (based on first contact), preventing trial resets
- **Override capability**: Administrators can manually adjust expiration dates via the License Activity page
- **Deactivation clears records**: When a license is deactivated, the device record's activation key and expiration are cleared, allowing trial mode to resume if applicable

## 📚 Documentation

- [SDK Documentation](https://www.exisone.com/docs-sdk.html)
- [API Reference](https://www.exisone.com/docs.html)
- [AI Integration Prompt](https://www.exisone.com/docs-sdk-ai.html)
- [Stripe Integration](https://www.exisone.com/stripe-docs.html)
- [PayPal Integration](https://www.exisone.com/paypal-docs.html)

## 🔑 Access Token Permissions

| Method | Permission Required |
|--------|---------------------|
| `ValidateAsync` | `verify` |
| `GenerateActivationKeyAsync` | `generate` |
| `SendSupportTicketAsync` | `email` |
| `ActivateAsync` | None |
| `DeactivateAsync` | None |
| `GenerateHardwareId` | None |

## 📝 Changelog

### v0.6.0 (Current)
- **Consistent Expiration Dates**: `expirationDate` is now always returned during validation, even for invalid/trial licenses
- **Trial Expiration Calculation**: For first-time visits with a valid product, expiration is calculated as `CreatedAt + TrialDays`
- **Device Tracking**: Device records are now created on first contact for analytics and trial management
- **Deactivation Fix**: Device records are properly cleared on license deactivation

### v0.5.0
- **Version Enforcement**: Pass client version during activation/validation
- **ActivationResult**: Structured return type with error codes
- **Server Version Info**: All responses include `serverVersion` and `minimumRequiredVersion`
- **version_outdated Status**: New status when client is below minimum version

### v0.4.0
- Offline license validation with RSA-signed codes
- Smart validation (auto-detect online/offline)
- Opportunistic deactivation sync

### v0.3.0
- License deactivation
- IP and country tracking

## 🏢 About Exis, LLC

ExisOne is developed by [Exis, LLC](https://www.exisone.com), providing software licensing solutions for developers worldwide.

- **Website**: https://www.exisone.com
- **Email**: exisllc@gmail.com
- **Phone**: +1 (423) 714-7047

## 📄 License

This demo project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

<p align="center">
  <a href="https://www.exisone.com">
    <img src="https://www.exisone.com/images/logo.svg" alt="ExisOne" width="200">
  </a>
  <br>
  <em>Software Licensing Made Simple</em>
</p>

