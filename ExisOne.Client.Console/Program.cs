// =============================================================================
// ExisOne.Client SDK Demo Console Application
// =============================================================================
// This console app demonstrates all features of the ExisOne.Client NuGet package
// for software licensing and activation.
//
// For more information, visit: https://www.exisone.com
// SDK Documentation: https://www.exisone.com/docs-sdk.html
// =============================================================================

using ExisOne.Client;

namespace ExisOne.Client.Console;

class Program
{
    // =========================================================================
    // CONFIGURATION - Replace with your actual values
    // =========================================================================
    
    // Your ExisOne API base URL (must be HTTPS)
    private const string BaseUrl = "https://www.exisone.com";
    
    // Your access token from the ExisOne dashboard (Access Tokens page)
    // Format: exo_at_<public>_<secret>
    private const string AccessToken = "exo_at_YOUR_PUBLIC_KEY_YOUR_SECRET_KEY";
    
    // Your product name as configured in ExisOne
    private const string ProductName = "MyProduct";
    
    // Your application version (used for version enforcement)
    private const string AppVersion = "1.0.0";
    
    // Optional: RSA public key for offline license validation (PEM format)
    // Get this from your Crypto Keys page in the ExisOne dashboard
    // Set to your PEM key string for offline support, e.g.:
    // private static readonly string? OfflinePublicKey = @"-----BEGIN PUBLIC KEY-----
    // MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A...
    // -----END PUBLIC KEY-----";
    private static readonly string? OfflinePublicKey = null;
    
    // =========================================================================
    // MAIN ENTRY POINT
    // =========================================================================
    
    static async Task Main(string[] args)
    {
        PrintHeader();
        
        // Initialize the ExisOne client
        var client = InitializeClient();
        
        // Run interactive demo menu
        await RunInteractiveDemo(client);
    }
    
    // =========================================================================
    // CLIENT INITIALIZATION
    // =========================================================================
    
    /// <summary>
    /// Initialize the ExisOne client with configuration options.
    /// In a real application, you would typically register this as a singleton service.
    /// </summary>
    static ExisOneClient InitializeClient()
    {
        System.Console.WriteLine("Initializing ExisOne Client...\n");
        
        var options = new ExisOneClientOptions
        {
            BaseUrl = BaseUrl,
            AccessToken = AccessToken,
            OfflinePublicKey = OfflinePublicKey // Set for offline license validation
        };
        
        var client = new ExisOneClient(options);
        
        // Display SDK version
        System.Console.WriteLine($"  SDK Version: {client.GetVersion()}");
        System.Console.WriteLine($"  Base URL: {BaseUrl}");
        System.Console.WriteLine($"  Offline Support: {(OfflinePublicKey != null ? "Enabled" : "Disabled")}\n");
        
        return client;
    }
    
    // =========================================================================
    // INTERACTIVE DEMO MENU
    // =========================================================================
    
