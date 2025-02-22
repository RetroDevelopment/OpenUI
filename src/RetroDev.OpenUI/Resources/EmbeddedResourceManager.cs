﻿using System.Reflection;

namespace RetroDev.OpenUI.Resources;

/// <summary>
/// Manages resources in the application.
/// All resources are treated as embedded resources, so they are assumed to be emebedded in some of the
/// assembly running.
/// </summary>
public class EmbeddedResourceManager : IResourceManager
{
    /// <summary>
    /// Manages windows xml UI definition resources.
    /// </summary>
    public ITextResources Windows => new EmbeddedWindowResources();

    // public ITextResources Langauges => new LanguageResources();
    // public ITextResources Themes => new ThemeResources();
    // internal ITextResources Shaders => new ShaderSrouces();
}
