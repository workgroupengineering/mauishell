using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Shiny.Maui.PermissionGenerator;

public class GeneratePermissionsTask : Microsoft.Build.Utilities.Task
{
    [Required] public ITaskItem[] AppPermissions { get; set; } = [];

    [Required] public string AndroidManifestPath { get; set; } = string.Empty;

    [Required] public string IOSInfoPlistPath { get; set; } = string.Empty;

    public override bool Execute()
    {
        try
        {
            Log.LogMessage(MessageImportance.Normal, $"AndroidManifestPath: '{AndroidManifestPath}'");
            Log.LogMessage(MessageImportance.Normal, $"IOSInfoPlistPath: '{IOSInfoPlistPath}'");
            
            if (string.IsNullOrEmpty(AndroidManifestPath))
            {
                Log.LogError("AndroidManifestPath is required");
                return false;
            }
            
            if (string.IsNullOrEmpty(IOSInfoPlistPath))
            {
                Log.LogError("IOSInfoPlistPath is required");
                return false;
            }

            var permissions = AppPermissions
                .Select(item => new AppPermissionInfo(
                    item.ItemSpec,
                    item.GetMetadata("Reason") ?? "Application requires this permission"))
                .ToArray();

            if (permissions.Length == 0)
            {
                Log.LogMessage(MessageImportance.Low, "No AppPermission items found");
                return true;
            }

            Log.LogMessage(MessageImportance.Normal, $"Generating permissions for {permissions.Length} permission(s)");

            // Generate Android manifest
            GenerateAndroidManifest(permissions, "./AndroidManifest.xml");
            
            // Generate iOS Info.plist
            GenerateIOSInfoPlist(permissions, "Info.plist");

            return true;
        }
        catch (System.Exception ex)
        {
            Log.LogError($"Error generating permissions: {ex.Message}");
            return false;
        }
    }

    private void GenerateAndroidManifest(AppPermissionInfo[] permissions, string outputPath)
    {
        var manifest = new StringBuilder();
        manifest.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        manifest.AppendLine("<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\">");

        foreach (var permission in permissions)
        {
            switch (permission.Type.ToLowerInvariant())
            {
                case "gps":
                case "location":
                    manifest.AppendLine("    <uses-feature android:name=\"android.permission.LOCATION.GPS\" android:required=\"false\" />");
                    manifest.AppendLine("    <uses-feature android:name=\"android.permission.LOCATION.NETWORK\" android:required=\"false\" />");
                    manifest.AppendLine("    <uses-permission android:name=\"android.permission.ACCESS_BACKGROUND_LOCATION\" />");
                    manifest.AppendLine("    <uses-permission android:name=\"android.permission.ACCESS_COARSE_LOCATION\" />");
                    manifest.AppendLine("    <uses-permission android:name=\"android.permission.ACCESS_FINE_LOCATION\" />");
                    manifest.AppendLine("    <uses-permission android:name=\"android.permission.POST_NOTIFICATIONS\" />");
                    manifest.AppendLine("    <uses-permission android:name=\"android.permission.FOREGROUND_SERVICE\" />");
                    manifest.AppendLine("    <uses-permission android:name=\"android.permission.FOREGROUND_SERVICE_LOCATION\" />");
                    break;

                case "camera":
                    manifest.AppendLine("    <uses-feature android:name=\"android.hardware.camera\" android:required=\"false\" />");
                    manifest.AppendLine("    <uses-permission android:name=\"android.permission.CAMERA\" />");
                    break;

                case "microphone":
                    manifest.AppendLine("    <uses-feature android:name=\"android.hardware.microphone\" android:required=\"false\" />");
                    manifest.AppendLine("    <uses-permission android:name=\"android.permission.RECORD_AUDIO\" />");
                    break;

                case "storage":
                    manifest.AppendLine("    <uses-permission android:name=\"android.permission.READ_EXTERNAL_STORAGE\" />");
                    manifest.AppendLine("    <uses-permission android:name=\"android.permission.WRITE_EXTERNAL_STORAGE\" />");
                    break;

                case "bluetooth":
                    manifest.AppendLine("    <uses-feature android:name=\"android.hardware.bluetooth\" android:required=\"false\" />");
                    manifest.AppendLine("    <uses-permission android:name=\"android.permission.BLUETOOTH\" />");
                    manifest.AppendLine("    <uses-permission android:name=\"android.permission.BLUETOOTH_ADMIN\" />");
                    manifest.AppendLine("    <uses-permission android:name=\"android.permission.BLUETOOTH_CONNECT\" />");
                    manifest.AppendLine("    <uses-permission android:name=\"android.permission.BLUETOOTH_SCAN\" />");
                    break;

                case "contacts":
                    manifest.AppendLine("    <uses-permission android:name=\"android.permission.READ_CONTACTS\" />");
                    manifest.AppendLine("    <uses-permission android:name=\"android.permission.WRITE_CONTACTS\" />");
                    break;

                case "notifications":
                    manifest.AppendLine("    <uses-permission android:name=\"android.permission.POST_NOTIFICATIONS\" />");
                    break;

                case "phone":
                    manifest.AppendLine("    <uses-permission android:name=\"android.permission.CALL_PHONE\" />");
                    manifest.AppendLine("    <uses-permission android:name=\"android.permission.READ_PHONE_STATE\" />");
                    break;
            }
        }

        manifest.AppendLine("</manifest>");

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(outputPath, manifest.ToString());
        
        Log.LogMessage(MessageImportance.Normal, $"Generated Android manifest: {outputPath}");
    }