    static async Task RunInteractiveDemo(ExisOneClient client)
    {
        while (true)
        {
            PrintMenu();
            var choice = System.Console.ReadLine()?.Trim();
            
            System.Console.WriteLine();
            
            try
            {
                switch (choice)
                {
                    case "1":
                        DemoHardwareId(client);
                        break;
                    case "2":
                        await DemoActivation(client);
                        break;
                    case "3":
                        await DemoSimpleValidation(client);
                        break;
                    case "4":
                        await DemoRichValidation(client);
                        break;
                    case "5":
                        await DemoSmartValidation(client);
                        break;
                    case "6":
                        DemoOfflineValidation(client);
                        break;
                    case "7":
                        await DemoDeactivation(client);
                        break;
                    case "8":
                        await DemoSmartDeactivation(client);
                        break;
                    case "9":
                        await DemoSupportTicket(client);
                        break;
                    case "10":
                        await DemoKeyGeneration(client);
                        break;
                    case "0":
                        System.Console.WriteLine("Goodbye!");
                        return;
                    default:
                        System.Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
            catch (HttpRequestException ex)
            {
                PrintError($"HTTP Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                PrintError($"Error: {ex.Message}");
            }
            
            System.Console.WriteLine("\nPress any key to continue...");
            System.Console.ReadKey(true);
        }
    }
    
    // =========================================================================
    // DEMO 1: HARDWARE ID GENERATION
    // =========================================================================
    
    /// <summary>
    /// Generate a unique hardware fingerprint for this machine.
    /// This ID should be persisted locally and reused for all license operations.
    /// </summary>
    static void DemoHardwareId(ExisOneClient client)
    {
        PrintSection("Hardware ID Generation");
        
        System.Console.WriteLine("Generating hardware fingerprint...\n");
        
        var hardwareId = client.GenerateHardwareId();
        
        System.Console.WriteLine($"  Hardware ID: {hardwareId}");
        System.Console.WriteLine();
        System.Console.WriteLine("  This ID is a salted SHA-256 hash of:");
        System.Console.WriteLine("    - CPU information");
        System.Console.WriteLine("    - Motherboard/BIOS serial numbers");
        System.Console.WriteLine("    - Network adapter MAC addresses");
        System.Console.WriteLine("    - OS installation identifiers");
        System.Console.WriteLine();
        System.Console.WriteLine("  TIP: Persist this ID locally (e.g., in app settings) and");
        System.Console.WriteLine("       display it to users so they can request offline licenses.");
        
        PrintSuccess("Hardware ID generated successfully!");
    }
    
    // =========================================================================
    // DEMO 2: LICENSE ACTIVATION (with version)
    // =========================================================================
    
    /// <summary>
    /// Activate a license key with hardware binding and version check.
    /// New in v0.5.0: Returns ActivationResult with server version info.
    /// </summary>
    static async Task DemoActivation(ExisOneClient client)
    {
        PrintSection("License Activation (with Version Check)");
        
        var hardwareId = client.GenerateHardwareId();
        
        System.Console.Write("Enter activation key (e.g., XXXX-XXXX-XXXX-XXXX): ");
        var activationKey = System.Console.ReadLine()?.Trim() ?? "";
        
        System.Console.Write("Enter email address: ");
        var email = System.Console.ReadLine()?.Trim() ?? "";
        
        if (string.IsNullOrEmpty(activationKey) || string.IsNullOrEmpty(email))
        {
            PrintError("Activation key and email are required.");
            return;
        }
        
        System.Console.WriteLine($"\nActivating license...");
        System.Console.WriteLine($"  Key: {activationKey}");
        System.Console.WriteLine($"  Email: {email}");
        System.Console.WriteLine($"  Hardware ID: {hardwareId}");
        System.Console.WriteLine($"  Product: {ProductName}");
        System.Console.WriteLine($"  Client Version: {AppVersion}");
        
        // NEW in v0.5.0: ActivateAsync returns ActivationResult
        var result = await client.ActivateAsync(
            activationKey, 
            email, 
            hardwareId, 
            ProductName, 
            version: AppVersion);
        
        System.Console.WriteLine();
        
        if (result.Success)
        {
            PrintSuccess("License activated successfully!");
            System.Console.WriteLine($"  Server Version: {result.ServerVersion ?? "N/A"}");
            System.Console.WriteLine($"  Minimum Required: {result.MinimumRequiredVersion ?? "None"}");
            
            // Check if update is available
            if (result.ServerVersion != null && result.ServerVersion != AppVersion)
            {
                PrintInfo($"Update available: v{result.ServerVersion}");
            }
        }
        else
        {
            // Handle specific error codes
            if (result.ErrorCode == "version_outdated")
            {
                PrintError($"Version outdated! Your version: {AppVersion}");
                System.Console.WriteLine($"  Minimum required: {result.MinimumRequiredVersion}");
                System.Console.WriteLine($"  Latest version: {result.ServerVersion}");
                System.Console.WriteLine("\n  Please upgrade your software to activate.");
            }
            else
            {
                PrintError($"Activation failed: {result.ErrorMessage}");
            }
        }
    }
    
    // =========================================================================
    // DEMO 3: SIMPLE LICENSE VALIDATION
    // =========================================================================
    
    /// <summary>
    /// Simple boolean validation - just checks if license is valid.
    /// </summary>
    static async Task DemoSimpleValidation(ExisOneClient client)
    {
        PrintSection("Simple License Validation");
        
        var hardwareId = client.GenerateHardwareId();
        
        System.Console.Write("Enter activation key: ");
        var activationKey = System.Console.ReadLine()?.Trim() ?? "";
        
        if (string.IsNullOrEmpty(activationKey))
        {
            PrintError("Activation key is required.");
            return;
        }
        
        System.Console.WriteLine($"\nValidating license...");
        System.Console.WriteLine($"  Key: {activationKey}");
        System.Console.WriteLine($"  Hardware ID: {hardwareId}");
        
        var isValid = await client.ValidateAsync(activationKey, hardwareId);
        
        System.Console.WriteLine();
        
        if (isValid)
        {
            PrintSuccess("License is VALID!");
        }
        else
        {
            PrintError("License is INVALID or not bound to this hardware.");
        }
    }
    
    // =========================================================================
    // DEMO 4: RICH LICENSE VALIDATION (with status, expiration, features, version)
    // =========================================================================
    
    /// <summary>
    /// Rich validation with detailed status, expiration, features, and version info.
    /// Updated in v0.5.0: Now returns serverVersion and minimumRequiredVersion.
    /// </summary>
    static async Task DemoRichValidation(ExisOneClient client)
    {
        PrintSection("Rich License Validation (with Version Check)");
        
        var hardwareId = client.GenerateHardwareId();
        
        System.Console.Write("Enter activation key (or leave blank for trial): ");
        var activationKey = System.Console.ReadLine()?.Trim();
        
        System.Console.WriteLine($"\nValidating...");
        System.Console.WriteLine($"  Hardware ID: {hardwareId}");
        System.Console.WriteLine($"  Product: {ProductName}");
        System.Console.WriteLine($"  Client Version: {AppVersion}");
        
        // NEW in v0.5.0: Tuple now includes serverVersion and minimumRequiredVersion
        var (isValid, status, expirationDate, features, serverVersion, minimumRequiredVersion) = 
            await client.ValidateAsync(
                hardwareId, 
                ProductName, 
                activationKey: string.IsNullOrEmpty(activationKey) ? null : activationKey,
                version: AppVersion);
        
        System.Console.WriteLine();
        System.Console.WriteLine("  ─────────────────────────────────────");
        System.Console.WriteLine($"  Status: {status}");
        System.Console.WriteLine($"  Valid: {isValid}");
        System.Console.WriteLine($"  Expiration: {expirationDate?.ToString("yyyy-MM-dd") ?? "N/A"}");
        System.Console.WriteLine($"  Features: [{string.Join(", ", features)}]");
        System.Console.WriteLine($"  Server Version: {serverVersion ?? "N/A"}");
        System.Console.WriteLine($"  Min Required Version: {minimumRequiredVersion ?? "None"}");
        System.Console.WriteLine("  ─────────────────────────────────────");
        
        // Handle version enforcement
        if (status == "version_outdated")
        {
            PrintError($"Your version ({AppVersion}) is outdated!");
            System.Console.WriteLine($"  Please upgrade to version {minimumRequiredVersion} or later.");
        }
        else if (isValid)
        {
            PrintSuccess($"License is valid ({status})!");
            
            // Notify about available updates
            if (serverVersion != null && serverVersion != AppVersion)
            {
                PrintInfo($"Update available: v{serverVersion}");
            }
        }
        else
        {
            PrintError($"License invalid: {status}");
        }
    }
    
    // =========================================================================
    // DEMO 5: SMART VALIDATION (auto-detects online/offline)
    // =========================================================================
    
    /// <summary>
    /// Smart validation that auto-detects online vs offline keys and falls back gracefully.
    /// Updated in v0.5.0: Result includes ServerVersion and MinimumRequiredVersion.
    /// </summary>
    static async Task DemoSmartValidation(ExisOneClient client)
    {
        PrintSection("Smart Validation (Online/Offline Auto-Detect)");
        
        var hardwareId = client.GenerateHardwareId();
        
        System.Console.Write("Enter activation key or offline code: ");
        var keyOrCode = System.Console.ReadLine()?.Trim() ?? "";
        
        if (string.IsNullOrEmpty(keyOrCode))
        {
            PrintError("Key or code is required.");
            return;
        }
        
        System.Console.WriteLine($"\nSmart validating...");
        System.Console.WriteLine($"  Hardware ID: {hardwareId}");
        System.Console.WriteLine($"  Product: {ProductName}");
        
        var result = await client.ValidateSmartAsync(keyOrCode, hardwareId, ProductName);
        
        System.Console.WriteLine();
        System.Console.WriteLine("  ─────────────────────────────────────");
        System.Console.WriteLine($"  Valid: {result.IsValid}");
        System.Console.WriteLine($"  Status: {result.Status}");
        System.Console.WriteLine($"  Expiration: {result.ExpirationDate?.ToString("yyyy-MM-dd") ?? "N/A"}");
        System.Console.WriteLine($"  Features: [{string.Join(", ", result.Features ?? Array.Empty<int>())}]");
        System.Console.WriteLine($"  Was Offline: {result.WasOffline}");
        System.Console.WriteLine($"  Server Version: {result.ServerVersion ?? "N/A"}");
        System.Console.WriteLine($"  Min Required: {result.MinimumRequiredVersion ?? "None"}");
        System.Console.WriteLine("  ─────────────────────────────────────");
        
        if (result.IsValid)
        {
            PrintSuccess($"License valid! (Mode: {(result.WasOffline ? "Offline" : "Online")})");
        }
        else
        {
            PrintError($"License invalid: {result.ErrorMessage ?? result.Status}");
        }
    }
    
    // =========================================================================
    // DEMO 6: OFFLINE VALIDATION (no network required)
    // =========================================================================
    
    /// <summary>
    /// Validate an offline activation code locally without any server connection.
    /// Requires OfflinePublicKey to be configured.
    /// </summary>
    static void DemoOfflineValidation(ExisOneClient client)
    {
        PrintSection("Offline Validation (No Network)");
        
        if (OfflinePublicKey == null)
        {
            PrintError("Offline validation is not configured.");
            System.Console.WriteLine("\n  To enable offline validation:");
            System.Console.WriteLine("  1. Go to your ExisOne dashboard → Crypto Keys");
            System.Console.WriteLine("  2. Copy your RSA Public Key (PEM format)");
            System.Console.WriteLine("  3. Set the OfflinePublicKey constant in this demo");
            return;
        }
        
        var hardwareId = client.GenerateHardwareId();
        
        System.Console.Write("Enter offline activation code: ");
        var offlineCode = System.Console.ReadLine()?.Trim() ?? "";
        
        if (string.IsNullOrEmpty(offlineCode))
        {
            PrintError("Offline code is required.");
            return;
        }
        
        System.Console.WriteLine($"\nValidating offline...");
        System.Console.WriteLine($"  Hardware ID: {hardwareId}");
        
        var result = client.ValidateOffline(offlineCode, hardwareId);
        
        System.Console.WriteLine();
        System.Console.WriteLine("  ─────────────────────────────────────");
        System.Console.WriteLine($"  Valid: {result.IsValid}");
        System.Console.WriteLine($"  Product: {result.ProductName ?? "N/A"}");
        System.Console.WriteLine($"  Expiration: {result.ExpirationDate?.ToString("yyyy-MM-dd") ?? "N/A"}");
        System.Console.WriteLine($"  Email: {result.Email ?? "N/A"}");
        System.Console.WriteLine($"  Features: [{string.Join(", ", result.Features ?? Array.Empty<int>())}]");
        System.Console.WriteLine($"  Is Expired: {result.IsExpired}");
        System.Console.WriteLine($"  Hardware Mismatch: {result.HardwareMismatch}");
        System.Console.WriteLine("  ─────────────────────────────────────");
        
        if (result.IsValid)
        {
            PrintSuccess("Offline license is valid!");
        }
        else if (result.IsExpired)
        {
            PrintError("Offline license has expired.");
        }
        else if (result.HardwareMismatch)
        {
            PrintError("This license is bound to a different machine.");
        }
        else
        {
            PrintError($"Invalid: {result.ErrorMessage}");
        }
    }
    
    // =========================================================================
    // DEMO 7: LICENSE DEACTIVATION
    // =========================================================================
    
    /// <summary>
    /// Deactivate a license, releasing it from this hardware.
    /// </summary>
    static async Task DemoDeactivation(ExisOneClient client)
    {
        PrintSection("License Deactivation");
        
        var hardwareId = client.GenerateHardwareId();
        
        System.Console.Write("Enter activation key to deactivate: ");
        var activationKey = System.Console.ReadLine()?.Trim() ?? "";
        
        if (string.IsNullOrEmpty(activationKey))
        {
            PrintError("Activation key is required.");
            return;
        }
        
        System.Console.WriteLine($"\nDeactivating license...");
        System.Console.WriteLine($"  Key: {activationKey}");
        System.Console.WriteLine($"  Hardware ID: {hardwareId}");
        System.Console.WriteLine($"  Product: {ProductName}");
        
        var success = await client.DeactivateAsync(activationKey, hardwareId, ProductName);
        
        if (success)
        {
            PrintSuccess("License deactivated successfully!");
            System.Console.WriteLine("  The license can now be activated on another machine.");
        }
        else
        {
            PrintError("Deactivation failed.");
        }
    }
    
    // =========================================================================
    // DEMO 8: SMART DEACTIVATION (with offline fallback)
    // =========================================================================
    
    /// <summary>
    /// Smart deactivation - tries server, succeeds locally even if offline.
    /// </summary>
    static async Task DemoSmartDeactivation(ExisOneClient client)
    {
        PrintSection("Smart Deactivation (Opportunistic Sync)");
        
        var hardwareId = client.GenerateHardwareId();
        
        System.Console.Write("Enter activation key or offline code: ");
        var keyOrCode = System.Console.ReadLine()?.Trim() ?? "";
        
        if (string.IsNullOrEmpty(keyOrCode))
        {
            PrintError("Key or code is required.");
            return;
        }
        
        System.Console.WriteLine($"\nDeactivating...");
        
        var result = await client.DeactivateSmartAsync(keyOrCode, hardwareId, ProductName);
        
        System.Console.WriteLine();
        System.Console.WriteLine($"  Success: {result.Success}");
        System.Console.WriteLine($"  Server Notified: {result.ServerNotified}");
        
        if (result.Success)
        {
            if (result.ServerNotified)
            {
                PrintSuccess("License deactivated and server notified!");
            }
            else
            {
                PrintInfo("License deactivated locally. Server will be notified when online.");
            }
        }
        else
        {
            PrintError($"Deactivation failed: {result.ErrorMessage}");
        }
    }
    
    // =========================================================================
    // DEMO 9: SUPPORT TICKET SUBMISSION
    // =========================================================================
    
    /// <summary>
    /// Submit a support ticket via the ExisOne platform.
    /// Requires 'email' permission on the access token.
    /// </summary>
    static async Task DemoSupportTicket(ExisOneClient client)
    {
        PrintSection("Support Ticket Submission");
        
        System.Console.Write("Enter your email: ");
        var email = System.Console.ReadLine()?.Trim() ?? "";
        
        System.Console.Write("Enter subject: ");
        var subject = System.Console.ReadLine()?.Trim() ?? "";
        
        System.Console.Write("Enter message: ");
        var message = System.Console.ReadLine()?.Trim() ?? "";
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(message))
        {
            PrintError("All fields are required.");
            return;
        }
        
        System.Console.WriteLine("\nSubmitting support ticket...");
        
        await client.SendSupportTicketAsync(ProductName, email, subject, message);
        
        PrintSuccess("Support ticket submitted successfully!");
        System.Console.WriteLine("  You should receive a response at your email address.");
    }
    
    // =========================================================================
    // DEMO 10: KEY GENERATION (Publisher Feature)
    // =========================================================================
    
    /// <summary>
    /// Generate a new activation key programmatically.
    /// Requires 'generate' permission on the access token.
    /// This is typically used by publishers/admins, not end users.
    /// </summary>
    static async Task DemoKeyGeneration(ExisOneClient client)
    {
        PrintSection("Activation Key Generation (Publisher)");
        
        System.Console.WriteLine("  NOTE: This requires 'generate' permission on your access token.\n");
        
        System.Console.Write("Enter customer email: ");
        var email = System.Console.ReadLine()?.Trim() ?? "";
        
        if (string.IsNullOrEmpty(email))
        {
            PrintError("Email is required.");
            return;
        }
        
        System.Console.WriteLine("\nGenerating activation key...");
        System.Console.WriteLine($"  Product: {ProductName}");
        System.Console.WriteLine($"  Email: {email}");
        
        var responseJson = await client.GenerateActivationKeyAsync(
            ProductName, 
            email, 
            planId: null,      // Use default plan
            validityDays: 365  // 1 year license
        );
        
        PrintSuccess("Activation key generated!");
        System.Console.WriteLine($"\n  Response: {responseJson}");
    }
    
    // =========================================================================
    // UI HELPER METHODS
    // =========================================================================
    
    static void PrintHeader()
    {
        System.Console.Clear();
        System.Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════════════════════╗
║                                                                               ║
║   ███████╗██╗  ██╗██╗███████╗ ██████╗ ███╗   ██╗███████╗                      ║
║   ██╔════╝╚██╗██╔╝██║██╔════╝██╔═══██╗████╗  ██║██╔════╝                      ║
║   █████╗   ╚███╔╝ ██║███████╗██║   ██║██╔██╗ ██║█████╗                        ║
║   ██╔══╝   ██╔██╗ ██║╚════██║██║   ██║██║╚██╗██║██╔══╝                        ║
║   ███████╗██╔╝ ██╗██║███████║╚██████╔╝██║ ╚████║███████╗                      ║
║   ╚══════╝╚═╝  ╚═╝╚═╝╚══════╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝                      ║
║                                                                               ║
║   ExisOne.Client SDK Demo - Software Licensing Made Simple                    ║
║   https://www.exisone.com                                                     ║
║                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════╝
");
    }
    
    static void PrintMenu()
    {
        System.Console.WriteLine(@"
┌───────────────────────────────────────────────────────────────────────────────┐
│  DEMO MENU                                                                    │
├───────────────────────────────────────────────────────────────────────────────┤
│                                                                               │
│   [1]  Generate Hardware ID            [6]  Offline Validation (no network)   │
│   [2]  Activate License (+ version)    [7]  Deactivate License                │
│   [3]  Simple Validation               [8]  Smart Deactivation                │
│   [4]  Rich Validation (+ version)     [9]  Submit Support Ticket             │
│   [5]  Smart Validation                [10] Generate Key (publisher)          │
│                                                                               │
│   [0]  Exit                                                                   │
│                                                                               │
└───────────────────────────────────────────────────────────────────────────────┘
");
        System.Console.Write("  Select option: ");
    }
    
    static void PrintSection(string title)
    {
        System.Console.WriteLine($"\n═══ {title} ═══\n");
    }
    
    static void PrintSuccess(string message)
    {
        var originalColor = System.Console.ForegroundColor;
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine($"\n  ✓ {message}");
        System.Console.ForegroundColor = originalColor;
    }
    
    static void PrintError(string message)
    {
        var originalColor = System.Console.ForegroundColor;
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine($"\n  ✗ {message}");
        System.Console.ForegroundColor = originalColor;
    }
    
    static void PrintInfo(string message)
    {
        var originalColor = System.Console.ForegroundColor;
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine($"\n  ℹ {message}");
        System.Console.ForegroundColor = originalColor;
    }
}
