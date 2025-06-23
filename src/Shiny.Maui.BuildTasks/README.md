# Shiny.Maui.PermissionGenerator

This MSBuild task automatically creates platform-specific permission files based on `AppPermission` MSBuild items defined in your project.

## Usage

Add `AppPermission` items to your MAUI project file:

```xml
<ItemGroup>
    <AppPermission Include="Gps" Reason="This application uses GPS to provide location-based services" />
    <AppPermission Include="Camera" Reason="This application needs camera access to capture photos" />
    <AppPermission Include="Notifications" Reason="This application sends important notifications" />
</ItemGroup>
```

## Generated Files

The MSBuild task creates two files in your project's intermediate output folder:

### AndroidManifest.xml
Contains the appropriate `<uses-permission>` and `<uses-feature>` entries for Android.
Location: `$(IntermediateOutputPath)GeneratedPermissions/AndroidManifest.xml`

### Info.plist
Contains the appropriate usage description keys and background modes for iOS.
Location: `$(IntermediateOutputPath)GeneratedPermissions/Info.plist`

## Integration Instructions

After generation, you need to manually integrate these permissions into your platform-specific files:

### For Android
1. Copy the permissions from the generated `AndroidManifest.xml`
2. Add them to your `Platforms/Android/AndroidManifest.xml` file

### For iOS
1. Copy the keys from the generated `Info.plist`
2. Add them to your `Platforms/iOS/Info.plist` file

## Supported Permission Types

### Location/GPS
- **Include**: `Gps` or `Location`
- **Android**: GPS features, location permissions, background location, foreground service
- **iOS**: Location usage descriptions, background location mode

### Camera
- **Include**: `Camera`
- **Android**: Camera feature and permission
- **iOS**: Camera usage description

### Microphone
- **Include**: `Microphone`
- **Android**: Microphone feature and record audio permission
- **iOS**: Microphone usage description

### Storage
- **Include**: `Storage`
- **Android**: Read/write external storage permissions
- **iOS**: Not applicable (handled automatically by system)

### Bluetooth
- **Include**: `Bluetooth`
- **Android**: Bluetooth features and permissions (including BLE)
- **iOS**: Bluetooth usage descriptions

### Contacts
- **Include**: `Contacts`
- **Android**: Read/write contacts permissions
- **iOS**: Contacts usage description

### Notifications
- **Include**: `Notifications`
- **Android**: Post notifications permission
- **iOS**: Remote notification background mode

### Phone
- **Include**: `Phone`
- **Android**: Call phone and read phone state permissions
- **iOS**: Not applicable

### Photos/Photo Library
- **Include**: `Photos` or `PhotoLibrary`
- **Android**: Not handled (use Storage)
- **iOS**: Photo library usage descriptions

### Calendar
- **Include**: `Calendar`
- **Android**: Not handled
- **iOS**: Calendar usage description

### Reminders
- **Include**: `Reminders`
- **Android**: Not handled
- **iOS**: Reminders usage description

### Speech Recognition
- **Include**: `Speech`
- **Android**: Not handled
- **iOS**: Speech recognition usage description

### Health
- **Include**: `Health`
- **Android**: Not handled
- **iOS**: Health usage descriptions

### Motion
- **Include**: `Motion`
- **Android**: Not handled
- **iOS**: Motion usage description

### Background Processing
- **Include**: `Background-Processing`
- **Android**: Not handled
- **iOS**: Background processing mode

### Background App Refresh
- **Include**: `Background-App-Refresh`
- **Android**: Not handled
- **iOS**: Background app refresh mode

## Installation

Add the NuGet package to your MAUI project:

```xml
<PackageReference Include="Shiny.Maui.PermissionGenerator" Version="1.0.0" />
```

The MSBuild task will automatically run during build and generate the necessary permission files.

## Example Generated Android Manifest

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    <uses-feature android:name="android.permission.LOCATION.GPS" android:required="false" />
    <uses-feature android:name="android.permission.LOCATION.NETWORK" android:required="false" />
    <uses-permission android:name="android.permission.ACCESS_BACKGROUND_LOCATION" />
    <uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
    <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
    <uses-permission android:name="android.permission.POST_NOTIFICATIONS" />
    <uses-permission android:name="android.permission.FOREGROUND_SERVICE" />
    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_LOCATION" />
    <uses-feature android:name="android.hardware.camera" android:required="false" />
    <uses-permission android:name="android.permission.CAMERA" />
</manifest>
```

## Example Generated iOS Info.plist

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>NSLocationAlwaysUsageDescription</key>
    <string>This application uses GPS to provide location-based services</string>
    <key>NSLocationAlwaysAndWhenInUseUsageDescription</key>
    <string>This application uses GPS to provide location-based services</string>
    <key>NSLocationWhenInUseUsageDescription</key>
    <string>This application uses GPS to provide location-based services</string>
    <key>NSCameraUsageDescription</key>
    <string>This application needs camera access to capture photos</string>
    <key>UIBackgroundModes</key>
    <array>
        <string>location</string>
    </array>
</dict>
</plist>
```

## Requirements

- .NET 6 or later
- MAUI project
- The reason text should be user-friendly as it will be displayed to users when requesting permissions

## Build Integration

The task runs during the `PrepareForBuild` target and will show output like:

```
Generating permissions for platform-specific files...
Generating permissions for 3 permission(s)
Generated Android manifest: obj/Debug/net9.0-android/GeneratedPermissions/AndroidManifest.xml
Generated iOS Info.plist: obj/Debug/net9.0-android/GeneratedPermissions/Info.plist
```
