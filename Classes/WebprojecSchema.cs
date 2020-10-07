using System;

namespace MicroserviceExplorer.Classes.Web
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Project
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PropertyGroup")]
        public ProjectPropertyGroup[] PropertyGroup { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ItemGroup")]
        public ProjectItemGroup[] ItemGroup { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("UsingTask")]
        public ProjectUsingTask[] UsingTask { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Target")]
        public ProjectTarget[] Target { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Sdk { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectPropertyGroup
    {
        /// <remarks/>
        public string NoWarn { get; set; }

        /// <remarks/>
        public string OutputPath { get; set; }

        /// <remarks/>
        public string TargetFramework { get; set; }

        /// <remarks/>
        public decimal TypeScriptToolsVersion { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TypeScriptToolsVersionSpecified { get; set; }

        /// <remarks/>
        public bool CopyLocalLockFileAssemblies { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CopyLocalLockFileAssembliesSpecified { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Condition { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroup
    {
        /// <remarks/>
        public ProjectItemGroupProjectReference ProjectReference { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PackageReference")]
        public ProjectItemGroupPackageReference[] PackageReference { get; set; }

        /// <remarks/>
        public ProjectItemGroupDotNetCliToolReference DotNetCliToolReference { get; set; }

        /// <remarks/>
        public ProjectItemGroupCompile Compile { get; set; }

        /// <remarks/>
        public ProjectItemGroupContent Content { get; set; }

        /// <remarks/>
        public ProjectItemGroupEmbeddedResource EmbeddedResource { get; set; }

        /// <remarks/>
        public ProjectItemGroupNone None { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroupProjectReference
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Include { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroupPackageReference
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Include { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Version { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroupDotNetCliToolReference
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Include { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Version { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroupCompile
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Remove { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroupContent
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Remove { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroupEmbeddedResource
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Remove { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectItemGroupNone
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Remove { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectUsingTask
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AssemblyFile { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TaskName { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectTarget
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("WebCompiler.CompilerCleanTask")]
        public ProjectTargetWebCompilerCompilerCleanTask WebCompilerCompilerCleanTask { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("WebCompiler.CompilerBuildTask")]
        public ProjectTargetWebCompilerCompilerBuildTask WebCompilerCompilerBuildTask { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AfterTargets { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Condition { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectTargetWebCompilerCompilerCleanTask
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool ContinueOnError { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FileName { get; set; }
    }

    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ProjectTargetWebCompilerCompilerBuildTask
    {
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool ContinueOnError { get; set; }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string FileName { get; set; }
    }
}