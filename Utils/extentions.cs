using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace MacroserviceExplorer.Utils
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

        public IValueConverter Converter { get; set; }
        public object ConverterParameter { get; set; }
        public object CachedValue { get; set; }

        public override object ProvideValue(IServiceProvider provider)
        {

            if (CachedValue != null)
                return CachedValue;

            var value = base.ProvideValue(provider);
            Expression e;

            if (value is Expression)
                value = Application.Current.TryFindResource(this.ResourceKey);

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

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