    private void GenerateIOSInfoPlist(AppPermissionInfo[] permissions, string outputPath)
    {
        var plist = new StringBuilder();
        plist.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        plist.AppendLine("<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">");
        plist.AppendLine("<plist version=\"1.0\">");
        plist.AppendLine("<dict>");

        var backgroundModes = new List<string>();

        foreach (var permission in permissions)
        {
            switch (permission.Type.ToLowerInvariant())
            {

                // case "locationalways":
                //     backgroundModes.Add("location");
                //     goto "locationwheninuse";
                    
                case "locationwheninuse":
                    plist.AppendLine("    <key>NSLocationAlwaysUsageDescription</key>");
                    plist.AppendLine($"    <string>{permission.Reason}</string>");
                    plist.AppendLine("    <key>NSLocationAlwaysAndWhenInUseUsageDescription</key>");
                    plist.AppendLine($"    <string>{permission.Reason}</string>");
                    plist.AppendLine("    <key>NSLocationWhenInUseUsageDescription</key>");
                    plist.AppendLine($"    <string>{permission.Reason}</string>");
                    
                    break;


                    
                case "camera":
                    plist.AppendLine("    <key>NSCameraUsageDescription</key>");
                    plist.AppendLine($"    <string>{permission.Reason}</string>");
                    break;

                case "microphone":
                    plist.AppendLine("    <key>NSMicrophoneUsageDescription</key>");
                    plist.AppendLine($"    <string>{permission.Reason}</string>");
                    break;

                case "photolibrary":
                case "photos":
                    plist.AppendLine("    <key>NSPhotoLibraryUsageDescription</key>");
                    plist.AppendLine($"    <string>{permission.Reason}</string>");
                    plist.AppendLine("    <key>NSPhotoLibraryAddUsageDescription</key>");
                    plist.AppendLine($"    <string>{permission.Reason}</string>");
                    break;

                case "bluetooth":
                    plist.AppendLine("    <key>NSBluetoothAlwaysUsageDescription</key>");
                    plist.AppendLine($"    <string>{permission.Reason}</string>");
                    plist.AppendLine("    <key>NSBluetoothPeripheralUsageDescription</key>");
                    plist.AppendLine($"    <string>{permission.Reason}</string>");
                    break;

                case "contacts":
                    plist.AppendLine("    <key>NSContactsUsageDescription</key>");
                    plist.AppendLine($"    <string>{permission.Reason}</string>");
                    break;

                case "calendar":
                    plist.AppendLine("    <key>NSCalendarsUsageDescription</key>");
                    plist.AppendLine($"    <string>{permission.Reason}</string>");
                    break;

                case "reminders":
                    plist.AppendLine("    <key>NSRemindersUsageDescription</key>");
                    plist.AppendLine($"    <string>{permission.Reason}</string>");
                    break;

                case "speech":
                    plist.AppendLine("    <key>NSSpeechRecognitionUsageDescription</key>");
                    plist.AppendLine($"    <string>{permission.Reason}</string>");
                    break;

                case "health":
                    plist.AppendLine("    <key>NSHealthShareUsageDescription</key>");
                    plist.AppendLine($"    <string>{permission.Reason}</string>");
                    plist.AppendLine("    <key>NSHealthUpdateUsageDescription</key>");
                    plist.AppendLine($"    <string>{permission.Reason}</string>");
                    break;

                case "motion":
                    plist.AppendLine("    <key>NSMotionUsageDescription</key>");
                    plist.AppendLine($"    <string>{permission.Reason}</string>");
                    break;

                case "notifications":
                    backgroundModes.Add("remote-notification");
                    break;

                case "background-processing":
                    backgroundModes.Add("background-processing");
                    break;

                case "background-app-refresh":
                    backgroundModes.Add("background-app-refresh");
                    break;
            }
        }

        if (backgroundModes.Count > 0)
        {
            plist.AppendLine("    <key>UIBackgroundModes</key>");
            plist.AppendLine("    <array>");
            foreach (var mode in backgroundModes.Distinct())
            {
                plist.AppendLine($"        <string>{mode}</string>");
            }
            plist.AppendLine("    </array>");
        }

        plist.AppendLine("</dict>");
        plist.AppendLine("</plist>");

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(outputPath, plist.ToString());
        
        Log.LogMessage(MessageImportance.Normal, $"Generated iOS Info.plist: {outputPath}");
    }
}

public record AppPermissionInfo(string Type, string Reason);
