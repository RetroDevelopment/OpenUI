﻿using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Exceptions;
using RetroDev.OpenUI.Graphics.Internal.OpenGL;
using RetroDev.OpenUI.Utils;
using SDL2;

namespace RetroDev.OpenUI.Core.Internal;

// TODO: add more window options (size, location, style like without borders, etc.)
internal class SDLWindowManager : IWindowManager
{
    private readonly Application _application;
    private readonly IntPtr _window;
    private bool _visible;

    public SDLWindowManager(Application application)
    {
        _application = application;

        // TODO: this is opengl specific code but also sdl specific code. Where to place it?
        LoggingUtils.SDLCheck(() => SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, 3), application.Logger);
        LoggingUtils.SDLCheck(() => SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, 3), application.Logger);
        LoggingUtils.SDLCheck(() => SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE), application.Logger);

        // Enable multisampling and set the number of samples
        LoggingUtils.SDLCheck(() => SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_MULTISAMPLEBUFFERS, 1), application.Logger); // Enable multisampling
        LoggingUtils.SDLCheck(() => SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_MULTISAMPLESAMPLES, 16), application.Logger);  // Number of samples (e.g., 4 for 4x MSAA)

        _window = SDL.SDL_CreateWindow("Test widnow",
                                       SDL.SDL_WINDOWPOS_CENTERED,
                                       SDL.SDL_WINDOWPOS_CENTERED,
                                       800, 600,
                                       SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL);

        if (_window == IntPtr.Zero) throw new UIInitializationException($"Error creating window: {SDL.SDL_GetError()}");
        RenderingEngine = new OpenGLRenderingEngine(_application, _window);
        WindowId = new SDLWindowId((int)SDL.SDL_GetWindowID(_window));
    }

    /// <summary>
    /// The rendering engine that renders in the window managed by <see langword="this"/> <see cref="SDLWindowManager"/>.
    /// </summary>
    public IRenderingEngine RenderingEngine { get; }

    /// <summary>
    /// An abstract representation of the window ID. The ID of windows generated using <see cref="this"/>
    /// <see cref="IWindowManager"/> must match with the respective window event generated by <see cref="IEventSystem"/>
    /// specified in an <see cref="Application"/> to ensure that events are sent to the right window.
    /// </summary>
    public IWindowId WindowId { get; }

    /// <summary>
    /// Whether the window is visible.
    /// </summary>
    public bool Visible
    {
        get
        {
            _application.LifeCycle.ThrowIfNotOnUIThread();
            return _visible;
        }

        set
        {
            _application.LifeCycle.ThrowIfNotOnUIThread();
            _visible = value;

            if (value)
            {
                SDL.SDL_ShowWindow(_window);
            }
            else
            {
                SDL.SDL_HideWindow(_window);
            }
        }
    }

    /// <summary>
    /// Dispose the window and deallocates all the graphical resources.
    /// </summary>
    public void Shutdown()
    {
        _application.LifeCycle.ThrowIfNotOnUIThread();
        RenderingEngine.Shutdown();
        if (_window != IntPtr.Zero) SDL.SDL_DestroyWindow(_window);
    }
}
