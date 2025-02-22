﻿namespace RetroDev.OpenUI.Resources;

/// <summary>
/// Manages resources in the application.s
/// </summary>
public interface IResourceManager
{
    /// <summary>
    /// Manages windows xml UI definition resources.
    /// </summary>
    ITextResources Windows { get; }
}
