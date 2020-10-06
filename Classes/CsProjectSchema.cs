using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroserviceExplorer.Classes
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [EscapeGCop("This class is auto generated.")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Project
    {
        ProjectPropertyGroup[] PropertyGroupField;

        List<ProjectPackageReference> itemGroupField;

        string sdkField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PropertyGroup")]
        public ProjectPropertyGroup[] PropertyGroup
        {
            get => PropertyGroupField;
            set => PropertyGroupField = value;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("PackageReference", IsNullable = false)]
        public List<ProjectPackageReference> ItemGroup
        {
            get => itemGroupField;
            set => itemGroupField = value;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Sdk
        {
            get => sdkField;
            set => sdkField = value;
        }
    }

    /// <remarks/>
    [EscapeGCop("This class is auto generated.")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectPropertyGroup
    {
        string outputPathField, noWarnField, targetFrameworkField, conditionField;

        /// <remarks/>
        public string OutputPath
        {
            get => outputPathField;
            set => outputPathField = value;
        }

        /// <remarks/>
        public string NoWarn
        {
            get => noWarnField;
            set => noWarnField = value;
        }

        /// <remarks/>
        public string TargetFramework
        {
            get => targetFrameworkField;
            set => targetFrameworkField = value;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Condition
        {
            get => conditionField;
            set => conditionField = value;
        }
    }

    /// <remarks/>

    [EscapeGCop("This class is auto generated.")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectPackageReference
    {
        string includeField, versionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Include
        {
            get => includeField;
            set => includeField = value;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Version
        {
            get => versionField;
            set => versionField = value;
        }
    }
}