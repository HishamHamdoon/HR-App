# Starter R8/ProGuard keep rules. Only used when isMinifyEnabled is turned on in
# build.gradle.kts (currently off until a release build is device-tested).

# Flutter engine / embedding.
-keep class io.flutter.** { *; }
-keep class io.flutter.plugins.** { *; }

# Plugins that use reflection or native bridges.
-keep class com.it_nomads.fluttersecurestorage.** { *; }

# Keep annotations and generic signatures used by JSON/reflection.
-keepattributes *Annotation*, Signature, InnerClasses, EnclosingMethod
