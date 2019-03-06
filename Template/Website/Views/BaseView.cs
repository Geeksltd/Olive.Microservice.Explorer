using Domain;
using Olive.Mvc;
using System;

public abstract class BaseView<TModel> : RazorPage<TModel>
{
}

public static class ResourceConfig
{
    static readonly string RSOURCE_VERSION = Olive.Config.Get("App.Resource.Version");
    const string RSOURCE_VERSION_CONFIG_DEFAULT = "%APP_VERSION%";

    static Lazy<string> _version =
        new Lazy<string>(() => RSOURCE_VERSION != RSOURCE_VERSION_CONFIG_DEFAULT ? RSOURCE_VERSION : "1");

    public static string Version => _version.Value;
}