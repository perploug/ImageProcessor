﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageProcessorConfig.cs" company="James South">
//   Copyright (c) James South.
//   Licensed under the Apache License, Version 2.0.
// </copyright>
// <summary>
//   Encapsulates methods to allow the retrieval of ImageProcessor settings.
//   <see cref="http://csharpindepth.com/Articles/General/Singleton.aspx" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ImageProcessor.Web.Config
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.Compilation;
    using ImageProcessor.Processors;
    #endregion

    /// <summary>
    /// Encapsulates methods to allow the retrieval of ImageProcessor settings.
    /// <see cref="http://csharpindepth.com/Articles/General/Singleton.aspx"/>
    /// </summary>
    public sealed class ImageProcessorConfig
    {
        #region Fields
        /// <summary>
        /// A new instance Initializes a new instance of the <see cref="T:ImageProcessor.Web.Config.ImageProcessorConfig"/> class.
        /// with lazy initialization.
        /// </summary>
        private static readonly Lazy<ImageProcessorConfig> Lazy =
                        new Lazy<ImageProcessorConfig>(() => new ImageProcessorConfig());

        /// <summary>
        /// A collection of the <see cref="T:ImageProcessor.Web.Config.ImageProcessingSection.SettingElementCollection"/> elements 
        /// for available plugins.
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, string>> PluginSettings =
            new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// A collection of the processing presets defined in the configuration. 
        /// for available plugins.
        /// </summary>
        private static readonly Dictionary<string, string> PresetSettings = new Dictionary<string, string>();

        /// <summary>
        /// The processing configuration section from the current application configuration. 
        /// </summary>
        private static ImageProcessingSection imageProcessingSection;

        /// <summary>
        /// The cache configuration section from the current application configuration. 
        /// </summary>
        private static ImageCacheSection imageCacheSection;

        /// <summary>
        /// The security configuration section from the current application configuration. 
        /// </summary>
        private static ImageSecuritySection imageSecuritySection;
        #endregion

        #region Constructors
        /// <summary>
        /// Prevents a default instance of the <see cref="T:ImageProcessor.Web.Config.ImageProcessorConfig"/> class from being created.
        /// </summary>
        private ImageProcessorConfig()
        {
            this.LoadGraphicsProcessors();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the current instance of the <see cref="T:ImageProcessor.Web.Config.ImageProcessorConfig"/> class.
        /// </summary>
        public static ImageProcessorConfig Instance
        {
            get
            {
                return Lazy.Value;
            }
        }

        /// <summary>
        /// Gets the list of available GraphicsProcessors.
        /// </summary>
        public IList<IGraphicsProcessor> GraphicsProcessors { get; private set; }

        #region Caching
        /// <summary>
        /// Gets the maximum number of days to store images in the cache.
        /// </summary>
        public int MaxCacheDays
        {
            get
            {
                return GetImageCacheSection().MaxDays;
            }
        }

        /// <summary>
        /// Gets or the virtual path of the cache folder.
        /// </summary>
        /// <value>The virtual path of the cache folder.</value>
        public string VirtualCachePath
        {
            get
            {
                return GetImageCacheSection().VirtualPath;
            }
        }
        #endregion

        #region Security
        /// <summary>
        /// Gets a list of white listed url[s] that images can be downloaded from.
        /// </summary>
        public Uri[] RemoteFileWhiteList
        {
            get
            {
                return GetImageSecuritySection().WhiteList.Cast<ImageSecuritySection.SafeUrl>().Select(x => x.Url).ToArray();
            }
        }

        /// <summary>
        /// Gets a list of image extensions for url[s] with no extension.
        /// </summary>
        public ImageSecuritySection.SafeUrl[] RemoteFileWhiteListExtensions
        {
            get
            {
                return GetImageSecuritySection().WhiteList.Cast<ImageSecuritySection.SafeUrl>().ToArray();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current application is allowed to download remote files.
        /// </summary>
        public bool AllowRemoteDownloads
        {
            get
            {
                return GetImageSecuritySection().AllowRemoteDownloads;
            }
        }

        /// <summary>
        /// Gets the maximum length to wait in milliseconds before throwing an error requesting a remote file.
        /// </summary>
        public int Timeout
        {
            get
            {
                return GetImageSecuritySection().Timeout;
            }
        }

        /// <summary>
        /// Gets the maximum allowable size in bytes of e remote file to process.
        /// </summary>
        public int MaxBytes
        {
            get
            {
                return GetImageSecuritySection().MaxBytes;
            }
        }

        /// <summary>
        /// Gets the remote prefix for external files for the application.
        /// </summary>
        public string RemotePrefix
        {
            get
            {
                return GetImageSecuritySection().RemotePrefix;
            }
        }
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Returns the collection of the processing presets defined in the configuration.
        /// </summary>
        /// <param name="name">
        /// The name of the plugin to get the settings for.
        /// </param>
        /// <returns>
        /// The <see cref="T:Systems.Collections.Generic.Dictionary{string, string}"/> containing the processing presets defined in the configuration.
        /// </returns>
        public string GetPresetSettings(string name)
        {
            if (!PresetSettings.ContainsKey(name))
            {
                var presetElement =
                    GetImageProcessingSection().Presets
                    .Cast<ImageProcessingSection.PresetElement>()
                    .FirstOrDefault(x => x.Name == name);

                if (presetElement != null)
                {
                    PresetSettings[presetElement.Name] = presetElement.Value;
                }
            }

            string preset;
            PresetSettings.TryGetValue(name, out preset);

            return preset;
        }

        /// <summary>
        /// Returns the <see cref="T:ImageProcessor.Web.Config.ImageProcessingSection.SettingElementCollection"/> for the given plugin.
        /// </summary>
        /// <param name="name">
        /// The name of the plugin to get the settings for.
        /// </param>
        /// <returns>
        /// The <see cref="T:ImageProcessor.Web.Config.ImageProcessingSection.SettingElementCollection"/> for the given plugin.
        /// </returns>
        public Dictionary<string, string> GetPluginSettings(string name)
        {
            if (!PluginSettings.ContainsKey(name))
            {
                var pluginElement =
                    GetImageProcessingSection().Plugins
                    .Cast<ImageProcessingSection.PluginElement>()
                    .FirstOrDefault(x => x.Name == name);

                Dictionary<string, string> settings;

                if (pluginElement != null)
                {
                    settings = pluginElement.Settings
                        .Cast<ImageProcessingSection.SettingElement>()
                        .ToDictionary(setting => setting.Key, setting => setting.Value);
                }
                else
                {
                    settings = new Dictionary<string, string>();
                }

                PluginSettings.Add(name, settings);
                return settings;
            }

            return PluginSettings[name];
        }

        /// <summary>
        /// Retrieves the processing configuration section from the current application configuration. 
        /// </summary>
        /// <returns>The processing configuration section from the current application configuration. </returns>
        private static ImageProcessingSection GetImageProcessingSection()
        {
            return imageProcessingSection ?? (imageProcessingSection = ImageProcessingSection.GetConfiguration());
        }

        /// <summary>
        /// Retrieves the caching configuration section from the current application configuration. 
        /// </summary>
        /// <returns>The caching configuration section from the current application configuration. </returns>
        private static ImageCacheSection GetImageCacheSection()
        {
            return imageCacheSection ?? (imageCacheSection = ImageCacheSection.GetConfiguration());
        }

        /// <summary>
        /// Retrieves the security configuration section from the current application configuration. 
        /// </summary>
        /// <returns>The security configuration section from the current application configuration. </returns>
        private static ImageSecuritySection GetImageSecuritySection()
        {
            return imageSecuritySection ?? (imageSecuritySection = ImageSecuritySection.GetConfiguration());
        }

        /// <summary>
        /// Gets the list of available GraphicsProcessors.
        /// </summary>
        private void LoadGraphicsProcessors()
        {
            if (this.GraphicsProcessors == null)
            {
                if (GetImageProcessingSection().Plugins.AutoLoadPlugins)
                {
                    Type type = typeof(IGraphicsProcessor);
                    try
                    {
                        // Build a list of native IGraphicsProcessor instances.
                        List<Type> availableTypes = BuildManager.GetReferencedAssemblies()
                                                                .Cast<Assembly>()
                                                                .SelectMany(s => s.GetTypes())
                                                                .Where(t => t != null && type.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                                                                .ToList();

                        // Create them and add.
                        this.GraphicsProcessors = availableTypes.Select(x => (Activator.CreateInstance(x) as IGraphicsProcessor)).ToList();

                        // Add the available settings.
                        foreach (IGraphicsProcessor processor in this.GraphicsProcessors)
                        {
                            processor.Settings = this.GetPluginSettings(processor.GetType().Name);
                        }
                    }
                    catch (ReflectionTypeLoadException)
                    {
                        this.LoadGraphicsProcessorsFromConfiguration();
                    }
                }
                else
                {
                    this.LoadGraphicsProcessorsFromConfiguration();
                }
            }
        }

        /// <summary>
        /// Loads graphics processors from configuration.
        /// </summary>
        /// <exception cref="TypeLoadException">
        /// Thrown when an <see cref="IGraphicsProcessor"/> cannot be loaded.
        /// </exception>
        private void LoadGraphicsProcessorsFromConfiguration()
        {
            ImageProcessingSection.PluginElementCollection pluginConfigs = imageProcessingSection.Plugins;
            this.GraphicsProcessors = new List<IGraphicsProcessor>();
            foreach (ImageProcessingSection.PluginElement pluginConfig in pluginConfigs)
            {
                Type type = Type.GetType(pluginConfig.Type);

                if (type == null)
                {
                    throw new TypeLoadException("Couldn't load IGraphicsProcessor: " + pluginConfig.Type);
                }

                this.GraphicsProcessors.Add(Activator.CreateInstance(type) as IGraphicsProcessor);
            }

            // Add the available settings.
            foreach (IGraphicsProcessor processor in this.GraphicsProcessors)
            {
                processor.Settings = this.GetPluginSettings(processor.GetType().Name);
            }
        }
        #endregion
    }
}
