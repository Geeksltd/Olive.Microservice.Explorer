using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace MicroserviceExplorer.Utils
{
    [MarkupExtensionReturnType(typeof(Color))]
    public class DynamicResourceWithConverterExtension : DynamicResourceExtension, INotifyPropertyChanged
    {
        public DynamicResourceWithConverterExtension()
        {
        }

        public DynamicResourceWithConverterExtension(object resourceKey)
            : base(resourceKey)
        {
        }

        IValueConverter Converter;
        object ConverterParameter, CachedValue;

        public override object ProvideValue(IServiceProvider provider)
        {
            if (CachedValue != null) return CachedValue;

            var value = base.ProvideValue(provider);

            if (value is Expression)
                value = Application.Current.TryFindResource(ResourceKey);

            if (value != this && Converter != null)
            {
                Type targetType = null;
                if (provider != null)
                {
                    var target = (IProvideValueTarget)provider.GetService(typeof(IProvideValueTarget));
                    if (target?.TargetProperty is DependencyProperty targetDp)
                    {
                        targetType = targetDp.PropertyType;
                    }
                }

                if (targetType != null)
                    CachedValue = Converter.Convert(value, targetType, ConverterParameter, CultureInfo.CurrentCulture);
            }
            else
            {
                CachedValue = value;
            }

            return CachedValue;
        }

        #region INotifyPropertyChanged Members
        [EscapeGCop("It's not applicable because class needs INotifyPropertyChanged interface")]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}