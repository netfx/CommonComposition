﻿using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("CommonComposition")]
[assembly: AssemblyProduct("CommonComposition")]
[assembly: AssemblyCopyright("Copyright 2013, CommonComposition under Apache 2.0 license.")]
[assembly: AssemblyTrademark("CommonComposition")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion("0.1")]
[assembly: AssemblyFileVersion("0.1")]

// This attribute should be the SemanticVersion one.
[assembly: AssemblyInformationalVersion("0.1")]

[assembly: NeutralResourcesLanguage("en")]

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#endif
#if RELEASE
[assembly: AssemblyConfiguration("RELEASE")]
#endif