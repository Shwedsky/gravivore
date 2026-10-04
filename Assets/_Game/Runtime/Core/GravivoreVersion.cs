namespace Gravivore.Core
{
    public static class GravivoreVersion
    {
        public const string AppVersion = "0.1.0";
        public const int AndroidVersionCode = 1;
        public const string ProductName = "GRAVIVORE";
        public const string CompanyName = "Project Gravivore";
        public const string AndroidCandidatePackageId = "com.gravivore.mobile";
        public const string AndroidDevPackageId = "com.gravivore.mobile.dev";

        // Canonical committed PlayerSettings identity. Flavor-specific build identity is
        // applied only inside the scoped Android build and restored afterwards.
        public const string AndroidPackageId = AndroidCandidatePackageId;
    }
}
