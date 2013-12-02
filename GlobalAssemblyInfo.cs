using System.Reflection;
using System.Resources;

[assembly: AssemblyCompany("CommonComposition")]
[assembly: AssemblyProduct("CommonComposition")]
[assembly: AssemblyCopyright("Copyright 2013, ClariusLabs under Apache 2.0 license.")]
[assembly: AssemblyTrademark("CommonComposition")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion("0.2.1311.2722")]
[assembly: AssemblyFileVersion("0.2.1311.2722")]
[assembly: AssemblyInformationalVersion("%version%, Branch=%branch%, Sha=%githash%")]

[assembly: NeutralResourcesLanguage("en")]

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#endif
#if RELEASE
[assembly: AssemblyConfiguration("RELEASE")]
#endif
